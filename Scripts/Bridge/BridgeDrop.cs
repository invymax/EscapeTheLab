using UnityEngine;
using System.Collections;

public class BridgeDrop : MonoBehaviour, IInteractable
{
    public Bridge bridge;

    public void Interact()
    {
        if (bridge != null)
        {
            bridge.DropBridge();
        }
    }
}
