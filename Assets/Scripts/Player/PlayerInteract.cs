using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    private Animator _Animator;

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
        _Animator.SetTrigger("ItemPickUp");
    }
}
