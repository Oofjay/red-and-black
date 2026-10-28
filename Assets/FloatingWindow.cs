using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingWindow : MonoBehaviour
{
    private Transform TitleBar;
    public List<Tab> Tabs;

    bool HasBeenActivated = false;
    bool UINeedsUpdating = false;

    // Start is called before the first frame update
    void Awake()
    {
        TitleBar = transform.Find("TitleBar");
        TitleBar.Find("Buttons/CloseButton").GetComponent<Button>().onClick.AddListener(Close);
    }

    private void OnEnable()
    {
        if (!HasBeenActivated && Tabs != null && Tabs.Count > 0)
        {
            Tabs[0].SwitchTo();
            HasBeenActivated = true;
        }
    }

    private void LateUpdate()
    {
        if (UINeedsUpdating)
        {
            //LayoutRebuilder.ForceRebuildLayoutImmediate(TitleBar.GetComponent<RectTransform>());
            //LayoutRebuilder.ForceRebuildLayoutImmediate(TitleBar.Find("Title").GetComponent<RectTransform>());
            //LayoutRebuilder.ForceRebuildLayoutImmediate(TitleBar.Find("Buttons").GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

            UINeedsUpdating = false;
        }
    }

    public void SwitchToTab(int TabNumber)
    {
        Tabs[TabNumber].SwitchTo();
        MarkAsNeedUpdating();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void MarkAsNeedUpdating()
    {
        UINeedsUpdating = true;
    }
}
