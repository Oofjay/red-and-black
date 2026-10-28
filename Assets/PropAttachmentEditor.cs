using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PropAttachmentEditor : MonoBehaviour
{
    public enum AttachmentType
    {
        Pad,
        Prop,
        Global,
        Character
    }

    public BetterDropdown PadDropdown;
    public BetterDropdown PropDropdown;
    public BetterDropdown CharacterDropdown;

    private AttachmentType AttachmentID;

    private Prop CurrentProp;

    private List<Pad> SelectablePads = new();
    private List<Prop> SelectableProps = new();
    private List<Prop> SelectableCharacters = new();

    public void Initiate(Prop InProp)
    {
        gameObject.SetActive(true);
        CurrentProp = InProp;

        SelectablePads.Clear();
        SelectableProps.Clear();
        SelectableCharacters.Clear();

        PadDropdown.ClearOptions();
        PropDropdown.ClearOptions();
        CharacterDropdown.ClearOptions();

        foreach (Pad IteratedPad in DataHolder.Pads)
        {
            if (IteratedPad.PadProp == null)
            {
                PadDropdown.OptionList.Add(IteratedPad.NumberString);
                SelectablePads.Add(IteratedPad);
            }
        }

        if (CurrentProp.Type != PropType.Character)
        {
            transform.Find("Controls/Prop").gameObject.SetActive(true);
            foreach (Prop IteratedProp in DataHolder.VisibleProps)
            {
                if (IteratedProp.SubProps.Count < 6 && !IteratedProp.IsGlobal && !IteratedProp.IsAttachedToOtherProp && !IteratedProp.IsAttachedToCharacter)
                {
                    PropDropdown.OptionList.Add(IteratedProp.GetName());
                    SelectableProps.Add(IteratedProp);
                }
            }

            transform.Find("Controls/Character").gameObject.SetActive(true);
            foreach (Prop IteratedCharacter in DataHolder.Characters)
            {
                if (IteratedCharacter.SubProps.Count < 6 && !IteratedCharacter.IsAbstract)
                {
                    CharacterDropdown.OptionList.Add(IteratedCharacter.GetName());
                    SelectableCharacters.Add(IteratedCharacter);
                }
            }

            transform.Find("Controls/Global").gameObject.SetActive(true);
        }
        else
        {
            transform.Find("Controls/Prop").gameObject.SetActive(false);
            transform.Find("Controls/Character").gameObject.SetActive(false);
            transform.Find("Controls/Global").gameObject.SetActive(false);
        }

        TextMeshProUGUI AttachmentText = transform.Find("Controls/CurrentAttachment").GetComponentInChildren<TextMeshProUGUI>();

        if (CurrentProp.IsGlobal)
        {
            transform.Find("Controls/Global").GetComponentInChildren<Toggle>().isOn = true;

            AttachmentText.text = "Current attachment: Global";
        }
        else if (CurrentProp.IsAttachedToOtherProp)
        {
            transform.Find("Controls/Prop").GetComponentInChildren<Toggle>().isOn = true;

            AttachmentText.text = "Current attachment: Prop " + CurrentProp.ParentProp.GetName();
        }
        else if (CurrentProp.IsAttachedToCharacter)
        {
            transform.Find("Controls/Character").GetComponentInChildren<Toggle>().isOn = true;

            AttachmentText.text = "Current attachment: Character " + CurrentProp.ParentProp.GetName();
        }
        else
        {
            transform.Find("Controls/Pad").GetComponentInChildren<Toggle>().isOn = true;

            AttachmentText.text = "Current attachment: Pad " + CurrentProp.ParentPad.NumberString;
        }
    }

    public void OnToggleClicked(int ID)
    {
        AttachmentID = (AttachmentType)ID;
    }

    public void OnApplyButtonClicked()
    {
        if (CurrentProp.IsAttachedToCharacter || CurrentProp.IsAttachedToOtherProp)
        {
            CurrentProp.ParentProp.SubProps.Remove(CurrentProp);
            CurrentProp.ParentProp = null;
        }
        else if (CurrentProp.ParentPad != null)
        {
            CurrentProp.ParentPad.DetachProp();
        }

        if (AttachmentID == AttachmentType.Pad)
        {
            Pad NewPad = SelectablePads[PadDropdown.CurrentIndex];

            CurrentProp.IsAttachedToCharacter = false;
            CurrentProp.IsAttachedToOtherProp = false;
            CurrentProp.IsGlobal = false;

            CurrentProp.RemoveFlags(GameBitFlagHolder.GetBitFlag("OBJFLAG", 0x00004000));
            CurrentProp.RemoveFlags(GameBitFlagHolder.GetBitFlag("OBJFLAG", 0x00008000));

            CurrentProp.ParentPad = NewPad;

            NewPad.AddProp(CurrentProp);

            DropdownOptions.RefreshObjects();
        }
        else if (AttachmentID == AttachmentType.Prop)
        {
            Prop NewProp = SelectableProps[PropDropdown.CurrentIndex];

            CurrentProp.IsAttachedToCharacter = false;
            CurrentProp.IsAttachedToOtherProp = true;
            CurrentProp.IsGlobal = false;

            CurrentProp.ParentProp = NewProp;
            CurrentProp.ParentPad = NewProp.ParentPad;
            NewProp.SubProps.Add(CurrentProp);

            CurrentProp.RemoveFlags(GameBitFlagHolder.GetBitFlag("OBJFLAG", 0x00004000));
            CurrentProp.AddFlags(GameBitFlagHolder.GetBitFlag("OBJFLAG", 0x00008000));

            DropdownOptions.RefreshObjects();
        }
        else if (AttachmentID == AttachmentType.Global)
        {
            CurrentProp.IsAttachedToCharacter = false;
            CurrentProp.IsAttachedToOtherProp = false;
            CurrentProp.IsGlobal = true;

            CurrentProp.RemoveFlags(GameBitFlagHolder.GetBitFlag("OBJFLAG", 0x00004000));
            CurrentProp.RemoveFlags(GameBitFlagHolder.GetBitFlag("OBJFLAG", 0x00008000));

            DropdownOptions.RefreshObjects();
        }
        else if (AttachmentID == AttachmentType.Character)
        {
            Prop NewProp = SelectableCharacters[CharacterDropdown.CurrentIndex];

            CurrentProp.IsAttachedToCharacter = true;
            CurrentProp.IsAttachedToOtherProp = false;
            CurrentProp.IsGlobal = false;

            CurrentProp.ParentProp = NewProp;
            CurrentProp.ParentPad = NewProp.ParentPad;
            NewProp.SubProps.Add(CurrentProp);

            CurrentProp.AddFlags(GameBitFlagHolder.GetBitFlag("OBJFLAG", 0x00004000));
            CurrentProp.RemoveFlags(GameBitFlagHolder.GetBitFlag("OBJFLAG", 0x00008000));

            DropdownOptions.RefreshObjects();
        }

        gameObject.SetActive(false);
    }
}
