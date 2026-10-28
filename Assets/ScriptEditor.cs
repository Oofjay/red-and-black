using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System;

public class ScriptEditor : AttributeEditor
{
    public AIList CurrentAIList;
    public AICommand CurrentCommandType;
    public AICommandInstance CurrentCommandInstance;
    public Label CurrentLabel;

    public Transform ParameterHolder;
    public ScrollableList CommandList;
    public TextMeshProUGUI CommandDescription;

    public GameObject DropdownTemplate;
    public GameObject TextFieldTemplate;
    public GameObject ToggleTemplate;
    public GameObject ButtonTemplate;

    public FlagsEditor FlagsEditorInstance;

    private bool UINeedsUpdating;

    // Start is called before the first frame update
    void Awake()
    {
        transform.Find("SelectAndDelete/SelectScript").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.AILists;
        transform.Find("EditCommand/SelectCommand").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.AICommands;

        ParameterHolder = transform.Find("Parameters");
        CommandDescription = transform.Find("CommandDescription").GetComponentInChildren<TextMeshProUGUI>();

        CommandList = transform.Find("ScriptCommands/CommandList").GetComponentInChildren<ScrollableList>();
        CommandList.ListItemSelected += OnCommandInstanceSelected;
    }

    private void LateUpdate()
    {
        if (UINeedsUpdating)
        {
            /*ParameterColumn1.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
            ParameterColumn1.GetComponent<ContentSizeFitter>().SetLayoutVertical();

            ParameterColumn2.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
            ParameterColumn2.GetComponent<ContentSizeFitter>().SetLayoutVertical();

            ParameterColumnHolder.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
            ParameterColumnHolder.GetComponent<ContentSizeFitter>().SetLayoutVertical();

            GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
            GetComponent<ContentSizeFitter>().SetLayoutVertical();

            ParameterColumn1.GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
            ParameterColumn1.GetComponent<VerticalLayoutGroup>().SetLayoutHorizontal();*/
            LayoutRebuilder.ForceRebuildLayoutImmediate(ParameterHolder.GetComponent<RectTransform>());

            //GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
            //GetComponent<VerticalLayoutGroup>().SetLayoutHorizontal();
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

            UINeedsUpdating = false;
        }
    }

    public void MarkAsNeedUpdating()
    {
        UINeedsUpdating = true;
        GetComponentInParent<FloatingWindow>().MarkAsNeedUpdating();
    }

    public void EnterScript(AIList InAIList)
    {
        gameObject.SetActive(true);

        BetterDropdown SelectScriptDropdown = transform.Find("SelectAndDelete/SelectScript").GetComponentInChildren<BetterDropdown>();
        SelectScriptDropdown.TemporarilyDisableDefaultOptionOnEnable = true;
        SelectScriptDropdown.SetSelectedOption(DataHolder.AILists.IndexOf(InAIList));
    }

