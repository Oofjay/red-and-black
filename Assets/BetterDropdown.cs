using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using UnityEngine.Events;

public class BetterDropdown : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private bool IsOpen = false;
    private bool PointerOverDropdown = false;

    public bool SetDefaultOptionOnEnable = false;
    public bool TemporarilyDisableDefaultOptionOnEnable = false;

    private GameObject ScrollViewInstance;
    private Transform ContentTransform;

    public GameObject ScrollViewTemplate;
    public BetterDropdownOption OptionTemplate;

    public List<string> OptionList = new();

    private int Index;
    public int CurrentIndex { get { return Index; } set { SetSelectedOption(value); } }

    private TextMeshProUGUI BoxLabel;
    public string CurrentText { get { return BoxLabel.text; } set { BoxLabel.text = value; } }

    public UnityEvent<int, string> OptionSelected = new();

    private void Awake()
    {
        BoxLabel = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (TemporarilyDisableDefaultOptionOnEnable)
        {
            TemporarilyDisableDefaultOptionOnEnable = false;
            return;
        }

        if (SetDefaultOptionOnEnable)
        {
            StartCoroutine(OnEnableCoroutine());
        }
    }

    private void OnDisable()
    {
        Close();
    }

    private void Update()
    {
        if (IsOpen && !PointerOverDropdown && Input.GetMouseButtonUp(0))
        {
            Close();
        }
    }

    IEnumerator OnEnableCoroutine()
    {
        yield return new WaitForNextFrameUnit();
        SetSelectedOption(0);
    }

    public void OnClick()
    {
        if (OptionList.Count == 0)
        {
            return;
        }

        if (!IsOpen)
        {
            Open();
        }
        else
        {
            Close();
        }
    }

    public void OnPointerEnter(PointerEventData EventData)
    {
        PointerOverDropdown = true;
    }

    public void OnPointerExit(PointerEventData EventData)
    {
        PointerOverDropdown = false;
    }

    public void Open()
    {
        IsOpen = true;

        ScrollViewInstance = Instantiate(ScrollViewTemplate, transform);
        ContentTransform = ScrollViewInstance.transform.GetChild(0).GetChild(0);
        
        foreach(Transform Child in ContentTransform)
        {
            Destroy(Child.gameObject);
        }

        int Index = 0;
        foreach (string OptionName in OptionList)
        {
            BetterDropdownOption NewOption = Instantiate(OptionTemplate, ContentTransform);
            NewOption.Text = OptionName;
            NewOption.ParentDropdown = this;
            NewOption.ID = Index;

            ++Index;
        }

        if (CurrentIndex > 0)
        {
            ContentTransform.GetChild(CurrentIndex).GetComponent<BetterDropdownOption>().SetAsSelected();

            RectTransform ContentRectTransform = ContentTransform.GetComponent<RectTransform>();
            RectTransform RootRectTransform = ScrollViewInstance.GetComponent<RectTransform>();

            Vector3 HalfHeight = new(0, RootRectTransform.sizeDelta.y / 2 - 12.5f, 0);

            ContentRectTransform.anchoredPosition = 25 * CurrentIndex * Vector3.up - HalfHeight;
        }
    }
    
    public void Close()
    {
        IsOpen = false;

        Destroy(ScrollViewInstance);
    }

    public void SetSelectedOption(int SelectedID)
    {
        if (SelectedID < 0 || SelectedID >= OptionList.Count)
        {
            Debug.LogWarning("Tried to select option out of bounds: " + SelectedID + " in dropdown " + transform.parent.name);
            return;
        }

        SetSelectedOptionInternal(SelectedID);

        OptionSelected?.Invoke(SelectedID, OptionList[SelectedID]);

        AttributeEditor ParentEditor = GetComponentInParent<AttributeEditor>();

        if (ParentEditor != null)
        {
            ParentEditor.OnDropdownChanged(transform.parent.name, SelectedID, OptionList[SelectedID]);
        }
    }

    public void SetSelectedOptionWithoutEvent(int SelectedID)
    {
        if (SelectedID < 0 || SelectedID >= OptionList.Count)
        {
            Debug.LogWarning("Tried to select option out of bounds: " + SelectedID + " in dropdown " + transform.parent.name);
            return;
        }

        SetSelectedOptionInternal(SelectedID);
    }

    private void SetSelectedOptionInternal(int SelectedID)
    {
        CurrentText = OptionList[SelectedID];
        Index = SelectedID;
    }

    public void SetNoOption()
    {
        Index = -1;
        CurrentText = "";
    }

    public void ClearOptions()
    {
        OptionList.Clear();

        Index = 0;
        CurrentText = "Select...";
    }

    public void RefreshCurrentText()
    {
        if (OptionList.Count > 0)
        {
            CurrentText = OptionList[CurrentIndex];
        }
        else
        {
            Index = 0;
            CurrentText = "Select...";
        }
    }

    public void OnOptionSelected(int SelectedID)
    {
        Debug.Log("ID " + SelectedID + " selected with name " + OptionList[SelectedID]);

        Close();

        SetSelectedOption(SelectedID);
    }
}
