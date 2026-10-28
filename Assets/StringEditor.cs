using TMPro;
using UnityEngine;

public class StringEditor : MonoBehaviour
{
    public LanguageString CurrentLanguageString;

    TMP_InputField TextField;

    public TextEditor TextEditorInstance;

    // Start is called before the first frame update
    void Awake()
    {
        TextField = transform.Find("StringControls/TextField").GetComponent<TMP_InputField>();
    }

    public void EnterString(LanguageString InLanguageString)
    {
        gameObject.SetActive(true);
        CurrentLanguageString = InLanguageString;

        TextField.text = CurrentLanguageString.en;
        TextField.caretPosition = 0;
        TextField.verticalScrollbar.value = 0;
    }

    public void Save()
    {
        CurrentLanguageString.en = TextField.text;

        TextEditorInstance.UpdateSelectedItem(CurrentLanguageString);

        gameObject.SetActive(false);
    }
}
