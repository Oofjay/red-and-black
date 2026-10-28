using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditorCamera : MonoBehaviour
{
    Camera ParentCamera;

    private int MovementSpeed { get; set; } = 1000;
    private int MinMovementSpeed { get; } = 100;
    private int MaxMovementSpeed { get; } = 10000;

    public bool IsPanning { get; set; }

    void Awake()
    {
        ParentCamera = GetComponent<Camera>();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus == true)
        {
            // If the editor is not focused, it still records mouse movement, stores it, and then activates it when the window gets focused on again.
            // Reset all inputs as soon as the application is focused, so the recorded mouse movement doesn't get triggered and doesn't move the camera.
            Input.ResetInputAxes();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject() && !IsPanning) // Is the mouse over a UI element?
        {
            return;
        }

        if (Input.GetAxis("Mouse ScrollWheel") == 0.1f)
        {
            if (MovementSpeed != MaxMovementSpeed)
            {
                MovementSpeed += 100;
            }
        }
        else if (Input.GetAxis("Mouse ScrollWheel") == -0.1f)
        {
            if (MovementSpeed != MinMovementSpeed)
            {
                MovementSpeed -= 100;
            }
        }

        if (Input.GetMouseButton(1))
        { 
            float MouseX = Input.GetAxis("Mouse X");
            float MouseY = Input.GetAxis("Mouse Y");

            if (MouseY != 0f)
            {
                transform.Rotate(new Vector3(-MouseY, 0, 0) * 10, Space.Self);
                IsPanning = true;
            }
            if (MouseX != 0f)
            {
                transform.Rotate(new Vector3(0, MouseX, 0) * 10, Space.World);
                IsPanning = true;
            }
        }
        if (Input.GetMouseButtonUp(1))
        {
            IsPanning = false;
        }

        if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.TryGetComponent(out TMP_InputField InputField)) // Are we focusing on an input field?
        {
            return;
        }

        if (Input.GetKey(KeyCode.W))
        {
            Vector3 CameraPosition = transform.position;
            CameraPosition += MovementSpeed * Time.deltaTime * transform.forward;

            transform.position = CameraPosition;
        }
        if (Input.GetKey(KeyCode.S))
        {
            Vector3 CameraPosition = transform.position;
            CameraPosition -= MovementSpeed * Time.deltaTime * transform.forward;

            transform.position = CameraPosition;
        }
        if (Input.GetKey(KeyCode.A))
        {
            Vector3 CameraPosition = transform.position;
            CameraPosition -= MovementSpeed * Time.deltaTime * transform.right;

            transform.position = CameraPosition;
        }
        if (Input.GetKey(KeyCode.D))
        {
            Vector3 CameraPosition = transform.position;
            CameraPosition += MovementSpeed * Time.deltaTime * transform.right;

            transform.position = CameraPosition;
        }
    }
}
