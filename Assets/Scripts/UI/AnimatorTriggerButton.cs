
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AnimatorTriggerButton : MonoBehaviour
{
    [SerializeField] string _Key;
    [SerializeField] Animator _Animator;
    private Button _Button;

    void Start()
    {
        if (_Animator != null)
            Setup(_Animator); 
    }


    public void Setup(Animator animator)
    {
        _Button = GetComponent<Button>();
        _Button.onClick.AddListener(() => animator.SetTrigger(_Key));
    }

}
