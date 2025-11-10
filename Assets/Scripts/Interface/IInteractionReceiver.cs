using UnityEngine;

public interface IInteractionReceiver
{
    // 누가 상호작용했는지 전달
    void BeginInteraction(Transform interactor);
    void EndInteraction();
}