    public void OnButtonClicked(Button EventButton)
    {
        string FieldName = EventButton.name;

        switch (FieldName)
        {
            case "AddScript":
                AIList NewAIList = DataHolder.AddAIList();

                if (NewAIList == null)
                {
                    Debug.LogError("Could not create script, too many exist already");
                    return;
                }

                transform.Find("SelectAndDelete/SelectScript").GetComponentInChildren<BetterDropdown>().SetSelectedOption(DropdownOptions.FindAIListIndex(NewAIList));
                break;
            case "DeleteScript":
                if (DataHolder.AILists.Count == 0)
                {
                    return;
                }

                BetterDropdown SelectScriptDropdown = transform.Find("SelectAndDelete/SelectScript").GetComponentInChildren<BetterDropdown>();

                DataHolder.DeleteAIList(CurrentAIList);


                if (SelectScriptDropdown.OptionList.Count == 0)
                {
                    SelectScriptDropdown.RefreshCurrentText();
                }
                else if (SelectScriptDropdown.CurrentIndex == 0)
                {
                    SelectScriptDropdown.CurrentIndex = 0;
                }
                else
                {
                    SelectScriptDropdown.CurrentIndex--;
                }

                if (DataHolder.AILists.Count == 0)
                {
                    CurrentAIList = null;
                    CurrentCommandInstance = null;

                    CommandList.Clear();
                }
                break;
            case "AddCommand":
                if (CurrentAIList == null)
                {
                    return;
                }

                if (CommandList.SelectedItem == null)
                {
                    AICommandInstance NewCommandInstance = CurrentAIList.InsertCommand(CurrentAIList.Commands.Count, CurrentCommandType);
                    CommandList.AddItemAtEnd(CurrentCommandType.DisplayString, true);
                }
                else
                {
                    AICommandInstance NewCommandInstance = CurrentAIList.InsertCommand(CommandList.SelectedItem.ID + 1, CurrentCommandType);
                    CommandList.AddItemAtIndex(CommandList.SelectedItem.ID + 1, CurrentCommandType.DisplayString);
                }
                break;
            case "DeleteCommand":
                if (CurrentAIList == null || CurrentCommandInstance == null)
                {
                    return;
                }

                CurrentAIList.DeleteCommand(CurrentCommandInstance);

                CommandList.RemoveSelectedItem();

                if (CurrentAIList.Commands.Count == 0)
                {
                    CurrentCommandInstance = null;
                }
                break;
            case "AddLabel":
                if (CurrentAIList == null)
                {
                    return;
                }

                Label NewLabel = CurrentAIList.CreateNewLabel();

                BetterDropdown LabelDropdown = transform.Find("SelectAndDelete/LabelOptions/SelectLabel").GetComponentInChildren<BetterDropdown>();

                LabelDropdown.OptionList = CurrentAIList.LabelOptions;
                LabelDropdown.CurrentIndex = CurrentAIList.Labels.IndexOf(NewLabel);
                break;
            case "DeleteLabel":
                if (CurrentAIList == null || CurrentAIList.Labels.Count == 0)
                {
                    return;
                }

                LabelDropdown = transform.Find("SelectAndDelete/LabelOptions/SelectLabel").GetComponentInChildren<BetterDropdown>();

                Label CurrentLabel = CurrentAIList.Labels[LabelDropdown.CurrentIndex];

                foreach (AICommandInstance CurrentCommandInstance in CurrentAIList.Commands)
                {
                    foreach (object CurrentParameter in CurrentCommandInstance.Parameters)
                    {
                        if (CurrentParameter == CurrentLabel)
                        {
                            Debug.Log("Cannot delete label, is still in use");
                            return;
                        }
                    }
                }

                CurrentAIList.DeleteLabel(CurrentLabel);

                if (CurrentAIList.Labels.Count == 0)
                {
                    LabelDropdown.RefreshCurrentText();
                    CurrentLabel = null;
                }
                else if (LabelDropdown.CurrentIndex == 0)
                {
                    LabelDropdown.CurrentIndex = 0;
                }
                else
                {
                    LabelDropdown.CurrentIndex--;
                }
                break;
            case "MoveUp":
                if (CommandList.SelectedItem == null || CurrentCommandInstance == null)
                {
                    return;
                }

                if (CommandList.SelectedItem.ID != 0)
                {
                    CurrentAIList.Commands.RemoveAt(CommandList.SelectedItem.ID);
                    CurrentAIList.Commands.Insert(CommandList.SelectedItem.ID - 1, CurrentCommandInstance);
                }

                CommandList.MoveSelectedItemUp();
                break;
            case "MoveDown":
                if (CommandList.SelectedItem == null || CurrentCommandInstance == null)
                {
                    return;
                }

                if (CommandList.SelectedItem.ID != CommandList.ListItems.Count - 1)
                {
                    CurrentAIList.Commands.RemoveAt(CommandList.SelectedItem.ID);
                    CurrentAIList.Commands.Insert(CommandList.SelectedItem.ID + 1, CurrentCommandInstance);
                }

                CommandList.MoveSelectedItemDown();
                break;
        }
    }

