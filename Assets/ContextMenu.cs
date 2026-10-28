using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;

public class ContextMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform Transform;

    public Vector3 HitPosition;
    bool PointerOverMenu;

    public DataHolder DataHolderInstance;
    public PropModelPicker PropModelPickerInstance;

    void Update()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && !PointerOverMenu)
        {
            gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData EventData)
    {
        PointerOverMenu = true;
    }

    public void OnPointerExit(PointerEventData EventData)
    {
        PointerOverMenu = false;
    }

    public void Display(Vector3 MousePosition, Vector3 InHitPosition)
    {
        HitPosition = InHitPosition;

        Vector2 MenuPosition = new Vector2(MousePosition.x, MousePosition.y - Screen.height);
        GetComponent<RectTransform>().anchoredPosition = MenuPosition;

        if (SelectionManager.CurrentSelectedObject == null || !SelectionManager.CurrentSelectedObject.TryGetComponent(out Pad NotPad))
        {
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.SetActive(true);
            transform.GetChild(2).gameObject.SetActive(true);
            transform.GetChild(3).gameObject.SetActive(true);
            transform.GetChild(4).gameObject.SetActive(true);
            transform.GetChild(5).gameObject.SetActive(false);
            transform.GetChild(6).gameObject.SetActive(false);
            transform.GetChild(7).gameObject.SetActive(false);
            transform.GetChild(8).gameObject.SetActive(false);
            transform.GetChild(9).gameObject.SetActive(false);
            transform.GetChild(10).gameObject.SetActive(false);
        }
        else if (SelectionManager.CurrentSelectedObject.TryGetComponent(out Pad SelectedPad))
        {
            if (SelectedPad.PadProp == null)
            {
                transform.GetChild(0).gameObject.SetActive(false);
                transform.GetChild(1).gameObject.SetActive(false);
                transform.GetChild(2).gameObject.SetActive(false);
                transform.GetChild(3).gameObject.SetActive(false);
                transform.GetChild(4).gameObject.SetActive(false);
                transform.GetChild(5).gameObject.SetActive(true);
                transform.GetChild(6).gameObject.SetActive(true);
                transform.GetChild(7).gameObject.SetActive(true);
                transform.GetChild(8).gameObject.SetActive(true);
                transform.GetChild(9).gameObject.SetActive(false);
                transform.GetChild(10).gameObject.SetActive(false);
            }
            else
            {
                transform.GetChild(0).gameObject.SetActive(false);
                transform.GetChild(1).gameObject.SetActive(false);
                transform.GetChild(2).gameObject.SetActive(false);
                transform.GetChild(3).gameObject.SetActive(false);
                transform.GetChild(4).gameObject.SetActive(false);
                transform.GetChild(5).gameObject.SetActive(true);
                transform.GetChild(6).gameObject.SetActive(true);
                transform.GetChild(7).gameObject.SetActive(true);
                transform.GetChild(8).gameObject.SetActive(true);
                transform.GetChild(9).gameObject.SetActive(true);
                transform.GetChild(10).gameObject.SetActive(true);
            }
        }

        gameObject.SetActive(true);
    }

    public void InsertPad()
    {
        DataHolderInstance.AddPad(HitPosition);
    }

    public void InsertCoverPad()
    {
        DataHolderInstance.AddCoverPad(HitPosition);
    }

    public void InsertProp()
    {
        PropModelPickerInstance.CreateNewProp(HitPosition, PropModelPicker.ModelMode.Prop);
    }

    public void InsertCharacter()
    {
        PropModelPickerInstance.CreateNewProp(HitPosition, PropModelPicker.ModelMode.Character);
    }

    public void Clone()
    {
        DataHolderInstance.Clone(SelectionManager.SelectedObjects);
    }

    public void Cut()
    {
        DataHolderInstance.Copy(SelectionManager.SelectedObjects);
        DataHolderInstance.DeleteObject(SelectionManager.SelectedObjects);
    }

    public void Copy()
    {
        DataHolderInstance.Copy(SelectionManager.SelectedObjects);
    }

    public void Paste()
    {
        DataHolderInstance.Paste(HitPosition);
    }

    public void Delete()
    {
        DataHolderInstance.DeleteObject(SelectionManager.SelectedObjects);
    }

    public void RemoveProp()
    {
        DataHolderInstance.DeleteProp(SelectionManager.SelectedObjects);
    }

    public void AddTag()
    {
        DataHolderInstance.AddTag(SelectionManager.SelectedObjects);
    }
}
