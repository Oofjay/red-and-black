using UnityEngine;
using UnityEngine.EventSystems;

public class TitleBar : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    bool MouseDown;
    Vector2 PreviousMousePosition;

    RectTransform MovingRectTransform;

    // Start is called before the first frame update
    void Start()
    {
        MovingRectTransform = transform.parent.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (MouseDown)
        {
            //Debug.Log("Mouse down");
            Vector2 MousePosition = Input.mousePosition;
            MovingRectTransform.anchoredPosition += MousePosition - PreviousMousePosition;

            PreviousMousePosition = MousePosition;
        }
    }

    //Detect current clicks on the GameObject (the one with the script attached)
    public void OnPointerDown(PointerEventData pointerEventData)
    {
        MouseDown = true;
        PreviousMousePosition = Input.mousePosition;
    }

    //Detect if clicks are no longer registering
    public void OnPointerUp(PointerEventData pointerEventData)
    {
        MouseDown = false;
    }
}
