using UnityEngine;
using UnityEngine.UI;

public class Foldout : MonoBehaviour
{
    private Button Label;
    private GameObject Content;
    private RectTransform Arrow;
    public bool IsActive = true;
    public bool IsOpen = true;

    void Awake()
    {
        Label = GetComponentInChildren<Button>();
        Content = transform.GetChild(1).gameObject;
        Arrow = transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Label.onClick.AddListener(OnClicked);
    }

    public void OnClicked()
    {
        if (IsActive)
        {
            if (IsOpen)
            {
                Content.SetActive(false);
                IsOpen = false;
                Arrow.rotation = Quaternion.Euler(0, 0, 90);
                GetComponent<RectTransform>().ForceUpdateRectTransforms();
            }
            else
            {
                Content.SetActive(true);
                IsOpen = true;
                Arrow.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    public void Open()
    {
        if (IsActive)
        {
            Content.SetActive(true);
            IsOpen = true;
            Arrow.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public void Close()
    {
        if (IsActive)
        {
            Content.SetActive(false);
            IsOpen = false;
            Arrow.rotation = Quaternion.Euler(0, 0, 90);
            GetComponent<RectTransform>().ForceUpdateRectTransforms();
        }
    }

    public void Activate()
    {
        if (IsOpen)
        {
            Content.SetActive(true);
        }
        Arrow.gameObject.SetActive(true);
        IsActive = true;
    }

    public void Deactivate()
    {
        Content.SetActive(false);
        Arrow.gameObject.SetActive(false);
        IsActive = false;
    }
}
