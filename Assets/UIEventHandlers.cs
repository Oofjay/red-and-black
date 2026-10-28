using UnityEngine;

public class AttributeEditor : MonoBehaviour
{
    public virtual void OnDropdownChanged(string DropdownName, int SelectedIndex, string SelectedText)
    {
        Debug.Log("Dropdown toggled");
    }

}
