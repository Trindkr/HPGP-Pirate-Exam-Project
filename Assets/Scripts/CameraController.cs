using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float fastSpeed = 25f;

    [Header("Mouse Look")]
    public float lookSensitivity = 2f;
    public float maxLookAngle = 85f;

    private float rotX = 0f;
    private bool mouseLookEnabled = true;

    void Start()
    {
        SetMouseState(true);
    }

    void Update()
    {
        ToggleMouseLook();

        if (mouseLookEnabled)
            Look();

        Move();
    }

    void ToggleMouseLook()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            mouseLookEnabled = !mouseLookEnabled;
            SetMouseState(mouseLookEnabled);
        }
    }

    void SetMouseState(bool enable)
    {
        Cursor.lockState = enable ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !enable;
    }

    void Move()
    {
        float speed = Keyboard.current.leftShiftKey.isPressed ? fastSpeed : moveSpeed;

        Vector3 move = Vector3.zero;

        if (Keyboard.current.wKey.isPressed) move += transform.forward;
        if (Keyboard.current.sKey.isPressed) move -= transform.forward;
        if (Keyboard.current.aKey.isPressed) move -= transform.right;
        if (Keyboard.current.dKey.isPressed) move += transform.right;

        if (Keyboard.current.spaceKey.isPressed) move += transform.up;
        if (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed) 
            move -= transform.up;

        transform.position += move * (speed * Time.deltaTime);
    }

    void Look()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * (lookSensitivity * 0.1f);

        rotX -= mouseDelta.y;
        rotX = Mathf.Clamp(rotX, -maxLookAngle, maxLookAngle);

        float yaw = transform.localEulerAngles.y + mouseDelta.x;

        transform.localRotation = Quaternion.Euler(rotX, yaw, 0f);
    }
}
