using UnityEngine;

public class DoorButton : MonoBehaviour, IInteractable
{
    public Door[] doors;

    public void Interact()
    {
        if (doors != null)
        {
            foreach (Door door in doors)
            {
                if (door != null)
                {
                    door.OpenDoor();
                }
            }
        }
    }
}
