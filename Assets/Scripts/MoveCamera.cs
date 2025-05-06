using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition;

    void Update()
    {
        // Move this camera to the same place as cameraPosition
        transform.position = cameraPosition.position;
    }
}
