using Photon.Pun;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    private Animator _Animator;
    private PhotonView _PV;

    void Awake()
    {
        _Animator = gameObject.GetComponent<Animator>();
    }

    void OnEnable()
    {
        ItemPickup.OnItemPickUp += HandlePickUp;
        CurrencyPickup.OnCurrecnyPickUp += HandlePickUp;
    }   

    void OnDisable()
    {
        ItemPickup.OnItemPickUp -= HandlePickUp;
        CurrencyPickup.OnCurrecnyPickUp -= HandlePickUp;
    }

    void HandlePickUp()
    {
        if(_PV.IsMine)
        {
            _Animator.SetTrigger("ItemPickUp");    
        }
        
    }
}
