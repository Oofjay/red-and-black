using Assimp;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;
using Material = UnityEngine.Material;
using Matrix4x4 = Assimp.Matrix4x4;
using Mesh = UnityEngine.Mesh;
using Quaternion = UnityEngine.Quaternion;

// The setup files work with some props referencing others (like a door referencing its twin that opens at the same time).
// The prop uses an offset to the other prop in the list (i.e. if "linked door" is 1, that means the linked door is the next prop).
// This object holds all the necessary offsets when the file is read, then at the end it reads through all the offset objects
// and assigns all the props their offset props by reading all the offsets and converting that into Prop objects.
public class PropOffsetHolder
{
    public int OffsetPropID;

    public int ParentPropOffset;

    // Door
    public int LinkedDoorOffset;

    // Lift
    public int LiftDoor1Offset;
    public int LiftDoor2Offset;
    public int LiftDoor3Offset;
    public int LiftDoor4Offset;

    // Link gun object
    public int LinkedGunOffset1;
    public int LinkedGunOffset2;

    // Lift door stop
    public int LiftStopDoorOffset;
    public int LiftStopLiftOffset;

    // Tag
    public int TaggedPropOffset;

    // Conditional scenery
    public int TriggerPropOffset;
    public int UnexplodedPropOffset;
    public int ExplodedPropOffset;

    // Blocked path
    public int PathBlockingPropOffset;

    // Rename object
    public int RenameObjectOffset;

    // Character
    public int ChairTagID;

    // Character and hover vehicle
    public int AIListID;
}

public enum PropType
{
    // Visible in regular prop dropdowns
    Door,
    StandardObject,
    Key,
    Camera,
    AmmoCrate,
    Weapon,
    SingleMonitor,
    MultiMonitor,
    Autogun,
    Debris,
    Hat,
    AmmoCrateMulti,
    Shield,
    Glass,
    Camera2,
    TintedGlass,
    Lift,
    ConditionalScenery,
    BlockedPath,
    Hoverbike,
    Hoverprop,
    Fan,
    Hovercar,
    Effect,
    Hovercopter,
    Mine,
    EscalatorStep,

    // In character dropdowns only
    Character,

    // In tag dropdowns only
    Tag,

    // Invisible in dropdowns, obscured
    DoorScale,
    LinkGuns,
    LiftDoor,
    Rename,

    // Beta - unused?
    GasBottle,
    PadlockedDoor,
    Safe,
    SafeItem,

    // in CI training only, normally placed in objectives
    TrainingHolographRequirement
}

public class Prop
{
    public string DisplayName; // Not used by the game, just for editor display only
    public int InternalID { get; set; } // This is not used by the game, but makes it easier to keep track of each prop.
    public ModelData Data { get; set; }
    public ModelData HeadData { get; set; } // Characters only

    public bool IsAbstract; // Is it a dynamic marker like player character, target character, character last shot, etc.

    public PropType Type;
    public int Scale;
    public PropModel Model { get; set; }
    public Pad ParentPad;
    public BitFlagCollection Flags1 = new();
    public BitFlagCollection Flags2 = new();
    public BitFlagCollection Flags3 = new();
    public int MaxDamage;

    public bool IsGlobal;
    public bool IsAttachedToCharacter;
    public bool IsAttachedToOtherProp;
    public Prop ParentProp; // For embedded props and character subprops only
    public List<Prop> SubProps = new(); // Embedded props like weapons, hats and things that drop when prop is destroyed
    public Prop AttachedTag; // The tag that refers to this prop
    public int ParentPropID;

    public Prop RenameObject;

    // Door only
    public float MaxFrac;
    public float PerimFrac;
    public float Deceleration;
    public BitFlagCollection DoorFlags = new();
    public Constant DoorType;
    public int AutoCloseTime;
    public int unk88;
    public Prop LinkedDoor;
    public int unkc4;

    // Door & key only
    public BitFlagCollection LockBits = new();

    // Character & hovercar only
    public AIList AIList;

    // Camera only
    public float Yaw;
    public float YawMaxSpeed;
    public float MaxDistance;

    // Camera & autogun only
    public Pad TargetPad;

    // Autogun only
    public int unk64;
    public int unk68;
    public int unk80;

    // Autogun & lift only
    public int unk84;

    // Lift only
    public Pad LiftPad1;
    public Pad LiftPad2;
    public Pad LiftPad3;
    public Pad LiftPad4;
    public Prop LiftDoor1;
    public Prop LiftDoor2;
    public Prop LiftDoor3;
    public Prop LiftDoor4;
    public List<Prop> LiftStops = new();

    // Lift, fan & door only
    public float Acceleration;
    public float MaxSpeed;

    // Fan only
    public bool On;

    // Ammo crate only
    public Constant AmmoType;

    // Single monitor only
    public Constant Image;

    // Multimonitor only
    public Constant Image1;
    public Constant Image2;
    public Constant Image3;
    public Constant Image4;

    // Hover prop only
    public Constant HoverType;

    // Tinted glass only
    public int unk5c;

    // Mine, weapon and rename object only
    public Constant WeaponType;

    // Weapon only
    public Prop LinkedWeapon;

    // Character only
    public BitFlagCollection SpawnFlags = new();
    public int CharacterID;
    public PropModel Body;
    public PropModel Head;
    public Pad PadPreset;
    public Pad CharacterPreset;
    public int HearScale;
    public int ViewDistance;
    public Constant Team; // Teams are stored as a constant in .c files in the decomp
    public BitFlagCollection TeamFlags = new(); // Teams are stored as bitflags in binary setup files
    public Constant Squadron;
    public Prop Chair;
    public int ConvTalk; // ??
    public int Tude; // ??
    public int NaturalAnimation;
    public int YVisAngle; // ??
    public int TeamScanDistance; // ??

    // Pad effect only
    public Constant Effect;

    // Camera pad only
    public float XOffset;
    public float YOffset;
    public float ZOffset;
    public float LateralRotation;
    public float VerticalRotation;

    // Tag only
    public int TagID;
    public Prop TaggedProp; //  The prop that is referred to by this tag
    public Prop ConnectedCharacter;

    // Lift door only
    public Prop LiftStopLift;
    public Prop LiftStopDoor;
    public int StopNumber;

    // Conditional scenery only
    public Prop TriggerProp;
    public Prop UnexplodedProp;
    public Prop ExplodedProp;

    // Blocked path only
    public Prop PathBlockingProp;
    public Waypoint Waypoint1;
    public Waypoint Waypoint2;

    // Rename object only
    public LanguageString OtherText1; // currently unused
    public LanguageString OtherText2; // currently unused
    public LanguageString MenuListText;
    public LanguageString OtherText3; // currently unused
    public LanguageString PickUpText;

    // Gun linker only
    public Prop LinkedWeapon1;
    public Prop LinkedWeapon2;

    public List<ObjectiveCondition> ConnectedObjectiveConditions { get; set; } = new();
    public List<IntroSetting> ConnectedIntroSettings { get; set; } = new();

    public Prop()
    {

    }

