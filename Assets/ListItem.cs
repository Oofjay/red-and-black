using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ListItem : MonoBehaviour
{
    public string Value { get { return GetComponentInChildren<TextMeshProUGUI>().text; } set { GetComponentInChildren<TextMeshProUGUI>().text = value; } }
    public int ID { get; set; }
    // Sometimes you want to access an object or a value that is meant to be represented by a list item. You can store it in this variable to access it when the item is clicked or submitted.
    // For example, in a list of props, you can store the prop that matches the list item, so you don't have to hassle around by matching the list item's string text to all the available props.
    public object AssociatedObject { get; set; }

    bool Selected = false;
    float DoubleClickTimer;
    bool EligibleForDoubleClick = false;
    
    ScrollableList ParentList;
    Image Background;

    // Start is called before the first frame update
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);

        ParentList = GetComponentInParent<ScrollableList>();
        Background = GetComponent<Image>();
        Value = GetComponentInChildren<TextMeshProUGUI>().text;
    }

    // Update is called once per frame
    void Update()
    {
        if (EligibleForDoubleClick)
        {
            DoubleClickTimer += Time.deltaTime;

            if (DoubleClickTimer > 0.5f)
            {
                EligibleForDoubleClick = false;
                DoubleClickTimer = 0.0f;
            }
        }
    }

    void OnClick()
    {
        if (!Selected) // Clicked and not selected
        {
            Select();
            EligibleForDoubleClick = true;
        }
        else if (EligibleForDoubleClick) // Double clicked and selected
        {
            OnDoubleClick();
            EligibleForDoubleClick = false;
        }
        else // Clicked and selected
        {
            EligibleForDoubleClick = true;
        }
    }

    void OnDoubleClick()
    {
        if (GetComponentInParent<TextEditor>() != null)
        {
            GetComponentInParent<TextEditor>().EditSelectedString();
        }
    }

    public void Select()
    {
        if (ParentList.SelectedItem != null)
        {
            ParentList.SelectedItem.Unselect();
        }

        Background.color = new Color32(127, 127, 255, 255);

        ParentList.SelectedItem = this;
        Selected = true;

        ParentList.OnListItemSelected(this);
    }

    public void Unselect()
    {
        Background.color = new Color32(0, 0, 0, 0);
        Selected = false;
    }
}
