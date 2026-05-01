using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class DestroyEvent : MonoBehaviour
{
    public event Action<DestroyEvent, DestroyedEventArgs> OnDestroyed;

    public void CallDestroyedEvent(bool playerDied, int points) // 포인트는 점수 얻기 용, 현재는 사용 X
    {
        OnDestroyed?.Invoke(this, new DestroyedEventArgs() { playerDied = playerDied, points = points });
    }
}

public class DestroyedEventArgs : EventArgs
{
    public bool playerDied;
    public int points;
}