using UnityEngine;
using UnityEngine.EventSystems;

public class SubmenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool IsSubmenuButton = false;

    bool PointerOverButton = false;
    float Timer = 0f;

    public GameObject Submenu;
    private MenuBarButton ParentButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ParentButton = GetComponentInParent<MenuBarButton>();
        ParentButton.Submenus.Add(Submenu);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsSubmenuButton && PointerOverButton)
        {
            Timer += Time.deltaTime;

            if (Timer > 0.25f)
            {
                ParentButton.ShowSubmenu(Submenu);
                Vector2 SubmenuPosition = GetComponent<RectTransform>().offsetMax;
                SubmenuPosition.y -= 40;
                Submenu.GetComponent<RectTransform>().anchoredPosition = SubmenuPosition;
            }
        }
    }

    void OnDisable()
    {
        if (IsSubmenuButton)
        {
            Submenu.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData EventData)
    {
        PointerOverButton = true;

        if (ParentButton.CurrentSubmenu != Submenu)
        {
            ParentButton.HideSubmenu();
        }
    }

    public void OnPointerExit(PointerEventData EventData)
    {
        PointerOverButton = false;
        Timer = 0f;
    }
}
