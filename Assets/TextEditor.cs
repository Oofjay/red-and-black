using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextEditor : MonoBehaviour
{
    LanguageBank CurrentLanguageFile;

    ScrollableList StringList;

    public GameObject StringEditorWindow;

    // Start is called before the first frame update
    void Awake()
    {
        StringList = GetComponentInChildren<ScrollableList>();

        transform.Find("TextFile/SelectTextFile").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.LanguageFiles;

        List<string> Languages = new()
        {
            "English (US)",
            "English (UK)",
            "Japanese",
            "Italian",
            "French",
            "German",
            "Spanish"
        };

        transform.Find("TextFile/Language").GetComponentInChildren<BetterDropdown>().OptionList = Languages;
    }

    public void OnTextFileSelected(int SelectedIndex, string SelectedText)
    {
        Debug.Log("OnTextFileSelected");
        CurrentLanguageFile = LanguageFileHolder.GetLanguageFile(SelectedText);

        List<string> LanguageStrings = new();
        foreach (LanguageString CurrentLanguageString in CurrentLanguageFile.LanguageStrings)
        {
            if (CurrentLanguageString.en == null)
            {
                LanguageStrings.Add(" ");
            }
            else
            {
                LanguageStrings.Add(CurrentLanguageString.Abbreviate());
            }
        }
        StringList.AddToList(LanguageStrings);
    }

    public void EditSelectedString()
    {
        StringEditorWindow.GetComponent<StringEditor>().EnterString(CurrentLanguageFile.LanguageStrings[StringList.SelectedItem.ID]);
    }

    public void UpdateSelectedItem(LanguageString InLanguageString)
    {
        string DisplayString = InLanguageString.en;
        if (DisplayString.Length > 50)
        {
            DisplayString = DisplayString[..50];
        }
        DisplayString = DisplayString.Replace("\n", null);

        StringList.SelectedItem.GetComponentInChildren<TextMeshProUGUI>().text = DisplayString;
    }
}
