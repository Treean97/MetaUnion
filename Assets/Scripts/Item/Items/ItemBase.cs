using Photon.Pun;
using UnityEngine;

public abstract class ItemBase : MonoBehaviourPun, IInteractable, IPunInstantiateMagicCallback
{
    [Header("공통 설정")]
    [SerializeField] protected ItemDataSO _ItemData;
    [SerializeField] private float _RotationSpeed = 30f;

    [SerializeField]
    protected int _Amount;

    /// <summary>네트워크 생성 시 전달된 데이터를 처리</summary>
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        var data = info.photonView.InstantiationData;
        if (data != null && data.Length > 0)
            ProcessInstantiationData(data);
    }

    /// <summary>서브 클래스에서 인스턴스 데이터(예: 수량)를 처리</summary>
    protected virtual void ProcessInstantiationData(object[] data) { }

    protected virtual void Update()
    {
        // 공통 회전 로직
        transform.Rotate(Vector3.up, _RotationSpeed * Time.deltaTime, Space.World);
    }

    // IInteractable 인터페이스 기본 구현
    public virtual ItemInfoSO GetObjectInfo() => _ItemData.ItemInfo;
    public virtual void OnFocus()   => GameEvents.RaiseFocus(_ItemData.ItemInfo);
    public virtual void OnDefocus() => GameEvents.RaiseDefocus();
    public abstract void OnInteract();
}
