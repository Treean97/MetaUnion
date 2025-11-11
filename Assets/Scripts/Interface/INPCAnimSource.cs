using System;

public enum NPCAnimState
{
    None,
    Idle,
    Walk,
    Interact,
}

public interface INPCAnimSource
{
    NPCAnimState CurrentAnimState { get; }

    event Action<NPCAnimState> OnAnimStateChanged;
}