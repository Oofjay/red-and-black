using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroSettingsEditor : AttributeEditor
{
    IntroSetting CurrentIntro;
    IntroType CurrentIntroType;
    List<IntroSetting> IntroSettingList = new();

    // Start is called before the first frame update
    void Awake()
    {
        BetterDropdown IntroTypeDropdown = transform.Find("IntroSettings/SelectIntroType").GetComponentInChildren<BetterDropdown>();

        foreach (string IntroType in IntroSetting.IntroNames)
        {
            IntroTypeDropdown.OptionList.Add(IntroType);
        }

        transform.Find("IntroProperties/Pad").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.Pads;
        transform.Find("IntroProperties/WeaponType").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.GetConstantOptions("WEAPON");
        transform.Find("IntroProperties/AmmoType").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.GetConstantOptions("AMMOTYPE");
        transform.Find("IntroProperties/Outfit").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.GetConstantOptions("OUTFIT");
    }

    public override void OnDropdownChanged(string DropdownName, int SelectedIndex, string SelectedText)
    {
        if (DropdownName == "SelectIntroType")
        {
            BetterDropdown SelectIntroDropdown = transform.Find("IntroSettings/SelectIntroSettings").GetComponentInChildren<BetterDropdown>();
            SelectIntroDropdown.ClearOptions();

            CurrentIntroType = (IntroType)SelectedIndex;

            IntroSettingList.Clear();

            int IntroIndex = 1;
            foreach (IntroSetting IteratedIntro in DataHolder.IntroSettings)
            {
                if (IteratedIntro.Type == (IntroType)SelectedIndex)
                {
                    IntroSettingList.Add(IteratedIntro);
                    SelectIntroDropdown.OptionList.Add(IntroIndex.ToString());
                    ++IntroIndex;
                }
            }

            foreach (Transform Child in transform.Find("IntroProperties"))
            {
                Child.gameObject.SetActive(false);
            }

            if (IntroSettingList.Count == 0)
            {
                return;
            }

            if (SelectedIndex == 0 || SelectedIndex == 6) // Pad & hill
            {
                transform.Find("IntroProperties/Pad").gameObject.SetActive(true);
            }
            else if (SelectedIndex == 1) // Weapon
            {
                transform.Find("IntroProperties/WeaponType").gameObject.SetActive(true);
            }
            else if (SelectedIndex == 2) // Ammo
            {
                transform.Find("IntroProperties/AmmoType").gameObject.SetActive(true);
                transform.Find("IntroProperties/AmmoAmount").gameObject.SetActive(true);
            }
            else if (SelectedIndex == 3) // Outfit
            {
                transform.Find("IntroProperties/Outfit").gameObject.SetActive(true);
            }
            else if (SelectedIndex == 4 || SelectedIndex == 5) // Case & case ID
            {
                transform.Find("IntroProperties/CaseID").gameObject.SetActive(true);
                transform.Find("IntroProperties/Pad").gameObject.SetActive(true);
            }

            SelectIntroDropdown.CurrentIndex = 0;
        }
        else if (DropdownName == "SelectIntroSettings")
        {
            if (IntroSettingList.Count == 0)
            {
                return;
            }

            CurrentIntro = IntroSettingList[SelectedIndex];

            if (CurrentIntro.Type == IntroType.Spawn || CurrentIntro.Type == IntroType.Hill) // Pad & hill
            {
                transform.Find("IntroProperties/Pad").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.FindPadIndex(CurrentIntro.IntroPad, false));
            }
            else if (CurrentIntro.Type == IntroType.Weapon) // Weapon
            {
                transform.Find("IntroProperties/WeaponType").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.GetConstantIndex(CurrentIntro.WeaponType));
            }
            else if (CurrentIntro.Type == IntroType.Ammo) // Ammo
            {
                transform.Find("IntroProperties/AmmoType").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.GetConstantIndex(CurrentIntro.AmmoType));
                transform.Find("IntroProperties/AmmoAmount").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentIntro.AmmoAmount.ToString());
            }
            else if (CurrentIntro.Type == IntroType.PlayerOutfit) // Outfit
            {
                transform.Find("IntroProperties/Outfit").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.GetConstantIndex(CurrentIntro.Outfit));
            }
            else if (CurrentIntro.Type == IntroType.Case || CurrentIntro.Type == IntroType.CaseRespawn) // Case & case ID
            {
                transform.Find("IntroProperties/CaseID").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentIntro.CaseID.ToString());
                transform.Find("IntroProperties/Pad").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.FindPadIndex(CurrentIntro.IntroPad, false));
            }
        }
        else if (DropdownName == "Pad")
        {
            CurrentIntro.IntroPad = DropdownOptions.FindPad(SelectedText);
        }
        else if (DropdownName == "WeaponType")
        {
            CurrentIntro.WeaponType = DropdownOptions.GetConstant("WEAPON", SelectedIndex);
        }
        else if (DropdownName == "AmmoType")
        {
            CurrentIntro.AmmoType = DropdownOptions.GetConstant("AMMOTYPE", SelectedIndex);
        }
        else if (DropdownName == "Outfit")
        {
            CurrentIntro.Outfit = DropdownOptions.GetConstant("OUTFIT", SelectedIndex);
        }
    }

    public void OnTextFieldChanged(TMP_InputField EventField, string NewValue)
    {
        string FieldName = EventField.transform.parent.name;

        if(string.IsNullOrEmpty(NewValue))
        {
            return;
        }

        if (FieldName == "CaseID")
        {
            CurrentIntro.CaseID = int.Parse(NewValue);
        }
        else if (FieldName == "AmmoAmount")
        {
            CurrentIntro.AmmoAmount = int.Parse(NewValue);
        }
    }

    public void OnButtonClicked(Button EventButton)
    {
        string FieldName = EventButton.name;

        if (FieldName == "AddIntro")
        {
            IntroSetting NewIntro = new() { Type = CurrentIntroType };
            DataHolder.IntroSettings.Add(NewIntro);
            IntroSettingList.Add(NewIntro);

            BetterDropdown SelectIntroDropdown = transform.Find("IntroSettings/SelectIntroSettings").GetComponentInChildren<BetterDropdown>();
            SelectIntroDropdown.OptionList.Add(IntroSettingList.Count.ToString());

            if (SelectIntroDropdown.OptionList.Count == 1)
            {
                SelectIntroDropdown.CurrentIndex = 0;

                if (CurrentIntroType == IntroType.Spawn || CurrentIntroType == IntroType.Hill) // Pad & hill
                {
                    transform.Find("IntroProperties/Pad").gameObject.SetActive(true);
                }
                else if (CurrentIntroType == IntroType.Weapon) // Weapon
                {
                    transform.Find("IntroProperties/WeaponType").gameObject.SetActive(true);
                }
                else if (CurrentIntroType == IntroType.Ammo) // Ammo
                {
                    transform.Find("IntroProperties/AmmoType").gameObject.SetActive(true);
                    transform.Find("IntroProperties/AmmoAmount").gameObject.SetActive(true);
                }
                else if (CurrentIntroType == IntroType.PlayerOutfit) // Outfit
                {
                    transform.Find("IntroProperties/Outfit").gameObject.SetActive(true);
                }
                else if (CurrentIntroType == IntroType.Case || CurrentIntroType == IntroType.CaseRespawn) // Case & case ID
                {
                    transform.Find("IntroProperties/CaseID").gameObject.SetActive(true);
                    transform.Find("IntroProperties/Pad").gameObject.SetActive(true);
                }
            }
            else
            {
                SelectIntroDropdown.CurrentIndex = SelectIntroDropdown.OptionList.Count - 1;
            }
        }
        else if (FieldName == "DeleteIntro")
        {
            if (IntroSettingList.Count == 0)
            {
                return;
            }

            DataHolder.IntroSettings.Remove(CurrentIntro);
            IntroSettingList.Remove(CurrentIntro);
            BetterDropdown SelectIntroDropdown = transform.Find("IntroSettings/SelectIntroSettings").GetComponentInChildren<BetterDropdown>();
            SelectIntroDropdown.OptionList.RemoveAt(SelectIntroDropdown.CurrentIndex);

            if (SelectIntroDropdown.OptionList.Count == 0)
            {
                SelectIntroDropdown.RefreshCurrentText();
            }
            else if (SelectIntroDropdown.CurrentIndex == 0)
            {
                SelectIntroDropdown.CurrentIndex = 0;
            }
            else
            {
                SelectIntroDropdown.CurrentIndex--;
            }

            if (IntroSettingList.Count == 0)
            {
                CurrentIntro = null;

                foreach (Transform Child in transform.Find("IntroProperties"))
                {
                    Child.gameObject.SetActive(false);
                }
            }
        }
    }
}
