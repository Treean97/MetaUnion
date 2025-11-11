using UnityEngine;

public interface INPCBrain
{
    void BeginInteraction(Transform interactor);
    void EndInteraction();
}