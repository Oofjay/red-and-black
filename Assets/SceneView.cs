using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class SceneView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    bool IsPointerOverView = false;

    public GameObject ModelSample;
    public Camera ViewCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsPointerOverView)
        {
            if (Input.GetAxis("Mouse ScrollWheel") == 0.1f && ViewCamera.transform.position.z < -15.0f)
            {
                Vector3 CameraPosition = ViewCamera.transform.position;
                CameraPosition.z += 30;
                ViewCamera.transform.position = CameraPosition;
            }
            else if (Input.GetAxis("Mouse ScrollWheel") == -0.1f && ViewCamera.transform.position.z > -2405.0f)
            {
                Vector3 CameraPosition = ViewCamera.transform.position;
                CameraPosition.z -= 30;
                ViewCamera.transform.position = CameraPosition;
            }

            if (Input.GetMouseButton(0))
            {
                float MouseX = Input.GetAxis("Mouse X");
                float MouseY = Input.GetAxis("Mouse Y");

                if (MouseY != 0f)
                {
                    ModelSample.transform.Rotate(new Vector3(-MouseY, 0, 0) * 20, Space.Self);
                }
                if (MouseX != 0f)
                {
                    ModelSample.transform.Rotate(new Vector3(0, MouseX, 0) * 20, Space.Self);
                }
            }
        }

    }

    public void ResetView()
    {
        ViewCamera.transform.position = new Vector3(0, 0, -400);
        ModelSample.transform.eulerAngles = Vector3.zero;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsPointerOverView = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsPointerOverView = false;
    }
}
