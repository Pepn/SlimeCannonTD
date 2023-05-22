using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Moves the camera on input over the playing field.
/// </summary>
public class CameraController : MonoBehaviour
{
    [SerializeField, FoldoutGroup("References"), Required] private InputActionReference moveCamera;
    [SerializeField, FoldoutGroup("References"), Required] private Transform cam;

    [SerializeField, FoldoutGroup("Settings")] private float movementSpeed;

    private void Update()
    {
        transform.position += moveCamera.action.ReadValue<Vector2>().x * movementSpeed * Time.deltaTime * cam.right;
        transform.position += moveCamera.action.ReadValue<Vector2>().y * movementSpeed * Time.deltaTime * cam.up;
    }
}
