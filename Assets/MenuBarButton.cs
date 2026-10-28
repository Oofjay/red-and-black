using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuBarButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    bool IsOpen = false;
    bool PointerOverButton = false;
    public GameObject MenuObject;

    public List<GameObject> Submenus;
    public GameObject CurrentSubmenu;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    void Update()
    {
        if (IsOpen && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && !PointerOverButton)
        {
            Close();
        }
    }

    void OnClicked()
    {
        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void OnPointerEnter(PointerEventData EventData)
    {
        PointerOverButton = true;
    }

    public void OnPointerExit(PointerEventData EventData)
    {
        PointerOverButton = false;
    }

    public void Open()
    {
        IsOpen = true;
        MenuObject.SetActive(true);
    }

    public void Close()
    {
        IsOpen = false;
        MenuObject.SetActive(false);
    }

    public void ShowSubmenu(GameObject InSubmenu)
    {
        CurrentSubmenu = InSubmenu;
        InSubmenu.SetActive(true);
    }

    public void HideSubmenu()
    {
        if (CurrentSubmenu != null)
        {
            CurrentSubmenu.SetActive(false);
        }
    }
}
