using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VisibilityEditor : AttributeEditor
{
    int CurrentCommandType = 0;

    VisibilityCommand CurrentCommand;

    ScrollableList CommandList;

    GameObject TextField;
    GameObject FirstDropdown;
    GameObject SecondDropdown;

    // Start is called before the first frame update
    void Awake()
    {
        transform.Find("CommandOptions/SelectCommand").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.VisibilityCommands;

        CommandList = transform.Find("VisibilityCommands").GetComponentInChildren<ScrollableList>();
        CommandList.ListItemSelected += OnVisibilityCommandSelected;

        TextField = transform.Find("Parameters/TextField").gameObject;
        FirstDropdown = transform.Find("Parameters/FirstDropdown").gameObject;
        SecondDropdown = transform.Find("Parameters/SecondDropdown").gameObject;
    }

    private void Start()
    {
        CommandList.Clear();

        foreach (VisibilityCommand CurrentCommand in DataHolder.VisibilityCommands)
        {
            CommandList.AddItemAtEnd(CurrentCommand.DisplayString, false);
        }
    }

    private void OnEnable()
    {
        CommandList.Clear();

        foreach (VisibilityCommand CurrentCommand in DataHolder.VisibilityCommands)
        {
            CommandList.AddItemAtEnd(CurrentCommand.DisplayString, false);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnVisibilityCommandSelected(ListItem SelectedItem)
    {
        CurrentCommand = DataHolder.VisibilityCommands[SelectedItem.ID];

        FirstDropdown.GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.Rooms;

        switch (CurrentCommand.Type)
        {
            // Number
            case 0x01:
                TextField.SetActive(true);
                FirstDropdown.SetActive(false);
                SecondDropdown.SetActive(false);

                int NumberArgument = CurrentCommand.Arguments[0];

                TextField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(NumberArgument.ToString());
                break;
            // One room
            case 0x20:
            case 0x24:
            case 0x26:
            case 0x2A:
                TextField.SetActive(false);
                FirstDropdown.SetActive(true);
                SecondDropdown.SetActive(false);

                FirstDropdown.GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.Rooms;
                FirstDropdown.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(CurrentCommand.Arguments[0] - 1);
                FirstDropdown.GetComponentInChildren<TextMeshProUGUI>().text = "Room";
                break;
            // Two rooms
            case 0x14:
            case 0x25:
            case 0x27:
                TextField.SetActive(false);
                FirstDropdown.SetActive(true);
                SecondDropdown.SetActive(true);

                FirstDropdown.GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.Rooms;
                FirstDropdown.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(CurrentCommand.Arguments[0] - 1);
                FirstDropdown.GetComponentInChildren<TextMeshProUGUI>().text = "Room 1";

                SecondDropdown.GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.Rooms;
                SecondDropdown.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(CurrentCommand.Arguments[1] - 1);
                SecondDropdown.GetComponentInChildren<TextMeshProUGUI>().text = "Room 2";
                break;
            // Portal
            case 0x1F:
            case 0x22:
            case 0x23:
            case 0x29:
                TextField.SetActive(false);
                FirstDropdown.SetActive(true);
                SecondDropdown.SetActive(false);

                FirstDropdown.GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.Portals;
                FirstDropdown.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(CurrentCommand.Arguments[0]);
                FirstDropdown.GetComponentInChildren<TextMeshProUGUI>().text = "Portal";
                break;
            default:
                TextField.SetActive(false);
                FirstDropdown.SetActive(false);
                SecondDropdown.SetActive(false);
                break;
        }
    }

    public override void OnDropdownChanged(string DropdownName, int SelectedIndex, string SelectedText)
    {
        if (DropdownName == "SelectCommand")
        {
            CurrentCommandType = VisibilityCommand.CommandTypes[SelectedIndex];
        }
        else if (DropdownName == "FirstDropdown")
        {
            switch (CurrentCommand.Type)
            {
                // Room
                case 0x20:
                case 0x24:
                case 0x26:
                case 0x2A:
                case 0x14:
                case 0x25:
                case 0x27:
                    CurrentCommand.Arguments[0] = SelectedIndex + 1;
                    break;
                // Portal
                case 0x1F:
                case 0x22:
                case 0x23:
                case 0x29:
                    CurrentCommand.Arguments[0] = SelectedIndex;
                    break;
            }
        }
        else if (DropdownName == "SecondDropdown")
        {
            // Always room
            CurrentCommand.Arguments[1] = SelectedIndex + 1;
        }
    }

    public void OnTextFieldChanged(TMP_InputField EventField, string NewValue)
    {
        string FieldName = EventField.transform.parent.name;

        if (NewValue == "" || NewValue == "-")
        {
            return;
        }

        if (FieldName == "TextField")
        {
            Debug.Log("value changed");
            CurrentCommand.Arguments[0] = int.Parse(EventField.text);
        }
    }

    public void OnButtonClicked(Button EventButton)
    {
        string FieldName = EventButton.name;

        if (FieldName == "AddCommand")
        {
            VisibilityCommand NewCommand = new();

            NewCommand.Type = CurrentCommandType;

            switch (NewCommand.Type)
            {
                // One argument
                case 0x01:
                case 0x20:
                case 0x24:
                case 0x26:
                case 0x2A:
                case 0x1F:
                case 0x22:
                case 0x23:
                case 0x29:
                    NewCommand.Arguments = new() { 0 };
                    break;
                // Two arguments
                case 0x14:
                case 0x25:
                case 0x27:
                    NewCommand.Arguments = new() { 0, 0 };
                    break;
            }
            if (CommandList.SelectedItem != null)
            {
                DataHolder.VisibilityCommands.Insert(CommandList.SelectedItem.ID + 1, NewCommand);

                CommandList.AddItemAtIndex(CommandList.SelectedItem.ID + 1, NewCommand.DisplayString);
            }
            else
            {
                DataHolder.VisibilityCommands.Add(NewCommand);

                CommandList.AddItemAtEnd(NewCommand.DisplayString, true);
            }
        }
        else if (FieldName == "DeleteCommand")
        {
            if (CurrentCommand == null)
            {
                return;
            }

            DataHolder.VisibilityCommands.Remove(CurrentCommand);

            CommandList.RemoveSelectedItem();

            if (DataHolder.VisibilityCommands.Count == 0)
            {
                CurrentCommand = null;
            }
        }
        else if (FieldName == "MoveUp")
        {
            if (CurrentCommand == null)
            {
                return;
            }

            if (CommandList.SelectedItem.ID != 0)
            {
                DataHolder.VisibilityCommands.RemoveAt(CommandList.SelectedItem.ID);
                DataHolder.VisibilityCommands.Insert(CommandList.SelectedItem.ID - 1, CurrentCommand);
            }

            CommandList.MoveSelectedItemUp();
        }
        else if (FieldName == "MoveDown")
        {
            if (CommandList == null)
            {
                return;
            }

            if (CommandList.SelectedItem.ID != CommandList.ListItems.Count - 1)
            {
                DataHolder.VisibilityCommands.RemoveAt(CommandList.SelectedItem.ID);
                DataHolder.VisibilityCommands.Insert(CommandList.SelectedItem.ID + 1, CurrentCommand);
            }

            CommandList.MoveSelectedItemDown();
        }
    }
}
