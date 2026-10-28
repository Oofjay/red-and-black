using TMPro;
using UnityEngine;

public class TextFieldEventHandler : MonoBehaviour
{
    private TMP_InputField EventField;
    private string FieldName;
    private Pad CurrentPad;
    private Prop CurrentProp;

    void Awake()
    {
        EventField = GetComponent<TMP_InputField>();
        FieldName = EventField.transform.parent.name;
        EventField.onValueChanged.AddListener(OnValueChanged);
    }

    public void OnValueChanged(string NewValue)
    {
        if (GetComponentInParent<IntroSettingsEditor>() != null)
        {
            GetComponentInParent<IntroSettingsEditor>().OnTextFieldChanged(EventField, NewValue);
        }
        else if (GetComponentInParent<FlagEditor>() != null)
        {
            GetComponentInParent<FlagEditor>().OnTextFieldChanged(EventField, NewValue);
        }
        else if (GetComponentInParent<ObjectInspector>() != null)
        {
            GetComponentInParent<ObjectInspector>().OnTextFieldChanged(EventField, NewValue);
        }
        else if (GetComponentInParent<ScriptEditor>() != null)
        {
            GetComponentInParent<ScriptEditor>().OnTextFieldChanged(EventField, NewValue);
        }
        else if (GetComponentInParent<VisibilityEditor>() != null)
        {
            GetComponentInParent<VisibilityEditor>().OnTextFieldChanged(EventField, NewValue);
        }
    }
}
