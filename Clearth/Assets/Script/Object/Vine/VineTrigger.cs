using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineTrigger : MonoBehaviour
{
    private VineController vine;

    void Start()
    {
        vine = GetComponentInParent<VineController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<VineHangHandler>()?.AttachToVine(vine);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<VineHangHandler>()?.DetachFromVine();
        }
    }
}
