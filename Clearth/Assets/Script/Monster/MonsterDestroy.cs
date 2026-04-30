using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DestroyEvent))]
[DisallowMultipleComponent]
public class MonsterDestroy : MonoBehaviour
{
    private DestroyEvent destroyEvent;

    private void Awake()
    {
        // Load components
        destroyEvent = GetComponent<DestroyEvent>();
    }

    private void OnEnable()
    {
        //Subscribe to destroyed event
        destroyEvent.OnDestroyed += DestroyedEvent_OnDestroyed;
    }

    private void OnDisable()
    {
        //Unsubscribe to destroyed event
        destroyEvent.OnDestroyed -= DestroyedEvent_OnDestroyed;

    }

    private void DestroyedEvent_OnDestroyed(DestroyEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
    {
        if (destroyedEventArgs.playerDied)
        {
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}