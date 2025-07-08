using System;
using UnityEngine;

public interface IDestructible
{
    event Action OnDestroyed;
}