    public Prop(Prop InProp)
    {
        DisplayName = InProp.DisplayName;
        Data = InProp.Data;
        HeadData = InProp.HeadData;

        Type = InProp.Type;
        Scale = InProp.Scale;
        Model = InProp.Model;
        Flags1.AddRange(InProp.Flags1);
        Flags2.AddRange(InProp.Flags2);
        Flags3.AddRange(InProp.Flags3);
        MaxDamage = InProp.MaxDamage;

        foreach (Prop CurrentSubProp in InProp.SubProps)
        {
            Prop NewSubProp = new Prop(CurrentSubProp) { ParentProp = this };
            SubProps.Add(NewSubProp);

            if (Type == PropType.Character)
            {
                NewSubProp.IsAttachedToCharacter = true;
            }
            else
            {
                NewSubProp.IsAttachedToOtherProp = true;
            }
        }

        RenameObject = InProp.RenameObject != null ? new Prop(InProp.RenameObject) : null;

        MaxFrac = InProp.MaxFrac;
        PerimFrac = InProp.PerimFrac;
        Deceleration = InProp.Deceleration;
        DoorFlags.AddRange(InProp.DoorFlags);
        DoorType = InProp.DoorType;
        AutoCloseTime = InProp.AutoCloseTime;
        unk88 = InProp.unk88;
        LinkedDoor = InProp.LinkedDoor;
        unkc4 = InProp.unkc4;

        LockBits.AddRange(InProp.LockBits);

        AIList = InProp.AIList;

        Yaw = InProp.Yaw;
        YawMaxSpeed = InProp.YawMaxSpeed;
        MaxDistance = InProp.MaxDistance;

        TargetPad = InProp.TargetPad;

        unk64 = InProp.unk64;
        unk68 = InProp.unk68;
        unk80 = InProp.unk80;

        unk84 = InProp.unk84;

        LiftPad1 = InProp.LiftPad1;
        LiftPad2 = InProp.LiftPad2;
        LiftPad3 = InProp.LiftPad3;
        LiftPad4 = InProp.LiftPad4;
        LiftDoor1 = InProp.LiftDoor1;
        LiftDoor2 = InProp.LiftDoor2;
        LiftDoor3 = InProp.LiftDoor3;
        LiftDoor4 = InProp.LiftDoor4;
        LiftStops.AddRange(InProp.LiftStops);

        Acceleration = InProp.Acceleration;
        MaxSpeed = InProp.MaxSpeed;

        On = InProp.On;

        AmmoType = InProp.AmmoType;

        Image = InProp.Image;

        Image1 = InProp.Image1;
        Image2 = InProp.Image2;
        Image3 = InProp.Image3;
        Image4 = InProp.Image4;

        HoverType = InProp.HoverType;

        unk5c = InProp.unk5c;

        WeaponType = InProp.WeaponType;

        LinkedWeapon = InProp.LinkedWeapon;

        SpawnFlags.AddRange(InProp.SpawnFlags);

        IEnumerable<int> CharacterIDs = DataHolder.Characters.Select(x => x.CharacterID);
        CharacterID = Enumerable.Range(0, int.MaxValue)
            .Except(CharacterIDs)
            .FirstOrDefault();

        Body = InProp.Body;
        Head = InProp.Head;
        PadPreset = InProp.PadPreset;
        CharacterPreset = InProp.CharacterPreset;
        HearScale = InProp.HearScale;
        ViewDistance = InProp.ViewDistance;
        Team = InProp.Team; // Teams are stored as a constant in .c files in the decomp
        TeamFlags.AddRange(TeamFlags); // Teams are stored as bitflags in binary setup files
        Squadron = InProp.Squadron;
        Chair = InProp.Chair;
        ConvTalk = InProp.ConvTalk;
        Tude = InProp.Tude;
        NaturalAnimation = InProp.NaturalAnimation;
        YVisAngle = InProp.YVisAngle;
        TeamScanDistance = InProp.TeamScanDistance;

        Effect = InProp.Effect;

        XOffset = InProp.XOffset;
        YOffset = InProp.YOffset;
        ZOffset = InProp.ZOffset;
        LateralRotation = InProp.LateralRotation;
        VerticalRotation = InProp.VerticalRotation;

        /*LiftStopLift;
        LiftStopDoor;
        StopNumber;

        TriggerProp;
        UnexplodedProp;
        ExplodedProp;

        PathBlockingProp;
        Waypoint1;
        Waypoint2;

        OtherText1; // currently unused
        OtherText2; // currently unused
        MenuListText;
        OtherText3; // currently unused
        PickUpText;

        LinkedWeapon1;
        LinkedWeapon2;*/
    }

    public string GetName()
    {
        if (Type == PropType.Character)
        {
            if (CharacterID >= 0xF1) // static presets like player character, target character
            {
                return DisplayName;
            }
            else
            {
                return $"{CharacterID:X2} {DisplayName ?? Data?.FileName ?? Type.ToString()}";
            }
        }
        else if (IsAttachedToCharacter || IsAttachedToOtherProp)
        {
            return $"{ParentProp?.ParentPad.NumberString} {DisplayName ?? Data?.FileName ?? Type.ToString()}";
        }
        else if (IsGlobal)
        {
            return $"---- {DisplayName ?? Data?.FileName ?? Type.ToString()}";
        }
        else
        {
            return $"{ParentPad?.NumberString} {DisplayName ?? Data?.FileName ?? Type.ToString()}";
        }
    }

    public void ChangePropType(PropType InType)
    {
        if (Type == InType)
        {
            return;
        }

        PropType OldType = Type;
        Type = InType;

        // If we're transforming this from or into a door, the position, rotation and scaling need to be recalculated
        if (ParentPad != null && (OldType == PropType.Door || Type == PropType.Door || OldType == PropType.Character || Type == PropType.Character))
        {
            ParentPad.ChangePosition(ParentPad.Position);
            ParentPad.ChangeRotation(ParentPad.Direction, ParentPad.Up);
            if (ParentPad.HasBoundingBoxData)
            {
                ParentPad.ChangeScale(ParentPad.XMin, ParentPad.XMax, ParentPad.YMin, ParentPad.YMax, ParentPad.ZMin, ParentPad.ZMax);
            }
            else
            {
                ParentPad.ChangeScale(Scale);
            }
        }
    }

    public void AddFlags(BitFlag InBitFlag)
    {
        string FlagType = InBitFlag.Constant[..InBitFlag.Constant.IndexOf('_')];

        switch (FlagType)
        {
            case "OBJFLAG":
            case "CHRFLAG0":
                Flags1.Add(InBitFlag);
                break;
            case "OBJFLAG2":
            case "CHRFLAG1":
                Flags2.Add(InBitFlag);
                break;
            case "OBJFLAG3":
                Flags3.Add(InBitFlag);
                break;
            case "SPAWNFLAG":
                SpawnFlags.Add(InBitFlag);
                break;
            case "DOORFLAG":
                DoorFlags.Add(InBitFlag);
                break;
            case "LOCKFLAG":
                LockBits.Add(InBitFlag);
                InBitFlag.ConnectedProps.Add(this);
                break;
            default:
                Debug.LogError("Invalid bit flag type in AddFlags");
                break;
        }
    }

    public void RemoveFlags(BitFlag InBitFlag)
    {
        string FlagType = InBitFlag.Constant[..InBitFlag.Constant.IndexOf('_')];

        switch (FlagType)
        {
            case "OBJFLAG":
            case "CHRFLAG0":
                Flags1.Remove(InBitFlag);
                break;
            case "OBJFLAG2":
            case "CHRFLAG1":
                Flags2.Remove(InBitFlag);
                break;
            case "OBJFLAG3":
                Flags3.Remove(InBitFlag);
                break;
            case "SPAWNFLAG":
                SpawnFlags.Remove(InBitFlag);
                break;
            case "DOORFLAG":
                DoorFlags.Remove(InBitFlag);
                break;
            case "LOCKFLAG":
                LockBits.Remove(InBitFlag);
                InBitFlag.ConnectedProps.Remove(this);
                break;
            default:
                Debug.LogError("Invalid bit flag type in RemoveFlags");
                break;
        }
    }

