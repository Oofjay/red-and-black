using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BetterDropdownOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private TextMeshProUGUI Label;
    private Image Background;

    public string Text { get { return Label.text; } set { Label.text = value; } }
    public int ID;

    private bool Selected;

    public BetterDropdown ParentDropdown;

    void Awake()
    {
        Label = GetComponentInChildren<TextMeshProUGUI>();
        Background = GetComponent<Image>();
    }

    public void SetAsSelected()
    {
        Selected = true;

        Background.color = new Color32(127, 127, 255, 255);
    }

    public void OnPointerEnter(PointerEventData EventData)
    {
        if (Selected)
        {
            return;
        }

        Background.color = new Color32(213, 213, 255, 255);
    }

    public void OnPointerExit(PointerEventData EventData)
    {
        if (Selected)
        {
            Background.color = new Color32(127, 127, 255, 255);
        }
        else
        {
            Background.color = new Color32(0, 0, 0, 0);
        }
    }

    public void OnPointerClick(PointerEventData EventData)
    {
        ParentDropdown.OnOptionSelected(ID);
    }
}
