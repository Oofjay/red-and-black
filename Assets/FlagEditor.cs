using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FlagEditor : AttributeEditor
{
    BetterDropdown SelectStageFlagDropdown;
    BetterDropdown SelectLockFlagDropdown;

    BitFlag CurrentStageFlag;
    BitFlag CurrentLockFlag;

    public ScrollableList StageFlagUsageList;
    public ScrollableList LockFlagUsageList;

    public ScriptEditor ScriptEditorInstance;
    public ObjectiveEditor ObjectiveEditorInstance;

    List<object> CurrentStageFlagUsages = new();

    // Start is called before the first frame update
    void Awake()
    {
        SelectStageFlagDropdown = transform.Find("StageFlags/SelectStageFlag").GetComponentInChildren<BetterDropdown>();
        SelectLockFlagDropdown = transform.Find("LockFlags/SelectLockFlag").GetComponentInChildren<BetterDropdown>();

        SelectStageFlagDropdown.OptionList = DropdownOptions.StageFlags;
        SelectLockFlagDropdown.OptionList = DropdownOptions.LockFlags;
    }

    public override void OnDropdownChanged(string DropdownName, int SelectedIndex, string SelectedText)
    {
        if (DropdownName == "SelectStageFlag")
        {
            CurrentStageFlag = GameBitFlagHolder.GetStageFlags[SelectedIndex];
            transform.Find("StageFlags/StageFlagName").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentStageFlag.Name ?? "");

            StageFlagUsageList.Clear();
            CurrentStageFlagUsages.Clear();

            List<string> AIListNames = new();
            foreach (AIListUsage CurrentUsage in CurrentStageFlag.ConnectedAICommands)
            {
                AIList CurrentAIList = CurrentUsage.CommandInstance.ParentAIList;
                if (!AIListNames.Contains("Script " + CurrentAIList.NumberString + ": " + CurrentAIList.DisplayName))
                {
                    AIListNames.Add("Script " + CurrentAIList.NumberString + ": " + CurrentAIList.DisplayName);
                    CurrentStageFlagUsages.Add(CurrentUsage);
                }
            }
            StageFlagUsageList.AddToList(AIListNames);

            foreach (ObjectiveCondition CurrentCondition in CurrentStageFlag.ConnectedObjectiveConditions)
            {
                StageFlagUsageList.AddItemAtEnd("Objective " + (CurrentCondition.AttachedObjective.ID + 1) + ": " + CurrentCondition.ConditionName, false);
                CurrentStageFlagUsages.Add(CurrentCondition);
            }
        }
        else if (DropdownName == "SelectLockFlag")
        {
            CurrentLockFlag = GameBitFlagHolder.GetLockFlags[SelectedIndex];
            transform.Find("LockFlags/LockFlagName").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentLockFlag.Name ?? "");

            LockFlagUsageList.Clear();

            foreach (Prop CurrentProp in CurrentLockFlag.ConnectedProps)
            {
                LockFlagUsageList.AddItemAtEnd(CurrentProp.GetName(), false);
            }
        }
    }

    public void OnButtonClicked(Button EventButton)
    {
        string FieldName = EventButton.name;

        if (FieldName == "AddStageFlag")
        {
            BitFlag NewStageFlag = GameBitFlagHolder.CreateNewStageFlag();

            if (NewStageFlag == null)
            {
                Debug.LogError("Too many BitFlags, could not create a new one");
                return;
            }

            DropdownOptions.StageFlags.Add(NewStageFlag.Name);

            if (GameBitFlagHolder.GetStageFlags.Count == 1)
            {
                SelectStageFlagDropdown.CurrentIndex = 0;
                transform.Find("StageFlags/StageFlagName").gameObject.SetActive(true);
            }
            else
            {
                SelectStageFlagDropdown.CurrentIndex = GameBitFlagHolder.GetStageFlags.Count - 1;
            }
        }
        else if (FieldName == "DeleteStageFlag")
        {
            if (GameBitFlagHolder.GetStageFlags.Count == 0)
            {
                return;
            }

            GameBitFlagHolder.DeleteStageFlag(CurrentStageFlag);

            if (SelectStageFlagDropdown.OptionList.Count == 0)
            {
                SelectStageFlagDropdown.RefreshCurrentText();
            }
            else if (SelectStageFlagDropdown.CurrentIndex == 0)
            {
                SelectStageFlagDropdown.CurrentIndex = 0;
            }
            else
            {
                SelectStageFlagDropdown.CurrentIndex--;
            }

            if (GameBitFlagHolder.GetStageFlags.Count == 0)
            {
                CurrentStageFlag = null;

                transform.Find("StageFlags/StageFlagName").gameObject.SetActive(false);

                StageFlagUsageList.Clear();
            }
        }
        else if (FieldName == "AddLockFlag")
        {
            BitFlag NewLockFlag = GameBitFlagHolder.CreateNewLockFlag();

            if (NewLockFlag == null)
            {
                Debug.LogError("Too many BitFlags, could not create a new one");
                return;
            }

            DropdownOptions.LockFlags.Add(NewLockFlag.Name);

            if (GameBitFlagHolder.GetLockFlags.Count == 1)
            {
                SelectLockFlagDropdown.CurrentIndex = 0;
                transform.Find("LockFlags/LockFlagName").gameObject.SetActive(true);
            }
            else
            {
                SelectLockFlagDropdown.CurrentIndex = GameBitFlagHolder.GetLockFlags.Count - 1;
            }
        }
        else if (FieldName == "DeleteLockFlag")
        {
            if (GameBitFlagHolder.GetLockFlags.Count == 0)
            {
                return;
            }

            GameBitFlagHolder.DeleteLockFlag(CurrentLockFlag);

            if (SelectLockFlagDropdown.OptionList.Count == 0)
            {
                SelectLockFlagDropdown.RefreshCurrentText();
            }
            else if (SelectLockFlagDropdown.CurrentIndex == 0)
            {
                SelectLockFlagDropdown.CurrentIndex = 0;
            }
            else
            {
                SelectLockFlagDropdown.CurrentIndex--;
            }

            if (GameBitFlagHolder.GetLockFlags.Count == 0)
            {
                CurrentLockFlag = null;

                transform.Find("LockFlags/LockFlagName").gameObject.SetActive(false);

                LockFlagUsageList.Clear();
            }
        }
        else if (FieldName == "GoToProp")
        {
            if (CurrentLockFlag == null || CurrentLockFlag.ConnectedProps.Count == 0 || LockFlagUsageList.SelectedItem == null)
            {
                return;
            }

            Pad LookPad;
            Prop LookProp = CurrentLockFlag.ConnectedProps[LockFlagUsageList.SelectedItem.ID];

            if (LookProp.ParentPad != null) // Attached to pad
            {
                LookPad = LookProp.ParentPad;
            }
            else if (LookProp?.ParentProp.ParentPad != null) // Attached to character or other prop
            {
                LookPad = LookProp.ParentProp.ParentPad;
            }
            else
            {
                return;
            }

            Vector3 CameraPosition = LookPad.transform.position;
            CameraPosition.y += 200;
            CameraPosition.z += 150;

            //DataHolder.MainCameraInstance.SetCameraPositionAndPivot(CameraPosition, LookPad.transform.position);
        }
        else if (FieldName == "GoToScriptOrObjective")
        {
            if (CurrentStageFlag == null || StageFlagUsageList.SelectedItem == null || (CurrentStageFlag.ConnectedAICommands.Count == 0 && CurrentStageFlag.ConnectedObjectiveConditions.Count == 0))
            {
                return;
            }

            if (StageFlagUsageList.SelectedItem.Value.StartsWith("Script"))
            {
                AIListUsage Usage = (AIListUsage)CurrentStageFlagUsages[StageFlagUsageList.SelectedItem.ID];

                ScriptEditorInstance.EnterScript(Usage.CommandInstance.ParentAIList);

                GetComponentInParent<FloatingWindow>().SwitchToTab(3);
            }
            else // Objective
            {
                ObjectiveCondition Condition = (ObjectiveCondition)CurrentStageFlagUsages[StageFlagUsageList.SelectedItem.ID];

                ObjectiveEditorInstance.EnterObjective(Condition.AttachedObjective, Condition);

                GetComponentInParent<FloatingWindow>().SwitchToTab(2);
            }
        }
    }

    public void OnTextFieldChanged(TMP_InputField EventField, string NewValue)
    {
        string FieldName = EventField.transform.parent.name;

        if (FieldName == "StageFlagName")
        {
            CurrentStageFlag.Name = NewValue;
            SelectStageFlagDropdown.OptionList[SelectStageFlagDropdown.CurrentIndex] = NewValue;
            DropdownOptions.StageFlagsWithNone[SelectStageFlagDropdown.CurrentIndex + 1] = NewValue;
            SelectStageFlagDropdown.RefreshCurrentText();
        }
        else if (FieldName == "LockFlagName")
        {
            CurrentLockFlag.Name = NewValue;
            SelectLockFlagDropdown.OptionList[SelectLockFlagDropdown.CurrentIndex] = NewValue;
            DropdownOptions.LockFlagsWithNone[SelectStageFlagDropdown.CurrentIndex + 1] = NewValue;
            SelectLockFlagDropdown.RefreshCurrentText();
        }
    }
}
