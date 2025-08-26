using System.Collections;
using Photon.Pun;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private string _PrefabName;
    [SerializeField] private float _RespawnSeconds  = 5f;
    [SerializeField] private MonoBehaviour _ExistingRespawnable; // ← 씬 배치본

    
    private PhotonView _CurrentPV;
    private IRespawnable _CurrentRespawnable;
    private IDestructible _CurrentDestructible;
    private Coroutine _RespawnRoutine;

    public float GetRespawnDelayFor(IRespawnable _) => _RespawnSeconds;

    void Start()
    {
        if (_ExistingRespawnable != null)
        {
            Bind((_ExistingRespawnable as Component).gameObject);
            _ExistingRespawnable = null;
        }    
    }

    void OnDisable()
    {
        if (_CurrentDestructible != null)
            _CurrentDestructible.OnDestroyed -= HandleDestroyed;

        if (_RespawnRoutine != null)
        {
            StopCoroutine(_RespawnRoutine);
            _RespawnRoutine = null;
        }
    }


    private void Bind(GameObject go)
    {
        _CurrentPV = go.GetComponent<PhotonView>();

        _CurrentRespawnable = go.GetComponent<IRespawnable>();
        _CurrentRespawnable?.Init(this);
        _CurrentRespawnable?.OnSpawned();

        _CurrentDestructible = go.GetComponent<IDestructible>();
        if (_CurrentDestructible != null)
        {
            _CurrentDestructible.OnDestroyed -= HandleDestroyed;
            _CurrentDestructible.OnDestroyed += HandleDestroyed;
        }
    }

    public void SpawnNow()
    {
        if (!PhotonNetwork.IsMasterClient) return;   // 마스터만 생성
        if (_CurrentRespawnable != null) return;     // 이미 존재

        var go = PhotonNetwork.Instantiate(
            _PrefabName,
            transform.position,
            transform.rotation
            );

        Bind(go);
    }

    private void HandleDestroyed()
    {
        if (!PhotonNetwork.IsMasterClient) return;   // 마스터만 파괴/리스폰 스케줄

        // 이벤트 구독 해제
        if (_CurrentDestructible != null)
            _CurrentDestructible.OnDestroyed -= HandleDestroyed;

        float delay = _CurrentRespawnable?.GetRespawnDelay() ?? _RespawnSeconds;

        if (_CurrentPV != null)
        {
            // 인스턴스 객체면 네트워크 파괴
            if (_CurrentPV.InstantiationId > 0)
            {
                PhotonNetwork.Destroy(_CurrentPV);
            }
            else
            {
                // 씬 배치 PV: 모든 클라에서 동일하게 로컬 파괴/비활성
                _CurrentPV.RPC(nameof(RPC_DespawnSceneObject), RpcTarget.AllBuffered);
            }
        }

        _CurrentPV = null;
        _CurrentRespawnable = null;
        _CurrentDestructible = null;

        // 기존 코루틴 중복 방지
        if (_RespawnRoutine != null) StopCoroutine(_RespawnRoutine);
        _RespawnRoutine = StartCoroutine(RespawnRoutine(delay));
    }

    [PunRPC]
    void RPC_DespawnSceneObject()
    {
        if (this != null && gameObject != null)
            Destroy(gameObject); // 또는 SetActive(false)로 교체 가능(원복 필요 시)
    }


    private IEnumerator RespawnRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        _RespawnRoutine = null;

        if (!isActiveAndEnabled) yield break;
        if (!PhotonNetwork.IsMasterClient) yield break;

        SpawnNow();
    }
    
}