    public BitFlagCollection GetFlagList(string FlagType)
    {
        switch (FlagType)
        {
            case "OBJFLAG":
            case "CHRFLAG0":
                return Flags1;
            case "OBJFLAG2":
            case "CHRFLAG1":
                return Flags2;
            case "OBJFLAG3":
                return Flags3;
            case "SPAWNFLAG":
                return SpawnFlags;
            case "DOORFLAG":
                return DoorFlags;
            case "LOCKFLAG":
                return LockBits;
            default:
                Debug.LogError("Invalid bit flag type in GetFlagList");
                return null;
        }
    }
}

public class WaypointNeighbour
{
    public WaypointNeighbour() { }

    public WaypointNeighbour(Waypoint Neighbour)
    {
        LinkedWaypoint = Neighbour;
    }

    public WaypointNeighbour(WaypointNeighbourProperties Properties)
    {
        NeighbourID = int.Parse(Properties.waypoint[^4..], System.Globalization.NumberStyles.AllowHexSpecifier);
        MoveOutwardsOnly = Properties.flag4000;
        MoveInwardsOnly = Properties.flag8000;
    }

    public Waypoint LinkedWaypoint;
    public string NumberString => LinkedWaypoint.ParentPad.NumberString;
    public bool MoveOutwardsOnly;
    public bool MoveInwardsOnly;

    // This temporarily holds the ID as a number until the proper neighbour pointer gets assigned.
    public int NeighbourID;
}

public class WaygroupNeighbour
{
    public WaygroupNeighbour() { }

    public WaygroupNeighbour(WaygroupNeighbourProperties Properties)
    {
        NeighbourID = int.Parse(Properties.waygroup[^4..], System.Globalization.NumberStyles.AllowHexSpecifier);
        MoveOutwardsOnly = Properties.flag4000;
        MoveInwardsOnly = Properties.flag8000;
    }

    public Waygroup LinkedWaygroup;
    public string NumberString => LinkedWaygroup.ID.ToString("X4");
    public bool MoveOutwardsOnly;
    public bool MoveInwardsOnly;

    // This temporarily holds the ID as a number until the proper neighbour pointer gets assigned.
    public int NeighbourID;
}

public class Waygroup
{
    public Waygroup() { }

    public Waygroup(WaygroupProperties Properties)
    {
        ID = int.Parse(Properties.id[^4..], System.Globalization.NumberStyles.AllowHexSpecifier);

        foreach (WaygroupNeighbourProperties CurrentNeighbour in Properties.neighbours)
        {
            Neighbours.Add(new WaygroupNeighbour(CurrentNeighbour));
        }
    }

    public int ID { get; set; }
    public List<WaygroupNeighbour> Neighbours { get; set; } = new();
    public List<Waypoint> Waypoints { get; set; } = new();
}

public class Waypoint
{
    public Waypoint() { }

    public Waypoint(WaypointProperties Properties)
    {
        ID = int.Parse(Properties.id[^4..], System.Globalization.NumberStyles.AllowHexSpecifier);
        WaygroupID = int.Parse(Properties.waygroup[^4..], System.Globalization.NumberStyles.AllowHexSpecifier);

        //Debug.Log(ID.ToString("X") + " " + WaygroupID.ToString("X") + " " + DataHolder.Waygroups.Count.ToString("X"));

        foreach (WaypointNeighbourProperties CurrentNeighbour in Properties.neighbours)
        {
            Neighbours.Add(new WaypointNeighbour(CurrentNeighbour));
        }
    }

    public void DeleteNeighbour(WaypointNeighbour InNeighbour)
    {
        InNeighbour.LinkedWaypoint.Neighbours.RemoveAll(x => x.LinkedWaypoint == this);
        Neighbours.Remove(InNeighbour);
    }

    public int ID { get; set; }
    public Pad ParentPad;
    public List<WaypointNeighbour> Neighbours { get; set; } = new();
    public Waygroup ParentWaygroup { get; set; }

    // These temporarily hold the ID's as a number until the proper pad and waygroup pointers get assigned.
    public int WaygroupID { get; set; }
    public int ParentPadID { get; set; }
}

public class Pad : MonoBehaviour
{
    public int Number { get; set; } // The pad's number identifier. Example: while "name" is PAD_EAR_0001, "Number" is 0001 (decimal).
    public string NumberString => Number.ToString("X4"); // Same as Number, but converted to hexadecimal and string.

    public bool IsCoverPad;

    public Vector3 Position { get; set; }
    public Vector3 Direction { get; set; }
    public Vector3 Up { get; set; }
    public Vector3 Normal { get; set; }
    public float XMin;
    public float XMax;
    public float YMin;
    public float YMax;
    public float ZMin;
    public float ZMax;

    public bool IntegerPosition;

    public bool DirectionAlignToX;
    public bool DirectionAlignToY;
    public bool DirectionAlignToZ;
    public bool DirectionAlignInverted;
    public bool UpAlignToX;
    public bool UpAlignToY;
    public bool UpAlignToZ;
    public bool UpAlignInverted;

    public bool HasBoundingBoxData;

    public bool AIWaitsForLift;
    public bool AIRidesLift;
    public bool AIWalksDirectlyTo;
    public bool AIDrops;
    public bool AICrouches;
    public bool AIMovesWhileFalling;
    public bool AIDucks;

    public int LiftNumber;

    public int CoverFlags;

    public Prop PadProp;
    public Waypoint PadWaypoint;

    public List<IntroSetting> ConnectedIntroSettings = new();
    public List<GuardPath> ConnectedPaths = new();

    public Mesh PadMesh;

    public void Init(PadProperties Properties)
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;

        Number = int.Parse(Properties.id[^4..], System.Globalization.NumberStyles.HexNumber);
        name = "Pad" + NumberString; // Unity's GameObject display name

        Position = new Vector3() { x = Properties.pos[0], y = Properties.pos[1], z = -Properties.pos[2] };
        Direction = new Vector3() { x = Properties.dir[0], y = Properties.dir[1], z = -Properties.dir[2] };
        Up = new Vector3() { x = Properties.up[0], y = Properties.up[1], z = -Properties.up[2] };

        RecalculateNormal();

        AIWaitsForLift = Properties.aiwaitlift;
        AIRidesLift = Properties.aionlift;
        AIWalksDirectlyTo = Properties.aiwalkdirect;
        AIDrops = Properties.aidrop;
        AICrouches = Properties.aicrouch;
        AIMovesWhileFalling = Properties.aiignorey;
        AIDucks = Properties.aiduck;
        LiftNumber = Properties.liftnum;

