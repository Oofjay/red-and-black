using UnityEngine;
using UnityEngine.UI;

public class ToggleEventHandler : MonoBehaviour
{
    private Toggle EventToggle;
    private string FieldName;

    void Awake()
    {
        EventToggle = GetComponent<Toggle>();
        FieldName = EventToggle.transform.parent.name;
        EventToggle.onValueChanged.AddListener(OnValueChanged);
    }

    public void OnValueChanged(bool NewValue)
    {
        if (GetComponentInParent<ObjectiveEditor>() != null)
        {
            GetComponentInParent<ObjectiveEditor>().OnToggleClicked(EventToggle, NewValue);
        }
        else if (GetComponentInParent<PadEditor>() != null)
        {
            GetComponentInParent<PadEditor>().OnToggleClicked(EventToggle, NewValue);
        }
        else if (GetComponentInParent<PropEditor>() != null)
        {
            GetComponentInParent<PropEditor>().OnToggleClicked(EventToggle, NewValue);
        }
        else if (GetComponentInParent<ObjectInspector>() != null)
        {
            GetComponentInParent<ObjectInspector>().OnToggleClicked(EventToggle, NewValue);
        }
    }
}
