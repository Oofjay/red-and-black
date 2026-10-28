using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveEditor : AttributeEditor
{
    List<string> ConditionDropdownOptions = new();

    Objective CurrentObjective;
    ObjectiveCondition CurrentCondition;

    BetterDropdown SelectObjectiveDropdown;

    void Awake()
    {
        SelectObjectiveDropdown = transform.Find("Objectives/SelectObjective").GetComponentInChildren<BetterDropdown>(true);
        SelectObjectiveDropdown.OptionList = DropdownOptions.Objectives;

        List<string> ConditionNames = new()
        {
            "Destroy object",
            "Completion flags",
            "Failure flags",
            "Collect object",
            "Deposit object anywhere",
            "Holograph object",
            "Enter room"
            //"Deposit object on target (beta)",
            //"Need key analyzer (beta)"
        };

        transform.Find("Objectives/ConditionType").GetComponentInChildren<BetterDropdown>().OptionList = ConditionNames;

        transform.Find("Briefing/StageTitle").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.LanguageStrings;
        transform.Find("Briefing/AgentBriefing").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.LanguageStrings;
        transform.Find("Briefing/SpecialAgentBriefing").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.LanguageStrings;
        transform.Find("Briefing/PerfectAgentBriefing").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.LanguageStrings;

        transform.Find("Objectives/DisplayText").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.LanguageStrings;

        transform.Find("Objectives/Tag").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.TagsWithNone;
        transform.Find("Objectives/StageFlag").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.StageFlagsWithNone;
        transform.Find("Objectives/Room").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.Rooms;
    }

    void Start()
    {
        transform.Find("Briefing/StageTitle").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.FindLanguageStringIndex(Briefing.StageTitle));
        transform.Find("Briefing/AgentBriefing").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.FindLanguageStringIndex(Briefing.AgentBriefing));
        transform.Find("Briefing/SpecialAgentBriefing").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.FindLanguageStringIndex(Briefing.SpecialAgentBriefing));
        transform.Find("Briefing/PerfectAgentBriefing").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.FindLanguageStringIndex(Briefing.PerfectAgentBriefing));
    }

    public void EnterObjective(Objective InObjective, ObjectiveCondition InCondition)
    {
        gameObject.SetActive(true);

        SelectObjectiveDropdown.TemporarilyDisableDefaultOptionOnEnable = true;
        SelectObjectiveDropdown.SetSelectedOption(InObjective.ID);

        transform.Find("Objectives/SelectCondition").GetComponentInChildren<BetterDropdown>().SetSelectedOption(InCondition.ID);
    }

    public override void OnDropdownChanged(string DropdownName, int SelectedIndex, string SelectedText)
    {
        if (DropdownName == "SelectObjective")
        {
            CurrentObjective = DataHolder.Objectives[SelectedIndex];
            if (CurrentObjective.Conditions.Count > 0)
            {
                CurrentCondition = CurrentObjective.Conditions[0];
            }
            else
            {
                CurrentCondition = null;
            }

            transform.Find("Objectives/DisplayText").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.FindLanguageStringIndex(CurrentObjective.DisplayText));
            transform.Find("Objectives/Agent").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentObjective.Agent);
            transform.Find("Objectives/SpecialAgent").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentObjective.SpecialAgent);
            transform.Find("Objectives/PerfectAgent").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentObjective.PerfectAgent);
            transform.Find("Objectives/PerfectDark").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentObjective.PerfectDark);

            RefreshConditionDropdown();

            transform.Find("Objectives/SelectCondition").GetComponentInChildren<BetterDropdown>().OptionList = ConditionDropdownOptions;
            transform.Find("Objectives/SelectCondition").GetComponentInChildren<BetterDropdown>().SetSelectedOption(0);

            if (CurrentObjective.Conditions.Count > 0)
            {
                transform.Find("Objectives/ConditionType").gameObject.SetActive(true);
            }
            else
            {
                transform.Find("Objectives/ConditionType").gameObject.SetActive(false);
                transform.Find("Objectives/StageFlag").gameObject.SetActive(false);
                transform.Find("Objectives/Tag").gameObject.SetActive(false);
                transform.Find("Objectives/Room").gameObject.SetActive(false);
            }
        }
        else if (DropdownName == "DisplayText")
        {
            if (CurrentObjective == null)
            {
                return;
            }

            LanguageString DisplayText = LanguageFileHolder.GetLevelLanguageFile().LanguageStrings[SelectedIndex];
            DropdownOptions.Objectives[SelectObjectiveDropdown.CurrentIndex] = $"{CurrentObjective.ID + 1}: {DisplayText.en}";
            SelectObjectiveDropdown.RefreshCurrentText();
            CurrentObjective.DisplayText = DisplayText;
        }
        else if (DropdownName == "SelectCondition")
        {
            CurrentCondition = CurrentObjective.Conditions[SelectedIndex];

            transform.Find("Objectives/ConditionType").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent((int)CurrentCondition.Type);

            switch (CurrentCondition.Type)
            {
                case ConditionType.DestroyObject:
                case ConditionType.CollectObject:
                case ConditionType.DepositObject:
                case ConditionType.HolographObject:
                    transform.Find("Objectives/StageFlag").gameObject.SetActive(false);
                    transform.Find("Objectives/Room").gameObject.SetActive(false);

                    transform.Find("Objectives/Tag").gameObject.SetActive(true);

                    if (CurrentCondition.ObjectiveTag == null)
                    {
                        transform.Find("Objectives/Tag").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(0);
                    }
                    else
                    {
                        transform.Find("Objectives/Tag").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(CurrentCondition.ObjectiveTag.TagID + 1);
                    }
                    break;
                case ConditionType.CompletionFlags:
                case ConditionType.FailureFlags:
                    transform.Find("Objectives/Tag").gameObject.SetActive(false);
                    transform.Find("Objectives/Room").gameObject.SetActive(false);

                    transform.Find("Objectives/StageFlag").gameObject.SetActive(true);

                    if (CurrentCondition.StageFlag == null)
                    {
                        transform.Find("Objectives/StageFlag").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(0);
                    }
                    else
                    {
                        transform.Find("Objectives/StageFlag").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(GameBitFlagHolder.GetStageFlags.IndexOf(CurrentCondition.StageFlag) + 1);
                    }
                    break;
                case ConditionType.EnterRoom:
                    transform.Find("Objectives/Tag").gameObject.SetActive(false);
                    transform.Find("Objectives/StageFlag").gameObject.SetActive(false);

                    transform.Find("Objectives/Room").gameObject.SetActive(true);
                    transform.Find("Objectives/Room").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(CurrentCondition.ObjectiveRoom.ID - 1);
                    break;
            }
        }
        else if (DropdownName == "ConditionType")
        {
            BetterDropdown SelectConditionDropdown = transform.Find("Objectives/SelectCondition").GetComponentInChildren<BetterDropdown>();
            CurrentCondition.ChangeConditionType((ConditionType)SelectedIndex);

            SelectConditionDropdown.OptionList[SelectConditionDropdown.CurrentIndex] = $"{CurrentCondition.ID + 1}: {CurrentCondition.ConditionName}";
            SelectConditionDropdown.RefreshCurrentText();

            switch (CurrentCondition.Type)
            {
                case ConditionType.DestroyObject:
                case ConditionType.CollectObject:
                case ConditionType.DepositObject:
                case ConditionType.HolographObject:
                    transform.Find("Objectives/StageFlag").gameObject.SetActive(false);
                    transform.Find("Objectives/Room").gameObject.SetActive(false);

                    transform.Find("Objectives/Tag").gameObject.SetActive(true);
                    transform.Find("Objectives/Tag").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(0);
                    break;
                case ConditionType.CompletionFlags:
                case ConditionType.FailureFlags:
                    transform.Find("Objectives/Tag").gameObject.SetActive(false);
                    transform.Find("Objectives/Room").gameObject.SetActive(false);

                    transform.Find("Objectives/StageFlag").gameObject.SetActive(true);
                    transform.Find("Objectives/Tag").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(0);
                    break;
                case ConditionType.EnterRoom:
                    transform.Find("Objectives/Tag").gameObject.SetActive(false);
                    transform.Find("Objectives/StageFlag").gameObject.SetActive(false);

                    transform.Find("Objectives/Room").gameObject.SetActive(true);
                    transform.Find("Objectives/Room").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(0);
                    break;
            }
        }
        else if (DropdownName == "StageFlag")
        {
            if (SelectedIndex == 0)
            {
                CurrentCondition.AddNewStageFlag(null);
            }
            else
            {
                CurrentCondition.AddNewStageFlag(GameBitFlagHolder.GetStageFlags[SelectedIndex - 1]);
            }
        }
        else if (DropdownName == "Tag")
        {
            if (SelectedIndex == 0)
            {
                CurrentCondition.AddNewTag(null);
            }
            else
            {
                CurrentCondition.AddNewTag(DataHolder.Tags[SelectedIndex - 1]);
            }
        }
        else if (DropdownName == "Room")
        {
            CurrentCondition.AddNewRoom(DataHolder.Rooms[SelectedIndex]);
        }
    }

    public void OnToggleClicked(Toggle EventToggle, bool NewValue)
    {
        string FieldName = EventToggle.transform.parent.name;

        if (CurrentObjective == null)
        { 
            return;
        }

        if (FieldName == "Agent")
        {
            CurrentObjective.Agent = NewValue;
        }
        else if (FieldName == "SpecialAgent")
        {
            CurrentObjective.SpecialAgent = NewValue;
        }
        else if (FieldName == "PerfectAgent")
        {
            CurrentObjective.PerfectAgent = NewValue;
        }
        else if (FieldName == "PerfectDark")
        {
            CurrentObjective.PerfectDark = NewValue;
        }
    }

    public void OnButtonClicked(Button EventButton)
    {
        string FieldName = EventButton.name;

        if (FieldName == "AddObjective")
        {
            Objective NewObjective = new() { DisplayText = LanguageFileHolder.GetLevelLanguageFile().LanguageStrings[0], ID = DataHolder.Objectives.Count };
            DataHolder.Objectives.Add(NewObjective);

            DropdownOptions.Objectives.Add($"{NewObjective.ID + 1}: {NewObjective.DisplayText.Abbreviate()}");

            if (DropdownOptions.Objectives.Count == 1)
            {
                SelectObjectiveDropdown.CurrentIndex = 0;
            }
            else
            {
                SelectObjectiveDropdown.CurrentIndex = DropdownOptions.Objectives.Count - 1;
            }
        }
        else if (FieldName == "DeleteObjective")
        {
            if (DataHolder.Objectives.Count == 0)
            {
                return;
            }

            DataHolder.DeleteObjective(CurrentObjective);

            if (SelectObjectiveDropdown.OptionList.Count == 0)
            {
                SelectObjectiveDropdown.RefreshCurrentText();
            }
            else if (SelectObjectiveDropdown.CurrentIndex == 0)
            {
                SelectObjectiveDropdown.CurrentIndex = 0;
            }
            else
            {
                SelectObjectiveDropdown.CurrentIndex--;
            }

            Debug.Log(DataHolder.Objectives.Count);
            if (DataHolder.Objectives.Count == 0)
            {
                CurrentObjective = null;
                CurrentCondition = null;

                transform.Find("Objectives/Agent").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(false);
                transform.Find("Objectives/SpecialAgent").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(false);
                transform.Find("Objectives/PerfectAgent").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(false);
                transform.Find("Objectives/PerfectDark").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(false);

                ConditionDropdownOptions.Clear();
                transform.Find("Objectives/SelectCondition").GetComponentInChildren<BetterDropdown>().RefreshCurrentText();
                transform.Find("Objectives/ConditionType").gameObject.SetActive(false);
                transform.Find("Objectives/StageFlag").gameObject.SetActive(false);
                transform.Find("Objectives/Tag").gameObject.SetActive(false);
                transform.Find("Objectives/Room").gameObject.SetActive(false);
            }
        }
        if (FieldName == "AddCondition")
        {
            if (CurrentObjective == null)
            {
                return;
            }

            if (CurrentObjective.Conditions.Count == 0)
            {
                transform.Find("Objectives/ConditionType").gameObject.SetActive(true);
            }

            ObjectiveCondition NewCondition = new() { Type = 0, ID = CurrentObjective.Conditions.Count };
            CurrentObjective.Conditions.Add(NewCondition);
            ConditionDropdownOptions.Add($"{NewCondition.ID + 1}: {NewCondition.ConditionName}");

            if (ConditionDropdownOptions.Count == 1)
            {
                transform.Find("Objectives/SelectCondition").GetComponentInChildren<BetterDropdown>().CurrentIndex = 0;
            }
            else
            {
                transform.Find("Objectives/SelectCondition").GetComponentInChildren<BetterDropdown>().CurrentIndex = ConditionDropdownOptions.Count - 1;
            }
        }
        else if (FieldName == "DeleteCondition")
        {
            if (CurrentObjective == null || CurrentObjective.Conditions.Count == 0)
            {
                return;
            }
            
            CurrentObjective.DeleteCondition(CurrentCondition);

            BetterDropdown SelectConditionDropdown = transform.Find("Objectives/SelectCondition").GetComponentInChildren<BetterDropdown>();
            RefreshConditionDropdown();

            if (SelectConditionDropdown.OptionList.Count == 0)
            {
                SelectConditionDropdown.RefreshCurrentText();
            }
            else if (SelectConditionDropdown.CurrentIndex == 0)
            {
                SelectConditionDropdown.CurrentIndex = 0;
            }
            else
            {
                SelectConditionDropdown.CurrentIndex--;
            }

            if (CurrentObjective.Conditions.Count == 0)
            {
                CurrentCondition = null;

                transform.Find("Objectives/ConditionType").gameObject.SetActive(false);
                transform.Find("Objectives/StageFlag").gameObject.SetActive(false);
                transform.Find("Objectives/Tag").gameObject.SetActive(false);
                transform.Find("Objectives/Room").gameObject.SetActive(false);
            }
        }
    }

    void RefreshConditionDropdown()
    {
        ConditionDropdownOptions.Clear();

        foreach (ObjectiveCondition IteratedCondition in CurrentObjective.Conditions)
        {
            ConditionDropdownOptions.Add($"{IteratedCondition.ID + 1}: {IteratedCondition.ConditionName}");
        }
    }
}