        UpAlignToX = Math.Abs(Up.x) == 1.0f && Up.y == 0.0f && Up.z == 0.0f;
        UpAlignToY = Up.x == 0.0f && Math.Abs(Up.y) == 1.0f && Up.z == 0.0f;
        UpAlignToZ = Up.x == 0.0f && Up.y == 0.0f && Math.Abs(Up.z) == 1.0f;
        UpAlignInverted = (UpAlignToX || UpAlignToY || UpAlignToZ) && (Up.x == -1.0f || Up.y == -1.0f || Up.z == 1.0f);
        DirectionAlignToX = Math.Abs(Direction.x) == 1.0f && Direction.y == 0.0f && Direction.z == 0.0f;
        DirectionAlignToY = Direction.x == 0.0f && Math.Abs(Direction.y) == 1.0f && Direction.z == 0.0f;
        DirectionAlignToZ = Direction.x == 0.0f && Direction.y == 0.0f && Math.Abs(Direction.z) == 1.0f;
        DirectionAlignInverted = (DirectionAlignToX || DirectionAlignToY || DirectionAlignToZ) && (Direction.x == -1.0f || Direction.y == -1.0f || Direction.z == 1.0f);

        HasBoundingBoxData = Properties.xmin != -100 || Properties.xmax != 100 || Properties.ymin != -100 || Properties.ymax != 100 || Properties.zmin != -100 || Properties.zmax != 100;

