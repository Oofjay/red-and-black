using UnityEngine;
using UnityEngine.UI;

public class ButtonEventHandler : MonoBehaviour
{
    private Button EventButton;
    private string FieldName;

    void Awake()
    {
        EventButton = GetComponent<Button>();
        FieldName = EventButton.name;
        EventButton.onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        if (GetComponentInParent<ObjectInspector>() != null)
        {
            GetComponentInParent<ObjectInspector>().OnButtonClicked(EventButton);
        }
        else if (GetComponentInParent<ObjectiveEditor>() != null)
        {
            GetComponentInParent<ObjectiveEditor>().OnButtonClicked(EventButton);
        }
        else if (GetComponentInParent<PadEditor>() != null)
        {
            GetComponentInParent<PadEditor>().OnButtonClicked(EventButton);
        }
        else if (GetComponentInParent<IntroSettingsEditor>() != null)
        {
            GetComponentInParent<IntroSettingsEditor>().OnButtonClicked(EventButton);
        }
        else if (GetComponentInParent<FlagEditor>() != null)
        {
            GetComponentInParent<FlagEditor>().OnButtonClicked(EventButton);
        }
        else if (GetComponentInParent<PropEditor>() != null)
        {
            GetComponentInParent<PropEditor>().OnButtonClicked(EventButton);
        }
        else if (GetComponentInParent<ScriptEditor>() != null)
        {
            GetComponentInParent<ScriptEditor>().OnButtonClicked(EventButton);
        }
    }
}
