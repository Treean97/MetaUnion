using UnityEngine;

public class UIRegistrar : MonoBehaviour
{
    private void OnEnable()
    {
        RegisterAllUI();
    }

    private void OnDisable()
    {
        UnregisterAllUI();
    }

    private void RegisterAllUI()
    {
        IUI[] uis = GetComponentsInChildren<IUI>(true);

        foreach (IUI ui in uis)
        {
            // ui.GetType()이 구현한 IUI 인터페이스를 찾아서 등록
            foreach (var interfaceType in ui.GetType().GetInterfaces())
            {
                if (typeof(IUI).IsAssignableFrom(interfaceType) && interfaceType != typeof(IUI))
                {
                    // UIRouter.RegisterAs<T> 메서드를 동적으로 호출
                    var registerMethod = typeof(UIRouter).GetMethod("RegisterAs")
                                           .MakeGenericMethod(interfaceType);
                    registerMethod.Invoke(UIRouter._Inst, new object[] { ui });
                    Debug.Log($"{interfaceType.Name}이(가) 등록되었습니다.");
                }
            }
        }
    }

    private void UnregisterAllUI()
    {
        IUI[] uis = GetComponentsInChildren<IUI>(true);

        foreach (IUI ui in uis)
        {
            foreach (var interfaceType in ui.GetType().GetInterfaces())
            {
                if (typeof(IUI).IsAssignableFrom(interfaceType) && interfaceType != typeof(IUI))
                {
                    var unregisterMethod = typeof(UIRouter).GetMethod("UnregisterAs")
                                           .MakeGenericMethod(interfaceType);
                    unregisterMethod.Invoke(UIRouter._Inst, new object[] { ui });
                    Debug.Log($"{interfaceType.Name}이(가) 해제되었습니다.");
                }
            }
        }
    }
}