        ChangeScale(Properties.xmin, Properties.xmax, Properties.ymin, Properties.ymax, Properties.zmin, Properties.zmax);
    }

    public virtual void Init(PadData Properties)
    {
        if (!Properties.IsCoverPad)
        {
            GetComponent<MeshRenderer>().material.color = Color.blue;

            IsCoverPad = false;

            Number = Properties.Number;
            name = "Pad" + NumberString; // Unity's GameObject display name

            Position = Properties.Position;
            Direction = Properties.Direction;
            Up = Properties.Up;
            Normal = Properties.Normal;

            AIWaitsForLift = Properties.AIWaitsForLift;
            AIRidesLift = Properties.AIRidesLift;
            AIWalksDirectlyTo = Properties.AIWalksDirectlyTo;
            AIDrops = Properties.AIDrops;
            AICrouches = Properties.AICrouches;
            AIMovesWhileFalling = Properties.AIMovesWhileFalling;
            AIDucks = Properties.AIDucks;
            LiftNumber = Properties.LiftNumber;

            UpAlignToX = Properties.UpAlignToX;
            UpAlignToY = Properties.UpAlignToY;
            UpAlignToZ = Properties.UpAlignToZ;
            UpAlignInverted = Properties.UpAlignInverted;
            DirectionAlignToX = Properties.DirectionAlignToX;
            DirectionAlignToY = Properties.DirectionAlignToY;
            DirectionAlignToZ = Properties.DirectionAlignToZ;
            DirectionAlignInverted = Properties.DirectionAlignInverted;

            HasBoundingBoxData = Properties.HasBoundingBoxData;

            ChangeScale(Properties.XMin, Properties.XMax, Properties.YMin, Properties.YMax, Properties.ZMin, Properties.ZMax);

            if (Properties.PadWaypoint != null)
            {
                AddWaypoint(Properties.PadWaypoint);
            }
        }
        else
        {
            GetComponent<MeshRenderer>().material.color = Color.red;

            IsCoverPad = true;

            Number = Properties.Number;
            name = "CoverPad" + NumberString; // Unity's GameObject display name

            Position = Properties.Position;
            Direction = Properties.Direction;

            CoverFlags = Properties.CoverFlags;
        }

    }

    // This function should be called when the user creates a new pad via a menu.
    public virtual void InitNew()
    {
        if (!IsCoverPad)
        {
            GetComponent<MeshRenderer>().material.color = Color.blue;

            Number = DataHolder.Pads.Count;
            name = "Pad" + NumberString;

            Position = transform.position;
            Direction = new Vector3(0, 0, 1);
            Up = new Vector3(0, 1, 0);

            RecalculateNormal();

            ChangeScale(-100, 100, -100, 100, -100, 100);

            AIWaitsForLift = false;
            AIRidesLift = false;
            AIWalksDirectlyTo = false;
            AIDrops = false;
            AICrouches = false;
            AIMovesWhileFalling = false;
            AIDucks = false;
            LiftNumber = 0;

            UpAlignToX = Math.Abs(Up.x) == 1.0f && Up.y == 0.0f && Up.z == 0.0f;
            UpAlignToY = Up.x == 0.0f && Math.Abs(Up.y) == 1.0f && Up.z == 0.0f;
            UpAlignToZ = Up.x == 0.0f && Up.y == 0.0f && Math.Abs(Up.z) == 1.0f;
            UpAlignInverted = (UpAlignToX || UpAlignToY || UpAlignToZ) && (Up.x == -1.0f || Up.y == -1.0f || Up.z == 1.0f);
            DirectionAlignToX = Math.Abs(Direction.x) == 1.0f && Direction.y == 0.0f && Direction.z == 0.0f;
            DirectionAlignToY = Direction.x == 0.0f && Math.Abs(Direction.y) == 1.0f && Direction.z == 0.0f;
            DirectionAlignToZ = Direction.x == 0.0f && Direction.y == 0.0f && Math.Abs(Direction.z) == 1.0f;
            DirectionAlignInverted = (DirectionAlignToX || DirectionAlignToY || DirectionAlignToZ) && (Direction.x == -1.0f || Direction.y == -1.0f || Direction.z == 1.0f);

            HasBoundingBoxData = XMin != -100 || XMax != 100 || YMin != -100 || YMax != 100 || ZMin != -100 || ZMax != 100;
        }
        else
        {
            GetComponent<MeshRenderer>().material.color = Color.red;

            Number = DataHolder.CoverPads.Count;
            name = "CoverPad" + NumberString;

            Position = transform.position;
            Direction = new Vector3(0, 0, 1);

            CoverFlags = 0;
        }
    }

    public void AddWaypoint(WaypointProperties Properties)
    {
        PadWaypoint = new Waypoint(Properties)
        {
            ParentPad = this
        };

        GetComponent<MeshRenderer>().material.color = Color.green;
    }

    public void AddWaypoint(Waypoint InWaypoint)
    {
        PadWaypoint = InWaypoint;
        InWaypoint.ParentPad = this;

        GetComponent<MeshRenderer>().material.color = Color.green;
    }

    public void AddNewWaypoint()
    {
        PadWaypoint = new()
        {
            ID = DataHolder.Waypoints.Count,
            ParentPad = this,
            ParentWaygroup = DataHolder.Waygroups.FirstOrDefault()
        };

        DataHolder.Waypoints.Add(PadWaypoint);

        GetComponent<MeshRenderer>().material.color = Color.green;
    }

    public void DeleteWaypoint()
    {
        foreach (WaypointNeighbour CurrentNeighbour in PadWaypoint.Neighbours)
        {
            CurrentNeighbour.LinkedWaypoint.Neighbours.RemoveAll(x => x.LinkedWaypoint == PadWaypoint);
        }

        PadWaypoint.ParentWaygroup.Waypoints.Remove(PadWaypoint);
        DataHolder.Waypoints.Remove(PadWaypoint);
        PadWaypoint = null;

        for (int i = 0; i < DataHolder.Waypoints.Count; ++i)
        {
            DataHolder.Waypoints[i].ID = i;
        }

        GetComponent<MeshRenderer>().material.color = Color.blue;
    }

    public void AddProp(Prop InProp)
    {
        PadProp = InProp;

        if (InProp.ParentPad != null)
        {
            InProp.ParentPad = this;
        }

        if (PadProp.Data == null && PadProp.HeadData == null)
        {
            if (PadProp.Type == PropType.Effect)
            {
                GetComponent<MeshRenderer>().material.color = Color.cyan;
            }
            //Debug.Log("Padprop data is null");
            return;
        }

        //float PropScale = PadProp.Scale * (1.0f / 256.0f);
        Vector3 Centre = Vector3.zero;

        if (PadProp.IsAttachedToCharacter || PadProp.IsAttachedToOtherProp || PadProp.IsGlobal)
        {
            //PadProp.IsAttachedToCharacter = false;
            //PadProp.IsAttachedToOtherProp = false;
            //PadProp.IsGlobal = false;
        }
        else if (PadProp.Type == PropType.Camera2)
        {

        }
        else if (PadProp.Type == PropType.Character)
        {
            GetComponent<MeshFilter>().mesh = PadProp.Body.Meshes[0];
            Mesh CurrentMesh = GetComponent<MeshFilter>().mesh;

            Material[] MeshMaterials = new Material[CurrentMesh.subMeshCount];
            for (int i = 0; i < CurrentMesh.subMeshCount; ++i)
            {
                Material NewMaterial = DataHolder.Materials[PadProp.Body.TextureIDs[i]];
                MeshMaterials[i] = NewMaterial;
            }
            GetComponent<MeshRenderer>().materials = MeshMaterials;

            if (TryGetComponent(out MeshCollider Collider))
            {
                Collider.cookingOptions = MeshColliderCookingOptions.UseFastMidphase;
                Collider.sharedMesh = GetComponent<MeshFilter>().mesh;
            }
        }
        else
        {
            GetComponent<MeshFilter>().mesh = PadProp.Model.Meshes[0];
            Mesh CurrentMesh = GetComponent<MeshFilter>().mesh;

            Material[] MeshMaterials = new Material[CurrentMesh.subMeshCount];
            for (int i = 0; i < CurrentMesh.subMeshCount; ++i)
            {
                if (DataHolder.Materials.TryGetValue(PadProp.Model.TextureIDs[i], out Material NewMaterial))
                {
                    MeshMaterials[i] = NewMaterial;
                }
                else
                {
                    Debug.LogWarning($"Could not find material of texture ID {PadProp.Model.TextureIDs[i]} in model {PadProp.Data.FileName}");
                    MeshMaterials[i] = DataHolder.Materials[0];
                }
            }
            GetComponent<MeshRenderer>().materials = MeshMaterials;

            if (TryGetComponent(out MeshCollider Collider))
            {
                Collider.cookingOptions = MeshColliderCookingOptions.UseFastMidphase;
                Collider.sharedMesh = GetComponent<MeshFilter>().mesh;
            }
        }

        if (PadProp.Type == PropType.Character)
        {
            transform.localScale = new Vector3() { x = 0.1f, y = 0.1f, z = 0.1f };
        }
        else if (PadProp.Type == PropType.Door)
        {
            Vector3 LookAtVector = transform.position + Direction;
            transform.LookAt(LookAtVector, Up);

            float Degrees = 180.0f / Mathf.PI * 1.5705462694168f;
            transform.Rotate(new Vector3(0, 0, 1), Degrees);
            transform.Rotate(new Vector3(1, 0, 0), Degrees);

            Centre.x = transform.position.x + (
                    (XMin + XMax) * Normal.x +
                    (YMin + YMax) * Up.x +
                    (ZMin + ZMax) * Direction.x) * 0.5f;

            Centre.y = transform.position.y + (
                    (XMin + XMax) * Normal.y +
                    (YMin + YMax) * Up.y +
                    (ZMin + ZMax) * Direction.y) * 0.5f;

            Centre.z = transform.position.z + (
                    (XMin + XMax) * Normal.z +
                    (YMin + YMax) * Up.z +
                    (ZMin + ZMax) * Direction.z) * 0.5f;

            transform.position = Centre;

            NodeData BoundingBox = PadProp.Model.GetFirstBoundingBox().Data;

            float XScale;
            float YScale;
            float ZScale;

            XScale = (YMax - YMin) / (BoundingBox.XMax - BoundingBox.XMin);
            YScale = (ZMax - ZMin) / (BoundingBox.YMax - BoundingBox.YMin);
            ZScale = (XMax - XMin) / (BoundingBox.ZMax - BoundingBox.ZMin);

            transform.localScale = new Vector3(XScale, YScale, ZScale);
        }
        else
        {
            Centre = Position;

            Vector3 LookAtVector = transform.position + Direction;
            transform.LookAt(LookAtVector, Up);

            NodeData BoundingBox = PadProp.Model.GetFirstBoundingBox().Data;

            if (HasBoundingBoxData)
            {
                float XScale = 1;
                float YScale = 1;
                float ZScale = 1;
                if (PadProp.Flags1.HasBits(0x00000020))
                {
                    if (BoundingBox.XMin < BoundingBox.XMax)
                    {
                        XScale = (XMax - XMin) / (BoundingBox.XMax - BoundingBox.XMin * (PadProp.Scale / 256.0f));
                    }
                    else
                    {
                        Debug.Log($"XMin: {XMin}, XMax: {XMax}");
                        Debug.LogError("XMin is greater than XMax for pad " + NumberString);
                    }
                }
                if (PadProp.Flags1.HasBits(0x00000040))
                {
                    if (BoundingBox.YMin < BoundingBox.YMax)
                    {
                        if (PadProp.Flags1.HasBits(0x00000002))
                        {
                            YScale = (ZMax - ZMin) / (BoundingBox.YMax - BoundingBox.YMin * (PadProp.Scale / 256.0f));
                        }
                        else
                        {
                            YScale = (YMax - YMin) / (BoundingBox.YMax - BoundingBox.YMin * (PadProp.Scale / 256.0f));
                        }
                    }
                    else
                    {
                        Debug.Log($"XMin: {XMin}, XMax: {XMax}");
                        Debug.LogError("YMin is greater than YMax for pad " + NumberString);
                    }
                }
                if (PadProp.Flags1.HasBits(0x00000080))
                {
                    if (BoundingBox.ZMin < BoundingBox.ZMax)
                    {
                        if (PadProp.Flags1.HasBits(0x00000002))
                        {
                            ZScale = (YMax - YMin) / (BoundingBox.ZMax - BoundingBox.ZMin * (PadProp.Scale / 256.0f));
                        }
                        else
                        {
                            ZScale = (ZMax - ZMin) / (BoundingBox.ZMax - BoundingBox.ZMin * (PadProp.Scale / 256.0f));
                        }
                    }
                    else
                    {
                        Debug.Log($"XMin: {XMin}, XMax: {XMax}");
                        Debug.LogError("ZMin is greater than ZMax for pad " + NumberString);
                    }
                }
                transform.localScale = new Vector3(XScale, YScale, ZScale);

                /*Centre.x = Position.x + (
                (BoundingBox.XMin + BoundingBox.XMax) * Normal.x +
                (BoundingBox.YMin + BoundingBox.YMax) * Up.x +
                (BoundingBox.ZMin + BoundingBox.ZMax) * Direction.x) * 0.5f;

                Centre.y = Position.y + (
                (BoundingBox.XMin + BoundingBox.XMax) * Normal.y +
                (BoundingBox.YMin + BoundingBox.YMax) * Up.y +
                (BoundingBox.ZMin + BoundingBox.ZMax) * Direction.y) * 0.5f;

                Centre.z = Position.z + (
                (BoundingBox.XMin + BoundingBox.XMax) * Normal.z +
                (BoundingBox.YMin + BoundingBox.YMax) * Up.z +
                (BoundingBox.ZMin + BoundingBox.ZMax) * Direction.z) * 0.5f;


                Centre.x += (BoundingBox.YMin - BoundingBox.YMax) * 0.5f * Up.x;
                Centre.y += (BoundingBox.YMin - BoundingBox.YMax) * 0.5f * Up.y;
                Centre.z += (BoundingBox.YMin - BoundingBox.YMax) * 0.5f * Up.z;*/

                Centre = Position;
            }
            else
            {
                float TempScale = PadProp.Scale / 256.0f;
                if (PadProp.Data.Scale == 409)
                {
                    TempScale *= 0.1f;
                }

                transform.localScale = new Vector3(TempScale, TempScale, TempScale);

                Centre = Position;

            }

            if (PadProp.Flags1.HasBits(0x00000002)) // 2D object
            {
                //Vector3 PadRotation = transform.eulerAngles;
                //PadRotation.x += 270;// ((270) * 3.141092641f / 180.0f);
                //PadRotation.y += 180; // ((180) * 3.141092641f / 180.0f);
                //transform.eulerAngles = PadRotation;

                transform.position = Centre;
            }
            else
            {
                if (PadProp.Flags1.HasBits(0x00000004))
                {

                }
                else if (PadProp.Flags1.HasBits(0x00000008))
                {

                }
                else
                {

                }

                transform.position = Centre;
            }

            //Debug.Log($"ROTATION: {ExtractRotation(Matrix)}");
            //transform.rotation = ExtractRotation(Matrix);
        }
        /*else
        {
            Vector3 LookAtVector = transform.position + Direction;
            transform.LookAt(LookAtVector, Up);

            NodeData BoundingBox = PadProp.Model.GetFirstBoundingBox().Data;

            if (HasBoundingBoxData)
            {
                float XScale = 1;
                float YScale = 1;
                float ZScale = 1;
                if (PadProp.Flags1.HasBits(0x00000020))
                {
                    if (BoundingBox.XMin < BoundingBox.XMax)
                    {
                        XScale = (XMax - XMin) / (BoundingBox.XMax - BoundingBox.XMin * (PadProp.Scale / 256.0f));
                    }
                    else
                    {
                        Debug.Log($"XMin: {XMin}, XMax: {XMax}");
                        Debug.LogError("XMin is greater than XMax for pad " + Number);
                    }
                }
                if (PadProp.Flags1.HasBits(0x00000040))
                {
                    if (BoundingBox.YMin < BoundingBox.YMax)
                    {
                        if (PadProp.Flags1.HasBits(0x00000002))
                        {
                            YScale = (ZMax - ZMin) / (BoundingBox.YMax - BoundingBox.YMin * (PadProp.Scale / 256.0f));
                        }
                        else
                        {
                            YScale = (YMax - YMin) / (BoundingBox.YMax - BoundingBox.YMin * (PadProp.Scale / 256.0f));
                        }
                    }
                    else
                    {
                        Debug.LogError("YMin is greater than YMax for pad " + Number);
                    }
                }
                if (PadProp.Flags1.HasBits(0x00000080))
                {
                    if (BoundingBox.ZMin < BoundingBox.ZMax)
                    {
                        if (PadProp.Flags1.HasBits(0x00000002))
                        {
                            ZScale = (YMax - YMin) / (BoundingBox.ZMax - BoundingBox.ZMin * (PadProp.Scale / 256.0f));
                        }
                        else
                        {
                            ZScale = (ZMax - ZMin) / (BoundingBox.ZMax - BoundingBox.ZMin * (PadProp.Scale / 256.0f));
                        }
                    }
                    else
                    {
                        Debug.LogError("ZMin is greater than ZMax for pad " + Number);
                    }
                }
                transform.localScale = new Vector3(XScale, YScale, ZScale);
            }
            else
            {
                float TempScale = PadProp.Scale / 256.0f;
                if (PadProp.Data.Scale == 409)
                {
                    TempScale *= 0.1f;
                }

                transform.localScale = new Vector3(TempScale, TempScale, TempScale);
            }
        }*/
    }


    public void DetachProp()
    {
        PadProp.ParentPad = null;

        PadProp = null;

        if (HasBoundingBoxData)
        {
            ChangeScale(XMin, XMax, YMin, YMax, ZMin, ZMax);
        }
        else
        {
            transform.localScale = Vector3.one;
        }

        ChangePosition(Position);
        ChangeRotation(Direction, Up);

        GetComponent<MeshFilter>().mesh = Resources.Load<Mesh>("PadMesh");
        GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { Resources.Load<Material>("PadMaterial") });
        GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().mesh;

        if (PadWaypoint != null)
        {
            GetComponent<MeshRenderer>().material.color = Color.green;
        }
        else
        {
            GetComponent<MeshRenderer>().material.color = Color.blue;
        }
    }

    public virtual void ChangePosition(Vector3 InPosition)
    {
        Position = InPosition;

        if (PadProp == null || PadProp.Type == PropType.Character)
        {
            transform.position = InPosition;
        }
        else if (PadProp.Type == PropType.Door)
        {
            Vector3 Centre;
            Centre.x = InPosition.x + (
                    (XMin + XMax) * Normal.x +
                    (YMin + YMax) * Up.x +
                    (ZMin + ZMax) * Direction.x) * 0.5f;

            Centre.y = InPosition.y + (
                    (XMin + XMax) * Normal.y +
                    (YMin + YMax) * Up.y +
                    (ZMin + ZMax) * Direction.y) * 0.5f;

            Centre.z = InPosition.z + (
                    (XMin + XMax) * Normal.z +
                    (YMin + YMax) * Up.z +
                    (ZMin + ZMax) * Direction.z) * 0.5f;

            transform.position = Centre;
        }
        else
        {
            if (PadProp.Flags1.HasBits(0x00000002))
            {

            }
            else if (PadProp.Flags1.HasBits(0x00000004))
            {

            }
            else
            {
                transform.position = InPosition;
            }
        }
    }

    public void ChangeRotation(Vector3 InDirection, Vector3 InUp)
    {
        Direction = InDirection;
        Up = InUp;

        RecalculateNormal();

        UpAlignToX = Math.Abs(Up.x) == 1.0f && Up.y == 0.0f && Up.z == 0.0f;
        UpAlignToY = Up.x == 0.0f && Math.Abs(Up.y) == 1.0f && Up.z == 0.0f;
        UpAlignToZ = Up.x == 0.0f && Up.y == 0.0f && Math.Abs(Up.z) == 1.0f;
        UpAlignInverted = (UpAlignToX || UpAlignToY || UpAlignToZ) && (Up.x == -1.0f || Up.y == -1.0f || Up.z == 1.0f);
        DirectionAlignToX = Math.Abs(Direction.x) == 1.0f && Direction.y == 0.0f && Direction.z == 0.0f;
        DirectionAlignToY = Direction.x == 0.0f && Math.Abs(Direction.y) == 1.0f && Direction.z == 0.0f;
        DirectionAlignToZ = Direction.x == 0.0f && Direction.y == 0.0f && Math.Abs(Direction.z) == 1.0f;
        DirectionAlignInverted = (DirectionAlignToX || DirectionAlignToY || DirectionAlignToZ) && (Direction.x == -1.0f || Direction.y == -1.0f || Direction.z == 1.0f);

        if (PadProp == null || PadProp.Type == PropType.Character)
        {
            Vector3 LookAtVector = transform.position + Direction;
            transform.LookAt(LookAtVector, Up);
        }
        else if (PadProp.Type == PropType.Door)
        {
            Vector3 LookAtVector = transform.position + Direction;
            transform.LookAt(LookAtVector, Up);

            float Degrees = 180.0f / Mathf.PI * 1.5705462694168f;
            transform.Rotate(new Vector3(0, 0, 1), Degrees);
            transform.Rotate(new Vector3(1, 0, 0), Degrees);

            Vector3 Centre;
            Centre.x = Position.x + (
                    (XMin + XMax) * Normal.x +
                    (YMin + YMax) * Up.x +
                    (ZMin + ZMax) * Direction.x) * 0.5f;

            Centre.y = Position.y + (
                    (XMin + XMax) * Normal.y +
                    (YMin + YMax) * Up.y +
                    (ZMin + ZMax) * Direction.y) * 0.5f;

            Centre.z = Position.z + (
                    (XMin + XMax) * Normal.z +
                    (YMin + YMax) * Up.z +
                    (ZMin + ZMax) * Direction.z) * 0.5f;

            transform.position = Centre;
        }
        else
        {
            Vector3 LookAtVector = Position + Direction;
            transform.LookAt(LookAtVector, Up);

            if (PadProp.Flags1.HasBits(0x00000002))
            {
                float XRotation = 180.0f / Mathf.PI * 4.7116389274597f;
                float YRotation = 180.0f / Mathf.PI * 3.141092641f;
                transform.Rotate(new Vector3(1, 0, 0), -XRotation);
                transform.Rotate(new Vector3(0, 1, 0), YRotation);
            }
            else if (PadProp.Flags1.HasBits(0x00000004))
            {
                float ZRotation = 180.0f / Mathf.PI * 3.141092641f;
                transform.Rotate(new Vector3(1, 0, 0), ZRotation);
            }
        }
    }

    public void ChangeScale(float InXMin, float InXMax, float InYMin, float InYMax, float InZMin, float InZMax)
    {
        XMin = InXMin;
        XMax = InXMax;
        YMin = InYMin;
        YMax = InYMax;
        ZMin = InZMin;
        ZMax = InZMax;

        if (!HasBoundingBoxData)
        {
            return;
        }

        if (PadProp == null)
        {
            float XScale = (YMax - YMin) / 10;
            float YScale = (ZMax - ZMin) / 10;
            float ZScale = (XMax - XMin) / 10;

            transform.localScale = new Vector3(XScale, YScale, ZScale);
        }
        else if (PadProp.Type == PropType.Character)
        {
            transform.localScale = new Vector3() { x = 0.1f, y = 0.1f, z = 0.1f };
        }
        else if (PadProp.Type == PropType.Door)
        {
            Vector3 Centre;
            Centre.x = Position.x + (
                    (XMin + XMax) * Normal.x +
                    (YMin + YMax) * Up.x +
                    (ZMin + ZMax) * Direction.x) * 0.5f;

            Centre.y = Position.y + (
                    (XMin + XMax) * Normal.y +
                    (YMin + YMax) * Up.y +
                    (ZMin + ZMax) * Direction.y) * 0.5f;

            Centre.z = Position.z + (
                    (XMin + XMax) * Normal.z +
                    (YMin + YMax) * Up.z +
                    (ZMin + ZMax) * Direction.z) * 0.5f;

            transform.position = Centre;

            NodeData BoundingBox = PadProp.Model.GetFirstBoundingBox().Data;

            float XScale;
            float YScale;
            float ZScale;

            XScale = (YMax - YMin) / (BoundingBox.XMax - BoundingBox.XMin);
            YScale = (ZMax - ZMin) / (BoundingBox.YMax - BoundingBox.YMin);
            ZScale = (XMax - XMin) / (BoundingBox.ZMax - BoundingBox.ZMin);

            transform.localScale = new Vector3(XScale, YScale, ZScale);
        }
        else
        {
            NodeData BoundingBox = PadProp.Model.GetFirstBoundingBox().Data;
            float XScale = 1;
            float YScale = 1;
            float ZScale = 1;
            if (PadProp.Flags1.HasBits(0x00000020))
            {
                if (BoundingBox.XMin < BoundingBox.XMax)
                {
                    XScale = (XMax - XMin) / (BoundingBox.XMax - BoundingBox.XMin * (PadProp.Scale / 256.0f));
                }
                else
                {
                    Debug.LogError("XMin is greater than XMax for pad " + Number);
                }
            }
            if (PadProp.Flags1.HasBits(0x00000040))
            {
                if (BoundingBox.YMin < BoundingBox.YMax)
                {
                    if (PadProp.Flags1.HasBits(0x00000002))
                    {
                        YScale = (ZMax - ZMin) / (BoundingBox.YMax - BoundingBox.YMin * (PadProp.Scale / 256.0f));
                    }
                    else
                    {
                        YScale = (YMax - YMin) / (BoundingBox.YMax - BoundingBox.YMin * (PadProp.Scale / 256.0f));
                    }
                }
                else
                {
                    Debug.LogError("YMin is greater than YMax for pad " + Number);
                }
            }
            if (PadProp.Flags1.HasBits(0x00000080))
            {
                if (BoundingBox.ZMin < BoundingBox.ZMax)
                {
                    if (PadProp.Flags1.HasBits(0x00000002))
                    {
                        ZScale = (YMax - YMin) / (BoundingBox.ZMax - BoundingBox.ZMin * (PadProp.Scale / 256.0f));
                    }
                    else
                    {
                        ZScale = (ZMax - ZMin) / (BoundingBox.ZMax - BoundingBox.ZMin * (PadProp.Scale / 256.0f));
                    }
                }
                else
                {
                    Debug.LogError("ZMin is greater than ZMax for pad " + Number);
                }
            }
            transform.localScale = new Vector3(XScale, YScale, ZScale);
        }
    }

    public void ChangeScale(int InScale)
    {
        PadProp.Scale = InScale;

        if (PadProp.Type == PropType.Character)
        {
            transform.localScale = new Vector3() { x = 0.1f, y = 0.1f, z = 0.1f };
        }
        else if (PadProp.Type == PropType.Door)
        {
            Vector3 Centre;
            Centre.x = transform.position.x + (
                    (XMin + XMax) * Normal.x +
                    (YMin + YMax) * Up.x +
                    (ZMin + ZMax) * Direction.x) * 0.5f;

            Centre.y = transform.position.y + (
                    (XMin + XMax) * Normal.y +
                    (YMin + YMax) * Up.y +
                    (ZMin + ZMax) * Direction.y) * 0.5f;

            Centre.z = transform.position.z + (
                    (XMin + XMax) * Normal.z +
                    (YMin + YMax) * Up.z +
                    (ZMin + ZMax) * Direction.z) * 0.5f;

            transform.position = Centre;

            NodeData BoundingBox = PadProp.Model.GetFirstBoundingBox().Data;

            float XScale;
            float YScale;
            float ZScale;

            XScale = (YMax - YMin) / (BoundingBox.XMax - BoundingBox.XMin);
            YScale = (ZMax - ZMin) / (BoundingBox.YMax - BoundingBox.YMin);
            ZScale = (XMax - XMin) / (BoundingBox.ZMax - BoundingBox.ZMin);

            transform.localScale = new Vector3(XScale, YScale, ZScale);
        }
        else
        {
            NodeData BoundingBox = PadProp.Model.GetFirstBoundingBox().Data;

            if (HasBoundingBoxData)
            {
                float XScale = 1;
                float YScale = 1;
                float ZScale = 1;
                if (PadProp.Flags1.HasBits(0x00000020))
                {
                    if (BoundingBox.XMin < BoundingBox.XMax)
                    {
                        XScale = (XMax - XMin) / (BoundingBox.XMax - BoundingBox.XMin * (PadProp.Scale / 256.0f));
                    }
                    else
                    {
                        Debug.Log($"XMin: {XMin}, XMax: {XMax}");
                        Debug.LogError("XMin is greater than XMax for pad " + Number);
                    }
                }
                if (PadProp.Flags1.HasBits(0x00000040))
                {
                    if (BoundingBox.YMin < BoundingBox.YMax)
                    {
                        if (PadProp.Flags1.HasBits(0x00000002))
                        {
                            YScale = (ZMax - ZMin) / (BoundingBox.YMax - BoundingBox.YMin * (PadProp.Scale / 256.0f));
                        }
                        else
                        {
                            YScale = (YMax - YMin) / (BoundingBox.YMax - BoundingBox.YMin * (PadProp.Scale / 256.0f));
                        }
                    }
                    else
                    {
                        Debug.LogError("YMin is greater than YMax for pad " + Number);
                    }
                }
                if (PadProp.Flags1.HasBits(0x00000080))
                {
                    if (BoundingBox.ZMin < BoundingBox.ZMax)
                    {
                        if (PadProp.Flags1.HasBits(0x00000002))
                        {
                            ZScale = (YMax - YMin) / (BoundingBox.ZMax - BoundingBox.ZMin * (PadProp.Scale / 256.0f));
                        }
                        else
                        {
                            ZScale = (ZMax - ZMin) / (BoundingBox.ZMax - BoundingBox.ZMin * (PadProp.Scale / 256.0f));
                        }
                    }
                    else
                    {
                        Debug.LogError("ZMin is greater than ZMax for pad " + Number);
                    }
                }
                transform.localScale = new Vector3(XScale, YScale, ZScale);
            }
            else
            {
                float TempScale = PadProp.Scale / 256.0f;
                if (PadProp.Data.Scale == 409)
                {
                    TempScale *= 0.1f;
                }

                transform.localScale = new Vector3(TempScale, TempScale, TempScale);
            }
        }
    }

    protected void RecalculateNormal()
    {
        Vector3 NewNormal = new()
        {
            x = Up.y * Direction.z - Direction.y * Up.z,
            y = Up.z * Direction.x - Direction.z * Up.x,
            z = Up.x * Direction.y - Direction.x * Up.y
        };
        Normal = NewNormal;
    }

    public void ChangeModel(ModelData InModel)
    {
        if (PadProp.Type == PropType.Character)
        {
            PadProp.Data = InModel;
            PadProp.Body = DataHolder.CharacterModels[PadProp.Data.FilePath];

            GetComponent<MeshFilter>().mesh = PadProp.Body.Meshes[0];
            Mesh CurrentMesh = GetComponent<MeshFilter>().mesh;

            Material[] MeshMaterials = new Material[CurrentMesh.subMeshCount];
            for (int i = 0; i < CurrentMesh.subMeshCount; ++i)
            {
                Material NewMaterial = DataHolder.Materials[PadProp.Body.TextureIDs[i]];
                MeshMaterials[i] = NewMaterial;
            }
            GetComponent<MeshRenderer>().materials = MeshMaterials;

            if (TryGetComponent(out MeshCollider Collider))
            {
                Collider.cookingOptions = MeshColliderCookingOptions.UseFastMidphase;
                Collider.sharedMesh = GetComponent<MeshFilter>().mesh;
            }
        }
        else
        {
            PadProp.Data = InModel;
            PadProp.Model = DataHolder.PropModels[PadProp.Data.FilePath];

            GetComponent<MeshFilter>().mesh = PadProp.Model.Meshes[0];
            Mesh CurrentMesh = GetComponent<MeshFilter>().mesh;

            Material[] MeshMaterials = new Material[CurrentMesh.subMeshCount];
            for (int i = 0; i < CurrentMesh.subMeshCount; ++i)
            {
                if (DataHolder.Materials.TryGetValue(PadProp.Model.TextureIDs[i], out Material NewMaterial))
                {
                    MeshMaterials[i] = NewMaterial;
                }
                else
                {
                    Debug.LogWarning($"Could not find material of texture ID {PadProp.Model.TextureIDs[i]} in model {PadProp.Data.FileName}");
                    MeshMaterials[i] = DataHolder.Materials[0];
                }
            }
            GetComponent<MeshRenderer>().materials = MeshMaterials;

            if (TryGetComponent(out MeshCollider Collider))
            {
                Collider.cookingOptions = MeshColliderCookingOptions.UseFastMidphase;
                Collider.sharedMesh = GetComponent<MeshFilter>().mesh;
            }
        }
    }

    public void ChangeHead(ModelData InModel)
    {

    }

    public void SetBoundingBoxData(bool HasBox)
    {
        if (HasBox == false)
        {
            HasBoundingBoxData = false;
            XMin = -100;
            XMax = 100;
            YMin = -100;
            YMax = 100;
            ZMin = -100;
            ZMax = 100;

            if (PadProp == null)
            {
                transform.localScale = Vector3.one;
            }
            else
            {
                ChangeScale(PadProp.Scale);
            }
        }
        else
        {
            HasBoundingBoxData = true;

            ChangeScale(-10, 10, -10, 10, -10, 10);
        }
    }

    public void ExportModel()
    {
        AssimpContext Importer = new();
        Scene Scene = new()
        {
            RootNode = new Assimp.Node("Root")
        };

        Mesh CurrentMesh = GetComponent<MeshFilter>().mesh;
        Assimp.Mesh AssimpMesh = new();

        Assimp.Material mat = new()
        {
            Name = "MyMaterial"
        };
        Scene.Materials.Add(mat);

        foreach (Vector3 vrt in CurrentMesh.vertices)
        {
            AssimpMesh.Vertices.Add(new Vector3D(vrt.x, vrt.y, vrt.z));
        }

        AssimpMesh.SetIndices(CurrentMesh.triangles, 3);

        foreach (Vector2 uv in CurrentMesh.uv)
        {
            AssimpMesh.TextureCoordinateChannels[0].Add(new Vector3D(uv.x, uv.y, 0f));
        }

        AssimpMesh.MaterialIndex = 0;

        Scene.Meshes.Add(AssimpMesh);

        Scene.RootNode.MeshIndices.Add(0);
        Debug.Log(Scene.RootNode.MeshIndices.Count);

        Importer.ExportFile(Scene, "C:\\Users\\Oofjay\\Desktop\\Temp\\ExportedModel.obj", "obj");

    }

    public void OnHit(RaycastHit Hit)
    {
        Mesh HitMesh = GetComponent<MeshFilter>().mesh;

        if (HitMesh != null && HitMesh.isReadable == true)
        {
            int FirstVert = HitMesh.triangles[Hit.triangleIndex * 3];
            int SecondVert = HitMesh.triangles[Hit.triangleIndex * 3 + 1];
            int ThirdVert = HitMesh.triangles[Hit.triangleIndex * 3 + 2];

            Debug.Log(FirstVert.ToString() + HitMesh.vertices[FirstVert] + HitMesh.uv[FirstVert] + SecondVert.ToString() + HitMesh.vertices[SecondVert] + HitMesh.uv[SecondVert] + ThirdVert.ToString() + HitMesh.vertices[ThirdVert] + HitMesh.uv[ThirdVert]);
        }
    }
}
