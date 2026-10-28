using UnityEngine;
using UnityEngine.UI;

public class PropEditor : AttributeEditor
{
    Prop CurrentProp;
    Prop CurrentTag;
    Prop CurrentCharacter;

    // Start is called before the first frame update
    void Awake()
    {
        transform.Find("Props/SelectProp").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.Props;

        transform.Find("Characters/SelectCharacter").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.Characters;

        transform.Find("Props/SelectTag").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.Tags;
        transform.Find("Props/ReferredProp").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.PropsWithNone;
    }

    public override void OnDropdownChanged(string DropdownName, int SelectedIndex, string SelectedText)
    {
        if (DropdownName == "SelectProp")
        {
            CurrentProp = DataHolder.VisibleProps[SelectedIndex];
        }
        else if (DropdownName == "SelectTag")
        {
            CurrentTag = DataHolder.Tags[SelectedIndex];
            transform.Find("Props/ReferredProp").gameObject.SetActive(true);
            transform.Find("Props/ReferredProp").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.FindPropIndex(CurrentTag.TaggedProp, true));
        }
        else if (DropdownName == "ReferredProp")
        {
            if (CurrentTag == null)
            {
                return;
            }

            if (SelectedIndex == 0)
            {
                CurrentTag.TaggedProp = null;
            }
            else
            {
                CurrentTag.TaggedProp = DataHolder.VisibleProps[SelectedIndex - 1];
            }

            DropdownOptions.Tags[CurrentTag.TagID] = CurrentTag.TagID.ToString() + " - " + (CurrentTag.TaggedProp?.GetName() ?? "No prop");
            transform.Find("Props/SelectTag").GetComponentInChildren<BetterDropdown>().RefreshCurrentText();
        }
        else if (DropdownName == "SelectCharacter")
        {
            CurrentCharacter = DropdownOptions.FindCharacter(SelectedText);
        }
    }

    public void OnToggleClicked(Toggle EventToggle, bool NewValue)
    {
        string FieldName = EventToggle.name;
    }

    public void OnButtonClicked(Button EventButton)
    {
        string FieldName = EventButton.name;

        if (FieldName == "GoToProp")
        {
            if (CurrentProp != null && !CurrentProp.IsGlobal)
            {
                Pad LookPad;

                if (CurrentProp.ParentProp == null) // Attached to pad
                {
                    LookPad = CurrentProp.ParentPad;
                }
                else // Attached to character or other prop
                {
                    LookPad = CurrentProp.ParentProp.ParentPad;
                }

                Vector3 CameraPosition = LookPad.transform.position;
                CameraPosition += LookPad.transform.forward * 100;
                CameraPosition.y += 200;

                DataHolder.MainCameraInstance.transform.position = CameraPosition;
                DataHolder.MainCameraInstance.transform.LookAt(LookPad.transform.position);
            }
        }
        else if (FieldName == "EditInInspector")
        {
            if (CurrentProp == null)
            {
                return;
            }

            if (CurrentProp.ParentPad != null)
            {
                DataHolder.SelectionManagerInstance.SelectNewObject(CurrentProp.ParentPad.gameObject);
            }
            else
            {
                DataHolder.ObjectInspectorInstance.EnterObject(CurrentProp);
                DataHolder.ObjectInspectorInstance.MarkAsNeedUpdating();
            }
        }
        else if (FieldName == "SelectTagToProp")
        {
            if (CurrentTag != null && CurrentTag.TaggedProp != null)
            {
                transform.Find("Props/SelectProp").GetComponentInChildren<BetterDropdown>().CurrentIndex = DropdownOptions.FindPropIndex(CurrentTag.TaggedProp, false);
            }
        }
        else if (FieldName == "AddProp")
        {
            Prop NewProp = DataHolder.AddProp(PropType.StandardObject);

            CurrentProp = NewProp;

            transform.Find("Props/SelectProp").GetComponentInChildren<BetterDropdown>().CurrentIndex = DataHolder.VisibleProps.Count - 1;
        }
        else if (FieldName == "DeleteProp")
        {
            if (CurrentProp != null)
            {
                DataHolder.DeleteProp(CurrentProp);

                BetterDropdown SelectPropDropdown = transform.Find("Props/SelectProp").GetComponentInChildren<BetterDropdown>();

                if (SelectPropDropdown.OptionList.Count == 0)
                {
                    SelectPropDropdown.RefreshCurrentText();
                    CurrentProp = null;
                }
                else if (SelectPropDropdown.CurrentIndex == 0)
                {
                    SelectPropDropdown.CurrentIndex = 0;
                }
                else
                {
                    SelectPropDropdown.CurrentIndex--;
                }
            }
        }
        else if (FieldName == "AddTag")
        {
            Prop NewTag = DataHolder.AddProp(PropType.Tag);

            CurrentTag = NewTag;

            transform.Find("Props/SelectTag").GetComponentInChildren<BetterDropdown>().CurrentIndex = DataHolder.Tags.Count - 1;
        }
        else if (FieldName == "DeleteTag")
        {
            if (CurrentTag != null)
            {
                DataHolder.DeleteProp(CurrentTag);

                BetterDropdown SelectTagDropdown = transform.Find("Props/SelectTag").GetComponentInChildren<BetterDropdown>();

                if (SelectTagDropdown.OptionList.Count == 0)
                {
                    SelectTagDropdown.RefreshCurrentText();
                    CurrentTag = null;
                }
                else if (SelectTagDropdown.CurrentIndex == 0)
                {
                    SelectTagDropdown.CurrentIndex = 0;
                }
                else
                {
                    SelectTagDropdown.CurrentIndex--;
                }
            }
        }
        else if (FieldName == "DeleteCharacter")
        {
            if (CurrentCharacter != null)
            {
                DataHolder.DeleteProp(CurrentCharacter);

                BetterDropdown SelectCharacterDropdown = transform.Find("Characters/SelectCharacter").GetComponentInChildren<BetterDropdown>();

                if (SelectCharacterDropdown.OptionList.Count == 0)
                {
                    SelectCharacterDropdown.RefreshCurrentText();
                    CurrentCharacter = null;
                }
                else if (SelectCharacterDropdown.CurrentIndex == 0)
                {
                    SelectCharacterDropdown.CurrentIndex = 0;
                }
                else
                {
                    SelectCharacterDropdown.CurrentIndex--;
                }
            }
        }
        else if (FieldName == "GoToCharacter")
        {
            if (CurrentCharacter != null)
            {
                Pad LookPad;

                LookPad = CurrentCharacter.ParentPad;

                Vector3 CameraPosition = LookPad.transform.position;
                CameraPosition += LookPad.transform.forward * 100;
                CameraPosition.y += 200;

                DataHolder.MainCameraInstance.transform.position = CameraPosition;
                DataHolder.MainCameraInstance.transform.LookAt(LookPad.transform.position);
            }
        }
        else if (FieldName == "EditCharacterInInspector")
        {
            if (CurrentCharacter != null)
            {
                DataHolder.SelectionManagerInstance.SelectNewObject(CurrentCharacter.ParentPad.gameObject);
            }
        }
    }
}
