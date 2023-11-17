using UnityEngine;
using System.Collections;

public class OpenDoor : MonoBehaviour
{
    public Transform door;
    public Transform player;
    public KeyCode openDoorKey = KeyCode.E;
    public float duration = 1.0f; // Duration of the door opening/closing

    private bool open = false;
    private bool isAnimating = false;

    void Update()
    {
        if (Input.GetKeyDown(openDoorKey) && !isAnimating)
        {
            if (Vector3.Distance(player.position, door.position) < 5)
            {
                StartCoroutine(AnimateDoor(open));
            }
        }
    }

    IEnumerator AnimateDoor(bool openState)
    {
        isAnimating = true;
        float time = 0;
        Quaternion startRotation = door.rotation;
        Quaternion endRotation;

        if (openState)
        {
            // Close door
            endRotation = Quaternion.Euler(door.eulerAngles.x, door.eulerAngles.y - 90, door.eulerAngles.z);
        }
        else
        {
            // Open door
            endRotation = Quaternion.Euler(door.eulerAngles.x, door.eulerAngles.y + 90, door.eulerAngles.z);
        }

        while (time < duration)
        {
            door.rotation = Quaternion.Lerp(startRotation, endRotation, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        door.rotation = endRotation;
        open = !open;
        isAnimating = false;
    }
}