    public override void OnDropdownChanged(string DropdownName, int SelectedIndex, string SelectedText)
    {
        switch (DropdownName)
        {
            case "SelectScript":
                CurrentAIList = DataHolder.AILists[SelectedIndex];
                CurrentCommandInstance = null;
                CurrentLabel = null;

                transform.Find("SelectAndDelete/ScriptName").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentAIList.DisplayName);

                transform.Find("SelectAndDelete/ScriptType").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent((int)(CurrentAIList.Type - 1));

                CommandList.Clear();
                foreach (AICommandInstance CurrentCommandInstance in CurrentAIList.Commands)
                {
                    CommandList.AddItemAtEnd(CurrentCommandInstance.Command.DisplayString, false);
                }

                BetterDropdown SelectLabelDropdown = transform.Find("SelectAndDelete/LabelOptions/SelectLabel").GetComponentInChildren<BetterDropdown>();

                if (CurrentAIList.Labels.Count > 0)
                {

                    SelectLabelDropdown.OptionList = CurrentAIList.LabelOptions;
                    SelectLabelDropdown.CurrentIndex = 0;
                }
                else
                {
                    SelectLabelDropdown.ClearOptions();
                }

                foreach (Transform Child in ParameterHolder)
                {
                    Destroy(Child.gameObject);
                }
                break;
            case "SelectCommand":
                CurrentCommandType = DataHolder.AICommands[SelectedIndex];

                CommandDescription.text = CurrentCommandType.Description;
                break;
            case "ScriptType":
                AIListType NewType = (AIListType)SelectedIndex + 1;

                if (CurrentAIList.Type == NewType)
                {
                    return;
                }

                bool Succeeded = CurrentAIList.ChangeType(NewType);

                if (Succeeded)
                {
                    transform.Find("SelectAndDelete/SelectScript").GetComponentInChildren<BetterDropdown>().CurrentIndex = DropdownOptions.FindAIListIndex(CurrentAIList);
                }
                else
                {
                    Debug.LogError("Cannot change script type, maximum amount of scripts in that type reached");
                }
                break;
            case "SelectLabel":
                CurrentLabel = CurrentAIList.Labels[SelectedIndex];
                transform.Find("SelectAndDelete/LabelName").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentLabel.Name);
                break;
        }
    }

    public void OnTextFieldChanged(TMP_InputField EventField, string NewValue)
    {
        string FieldName = EventField.transform.parent.name;

        switch (FieldName)
        {
            case "ScriptName":
                if (CurrentAIList == null)
                {
                    return;
                }

                CurrentAIList.DisplayName = NewValue;

                BetterDropdown SelectScriptDropdown = transform.Find("SelectAndDelete/SelectScript").GetComponentInChildren<BetterDropdown>();
                SelectScriptDropdown.OptionList[SelectScriptDropdown.CurrentIndex] = CurrentAIList.NumberString + " " + NewValue;
                SelectScriptDropdown.RefreshCurrentText();
                break;
            case "LabelName":
                if (CurrentLabel == null)
                {
                    return;
                }

                CurrentLabel.Name = NewValue;

                BetterDropdown SelectLabelDropdown = transform.Find("SelectAndDelete/LabelOptions/SelectLabel").GetComponentInChildren<BetterDropdown>();
                SelectLabelDropdown.OptionList[SelectLabelDropdown.CurrentIndex] = CurrentLabel.DisplayString;
                SelectLabelDropdown.RefreshCurrentText();
                break;
        }

    }

    public void OnCommandInstanceSelected(ListItem SelectedItem)
    {
        CurrentCommandInstance = CurrentAIList.Commands[SelectedItem.ID];
        CommandDescription.text = CurrentCommandInstance.Command.Description;

        foreach (Transform Child in ParameterHolder)
        {
            Destroy(Child.gameObject);
        }

        for (int i = 0; i < CurrentCommandInstance.Parameters.Count; ++i)
        {
            //GameObject FieldGameObject = null;

            TMP_InputField ParameterField;
            BetterDropdown ParameterDropdown;
            Toggle ParameterToggle;
            Button ParameterButton;

            object CurrentParameter = CurrentCommandInstance.Parameters[i];
            ParameterType CurrentType = CurrentCommandInstance.Command.ParameterTypes[i];

            switch (CurrentType.ClassType)
            {
                case "System.Int32":
                    ParameterField = CreateNewTextField(CurrentType, i);

                    ParameterField.characterValidation = TMP_InputField.CharacterValidation.Integer;
                    ParameterField.SetTextWithoutNotify(CurrentParameter.ToString());
                    break;
                case "Int24":
                    ParameterField = CreateNewTextField(CurrentType, i);

                    ParameterField.characterValidation = TMP_InputField.CharacterValidation.Integer;
                    ParameterField.SetTextWithoutNotify(CurrentParameter.ToString());
                    break;
                case "System.Int16":
                    ParameterField = CreateNewTextField(CurrentType, i);

                    ParameterField.characterValidation = TMP_InputField.CharacterValidation.Integer;
                    ParameterField.SetTextWithoutNotify(CurrentParameter.ToString());
                    break;
                case "System.Byte":
                    ParameterField = CreateNewTextField(CurrentType, i);

                    ParameterField.characterValidation = TMP_InputField.CharacterValidation.Integer;
                    ParameterField.SetTextWithoutNotify(CurrentParameter.ToString());
                    break;
                case "String":
                    ParameterField = CreateNewTextField(CurrentType, i);

                    ParameterField.SetTextWithoutNotify((string)CurrentParameter);
                    break;
                case "Label":
                    ParameterDropdown = CreateNewDropdown(CurrentType, i);

                    ParameterDropdown.OptionList = CurrentAIList.LabelOptions;
                    ParameterDropdown.SetSelectedOptionWithoutEvent(CurrentAIList.Labels.IndexOf((Label)CurrentParameter));
                    break;
                case "Character":
                    ParameterDropdown = CreateNewDropdown(CurrentType, i);

                    ParameterDropdown.OptionList = DropdownOptions.CharactersWithPresets;

                    if (CurrentParameter is Prop)
                    {
                        ParameterDropdown.SetSelectedOptionWithoutEvent(DropdownOptions.FindCharacterIndex((Prop)CurrentParameter, true));
                    }
                    else if (CurrentParameter is int)
                    {
                        ParameterDropdown.SetSelectedOptionWithoutEvent(ParameterDropdown.OptionList.Count); // Set to the last option, which is custom character ID

                        ParameterField = CreateNewTextField(CurrentType, i);
                        ParameterField.SetTextWithoutNotify(CurrentParameter.ToString());
                    }
                    break;
                case "AIList":
                    ParameterDropdown = CreateNewDropdown(CurrentType, i);

                    ParameterDropdown.OptionList = DropdownOptions.AssignableAILists;
                    ParameterDropdown.SetSelectedOptionWithoutEvent(DropdownOptions.FindAssignableAIListIndex((AIList)CurrentParameter));
                    break;
                case "Animation":
                    ParameterDropdown = CreateNewDropdown(CurrentType, i);

                    ParameterDropdown.OptionList = DropdownOptions.Animations;
                    ParameterDropdown.SetSelectedOptionWithoutEvent(DataHolder.Animations.IndexOf((AnimationSequence)CurrentParameter));
                    break;
                case "Model":
                    ParameterDropdown = CreateNewDropdown(CurrentType, i);

                    ParameterDropdown.OptionList = DropdownOptions.PropModels;
                    ParameterDropdown.SetSelectedOptionWithoutEvent(DropdownOptions.FindPropModelIndex((ModelData)CurrentParameter));
                    break;
                case "Pad":
                    ParameterDropdown = CreateNewDropdown(CurrentType, i);

                    ParameterDropdown.OptionList = DropdownOptions.PadsWithPreset;
                    ParameterDropdown.SetSelectedOptionWithoutEvent(DropdownOptions.FindPadIndex((Pad)CurrentParameter, true));
                    break;
                case "Path":
                    ParameterDropdown = CreateNewDropdown(CurrentType, i);

                    ParameterDropdown.OptionList = DropdownOptions.Paths;
                    ParameterDropdown.SetSelectedOptionWithoutEvent(DataHolder.Paths.IndexOf((GuardPath)CurrentParameter));
                    break;
                case "Room":
                    ParameterDropdown = CreateNewDropdown(CurrentType, i);

                    ParameterDropdown.OptionList = DropdownOptions.Rooms;
                    ParameterDropdown.SetSelectedOptionWithoutEvent(DataHolder.Rooms.IndexOf((Room)CurrentParameter));
                    break;
                case "System.Boolean":
                    ParameterToggle = CreateNewToggle(CurrentType, i);

                    ParameterToggle.SetIsOnWithoutNotify((bool)CurrentParameter);
                    break;
                case "AimEntity":
                    BitFlagCollection AimFlags = (BitFlagCollection)CurrentCommandInstance.Parameters[0];

                    if (AimFlags.HasBits(0x0004)) // Aim at character
                    {
                        ParameterDropdown = CreateNewDropdown(CurrentType, i);

                        ParameterDropdown.OptionList = DropdownOptions.CharactersWithPresets;
                        ParameterDropdown.SetSelectedOptionWithoutEvent(DropdownOptions.FindCharacterIndex((Prop)CurrentParameter, true));
                    }
                    else if (AimFlags.HasBits(0x0008)) // Aim at pad
                    {
                        ParameterDropdown = CreateNewDropdown(CurrentType, i);

                        ParameterDropdown.OptionList = DropdownOptions.PadsWithPreset;
                        ParameterDropdown.SetSelectedOptionWithoutEvent(DropdownOptions.FindPadIndex((Pad)CurrentParameter, true));
                    }
                    else if (AimFlags.HasBits(0x0010)) // Aim at direction
                    {
                        ParameterField = CreateNewTextField(CurrentType, i);

                        ParameterField.characterValidation = TMP_InputField.CharacterValidation.Integer;
                        ParameterField.SetTextWithoutNotify(CurrentParameter.ToString());
                    }
                    else if (AimFlags.HasBits(0x0200)) // Aim at target
                    {
                        ParameterToggle = CreateNewToggle(CurrentType, i);

                        ParameterToggle.SetIsOnWithoutNotify((bool)CurrentParameter);
                    }
                    else
                    {
                        ParameterField = CreateNewTextField(CurrentType, i);

                        ParameterField.characterValidation = TMP_InputField.CharacterValidation.Integer;
                        ParameterField.SetTextWithoutNotify(CurrentParameter.ToString());
                    }
                    break;
                case "Prop":
                    ParameterDropdown = CreateNewDropdown(CurrentType, i);

                    ParameterDropdown.OptionList = DropdownOptions.Tags;
                    ParameterDropdown.SetSelectedOptionWithoutEvent(DataHolder.Tags.IndexOf((Prop)CurrentParameter));
                    break;
                case "PropOrCharacter":
                    ParameterDropdown = CreateNewDropdown(CurrentType, i);

                    ParameterDropdown.OptionList = DropdownOptions.TagsAndCharacterPresets;

                    Prop PropOrCharacter = (Prop)CurrentParameter;

                    if (PropOrCharacter.IsAbstract) // Character preset
                    {
                        ParameterDropdown.SetSelectedOptionWithoutEvent(DataHolder.Characters.IndexOf(PropOrCharacter));
                    }
                    else // Regular prop tag
                    {
                        ParameterDropdown.SetSelectedOptionWithoutEvent(DataHolder.Tags.IndexOf(PropOrCharacter) + 12);
                    }
                    break;
                case "Objective":
                    ParameterDropdown = CreateNewDropdown(CurrentType, i);

                    ParameterDropdown.OptionList = DropdownOptions.Objectives;
                    ParameterDropdown.SetSelectedOptionWithoutEvent(DataHolder.Objectives.IndexOf((Objective)CurrentParameter));
                    break;
                case "Stage":
                    ParameterDropdown = CreateNewDropdown(CurrentType, i);

                    ParameterDropdown.OptionList = DropdownOptions.Stages;
                    ParameterDropdown.SetSelectedOptionWithoutEvent(DataHolder.Stages.IndexOf((Stage)CurrentParameter));
                    break;
                case "Body":
                    ParameterDropdown = CreateNewDropdown(CurrentType, i);

                    ParameterDropdown.OptionList = DropdownOptions.BodyModels;
                    ParameterDropdown.SetSelectedOptionWithoutEvent(DropdownOptions.FindBodyModelIndex((ModelData)CurrentParameter));
                    break;
                case "Head":
                    ParameterDropdown = CreateNewDropdown(CurrentType, i);

                    ParameterDropdown.OptionList = DropdownOptions.HeadModels;
                    ParameterDropdown.SetSelectedOptionWithoutEvent(DropdownOptions.FindHeadModelIndex((ModelData)CurrentParameter));
                    break;
                case "LanguageString":
                    ParameterButton = CreateNewButton(CurrentType, i);
                    /*ParameterDropdown = CreateNewDropdown(CurrentType, i);

                    ParameterDropdown.OptionList = DropdownOptions.LanguageStrings;
                    ParameterDropdown.SetSelectedOptionWithoutEvent(DropdownOptions.FindLanguageStringIndex((LanguageString)CurrentParameter));*/
                    break;
                case "SoundEffect":
                    ParameterDropdown = CreateNewDropdown(CurrentType, i);

                    ParameterDropdown.OptionList = DropdownOptions.SoundEffects;
                    ParameterDropdown.SetSelectedOptionWithoutEvent(DataHolder.SoundEffects.IndexOf((SoundEffect)CurrentParameter));
                    break;
                case "ScreenImage":
                    ParameterDropdown = CreateNewDropdown(CurrentType, i);

                    ParameterDropdown.OptionList = DropdownOptions.ScreenImages;
                    ParameterDropdown.SetSelectedOptionWithoutEvent(DataHolder.ScreenImages.IndexOf((ScreenImage)CurrentParameter));
                    break;
                case "MusicTrack":
                    ParameterDropdown = CreateNewDropdown(CurrentType, i);

                    ParameterDropdown.OptionList = DropdownOptions.MusicTracks;
                    ParameterDropdown.SetSelectedOptionWithoutEvent(DataHolder.MusicTracks.IndexOf((MusicTrack)CurrentParameter));
                    break;
                case "Portal":
                    ParameterDropdown = CreateNewDropdown(CurrentType, i);

                    ParameterDropdown.OptionList = DropdownOptions.Portals;
                    ParameterDropdown.SetSelectedOptionWithoutEvent(DataHolder.Portals.IndexOf((Portal)CurrentParameter));
                    break;
                case "Quip":
                    ParameterField = CreateNewTextField(CurrentType, i);

                    ParameterField.characterValidation = TMP_InputField.CharacterValidation.Integer;
                    ParameterField.SetTextWithoutNotify(CurrentParameter.ToString());
                    break;
                case "SpecialSoundBank":
                    ParameterField = CreateNewTextField(CurrentType, i);

                    ParameterField.characterValidation = TMP_InputField.CharacterValidation.Integer;
                    ParameterField.SetTextWithoutNotify(CurrentParameter.ToString());
                    break;
                case "Constant":
                    ParameterDropdown = CreateNewDropdown(CurrentType, i);

                    ParameterDropdown.OptionList = DropdownOptions.GetConstantOptions(CurrentType.ConstantName);
                    ParameterDropdown.SetSelectedOptionWithoutEvent(DropdownOptions.GetConstantIndex((Constant)CurrentParameter));
                    break;
                case "BitFlag":
                    ParameterButton = CreateNewButton(CurrentType, i);
                    break;
                case "null":
                    break;
                default:
                    Debug.LogError("Unrecognized parameter: " + CurrentType?.ClassType);
                    break;
            }
        }
        
        MarkAsNeedUpdating();

        //Debug.Log("updated");
    }

    public void OnParameterTextFieldChanged(TMP_InputField EventField, int ID)
    {
        Debug.Log("Text field has been changed: " + EventField.text);

        string NewValue = EventField.text;

        if (NewValue == "" || NewValue == "-")
        {
            return;
        }

        CurrentCommandInstance.Parameters[ID] = int.Parse(NewValue);
    }

    public void OnParameterDropdownChanged(int ID, int SelectedIndex, string SelectedText)
    {
        Debug.Log("Dropdown has been changed: " + SelectedIndex);
        ParameterType CurrentType = CurrentCommandInstance.Command.ParameterTypes[ID];

        switch (CurrentType.ClassType)
        {
            case "Label":
                CurrentCommandInstance.Parameters[ID] = CurrentAIList.Labels[SelectedIndex];
                break;
            case "Character":
                CurrentCommandInstance.Parameters[ID] = DataHolder.Characters[SelectedIndex];
                break;
            case "AIList":
                CurrentCommandInstance.Parameters[ID] = DropdownOptions.FindAssignableAIList(SelectedText);
                break;
            case "Animation":
                CurrentCommandInstance.Parameters[ID] = DataHolder.Animations[SelectedIndex];
                break;
            case "Model":
                CurrentCommandInstance.Parameters[ID] = DropdownOptions.FindPropModel(SelectedText);
                break;
            case "Pad":
                CurrentCommandInstance.Parameters[ID] = DropdownOptions.FindPad(SelectedText);
                break;
            case "Path":
                CurrentCommandInstance.Parameters[ID] = DataHolder.Paths[SelectedIndex];
                break;
            case "Room":
                CurrentCommandInstance.Parameters[ID] = DataHolder.Rooms[SelectedIndex];
                break;
            case "AimEntity":
                BitFlagCollection AimFlags = (BitFlagCollection)CurrentCommandInstance.Parameters[0];

                if (AimFlags.HasBits(0x0004)) // Aim at character
                {
                    CurrentCommandInstance.Parameters[ID] = DataHolder.Characters[SelectedIndex];
                }
                else if (AimFlags.HasBits(0x0008)) // Aim at pad
                {
                    CurrentCommandInstance.Parameters[ID] = DropdownOptions.FindPad(SelectedText);
                }
                break;
            case "Prop":
                CurrentCommandInstance.Parameters[ID] = DropdownOptions.FindTag(SelectedIndex, false);
                break;
            case "Objective":
                CurrentCommandInstance.Parameters[ID] = DataHolder.Objectives[SelectedIndex];
                break;
            case "Stage":
                CurrentCommandInstance.Parameters[ID] = DataHolder.Stages[SelectedIndex];
                break;
            case "Body":
            case "Head":
                CurrentCommandInstance.Parameters[ID] = DropdownOptions.FindCharacterModel(SelectedText);
                break;
            case "SoundEffect":
                CurrentCommandInstance.Parameters[ID] = DataHolder.SoundEffects[SelectedIndex];
                break;
            case "ScreenImage":
                CurrentCommandInstance.Parameters[ID] = DataHolder.ScreenImages[SelectedIndex];
                break;
            case "MusicTrack":
                CurrentCommandInstance.Parameters[ID] = DataHolder.MusicTracks[SelectedIndex];
                break;
            case "Portal":
                CurrentCommandInstance.Parameters[ID] = DataHolder.Portals[SelectedIndex];
                break;
            case "Quip":
                CurrentCommandInstance.Parameters[ID] = DropdownOptions.GetConstant("QUIP", SelectedIndex);
                break;
            case "Constant":
                CurrentCommandInstance.Parameters[ID] = DropdownOptions.GetConstant(CurrentType.ConstantName, SelectedIndex);
                break;
            default:
                Debug.Log("Unrecognized parameter: " + CurrentType?.ClassType);
                break;
        }
    }

    public void OnParameterToggleClicked(Toggle EventToggle, int ID)
    {
        CurrentCommandInstance.Parameters[ID] = EventToggle.isOn;
    }

    public void OnParameterButtonClicked(int ID)
    {
        FlagsEditorInstance.EnterAICommandInstance(CurrentCommandInstance, ID);
    }

    private TMP_InputField CreateNewTextField(ParameterType CurrentType, int ID)
    {
        GameObject FieldGameObject = Instantiate(TextFieldTemplate, ParameterHolder);
        FieldGameObject.GetComponentInChildren<TextMeshProUGUI>().text = CurrentType.Name ?? "Parameter";

        TMP_InputField FieldComponent = FieldGameObject.GetComponentInChildren<TMP_InputField>();
        FieldComponent.onValueChanged.AddListener(x => OnParameterTextFieldChanged(FieldComponent, ID));

        return FieldComponent;
    }

    private BetterDropdown CreateNewDropdown(ParameterType CurrentType, int ID)
    {
        GameObject FieldGameObject = Instantiate(DropdownTemplate, ParameterHolder);
        FieldGameObject.GetComponentInChildren<TextMeshProUGUI>().text = CurrentType.Name ?? "Parameter";

        BetterDropdown FieldComponent = FieldGameObject.GetComponentInChildren<BetterDropdown>();
        FieldComponent.OptionSelected.AddListener((SelectedIndex, SelectedText) => OnParameterDropdownChanged(ID, SelectedIndex, SelectedText));

        return FieldComponent;
    }

    private Button CreateNewButton(ParameterType CurrentType, int ID)
    {
        GameObject FieldGameObject = Instantiate(ButtonTemplate, ParameterHolder);
        FieldGameObject.GetComponentInChildren<TextMeshProUGUI>().text = CurrentType.Name ?? "Parameter";

        Button FieldComponent = FieldGameObject.GetComponentInChildren<Button>();
        FieldComponent.onClick.AddListener(() => OnParameterButtonClicked(ID));

        return FieldComponent;
    }

    private Toggle CreateNewToggle(ParameterType CurrentType, int ID)
    {
        GameObject FieldGameObject = Instantiate(ToggleTemplate, ParameterHolder);
        FieldGameObject.GetComponentInChildren<TextMeshProUGUI>().text = CurrentType.Name ?? "Parameter";

        Toggle FieldComponent = FieldGameObject.GetComponentInChildren<Toggle>();
        FieldComponent.onValueChanged.AddListener(x => OnParameterToggleClicked(FieldComponent, ID));

        return FieldComponent;
    }
}
