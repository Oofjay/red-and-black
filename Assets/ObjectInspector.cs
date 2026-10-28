using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ObjectInspector : AttributeEditor
{
    bool MultipleObjectsSelected;

    SelectionManager SelectionManagerInstance;

    public Pad CurrentPad { get { return CurrentPads[0]; } }
    public Pad[] CurrentPads;
    public Waypoint CurrentWaypoint;
    public WaypointNeighbour CurrentNeighbour;
    public Prop CurrentProp;

    public Room CurrentRoom { get { return CurrentRooms[0]; } }
    public Room[] CurrentRooms;

    public Portal CurrentPortal { get { return CurrentPortals[0]; } }
    public Portal[] CurrentPortals;

    public Collision CurrentCollision;
    public Tile CurrentTile;

    public BackgroundLight CurrentBackgroundLight { get { return CurrentBackgroundLights[0]; } }
    public BackgroundLight[] CurrentBackgroundLights;

    private Transform PadEditor;
    private Transform PropEditor;
    private Transform RoomEditor;
    private Transform PortalEditor;
    private Transform CollisionEditor;
    private Transform BackgroundLightEditor;

    private TextMeshProUGUI PadLabel;
    private TextMeshProUGUI PropLabel;
    private TextMeshProUGUI RoomLabel;
    private TextMeshProUGUI PortalLabel;
    private TextMeshProUGUI CollisionLabel;
    private TextMeshProUGUI BackgroundLightLabel;

    public GameObject SubPropContent;
    public GameObject LiftDoorStopContent;

    private List<GameObject> SubProps = new();
    private List<GameObject> LiftStops = new();

    public FlagsEditor FlagsEditorInstance;
    public ColourSelector ColourSelectorInstance;
    public PropAttachmentEditor AttachmentEditorInstance;
    public PropModelPicker PropModelPickerInstance;

    bool UINeedsUpdating;

    private void Start()
    {
        SelectionManagerInstance = DataHolder.SelectionManagerInstance;
        SelectionManagerInstance.SelectionChanged += OnSelectionChanged;

        PadEditor = transform.GetChild(0);
        PropEditor = transform.GetChild(1);
        RoomEditor = transform.GetChild(2);
        PortalEditor = transform.GetChild(3);
        CollisionEditor = transform.GetChild(4);
        BackgroundLightEditor = transform.GetChild(5);

        PadLabel = PadEditor.GetChild(0).GetComponent<TextMeshProUGUI>();
        PropLabel = PropEditor.GetChild(0).GetComponent<TextMeshProUGUI>();
        RoomLabel = RoomEditor.GetChild(0).GetComponent<TextMeshProUGUI>();
        PortalLabel = PortalEditor.GetChild(0).GetComponent<TextMeshProUGUI>();
        CollisionLabel = CollisionEditor.GetChild(0).GetComponent<TextMeshProUGUI>();
        BackgroundLightLabel = BackgroundLightEditor.GetChild(0).GetComponent<TextMeshProUGUI>();

        PadEditor.Find("NeighbourList").GetComponent<ScrollableList>().ListItemSelected += OnWaypointNeighbourSelected;

        PropEditor.gameObject.SetActive(false);

        PropEditor.Find("PadPreset").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.PadsWithNone;
        PropEditor.Find("CharacterPreset").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.PadsWithNone;
        PropEditor.Find("LinkedDoor").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.DoorsWithNone;
        PropEditor.Find("TargetPad").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.PadsWithNone;
        PropEditor.Find("Chair").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.TagsWithNone;
        PropEditor.Find("LiftPad1").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.PadsWithNone;
        PropEditor.Find("LiftPad2").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.PadsWithNone;
        PropEditor.Find("LiftPad3").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.PadsWithNone;
        PropEditor.Find("LiftPad4").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.PadsWithNone;
        PropEditor.Find("LiftDoor1").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.DoorsWithNone;
        PropEditor.Find("LiftDoor2").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.DoorsWithNone;
        PropEditor.Find("LiftDoor3").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.DoorsWithNone;
        PropEditor.Find("LiftDoor4").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.DoorsWithNone;
        PropEditor.Find("LinkedGun").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.WeaponsWithNone;
        PropEditor.Find("Script").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.AssignableAILists;

        PropEditor.Find("DoorType").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.GetConstantOptions("DOORTYPE");
        PropEditor.Find("WeaponType").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.GetConstantOptions("WEAPON");
        PropEditor.Find("NameChangeFoldout/Content/WeaponAppearance").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.GetConstantOptions("WEAPON");
        PropEditor.Find("NameChangeFoldout/Content/MenuListName").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.LanguageStrings;
        PropEditor.Find("NameChangeFoldout/Content/PickupText").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.LanguageStrings;
        PropEditor.Find("AmmoType").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.GetConstantOptions("AMMOTYPE");
        PropEditor.Find("Team").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.GetConstantOptions("TEAM");
        PropEditor.Find("Squadron").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.GetConstantOptions("SQUADRON");
        PropEditor.Find("Effect").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.GetConstantOptions("PADEFFECT");
        PropEditor.Find("HoverType").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.GetConstantOptions("HOVTYPE");

        List<string> PropTypeOptions = new()
        {
            "Door",
            "Standard object",
            "Key",
            "Camera",
            "Ammo crate",
            "Weapon",
            "Single-screen monitor",
            "Multi-screen monitor",
            "Autogun",
            "Debris",
            "Hat",
            "Ammo crate (multiplayer)",
            "Shield",
            "Glass",
            "Camera position",
            "Tinted glass",
            "Lift",
            "Conditional scenery",
            "Blocked path",
            "Hoverbike",
            "Hover prop",
            "Fan",
            "Hovercar",
            "Effect",
            "Chopper",
            "Mine",
            "Escalator step",
        };

        PropEditor.Find("PropType").GetComponentInChildren<BetterDropdown>().OptionList = PropTypeOptions;

        PortalEditor.Find("FirstRoom").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.Rooms;
        PortalEditor.Find("SecondRoom").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.Rooms;
        BackgroundLightEditor.Find("Room").GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.Rooms;
    }

    private void LateUpdate()
    {
        if (UINeedsUpdating)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(PadEditor.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(PropEditor.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(RoomEditor.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(CollisionEditor.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(BackgroundLightEditor.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(PortalEditor.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

            UINeedsUpdating = false;
        }
    }

    public void MarkAsNeedUpdating()
    {
        UINeedsUpdating = true;
    }

    public void EnterObject(Pad InPad)
    {
        PadEditor.gameObject.SetActive(true);
        EnterPad(InPad);

        if (InPad.PadProp != null)
        {
            PropEditor.gameObject.SetActive(true);
            EnterProp(InPad.PadProp);
        }
        else
        {
            PropEditor.gameObject.SetActive(false);
        }

        MarkAsNeedUpdating();
    }

    public void EnterObject(Prop InProp)
    {
        PropEditor.gameObject.SetActive(true);
        EnterProp(InProp);

        if (InProp.ParentPad != null)
        {
            PadEditor.gameObject.SetActive(true);
            EnterPad(InProp.ParentPad);
        }
        else
        {
            PadEditor.gameObject.SetActive(false);
        }

        MarkAsNeedUpdating();
    }

    private void EnterPad(Pad InPad)
    {
        CurrentPads = new Pad[1] { InPad };

        Transform PositionField = PadEditor.Find("Position");
        PositionField.gameObject.SetActive(true);
        PositionField.Find("XPosition").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(InPad.Position.x.ToString());
        PositionField.Find("YPosition").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(InPad.Position.y.ToString());
        PositionField.Find("ZPosition").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(InPad.Position.z.ToString());

        Transform DirectionField = PadEditor.Find("Direction");
        DirectionField.gameObject.SetActive(true);
        DirectionField.Find("XDirection").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(InPad.Direction.x.ToString());
        DirectionField.Find("YDirection").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(InPad.Direction.y.ToString());
        DirectionField.Find("ZDirection").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(InPad.Direction.z.ToString());

        PadEditor.Find("WaypointLinkText").gameObject.SetActive(false);

        if (InPad.IsCoverPad)
        {
            PadLabel.text = "Cover pad " + InPad.NumberString;

            PadEditor.Find("Up").gameObject.SetActive(false);
            PadEditor.Find("XSize").gameObject.SetActive(false);
            PadEditor.Find("YSize").gameObject.SetActive(false);
            PadEditor.Find("ZSize").gameObject.SetActive(false);
            PadEditor.Find("AIWaitsLift").gameObject.SetActive(false);
            PadEditor.Find("AIRidesLift").gameObject.SetActive(false);
            PadEditor.Find("AIWalksDirectly").gameObject.SetActive(false);
            PadEditor.Find("AIDropsDown").gameObject.SetActive(false);
            PadEditor.Find("AIDucks").gameObject.SetActive(false);
            PadEditor.Find("AIMovesWhileFalling").gameObject.SetActive(false);
            PadEditor.Find("AICrouches").gameObject.SetActive(false);
            PadEditor.Find("LiftNumber").gameObject.SetActive(false);
            PadEditor.Find("HasBoundingBoxData").gameObject.SetActive(false);
            PadEditor.Find("AddOrDeleteWaypoint").gameObject.SetActive(false);
            PadEditor.Find("WaypointLabel").gameObject.SetActive(false);
            PadEditor.Find("NeighbourList").gameObject.SetActive(false);
            PadEditor.Find("MoveOutwardsOnly").gameObject.SetActive(false);
            PadEditor.Find("MoveInwardsOnly").gameObject.SetActive(false);
            PadEditor.Find("WaypointButtons").gameObject.SetActive(false);
            PadEditor.Find("SelectWaygroup").gameObject.SetActive(false);

            // unk18

            PropEditor.gameObject.SetActive(false);
        }
        else
        {
            PadEditor.Find("AddOrDeleteWaypoint").gameObject.SetActive(true);

            if (InPad.PadWaypoint != null)
            {
                CurrentWaypoint = CurrentPad.PadWaypoint;

                PadLabel.text = "Waypoint pad " + InPad.NumberString;
                PadEditor.Find("AddOrDeleteWaypoint").GetComponentInChildren<TextMeshProUGUI>().text = "Remove waypoint from pad";
                PadEditor.Find("WaypointLabel").gameObject.SetActive(true);

                ScrollableList NeighbourList = PadEditor.Find("NeighbourList").GetComponent<ScrollableList>();
                NeighbourList.gameObject.SetActive(true);
                NeighbourList.Clear();

                foreach (WaypointNeighbour CurrentNeighbour in CurrentWaypoint.Neighbours)
                {
                    NeighbourList.AddItemAtEnd(CurrentNeighbour.NumberString, false);
                }

                // These are only shown when you select a waypoint neighbour from the list
                PadEditor.Find("MoveOutwardsOnly").gameObject.SetActive(false);
                PadEditor.Find("MoveInwardsOnly").gameObject.SetActive(false);

                PadEditor.Find("WaypointButtons").gameObject.SetActive(true);

                GameObject SelectWaygroupDropdown = PadEditor.Find("SelectWaygroup").gameObject;
                SelectWaygroupDropdown.SetActive(true);
                SelectWaygroupDropdown.GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.Waygroups;
                SelectWaygroupDropdown.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DataHolder.Waygroups.IndexOf(CurrentWaypoint.ParentWaygroup));
            }
            else
            {
                CurrentWaypoint = null;

                PadLabel.text = "Pad " + InPad.NumberString;
                PadEditor.Find("AddOrDeleteWaypoint").GetComponentInChildren<TextMeshProUGUI>().text = "Add waypoint to pad";
                PadEditor.Find("WaypointLabel").gameObject.SetActive(false);
                PadEditor.Find("NeighbourList").gameObject.SetActive(false);
                PadEditor.Find("MoveOutwardsOnly").gameObject.SetActive(false);
                PadEditor.Find("MoveInwardsOnly").gameObject.SetActive(false);
                PadEditor.Find("WaypointButtons").gameObject.SetActive(false);
                PadEditor.Find("SelectWaygroup").gameObject.SetActive(false);
            }

            Transform UpField = PadEditor.Find("Up");
            UpField.gameObject.SetActive(true);
            UpField.Find("XUp").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(InPad.Up.x.ToString());
            UpField.Find("YUp").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(InPad.Up.y.ToString());
            UpField.Find("ZUp").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(InPad.Up.z.ToString());

            Transform BoundingBoxToggle = PadEditor.Find("HasBoundingBoxData");
            BoundingBoxToggle.gameObject.SetActive(true);
            BoundingBoxToggle.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentPad.HasBoundingBoxData);

            if (CurrentPad.HasBoundingBoxData)
            {
                Transform XSizeField = PadEditor.Find("XSize");
                XSizeField.gameObject.SetActive(true);
                XSizeField.Find("XMin").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(InPad.XMin.ToString());
                XSizeField.Find("XMax").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(InPad.XMax.ToString());

                Transform YSizeField = PadEditor.Find("YSize");
                YSizeField.gameObject.SetActive(true);
                YSizeField.Find("YMin").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(InPad.YMin.ToString());
                YSizeField.Find("YMax").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(InPad.YMax.ToString());

                Transform ZSizeField = PadEditor.Find("ZSize");
                ZSizeField.gameObject.SetActive(true);
                ZSizeField.Find("ZMin").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(InPad.ZMin.ToString());
                ZSizeField.Find("ZMax").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(InPad.ZMax.ToString());
            }
            else
            {
                PadEditor.Find("XSize").gameObject.SetActive(false);
                PadEditor.Find("YSize").gameObject.SetActive(false);
                PadEditor.Find("ZSize").gameObject.SetActive(false);
            }

            GameObject AIWaitsLift = PadEditor.Find("AIWaitsLift").gameObject;
            AIWaitsLift.SetActive(true);
            AIWaitsLift.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentPad.AIWaitsForLift);

            GameObject AIRidesLift = PadEditor.Find("AIRidesLift").gameObject;
            AIRidesLift.SetActive(true);
            AIRidesLift.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentPad.AIRidesLift);

            GameObject AIWalksDirectly = PadEditor.Find("AIWalksDirectly").gameObject;
            AIWalksDirectly.SetActive(true);
            AIWalksDirectly.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentPad.AIWalksDirectlyTo);

            GameObject AIDropsDown = PadEditor.Find("AIDropsDown").gameObject;
            AIDropsDown.SetActive(true);
            AIDropsDown.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentPad.AIDrops);

            GameObject AICrouches = PadEditor.Find("AICrouches").gameObject;
            AICrouches.SetActive(true);
            AICrouches.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentPad.AICrouches);

            GameObject AIMovesWhileFalling = PadEditor.Find("AIMovesWhileFalling").gameObject;
            AIMovesWhileFalling.SetActive(true);
            AIMovesWhileFalling.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentPad.AIMovesWhileFalling);

            GameObject AIDucks = PadEditor.Find("AIDucks").gameObject;
            AIDucks.SetActive(true);
            AIDucks.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentPad.AIDucks);

            GameObject LiftNumberField = PadEditor.Find("LiftNumber").gameObject;
            LiftNumberField.SetActive(true);
            LiftNumberField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentPad.LiftNumber.ToString());
        }
    }

    public void EnterPads(Pad[] InPads)
    {
        PadEditor.gameObject.SetActive(true);
        PropEditor.gameObject.SetActive(false);

        PadLabel.text = InPads.Length + " pads selected";

        CurrentPads = InPads;

        Transform PositionField = PadEditor.Find("Position");
        PositionField.gameObject.SetActive(true);
        PositionField.Find("XPosition").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(SelectionManagerInstance.Gizmo.transform.position.x.ToString());
        PositionField.Find("YPosition").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(SelectionManagerInstance.Gizmo.transform.position.y.ToString());
        PositionField.Find("ZPosition").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(SelectionManagerInstance.Gizmo.transform.position.z.ToString());

        Transform DirectionField = PadEditor.Find("Direction");
        DirectionField.gameObject.SetActive(true);
        DirectionField.Find("XDirection").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");
        DirectionField.Find("YDirection").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");
        DirectionField.Find("ZDirection").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");

        Transform UpField = PadEditor.Find("Up");
        UpField.gameObject.SetActive(true);
        UpField.Find("XUp").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");
        UpField.Find("YUp").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");
        UpField.Find("ZUp").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");

        PadEditor.Find("WaypointLinkText").gameObject.SetActive(false);

        GameObject AIWaitsLift = PadEditor.Find("AIWaitsLift").gameObject;
        AIWaitsLift.SetActive(true);
        AIWaitsLift.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(true);

        GameObject AIRidesLift = PadEditor.Find("AIRidesLift").gameObject;
        AIRidesLift.SetActive(true);
        AIRidesLift.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(true);

        GameObject AIWalksDirectly = PadEditor.Find("AIWalksDirectly").gameObject;
        AIWalksDirectly.SetActive(true);
        AIWalksDirectly.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(true);

        GameObject AIDropsDown = PadEditor.Find("AIDropsDown").gameObject;
        AIDropsDown.SetActive(true);
        AIDropsDown.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(true);

        GameObject AICrouches = PadEditor.Find("AICrouches").gameObject;
        AICrouches.SetActive(true);
        AICrouches.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(true);

        GameObject AIMovesWhileFalling = PadEditor.Find("AIMovesWhileFalling").gameObject;
        AIMovesWhileFalling.SetActive(true);
        AIMovesWhileFalling.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(true);

        GameObject AIDucks = PadEditor.Find("AIDucks").gameObject;
        AIDucks.SetActive(true);
        AIDucks.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(true);

        GameObject LiftNumberField = PadEditor.Find("LiftNumber").gameObject;
        LiftNumberField.SetActive(true);
        LiftNumberField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");

        PadEditor.Find("WaypointLabel").gameObject.SetActive(false);
        PadEditor.Find("NeighbourList").gameObject.SetActive(false);
        PadEditor.Find("MoveOutwardsOnly").gameObject.SetActive(false);
        PadEditor.Find("MoveInwardsOnly").gameObject.SetActive(false);
        PadEditor.Find("WaypointButtons").gameObject.SetActive(false);

        bool OnlyCoverPads = true;
        bool HasWaypoints = false;
        bool OnlyBoundingBoxes = true;

        foreach (Pad IteratedPad in CurrentPads)
        {
            if (!IteratedPad.IsCoverPad)
            {
                OnlyCoverPads = false;
            }
            else
            {
                continue;
            }

            if (!HasWaypoints && IteratedPad.PadWaypoint != null)
            {
                HasWaypoints = true;
            }

            if (!IteratedPad.HasBoundingBoxData)
            {
                OnlyBoundingBoxes = false;
            }

            if (IteratedPad.AIWaitsForLift == false) { AIWaitsLift.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(false); }
            if (IteratedPad.AIRidesLift == false) { AIRidesLift.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(false); }
            if (IteratedPad.AIWalksDirectlyTo == false) { AIWalksDirectly.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(false); }
            if (IteratedPad.AIDrops == false) { AIDropsDown.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(false); }
            if (IteratedPad.AICrouches == false) { AICrouches.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(false); }
            if (IteratedPad.AIMovesWhileFalling == false) { AIMovesWhileFalling.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(false); }
            if (IteratedPad.AIDucks == false) { AIDucks.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(false); }
        }

        if (OnlyCoverPads)
        {
            PadEditor.Find("Up").gameObject.SetActive(false);
            PadEditor.Find("XSize").gameObject.SetActive(false);
            PadEditor.Find("YSize").gameObject.SetActive(false);
            PadEditor.Find("ZSize").gameObject.SetActive(false);
            PadEditor.Find("AIWaitsLift").gameObject.SetActive(false);
            PadEditor.Find("AIRidesLift").gameObject.SetActive(false);
            PadEditor.Find("AIWalksDirectly").gameObject.SetActive(false);
            PadEditor.Find("AIDropsDown").gameObject.SetActive(false);
            PadEditor.Find("AIDucks").gameObject.SetActive(false);
            PadEditor.Find("AIMovesWhileFalling").gameObject.SetActive(false);
            PadEditor.Find("AICrouches").gameObject.SetActive(false);
            PadEditor.Find("LiftNumber").gameObject.SetActive(false);
            PadEditor.Find("HasBoundingBoxData").gameObject.SetActive(false);
            PadEditor.Find("AddOrDeleteWaypoint").gameObject.SetActive(false);
            PadEditor.Find("WaypointLabel").gameObject.SetActive(false);
            PadEditor.Find("NeighbourList").gameObject.SetActive(false);
            PadEditor.Find("MoveOutwardsOnly").gameObject.SetActive(false);
            PadEditor.Find("MoveInwardsOnly").gameObject.SetActive(false);
            PadEditor.Find("WaypointButtons").gameObject.SetActive(false);
            PadEditor.Find("SelectWaygroup").gameObject.SetActive(false);

            // unk18
        }
        else
        {
            PadEditor.Find("AddOrDeleteWaypoint").gameObject.SetActive(true);

            if (HasWaypoints)
            {
                PadEditor.Find("AddOrDeleteWaypoint").GetComponentInChildren<TextMeshProUGUI>().text = "Remove waypoints from all pads";

                GameObject SelectWaygroupDropdown = PadEditor.Find("SelectWaygroup").gameObject;
                SelectWaygroupDropdown.SetActive(true);
                SelectWaygroupDropdown.GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.Waygroups;
                SelectWaygroupDropdown.GetComponentInChildren<BetterDropdown>().SetNoOption();
            }
            else
            {
                PadEditor.Find("AddOrDeleteWaypoint").GetComponentInChildren<TextMeshProUGUI>().text = "Add waypoint to all pads";
                PadEditor.Find("SelectWaygroup").gameObject.SetActive(false);
            }

            Transform BoundingBoxToggle = PadEditor.Find("HasBoundingBoxData");
            BoundingBoxToggle.gameObject.SetActive(true);

            if (OnlyBoundingBoxes)
            {
                BoundingBoxToggle.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(true);

                Transform XSizeField = PadEditor.Find("XSize");
                XSizeField.gameObject.SetActive(true);
                XSizeField.Find("XMin").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");
                XSizeField.Find("XMax").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");

                Transform YSizeField = PadEditor.Find("YSize");
                YSizeField.gameObject.SetActive(true);
                YSizeField.Find("YMin").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");
                YSizeField.Find("YMax").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");

                Transform ZSizeField = PadEditor.Find("ZSize");
                ZSizeField.gameObject.SetActive(true);
                ZSizeField.Find("ZMin").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");
                ZSizeField.Find("ZMax").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");
            }
            else
            {
                BoundingBoxToggle.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(false);

                PadEditor.Find("XSize").gameObject.SetActive(false);
                PadEditor.Find("YSize").gameObject.SetActive(false);
                PadEditor.Find("ZSize").gameObject.SetActive(false);
            }
        }

        MarkAsNeedUpdating();
    }

    private void EnterProp(Prop InProp)
    {
        CurrentProp = InProp;

        PropEditor.gameObject.SetActive(true);

        foreach (Transform Child in PropEditor)
        {
            Child.gameObject.SetActive(false);
        }

        PropLabel.gameObject.SetActive(true);

        if (CurrentProp.IsAttachedToCharacter || CurrentProp.IsAttachedToOtherProp)
        {
            PropEditor.Find("PropAttachmentLabel").gameObject.SetActive(true);
            PropEditor.Find("PropAttachmentLabel").GetComponent<TextMeshProUGUI>().text = "Embedded prop";

            PropEditor.Find("EditParentProp").gameObject.SetActive(true);
        }

        if (CurrentProp.Type != PropType.Character)
        {
            GameObject PropTypeDropdown = PropEditor.Find("PropType").gameObject;
            PropTypeDropdown.SetActive(true);
            PropTypeDropdown.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent((int)CurrentProp.Type);
        }

        PropEditor.Find("ChangeAttachment").gameObject.SetActive(true);

        if (CurrentProp.Type == PropType.Character)
        {
            PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Character)";

            GameObject CharacterIDField = PropEditor.Find("CharacterID").gameObject;
            CharacterIDField.SetActive(true);
            CharacterIDField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentProp.CharacterID.ToString());

            PropEditor.Find("SelectBody").gameObject.SetActive(true);
            PropEditor.Find("SelectHead").gameObject.SetActive(true);

            GameObject ScriptField = PropEditor.Find("Script").gameObject;
            ScriptField.SetActive(true);
            ScriptField.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.FindAssignableAIListIndex(CurrentProp.AIList));

            GameObject PadPresetField = PropEditor.Find("PadPreset").gameObject;
            PadPresetField.SetActive(true);
            int SelectedOption = (CurrentProp.PadPreset == null) ? 0 : DropdownOptions.FindPadIndex(CurrentProp.PadPreset, true);
            PadPresetField.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(SelectedOption);

            GameObject CharacterPresetField = PropEditor.Find("CharacterPreset").gameObject;
            CharacterPresetField.SetActive(true);
            SelectedOption = (CurrentProp.CharacterPreset == null) ? 0 : DropdownOptions.FindPadIndex(CurrentProp.CharacterPreset, true);
            CharacterPresetField.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(SelectedOption);

            GameObject HearScaleField = PropEditor.Find("HearScale").gameObject;
            HearScaleField.SetActive(true);
            HearScaleField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentProp.HearScale.ToString());

            GameObject ViewDistanceField = PropEditor.Find("ViewDistance").gameObject;
            ViewDistanceField.SetActive(true);
            ViewDistanceField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentProp.ViewDistance.ToString());

            PropEditor.Find("ChangeFlags").gameObject.SetActive(true);

            GameObject TeamField = PropEditor.Find("Team").gameObject;
            TeamField.SetActive(true);
            TeamField.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.GetConstantIndex(CurrentProp.Team));

            GameObject SquadronField = PropEditor.Find("Squadron").gameObject;
            SquadronField.SetActive(true);
            SquadronField.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.GetConstantIndex(CurrentProp.Squadron));

            GameObject ChairField = PropEditor.Find("Chair").gameObject;
            ChairField.SetActive(true);
            ChairField.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.FindTagIndex(CurrentProp.Chair, true));

            GameObject ConvTalkField = PropEditor.Find("ConvTalk").gameObject;
            ConvTalkField.SetActive(true);
            ConvTalkField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentProp.ConvTalk.ToString());

            GameObject TudeField = PropEditor.Find("Tude").gameObject;
            TudeField.SetActive(true);
            TudeField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentProp.Tude.ToString());

            GameObject NaturalAnimationField = PropEditor.Find("NaturalAnimation").gameObject;
            NaturalAnimationField.SetActive(true);
            NaturalAnimationField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentProp.NaturalAnimation.ToString());

            GameObject YVisionAngleField = PropEditor.Find("YVisionAngle").gameObject;
            YVisionAngleField.SetActive(true);
            YVisionAngleField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentProp.YVisAngle.ToString());

            GameObject TeamScanDistanceField = PropEditor.Find("TeamScanDistance").gameObject;
            TeamScanDistanceField.SetActive(true);
            TeamScanDistanceField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentProp.TeamScanDistance.ToString());
        }
        else if (CurrentProp.Type == PropType.Effect)
        {
            PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Visual FX)";

            GameObject EffectField = PropEditor.Find("Effect").gameObject;
            EffectField.SetActive(true);
            EffectField.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.GetConstantIndex(CurrentProp.Effect));
        }
        else if (CurrentProp.Type == PropType.Camera2)
        {
            PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Camera 2)";

            GameObject ScaleField = PropEditor.Find("Scale").gameObject;
            ScaleField.SetActive(true);
            float Scale = CurrentProp.Scale / 2.56f;
            ScaleField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(Scale.ToString());

            GameObject XOffsetField = PropEditor.Find("XOffset").gameObject;
            XOffsetField.SetActive(true);
            float XOffset = CurrentProp.XOffset / 100.0f;
            XOffsetField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(XOffset.ToString());

            GameObject YOffsetField = PropEditor.Find("YOffset").gameObject;
            YOffsetField.SetActive(true);
            float YOffset = CurrentProp.YOffset / 100.0f;
            YOffsetField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(YOffset.ToString());

            GameObject ZOffsetField = PropEditor.Find("ZOffset").gameObject;
            ZOffsetField.SetActive(true);
            float ZOffset = CurrentProp.ZOffset / 100.0f;
            ZOffsetField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(ZOffset.ToString());

            GameObject LateralRotationField = PropEditor.Find("LateralRotation").gameObject;
            LateralRotationField.SetActive(true);
            float LateralRotation = CurrentProp.LateralRotation / 65536.0f;
            LateralRotationField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(LateralRotation.ToString());

            GameObject VerticalRotationField = PropEditor.Find("VerticalRotation").gameObject;
            VerticalRotationField.SetActive(true);
            float VerticalRotation = CurrentProp.VerticalRotation / 65536.0f;
            VerticalRotationField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(VerticalRotation.ToString());
        }
        else
        {
            // the following all begin with Scale, Model, Pad, Flags1 (array), Flags2 (array), Flags3 (array), MaxDamage
            // these are: stdobject, debris, ammocratemulti, shield, gasbottle, glass, safe, hoverbike, escastep, key (key has no MaxDamage)
            GameObject ScaleField = PropEditor.Find("Scale").gameObject;
            ScaleField.SetActive(true);
            float PropScale = CurrentProp.Scale / 2.56f;
            ScaleField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(PropScale.ToString());

            PropEditor.Find("SelectModel").gameObject.SetActive(true);
            PropEditor.Find("ChangeFlags").gameObject.SetActive(true);
            if (CurrentProp.Type != PropType.Key && CurrentProp.Type != PropType.Weapon) // Keys and weapons don't take MaxDamage as an argument
            {
                GameObject MaxDamageField = PropEditor.Find("MaxDamage").gameObject;
                MaxDamageField.SetActive(true);
                MaxDamageField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentProp.MaxDamage.ToString());
            }

            switch (CurrentProp.Type)
            {
                case PropType.Door:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Door)";

                    GameObject MaxFracField = PropEditor.Find("MaxFrac").gameObject;
                    MaxFracField.SetActive(true);
                    float MaxFrac = CurrentProp.MaxFrac / 65536.0f;
                    MaxFracField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(MaxFrac.ToString());

                    GameObject PerimFracField = PropEditor.Find("PerimFrac").gameObject;
                    PerimFracField.SetActive(true);
                    float PerimFrac = CurrentProp.PerimFrac / 65536.0f;
                    PerimFracField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(PerimFrac.ToString());

                    GameObject AccelerationField = PropEditor.Find("Acceleration").gameObject;
                    AccelerationField.SetActive(true);
                    float Acceleration = CurrentProp.Acceleration / 65536.0f;
                    AccelerationField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(Acceleration.ToString());

                    GameObject DecelerationField = PropEditor.Find("Deceleration").gameObject;
                    DecelerationField.SetActive(true);
                    float Deceleration = CurrentProp.Deceleration / 65536.0f;
                    DecelerationField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(Deceleration.ToString());

                    GameObject MaxSpeedField = PropEditor.Find("MaxSpeed").gameObject;
                    MaxSpeedField.SetActive(true);
                    float MaxSpeed = CurrentProp.MaxSpeed / 65536.0f;
                    MaxSpeedField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(MaxSpeed.ToString());

                    GameObject DoorTypeField = PropEditor.Find("DoorType").gameObject;
                    DoorTypeField.SetActive(true);
                    DoorTypeField.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.GetConstantIndex(CurrentProp.DoorType));

                    GameObject LockFlagsButton = PropEditor.Find("ChangeLocks").gameObject;
                    LockFlagsButton.SetActive(true);

                    GameObject AutoCloseTimeField = PropEditor.Find("AutoCloseTime").gameObject;
                    AutoCloseTimeField.SetActive(true);
                    AutoCloseTimeField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentProp.AutoCloseTime.ToString());

                    //unk88

                    GameObject LinkedDoorField = PropEditor.Find("LinkedDoor").gameObject;
                    LinkedDoorField.SetActive(true);
                    int SelectedOption = (CurrentProp.LinkedDoor == null) ? 0 : DropdownOptions.FindDoorIndex(CurrentProp.LinkedDoor);
                    LinkedDoorField.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(SelectedOption);

                    //unkc4
                    break;
                case PropType.Key:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Key)";

                    LockFlagsButton = PropEditor.Find("ChangeLocks").gameObject;
                    LockFlagsButton.SetActive(true);
                    break;
                case PropType.Camera:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Camera)";

                    GameObject TargetPadField = PropEditor.Find("TargetPad").gameObject;
                    TargetPadField.SetActive(true);
                    TargetPadField.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.FindPadIndex(CurrentProp.TargetPad, true));

                    GameObject YawField = PropEditor.Find("Yaw").gameObject;
                    YawField.SetActive(true);
                    float Yaw = CurrentProp.Yaw * (3.141092641f * 2) / 65536.0f;
                    YawField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(Yaw.ToString());

                    GameObject YawMaxSpeedField = PropEditor.Find("YawMaxSpeed").gameObject;
                    YawMaxSpeedField.SetActive(true);
                    float YawMaxSpeed = CurrentProp.YawMaxSpeed * (3.141092641f * 2) / 65536.0f;
                    YawMaxSpeedField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(YawMaxSpeed.ToString());

                    GameObject MaxDistanceField = PropEditor.Find("MaxDistance").gameObject;
                    MaxDistanceField.SetActive(true);
                    MaxDistanceField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentProp.MaxDistance.ToString());
                    break;
                case PropType.AmmoCrate:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Ammo crate)";

                    GameObject AmmoTypeField = PropEditor.Find("AmmoType").gameObject;
                    AmmoTypeField.SetActive(true);
                    AmmoTypeField.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.GetConstantIndex(CurrentProp.AmmoType)); // Minus one, because ammo type 0 doesn't exist
                    break;
                case PropType.SingleMonitor:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Single-screen monitor)";

                    GameObject SelectScreenButton = PropEditor.Find("SelectScreenImage").gameObject;
                    SelectScreenButton.SetActive(true);
                    SelectScreenButton.GetComponentInChildren<TMP_Text>().text = "Select screen image";
                    break;
                case PropType.MultiMonitor:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Multi-screen monitor)";

                    SelectScreenButton = PropEditor.Find("SelectScreenImage").gameObject;
                    SelectScreenButton.SetActive(true);
                    SelectScreenButton.GetComponentInChildren<TMP_Text>().text = "Select screen images";
                    break;
                case PropType.Autogun:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Autogun)";

                    TargetPadField = PropEditor.Find("TargetPad").gameObject;
                    TargetPadField.SetActive(true);
                    SelectedOption = DropdownOptions.FindPadIndex(CurrentProp.TargetPad, true);
                    TargetPadField.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(SelectedOption);

                    //unk64
                    //unk68
                    //unk80
                    //unk84
                    break;
                case PropType.TintedGlass:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Tinted glass)";
                    //unk5c
                    break;
                case PropType.Hoverprop:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Hover prop)";

                    GameObject HoverTypeField = PropEditor.Find("HoverType").gameObject;
                    HoverTypeField.SetActive(true);
                    HoverTypeField.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.GetConstantIndex(CurrentProp.HoverType));
                    break;
                case PropType.Lift:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Lift)";

                    GameObject LiftPad1Field = PropEditor.Find("LiftPad1").gameObject;
                    LiftPad1Field.SetActive(true);
                    SelectedOption = (CurrentProp.LiftPad1 == null) ? 0 : DropdownOptions.FindPadIndex(CurrentProp.LiftPad1, true);
                    LiftPad1Field.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(SelectedOption);

                    GameObject LiftPad2Field = PropEditor.Find("LiftPad2").gameObject;
                    LiftPad2Field.SetActive(true);
                    SelectedOption = (CurrentProp.LiftPad2 == null) ? 0 : DropdownOptions.FindPadIndex(CurrentProp.LiftPad2, true);
                    LiftPad2Field.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(SelectedOption);

                    GameObject LiftPad3Field = PropEditor.Find("LiftPad3").gameObject;
                    LiftPad3Field.SetActive(true);
                    SelectedOption = (CurrentProp.LiftPad3 == null) ? 0 : DropdownOptions.FindPadIndex(CurrentProp.LiftPad3, true);
                    LiftPad3Field.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(SelectedOption);

                    GameObject LiftPad4Field = PropEditor.Find("LiftPad4").gameObject;
                    LiftPad4Field.SetActive(true);
                    SelectedOption = (CurrentProp.LiftPad4 == null) ? 0 : DropdownOptions.FindPadIndex(CurrentProp.LiftPad4, true);
                    LiftPad4Field.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(SelectedOption);

                    GameObject LiftDoor1Field = PropEditor.Find("LiftDoor1").gameObject;
                    LiftDoor1Field.SetActive(true);
                    SelectedOption = (CurrentProp.LiftDoor1 == null) ? 0 : DropdownOptions.FindDoorIndex(CurrentProp.LiftDoor1);
                    LiftDoor1Field.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(SelectedOption);

                    GameObject LiftDoor2Field = PropEditor.Find("LiftDoor2").gameObject;
                    LiftDoor2Field.SetActive(true);
                    SelectedOption = (CurrentProp.LiftDoor2 == null) ? 0 : DropdownOptions.FindDoorIndex(CurrentProp.LiftDoor2);
                    LiftDoor2Field.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(SelectedOption);

                    GameObject LiftDoor3Field = PropEditor.Find("LiftDoor3").gameObject;
                    LiftDoor3Field.SetActive(true);
                    SelectedOption = (CurrentProp.LiftDoor3 == null) ? 0 : DropdownOptions.FindDoorIndex(CurrentProp.LiftDoor3);
                    LiftDoor3Field.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(SelectedOption);

                    GameObject LiftDoor4Field = PropEditor.Find("LiftDoor4").gameObject;
                    LiftDoor4Field.SetActive(true);
                    SelectedOption = (CurrentProp.LiftDoor4 == null) ? 0 : DropdownOptions.FindDoorIndex(CurrentProp.LiftDoor4);
                    LiftDoor4Field.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(SelectedOption);

                    AccelerationField = PropEditor.Find("Acceleration").gameObject;
                    AccelerationField.SetActive(true);
                    Acceleration = CurrentProp.Acceleration / 65536.0f;
                    AccelerationField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(Acceleration.ToString());

                    MaxSpeedField = PropEditor.Find("MaxSpeed").gameObject;
                    MaxSpeedField.SetActive(true);
                    MaxSpeed = CurrentProp.MaxSpeed / 65536.0f;
                    MaxSpeedField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(MaxSpeed.ToString());

                    GameObject LiftDoorStopFoldout = PropEditor.Find("LiftDoorStopFoldout").gameObject;
                    LiftDoorStopFoldout.SetActive(true);

                    foreach (GameObject CurrentLiftStop in LiftStops)
                    {
                        Destroy(CurrentLiftStop);
                    }

                    if (CurrentProp.LiftStops.Count > 0)
                    {
                        LiftDoorStopFoldout.GetComponent<Foldout>().Open();

                        for (int i = 0; i < CurrentProp.LiftStops.Count; i++)
                        {
                            Prop LiftStop = CurrentProp.LiftStops[i];

                            // The panel that contains each lift stop's options
                            GameObject Content = Instantiate(LiftDoorStopContent, PropEditor.Find("LiftDoorStopFoldout/Content"));
                            Content.name = "LiftDoorStopContent" + i.ToString();
                            LiftStops.Add(Content);

                            string StopNumber = (LiftStop.StopNumber + 1).ToString();
                            Toggle ActiveToggle = Content.transform.Find("StopNumber/LiftStopToggle" + StopNumber).GetComponent<Toggle>();
                            ActiveToggle.SetIsOnWithoutNotify(true);

                            GameObject LiftStopDoorField = Content.transform.Find("LiftStopDoor").gameObject;
                            LiftStopDoorField.SetActive(true);
                            LiftStopDoorField.GetComponentInChildren<BetterDropdown>().ClearOptions();
                            LiftStopDoorField.GetComponentInChildren<BetterDropdown>().OptionSelected.AddListener((SelectedIndex, SelectedText) => OnLiftDoorSelected(i, SelectedIndex, SelectedText));
                            LiftStopDoorField.GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.DoorsWithNone;
                            SelectedOption = (LiftStop.LiftStopDoor == null) ? 0 : DropdownOptions.FindDoorIndex(LiftStop.LiftStopDoor);
                            LiftStopDoorField.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(SelectedOption);
                        }
                    }
                    else
                    {
                        LiftDoorStopFoldout.GetComponent<Foldout>().Close();
                    }

                    //unk84
                    break;
                case PropType.Fan:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Fan)";

                    AccelerationField = PropEditor.Find("Acceleration").gameObject;
                    AccelerationField.SetActive(true);
                    Acceleration = CurrentProp.Acceleration / 65536.0f;
                    AccelerationField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(Acceleration.ToString());

                    MaxSpeedField = PropEditor.Find("MaxSpeed").gameObject;
                    MaxSpeedField.SetActive(true);
                    MaxSpeed = CurrentProp.MaxSpeed / 65536.0f;
                    MaxSpeedField.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(MaxSpeed.ToString());

                    GameObject FanOnToggle = PropEditor.Find("FanOn").gameObject;
                    FanOnToggle.SetActive(true);
                    FanOnToggle.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentProp.On);
                    break;
                case PropType.Hovercar:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Hover vehicle)";

                    GameObject ScriptField = PropEditor.Find("Script").gameObject;
                    ScriptField.SetActive(true);
                    ScriptField.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.FindAssignableAIListIndex(CurrentProp.AIList));
                    break;
                case PropType.Hovercopter:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Chopper)";

                    ScriptField = PropEditor.Find("Script").gameObject;
                    ScriptField.SetActive(true);
                    ScriptField.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.FindAssignableAIListIndex(CurrentProp.AIList));
                    break;
                case PropType.Mine:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Mine)";

                    GameObject WeaponTypeField = PropEditor.Find("WeaponType").gameObject;
                    WeaponTypeField.SetActive(true);
                    WeaponTypeField.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.GetConstantIndex(CurrentProp.WeaponType));
                    break;
                case PropType.Weapon:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Weapon)";

                    WeaponTypeField = PropEditor.Find("WeaponType").gameObject;
                    WeaponTypeField.SetActive(true);
                    WeaponTypeField.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DropdownOptions.GetConstantIndex(CurrentProp.WeaponType));
                    
                    GameObject LinkedGunField = PropEditor.Find("LinkedGun").gameObject;
                    LinkedGunField.SetActive(true);
                    SelectedOption = DropdownOptions.FindWeaponIndex(CurrentProp.LinkedWeapon);

                    LinkedGunField.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(SelectedOption);
                    break;
                case PropType.StandardObject:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Standard object)";
                    break;
                case PropType.Debris:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Debris)";
                    break;
                case PropType.AmmoCrateMulti:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Ammo crate (multiplayer))";
                    break;
                case PropType.Shield:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Shield)";
                    break;
                case PropType.GasBottle:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Gas bottle)";
                    break;
                case PropType.Glass:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Glass)";
                    break;
                case PropType.Safe:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Safe)";
                    break;
                case PropType.Hoverbike:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Hoverbike)";
                    break;
                case PropType.EscalatorStep:
                    PropLabel.text = (CurrentProp.DisplayName ?? "Prop") + " (Escalator step)";
                    break;
                default:
                    // ERROR - incorrect syntax
                    break;
            }
        }

        GameObject NameChangeFoldout = PropEditor.Find("NameChangeFoldout").gameObject;
        NameChangeFoldout.SetActive(true);

        if (CurrentProp.RenameObject != null)
        {
            NameChangeFoldout.GetComponent<Foldout>().Open();

            Prop RenameObject = CurrentProp.RenameObject;

            NameChangeFoldout.transform.Find("Content/WeaponAppearance").GetComponentInChildren<BetterDropdown>(true).SetSelectedOptionWithoutEvent(DropdownOptions.GetConstantIndex(RenameObject.WeaponType));
            NameChangeFoldout.transform.Find("Content/MenuListName").GetComponentInChildren<BetterDropdown>(true).SetSelectedOptionWithoutEvent(LanguageFileHolder.GetLevelLanguageFile().LanguageStrings.IndexOf(RenameObject.MenuListText));
            NameChangeFoldout.transform.Find("Content/PickupText").GetComponentInChildren<BetterDropdown>(true).SetSelectedOptionWithoutEvent(LanguageFileHolder.GetLevelLanguageFile().LanguageStrings.IndexOf(RenameObject.PickUpText));

            NameChangeFoldout.transform.Find("Label/NameChangeAddDelete").GetComponentInChildren<TextMeshProUGUI>().text = "-";
        }
        else
        {
            NameChangeFoldout.GetComponent<Foldout>().Close();

            NameChangeFoldout.transform.Find("Label/NameChangeAddDelete").GetComponentInChildren<TextMeshProUGUI>().text = "+";
        }

        if (!CurrentProp.IsGlobal && !CurrentProp.IsAttachedToCharacter && !CurrentProp.IsAttachedToOtherProp)
        {
            GameObject SubPropFoldout = PropEditor.Find("SubPropFoldout").gameObject;
            SubPropFoldout.SetActive(true);

            foreach (GameObject CurrentSubProp in SubProps)
            {
                Destroy(CurrentSubProp);
            }
            SubProps.Clear();

            if (CurrentProp.SubProps.Count > 0)
            {
                SubPropFoldout.GetComponent<Foldout>().Open();

                for (int i = 0; i < CurrentProp.SubProps.Count; ++i)
                {
                    Prop SubProp = CurrentProp.SubProps[i];

                    GameObject Content = Instantiate(SubPropContent, PropEditor.Find("SubPropFoldout/Content"));
                    Content.name = "SubPropContent" + i;

                    Content.transform.Find("SubPropLabel").GetComponent<TextMeshProUGUI>().text = SubProp.Type.ToString();

                    SubProps.Add(Content);
                }
            }
            else
            {
                SubPropFoldout.GetComponent<Foldout>().Close();
            }
        }
        else
        {
            PropEditor.Find("SubPropFoldout").gameObject.SetActive(false);
        }
    }

    public void EnterRoom(Room InRoom)
    {
        CurrentRooms = new Room[1] { InRoom };

        RoomEditor.gameObject.SetActive(true);

        RoomLabel.text = "Room " + CurrentRoom.NumberString;

        Mesh RoomMesh = CurrentRoom.GetComponent<MeshFilter>().mesh;
        RoomEditor.Find("RoomInformation").gameObject.SetActive(true);
        RoomEditor.Find("RoomInformation").GetComponentInChildren<TextMeshProUGUI>().text = $"Triangles: {RoomMesh.triangles.Length / 3}, vertices: {RoomMesh.triangles.Length}";

        RoomEditor.Find("MinimumBrightness").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentRoom.MinimumBrightness.ToString());
        RoomEditor.Find("MaximumBrightness").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentRoom.MaximumBrightness.ToString());

        MarkAsNeedUpdating();
    }

    public void EnterRooms(Room[] InRooms)
    {
        CurrentRooms = InRooms;

        RoomEditor.gameObject.SetActive(true);

        RoomLabel.text = InRooms.Length + " rooms selected";

        RoomEditor.Find("RoomInformation").gameObject.SetActive(false);

        RoomEditor.Find("MinimumBrightness").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");
        RoomEditor.Find("MaximumBrightness").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");

        MarkAsNeedUpdating();
    }

    public void EnterCollision(Collision InCollision, Tile InTile)
    {
        Debug.Log($"Selected tile: {InTile.ID}");
        if (InTile != CurrentTile)
        {
            CurrentCollision?.UnselectTile(CurrentTile);
            InCollision.SelectTile(InTile);

            CurrentTile = InTile;
        }

        if (InCollision != CurrentCollision)
        {
            CurrentCollision = InCollision;
        }

        CollisionEditor.gameObject.SetActive(true);

        CollisionLabel.text = "Room " + CurrentCollision.CollisionRoom.NumberString + ", tile " + CurrentTile.ID.ToString();

        CollisionEditor.Find("Floor1").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentTile.Floor1);
        CollisionEditor.Find("Floor2").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentTile.Floor2);
        CollisionEditor.Find("Wall").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentTile.Wall);
        CollisionEditor.Find("BlockSight").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentTile.BlockSight);
        CollisionEditor.Find("BlockGunfire").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentTile.BlockGunfire);
        CollisionEditor.Find("LiftFloor").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentTile.LiftFloor);
        CollisionEditor.Find("Ladder").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentTile.Ladder);
        CollisionEditor.Find("RampWall").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentTile.RampWall);
        CollisionEditor.Find("Slope").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentTile.Slope);
        CollisionEditor.Find("Underwater").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentTile.Underwater);
        CollisionEditor.Find("Flag0400").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentTile.Flag0400);
        CollisionEditor.Find("AICrouchesTile").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentTile.AICrouches);
        CollisionEditor.Find("AIDucksTile").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentTile.AIDucks);
        CollisionEditor.Find("AlwaysAscend").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentTile.AlwaysAscend);
        CollisionEditor.Find("DieOnTouch").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentTile.DieOnTouch);
        CollisionEditor.Find("PlayerOnlyLadder").GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentTile.PlayerOnlyLadder);
        CollisionEditor.Find("FootstepSound").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(CurrentTile.FloorType);

        MarkAsNeedUpdating();
    }

    public void EnterPortal(Portal InPortal)
    {
        CurrentPortals = new Portal[1] { InPortal };

        PortalEditor.gameObject.SetActive(true);

        PortalLabel.text = "Portal " + CurrentPortal.ID;

        PortalEditor.Find("FirstRoom").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(CurrentPortal.FirstRoom.ID - 1);
        PortalEditor.Find("SecondRoom").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(CurrentPortal.SecondRoom.ID - 1);

        MarkAsNeedUpdating();
    }

    public void EnterPortals(Portal[] InPortals)
    {
        CurrentPortals = InPortals;

        PortalEditor.gameObject.SetActive(true);

        PortalLabel.text = CurrentPortals.Length + " portals selected";

        PortalEditor.Find("FirstRoom").GetComponentInChildren<BetterDropdown>().SetNoOption();
        PortalEditor.Find("SecondRoom").GetComponentInChildren<BetterDropdown>().SetNoOption();

        MarkAsNeedUpdating();
    }

    public void EnterBackgroundLight(BackgroundLight InLight)
    {
        CurrentBackgroundLights = new BackgroundLight[1] { InLight };

        BackgroundLightEditor.gameObject.SetActive(true);

        BackgroundLightLabel.text = "Light " + CurrentBackgroundLight.ID;

        Transform DirectionField = BackgroundLightEditor.Find("LightNormal");
        DirectionField.Find("XNormal").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentBackgroundLight.Direction.x.ToString());
        DirectionField.Find("YNormal").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentBackgroundLight.Direction.y.ToString());
        DirectionField.Find("ZNormal").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentBackgroundLight.Direction.z.ToString());

        BackgroundLightEditor.Find("Room").GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(CurrentBackgroundLight.LightRoom.ID - 1);
        BackgroundLightEditor.Find("Size").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentBackgroundLight.Size.ToString());
        BackgroundLightEditor.Find("RingSize").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentBackgroundLight.RingSize.ToString());

        MarkAsNeedUpdating();
    }

    public void EnterBackgroundLights(BackgroundLight[] InLights)
    {
        CurrentBackgroundLights = InLights;

        BackgroundLightEditor.gameObject.SetActive(true);

        BackgroundLightLabel.text = CurrentBackgroundLights.Length + " lights selected";

        Transform DirectionField = BackgroundLightEditor.Find("LightNormal");
        DirectionField.Find("XNormal").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");
        DirectionField.Find("YNormal").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");
        DirectionField.Find("ZNormal").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");

        BackgroundLightEditor.Find("Room").GetComponentInChildren<BetterDropdown>().SetNoOption();
        BackgroundLightEditor.Find("Size").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");
        BackgroundLightEditor.Find("RingSize").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");

        MarkAsNeedUpdating();
    }

    private void OnSelectionChanged(List<GameObject> UnselectedObjects)
    {
        if (DataHolder.RaycasterInstance.Mode == RaycastMode.WaypointLink)
        {
            DataHolder.RaycasterInstance.Mode = RaycastMode.Pad;
        }

        if (SelectionManager.CurrentSelectedObject == null)
        {
            Clear();
            return;
        }
        
        if (SelectionManager.SelectedObjects.Count > 1)
        {
            MultipleObjectsSelected = true;

            if (EditModeBar.CurrentMode == EditMode.Objects)
            {
                Pad[] Pads = new Pad[SelectionManager.SelectedObjects.Count];

                for (int i = 0; i < Pads.Length; ++i)
                {
                    GameObject CurrentObject = SelectionManager.SelectedObjects[i];
                    Pads[i] = CurrentObject.GetComponent<Pad>();
                }

                EnterPads(Pads);

                if (FlagsEditorInstance.isActiveAndEnabled)
                {
                    FlagsEditorInstance.gameObject.SetActive(false);
                }
            }
            else if (EditModeBar.CurrentMode == EditMode.Rooms)
            {
                Room[] Rooms = new Room[SelectionManager.SelectedObjects.Count];

                for (int i = 0; i < Rooms.Length; ++i)
                {
                    GameObject CurrentObject = SelectionManager.SelectedObjects[i];
                    Rooms[i] = CurrentObject.GetComponent<Room>();
                }

                EnterRooms(Rooms);
            }
            else if (EditModeBar.CurrentMode == EditMode.Portals)
            {
                Portal[] Portals = new Portal[SelectionManager.SelectedObjects.Count];

                for (int i = 0; i < Portals.Length; ++i)
                {
                    GameObject CurrentObject = SelectionManager.SelectedObjects[i];
                    Portals[i] = CurrentObject.GetComponent<Portal>();
                }

                EnterPortals(Portals);
            }
            /*else if (EditModeBar.CurrentMode == EditMode.Collision)
            {

            }*/
            else if (EditModeBar.CurrentMode == EditMode.BackgroundLights)
            {
                BackgroundLight[] BackgroundLights = new BackgroundLight[SelectionManager.SelectedObjects.Count];

                for (int i = 0; i < BackgroundLights.Length; ++i)
                {
                    GameObject CurrentObject = SelectionManager.SelectedObjects[i];
                    BackgroundLights[i] = CurrentObject.GetComponent<BackgroundLight>();
                }

                EnterBackgroundLights(BackgroundLights);
            }
        }
        else
        {
            MultipleObjectsSelected = false;

            if (SelectionManager.CurrentSelectedObject.TryGetComponent(out Pad SelectedPad))
            {
                EnterObject(SelectedPad);

                if (SelectedPad.PadProp != null && FlagsEditorInstance.isActiveAndEnabled)
                {
                    FlagsEditorInstance.EnterProp(SelectedPad.PadProp);
                }
            }
            else if (SelectionManager.CurrentSelectedObject.TryGetComponent(out Room SelectedRoom))
            {
                EnterRoom(SelectedRoom);
            }
            else if (SelectionManager.CurrentSelectedObject.TryGetComponent(out Portal SelectedPortal))
            {
                EnterPortal(SelectedPortal);
            }
            /*else if (SelectedObject.TryGetComponent(out LevelCollision SelectedCollision))
            {
                EnterCollision(SelectedCollision);
            }*/
            else if (SelectionManager.CurrentSelectedObject.TryGetComponent(out BackgroundLight SelectedBackgroundLight))
            {
                EnterBackgroundLight(SelectedBackgroundLight);
            }
        }
    }

    public void Clear()
    {
        foreach (Transform Child in transform)
        {
            Child.gameObject.SetActive(false);
        }

        CurrentPads = null;
        CurrentProp = null;
        CurrentRooms = null;
        CurrentPortals = null;

        if (CurrentCollision != null && CurrentTile != null)
        {
            CurrentCollision.UnselectTile(CurrentTile);
        }

        CurrentCollision = null;
        CurrentTile = null;
        CurrentBackgroundLights = null;
    }

    public void OnButtonClicked(Button EventButton)
    {
        string FieldName = EventButton.name;

        // *** PADS ***

        if (FieldName == "AddOrDeleteWaypoint")
        {
            if (!MultipleObjectsSelected)
            {
                if (CurrentWaypoint == null) // add
                {
                    CurrentPad.AddNewWaypoint();

                    CurrentWaypoint = CurrentPad.PadWaypoint;

                    PadLabel.text = "Waypoint pad " + CurrentPad.NumberString;
                    PadEditor.Find("AddOrDeleteWaypoint").GetComponentInChildren<TextMeshProUGUI>().text = "Remove waypoint from pad";
                    PadEditor.Find("WaypointLabel").gameObject.SetActive(true);

                    ScrollableList NeighbourList = PadEditor.Find("NeighbourList").GetComponent<ScrollableList>();
                    NeighbourList.gameObject.SetActive(true);
                    NeighbourList.Clear();

                    //PadEditor.Find("MoveOutwardsOnly").gameObject.SetActive(false);
                    //PadEditor.Find("MoveInwardsOnly").gameObject.SetActive(false);

                    PadEditor.Find("WaypointButtons").gameObject.SetActive(true);

                    GameObject SelectWaygroupDropdown = PadEditor.Find("SelectWaygroup").gameObject;
                    SelectWaygroupDropdown.SetActive(true);
                    SelectWaygroupDropdown.GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.Waygroups;
                    SelectWaygroupDropdown.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(DataHolder.Waygroups.IndexOf(CurrentWaypoint.ParentWaygroup));
                }
                else // delete
                {
                    CurrentPad.DeleteWaypoint();

                    CurrentWaypoint = null;

                    PadLabel.text = "Pad " + CurrentPad.NumberString;
                    PadEditor.Find("AddOrDeleteWaypoint").GetComponentInChildren<TextMeshProUGUI>().text = "Add waypoint to pad";
                    PadEditor.Find("WaypointLabel").gameObject.SetActive(false);
                    PadEditor.Find("NeighbourList").gameObject.SetActive(false);
                    PadEditor.Find("MoveOutwardsOnly").gameObject.SetActive(false);
                    PadEditor.Find("MoveInwardsOnly").gameObject.SetActive(false);
                    PadEditor.Find("WaypointButtons").gameObject.SetActive(false);
                    PadEditor.Find("SelectWaygroup").gameObject.SetActive(false);
                }
            }
            else
            {
                if (PadEditor.Find("AddOrDeleteWaypoint").GetComponentInChildren<TextMeshProUGUI>().text == "Add waypoint to all pads")
                {
                    foreach (Pad IteratedPad in CurrentPads)
                    {
                        IteratedPad.AddNewWaypoint();
                    }

                    PadEditor.Find("AddOrDeleteWaypoint").GetComponentInChildren<TextMeshProUGUI>().text = "Remove waypoints from all pads";

                    GameObject SelectWaygroupDropdown = PadEditor.Find("SelectWaygroup").gameObject;
                    SelectWaygroupDropdown.SetActive(true);
                    SelectWaygroupDropdown.GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.Waygroups;
                    SelectWaygroupDropdown.GetComponentInChildren<BetterDropdown>().SetNoOption();
                }
                else
                {
                    foreach (Pad IteratedPad in CurrentPads)
                    {
                        if (IteratedPad.PadWaypoint != null)
                        {
                            IteratedPad.DeleteWaypoint();
                        }
                    }

                    PadEditor.Find("AddOrDeleteWaypoint").GetComponentInChildren<TextMeshProUGUI>().text = "Add waypoint to all pads";
                    PadEditor.Find("SelectWaygroup").gameObject.SetActive(false);
                }
            }
        }
        else if (FieldName == "AddWaypointLink")
        {
            DataHolder.RaycasterInstance.Mode = RaycastMode.WaypointLink;
            PadEditor.Find("WaypointLinkText").gameObject.SetActive(true);
        }
        else if (FieldName == "DeleteWaypointLink")
        {
            if (CurrentNeighbour == null)
            {
                return;
            }

            CurrentWaypoint.DeleteNeighbour(CurrentNeighbour);

            ScrollableList WaypointNeighbourList = PadEditor.Find("NeighbourList").GetComponent<ScrollableList>();
            WaypointNeighbourList.RemoveSelectedItem();

            if (WaypointNeighbourList.ListItems.Count == 0)
            {
                PadEditor.Find("MoveOutwardsOnly").gameObject.SetActive(false);
                PadEditor.Find("MoveInwardsOnly").gameObject.SetActive(false);
            }
        }
        else if (FieldName == "CancelWaypointLinking")
        {
            DataHolder.RaycasterInstance.Mode = RaycastMode.None;
            PadEditor.Find("WaypointLinkText").gameObject.SetActive(false);
        }

        // *** PROPS ***

        else if (FieldName == "ChangeAttachment")
        {
            AttachmentEditorInstance.Initiate(CurrentProp);
        }
        else if (FieldName == "SelectModel")
        {
            PropModelPickerInstance.EnterPad(CurrentPad, PropModelPicker.ModelMode.Prop);
        }
        else if (FieldName == "SelectBody")
        {
            PropModelPickerInstance.EnterPad(CurrentPad, PropModelPicker.ModelMode.Character);
        }
        else if (FieldName == "SelectHead")
        {
            PropModelPickerInstance.EnterPad(CurrentPad, PropModelPicker.ModelMode.Head);
        }
        else if (FieldName == "ChangeFlags")
        {
            FlagsEditorInstance.EnterProp(CurrentProp);
        }
        else if (FieldName == "ChangeLocks")
        {
            FlagsEditorInstance.EnterPropWithLockFlags(CurrentProp);
        }
        else if (FieldName == "EditParentProp")
        {
            EnterObject(CurrentProp.ParentProp);
        }
        else if (FieldName == "SubPropAdd")
        {
            if (CurrentProp.SubProps.Count > 5)
            {
                Debug.LogError("Too many subprops, cannot add a new one");
                return;
            }
            if (CurrentProp.SubProps.Count == 0)
            {
                PropEditor.Find("SubPropFoldout").GetComponent<Foldout>().Open();
            }

            Prop NewSubProp = DataHolder.AddProp(PropType.StandardObject);

            NewSubProp.IsGlobal = false;
            NewSubProp.IsAttachedToOtherProp = true;
            NewSubProp.ParentProp = CurrentProp;

            CurrentProp.SubProps.Add(NewSubProp);

            GameObject Content = Instantiate(SubPropContent, PropEditor.Find("SubPropFoldout/Content"));
            Content.name = "SubPropContent" + (CurrentProp.SubProps.Count - 1);

            Content.transform.Find("SubPropLabel").GetComponent<TextMeshProUGUI>().text = NewSubProp.Type.ToString();

            SubProps.Add(Content);
        }
        else if (FieldName == "SubPropEdit")
        {
            int SubPropNumber = int.Parse(EventButton.transform.parent.name[^1..]); // Get the number of the sub prop from SubPropContent's name
            EnterObject(CurrentProp.SubProps[SubPropNumber]);
        }
        else if (FieldName == "SubPropDelete")
        {
            int SubPropNumber = int.Parse(EventButton.transform.parent.name[^1..]); // Get the number of the sub prop from SubPropContent's name

            DataHolder.DeleteProp(CurrentProp.SubProps[SubPropNumber]);
            Destroy(EventButton.transform.parent.gameObject);
            SubProps.RemoveAt(SubPropNumber);

            for (int i = 0; i < SubProps.Count; ++i)
            {
                SubProps[i].name = "SubPropContent" + i;
            }

            if (CurrentProp.SubProps.Count == 0)
            {
                PropEditor.Find("SubPropFoldout").GetComponent<Foldout>().Close();
            }
        }
        else if (FieldName == "NameChangeAddDelete")
        {
            GameObject NameChangeFoldout = PropEditor.Find("NameChangeFoldout").gameObject;

            if (CurrentProp.RenameObject == null)
            {
                Prop NewRenameObject = DataHolder.AddProp(PropType.Rename);
                CurrentProp.RenameObject = NewRenameObject;
                NewRenameObject.IsAttachedToOtherProp = true;
                NewRenameObject.ParentProp = CurrentProp;

                NewRenameObject.WeaponType = CurrentProp.WeaponType;
                NewRenameObject.PickUpText = LanguageFileHolder.GetLevelLanguageFile().LanguageStrings[0];
                NewRenameObject.MenuListText = LanguageFileHolder.GetLevelLanguageFile().LanguageStrings[0];

                NameChangeFoldout.GetComponent<Foldout>().Open();

                NameChangeFoldout.transform.Find("Content/WeaponAppearance").GetComponentInChildren<BetterDropdown>(true).SetSelectedOptionWithoutEvent(DropdownOptions.GetConstantIndex(NewRenameObject.WeaponType));
                NameChangeFoldout.transform.Find("Content/MenuListName").GetComponentInChildren<BetterDropdown>(true).SetSelectedOptionWithoutEvent(LanguageFileHolder.GetLevelLanguageFile().LanguageStrings.IndexOf(NewRenameObject.MenuListText));
                NameChangeFoldout.transform.Find("Content/PickupText").GetComponentInChildren<BetterDropdown>(true).SetSelectedOptionWithoutEvent(LanguageFileHolder.GetLevelLanguageFile().LanguageStrings.IndexOf(NewRenameObject.PickUpText));

                EventButton.GetComponentInChildren<TextMeshProUGUI>().text = "-";
            }
            else
            {
                DataHolder.DeleteProp(CurrentProp.RenameObject);

                NameChangeFoldout.GetComponent<Foldout>().Close();

                EventButton.GetComponentInChildren<TextMeshProUGUI>().text = "+";
            }
        }
        else if (FieldName == "LiftDoorStopAdd")
        {
            GameObject LiftDoorStopFoldout = PropEditor.Find("LiftDoorStopFoldout").gameObject;
            LiftDoorStopFoldout.SetActive(true);

            if (CurrentProp.LiftStops.Count == 4)
            {
                Debug.LogError("Too many lift stops, cannot add a new one");
                return;
            }
            if (CurrentProp.SubProps.Count == 0)
            {
                LiftDoorStopFoldout.GetComponent<Foldout>().Open();
            }

            Prop LiftStop = DataHolder.AddProp(PropType.LiftDoor);

            LiftStop.IsGlobal = false;
            LiftStop.IsAttachedToOtherProp = true;
            LiftStop.ParentProp = CurrentProp;
            LiftStop.LiftStopLift = CurrentProp;
            LiftStop.LiftStopDoor = null;
            LiftStop.StopNumber = 0;

            CurrentProp.LiftStops.Add(LiftStop);

            GameObject Content = Instantiate(LiftDoorStopContent, PropEditor.Find("LiftDoorStopFoldout/Content"));
            Content.name = "LiftDoorStopContent" + (CurrentProp.LiftStops.Count - 1);

            LiftStops.Add(Content);

            string StopNumber = (LiftStop.StopNumber + 1).ToString();
            Toggle ActiveToggle = Content.transform.Find("StopNumber/LiftStopToggle" + StopNumber).GetComponent<Toggle>();
            ActiveToggle.SetIsOnWithoutNotify(true);

            GameObject LiftStopDoorField = Content.transform.Find("LiftStopDoor").gameObject;
            LiftStopDoorField.SetActive(true);
            LiftStopDoorField.GetComponentInChildren<BetterDropdown>().ClearOptions();
            LiftStopDoorField.GetComponentInChildren<BetterDropdown>().OptionList = DropdownOptions.DoorsWithNone;
            int SelectedOption = (LiftStop.LiftStopDoor == null) ? 0 : DropdownOptions.FindDoorIndex(LiftStop.LiftStopDoor);
            LiftStopDoorField.GetComponentInChildren<BetterDropdown>().SetSelectedOptionWithoutEvent(SelectedOption);
        }
        else if (FieldName == "LiftDoorStopDelete")
        {
            int LiftStopNumber = int.Parse(EventButton.transform.parent.name[^1..]); // Get the number of the lift stop from LiftDoorStopContent's name

            Debug.Log(LiftStopNumber);
            Debug.Log(LiftStops.Count);

            DataHolder.DeleteProp(CurrentProp.LiftStops[LiftStopNumber]);
            Destroy(EventButton.transform.parent.gameObject);
            LiftStops.RemoveAt(LiftStopNumber);
            Debug.Log(LiftStops.Count);

            for (int i = 0; i < LiftStops.Count; ++i)
            {
                LiftStops[i].name = "LiftDoorStopContent" + i;
            }

            if (CurrentProp.LiftStops.Count == 0)
            {
                PropEditor.Find("LiftDoorStopFoldout").GetComponent<Foldout>().Close();
            }
        }

        // *** BG LIGHTS ***

        else if (FieldName == "SelectLightColour")
        {
            ColourSelectorInstance.EnterBackgroundLight(CurrentBackgroundLights);
        }
    }

    public override void OnDropdownChanged(string DropdownName, int SelectedIndex, string SelectedText)
    {
        if (DropdownName == "SelectWaygroup")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                if (IteratedPad.PadWaypoint != null)
                {
                    IteratedPad.PadWaypoint.ParentWaygroup = DataHolder.Waygroups[SelectedIndex];
                }
            }
        }
        else if (DropdownName == "PropType")
        {
            CurrentProp.ChangePropType((PropType)SelectedIndex);
            EnterObject(CurrentPad);
        }
        else if (DropdownName == "DoorType")
        {
            CurrentProp.DoorType = DropdownOptions.GetConstant("DOORTYPE", SelectedIndex);
        }
        else if (DropdownName == "LinkedDoor")
        {
            if (SelectedIndex == 0)
            {
                CurrentProp.LinkedDoor = null;
            }
            else
            {
                foreach (Pad IteratedPad in DataHolder.Pads)
                {
                    if (IteratedPad.PadProp != null && IteratedPad.PadProp.Type == PropType.Door && IteratedPad.NumberString == SelectedText[..4])
                    {
                        CurrentProp.LinkedDoor = IteratedPad.PadProp;
                        break;
                    }
                }
            }
        }
        else if (DropdownName == "TargetPad")
        {
            if (SelectedIndex == 0)
            {
                CurrentProp.TargetPad = null;
            }
            else
            {
                foreach (Pad IteratedPad in DataHolder.Pads)
                {
                    if (IteratedPad.NumberString == SelectedText[..4])
                    {
                        CurrentProp.TargetPad = IteratedPad;
                        break;
                    }
                }
            }
        }
        else if (DropdownName == "AmmoType")
        {
            CurrentProp.AmmoType = DropdownOptions.GetConstant("AMMOTYPE", SelectedIndex);
        }
        else if (DropdownName == "LiftPad1")
        {
            if (SelectedIndex == 0)
            {
                CurrentProp.LiftPad1 = null;
            }
            else
            {
                foreach (Pad IteratedPad in DataHolder.Pads)
                {
                    if (IteratedPad.NumberString == SelectedText[..4])
                    {
                        CurrentProp.LiftPad1 = IteratedPad;
                        break;
                    }
                }
            }
        }
        else if (DropdownName == "LiftPad2")
        {
            if (SelectedIndex == 0)
            {
                CurrentProp.LiftPad2 = null;
            }
            else
            {
                foreach (Pad IteratedPad in DataHolder.Pads)
                {
                    if (IteratedPad.NumberString == SelectedText[..4])
                    {
                        CurrentProp.LiftPad2 = IteratedPad;
                        break;
                    }
                }
            }
        }
        else if (DropdownName == "LiftPad3")
        {
            if (SelectedIndex == 0)
            {
                CurrentProp.LiftPad3 = null;
            }
            else
            {
                foreach (Pad IteratedPad in DataHolder.Pads)
                {
                    if (IteratedPad.NumberString == SelectedText[..4])
                    {
                        CurrentProp.LiftPad3 = IteratedPad;
                        break;
                    }
                }
            }
        }
        else if (DropdownName == "LiftPad4")
        {
            if (SelectedIndex == 0)
            {
                CurrentProp.LiftPad4 = null;
            }
            else
            {
                foreach (Pad IteratedPad in DataHolder.Pads)
                {
                    if (IteratedPad.NumberString == SelectedText[..4])
                    {
                        CurrentProp.LiftPad4 = IteratedPad;
                        break;
                    }
                }
            }
        }
        else if (DropdownName == "LiftDoor1")
        {
            if (SelectedIndex == 0)
            {
                CurrentProp.LiftDoor1 = null;
            }
            else
            {
                foreach (Pad IteratedPad in DataHolder.Pads)
                {
                    if (IteratedPad.PadProp != null && IteratedPad.PadProp.Type == PropType.Door && IteratedPad.NumberString == SelectedText[..4])
                    {
                        CurrentProp.LiftDoor1 = IteratedPad.PadProp;
                        break;
                    }
                }
            }
        }
        else if (DropdownName == "LiftDoor2")
        {
            if (SelectedIndex == 0)
            {
                CurrentProp.LiftDoor2 = null;
            }
            else
            {
                foreach (Pad IteratedPad in DataHolder.Pads)
                {
                    if (IteratedPad.PadProp != null && IteratedPad.PadProp.Type == PropType.Door && IteratedPad.NumberString == SelectedText[..4])
                    {
                        CurrentProp.LiftDoor2 = IteratedPad.PadProp;
                        break;
                    }
                }
            }
        }
        else if (DropdownName == "LiftDoor3")
        {
            if (SelectedIndex == 0)
            {
                CurrentProp.LiftDoor3 = null;
            }
            else
            {
                foreach (Pad IteratedPad in DataHolder.Pads)
                {
                    if (IteratedPad.PadProp != null && IteratedPad.PadProp.Type == PropType.Door && IteratedPad.NumberString == SelectedText[..4])
                    {
                        CurrentProp.LiftDoor3 = IteratedPad.PadProp;
                        break;
                    }
                }
            }
        }
        else if (DropdownName == "LiftDoor4")
        {
            if (SelectedIndex == 0)
            {
                CurrentProp.LiftDoor4 = null;
            }
            else
            {
                foreach (Pad IteratedPad in DataHolder.Pads)
                {
                    if (IteratedPad.PadProp != null && IteratedPad.PadProp.Type == PropType.Door && IteratedPad.NumberString == SelectedText[..4])
                    {
                        CurrentProp.LiftDoor4 = IteratedPad.PadProp;
                        break;
                    }
                }
            }
        }
        else if (DropdownName == "Script")
        {
            //int AIListIndex = int.Parse(EventDropdown.captionText.text[..4], System.Globalization.NumberStyles.HexNumber);
            CurrentProp.AIList?.ConnectedProps.Remove(CurrentProp);

            CurrentProp.AIList = DropdownOptions.FindAssignableAIList(SelectedText);
            CurrentProp.AIList.ConnectedProps.Add(CurrentProp);
        }
        else if (DropdownName == "WeaponType")
        {
            CurrentProp.WeaponType = DropdownOptions.GetConstant("WEAPON", SelectedIndex);
        }
        else if (DropdownName == "PadPreset")
        {
            if (SelectedIndex == 0)
            {
                CurrentProp.PadPreset = null;
            }
            else
            {
                foreach (Pad IteratedPad in DataHolder.Pads)
                {
                    if (IteratedPad.NumberString == SelectedText[..4])
                    {
                        CurrentProp.PadPreset = IteratedPad;
                        break;
                    }
                }
            }
        }
        else if (DropdownName == "CharacterPreset")
        {
            if (SelectedIndex == 0)
            {
                CurrentProp.CharacterPreset = null;
            }
            else
            {
                foreach (Pad IteratedPad in DataHolder.Pads)
                {
                    if (IteratedPad.NumberString == SelectedText[..4])
                    {
                        CurrentProp.CharacterPreset = IteratedPad;
                        break;
                    }
                }
            }
        }
        else if (DropdownName == "Team")
        {
            CurrentProp.Team = DropdownOptions.GetConstant("TEAM", SelectedIndex);
        }
        else if (DropdownName == "Squadron")
        {
            CurrentProp.Squadron = DropdownOptions.GetConstant("SQUADRON", SelectedIndex);
        }
        else if (DropdownName == "Chair")
        {
            Debug.Log("Edit chair");
            CurrentProp.Chair = DropdownOptions.FindTag(SelectedIndex, true);
        }
        else if (DropdownName == "Effect")
        {
            CurrentProp.Effect = DropdownOptions.GetConstant("PADEFFECT", SelectedIndex);
        }
        else if (DropdownName == "LinkedGun")
        {
            if (SelectedIndex == 0)
            {
                CurrentProp.LinkedWeapon = null;
            }
            else
            {
                CurrentProp.LinkedWeapon = DataHolder.Props.Find(x => x.GetName() == SelectedText);
            }
        }
        else if (DropdownName == "HoverType")
        {
            CurrentProp.HoverType = DropdownOptions.GetConstant("HOVTYPE", SelectedIndex);
        }

        // *** SUBPROPS ***

        //        else if (DropdownName == "SubPropObjectType")
        /*else if (DropdownName == "SubPropWeaponType")
        {
            int SubPropNumber = int.Parse(EventDropdown.transform.parent.parent.name[^1..]); // Get the number of the sub prop from SubPropContent's name
            CurrentProp.SubProps[SubPropNumber].WeaponType = DropdownOptions.GetConstant("WEAPON", SelectedIndex);
        }
        else if (DropdownName == "SubPropLinkedGun")
        {
            int SubPropNumber = int.Parse(EventDropdown.transform.parent.parent.name[^1..]); // Get the number of the sub prop from SubPropContent's name
            Prop SubProp = CurrentProp.SubProps[SubPropNumber];
            if (SelectedIndex == 0)
            {
                SubProp.LinkedWeapon = null;
            }
            else
            {
                foreach (Pad IteratedPad in DataHolder.Pads)
                {
                    if (IteratedPad.PadProp != null && IteratedPad.PadProp.Type == PropType.Weapon && IteratedPad.NumberString == SelectedText[..4])
                    {
                        SubProp.LinkedWeapon = IteratedPad.PadProp;
                        break;
                    }
                }
            }
        }*/

        // *** RENAME OBJECT ***

        else if (DropdownName == "WeaponAppearance")
        {
            CurrentProp.RenameObject.WeaponType = DropdownOptions.GetConstant("WEAPON", SelectedIndex);
        }
        else if (DropdownName == "MenuListName")
        {
            CurrentProp.RenameObject.MenuListText = LanguageFileHolder.GetLevelLanguageFile().LanguageStrings[SelectedIndex];
        }
        else if (DropdownName == "PickupText")
        {
            CurrentProp.RenameObject.PickUpText = LanguageFileHolder.GetLevelLanguageFile().LanguageStrings[SelectedIndex];
        }

        // *** PORTALS ***

        else if (DropdownName == "FirstRoom")
        {
            foreach (Portal IteratedPortal in CurrentPortals)
            {
                IteratedPortal.FirstRoom = DataHolder.Rooms[SelectedIndex];
            }
        }
        else if (DropdownName == "SecondRoom")
        {
            foreach (Portal IteratedPortal in CurrentPortals)
            {
                IteratedPortal.SecondRoom = DataHolder.Rooms[SelectedIndex];
            }
        }

        // *** COLLISION ***

        else if (DropdownName == "FootstepSound")
        {
            CurrentTile.FloorType = SelectedIndex;
        }

        // *** BG LIGHTS ***

        else if (DropdownName == "Room")
        {
            foreach (BackgroundLight IteratedBackgroundLight in CurrentBackgroundLights)
            {
                IteratedBackgroundLight.LightRoom = DataHolder.Rooms[SelectedIndex];
            }
        }
    }

    public void OnToggleClicked(Toggle EventToggle, bool NewValue)
    {
        string FieldName = EventToggle.transform.parent.name;

        if (FieldName == "AIWaitsLift")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                if (IteratedPad.IsCoverPad)
                {
                    continue;
                }
                IteratedPad.AIWaitsForLift = NewValue;
            }
        }
        else if (FieldName == "AIRidesLift")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                if (IteratedPad.IsCoverPad)
                {
                    continue;
                }
                IteratedPad.AIRidesLift = NewValue;
            }
        }
        else if (FieldName == "AIWalksDirectly")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                if (IteratedPad.IsCoverPad)
                {
                    continue;
                }
                IteratedPad.AIWalksDirectlyTo = NewValue;
            }
        }
        else if (FieldName == "AIDropsDown")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                if (IteratedPad.IsCoverPad)
                {
                    continue;
                }
                IteratedPad.AIDrops = NewValue;
            }
        }
        else if (FieldName == "AICrouches")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                if (IteratedPad.IsCoverPad)
                {
                    continue;
                }
                IteratedPad.AICrouches = NewValue;
            }
        }
        else if (FieldName == "AIMovesWhileFalling")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                if (IteratedPad.IsCoverPad)
                {
                    continue;
                }
                IteratedPad.AIMovesWhileFalling = NewValue;
            }
        }
        else if (FieldName == "AIDucks")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                if (IteratedPad.IsCoverPad)
                {
                    continue;
                }
                IteratedPad.AIDucks = NewValue;
            }
        }
        else if (FieldName == "HasBoundingBoxData")
        {
            if (MultipleObjectsSelected)
            {
                if (NewValue == true)
                {
                    foreach (Pad IteratedPad in CurrentPads)
                    {
                        IteratedPad.SetBoundingBoxData(true);
                    }

                    Transform XSizeField = PadEditor.Find("XSize");
                    XSizeField.gameObject.SetActive(true);
                    XSizeField.Find("XMin").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");
                    XSizeField.Find("XMax").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");

                    Transform YSizeField = PadEditor.Find("YSize");
                    YSizeField.gameObject.SetActive(true);
                    YSizeField.Find("YMin").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");
                    YSizeField.Find("YMax").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");

                    Transform ZSizeField = PadEditor.Find("ZSize");
                    ZSizeField.gameObject.SetActive(true);
                    ZSizeField.Find("ZMin").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");
                    ZSizeField.Find("ZMax").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify("");
                }
                else
                {
                    foreach (Pad IteratedPad in CurrentPads)
                    {
                        IteratedPad.SetBoundingBoxData(false);
                    }

                    PadEditor.Find("XSize").gameObject.SetActive(false);
                    PadEditor.Find("YSize").gameObject.SetActive(false);
                    PadEditor.Find("ZSize").gameObject.SetActive(false);
                }
            }
            else
            {
                CurrentPad.SetBoundingBoxData(!CurrentPad.HasBoundingBoxData);

                if (CurrentPad.HasBoundingBoxData)
                {
                    Transform XSizeField = PadEditor.Find("XSize");
                    XSizeField.gameObject.SetActive(true);
                    XSizeField.Find("XMin").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentPad.XMin.ToString());
                    XSizeField.Find("XMax").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentPad.XMax.ToString());

                    Transform YSizeField = PadEditor.Find("YSize");
                    YSizeField.gameObject.SetActive(true);
                    YSizeField.Find("YMin").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentPad.YMin.ToString());
                    YSizeField.Find("YMax").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentPad.YMax.ToString());

                    Transform ZSizeField = PadEditor.Find("ZSize");
                    ZSizeField.gameObject.SetActive(true);
                    ZSizeField.Find("ZMin").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentPad.ZMin.ToString());
                    ZSizeField.Find("ZMax").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentPad.ZMax.ToString());
                }
                else
                {
                    PadEditor.Find("XSize").gameObject.SetActive(false);
                    PadEditor.Find("YSize").gameObject.SetActive(false);
                    PadEditor.Find("ZSize").gameObject.SetActive(false);
                }
            }
        }
        else if (FieldName == "MoveOutwardsOnly")
        {
            CurrentNeighbour.MoveOutwardsOnly = NewValue;
        }
        else if (FieldName == "MoveInwardsOnly")
        {
            CurrentNeighbour.MoveInwardsOnly = NewValue;
        }
        else if (FieldName == "FanOn")
        {
            CurrentProp.On = NewValue;
        }
        else if (EventToggle.name == "LiftStopToggle1")
        {
            int StopNumber = int.Parse(EventToggle.transform.parent.parent.name[^1..]);
            CurrentProp.LiftStops[StopNumber].StopNumber = 0;
        }
        else if (EventToggle.name == "LiftStopToggle2")
        {
            int StopNumber = int.Parse(EventToggle.transform.parent.parent.name[^1..]);
            CurrentProp.LiftStops[StopNumber].StopNumber = 1;
        }
        else if (EventToggle.name == "LiftStopToggle3")
        {
            int StopNumber = int.Parse(EventToggle.transform.parent.parent.name[^1..]);
            CurrentProp.LiftStops[StopNumber].StopNumber = 2;
        }
        else if (EventToggle.name == "LiftStopToggle4")
        {
            int StopNumber = int.Parse(EventToggle.transform.parent.parent.name[^1..]);
            CurrentProp.LiftStops[StopNumber].StopNumber = 3;
        }
        else if (FieldName == "Floor1")
        {
            CurrentTile.Floor1 = NewValue;
        }
        else if (FieldName == "Floor2")
        {
            CurrentTile.Floor2 = NewValue;
        }
        else if (FieldName == "Wall")
        {
            CurrentTile.Wall = NewValue;
        }
        else if (FieldName == "BlockSight")
        {
            CurrentTile.BlockSight = NewValue;
        }
        else if (FieldName == "BlockGunfire")
        {
            CurrentTile.BlockGunfire = NewValue;
        }
        else if (FieldName == "LiftFloor")
        {
            CurrentTile.LiftFloor = NewValue;
        }
        else if (FieldName == "Ladder")
        {
            CurrentTile.Ladder = NewValue;
        }
        else if (FieldName == "RampWall")
        {
            CurrentTile.RampWall = NewValue;
        }
        else if (FieldName == "Slope")
        {
            CurrentTile.Slope = NewValue;
        }
        else if (FieldName == "Underwater")
        {
            CurrentTile.Underwater = NewValue;
        }
        else if (FieldName == "Flag0400")
        {
            CurrentTile.Flag0400 = NewValue;
        }
        else if (FieldName == "AICrouchesTile")
        {
            CurrentTile.AICrouches = NewValue;
        }
        else if (FieldName == "AIDucksTile")
        {
            CurrentTile.AIDucks = NewValue;
        }
        else if (FieldName == "AlwaysAscend")
        {
            CurrentTile.AlwaysAscend = NewValue;
        }
        else if (FieldName == "DieOnTouch")
        {
            CurrentTile.DieOnTouch = NewValue;
        }
        else if (FieldName == "PlayerOnlyLadder")
        {
            CurrentTile.PlayerOnlyLadder = NewValue;
        }
    }

    public void OnTextFieldChanged(TMP_InputField EventField, string NewValue)
    {
        string FieldName = EventField.transform.parent.name;

        if (NewValue == "" || NewValue == "-")
        {
            return;
        }

        // *** PADS ***

        if (FieldName == "XPosition")
        {
            if (MultipleObjectsSelected)
            {
                Vector3 GizmoPosition = SelectionManagerInstance.Gizmo.transform.position;

                float Difference = float.Parse(NewValue) - GizmoPosition.x;

                foreach (Pad IteratedPad in CurrentPads)
                {
                    IteratedPad.ChangePosition(new Vector3(IteratedPad.Position.x + Difference, IteratedPad.Position.y, IteratedPad.Position.z));
                }

                GizmoPosition.x += Difference;
                SelectionManagerInstance.Gizmo.transform.position = GizmoPosition;
            }
            else
            {
                Vector3 NewPosition = new Vector3(float.Parse(NewValue), CurrentPad.Position.y, CurrentPad.Position.z);
                CurrentPad.ChangePosition(NewPosition);
                SelectionManagerInstance.Gizmo.transform.position = NewPosition;
            }
        }
        else if (FieldName == "YPosition")
        {
            if (MultipleObjectsSelected)
            {
                Vector3 GizmoPosition = SelectionManagerInstance.Gizmo.transform.position;

                float Difference = float.Parse(NewValue) - GizmoPosition.y;

                foreach (Pad IteratedPad in CurrentPads)
                {
                    IteratedPad.ChangePosition(new Vector3(IteratedPad.Position.x, IteratedPad.Position.y + Difference, IteratedPad.Position.z));
                }

                GizmoPosition.y += Difference;
                SelectionManagerInstance.Gizmo.transform.position = GizmoPosition;
            }
            else
            {
                Vector3 NewPosition = new Vector3(CurrentPad.Position.x, float.Parse(NewValue), CurrentPad.Position.z);
                CurrentPad.ChangePosition(NewPosition);
                SelectionManagerInstance.Gizmo.transform.position = NewPosition;
            }
        }
        else if (FieldName == "ZPosition")
        {
            if (MultipleObjectsSelected)
            {
                Vector3 GizmoPosition = SelectionManagerInstance.Gizmo.transform.position;

                float Difference = float.Parse(NewValue) - GizmoPosition.z;

                foreach (Pad IteratedPad in CurrentPads)
                {
                    IteratedPad.ChangePosition(new Vector3(IteratedPad.Position.x, IteratedPad.Position.y, IteratedPad.Position.z + Difference));
                }

                GizmoPosition.z += Difference;
                SelectionManagerInstance.Gizmo.transform.position = GizmoPosition;
            }
            else
            {
                Vector3 NewPosition = new Vector3(CurrentPad.Position.x, CurrentPad.Position.y, float.Parse(NewValue));
                CurrentPad.ChangePosition(NewPosition);
                SelectionManagerInstance.Gizmo.transform.position = NewPosition;
            }
        }
        else if (FieldName == "XDirection")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                IteratedPad.ChangeRotation(new Vector3(float.Parse(NewValue), IteratedPad.Direction.y, IteratedPad.Direction.z), IteratedPad.Up);
            }
        }
        else if (FieldName == "YDirection")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                IteratedPad.ChangeRotation(new Vector3(IteratedPad.Direction.x, float.Parse(NewValue), IteratedPad.Direction.z), IteratedPad.Up);
            }
        }
        else if (FieldName == "ZDirection")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                IteratedPad.ChangeRotation(new Vector3(IteratedPad.Direction.x, IteratedPad.Direction.y, float.Parse(NewValue)), IteratedPad.Up);
            }
        }
        else if (FieldName == "XUp")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                IteratedPad.ChangeRotation(IteratedPad.Direction, new Vector3(float.Parse(NewValue), IteratedPad.Up.y, IteratedPad.Up.z));
            }
        }
        else if (FieldName == "YUp")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                IteratedPad.ChangeRotation(IteratedPad.Direction, new Vector3(IteratedPad.Up.x, float.Parse(NewValue), IteratedPad.Up.z));
            }
        }
        else if (FieldName == "ZUp")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                IteratedPad.ChangeRotation(IteratedPad.Direction, new Vector3(IteratedPad.Up.x, IteratedPad.Up.y, float.Parse(NewValue)));
            }
        }
        else if (FieldName == "XMin")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                IteratedPad.ChangeScale(float.Parse(NewValue), CurrentPad.XMax, CurrentPad.YMin, CurrentPad.YMax, CurrentPad.ZMin, CurrentPad.ZMax);
            }
        }
        else if (FieldName == "XMax")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                IteratedPad.ChangeScale(CurrentPad.XMin, float.Parse(NewValue), CurrentPad.YMin, CurrentPad.YMax, CurrentPad.ZMin, CurrentPad.ZMax);
            }
        }
        else if (FieldName == "YMin")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                IteratedPad.ChangeScale(CurrentPad.XMin, CurrentPad.XMax, float.Parse(NewValue), CurrentPad.YMax, CurrentPad.ZMin, CurrentPad.ZMax);
            }
        }
        else if (FieldName == "YMax")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                IteratedPad.ChangeScale(CurrentPad.XMin, CurrentPad.XMax, CurrentPad.YMin, float.Parse(NewValue), CurrentPad.ZMin, CurrentPad.ZMax);
            }
        }
        else if (FieldName == "ZMin")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                IteratedPad.ChangeScale(CurrentPad.XMin, CurrentPad.XMax, CurrentPad.YMin, CurrentPad.YMax, float.Parse(NewValue), CurrentPad.ZMax);
            }
        }
        else if (FieldName == "ZMax")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                IteratedPad.ChangeScale(CurrentPad.XMin, CurrentPad.XMax, CurrentPad.YMin, CurrentPad.YMax, CurrentPad.ZMin, float.Parse(NewValue));
            }
        }
        else if (FieldName == "LiftNumber")
        {
            foreach (Pad IteratedPad in CurrentPads)
            {
                IteratedPad.LiftNumber = int.Parse(NewValue);
            }
        }

        // *** PROPS ***

        else if (FieldName == "Scale")
        {
            CurrentPad.ChangeScale((int)(float.Parse(NewValue) * 2.56f));
        }
        else if (FieldName == "MaxDamage")
        {
            CurrentProp.MaxDamage = int.Parse(NewValue);
        }
        else if (FieldName == "MaxFrac")
        {
            CurrentProp.MaxFrac = float.Parse(NewValue);
        }
        else if (FieldName == "PerimFrac")
        {
            CurrentProp.PerimFrac = float.Parse(NewValue);
        }
        else if (FieldName == "Acceleration")
        {
            CurrentProp.Acceleration = float.Parse(NewValue);
        }
        else if (FieldName == "Deceleration")
        {
            CurrentProp.Deceleration = float.Parse(NewValue);
        }
        else if (FieldName == "MaxSpeed")
        {
            CurrentProp.MaxSpeed = float.Parse(NewValue);
        }
        else if (FieldName == "AutoCloseTime")
        {
            CurrentProp.AutoCloseTime = int.Parse(NewValue);
        }
        else if (FieldName == "Yaw")
        {
            CurrentProp.Yaw = float.Parse(NewValue);
        }
        else if (FieldName == "YawMaxSpeed")
        {
            CurrentProp.YawMaxSpeed = float.Parse(NewValue);
        }
        else if (FieldName == "MaxDistance")
        {
            CurrentProp.MaxDistance = float.Parse(NewValue);
        }
        else if (FieldName == "CharacterID")
        {
            CurrentProp.CharacterID = int.Parse(NewValue);
        }
        else if (FieldName == "HearScale")
        {
            CurrentProp.HearScale = int.Parse(NewValue);
        }
        else if (FieldName == "ViewDistance")
        {
            CurrentProp.ViewDistance = int.Parse(NewValue);
        }
        else if (FieldName == "ConvTalk")
        {
            CurrentProp.ConvTalk = int.Parse(NewValue);
        }
        else if (FieldName == "Tude")
        {
            CurrentProp.Tude = int.Parse(NewValue);
        }
        else if (FieldName == "NaturalAnimation")
        {
            CurrentProp.NaturalAnimation = int.Parse(NewValue);
        }
        else if (FieldName == "YVisionAngle")
        {
            CurrentProp.YVisAngle = int.Parse(NewValue);
        }
        else if (FieldName == "TeamScanDistance")
        {
            CurrentProp.TeamScanDistance = int.Parse(NewValue);
        }
        else if (FieldName == "XOffset")
        {
            CurrentProp.XOffset = float.Parse(NewValue);
        }
        else if (FieldName == "YOffset")
        {
            CurrentProp.YOffset = float.Parse(NewValue);
        }
        else if (FieldName == "ZOffset")
        {
            CurrentProp.ZOffset = float.Parse(NewValue);
        }
        else if (FieldName == "LateralRotation")
        {
            CurrentProp.LateralRotation = int.Parse(NewValue);
        }
        else if (FieldName == "VerticalRotation")
        {
            CurrentProp.VerticalRotation = int.Parse(NewValue);
        }

        // *** SUBPROPS ***

        else if (FieldName == "SubPropScale")
        {
            int SubPropNumber = int.Parse(EventField.transform.parent.parent.name[^1..]); // Get the number of the sub prop from SubPropContent's name
            CurrentProp.SubProps[SubPropNumber].Scale = (int)(float.Parse(NewValue) * 2.56f);
        }

        // *** ROOMS **

        else if (FieldName == "MinimumBrightness")
        {
            int NumberValue = int.Parse(NewValue);
            if (NumberValue < 0 || NumberValue > 255)
            {
                EventField.text = Mathf.Clamp(NumberValue, 0, 255).ToString();
                return;
            }

            foreach (Room IteratedRoom in CurrentRooms)
            {
                IteratedRoom.MinimumBrightness = NumberValue;
            }

        }
        else if (FieldName == "MaximumBrightness")
        {
            int NumberValue = int.Parse(NewValue);
            if (NumberValue < 0 || NumberValue > 255)
            {
                EventField.text = Mathf.Clamp(NumberValue, 0, 255).ToString();
                return;
            }

            foreach(Room IteratedRoom in CurrentRooms)
            {
                IteratedRoom.MaximumBrightness = NumberValue;
            }
        }

        // *** BG LIGHTS **

        else if (FieldName == "XNormal")
        {
            foreach (BackgroundLight IteratedBackgroundLight in CurrentBackgroundLights)
            {
                IteratedBackgroundLight.ChangeNormal(new Vector3(float.Parse(NewValue), IteratedBackgroundLight.Direction.y, IteratedBackgroundLight.Direction.z));
            }
        }
        else if (FieldName == "YNormal")
        {
            foreach (BackgroundLight IteratedBackgroundLight in CurrentBackgroundLights)
            {
                IteratedBackgroundLight.ChangeNormal(new Vector3(IteratedBackgroundLight.Direction.x, float.Parse(NewValue), IteratedBackgroundLight.Direction.z));
            }
        }
        else if (FieldName == "ZNormal")
        {
            foreach (BackgroundLight IteratedBackgroundLight in CurrentBackgroundLights)
            {
                IteratedBackgroundLight.ChangeNormal(new Vector3(IteratedBackgroundLight.Direction.x, IteratedBackgroundLight.Direction.y, float.Parse(NewValue)));
            }
        }
        else if (FieldName == "Size")
        {
            foreach (BackgroundLight IteratedBackgroundLight in CurrentBackgroundLights)
            {
                IteratedBackgroundLight.Size = int.Parse(NewValue);
            }
        }
        else if (FieldName == "RingSize")
        {
            foreach (BackgroundLight IteratedBackgroundLight in CurrentBackgroundLights)
            {
                IteratedBackgroundLight.RingSize = int.Parse(NewValue);
            }
        }
    }

    public void OnLiftDoorSelected(int ID, int SelectedIndex, string SelectedText)
    {
        if (SelectedIndex == 0)
        {
            CurrentProp.LiftStops[ID].LiftStopDoor = null;
        }
        else
        {
            foreach (Pad IteratedPad in DataHolder.Pads)
            {
                if (IteratedPad.PadProp != null && IteratedPad.PadProp.Type == PropType.Door && IteratedPad.NumberString == SelectedText[..4])
                {
                    CurrentProp.LiftStops[ID].LiftStopDoor = IteratedPad.PadProp;
                    break;
                }
            }
        }
    }

    public void OnWaypointNeighbourSelected(ListItem SelectedItem)
    {
        CurrentNeighbour = CurrentWaypoint.Neighbours[SelectedItem.ID];

        GameObject MoveOutwardsToggle = PadEditor.Find("MoveOutwardsOnly").gameObject;
        GameObject MoveInwardsToggle = PadEditor.Find("MoveInwardsOnly").gameObject;

        if (MoveOutwardsToggle.activeSelf == false && MoveInwardsToggle.activeSelf == false)
        {
            MoveInwardsToggle.SetActive(true);
            MoveOutwardsToggle.SetActive(true);
        }

        MoveOutwardsToggle.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentNeighbour.MoveOutwardsOnly);
        MoveInwardsToggle.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(CurrentNeighbour.MoveInwardsOnly);
    }

    public void LinkWaypoint(Pad InPad)
    {
        Waypoint InWaypoint = InPad.PadWaypoint;

        if (InWaypoint == CurrentWaypoint || CurrentWaypoint.Neighbours.Find(x => x.LinkedWaypoint == InWaypoint) != null)
        {
            PadEditor.Find("WaypointLinkText").gameObject.SetActive(false);
            return;
        }
        
        CurrentWaypoint.Neighbours.Add(new WaypointNeighbour(InWaypoint));
        InWaypoint.Neighbours.Add(new WaypointNeighbour(CurrentWaypoint));

        PadEditor.Find("NeighbourList").GetComponent<ScrollableList>().AddItemAtEnd(InPad.NumberString, false);

        PadEditor.Find("WaypointLinkText").gameObject.SetActive(false);

        //StartCoroutine(EnableSelectionHandle());
    }

    public void UpdatePositionFields()
    {
        Transform PositionField = PadEditor.Find("Position");
        if (EditModeBar.CurrentMode == EditMode.Objects)
        {
            PositionField.Find("XPosition").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentPad.Position.x.ToString());
            PositionField.Find("YPosition").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentPad.Position.y.ToString());
            PositionField.Find("ZPosition").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentPad.Position.z.ToString());
        }
        else if (EditModeBar.CurrentMode == EditMode.Rooms)
        {
            PositionField.Find("XPosition").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentRoom.transform.position.x.ToString());
            PositionField.Find("YPosition").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentRoom.transform.position.y.ToString());
            PositionField.Find("ZPosition").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentRoom.transform.position.z.ToString());
        }
        else if (EditModeBar.CurrentMode == EditMode.Portals)
        {
            PositionField.Find("XPosition").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentPortal.transform.position.x.ToString());
            PositionField.Find("YPosition").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentPortal.transform.position.y.ToString());
            PositionField.Find("ZPosition").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentPortal.transform.position.z.ToString());
        }
        else if (EditModeBar.CurrentMode == EditMode.BackgroundLights)
        {
            PositionField.Find("XPosition").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentBackgroundLight.transform.position.x.ToString());
            PositionField.Find("YPosition").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentBackgroundLight.transform.position.y.ToString());
            PositionField.Find("ZPosition").GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(CurrentBackgroundLight.transform.position.z.ToString());
        }
    }
}
