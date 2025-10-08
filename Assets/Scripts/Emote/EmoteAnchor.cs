using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// 네트워크로 생성되는 앵커 본체.
/// - InstantiationData로 메타(id/state/layer/len/t0/host/slotCount) 복구
/// - 생성자(주최자)는 생성 직후 자동 참여
/// - IInteractable/IFocusable 구현: E키 상호작용으로 주최 종료/참여/탈출 처리
/// </summary>
public class EmoteAnchor : MonoBehaviourPun, IPunInstantiateMagicCallback, IInteractable
{
    [Header("Slots (인스펙터에서 순서대로 할당)")]
    [SerializeField] private List<Transform> _Slots = new(); // Slot_0, Slot_1 ...

    // 생성 메타(읽기 전용)
    public string EmoteId { get; private set; }
    public string AnimatorState { get; private set; }
    public int    AnimatorLayer { get; private set; }
    public double EmoteLength { get; private set; }
    public double StartTime { get; private set; }
    public int    HostActorNumber { get; private set; }
    public int    DeclaredSlotCount { get; private set; }

    public int SlotCount => _Slots?.Count ?? 0;

    // Focus UI용 임시 데이터 캐시
    private ItemInfoSO _TempFocusInfo;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        var d = info.photonView.InstantiationData;
        if (d == null || d.Length < 7)
        {
            Debug.LogWarning("[EmoteAnchor] InstantiationData 부족.");
            return;
        }

        EmoteId           = d[0] as string;
        AnimatorState     = d[1] as string;
        AnimatorLayer     = (int)d[2];
        EmoteLength       = System.Convert.ToDouble(d[3]);
        StartTime         = System.Convert.ToDouble(d[4]);
        HostActorNumber   = (int)d[5];
        DeclaredSlotCount = (int)d[6];

        if (SlotCount < DeclaredSlotCount)
            Debug.LogWarning($"[EmoteAnchor] 슬롯 수({SlotCount}) < 선언({DeclaredSlotCount}). 인스펙터에 슬롯 Transform을 더 할당하세요.");

