using System;

public interface IDestructible
{
    event Action OnDestroyed;
}
