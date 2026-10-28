using UnityEngine;
using UnityEngine.EventSystems;

public class BetterDropdownOptionBox : MonoBehaviour, IPointerClickHandler
{
    public BetterDropdown ParentDropdown;

    public void OnPointerClick(PointerEventData EventData)
    {
        ParentDropdown.OnClick();
    }
}