        // === 주최자 자동 참여 ===
        // 이 뷰의 소유자는 생성한 주최자. 로컬이 소유자면 즉시 슬롯 점유 + 참여.
        if (photonView.IsMine)
        {
            TryAutoJoinHost();
        }
    }

    // ===== IInteractable / IFocusable =====

    // 상호작용 입력(E): 규칙
    // 1) 내가 이 앵커의 주최자면 -> 주최 종료
    // 2) 내가 이 앵커에 이미 참여 중이면 -> 탈출
    // 3) 그 외 -> 참여 시도
    public void OnInteract()
    {
        var me = FindLocalPlayerEmote();
        if (!me) return;

        if (Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber == HostActorNumber)
        {
            // 주최 종료
            EmoteManager._Inst?.RequestEndEmote(this);
            return;
        }

        if (me.IsInEmote && me.CurrentAnchor == this)
        {
            // 참여자 탈출
            me.LeaveEmote();
            return;
        }

        // 참여 시도
        Interact_AsParticipant(me);
    }

    public void OnFocus()
    {
        // 여기서는 데이터만 준비. 실제 UI 표시는 프로젝트의 Focus 시스템이 GetObjectInfo()를 읽어 처리.
        if (_TempFocusInfo == null) _TempFocusInfo = CreateFocusInfo();
        // 필요하면 여기서 FocusUI에 넘기는 호출을 추가(프로젝트 규칙에 맞춰)
        // FocusUI.Show(_TempFocusInfo);  // 예시
    }

    public void OnDefocus()
    {
        // 필요 시 포커스 UI 닫기
        // FocusUI.Hide(); // 예시
    }

    public ItemInfoSO GetObjectInfo()
    {
        if (_TempFocusInfo == null) _TempFocusInfo = CreateFocusInfo();
        return _TempFocusInfo;
    }

    ItemInfoSO CreateFocusInfo()
    {
        var soName = EmoteId;
        // SO 레지스트리에서 DisplayName을 우선 사용
        if (EmoteManager._Inst && EmoteManager._Inst.TryGetSO(EmoteId, out var so) && so)
        {
            soName = string.IsNullOrEmpty(so.DisplayName) ? so.ID : so.DisplayName;
        }

        var info = ScriptableObject.CreateInstance<ItemInfoSO>();
        info.DisplayName = soName;
        info.Description = "\"E\"를 눌러 이모트에 참여하세요";
        return info;
    }

    // ===== 기존 상호작용: 참여/주최종료 보조 =====

    // 참여자 측 진입(외부에서 직접 호출 가능)
    public void Interact_AsParticipant(PlayerEmote player)
    {
        if (!player) return;

        if (player.IsInEmote && player.CurrentAnchor == this)
        {
            player.LeaveEmote();
            return;
        }

        // 슬롯 점유 시도
        int slot = EmoteManager._Inst.TryOccupySlot(this);
        if (slot < 0)
        {
            Debug.Log("[EmoteAnchor] 빈 슬롯 없음.");
            return;
        }

        // 진행도 계산: (서버시간 - t0) / len
        double now = Photon.Pun.PhotonNetwork.Time;
        double elapsed = now - StartTime;
        float normalized = 0f;
        if (EmoteLength > 0.0001) normalized = (float)((elapsed % EmoteLength) / EmoteLength);

        var slotTf = GetSlot(slot);
        player.JoinEmote(this, slot, slotTf, normalized);
    }

    // 주최 종료(직접 호출용)
    public void Interact_AsHostEnd()
    {
        if (Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber != HostActorNumber)
        {
            Debug.LogWarning("[EmoteAnchor] 주최자만 종료 가능.");
            return;
        }
        EmoteManager._Inst.RequestEndEmote(this);
    }

    public Transform GetSlot(int index)
    {
        if (_Slots == null || index < 0 || index >= _Slots.Count || !_Slots[index]) return transform;
        return _Slots[index];
    }

    /// <summary>주최 종료 시, 모든 참여자에게 “해당 앵커에 참여 중이면 탈출” 요청</summary>
    public void KickAllParticipants_LocalAndAnnounce()
    {
        photonView.RPC(nameof(RPC_ForceLeaveIfInThisAnchor), RpcTarget.All);
    }

    [PunRPC]
    void RPC_ForceLeaveIfInThisAnchor()
    {
        var me = FindLocalPlayerEmote();
        if (me && me.IsInEmote && me.CurrentAnchor == this)
            me.LeaveEmote();
    }

    PlayerEmote FindLocalPlayerEmote()
    {
        // 프로젝트에서 로컬 플레이어 접근 방법에 맞게 구현
        // 예) PlayerSetup._LocalPlayer 사용
        var lp = PlayerSetup._LocalPlayer;
        if (!lp) return null;
        return lp.GetComponent<PlayerEmote>();
    }

    // ===== 주최자 자동 참여 =====
    void TryAutoJoinHost()
    {
        // 슬롯이 0이면 참여 불가
        if (DeclaredSlotCount <= 0 && SlotCount <= 0)
        {
            Debug.LogWarning("[EmoteAnchor] 슬롯 개수가 0입니다. 주최자 자동 참여 불가.");
            return;
        }

        var me = FindLocalPlayerEmote();
        if (!me)
        {
            Debug.LogWarning("[EmoteAnchor] 로컬 PlayerEmote를 찾지 못해 자동 참여 실패.");
            return;
        }

        // 이미 다른 이모트 중이면 무시
        if (me.IsInEmote && me.CurrentAnchor == this) return;

        // 슬롯 점유
        int slot = EmoteManager._Inst.TryOccupySlot(this);
        if (slot < 0)
        {
            Debug.LogWarning("[EmoteAnchor] 주최자 자동 참여 실패: 가용 슬롯 없음.");
            return;
        }

        // 현재 진행도 계산
        double now = Photon.Pun.PhotonNetwork.Time;
        double elapsed = now - StartTime;
        float normalized = 0f;
        if (EmoteLength > 0.0001) normalized = (float)((elapsed % EmoteLength) / EmoteLength);

        var slotTf = GetSlot(slot);
        me.JoinEmote(this, slot, slotTf, normalized);
        // Debug.Log("[EmoteAnchor] Host auto-joined.");
    }
}
