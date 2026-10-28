using UnityEngine;
using UnityEngine.UI;

public class PadEditor : AttributeEditor
{
    Pad CurrentPad;
    Pad CurrentCoverPad;
    GuardPath CurrentPath;

    ScrollableList PathPadList;

    //public GameObject PadType;
    public GameObject CoverPadType;

    public DataHolder DataHolderInstance;

    // Start is called before the first frame update
    void Awake()
    {
        PathPadList = GetComponentInChildren<ScrollableList>();

        transform.Find("Paths/SelectPath").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.Paths;

        transform.Find("Pads/SelectPad").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.Pads;

        transform.Find("Pads/SelectCoverPad").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.CoverPads;
    }

    public override void OnDropdownChanged(string DropdownName, int SelectedIndex, string SelectedText)
    {
        if (DropdownName == "SelectPad")
        {
            CurrentPad = DropdownOptions.FindPad(SelectedText);
        }
        else if (DropdownName == "SelectCoverPad")
        {
            CurrentCoverPad = DataHolder.CoverPads[SelectedIndex];
        }
        else if (DropdownName == "SelectPath")
        {
            if (DataHolder.Paths.Count > 0)
            {
                CurrentPath = DataHolder.Paths[SelectedIndex];
            }

            PathPadList.Clear();
            foreach(Pad IteratedPad in CurrentPath.Pads)
            {
                PathPadList.AddItemAtEnd(IteratedPad.NumberString, false);
            }

            transform.Find("Paths/PathFlags/CircularPath").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentPath.IsCircularPath);
            transform.Find("Paths/PathFlags/FlyingPath").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentPath.IsFlyingPath);
        }
    }

    public void OnToggleClicked(Toggle EventToggle, bool NewValue)
    {
        string FieldName = EventToggle.transform.parent.name;

        if (FieldName == "CircularPath")
        {
            CurrentPath.IsCircularPath = NewValue;
        }
        else if (FieldName == "FlyingPath")
        {
            CurrentPath.IsFlyingPath = NewValue;
        }
    }

    public void OnButtonClicked(Button EventButton)
    {
        string FieldName = EventButton.name;

        if (FieldName == "AddPath")
        {
            GuardPath NewPath = new();
            DataHolder.Paths.Add(NewPath);

            DropdownOptions.Paths.Add("Path " + (DataHolder.Paths.Count + 1));

            if (DropdownOptions.Paths.Count == 1)
            {
                transform.Find("Paths/SelectPath").GetComponentInChildren<BetterDropdown>().CurrentIndex = 0;
            }
            else
            {
                transform.Find("Paths/SelectPath").GetComponentInChildren<BetterDropdown>().CurrentIndex = DropdownOptions.Paths.Count - 1;
            }
        }
        else if (FieldName == "DeletePath")
        {
            BetterDropdown SelectPathDropdown = transform.Find("Paths/SelectPath").GetComponentInChildren<BetterDropdown>();

            if (DataHolder.Paths.Count == 0)
            {
                return;
            }

            DataHolder.Paths.Remove(CurrentPath);
            DropdownOptions.RefreshPaths();

            if (SelectPathDropdown.OptionList.Count == 0)
            {
                SelectPathDropdown.RefreshCurrentText();
                Debug.Log("Refreshcurrentext");
                Debug.Log(SelectPathDropdown.OptionList.Count);
            }
            else if (SelectPathDropdown.CurrentIndex == 0)
            {
                Debug.Log("Currentindex = 0");
                Debug.Log(SelectPathDropdown.OptionList.Count);
                SelectPathDropdown.CurrentIndex = 0;
            }
            else
            {
                SelectPathDropdown.CurrentIndex--;
            }

            if (DataHolder.Paths.Count == 0)
            {
                CurrentPath = null;

                PathPadList.Clear();
            }
        }
        else if (FieldName == "AddPad")
        {
            DataHolderInstance.AddPad(Vector3.zero);

            BetterDropdown SelectPadDropdown = transform.Find("Pads/SelectPad").GetComponentInChildren<BetterDropdown>();
            if (DropdownOptions.Pads.Count == 1)
            {
                SelectPadDropdown.CurrentIndex = 0;
            }
            else
            {
                SelectPadDropdown.CurrentIndex = SelectPadDropdown.OptionList.Count - 1;
            }
        }
        else if (FieldName == "DeletePad")
        {
            if (CurrentPad != null)
            {
                DataHolderInstance.DeletePad(CurrentPad);

                BetterDropdown SelectPadDropdown = transform.Find("Pads/SelectPad").GetComponentInChildren<BetterDropdown>();

                if (SelectPadDropdown.OptionList.Count == 0)
                {
                    SelectPadDropdown.RefreshCurrentText();
                    CurrentPad = null;
                }
                else if (SelectPadDropdown.CurrentIndex == 0)
                {
                    SelectPadDropdown.CurrentIndex = 0;
                }
                else
                {
                    SelectPadDropdown.CurrentIndex--;
                }
            }
        }
        else if (FieldName == "EditPadInInspector")
        {
            if (CurrentPad != null)
            {
                DataHolder.SelectionManagerInstance.SelectNewObject(CurrentPad.gameObject);
            }
        }
        else if (FieldName == "GoToPad")
        {
            if (CurrentPad != null)
            {
                Vector3 CameraPosition = CurrentPad.transform.position;
                CameraPosition += CurrentPad.transform.forward * 100;
                CameraPosition.y += 200;

                DataHolder.MainCameraInstance.transform.position = CameraPosition;
                DataHolder.MainCameraInstance.transform.LookAt(CurrentPad.transform.position);
            }
        }
        else if (FieldName == "AddCoverPad")
        {
            DataHolderInstance.AddCoverPad(Vector3.zero);

            BetterDropdown SelectCoverPadDropdown = transform.Find("Pads/SelectCoverPad").GetComponentInChildren<BetterDropdown>();
            if (DropdownOptions.CoverPads.Count == 1)
            {
                SelectCoverPadDropdown.CurrentIndex = 0;
            }
            else
            {
                SelectCoverPadDropdown.CurrentIndex = SelectCoverPadDropdown.OptionList.Count - 1;
            }
        }
        else if (FieldName == "DeleteCoverPad")
        {
            if (CurrentCoverPad != null)
            {
                DataHolderInstance.DeleteCoverPad(CurrentCoverPad);

                BetterDropdown SelectCoverPadDropdown = transform.Find("Pads/SelectCoverPad").GetComponentInChildren<BetterDropdown>();

                if (SelectCoverPadDropdown.OptionList.Count == 0)
                {
                    SelectCoverPadDropdown.RefreshCurrentText();
                    CurrentCoverPad = null;
                }
                else if (SelectCoverPadDropdown.CurrentIndex == 0)
                {
                    SelectCoverPadDropdown.CurrentIndex = 0;
                }
                else
                {
                    SelectCoverPadDropdown.CurrentIndex--;
                }
            }
        }
        else if (FieldName == "EditCoverPadInInspector")
        {
            if (CurrentCoverPad != null)
            {
                DataHolder.SelectionManagerInstance.SelectNewObject(CurrentCoverPad.gameObject);
            }
        }
        else if (FieldName == "GoToCoverPad")
        {
            if (CurrentCoverPad != null)
            {
                Vector3 CameraPosition = CurrentCoverPad.transform.position;
                CameraPosition += CurrentCoverPad.transform.forward * 100;
                CameraPosition.y += 200;

                DataHolder.MainCameraInstance.transform.position = CameraPosition;
                DataHolder.MainCameraInstance.transform.LookAt(CurrentCoverPad.transform.position);
            }
        }
        else if (FieldName == "AddPathPad")
        {
            if (CurrentPath == null || CurrentPad == null)
            {
                return;
            }

            if (PathPadList.ListItems.Count == 0 || PathPadList.SelectedItem == null)
            {
                PathPadList.AddItemAtEnd(CurrentPad.NumberString, true);
                CurrentPath.AddPad(CurrentPad);
            }
            else
            {
                PathPadList.AddItemAtIndex(PathPadList.SelectedItem.ID + 1, CurrentPad.NumberString);
                CurrentPath.AddPadAtIndex(PathPadList.SelectedItem.ID, CurrentPad);
            }
        }
        else if (FieldName == "DeletePathPad")
        {
            if (PathPadList.SelectedItem == null || CurrentPath == null)
            {
                return;
            }
            
            Pad PathPad = DataHolder.Pads.Find(x => x.NumberString == PathPadList.SelectedItem.name);
            CurrentPath.RemovePad(PathPad);
            PathPadList.RemoveSelectedItem();
        }
        else if (FieldName == "MoveUp")
        {
            if (PathPadList.SelectedItem == null || CurrentPath == null)
            {
                return;
            }

            if (PathPadList.SelectedItem.ID != 0)
            {
                Pad MovedPad = CurrentPath.Pads[PathPadList.SelectedItem.ID];
                CurrentPath.Pads.RemoveAt(PathPadList.SelectedItem.ID);
                CurrentPath.Pads.Insert(PathPadList.SelectedItem.ID - 1, MovedPad);
            }

            PathPadList.MoveSelectedItemUp();
        }
        else if (FieldName == "MoveDown")
        {
            if (PathPadList.SelectedItem == null || CurrentPath == null)
            {
                return;
            }

            if (PathPadList.SelectedItem.ID != PathPadList.ListItems.Count - 1)
            {
                Pad MovedPad = CurrentPath.Pads[PathPadList.SelectedItem.ID];
                CurrentPath.Pads.RemoveAt(PathPadList.SelectedItem.ID);
                CurrentPath.Pads.Insert(PathPadList.SelectedItem.ID + 1, MovedPad);
            }

            PathPadList.MoveSelectedItemDown();
        }
    }
}
