using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;
using Newtonsoft.Json;

/*
 * These are temporary objects that hold all the properties for a certain pad/waypoint/waygroup/cover pad. 
 * Since the pads are normally represented as Unity GameObjects in the scene, the FileReader cannot house them.
 * Therefore, we first use this object to assign all the properties from the file to it,
 * then we create all the Pad GameObjects in DataHolder and assign the properties from this class to the Pad GameObject.
 */

public class AnimationProperties
{
    public string id;
    public string file;
    public int numframes;
    public int bytesperframe;
    public int unk08;
    public int unk0a;
    public bool flag01;
    public bool flag02;
    public bool flag04;
    public bool flag08;
}

public class MusicTrackProperties
{
    public string id;
    public string file;
}

public class ParameterProperties
{
    public string type;
    public string name;
    public string constantname;
}

public class AICommandProperties
{
    public int id;
    public string name;
    public string displayname;
    public string description;
    public List<ParameterProperties> parameters;
}

public class PadProperties
{
    public string id;
    public List<float> pos;
    public List<float> dir;
    public List<float> up;
    public float xmin;
    public float xmax;
    public float ymin;
    public float ymax;
    public float zmin;
    public float zmax;
    public bool aiwaitlift;
    public bool aionlift;
    public bool aiwalkdirect;
    public bool aidrop;
    public bool aicrouch;
    public bool aiignorey;
    public bool aiduck;
    public int liftnum;
}

public class WaygroupNeighbourProperties
{
    public string waygroup;
    public bool flag4000;
    public bool flag8000;
}

public class WaypointNeighbourProperties
{
    public string waypoint;
    public bool flag4000;
    public bool flag8000;
}

public class WaypointProperties
{
    public string id;
    public string pad;
    public List<WaypointNeighbourProperties> neighbours;
    public string waygroup;
}

public class WaygroupProperties
{
    public string id;
    public List<WaygroupNeighbourProperties> neighbours;
}

public class CoverPadProperties
{
    public string id;
    public List<float> pos;
    public List<float> dir;
    public int unk18;
}

// This represents all the objects and arrays that are listed in the JSON. This will then be converted into their proper pad GameObjects in the DataHolder class.
public class JSONPadObject
{
    public List<PadProperties> pads = new();
    public List<WaypointProperties> waypoints = new();
    public List<WaygroupProperties> waygroups = new();
    public List<CoverPadProperties> cover = new();
}

public class TileProperties
{
    public bool flag0001;
    public bool flag0002;
    public bool flag0004;
    public bool flag0008;
    public bool flag0010;
    public bool flag0020;
    public bool ladder;
    public bool flag0080;
    public bool flag0100;
    public bool underwater;
    public bool flag0400;
    public bool aibotcrouch;
    public bool aibotduck;
    public bool flag2000;
    public bool die;
    public bool climbableledge;
    public string floortype;
    public int floorcolour;
    public List<Vector3> vertices;
}

public class JSONTileObject
{
    public Dictionary<string, List<TileProperties>> rooms = new();
}

public class AIListOffsetHolder
{
    public enum AIListOffsetType
    {
        AIList,
        Path
    }
    public AIListOffsetType OffsetType;
    public AICommandInstance CommandInstance;
    public int ParameterIndex;
}

public class FileReader
{
    //public string FilePath;
    private string[] FileLines;

    private int StartPosition; // Seek position that indicates the beginning of a word, usually an argument in a function or an object name.
    private int EndPosition; // Seek position that's similar to StartPosition, but for the end of a word.
    private int PipePosition; // Seek position that indicates a pipe (the '\' character). Used to separate bitflags that are OR'ed together.
    private bool PipeFound; // Has UpdateSeekPositions found a pipe, or are we at the end of the list of bitflags?

    private string Line; // The current line that is being read, usually stripped of whitespace at the start. Different from "CurrentLine", which is unprocessed.

    private bool ApplicableVersion = true;
    private bool VersionMatched = true;

    private Dictionary<string, int> Defines = new() { 
        { "FALSE", 0x0 }, 
        { "TRUE", 0x1 },
        { "CHR_P1P2_OPPOSITE", 0xF1 },
        { "CHR_P1P2", 0xF2 },
        { "CHR_ANY", 0xF3 },
        { "CHR_ANTI", 0xF4 },
        { "CHR_COOP", 0xF5 },
        { "CHR_TARGET", 0xF6 },
        { "CHR_BOND", 0xF8 },
        { "CHR_CLONE", 0xF9 },
        { "CHR_SEESHOT", 0xFA },
        { "CHR_SEEDIE", 0xFB },
        { "CHR_PRESET", 0xFC },
        { "CHR_SELF", 0xFD }
    };

    public JSONPadObject PadContainer;

    public List<Objective> Objectives = new();
    public List<IntroSetting> IntroSettings = new();
    public List<Prop> Props = new();
    public List<Prop> VisibleProps = new();
    public List<Prop> Characters = new()
    {
        new Prop() { Type = PropType.Character, CharacterID = 0xF1, IsAbstract = true, DisplayName = "Player 1/Player 2 (opposite)" },
        new Prop() { Type = PropType.Character, CharacterID = 0xF2, IsAbstract = true, DisplayName = "Player 1/Player 2" },
        new Prop() { Type = PropType.Character, CharacterID = 0xF3, IsAbstract = true, DisplayName = "Any (command 63 only)" },
        new Prop() { Type = PropType.Character, CharacterID = 0xF4, IsAbstract = true, DisplayName = "Counter-op" },
        new Prop() { Type = PropType.Character, CharacterID = 0xF5, IsAbstract = true, DisplayName = "Co-op" },
        new Prop() { Type = PropType.Character, CharacterID = 0xF6, IsAbstract = true, DisplayName = "Target" },
        new Prop() { Type = PropType.Character, CharacterID = 0xF8, IsAbstract = true, DisplayName = "Player" },
        new Prop() { Type = PropType.Character, CharacterID = 0xF9, IsAbstract = true, DisplayName = "Character's clone" },
        new Prop() { Type = PropType.Character, CharacterID = 0xFA, IsAbstract = true, DisplayName = "Character visibly shot" },
        new Prop() { Type = PropType.Character, CharacterID = 0xFB, IsAbstract = true, DisplayName = "Character visibly dying" },
        new Prop() { Type = PropType.Character, CharacterID = 0xFC, IsAbstract = true, DisplayName = "Character preset" },
        new Prop() { Type = PropType.Character, CharacterID = 0xFD, IsAbstract = true, DisplayName = "Self" },
    };
    public List<Prop> Tags = new();
    public List<GuardPath> Paths = new();
    public List<AIList> AILists = new();

    // This contains special objects that refer to other props in the list. We store them here until we handle them at the end, because they can refer to props that haven't been processed yet.
    private List<PropOffsetHolder> Offsets = new();

    // This refers to AI list parameters that have to be processed later, because they refer to or depend on objects that don't exist yet.
    private List<AIListOffsetHolder> AIListOffsets = new();

    public List<ModelData> PropDataList = new();
    public List<ModelData> CharacterDataList = new();
    public Dictionary<string, int> ModelNumbers = new();
    public Dictionary<string, int> FileNumbers = new(); // key is filename constant, value is file number
    public List<string> LanguageFileConstants = new();

    private readonly char[] TrimCharacters = new char[] { ' ', '\t' };
    private readonly char[] TrimCharactersWithClosingParenthesis = new char[] { ' ', '\t', ')' };

    private readonly BitFlagCollection StageFlags = GameBitFlagHolder.GetStageFlags;
    private readonly BitFlagCollection LockFlags = GameBitFlagHolder.GetLockFlags;

    public bool ReadSetup(string FilePath)
    {
        FileLines = File.ReadAllLines(FilePath);

        bool HandlingDefineAIList = false;
        bool HandlingEnum = false;

        bool HandlingProps = false;
        bool HandlingObjectives = false;
        bool HandlingIntro = false;

        bool HandlingPaths = false;
        bool HandlingPathPads = false;

        bool HandlingAICommands = false;
        bool HandlingAILists = false;

        Objective CurrentObjective = null;
        GuardPath CurrentPath = null;
        AIList CurrentAIList = null;

        int PropID = 0;
        int ConditionID = 0;
        int DefineID = 0;

        foreach (string CurrentLine in FileLines)
        {
            if (string.IsNullOrWhiteSpace(CurrentLine))
            {
                continue;
            }

            Line = CurrentLine.TrimStart(TrimCharacters);

            StartPosition = 0;
            EndPosition = 0;
            PipePosition = 0;
            PipeFound = false;

            if (LineStartsWith("#if") || LineStartsWith("#else") || LineStartsWith("#endif"))
            {
                HandleVersions();
                continue;
            }
            else if (!ApplicableVersion)
            {
                continue;
            }
            else if (HandlingDefineAIList)
            {
                if (Line[^1] != '\\')
                {
                    HandlingDefineAIList = false;
                }
                continue;
            }
            else if (LineStartsWith("enum"))
            {
                HandlingEnum = true;
                continue;
            }
            else if (LineStartsWith("u32 props[] = {"))
            {
                HandlingProps = true;
                continue;
            }
            else if (LineStartsWith("s32 intro[] = {"))
            {
                HandlingIntro = true;
                continue;
            }
            else if (LineStartsWith("struct path paths[] = {"))
            {
                HandlingPaths = true;
                continue;
            }
            else if (LineStartsWith("s32 path"))
            {
                HandlingPathPads = true;

                StartPosition = Line.IndexOf('p');
                EndPosition = Line.IndexOf('[');

                CurrentPath = new()
                {
                    Name = Line[StartPosition..EndPosition]
                };
                Paths.Add(CurrentPath);
                continue;
            }
            else if (LineStartsWith("u8"))
            {
                HandlingAICommands = true;

                StartPosition = Line.IndexOf(' ') + 1;
                EndPosition = Line.IndexOf('[');
                CurrentAIList = new() { Name = Line[StartPosition..EndPosition] };
                CurrentAIList.DisplayName = CurrentAIList.Name;
                AILists.Add(CurrentAIList);
                continue;
            }
            else if (LineStartsWith("struct ailist"))
            {
                HandlingAILists = true;
                continue;
            }
            else if (LineStartsWith("#")) // Compiler directive
            {
                if (LineStartsWith("#define"))
                {
                    if (Line[^1] == '\\')
                    {
                        HandlingDefineAIList = true;
                        continue;
                    }
                    else if (LineStartsWith("#define STAGEFLAG_"))
                    {
                        Constant Define = GetDefine();
                        StageFlags.Add(new BitFlag() { Constant = Define.Name, Bits = Define.Value });
                    }
                    else
                    {
                        Tuple<string, int> Define = GetDefineOld();
                        Defines.Add(Define.Item1, Define.Item2);
                    }
                }
                continue;
            }
            else if (LineStartsWith("/") || LineStartsWith("*")) // Comment
            {
                continue;
            }

            UpdateSeekPositions();
            if (HandlingProps == true)
            {
                if (LineStartsWith("endprops"))
                {
                    // if it's the endprops function (which is not important for parsing)
                    continue;
                }
                if (LineStartsWith("};"))
                {
                    // if we're at the end of the object list
                    HandlingProps = false;
                    continue;
                }

                // The object type is, in the code, the first word (before the opening bracket), for example: "door", "stdobject", "beginobjective" etc.
                string ObjectType = "";
                if (EndPosition - StartPosition > 0) // Only fill the object type if there's an opening bracket in the line, otherwise, just leave it empty.
                { 
                    ObjectType = Line[StartPosition..EndPosition];
                }

                if (ObjectType == "beginobjective")
                {
                    HandlingObjectives = true;

                    ConditionID = 0;

                    Objective NewObjective = new();
                    NewObjective.ID = AddNewInt();

                    string StringID = AddNewString();
                    NewObjective.DisplayText = LanguageFileHolder.GetLevelLanguageFile().GetLanguageString(StringID);

                    List<string> Difficulties = AddNewStrings();
                    foreach (string Difficulty in Difficulties)
                    {
                        switch (Difficulty)
                        {
                            case "DIFFBIT_A":
                                NewObjective.Agent = true;
                                break;
                            case "DIFFBIT_SA":
                                NewObjective.SpecialAgent = true;
                                break;
                            case "DIFFBIT_PA":
                                NewObjective.PerfectAgent = true;
                                break;
                            case "DIFFBIT_PD":
                                NewObjective.PerfectDark = true;
                                break;
                            default:
                                // ERROR - incorrect syntax
                                Debug.LogError("Syntax error (unrecognised difficulty) while reading file at line:");
                                Debug.LogError(CurrentLine);
                                return false;
                        }
                    }

                    Objectives.Add(NewObjective);
                    CurrentObjective = NewObjective;
                }
                else if (HandlingObjectives)
                {
                    if (LineStartsWith("endobjective"))
                    {
                        HandlingObjectives = false;
                        continue;
                    }

                    ObjectiveCondition NewCondition = new() { ID = ConditionID, AttachedObjective = CurrentObjective };

                    switch (ObjectType)
                    {
                        case "require_object_destroyed":
                            NewCondition.Type = ConditionType.DestroyObject;
                            NewCondition.TagID = AddNewHexInt();
                            break;
                        case "complete_flags":
                            NewCondition.Type = ConditionType.CompletionFlags;
                            NewCondition.AddNewStageFlag(StageFlags.GetBitFlag(AddNewString()));
                            break;
                        case "fail_flags":
                            NewCondition.Type = ConditionType.FailureFlags;
                            NewCondition.AddNewStageFlag(StageFlags.GetBitFlag(AddNewString()));
                            break;
                        case "require_object_collected":
                            NewCondition.Type = ConditionType.CollectObject;
                            NewCondition.TagID = AddNewHexInt();
                            break;
                        case "require_object_thrown":
                            NewCondition.Type = ConditionType.DepositObject;
                            NewCondition.TagID = AddNewHexInt();
                            break;
                        case "require_object_holographed":
                            NewCondition.Type = ConditionType.HolographObject;
                            NewCondition.TagID = AddNewHexInt();
                            break;
                        case "require_room_entered":
                            NewCondition.Type = ConditionType.EnterRoom;
                            NewCondition.AddNewRoom(DataHolder.Rooms[AddNewHexInt() - 1]);
                            Debug.Log(NewCondition.ObjectiveRoom.NumberString);
                            break;
                        default:
                            // ERROR - incorrect syntax
                            Debug.LogError("Syntax error (unrecognised object type) while reading file at line:");
                            Debug.LogError(CurrentLine);
                            return false;
                    }

                    CurrentObjective.Conditions.Add(NewCondition);
                    ++ConditionID;
                }
                else if (ObjectType == "briefing")
                {
                    string BriefingType = AddNewString();

                    switch (BriefingType)
                    {
                        case "BRIEFINGTYPE_LOCATION":
                            Briefing.StageTitle = LanguageFileHolder.GetLevelLanguageFile().GetLanguageString(AddNewString());
                            break;
                        case "BRIEFINGTYPE_TEXT_A":
                            Briefing.AgentBriefing = LanguageFileHolder.GetLevelLanguageFile().GetLanguageString(AddNewString());
                            break;
                        case "BRIEFINGTYPE_TEXT_SA":
                            Briefing.SpecialAgentBriefing = LanguageFileHolder.GetLevelLanguageFile().GetLanguageString(AddNewString());
                            break;
                        case "BRIEFINGTYPE_TEXT_PA":
                            Briefing.PerfectAgentBriefing = LanguageFileHolder.GetLevelLanguageFile().GetLanguageString(AddNewString());
                            break;
                    }
                }
                else // if it's an actual physical object in the level or a related modifier
                {
                    Prop NewProp = new() { InternalID = PropID };

                    if (ObjectType == "chr")
                    {
                        NewProp.Type = PropType.Character;
                        PropOffsetHolder NewOffset = new() { OffsetPropID = PropID };
                        Offsets.Add(NewOffset);

                        NewProp.SpawnFlags = GameBitFlagHolder.GetBitFlags("SPAWNFLAG", AddNewStrings());
                        NewProp.CharacterID = AddNewHexInt();
                        NewProp.ParentPad = GetPadFromSetupFile(AddNewString());

                        string BodyModelName = AddNewString();
                        if (DataHolder.ModelNumbers.TryGetValue(BodyModelName, out int BodyModelNumber))
                        {
                            NewProp.Data = DataHolder.CharacterDataList[BodyModelNumber];
                            NewProp.Body = DataHolder.CharacterModels[NewProp.Data.FilePath];
                        }
                        else
                        {
                            Debug.Log("Could not find body model for character " + NewProp.CharacterID + " in line: " + Line);
                        }

                        string HeadModelName = AddNewString();
                        if (HeadModelName == "HEAD_RANDOM")
                        {
                            NewProp.HeadData = DataHolder.RandomHeadData;
                            NewProp.Head = DataHolder.RandomHeadModel;
                        }
                        else if (DataHolder.ModelNumbers.TryGetValue(HeadModelName, out int HeadModelNumber))
                        {
                            NewProp.HeadData = DataHolder.CharacterDataList[HeadModelNumber];
                            NewProp.Head = DataHolder.CharacterModels[NewProp.HeadData.FilePath];
                        }
                        else
                        {
                            Debug.Log("Could not find head model for character " + NewProp.CharacterID + " in line: " + Line);
                        }

                        string AIListName = AddNewString();
                        if (int.TryParse(AIListName[2..], NumberStyles.HexNumber, null, out int AIListID))
                        {
                            NewOffset.AIListID = AIListID;
                        }
                        else if (Defines.ContainsKey(AIListName))
                        {
                            NewOffset.AIListID = Defines[AIListName];
                        }
                        else
                        {
                            NewProp.AIList = DataHolder.GlobalAILists.Find(x => x.Constant == AIListName);
                        }

                        NewProp.PadPreset = GetPadFromSetupFile(AddNewString());
                        NewProp.CharacterPreset = GetPadFromSetupFile(AddNewString());
                        NewProp.HearScale = AddNewInt();
                        NewProp.ViewDistance = AddNewInt();

                        NewProp.Flags1 = GameBitFlagHolder.GetBitFlags("CHRFLAG0", AddNewStrings());
                        NewProp.Flags2 = GameBitFlagHolder.GetBitFlags("CHRFLAG1", AddNewStrings());

                        NewProp.Team = ConstantHolder.GetConstant("TEAM", AddNewString());
                        NewProp.Squadron = ConstantHolder.GetConstant("SQUADRON", AddNewString());

                        string ChairID = AddNewString();
                        if (ChairID != "-1")
                        {
                            NewOffset.ChairTagID = int.Parse(ChairID[2..], NumberStyles.HexNumber);
                        }
                        else
                        {
                            NewOffset.ChairTagID = -1;
                        }

                        NewProp.ConvTalk = AddNewInt();
                        NewProp.Tude = AddNewInt();
                        NewProp.NaturalAnimation = AddNewInt();
                        NewProp.YVisAngle = AddNewInt();
                        NewProp.TeamScanDistance = AddNewInt();
                    }
                    else if (ObjectType == "door_scale")
                    {
                        NewProp.Type = PropType.DoorScale;
                        NewProp.Scale = AddNewHexInt();
                    }
                    else if (ObjectType == "tag")
                    {
                        NewProp.Type = PropType.Tag;
                        PropOffsetHolder NewOffset = new() { OffsetPropID = PropID };
                        NewProp.TagID = AddNewHexInt();
                        NewOffset.TaggedPropOffset = AddNewInt();
                        Offsets.Add(NewOffset);
                    }
                    else if (ObjectType == "pad_effect")
                    {
                        NewProp.Type = PropType.Effect;
                        NewProp.Effect = ConstantHolder.GetConstant("PADEFFECT", AddNewString());
                        NewProp.ParentPad = GetPadFromSetupFile(AddNewString());
                    }
                    else if (ObjectType == "camera2")
                    {
                        NewProp.Type = PropType.Camera2;
                        NewProp.Scale = AddNewHexInt();
                        NewProp.XOffset = AddNewHexInt();
                        NewProp.YOffset = AddNewHexInt();
                        NewProp.ZOffset = AddNewHexInt();
                        NewProp.LateralRotation = AddNewHexInt();
                        NewProp.VerticalRotation = AddNewHexInt();
                        NewProp.ParentPad = GetPadFromSetupFile(AddNewString());
                    }
                    else if (ObjectType == "link_guns")
                    {
                        NewProp.Type = PropType.LinkGuns;
                        PropOffsetHolder NewOffset = new() { OffsetPropID = PropID };
                        NewOffset.LinkedGunOffset1 = AddNewInt();
                        NewOffset.LinkedGunOffset2 = AddNewInt();
                        Offsets.Add(NewOffset);
                    }
                    else if (ObjectType == "lift_door")
                    {
                        NewProp.Type = PropType.LiftDoor;
                        PropOffsetHolder NewOffset = new() { OffsetPropID = PropID };
                        NewOffset.LiftStopDoorOffset = AddNewInt();
                        NewOffset.LiftStopLiftOffset = AddNewInt();
                        NewProp.StopNumber = AddNewInt();
                        Offsets.Add(NewOffset);
                    }
                    else if (ObjectType == "rename_object")
                    {
                        NewProp.Type = PropType.Rename;
                        PropOffsetHolder NewOffset = new() { OffsetPropID = PropID };
                        NewOffset.RenameObjectOffset = AddNewInt();
                        NewProp.WeaponType = ConstantHolder.GetConstant("WEAPON", AddNewString());
                        NewProp.OtherText1 = LanguageFileHolder.GetLevelLanguageFile().GetLanguageString(AddNewString());
                        NewProp.OtherText2 = LanguageFileHolder.GetLevelLanguageFile().GetLanguageString(AddNewString());
                        NewProp.MenuListText = LanguageFileHolder.GetLevelLanguageFile().GetLanguageString(AddNewString());
                        NewProp.OtherText3 = LanguageFileHolder.GetLevelLanguageFile().GetLanguageString(AddNewString());
                        NewProp.PickUpText = LanguageFileHolder.GetLevelLanguageFile().GetLanguageString(AddNewString());
                        NewProp.IsAttachedToOtherProp = true;
                        Offsets.Add(NewOffset);
                    }
                    else if (ObjectType == "padlocked_door") 
                    {
                        NewProp.Type = PropType.PadlockedDoor;
                        // Unused in base game
                    }
                    else if (ObjectType == "safeitem")
                    {
                        NewProp.Type = PropType.SafeItem;
                        // Unused in base game
                    }
                    else if (ObjectType == "conditional_scenery")
                    {
                        NewProp.Type = PropType.ConditionalScenery;
                        PropOffsetHolder NewOffset = new() { OffsetPropID = PropID };
                        NewOffset.TriggerPropOffset = AddNewInt();
                        NewOffset.UnexplodedPropOffset = AddNewInt();
                        NewOffset.ExplodedPropOffset = AddNewInt();
                        Offsets.Add(NewOffset);
                    }
                    else if (ObjectType == "blocked_path")
                    {
                        NewProp.Type = PropType.BlockedPath;
                        PropOffsetHolder NewOffset = new() { OffsetPropID = PropID };
                        NewOffset.PathBlockingPropOffset = AddNewInt();
                        NewProp.Waypoint1 = DataHolder.Waypoints[int.Parse(AddNewString(), NumberStyles.AllowHexSpecifier)];
                        NewProp.Waypoint2 = DataHolder.Waypoints[int.Parse(AddNewString(), NumberStyles.AllowHexSpecifier)];
                        Offsets.Add(NewOffset);
                    }
                    else if (ObjectType == "require_object_holographed")
                    {
                        NewProp.Type = PropType.TrainingHolographRequirement;
                    }
                    else
                    {
                        // the following all begin with Scale, Model, Pad, Flags1 (array), Flags2 (array), Flags3 (array), MaxDamage
                        // these are: stdobject, debris, ammocratemulti, shield, gasbottle, glass, safe, hoverbike, escastep, key (key has no MaxDamage)
                        NewProp.Scale = AddNewHexInt();

                        string ModelName = AddNewString();
                        if (DataHolder.ModelNumbers.TryGetValue(ModelName, out int ModelNumber))
                        {
                            NewProp.Data = DataHolder.PropDataList[ModelNumber];
                            NewProp.Model = DataHolder.PropModels[NewProp.Data.FilePath];
                        }
                        else
                        {
                            Debug.Log("Could not find prop model for prop in line: " + Line);
                        }

                        string PadOrCharNumber = AddNewString();
                        if (PadOrCharNumber.StartsWith("0x")) // if the next variable starts with 0x, it means it's a character number
                        {
                            NewProp.IsAttachedToCharacter = true;
                            NewProp.ParentPropID = int.Parse(PadOrCharNumber[2..], NumberStyles.AllowHexSpecifier);
                        }
                        else // otherwise it refers to a pad
                        {
                            NewProp.ParentPad = GetPadFromSetupFile(PadOrCharNumber);
                        }

                        NewProp.Flags1 = GameBitFlagHolder.GetBitFlags("OBJFLAG", AddNewStrings());
                        NewProp.Flags2 = GameBitFlagHolder.GetBitFlags("OBJFLAG2", AddNewStrings());
                        NewProp.Flags3 = GameBitFlagHolder.GetBitFlags("OBJFLAG3", AddNewStrings());

                        if (NewProp.Flags1.HasBits(0x00008000)) // The object is embedded in another
                        {
                            NewProp.IsAttachedToOtherProp = true;

                            if (PropID - 1 > 0)
                            {
                                NewProp.ParentProp = Props[PropID - 1];
                                NewProp.ParentProp.SubProps.Add(NewProp);
                            }
                            else
                            {
                                Debug.LogError("Syntax error (attached prop must have containing prop just before) while reading file at line:");
                                Debug.LogError(CurrentLine);
                                return false;
                            }
                        }

                        if (ObjectType != "key" && ObjectType != "weapon") // Keys and weapons don't take MaxDamage as an argument
                        {
                            NewProp.MaxDamage = AddNewInt();
                        }

                        if (ObjectType == "door")
                        {
                            NewProp.Type = PropType.Door;
                            PropOffsetHolder NewOffset = new() { OffsetPropID = PropID };
                            NewProp.MaxFrac = AddNewHexInt();
                            NewProp.PerimFrac = AddNewHexInt();
                            NewProp.Acceleration = AddNewHexInt();
                            NewProp.Deceleration = AddNewHexInt();
                            NewProp.MaxSpeed = AddNewHexInt();
                            NewProp.DoorFlags = GameBitFlagHolder.GetBitFlags("DOORFLAG", AddNewStrings());
                            NewProp.DoorType = ConstantHolder.GetConstant("DOORTYPE", AddNewString());

                            int NewBits = AddNewHexInt();
                            if (NewBits != 0)
                            {
                                BitFlag MatchedBitFlag = GameBitFlagHolder.GetLockFlags.GetBitFlag(NewBits);

                                if (MatchedBitFlag == null) // Bit flag doesn't exist yet
                                {
                                    BitFlag NewBitFlag = new() { Bits = NewBits, Name = $"Lock flag {LockFlags.Count + 1}", Constant = "LOCKFLAG_" + NewBits.ToString("X") };
                                    NewProp.AddFlags(NewBitFlag);
                                    LockFlags.Add(NewBitFlag);
                                }
                                else
                                {
                                    NewProp.AddFlags(MatchedBitFlag);
                                }
                            }

                            NewProp.AutoCloseTime = AddNewHexInt();
                            NewProp.unk88 = AddNewHexInt();
                            NewOffset.LinkedDoorOffset = AddNewHexInt();
                            NewProp.unkc4 = AddNewHexInt();
                            Offsets.Add(NewOffset);
                        }
                        else if (ObjectType == "key")
                        {
                            NewProp.Type = PropType.Key;

                            int NewBits = AddNewInt();
                            if (NewBits != 0)
                            {
                                BitFlag MatchedBitFlag = GameBitFlagHolder.GetLockFlags.GetBitFlag(NewBits);

                                if (MatchedBitFlag == null) // Bit flag doesn't exist yet
                                {
                                    BitFlag NewBitFlag = new() { Bits = NewBits, Name = $"Lock flag {LockFlags.Count + 1}", Constant = "LOCKFLAG_" + NewBits.ToString("X") };
                                    NewProp.AddFlags(NewBitFlag);
                                    LockFlags.Add(NewBitFlag);
                                }
                                else
                                {
                                    NewProp.AddFlags(MatchedBitFlag);
                                }
                            }
                        }
                        else if (ObjectType == "camera")
                        {
                            NewProp.Type = PropType.Camera;
                            NewProp.TargetPad = GetPadFromSetupFile(AddNewString());
                            NewProp.Yaw = AddNewHexInt();
                            NewProp.YawMaxSpeed = AddNewHexInt();
                            NewProp.MaxDistance = AddNewHexInt();
                        }
                        else if (ObjectType == "ammocrate")
                        {
                            NewProp.Type = PropType.AmmoCrate;
                            NewProp.AmmoType = ConstantHolder.GetConstant("AMMOTYPE", AddNewString());
                        }
                        else if (ObjectType == "singlemonitor")
                        {
                            NewProp.Type = PropType.SingleMonitor;
                            NewProp.Image = ConstantHolder.GetConstant("TVPROGRAM", AddNewString());
                        }
                        else if (ObjectType == "multimonitor")
                        {
                            NewProp.Type = PropType.MultiMonitor;
                            NewProp.Image1 = ConstantHolder.GetConstant("TVPROGRAM", AddNewString());
                            NewProp.Image2 = ConstantHolder.GetConstant("TVPROGRAM", AddNewString());
                            NewProp.Image3 = ConstantHolder.GetConstant("TVPROGRAM", AddNewString());
                            NewProp.Image4 = ConstantHolder.GetConstant("TVPROGRAM", AddNewString());
                        }
                        else if (ObjectType == "autogun")
                        {
                            NewProp.Type = PropType.Autogun;
                            NewProp.TargetPad = GetPadFromSetupFile(AddNewString());
                            NewProp.unk64 = AddNewHexInt();
                            NewProp.unk68 = AddNewHexInt();
                            NewProp.unk80 = AddNewHexInt();
                            NewProp.unk84 = AddNewHexInt();
                        }
                        else if (ObjectType == "tinted_glass")
                        {
                            NewProp.Type = PropType.TintedGlass;
                            NewProp.unk5c = AddNewHexInt();
                        }
                        else if (ObjectType == "hover_prop")
                        {
                            NewProp.Type = PropType.Hoverprop;

                            NewProp.HoverType = ConstantHolder.GetConstant("HOVTYPE", AddNewString());
                        }
                        else if (ObjectType == "lift")
                        {
                            NewProp.Type = PropType.Lift;
                            PropOffsetHolder NewOffset = new() { OffsetPropID = PropID };
                            NewProp.LiftPad1 = GetPadFromSetupFile(AddNewString());
                            NewProp.LiftPad2 = GetPadFromSetupFile(AddNewString());
                            NewProp.LiftPad3 = GetPadFromSetupFile(AddNewString());
                            NewProp.LiftPad4 = GetPadFromSetupFile(AddNewString());
                            NewOffset.LiftDoor1Offset = AddNewHexInt();
                            NewOffset.LiftDoor2Offset = AddNewHexInt();
                            NewOffset.LiftDoor3Offset = AddNewHexInt();
                            NewOffset.LiftDoor4Offset = AddNewHexInt();
                            NewProp.Acceleration = AddNewHexInt();
                            NewProp.MaxSpeed = AddNewHexInt();
                            NewProp.unk84 = AddNewHexInt();
                            Offsets.Add(NewOffset);
                        }
                        else if (ObjectType == "fan")
                        {
                            NewProp.Type = PropType.Fan;
                            NewProp.Acceleration = AddNewHexInt();
                            NewProp.MaxSpeed = AddNewHexInt();
                            int FanOn = AddNewHexInt();
                            NewProp.On = FanOn > 0;
                        }
                        else if (ObjectType == "hovercar")
                        {
                            NewProp.Type = PropType.Hovercar;

                            PropOffsetHolder NewOffset;
                            string AIListName = AddNewString();
                            if (int.TryParse(AIListName[2..], NumberStyles.HexNumber, null, out int AIListID))
                            {
                                NewOffset = new() { OffsetPropID = PropID };
                                NewOffset.AIListID = AIListID;
                                Offsets.Add(NewOffset);
                            }
                            else if (Defines.ContainsKey(AIListName))
                            {
                                NewOffset = new() { OffsetPropID = PropID };
                                NewOffset.AIListID = Defines[AIListName];
                                Offsets.Add(NewOffset);
                            }
                            else
                            {
                                NewProp.AIList = DataHolder.GlobalAILists.Find(x => x.Constant == AIListName);
                            }
                        }
                        else if (ObjectType == "chopper")
                        {
                            NewProp.Type = PropType.Hovercopter;

                            PropOffsetHolder NewOffset;
                            string AIListName = AddNewString();
                            if (int.TryParse(AIListName[2..], NumberStyles.HexNumber, null, out int AIListID))
                            {
                                NewOffset = new() { OffsetPropID = PropID };
                                NewOffset.AIListID = AIListID;
                                Offsets.Add(NewOffset);
                            }
                            else if (Defines.ContainsKey(AIListName))
                            {
                                NewOffset = new() { OffsetPropID = PropID };
                                NewOffset.AIListID = Defines[AIListName];
                                Offsets.Add(NewOffset);
                            }
                            else
                            {
                                NewProp.AIList = DataHolder.GlobalAILists.Find(x => x.Constant == AIListName);
                            }
                        }
                        else if (ObjectType == "mine")
                        {
                            NewProp.Type = PropType.Mine;
                            NewProp.WeaponType = ConstantHolder.GetConstant("WEAPON", AddNewString());
                        }
                        else if (ObjectType == "weapon")
                        {
                            NewProp.Type = PropType.Weapon;
                            NewProp.WeaponType = ConstantHolder.GetConstant("WEAPON", AddNewString());
                        }
                        else if (ObjectType == "stdobject")
                        {
                            NewProp.Type = PropType.StandardObject;
                        }
                        else if (ObjectType == "debris")
                        {
                            NewProp.Type = PropType.Debris;
                        }
                        else if (ObjectType == "ammocratemulti")
                        {
                            NewProp.Type = PropType.AmmoCrateMulti;
                        }
                        else if (ObjectType == "shield")
                        {
                            NewProp.Type = PropType.Shield;
                        }
                        else if (ObjectType == "gasbottle")
                        {
                            NewProp.Type = PropType.GasBottle;
                        }
                        else if (ObjectType == "glass")
                        {
                            NewProp.Type = PropType.Glass;
                        }
                        else if (ObjectType == "safe")
                        {
                            NewProp.Type = PropType.Safe;
                        }
                        else if (ObjectType == "hoverbike")
                        {
                            NewProp.Type = PropType.Hoverbike;
                        }
                        else if (ObjectType == "escastep")
                        {
                            NewProp.Type = PropType.EscalatorStep;
                        }
                        else
                        {
                            // ERROR - incorrect syntax
                            Debug.LogError("Syntax error (unrecognised object type) while reading file at line:");
                            Debug.LogError(CurrentLine);
                            return false;
                        }
                    }

                    Props.Add(NewProp);
                    switch (NewProp.Type)
                    {
                        case PropType.DoorScale:
                        case PropType.LinkGuns:
                        case PropType.LiftDoor:
                        case PropType.Rename:
                        case PropType.TrainingHolographRequirement:
                            break;
                        case PropType.Character:
                            Characters.Add(NewProp);
                            break;
                        case PropType.Tag:
                            Tags.Add(NewProp);
                            break;
                        default:
                            VisibleProps.Add(NewProp);
                            break;
                    }

                    ++PropID;
                }
            }
            else if (HandlingIntro)
            {
                if (LineStartsWith("endintro"))
                {
                    HandlingIntro = false;
                    continue;
                }

                string ObjectType = "";
                if (EndPosition - StartPosition > 0) // Only fill the object type if there's an opening bracket in the line, otherwise, just leave it empty.
                {
                    ObjectType = Line[StartPosition..EndPosition];
                }

                IntroSetting NewIntro = new();
                IntroSettings.Add(NewIntro);

                if (ObjectType == "spawn")
                {
                    NewIntro.Type = IntroType.Spawn;
                    NewIntro.IntroPad = GetPadFromSetupFile(AddNewString());
                }
                else if (ObjectType == "intro_weapon")
                {
                    NewIntro.Type = IntroType.Weapon;
                    NewIntro.WeaponType = ConstantHolder.GetConstant("WEAPON", AddNewString());
                    NewIntro.UnknownWeaponInt = AddNewInt();
                }
                else if (ObjectType == "ammo")
                {
                    NewIntro.Type = IntroType.Ammo;
                    NewIntro.AmmoType = ConstantHolder.GetConstant("AMMOTYPE", AddNewString());
                    NewIntro.AmmoAmount = AddNewInt();
                }
                else if (ObjectType == "outfit")
                {
                    NewIntro.Type = IntroType.PlayerOutfit;
                    NewIntro.Outfit = ConstantHolder.GetConstant("OUTFIT", AddNewString());
                }
                else if (ObjectType == "case")
                {
                    NewIntro.Type = IntroType.Case;
                    NewIntro.CaseID = AddNewInt();
                    NewIntro.IntroPad = GetPadFromSetupFile(AddNewString());
                }
                else if (ObjectType == "case_respawn")
                {
                    NewIntro.Type = IntroType.CaseRespawn;
                    NewIntro.CaseID = AddNewInt();
                    NewIntro.IntroPad = GetPadFromSetupFile(AddNewString());
                }
                else if (ObjectType == "hill")
                {
                    NewIntro.Type = IntroType.Hill;
                    NewIntro.IntroPad = GetPadFromSetupFile(AddNewString());
                }
            }
            else if (HandlingPaths)
            {
                if (LineStartsWith("};"))
                {
                    HandlingPaths = false;
                    continue;
                }

                StartPosition = Line.IndexOf('{') + 1;
                EndPosition = 0;

                for (int i = StartPosition; i < Line.Length; ++i) // skip any spaces at the start of the line
                {
                    if (Line[i] == '\t' || Line[i] == ' ')
                    {
                        continue;
                    }
                    else
                    {
                        StartPosition = i;
                        break;
                    }
                }
                EndPosition = Line.IndexOf(',', StartPosition);

                string PathName = Line[StartPosition..EndPosition];
                if (PathName == "NULL")
                {
                    continue;
                }

                GuardPath Path = Paths.Find(x => x.Name == PathName);

                StartPosition = EndPosition + 1;
                for (int i = StartPosition; i < Line.Length; ++i) // skip any spaces
                {
                    if (Line[i] == '\t' || Line[i] == ' ')
                    {
                        continue;
                    }
                    else
                    {
                        StartPosition = i;
                        break;
                    }
                }
                EndPosition = Line.IndexOf(',', StartPosition);

                int PathIndex = int.Parse(Line[StartPosition..EndPosition]);

                Path.ID = PathIndex;

                StartPosition = EndPosition + 1;
                for (int i = StartPosition; i < Line.Length; ++i) // skip any spaces
                {
                    if (Line[i] == '\t' || Line[i] == ' ')
                    {
                        continue;
                    }
                    else
                    {
                        StartPosition = i;
                        break;
                    }
                }
                EndPosition = Line.IndexOf('}', StartPosition);

                string FlagConstant = Line[StartPosition..EndPosition].TrimEnd(TrimCharacters);

                if (FlagConstant == "PATHFLAG_CIRCULAR")
                {
                    Path.IsCircularPath = true;
                }
                else if (FlagConstant == "PATHFLAG_FLYING")
                {
                    Path.IsFlyingPath = true;
                }
            }
            else if (HandlingPathPads)
            {
                if (LineStartsWith("};"))
                {
                    HandlingPathPads = false;
                    continue;
                }

                EndPosition = Line.IndexOf(',');
                string PathPad = Line[StartPosition..EndPosition];

                if (PathPad != "-1")
                {
                    CurrentPath.AddPad(GetPadFromSetupFile(PathPad));
                }
            }
            else if (HandlingAICommands)
            {
                if (LineStartsWith("};"))
                {
                    HandlingAICommands = false;
                    continue;
                }

                ReadAICommand(CurrentAIList);
            }
            else if (HandlingAILists)
            {
                if (LineStartsWith("};"))
                {
                    HandlingAILists = false;
                    continue;
                }

                ReadAIList();
            }
            else if (HandlingEnum)
            {
                if (LineStartsWith("};"))
                {
                    HandlingEnum = false;
                    continue;
                }

                StartPosition = 0;

                string DefineName;

                if (Line.IndexOf('=') != -1)
                {
                    EndPosition = Line.IndexOf('=') - 1;

                    DefineName = Line[StartPosition..EndPosition];

                    StartPosition = EndPosition + 2;
                    EndPosition = Line.IndexOf(',');

                    string StringValue = HandleVersionsInLine(Line);

                    DefineID = int.Parse(StringValue[2..], NumberStyles.HexNumber);
                }
                else
                {
                    EndPosition = Line.IndexOf(',');

                    if (EndPosition == -1)
                    {
                        EndPosition = Line.Length;
                    }

                    DefineName = Line[StartPosition..EndPosition];
                }

                //Debug.Log("Add define: " + DefineName + " " + DefineID);
                Defines.Add(DefineName, DefineID);
                ++DefineID;
            }
        }

        // Assign all the props to their holding pads (or other props)
        foreach (Prop CurrentProp in Props)
        {
            if (CurrentProp.IsAttachedToCharacter)
            {
                Prop CurrentCharacter = Characters.Find(x => x.CharacterID == CurrentProp.ParentPropID);
                CurrentCharacter.SubProps.Add(CurrentProp);
                CurrentProp.ParentProp = CurrentCharacter;
            }
            else if (CurrentProp.IsAttachedToOtherProp)
            {

            }
            else if (CurrentProp.ParentPad == null) // Prop is not attached to any pad
            {
                CurrentProp.IsGlobal = true;
            }
            else
            {
                Pad MatchingPad = CurrentProp.ParentPad;
                MatchingPad.AddProp(CurrentProp);
            }
        }

        foreach (PropOffsetHolder CurrentOffset in Offsets)
        {
            Prop CurrentProp = Props[CurrentOffset.OffsetPropID];

            if (CurrentProp.Type == PropType.ConditionalScenery)
            {
                if (CurrentOffset.TriggerPropOffset != 0) { CurrentProp.TriggerProp = Props[CurrentProp.InternalID + CurrentOffset.TriggerPropOffset]; }
                if (CurrentOffset.UnexplodedPropOffset != 0) { CurrentProp.UnexplodedProp = Props[CurrentProp.InternalID + CurrentOffset.UnexplodedPropOffset]; }
                if (CurrentOffset.ExplodedPropOffset != 0) { CurrentProp.ExplodedProp = Props[CurrentProp.InternalID + CurrentOffset.ExplodedPropOffset]; }
            }
            else if (CurrentProp.Type == PropType.BlockedPath)
            {
                CurrentProp.PathBlockingProp = Props[CurrentProp.InternalID + CurrentOffset.PathBlockingPropOffset];
            }
            else if (CurrentProp.Type == PropType.Door)
            {
                if (CurrentOffset.LinkedDoorOffset != 0) { CurrentProp.LinkedDoor = Props[CurrentProp.InternalID + CurrentOffset.LinkedDoorOffset]; }
            }
            else if (CurrentProp.Type == PropType.Weapon)
            {
                Prop Weapon1 = Props[CurrentProp.InternalID + CurrentOffset.LinkedGunOffset1];
                Prop Weapon2 = Props[CurrentProp.InternalID + CurrentOffset.LinkedGunOffset2];

                Weapon1.LinkedWeapon = Weapon2;
                Weapon2.LinkedWeapon = Weapon1;
            }
            else if (CurrentProp.Type == PropType.Lift)
            {
                if (CurrentOffset.LiftDoor1Offset != 0) { CurrentProp.LiftDoor1 = Props[CurrentProp.InternalID + CurrentOffset.LiftDoor1Offset]; }
                if (CurrentOffset.LiftDoor2Offset != 0) { CurrentProp.LiftDoor2 = Props[CurrentProp.InternalID + CurrentOffset.LiftDoor2Offset]; }
                if (CurrentOffset.LiftDoor3Offset != 0) { CurrentProp.LiftDoor3 = Props[CurrentProp.InternalID + CurrentOffset.LiftDoor3Offset]; }
                if (CurrentOffset.LiftDoor4Offset != 0) { CurrentProp.LiftDoor4 = Props[CurrentProp.InternalID + CurrentOffset.LiftDoor4Offset]; }
            }
            else if (CurrentProp.Type == PropType.LiftDoor)
            {
                Prop Lift = Props[CurrentProp.InternalID + CurrentOffset.LiftStopLiftOffset];
                Lift.LiftStops.Add(CurrentProp);

                CurrentProp.LiftStopLift = Lift;
                CurrentProp.LiftStopDoor = Props[CurrentProp.InternalID + CurrentOffset.LiftStopDoorOffset];
            }
            else if (CurrentProp.Type == PropType.Tag)
            {
                Prop TaggedProp = Props[CurrentProp.InternalID + CurrentOffset.TaggedPropOffset];

                CurrentProp.TaggedProp = TaggedProp;
                TaggedProp.AttachedTag = CurrentProp;
            }
            else if (CurrentProp.Type == PropType.Rename)
            {
                Prop ObjectWithNewName = Props[CurrentProp.InternalID + CurrentOffset.RenameObjectOffset];

                ObjectWithNewName.RenameObject = CurrentProp;
                CurrentProp.ParentProp = ObjectWithNewName;
            }
            else if (CurrentProp.Type == PropType.Character || CurrentProp.Type == PropType.Hovercar || CurrentProp.Type == PropType.Hovercopter)
            {
                if (CurrentProp.Type == PropType.Character && CurrentOffset.ChairTagID != -1)
                {
                    Prop Tag = Tags.Find(x => x.TagID == CurrentOffset.ChairTagID);

                    CurrentProp.Chair = Tag;
                    Tag.ConnectedCharacter = CurrentProp;
                }

                if (CurrentOffset.AIListID != 0)
                {
                    AIList CharacterAIList = AILists.Find(x => x.ID == CurrentOffset.AIListID);

                    CharacterAIList.ConnectedProps.Add(CurrentProp);
                    CurrentProp.AIList = CharacterAIList;
                }
            }
        }

        foreach (AIListOffsetHolder CurrentOffset in AIListOffsets)
        {
            AICommandInstance CurrentCommandInstance = CurrentOffset.CommandInstance;

            if (CurrentOffset.OffsetType == AIListOffsetHolder.AIListOffsetType.AIList)
            {
                string AIListString = (string)CurrentCommandInstance.Parameters[CurrentOffset.ParameterIndex];
                if (int.TryParse(AIListString, NumberStyles.AllowHexSpecifier, null, out int AIListID))
                {
                    CurrentCommandInstance.Parameters[CurrentOffset.ParameterIndex] = AILists.Find(x => x.ID == AIListID);
                }
                else if (Defines.ContainsKey(AIListString))
                {
                    CurrentCommandInstance.Parameters[CurrentOffset.ParameterIndex] = AILists.Find(x => x.ID == Defines[AIListString]);
                }
                else
                {
                    CurrentCommandInstance.Parameters[CurrentOffset.ParameterIndex] = DataHolder.GlobalAILists.Find(x => x.Constant == AIListString);
                }
            }
            else if (CurrentOffset.OffsetType == AIListOffsetHolder.AIListOffsetType.Path)
            {
                int PathID = (int)CurrentCommandInstance.Parameters[CurrentOffset.ParameterIndex];

                CurrentCommandInstance.Parameters[CurrentOffset.ParameterIndex] = Paths.Find(x => x.ID == PathID);
            }

        }

        // Assign all the tags to the objective conditions that refer to those tags
        foreach (Objective IteratedObjective in Objectives)
        {
            foreach (ObjectiveCondition IteratedCondition in IteratedObjective.Conditions)
            {
                switch (IteratedCondition.Type)
                {
                    case ConditionType.DestroyObject:
                    case ConditionType.CollectObject:
                    case ConditionType.DepositObject:
                    case ConditionType.HolographObject:
                        IteratedCondition.AddNewTag(Tags.Find(x => x.TagID == IteratedCondition.TagID));
                        break;
                    default:
                        continue;
                }
            }
        }

        // Make new ID's for all the tags, since in some Rare stages there are gaps
        for (int i = 0; i < Tags.Count; ++i)
        {
            Tags[i].TagID = i;
        }

        AILists.RemoveAll(x => x.ID == 0); // Remove all unregistered AI lists
        AILists.Sort((x, y) => x.ID.CompareTo(y.ID));

        return true;
    }
    
    // Returns a list with all the file names/paths. The indexes correspond to the file number that is used by the decomp project. This gets called when opening a project.
    public List<string> GetFileData(string SourcePath)
    {
        FileLines = File.ReadAllLines(SourcePath + "\\assets\\ntsc-final\\files\\list.c");

        List<string> FileNames = new();
        FileNames.Add(""); // Since the list of file name is not zero-indexed, add a dummy item to slot 0, so the array index matches up to the file number.

        foreach (string CurrentLine in FileLines)
        {
            Line = CurrentLine;

            StartPosition = CurrentLine.IndexOf('"');
            if (StartPosition == -1) 
            {
                continue;
            }
            else
            {
                StartPosition += 1;
            }
            EndPosition = CurrentLine.IndexOf('"', StartPosition);

            string FileName = "";
            if (CurrentLine[StartPosition] == 'A') // Voice line
            {
                FileName = CurrentLine.Substring(StartPosition + 1, EndPosition - StartPosition - 2);
                FileName += ".mp3";
            }
            else if (CurrentLine[StartPosition] == 'C' || CurrentLine[StartPosition] == 'G' || CurrentLine[StartPosition] == 'P') // 3D model
            {
                FileName = CurrentLine.Substring(StartPosition + 1, EndPosition - StartPosition - 2);
                FileName += ".bin";
            }
            else if (CurrentLine[StartPosition] == 'b')
            {
                FileName = CurrentLine[StartPosition..EndPosition];
                if (FileName[^3..] == "seg") // BG data
                {
                    StartPosition = Line.IndexOf("/", StartPosition) + 1;

                    FileName = CurrentLine[StartPosition..EndPosition];
                    //Debug.Log("BG: " + FileName);
                }
                else if (FileName[^6..] == "tilesZ" || FileName[^5..] == "padsZ") // Collision or pads
                {
                    StartPosition = Line.IndexOf('_') + 1;
                    EndPosition = Line.IndexOf('_', StartPosition);

                    FileName = CurrentLine[StartPosition..EndPosition] + ".json";
                    //Debug.Log("Tiles: " + FileName);
                }
            }
            else if (CurrentLine[StartPosition] == 'U') // Setup
            {
                FileName = CurrentLine[StartPosition..EndPosition];
            }
            else if (CurrentLine[StartPosition] == 'L') // In-game text
            {
                if (CurrentLine.IndexOf('_') == -1)
                {
                    FileName = CurrentLine.Substring(StartPosition + 1, EndPosition - StartPosition - 2);
                    FileName += ".json";
                }
                else
                {
                    FileName = CurrentLine.Substring(StartPosition + 1, CurrentLine.IndexOf('_') - StartPosition - 1);
                    //Debug.Log("Other?: " + FileName);
                }
            }
            FileNames.Add(FileName);
        }

        return FileNames;
    }

    public void GetFileNumbersAndPropData(string SourcePath)
    {
        FileLines = File.ReadAllLines(SourcePath + "\\include\\files.h");

        foreach (string CurrentLine in FileLines)
        {
            StartPosition = 0;
            EndPosition = 0;

            Line = CurrentLine;

            if (LineStartsWith("#d"))
            {
                Tuple<string, int> Define = GetDefineOld();
                if (Define.Item1 == "NUM_FILES")
                {
                    continue;
                }
                FileNumbers.Add(Define.Item1, Define.Item2);
            }
        }

        FileLines = File.ReadAllLines(SourcePath + "\\game\\lang.c");

        bool HandlingLanguageNumbers = false;

        foreach (string CurrentLine in FileLines)
        {
            StartPosition = 0;
            EndPosition = 0;

            Line = CurrentLine;

            if (HandlingLanguageNumbers)
            {
                if (LineStartsWith("};"))
                {
                    break;
                }

                StartPosition = Line.IndexOf("*/") + 3;
                EndPosition = Line.IndexOf(',');

                LanguageFileConstants.Add(Line[StartPosition..EndPosition]);
            }
            else if (LineStartsWith("u16 g_LangFiles[] = {"))
            {
                HandlingLanguageNumbers = true;
            }
        }

        FileLines = File.ReadAllLines(SourcePath + "\\game\\modeldata\\general.c");

        bool HandlingFileNames = false;
        bool HandlingExplosionTypes = false;
        int NumProps = 0;
        foreach (string CurrentLine in FileLines)
        {
            Line = CurrentLine;

            StartPosition = 0;
            EndPosition = 0;

            if (LineStartsWith("struct modelstate"))
            {
                HandlingFileNames = true;
            }
            else if (LineStartsWith("s8 g_PropExplosionTypes[]"))
            {
                HandlingExplosionTypes = true;
            }
            else if (HandlingFileNames)
            {
                if (LineStartsWith("#if") || LineStartsWith("#else") || LineStartsWith("#endif"))
                {
                    HandleVersions();
                    continue;
                }
                else if (!ApplicableVersion || LineStartsWith("//"))
                {
                    continue;
                }
                else if (LineStartsWith("};")) // At the end of the file name list
                {
                    HandlingFileNames = false;
                }
                /*else if (CurrentLine.IndexOf('{') == -1) // Not a prop data line
                {
                    Debug.Log("Not a prop data line");
                    continue;
                }*/
                else
                {
                    ModelData NewPropData = new();
                    StartPosition = CurrentLine.IndexOf(',') + 1;
                    EndPosition = CurrentLine.IndexOf(',', StartPosition);
                    NewPropData.FileName = CurrentLine[StartPosition..EndPosition].TrimStart(TrimCharacters);

                    StartPosition = CurrentLine.IndexOf('x', EndPosition) + 1;
                    EndPosition = CurrentLine.IndexOf('}');
                    NewPropData.Scale = int.Parse(CurrentLine[StartPosition..EndPosition], NumberStyles.HexNumber);

                    NewPropData.FileNumber = FileNumbers[NewPropData.FileName];
                    NewPropData.FilePath = DataHolder.FileNames[NewPropData.FileNumber];

                    PropDataList.Add(NewPropData);
                }
            }
            else if (HandlingExplosionTypes)
            {
                if (PropDataList.Count == NumProps)
                {
                    break;
                }
                if (LineStartsWith("};")) // At the end of the file name list
                {
                    HandlingExplosionTypes = false;
                }
                else if (LineStartsWith("#") || CurrentLine.IndexOf('E') == -1) // Not a prop data line
                {
                    continue;
                }
                else
                {
                    StartPosition = CurrentLine.IndexOf('E');
                    EndPosition = CurrentLine.IndexOf(',');
                    PropDataList[NumProps].ExplosionType = CurrentLine[StartPosition..EndPosition];
                    ++NumProps;
                }
            }
        }

        FileLines = File.ReadAllLines(SourcePath + "\\game\\modeldata\\robot.c");

        bool HandlingBodies = false;

        foreach (string CurrentLine in FileLines)
        {
            Line = CurrentLine;

            StartPosition = 0;
            EndPosition = 0;

            if (LineStartsWith("#if") || LineStartsWith("#else") || LineStartsWith("#endif"))
            {
                HandleVersions();
                continue;
            }
            else if (!ApplicableVersion || LineStartsWith("/"))
            {
                continue;
            }
            else if (HandlingBodies)
            {
                if (LineStartsWith("};"))
                {
                    HandlingBodies = false;
                    break;
                }

                ModelData NewCharacterData = new();

                StartPosition = Line.IndexOf(',') - 1;
                NewCharacterData.IsMale = Line[StartPosition].ToString().Trim(TrimCharacters) == "1";

                StartPosition = Line.IndexOf(',') + 1;
                EndPosition = Line.IndexOf(',', StartPosition);
                NewCharacterData.IsHead = Line[StartPosition..EndPosition].Trim(TrimCharacters) == "1";

                StartPosition = EndPosition + 1;
                EndPosition = Line.IndexOf(',', StartPosition);
                NewCharacterData.HeightCanVary = Line[StartPosition..EndPosition].Trim(TrimCharacters) == "1";

                StartPosition = EndPosition + 1;
                EndPosition = Line.IndexOf(',', StartPosition);
                NewCharacterData.BodyType = Line[StartPosition..EndPosition].Trim(TrimCharacters);

                StartPosition = EndPosition + 1;
                EndPosition = Line.IndexOf(',', StartPosition);
                NewCharacterData.Height = int.Parse(Line[StartPosition..EndPosition].Trim(TrimCharacters));

                StartPosition = EndPosition + 1;
                EndPosition = Line.IndexOf(',', StartPosition);
                NewCharacterData.FileName = Line[StartPosition..EndPosition].Trim(TrimCharacters);
                NewCharacterData.FileName = Line[StartPosition..EndPosition].Trim(TrimCharacters);

                StartPosition = EndPosition + 1;
                EndPosition = Line.IndexOf(',', StartPosition);
                NewCharacterData.CharacterScale = float.Parse(Line[StartPosition..EndPosition].Trim(TrimCharacters));

                StartPosition = EndPosition + 1;
                EndPosition = Line.IndexOf(',', StartPosition);
                NewCharacterData.AnimationScale = float.Parse(Line[StartPosition..EndPosition].Trim(TrimCharacters));

                StartPosition = EndPosition + 1;
                EndPosition = Line.IndexOf(',', StartPosition);
                NewCharacterData.ModelDef = int.Parse(Line[StartPosition..EndPosition].Trim(TrimCharacters));

                StartPosition = EndPosition + 1;
                EndPosition = Line.IndexOf(',', StartPosition);
                NewCharacterData.HandFileName = Line[StartPosition..EndPosition].Trim(TrimCharacters);

                if (NewCharacterData.FileName != "0")
                {
                    NewCharacterData.FileNumber = FileNumbers[NewCharacterData.FileName];
                    NewCharacterData.FilePath = DataHolder.FileNames[NewCharacterData.FileNumber];
                }

                CharacterDataList.Add(NewCharacterData);
            }
            else if (LineStartsWith("struct headorbody g_HeadsAndBodies[] = {"))
            {
                HandlingBodies = true;
            }

        }
    }

    public void GetImportantNumbers(string SourcePath)
    {
        FileLines = File.ReadAllLines(SourcePath + "\\include\\constants.h");

        ConstantHolder.AddConstantList("GAILIST");

        foreach (string CurrentLine in FileLines)
        {
            if (string.IsNullOrWhiteSpace(CurrentLine))
            {
                continue;
            }

            Line = CurrentLine.TrimStart(TrimCharacters);
            StartPosition = 0;
            EndPosition = 0;

            if (LineStartsWith("#define GAILIST_"))
            {
                ConstantHolder.AddConstant("GAILIST", GetDefine());
            }
            else if (LineStartsWith("#define MODEL_") || LineStartsWith("#define BODY_") || LineStartsWith("#define HEAD_"))
            {
                Constant ModelNameAndNumber = GetDefine();
                ModelNumbers.Add(ModelNameAndNumber.Name, ModelNameAndNumber.Value);
            }
        }
    }

    /*public void GetConstants(string SourcePath)
    {
        FileLines = File.ReadAllLines(SourcePath + "\\include\\constants.h");

        bool HandlingWeapons = false;
        int WeaponNumber = 0;

        foreach (string CurrentLine in FileLines)
        {
            Line = CurrentLine;
            StartPosition = 0;
            EndPosition = 0;

            // Weapon types are not a define, but an enum, so we have to process them a different way
            if (HandlingWeapons == true)
            {
                if (LineStartsWith("};"))
                {
                    HandlingWeapons = false;
                }
                else if (LineStartsWith("/"))
                {
                    Constant Define = new();
                    if (Line.IndexOf(',') == -1)
                    {
                        Define.Name = Line[Line.IndexOf('W')..];
                    }
                    else
                    {
                        Define.Name = Line[Line.IndexOf('W')..Line.IndexOf(',')];
                    }
                    Define.Value = WeaponNumber;
                    ++WeaponNumber;

                    //Debug.Log("New define: " + Define.Name + " " + Define.Value + " " + Define.DisplayName);
                    ConstantHolder.WeaponTypes.Add(Define);
                }
            }
            else
            {
                if (LineStartsWith("#define DOORTYPE"))
                {
                    ConstantHolder.DoorTypes.Add(GetDefine());
                }
                else if (LineStartsWith("#define AMMOTYPE"))
                {
                    ConstantHolder.AmmoTypes.Add(GetDefine());
                }
                else if (LineStartsWith("#define TEAM"))
                {
                    ConstantHolder.Teams.Add(GetDefine());
                }
                else if (LineStartsWith("#define SQUADRON"))
                {
                    ConstantHolder.Squadrons.Add(GetDefine());
                }
                else if (LineStartsWith("#define PADEFFECT"))
                {
                    ConstantHolder.Effects.Add(GetDefine());
                }
                else if (LineStartsWith("#define OUTFIT"))
                {
                    ConstantHolder.Outfits.Add(GetDefine());
                }
                else if (LineStartsWith("#define OBJFLAG_"))
                {
                    GameBitFlagHolder.ObjectFlags1.Add(GetBitFlag());
                }
                else if (LineStartsWith("#define OBJFLAG2"))
                {
                    GameBitFlagHolder.ObjectFlags2.Add(GetBitFlag());
                }
                else if (LineStartsWith("#define OBJFLAG3"))
                {
                    GameBitFlagHolder.ObjectFlags3.Add(GetBitFlag());
                }
                else if (LineStartsWith("#define CHRFLAG0"))
                {
                    GameBitFlagHolder.CharacterFlags1.Add(GetBitFlag());
                }
                else if (LineStartsWith("#define CHRFLAG1"))
                {
                    GameBitFlagHolder.CharacterFlags2.Add(GetBitFlag());
                }
                else if (LineStartsWith("#define SPAWNFLAG"))
                {
                    GameBitFlagHolder.SpawnFlags.Add(GetBitFlag());
                }
                else if (LineStartsWith("#define DOORFLAG"))
                {
                    GameBitFlagHolder.DoorFlags.Add(GetBitFlag());
                }
                else if (LineStartsWith("#define PATHFLAG"))
                {
                    GameBitFlagHolder.PathFlags.Add(GetBitFlag());
                }
                else if (LineStartsWith("#define PORTALFLAG"))
                {
                    GameBitFlagHolder.PortalFlags.Add(GetBitFlag());
                }
                else if (LineStartsWith("enum weaponnum"))
                {
                    HandlingWeapons = true;
                }
            }
        }
    }*/

    public class ConstantProperties
    {
        public string name;
        public int value;
        public string displayname;
        public string description;
    };

    public class ConstantListProperties
    {
        public string name;
        public string type;
        public int size;
        public List<ConstantProperties> constants;
    }

    /*public void GetConstants(string SourcePath)
    {
        FileLines = File.ReadAllLines(SourcePath + "\\include\\constants.h");

        List<(string, string)> ConstantTypes = new()
        {
            ( "OBJFLAG", "BitFlag" ),
            ( "OBJFLAG2", "BitFlag" ),
            ( "OBJFLAG3", "BitFlag" ),
            ( "DOORFLAG", "BitFlag" ),
            ( "CHRFLAG0", "BitFlag" ),
            ( "CHRFLAG1", "BitFlag" ),
            ( "SPAWNFLAG", "BitFlag" ),
            ( "PATHFLAG", "BitFlag" ),
            ( "PORTALFLAG", "BitFlag" ),
            ( "CHRANIMFLAG", "BitFlag" ),
            ( "ATTACKFLAG", "BitFlag" ),
            ( "CHRCFLAG", "BitFlag" ),
            ( "CHRHFLAG", "BitFlag" ),
            ( "COVERCRITERIA", "BitFlag" ),
            ( "ROOMFLAG", "BitFlag" ),
            ( "PSFLAG", "BitFlag" ),

            ( "DOORTYPE", "Constant" ),
            ( "AMMOTYPE", "Constant" ),
            ( "TEAM", "Constant" ),
            ( "SQUADRON", "Constant" ),
            ( "PADEFFECT", "Constant" ),
            ( "OUTFIT", "Constant" ),
            ( "WEAPON", "Constant" ),
            ( "HITPART", "Constant" ),
            ( "GOPOSFLAG", "Constant" ),
            ( "DOORSTATE", "Constant" ),
            ( "QUADRANT", "Constant" ),
            ( "DIFF", "Constant" ),
            ( "OPERATOR", "Constant" ),
            ( "COLOR", "Constant" ),
            ( "CHANNEL", "Constant" ),
            ( "XREASON", "Constant" ),
            ( "LIGHTOP", "Constant" ),
            ( "QUIP", "Constant" ),
            ( "MA", "Constant" ),
            ( "MODELPART", "Constant" ),
            ( "GAMEFILEFLAG", "Constant" ),
            ( "SPECIALDIE", "Constant" ),
            ( "CIQUIP", "Constant" ),
            ( "AIENVCMD", "Constant" ),
            ( "IDLEACTION", "Constant" )
        };

        //ConstantTypes.Sort((x, y) => x.CompareTo(y));

        int Index = 0;
        string CurrentConstantType = ConstantTypes[Index].Item1;

        List<ConstantListProperties> ConstantList = new();

        foreach ((string, string) con in ConstantTypes)
        {
            ConstantList.Add(new ConstantListProperties() {
                name = con.Item1,
                type = con.Item2,
                constants = new List<ConstantProperties>()
            });
        }

        foreach (string CurrentLine in FileLines)
        {
            Line = CurrentLine.TrimStart(TrimCharacters);
            StartPosition = 0;
            EndPosition = 0;

            if (!LineStartsWith("#define"))
            {
                continue;
            }

            StartPosition = Line.IndexOf(' ');
            EndPosition = Line.IndexOf('_');

            if (StartPosition == -1 || EndPosition == -1)
            {
                continue;
            }

            ++StartPosition;

            string ReadConstantType = Line[StartPosition..EndPosition];

            if (ReadConstantType != CurrentConstantType)
            {
                if (ConstantTypes.FindIndex(x => x.Item1 == ReadConstantType) == -1)
                {
                    //Debug.Log("Constant type not recognized");
                    continue;
                }
                else
                {
                    //Debug.Log("Switch to new constant type");
                    CurrentConstantType = ReadConstantType;
                }
            }

            EndPosition = Line.IndexOf(' ', StartPosition);
            string ConstantName = Line[StartPosition..EndPosition];

            StartPosition = EndPosition + 1;
            SkipWhitespace();
            EndPosition = Line.IndexOf(' ', StartPosition);
            if (EndPosition == -1) { EndPosition = Line.Length; }


            int BitFlagValue;
            string NumberString = Line[StartPosition..EndPosition];

            if (NumberString.StartsWith("0x"))
            {
                BitFlagValue = int.Parse(NumberString[2..], NumberStyles.HexNumber);
            }
            else
            {
                BitFlagValue = int.Parse(NumberString, NumberStyles.HexNumber);
            }

            //BitFlag CurrentBitFlag = GameBitFlagHolder.GetBitFlag(CurrentConstantType, ConstantName);
            //CurrentBitFlag.Bits = BitFlagValue;

            ConstantProperties NewProperties = new()
            {
                name = ConstantName,
                value = BitFlagValue,
            };

            ConstantList.Find(x => x.name == CurrentConstantType).constants.Add(NewProperties);
        }

        string FileText = JsonConvert.SerializeObject(ConstantList, Formatting.Indented);

        FileStream JSONStream = new("C:\\Users\\Oofjay\\Desktop\\Temp\\constants.json", FileMode.Create, FileAccess.Write);
        StreamWriter FileWriter = new(JSONStream);
        FileWriter.Write(FileText);

        FileWriter.Dispose();
        JSONStream.Dispose();
    }*/

    /*
    public void GetAICommands(string SourcePath)
    {
        FileLines = File.ReadAllLines(SourcePath + "\\include\\commands.h");

        List<AICommandProperties> AICommands = new();

        AICommandProperties CurrentProperties = null;
        bool HandlingProperties = false;

        foreach (string CurrentLine in FileLines)
        {
            Line = CurrentLine;
            StartPosition = 0;
            EndPosition = 0;

            if (LineStartsWith("#define"))
            {
                if (LineStartsWith("#define beginloop"))
                {
                    break;
                }

                bool HandlingParameters = true;

                CurrentProperties = new AICommandProperties();
                AICommands.Add(CurrentProperties);

                HandlingProperties = true;

                StartPosition = 8;
                if (Line.IndexOf('(') == -1)
                {
                    EndPosition = Line.IndexOf(' ', StartPosition);
                    HandlingParameters = false;
                }
                else
                {
                    EndPosition = Line.IndexOf('(');
                }

                CurrentProperties.name = Line[StartPosition..EndPosition];

                StartPosition = EndPosition + 1;

                CurrentProperties.parameters = new();

                while(HandlingParameters)
                {
                    if (Line.IndexOf(',', StartPosition) != -1)
                    {
                        EndPosition = Line.IndexOf(',', StartPosition);
                    }
                    else
                    {
                        EndPosition = Line.IndexOf(')', StartPosition);
                        HandlingParameters = false;
                    }

                    CurrentProperties.parameters.Add(Line[StartPosition..EndPosition]);

                    StartPosition = EndPosition + 1;
                }
            }
            else if (HandlingProperties)
            {
                StartPosition = Line.IndexOf('x') + 1;
                EndPosition = Line.IndexOf(')');


                Debug.Log(Line);
                Debug.Log(StartPosition + " " + EndPosition);
                CurrentProperties.id = int.Parse(Line[StartPosition..EndPosition], NumberStyles.HexNumber);
                HandlingProperties = false;
            }
        }

        string FileText = JsonConvert.SerializeObject(AICommands, Formatting.Indented);

        FileStream JSONStream = new("C:\\Users\\Oofjay\\Desktop\\Temp\\aicommands.json", FileMode.Create, FileAccess.Write);
        StreamWriter FileWriter = new(JSONStream);
        FileWriter.Write(FileText);

        FileWriter.Dispose();
        JSONStream.Dispose();
    }*/
    

    public void GetConstants(string SourcePath)
    {
        string FileText = File.ReadAllText(Application.streamingAssetsPath + "\\constants.json");

        List<ConstantListProperties> PropertyList = JsonConvert.DeserializeObject<List<ConstantListProperties>>(FileText);

        foreach (ConstantListProperties CurrentListProperties in PropertyList)
        {
            if (CurrentListProperties.type == "BitFlag")
            {
                List<BitFlag> CurrentBitFlagList = GameBitFlagHolder.AddBitFlagList(CurrentListProperties.name);
                GameBitFlagHolder.BitFlagSizes.Add(CurrentListProperties.name, CurrentListProperties.size);

                foreach (ConstantProperties CurrentBitFlagProperties in CurrentListProperties.constants)
                {
                    CurrentBitFlagList.Add(new BitFlag()
                    {
                        Constant = CurrentBitFlagProperties.name,
                        Bits = CurrentBitFlagProperties.value,
                        Name = CurrentBitFlagProperties.displayname,
                        Description = CurrentBitFlagProperties.description
                    });
                }
            }
            else // Regular constant
            {
                List<Constant> CurrentConstantList = ConstantHolder.AddConstantList(CurrentListProperties.name);
                ConstantHolder.ConstantSizes.Add(CurrentListProperties.name, CurrentListProperties.size);

                foreach (ConstantProperties CurrentConstantProperties in CurrentListProperties.constants)
                {
                    CurrentConstantList.Add(new Constant()
                    {
                        Name = CurrentConstantProperties.name,
                        Value = CurrentConstantProperties.value,
                        DisplayName = CurrentConstantProperties.displayname,
                        Description = CurrentConstantProperties.description
                    });
                }
            }
        }

        //Debug.Log(GameBitFlagHolder.BitFlagCollection.Count + " " + ConstantHolder.ConstantCollection.Count);
    }

    public List<AICommand> GetAICommands(string SourcePath)
    {
        string FileText = File.ReadAllText(Application.streamingAssetsPath + "\\aicommands.json");
        List<AICommandProperties> AIPropertyList = JsonConvert.DeserializeObject<List<AICommandProperties>>(FileText);

        List<AICommand> AICommands = new();

        foreach(AICommandProperties CurrentCommand in AIPropertyList)
        {
            AICommand NewCommand = new()
            {
                ID = CurrentCommand.id,
                Name = CurrentCommand.name,
                DisplayName = CurrentCommand.displayname,
                Description = CurrentCommand.description
            };

            foreach(ParameterProperties CurrentParameter in CurrentCommand.parameters)
            {
                ParameterType NewParameter = new()
                {
                    ClassType = CurrentParameter.type,
                    Name = CurrentParameter.name,
                    ConstantName = CurrentParameter.constantname
                };

                NewCommand.ParameterTypes.Add(NewParameter);
                //Debug.Log(Type.GetType(CurrentParameter));
            }

            AICommands.Add(NewCommand);
        }

        return AICommands;
    }

    public void GetGlobalAILists(string SourcePath)
    {
        FileLines = File.ReadAllLines(SourcePath + "\\game\\gailists.c");

        bool HandlingAILists = false;
        bool HandlingAICommands = false;
        bool HandlingDefineAIList = false;

        AIList CurrentAIList = null;

        foreach (string CurrentLine in FileLines)
        {
            StartPosition = 0;
            EndPosition = 0;
            Line = CurrentLine.TrimStart(TrimCharacters);

            if (string.IsNullOrWhiteSpace(CurrentLine) || LineStartsWith("/") || LineStartsWith("*"))
            {
                continue;
            }

            if (LineStartsWith("#if") || LineStartsWith("#else") || LineStartsWith("#endif"))
            {
                HandleVersions();
                continue;
            }
            else if (!ApplicableVersion)
            {
                continue;
            }
            else if (HandlingDefineAIList)
            {
                if (Line[^1] != '\\')
                {
                    HandlingDefineAIList = false;
                }
                continue;
            }
            else if (LineStartsWith("#define"))
            {
                if (Line[^1] == '\\')
                {
                    HandlingDefineAIList = true;
                    continue;
                }

                Tuple<string, int> Define = GetDefineOld();
                Defines.Add(Define.Item1, Define.Item2);
                continue;
            }
            else if (LineStartsWith("u8"))
            {
                HandlingAICommands = true;

                StartPosition = Line.IndexOf(' ') + 1;
                EndPosition = Line.IndexOf('[');

                CurrentAIList = new() { Name = Line[StartPosition..EndPosition] };
                CurrentAIList.DisplayName = CurrentAIList.Name;
                AILists.Add(CurrentAIList);
                continue;
            }
            else if (LineStartsWith("struct ailist"))
            {
                HandlingAILists = true;
                continue;
            }
            else if (HandlingAICommands)
            {
                if (LineStartsWith("};"))
                {
                    HandlingAICommands = false;
                    continue;
                }

                UpdateSeekPositions();

                ReadAICommand(CurrentAIList);
            }
            else if (HandlingAILists)
            {
                if (LineStartsWith("};"))
                {
                    HandlingAILists = false;
                    continue;
                }

                ReadGlobalAIList();
            }
        }

        foreach (AIListOffsetHolder CurrentOffset in AIListOffsets)
        {
            AICommandInstance CurrentCommandInstance = CurrentOffset.CommandInstance;

            string AIListString = (string)CurrentCommandInstance.Parameters[CurrentOffset.ParameterIndex];
            if (int.TryParse(AIListString, NumberStyles.AllowHexSpecifier, null, out int AIListID))
            {
                CurrentCommandInstance.Parameters[CurrentOffset.ParameterIndex] = AILists.Find(x => x.ID == AIListID);
            }
            else
            {
                CurrentCommandInstance.Parameters[CurrentOffset.ParameterIndex] = AILists.Find(x => x.Constant == AIListString);
            }

        }

        AILists.RemoveAll(x => x.Constant == null); // Remove all unregistered AI lists
        AILists.Sort((x, y) => x.ID.CompareTo(y.ID));
    }

    public List<AnimationSequence> GetAnimations(string SourcePath)
    {
        string FileText = File.ReadAllText(SourcePath + "\\assets\\ntsc-final\\animations.json");
        List<AnimationProperties> AnimationPropertyList = JsonConvert.DeserializeObject<List<AnimationProperties>>(FileText);

        List<AnimationSequence> AnimationList = new();

        foreach (AnimationProperties CurrentAnimation in AnimationPropertyList)
        {
            AnimationSequence NewAnimation = new()
            {
                Name = CurrentAnimation.id,
                FileName = CurrentAnimation.file,
                NumberOfFrames = CurrentAnimation.numframes,
                BytesPerFrame = CurrentAnimation.bytesperframe,
                Unknown08 = CurrentAnimation.unk08,
                Unknown0A = CurrentAnimation.unk0a,
                Loops = CurrentAnimation.flag01,
                HasAbsoluteTranslation = CurrentAnimation.flag02,
                HasGuardRepeatPoints = CurrentAnimation.flag04,
                HasCameraRepeatPoints = CurrentAnimation.flag08
            };

            AnimationList.Add(NewAnimation);
        }

        return AnimationList;
    }

    public List<SoundEffect> GetSoundEffects(string SourcePath)
    {
        FileLines = File.ReadAllLines(SourcePath + "\\include\\sfx.h");

        List<SoundEffect> SoundEffects = new();

        bool HandlingWavs = false;
        int SoundID = 0;

        foreach (string CurrentLine in FileLines)
        {
            StartPosition = 0;
            EndPosition = 0;
            Line = CurrentLine.TrimStart(TrimCharacters);

            if (string.IsNullOrWhiteSpace(Line) || LineStartsWith("//"))
            {
                continue;
            }
            else if (LineStartsWith("#if") || LineStartsWith("#else") || LineStartsWith("#endif"))
            {
                HandleVersions();
                continue;
            }
            else if (!ApplicableVersion)
            {
                continue;
            }
            else if (HandlingWavs)
            {
                if (Line == "};")
                {
                    HandlingWavs = false;
                    continue;
                }

                string SoundName;

                if (Line.IndexOf('=') == -1)
                {
                    EndPosition = Line.IndexOf(',', StartPosition);
                    if (EndPosition == -1) { EndPosition = Line.Length; }
                    SoundName = Line[..EndPosition];
                }
                else
                {
                    EndPosition = Line.IndexOf(' ', StartPosition);
                    SoundName = Line[..EndPosition];

                    StartPosition = Line.IndexOf('=', EndPosition) + 4;
                    EndPosition = Line.IndexOf(',', StartPosition);
                    SoundID = int.Parse(Line[StartPosition..EndPosition], NumberStyles.HexNumber);
                }

                SoundEffect NewSoundEffect = new()
                {
                    Name = SoundName,
                    ID = SoundID,
                    IsMP3 = false
                };

                SoundEffects.Add(NewSoundEffect);
                ++SoundID;
            }
            else if (LineStartsWith("enum sfx {"))
            {
                HandlingWavs = true;
            }
        }

        return SoundEffects;
    }

    public List<MusicTrack> GetMusicTracks(string SourcePath)
    {
        string FileText = File.ReadAllText(SourcePath + "\\assets\\pal-final\\sequences.json");
        List<MusicTrackProperties> PropertyList = JsonConvert.DeserializeObject<List<MusicTrackProperties>>(FileText);
        List<MusicTrack> MusicTracks = new();

        foreach (MusicTrackProperties CurrentProperties in PropertyList)
        {
            MusicTracks.Add(new MusicTrack()
            {
                Name = CurrentProperties.id,
                FileName = CurrentProperties.file
            });
        }

        return MusicTracks;
    }

    public List<ScreenImage> GetScreenImages(string SourcePath)
    {
        FileLines = File.ReadAllLines(SourcePath + "\\game\\propobj.c");

        List<ScreenImage> ScreenImages = new();

        bool HandlingImageNames = false;
        bool HandlingImageCommands = false;

        foreach (string CurrentLine in FileLines)
        {
            StartPosition = 0;
            EndPosition = 0;

            if (string.IsNullOrWhiteSpace(CurrentLine))
            {
                continue;
            }

            Line = CurrentLine.TrimStart(TrimCharacters);

            if (HandlingImageNames)
            {
                if (LineStartsWith("case"))
                {
                    StartPosition = Line.IndexOf(' ') + 1;
                    EndPosition = Line.IndexOf(':');

                    string Constant = Line[StartPosition..EndPosition];

                    if (Constant == "TVCMDLIST_00")
                    {
                        HandlingImageNames = false;
                        continue;
                    }

                    StartPosition = Line.IndexOf('=') + 2;
                    EndPosition = Line.IndexOf(';');

                    string Name = Line[StartPosition..EndPosition];

                    ScreenImage NewScreenImage = new()
                    {
                        Constant = Constant,
                        Name = Name
                    };

                    ScreenImages.Add(NewScreenImage);
                }
            }
            else if (HandlingImageCommands)
            {
                // Add more stuff here in the future
            }
            else if (LineStartsWith("void tvscreenSetImageByNum"))
            {
                HandlingImageNames = true;
            }
            else if (LineStartsWith("u32 g_") && Line.IndexOf('{') != -1)
            {
                //HandlingImageCommands = true;

                // Add more stuff here in the future
            }
        }

        return ScreenImages;
    }

    public List<Stage> GetStages(string SourcePath)
    {
        FileLines = File.ReadAllLines(SourcePath + "\\game\\stagetable.c");

        List<Stage> Stages = new();

        bool HandlingStages = false;

        int StageID = 0;

        foreach (string CurrentLine in FileLines)
        {
            if (string.IsNullOrWhiteSpace(CurrentLine))
            {
                continue;
            }

            StartPosition = 0;
            Line = CurrentLine.TrimStart(TrimCharacters);

            if (HandlingStages)
            {
                if (LineStartsWith("/*"))
                {
                    EndPosition = Line.IndexOf(' ');

                    Stage NewStage = new()
                    {
                        ID = StageID,
                        Name = AddNewString(),
                        LightingCoronaSize = AddNewString(),
                        LIA = AddNewInt(),
                        LIW = AddNewInt(),
                        LIH = AddNewInt(),
                        Unknown1 = AddNewInt(),
                        BackgroundFile = AddNewString(),
                        CollisionFile = AddNewString(),
                        PadsFile = AddNewString(),
                        SetupFile = AddNewString(),
                        MultiplayerSetupFile = AddNewString(),
                        Unknown2 = AddNewFloat(),
                        Visibility = AddNewFloat(),
                        Unknown3 = AddNewFloat(),
                        Unknown4 = AddNewInt(),
                        Unknown5 = AddNewInt(),
                        Unknown6 = AddNewInt(),
                        Unknown7 = AddNewInt(),
                        Unknown8 = AddNewHexInt(),
                        Unknown9 = AddNewInt(),
                        Unknown10 = AddNewInt(),
                        Unknown11 = AddNewInt(),
                        Unknown12 = AddNewInt(),
                    };

                    Stages.Add(NewStage);
                    ++StageID;
                }
                else if (LineStartsWith("};"))
                {
                    HandlingStages = false;
                    break;
                }
            }
            else if (LineStartsWith("struct stagetableentry g_Stages[61]"))
            {
                HandlingStages = true;
                continue;
            }
        }

        return Stages;
    }

    public void ReadPads(string FilePath)
    {
        string FileText = File.ReadAllText(FilePath);
        PadContainer = JsonConvert.DeserializeObject<JSONPadObject>(FileText);
    }

    public JSONTileObject ReadCollision(string FilePath)
    {
        string FileText = File.ReadAllText(FilePath);
        return JsonConvert.DeserializeObject<JSONTileObject>(FileText);
    }

    public void ReadAICommand(AIList InAIList)
    {
        if (EndPosition == -1) // No opening bracket in the line
        {
            EndPosition = Line.IndexOf(' ');

            if (EndPosition == -1) // No space in the line, which means it's likely a command without arguments
            {
                EndPosition = Line.Length;
            }
        }

        string CommandName = Line[StartPosition..EndPosition].TrimEnd(TrimCharacters);
        AICommand CurrentCommand = DataHolder.GetAICommand(CommandName);

        if (CurrentCommand == null)
        {
            //Debug.Log(CommandName + " is not recognized as an AI command");
            return;
        }

        AICommandInstance NewCommandInstance = InAIList.AddEmptyCommand(CurrentCommand);
        NewCommandInstance.Command = CurrentCommand;

        foreach (ParameterType CurrentParameter in NewCommandInstance.Command.ParameterTypes)
        {
            switch (CurrentParameter.ClassType)
            {
                case "System.Int32":
                    NewCommandInstance.Parameters.Add(AddNewIntOrHexInt());
                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {(int)NewCommandInstance.Parameters[^1]}");
                    break;
                case "System.Int24":
                    NewCommandInstance.Parameters.Add(AddNewIntOrHexInt());
                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {(int)NewCommandInstance.Parameters[^1]}");
                    break;
                case "System.Int16":
                    NewCommandInstance.Parameters.Add(AddNewIntOrHexInt());
                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {(int)NewCommandInstance.Parameters[^1]}");
                    break;
                case "System.Byte":
                    NewCommandInstance.Parameters.Add(AddNewIntOrHexInt());
                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {(int)NewCommandInstance.Parameters[^1]}");
                    break;
                case "String":
                    List<string> DebugCommentArray = new();

                    EndPosition = StartPosition;
                    bool ReadingCharacters = true;

                    while (ReadingCharacters)
                    {
                        StartPosition = Line.IndexOf('\'', EndPosition);

                        if (StartPosition == -1)
                        {
                            ReadingCharacters = false;
                            break;
                        }

                        EndPosition = Line.IndexOf('\'', StartPosition + 1);

                        DebugCommentArray.Add(Line[(StartPosition + 1)..EndPosition]);

                        EndPosition += 1;
                    }

                    string DebugComment = string.Concat(DebugCommentArray);
                    NewCommandInstance.Parameters.Add(DebugComment);

                    //Debug.Log($"Parameter added to {InAIList.Name} in {CurrentCommand.Name}: {DebugComment}");
                    break;
                case "Label":
                    NewCommandInstance.Parameters.Add(InAIList.GetNewOrExistingLabel(AddNewIntOrHexInt()));
                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {Parameter.ID}");
                    break;
                case "Character":
                    int chrid = AddNewIntOrHexInt();
                    //Debug.Log(chrid.ToString("X"));
                    Prop chr = GetCharacterFromSetupFile(chrid);

                    NewCommandInstance.Parameters.Add(chr);

                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {chr?.GetName()}");
                    break;
                case "AIList":
                    string AIListID = AddNewString();
                    NewCommandInstance.Parameters.Add(AIListID);

                    AIListOffsets.Add(new AIListOffsetHolder
                    {
                        OffsetType = AIListOffsetHolder.AIListOffsetType.AIList,
                        CommandInstance = NewCommandInstance,
                        ParameterIndex = NewCommandInstance.Parameters.Count - 1
                    });

                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {AIListID}");
                    break;
                case "Animation":
                    string AnimationName = AddNewString();
                    AnimationSequence AnimationParameter = DataHolder.Animations.Find(x => x.Name == AnimationName);
                    NewCommandInstance.Parameters.Add(AnimationParameter);

                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {AnimationParameter.Name}");
                    break;
                case "Model":
                    string PropModelName = AddNewString();

                    //NewCommandInstance.Parameters.Add(new object());

                    if (DataHolder.ModelNumbers.TryGetValue(PropModelName, out int PropModelNumber))
                    {
                        ModelData PropData = DataHolder.PropDataList[PropModelNumber];
                        NewCommandInstance.Parameters.Add(PropData);
                        //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {PropData.FileName}");
                    }
                    else
                    {
                        Debug.Log("Model not found: " + Line);
                    }
                    break;
                case "Pad":
                    NewCommandInstance.Parameters.Add(GetPadFromSetupFile(AddNewString()));

                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {newpad?.name}");
                    break;
                case "Path":
                    int PathID = AddNewIntOrHexInt();
                    NewCommandInstance.Parameters.Add(PathID);

                    AIListOffsets.Add(new AIListOffsetHolder
                    {
                        OffsetType = AIListOffsetHolder.AIListOffsetType.Path,
                        CommandInstance = NewCommandInstance,
                        ParameterIndex = NewCommandInstance.Parameters.Count - 1
                    });
                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {PathID}");
                    break;
                case "Room":
                    int RoomID = AddNewHexInt();

                    // If the second parameter of action 0x005B is 1, the room parameter is not used.
                    // If it is 2, it's a pad, not a room (hardcoded to work only in G5 Building.)
                    if (NewCommandInstance.Command.ID == 0x005B)
                    {
                        int RoomOrPad = (int)NewCommandInstance.Parameters[1];

                        if (RoomOrPad == 1)
                        {
                            NewCommandInstance.Parameters.Add(null);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: No room");
                            break;
                        }
                        else if (RoomOrPad == 2)
                        {
                            Pad PadParameter = DataHolder.Pads[RoomID];
                            NewCommandInstance.Parameters.Add(PadParameter);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {PadParameter.NumberString}");
                            break;
                        }
                    }

                    NewCommandInstance.Parameters.Add(DataHolder.GetRoom(RoomID));

                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {RoomParameter?.NumberString}");
                    break;
                case "System.Boolean":
                    NewCommandInstance.Parameters.Add(bool.Parse(AddNewString()));
                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {NewBool}");
                    break;
                case "AimEntity":
                    BitFlagCollection AimFlags = (BitFlagCollection)NewCommandInstance.Parameters[0];

                    if (AimFlags.HasBits(0x0004)) // Aim at character
                    {
                        Prop AimCharacter = GetCharacterFromSetupFile(AddNewHexInt());
                        NewCommandInstance.Parameters.Add(AimCharacter);
                        //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {AimCharacter?.CharacterID}");
                    }
                    else if (AimFlags.HasBits(0x0008)) // Aim at pad
                    {
                        Pad AimPad = GetPadFromSetupFile(AddNewString());
                        NewCommandInstance.Parameters.Add(AimPad);
                        //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {AimPad?.NumberString}");
                    }
                    else if (AimFlags.HasBits(0x0010)) // Aim at direction
                    {
                        int AimDirection = AddNewIntOrHexInt();
                        NewCommandInstance.Parameters.Add(AimDirection);
                        //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {AimDirection}");
                    }
                    else if (AimFlags.HasBits(0x0200)) // Aim at target
                    {
                        bool PlaysWalkAnimation = Convert.ToBoolean(AddNewInt());
                        NewCommandInstance.Parameters.Add(PlaysWalkAnimation);
                        //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {PlaysWalkAnimation}");
                    }
                    else
                    {
                        AddNewString();
                        NewCommandInstance.Parameters.Add(null);
                        //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: null");
                    }
                    break;
                case "Prop":
                    int TagIndex = AddNewIntOrHexInt();
                    Prop TagParameter = Tags.Find(x => x.TagID == TagIndex);
                    NewCommandInstance.Parameters.Add(TagParameter);

                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {TagParameter.TagID}");
                    break;
                case "PropOrCharacter":
                    int EntityIndex = AddNewIntOrHexInt();

                    if (EntityIndex >= 0xF0) // Character preset
                    {
                        Prop CharacterParameter = GetCharacterFromSetupFile(EntityIndex);
                        NewCommandInstance.Parameters.Add(CharacterParameter);

                        //Debug.Log($"Parameter added to {InAIList.Name} in {CurrentCommand.Name}: {CharacterParameter.CharacterID}");
                    }
                    else // Prop tag
                    {
                        TagParameter = Tags.Find(x => x.TagID == EntityIndex);
                        NewCommandInstance.Parameters.Add(TagParameter);

                        //Debug.Log($"Parameter added to {InAIList.Name} in {CurrentCommand.Name}: {TagParameter.TagID}");
                    }
                    break;
                case "Objective":
                    int ObjectiveIndex = AddNewInt();
                    Objective ObjectiveParameter = Objectives.Find(x => x.ID == ObjectiveIndex);
                    NewCommandInstance.Parameters.Add(ObjectiveParameter);

                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {ObjectiveParameter.DisplayText?.en}");
                    break;
                case "Stage":
                    string StageName = AddNewString();
                    Stage StageParameter = DataHolder.Stages.Find(x => x.Name == StageName);

                    NewCommandInstance.Parameters.Add(StageParameter);
                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {StageParameter.Name}");
                    break;
                case "Body":
                    string BodyModelName = AddNewString();

                    if (DataHolder.ModelNumbers.TryGetValue(BodyModelName, out int BodyModelNumber))
                    {
                        ModelData BodyData = DataHolder.CharacterDataList[BodyModelNumber];
                        NewCommandInstance.Parameters.Add(BodyData);
                        //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {BodyData.FileName}");
                    }
                    else
                    {
                        Debug.Log("Model not found: " + Line);
                    }
                    break;
                case "Head":
                    string HeadModelName = AddNewString();

                    if (HeadModelName == "HEAD_RANDOM")
                    {
                        NewCommandInstance.Parameters.Add(DataHolder.RandomHeadData);
                    }
                    else if (DataHolder.ModelNumbers.TryGetValue(HeadModelName, out int HeadModelNumber))
                    {
                        //Debug.Log(HeadModelName + " " + HeadModelNumber + " " + DataHolder.CharacterDataList.Count);
                        ModelData HeadData = DataHolder.CharacterDataList[HeadModelNumber];
                        NewCommandInstance.Parameters.Add(HeadData);
                        //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {HeadData.FileName}");
                    }
                    else
                    {
                        Debug.Log("Model not found: " + Line);
                    }
                    break;
                case "LanguageString":
                    string LanguageStringName = AddNewString();

                    LanguageString LanguageStringParameter;
                    LanguageBank LevelLanguageFile = LanguageFileHolder.GetLevelLanguageFile();

                    if (LevelLanguageFile != null)
                    {
                        LanguageStringParameter = LanguageFileHolder.GetLevelLanguageFile().GetLanguageString(LanguageStringName);
                    }
                    else
                    {
                        LanguageStringParameter = LanguageFileHolder.GetLanguageFile("misc").GetLanguageString(LanguageStringName);
                    }

                    NewCommandInstance.Parameters.Add(LanguageStringParameter);

                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {LanguageStringParameter?.en}");
                    break;
                case "SoundEffect":
                    string SoundEffectName = AddNewString();
                    NewCommandInstance.Parameters.Add(DataHolder.SoundEffects.Find(x => x.Name == SoundEffectName));
                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {DataHolder.SoundEffects.Find(x => x.Name == SoundEffectName).Name}");
                    break;
                case "ScreenImage":
                    string ScreenImageConstant = AddNewString();

                    ScreenImage ScreenImageParameter = DataHolder.ScreenImages.Find(x => x.Constant == ScreenImageConstant);
                    NewCommandInstance.Parameters.Add(ScreenImageParameter);

                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {ScreenImageParameter.Name}");
                    break;
                case "MusicTrack":
                    string MusicTrackName = AddNewString();
                    NewCommandInstance.Parameters.Add(DataHolder.MusicTracks.Find(x => x.Name == MusicTrackName));
                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {DataHolder.MusicTracks.Find(x => x.Name == MusicTrackName).Name}");
                    break;
                case "Portal":
                    int PortalNumber = AddNewHexInt();

                    NewCommandInstance.Parameters.Add(DataHolder.Portals[PortalNumber]);
                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: Portal");
                    break;
                case "Quip":
                    string Quip = AddNewString();

                    if (Quip.StartsWith('Q'))
                    {
                        NewCommandInstance.Parameters.Add(ConstantHolder.GetConstant("QUIP", Quip));
                        //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {ConstantHolder.GetConstant("QUIP", Quip).Name}");
                    }
                    else
                    {
                        NewCommandInstance.Parameters.Add(int.Parse(Quip[2..], NumberStyles.HexNumber));
                        //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {int.Parse(Quip[2..], NumberStyles.HexNumber)}");
                    }
                    break;
                case "SpecialSoundBank":
                    string SoundBankProperty = AddNewString();

                    if (SoundBankProperty == "BANK_0")
                    {
                        NewCommandInstance.Parameters.Add(0);
                        //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: 0");
                    }
                    else if (SoundBankProperty == "BANK_1")
                    {
                        NewCommandInstance.Parameters.Add(1);
                        //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: 1");
                    }
                    else
                    {
                        NewCommandInstance.Parameters.Add(int.Parse(SoundBankProperty[2..], NumberStyles.HexNumber));
                        //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {int.Parse(SoundBankProperty[2..], NumberStyles.HexNumber)}");
                    }
                    break;
                case "Constant":
                    string ConstantName = AddNewString();
                    Constant Constant;

                    /*if (ConstantName.StartsWith("0x"))
                    {
                        int ConstantNumber = int.Parse(ConstantName[2..], NumberStyles.HexNumber);
                        Constant = ConstantHolder.GetConstant(CurrentParameter.ConstantName, ConstantNumber);

                        if (Constant == null)
                        {
                            NewCommandInstance.Parameters.Add(ConstantNumber);
                            Debug.LogWarning($"Constant not found in AI list {InAIList.Name} with command {CurrentCommand.Name}: {ConstantNumber}");
                            break;
                        }
                    }*/
                    /*if (ConstantName == "-1")
                    {
                        NewCommandInstance.Parameters.Add(null);
                        Debug.LogWarning($"Constant set to -1 in AI list {InAIList.Name} with command {CurrentCommand.Name}");
                        break;
                    }*/

                    string ConstantType = CurrentParameter.ConstantName == "CUTSCENEWEAPON" ? "WEAPON" : CurrentParameter.ConstantName;

                    Constant = ConstantHolder.GetConstant(ConstantType, ConstantName);

                    if (Constant == null)
                    {
                        bool IsNumber = int.TryParse(ConstantName, out int ConstantNumber);

                        if (IsNumber)
                        {
                            Constant = ConstantHolder.GetConstant(ConstantType, ConstantNumber);

                            if (Constant == null)
                            {
                                Debug.LogWarning("Constant " + ConstantName + " does not exist in parameter number " + NewCommandInstance.Parameters.Count + " in " + Line);
                            }
                        }
                        else
                        {
                            Defines.TryGetValue(ConstantName, out int DefineNumber);
                            Constant = ConstantHolder.GetConstant(ConstantType, DefineNumber);

                            if (Constant == null)
                            {
                                Debug.LogWarning("Constant " + ConstantName + " does not exist in parameter number " + NewCommandInstance.Parameters.Count + " in " + Line);
                            }
                        }
                    }

                    NewCommandInstance.Parameters.Add(Constant);
                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {Constant.Name}");
                    break;
                case "BitFlag":
                    List<string> BitFlagNames;
                    BitFlagCollection BitFlags = new();

                    if (CurrentParameter.ConstantName == "VARIABLE")
                    {
                        BitFlagNames = AddNewStrings();
                        BitFlags = GameBitFlagHolder.GetBitFlags(BitFlagNames);
                    }
                    else if (CurrentParameter.ConstantName == "STAGEFLAG")
                    {
                        BitFlagNames = AddNewStrings();
                        BitFlags = StageFlags.GetBitFlags(BitFlagNames);

                        foreach (BitFlag CurrentBitFlag in BitFlags)
                        {
                            CurrentBitFlag.ConnectedAICommands.Add(new() { CommandInstance = NewCommandInstance, ParameterID = NewCommandInstance.Parameters.Count });
                        }
                    }
                    else if (CurrentParameter.ConstantName == "LOCKFLAG")
                    {
                        int Bits = AddNewHexInt();

                        BitFlag MatchedBitFlag = GameBitFlagHolder.GetLockFlags.GetBitFlag(Bits);

                        if (MatchedBitFlag == null) // Bit flag doesn't exist yet
                        {
                            BitFlag NewBitFlag = new() { Bits = Bits, Name = $"Lock flag {LockFlags.Count + 1}", Constant = "LOCKFLAG_" + Bits.ToString("X") };
                            BitFlags.Add(NewBitFlag);
                            LockFlags.Add(NewBitFlag);

                            NewBitFlag.ConnectedAICommands.Add(new() { CommandInstance = NewCommandInstance, ParameterID = NewCommandInstance.Parameters.Count });
                        }
                        else
                        {
                            BitFlags.Add(MatchedBitFlag);

                            MatchedBitFlag.ConnectedAICommands.Add(new() { CommandInstance = NewCommandInstance, ParameterID = NewCommandInstance.Parameters.Count });
                        }
                    }
                    else
                    {
                        BitFlagNames = AddNewStrings();
                        BitFlags = GameBitFlagHolder.GetBitFlags(CurrentParameter.ConstantName, BitFlagNames);
                    }

                    NewCommandInstance.Parameters.Add(BitFlags);
                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {BitFlags.Count}");
                    break;
                default:
                    NewCommandInstance.Parameters.Add(new object());
                    //Debug.LogWarning($"Parameter {CurrentParameter.ClassType} not recognized in {InAIList.Name}: {AddNewString()}");
                    //Debug.LogWarning("Command: " + Line);
                    break;
            }
        }
    }

    public void ReadAIList()
    {
        StartPosition = Line.IndexOf('{') + 1;
        SkipWhitespace();

        EndPosition = Line.IndexOf(',', StartPosition);
        string AIListName = Line[StartPosition..EndPosition];

        if (AIListName == "NULL")
        {
            return;
        }

        StartPosition = Line.IndexOf('0', EndPosition);

        string AIListIDString = Line[(StartPosition + 2)..(StartPosition + 6)];
        int AIListID = int.Parse(AIListIDString, NumberStyles.HexNumber);

        AIList MatchingAIList = AILists.Find(x => x.Name == AIListName);
        MatchingAIList.ID = AIListID;

        switch (AIListIDString[0..2].ToUpper())
        {
            case "04":
                MatchingAIList.Type = AIListType.Character;
                break;
            case "0C":
                MatchingAIList.Type = AIListType.Cutscene;
                break;
            case "10":
                MatchingAIList.Type = AIListType.Background;
                break;
            case "14":
                MatchingAIList.Type = AIListType.Environmental;
                break;
        }
    }

    public void ReadGlobalAIList()
    {
        StartPosition = Line.IndexOf('{') + 1;
        SkipWhitespace();

        EndPosition = Line.IndexOf(',', StartPosition);
        string AIListName = Line[StartPosition..EndPosition];

        if (AIListName == "NULL }")
        {
            return;
        }

        StartPosition = EndPosition + 1;
        SkipWhitespace();
        EndPosition = Line.IndexOf('}');

        string AIListConstant = Line[StartPosition..EndPosition].TrimEnd(TrimCharacters);

        AIList MatchingAIList = AILists.Find(x => x.Name == AIListName);
        MatchingAIList.Constant = AIListConstant;
        MatchingAIList.Type = AIListType.Global;
        MatchingAIList.ID = ConstantHolder.GetConstant(AIListConstant).Value;
    }

    public void GetLanguageFiles(string FilePath)
    {
        List<LanguageBank> NewLanguageFiles = new();

        for (int i = 0; i < DataHolder.LanguageFileConstants.Count; ++i)// string Constant in DataHolder.LanguageFileConstants)
        {
            string Constant = DataHolder.LanguageFileConstants[i];

            if (Constant == "0")
            {
                NewLanguageFiles.Add(new LanguageBank() { Name = "0", LanguageStrings = new() { new LanguageString() } });
                continue;
            }

            string FileName = DataHolder.FileNames[DataHolder.FileNumbers[Constant]];

            string FileText = File.ReadAllText(FilePath + "\\assets\\ntsc-final\\lang\\" + FileName);

            List<LanguageString> LanguageContainer = JsonConvert.DeserializeObject<List<LanguageString>>(FileText);
            LanguageBank NewLanguageFile = new() { Name = Path.GetFileNameWithoutExtension(FileName), LanguageStrings = LanguageContainer };
            NewLanguageFile.ID = i;

            int StringID = 0;

            foreach (LanguageString CurrentString in LanguageContainer)
            {
                CurrentString.Number = StringID;
                CurrentString.ParentBank = NewLanguageFile;
                ++StringID;
            }

            NewLanguageFiles.Add(NewLanguageFile);
        }

        LanguageFileHolder.LanguageFiles = NewLanguageFiles;
    }

    // Only use this after setting StartPosition to 0. Returns whether the line starts with the given string (ignoring spaces). This is more efficient than C#'s in-built String.StartsWith().
    bool LineStartsWith(string MatchString)
    {
        SkipWhitespace();

        // If the MatchString is longer than the line minus all whitespaces at the start, it sure doesn't match
        if (MatchString.Length > Line[StartPosition..].Length)
        {
            return false;
        }

        return MatchString == Line.Substring(StartPosition, MatchString.Length);
    }

    // This is used in the middle of a line. Accepts a substring of the line, then checks whether that substring starts with the match string. Does not ignore spaces. This is more efficient than C#'s in-built String.StartsWith().
    bool SubLineStartsWith(string OriginalString, string MatchString)
    {
        // If the MatchString is longer than original substring, it sure doesn't match
        if (MatchString.Length > OriginalString.Length)
        {
            return false;
        }

        return MatchString == OriginalString[..MatchString.Length];
    }

    /*
     * This sets the starting position to the first alphanumerical character after the comma, and the ending position to the next comma after that character.
     * If there's no comma left, it means we're at the end of the list of arguments, so the ending position is the next right parenthesis.
     * This basically gets the beginning character index and ending (comma/parenthesis) index of the next argument.
     */
    void UpdateSeekPositions()
    {
        if (EndPosition != 0) // if we're not at the beginning of the line, set the start position just after the comma.
        {
            StartPosition = EndPosition + 1;

            for (int i = StartPosition; i < Line.Length; ++i) // then skip any spaces after the comma
            {
                if (Line[i] == '\t' || Line[i] == ' ')
                {
                    continue;
                }
                else if (Line[i..(i + 2)] == "/*") // if there's a comment in the middle
                {
                    i = Line.IndexOf("*/", i) + 2; // skip it
                    //Debug.Log("Skipped to: " + Line[i] + " " + i);
                    continue;
                }
                else
                {
                    StartPosition = i;
                    break;
                }
            }
        }

        if (EndPosition == 0) // if we're at the beginning
        {
            // set the end position to the next left parenthesis.
            EndPosition = Line.IndexOf("(", StartPosition);
        }
        else
        {
            // otherwise, set the end position to the next comma.
            EndPosition = Line.IndexOf(",", StartPosition);

            int CommentPosition = Line.IndexOf("//");
            if (CommentPosition > -1 && EndPosition > CommentPosition) // if the next comma is in a comment, we're at the end.
            {
                EndPosition = -1;
            }
        }

        if (EndPosition == -1) // if we're at the end
        {
            // set the end position to the next right parenthesis.
            EndPosition = Line.IndexOf(")", StartPosition);
        }
    }

    /*
     * This sets the seek position to the first alphanumerical character after the comma/pipe, and the pipe position to the next pipe after that character.
     * This basically gets the beginning (seek) and ending (pipe) index of the subargument (from an array full of BitFlags, for example).
     * Returns whether a valid pipe was found. If false, no further pipe is found, or pipe is part of another set of subarguments.
     */
    bool UpdatePipePosition()
    {
        if (PipePosition > 0) // check if we've passed a pipe already, if so, set the start position just after the pipe.
        {
            StartPosition = PipePosition + 1;
        }
        for (int i = StartPosition; i < Line.Length; ++i) // skip any spaces or left parentheses after the comma/pipe;
        {
            if (Line[i] == '\t' || Line[i] == ' ' || Line[i] == '(')
            {
                continue;
            }
            else
            {
                StartPosition = i;
                break;
            }
        }
        // then, set the pipe position to the next pipe (if there is one).
        PipePosition = Line.IndexOf("|", StartPosition);
        if (PipePosition == -1 || PipePosition > EndPosition)  // if there's no pipe, or the next pipe doesn't belong to this argument
        {
            PipePosition = -1;
            return false;
        }
        else
        {
            return true;
        }
    }

    // Skips over all spaces and tabs and sets the StartPosition to the next character after the whitespace.
    public void SkipWhitespace()
    {
        for (int i = StartPosition; i < Line.Length; ++i)
        {
            if (Line[i] == '\t' || Line[i] == ' ')
            {
                continue;
            }
            else
            {
                StartPosition = i;
                break;
            }
        }
    }

    Tuple<string, int> GetDefineOld()
    {
        for (int i = StartPosition; i < Line.Length; ++i) // skip any spaces;
        {
            if (Line[i] == '\t' || Line[i] == ' ')
            {
                continue;
            }
            else if (Line[i] == '#')
            {
                i += 7;
            }
            else
            {
                StartPosition = i;
                break;
            }
        }
        for (int i = StartPosition; i < Line.Length; ++i) // if it encounters any spaces, set the endposition to the end of the first argument.
        {
            if (Line[i] == '\t' || Line[i] == ' ')
            {
                EndPosition = i;
                break;
            }
        }

        string FirstArgument = Line[StartPosition..EndPosition];
        StartPosition = EndPosition;

        for (int i = StartPosition; i < Line.Length; ++i) // skip any spaces;
        {
            if (Line[i] == '\t' || Line[i] == ' ')
            {
                continue;
            }
            else
            {
                StartPosition = i;
                break;
            }
        }
        for (int i = StartPosition; i < Line.Length; ++i) // if it encounters any spaces, set the endposition to the end of the first argument.
        {
            if (Line[i] == '\t' || Line[i] == ' ')
            {
                EndPosition = i;
                break;
            }
            else if (i == Line.Length - 1)
            {
                EndPosition = i + 1;
            }
        }
        string SecondArgument = Line[StartPosition..EndPosition];
        int Value;

        if (SecondArgument.StartsWith("0x"))
        {
            Value = int.Parse(SecondArgument[2..], NumberStyles.HexNumber);
        }
        else
        {
            Value = int.Parse(SecondArgument);
        }

        return new Tuple<string, int>(FirstArgument, Value);
    }

    Constant GetDefine()
    {
        Constant Define = new();

        for (int i = StartPosition; i < Line.Length; ++i) // skip any spaces;
        {
            if (Line[i] == '\t' || Line[i] == ' ')
            {
                continue;
            }
            else if (Line[i] == '#')
            {
                i += 7;
            }
            else
            {
                StartPosition = i;
                break;
            }
        }
        for (int i = StartPosition; i < Line.Length; ++i) // if it encounters any spaces, set the endposition to the end of the first argument.
        {
            if (Line[i] == '\t' || Line[i] == ' ')
            {
                EndPosition = i;
                break;
            }
        }
        Define.Name = Line[StartPosition..EndPosition];
        StartPosition = EndPosition;

        for (int i = StartPosition; i < Line.Length; ++i) // skip any spaces;
        {
            if (Line[i] == '\t' || Line[i] == ' ')
            {
                continue;
            }
            else
            {
                StartPosition = i;
                break;
            }
        }

        if (Line[StartPosition..].StartsWith("(VERSION"))
        {
            string StringValue = HandleVersionsInLine(Line[StartPosition..]);
            Define.Value = int.Parse(StringValue[2..], NumberStyles.HexNumber);

            return Define;
        }

        for (int i = StartPosition; i < Line.Length; ++i) // if it encounters any spaces, set the endposition to the end of the first argument.
        {
            if (Line[i] == '\t' || Line[i] == ' ')
            {
                EndPosition = i;
                break;
            }
            else if (i == Line.Length - 1)
            {
                EndPosition = i + 1;
            }
        }
        string Value = Line[StartPosition..EndPosition];
        if (Value.Length > 2 && Value[0..2] == "0x")
        {
            Define.Value = int.Parse(Value[2..], NumberStyles.HexNumber);
        }
        else
        {
            Define.Value = int.Parse(Value);
        }

        return Define;
    }

    // If there are any #if, #else and #endif directives that indicate versions in the file, this function is called to ignore and process the applicable lines, taking into account the current version.
    void HandleVersions()
    {
        if (LineStartsWith("#if"))
        {
            Line = Line[3..].TrimStart(TrimCharacters);
            if (Line[..7] != "VERSION")
            {
                return;
            }
            Line = Line[7..].TrimStart(TrimCharacters);

            EndPosition = Line.IndexOfAny(TrimCharacters);
            string Operator = Line[..EndPosition].TrimEnd(TrimCharacters);

            string Version = Line[EndPosition..].Trim(TrimCharacters);

            switch (Operator)
            {
                case "==":
                    if (ROMVersions.GetCurrentVersionIndex() == ROMVersions.GetVersionIndex(Version))
                    {
                        ApplicableVersion = true;
                    }
                    else
                    {
                        ApplicableVersion = false;
                        VersionMatched = false;
                    }
                    break;
                case "<":
                    if (ROMVersions.GetCurrentVersionIndex() < ROMVersions.GetVersionIndex(Version))
                    {
                        ApplicableVersion = true;
                    }
                    else
                    {
                        ApplicableVersion = false;
                        VersionMatched = false;
                    }
                    break;
                case ">":
                    if (ROMVersions.GetCurrentVersionIndex() > ROMVersions.GetVersionIndex(Version))
                    {
                        ApplicableVersion = true;
                    }
                    else
                    {
                        ApplicableVersion = false;
                        VersionMatched = false;
                    }
                    break;
                case "<=":
                    if (ROMVersions.GetCurrentVersionIndex() <= ROMVersions.GetVersionIndex(Version))
                    {
                        ApplicableVersion = true;
                    }
                    else
                    {
                        ApplicableVersion = false;
                        VersionMatched = false;
                    }
                    break;
                case ">=":
                    if (ROMVersions.GetCurrentVersionIndex() >= ROMVersions.GetVersionIndex(Version))
                    {
                        ApplicableVersion = true;
                    }
                    else
                    {
                        ApplicableVersion = false;
                        VersionMatched = false;
                    }
                    break;
            }
        }
        else if (LineStartsWith("#else"))
        {
            if (VersionMatched)
            {
                ApplicableVersion = false;
            }
            else
            {
                ApplicableVersion = true;
            }
        }
        else if (LineStartsWith("#endif"))
        {
            ApplicableVersion = true;
            VersionMatched = true;
        }
    }

    // If there are version comparisons embedded in a line itself, this returns the correct value depending on the current ROM version.
    string HandleVersionsInLine(string InLine)
    {
        bool WhichVersion = true;

        int VersionStartPosition = InLine.IndexOf(' ') + 1;
        int VersionEndPosition = InLine.IndexOf(' ', VersionStartPosition);

        string Operator = Line[VersionStartPosition..VersionEndPosition];

        VersionStartPosition = VersionEndPosition + 1;
        VersionEndPosition = InLine.IndexOf(' ', VersionStartPosition);

        string Version = Line[VersionStartPosition..VersionEndPosition];

        switch (Operator)
        {
            case "==":
                if (ROMVersions.GetCurrentVersionIndex() == ROMVersions.GetVersionIndex(Version))
                {
                    WhichVersion = true;
                }
                else
                {
                    WhichVersion = false;
                }
                break;
            case "<":
                if (ROMVersions.GetCurrentVersionIndex() < ROMVersions.GetVersionIndex(Version))
                {
                    WhichVersion = true;
                }
                else
                {
                    WhichVersion = false;
                }
                break;
            case ">":
                if (ROMVersions.GetCurrentVersionIndex() > ROMVersions.GetVersionIndex(Version))
                {
                    WhichVersion = true;
                }
                else
                {
                    WhichVersion = false;
                }
                break;
            case "<=":
                if (ROMVersions.GetCurrentVersionIndex() <= ROMVersions.GetVersionIndex(Version))
                {
                    WhichVersion = true;
                }
                else
                {
                    WhichVersion = false;
                }
                break;
            case ">=":
                if (ROMVersions.GetCurrentVersionIndex() >= ROMVersions.GetVersionIndex(Version))
                {
                    WhichVersion = true;
                }
                else
                {
                    WhichVersion = false;
                }
                break;
        }

        if (WhichVersion == true)
        {
            VersionStartPosition = InLine.IndexOf('?', VersionStartPosition) + 2;
            VersionEndPosition = InLine.IndexOf(' ', VersionStartPosition);
        }
        else
        {
            VersionStartPosition = InLine.IndexOf(':', VersionStartPosition) + 2;
            VersionEndPosition = InLine.IndexOf(')', VersionStartPosition);
            if (VersionEndPosition == -1) { VersionEndPosition = InLine.Length - 1; }
        }

        return InLine[VersionStartPosition..VersionEndPosition];
    }

    BitFlag GetBitFlag()
    {
        BitFlag NewBitFlag = new();

        for (int i = StartPosition; i < Line.Length; ++i) // skip any spaces;
        {
            if (Line[i] == '\t' || Line[i] == ' ')
            {
                continue;
            }
            else if (Line[i] == '#')
            {
                i += 7;
            }
            else
            {
                StartPosition = i;
                break;
            }
        }
        for (int i = StartPosition; i < Line.Length; ++i) // if it encounters any spaces, set the endposition to the end of the first argument.
        {
            if (Line[i] == '\t' || Line[i] == ' ')
            {
                EndPosition = i;
                break;
            }
        }
        NewBitFlag.Constant = Line[StartPosition..EndPosition];
        StartPosition = EndPosition;

        for (int i = StartPosition; i < Line.Length; ++i) // skip any spaces;
        {
            if (Line[i] == '\t' || Line[i] == ' ')
            {
                continue;
            }
            else
            {
                StartPosition = i;
                break;
            }
        }
        for (int i = StartPosition; i < Line.Length; ++i) // if it encounters any spaces, set the endposition to the end of the first argument.
        {
            if (Line[i] == '\t' || Line[i] == ' ')
            {
                EndPosition = i;
                break;
            }
            else if (i == Line.Length - 1)
            {
                EndPosition = i + 1;
            }
        }
        string Value = Line[StartPosition..EndPosition];
        if (Value.Length > 2 && Value[0..2] == "0x")
        {
            NewBitFlag.Bits = int.Parse(Value[2..], NumberStyles.HexNumber);
        }
        else
        {
            NewBitFlag.Bits = int.Parse(Value);
        }

        return NewBitFlag;
    }

    // Read a pad that is formatted like PAD_LVL_0000 or 0x0000 in the file.
    // Returns the pad object.
    Pad GetPadFromSetupFile(string InPad)
    {
        if (InPad == "-1")
        {
            return null;
        }
        else if (InPad == "PAD_PRESET")
        {
            return null;
        }

        string PadNumber = InPad[^4..].ToUpper();
        /*if (InPad[..2] == "0x")
        {
            PadNumber = InPad[2..].ToUpper();
        }
        PadNumber = InPad[^4..];*/
        return DataHolder.Pads.Find(x => x.NumberString == PadNumber);
    }

    // Read a character number, like 0x09. Can also read set constants, like CHR_SELF or CHR_BOND.
    Prop GetCharacterFromSetupFile(int CharacterID)
    {
        return CharacterID switch
        {
            -1 => null,
            0 => null,
            _ => Characters.Find(x => x.CharacterID == CharacterID),
        };
    }

    // WARNING: only use after setting UpdateSeekPositions once. Get the next argument from a line, like size, pad, attached weapon etc.
    // More specifically, it returns the substring between the opening bracket/comma and the next comma.
    string AddNewString()
    {
        UpdateSeekPositions();

        string ReadValue = Line[StartPosition..EndPosition];

        if (SubLineStartsWith(ReadValue, "VERSION"))
        {
            ReadValue = HandleVersionsInLine(Line);
            //Debug.Log("Version handled string: " + ReadValue);
        }
        
        //Debug.Log("AddString: " + Line.Substring(StartPosition, EndPosition - StartPosition).Trim());
        return ReadValue.Trim(TrimCharacters);
    }

    // WARNING: only use after setting UpdateSeekPositions once. Gets all the separate bit flags that are OR'd together.
    // More specifically, it returns an array of strings that are separated by pipes.
    List<string> AddNewStrings()
    {
        List<string> NewArray = new();
        UpdateSeekPositions();

        do
        {
            PipeFound = UpdatePipePosition();
            if (PipeFound) // found a pipe?
            {
                NewArray.Add(Line[StartPosition..PipePosition].Trim(TrimCharacters));
                //Debug.Log("AddStrings: " + Line.Substring(StartPosition, PipePosition - StartPosition).Trim());
            }
            else // if not, that means there's only one flag (left)
            {
                if (Line[StartPosition..EndPosition].Trim(TrimCharacters) == "0")
                {
                    return NewArray; // if the flags argument only contains 0, then there are no flags, so return an empty array
                }
                else
                {
                    NewArray.Add(Line[StartPosition..EndPosition].Trim(TrimCharactersWithClosingParenthesis));
                }
                //Debug.Log("AddStrings: " + Line.Substring(StartPosition, EndPosition - StartPosition).Trim());
            }
        } while (PipeFound); // if it found a valid pipe, go to the next iteration. if not, this is the last subargument, and the loop will stop.

        return NewArray;
    }

    // WARNING: only use after setting UpdateSeekPositions once. Get the next argument from a line, like size, pad, attached weapon etc.
    // More specifically, it returns the substring between the opening bracket/comma and the next comma, and converts it to int.
    int AddNewInt()
    {
        UpdateSeekPositions();
        //Debug.Log("AddNewInt: " + Line.Substring(StartPosition, EndPosition - StartPosition).Trim() + " in: " + Line);
        return int.Parse(Line[StartPosition..EndPosition].Trim(TrimCharacters));
    }

    // WARNING: only use after setting UpdateSeekPositions once. Get the next argument from a line, like size, pad, attached weapon etc.
    // More specifically, it returns the substring between the opening bracket/comma and the next comma, and converts it to int.
    int AddNewHexInt()
    {
        UpdateSeekPositions();

        string ReadValue = Line[StartPosition..EndPosition];

        if (SubLineStartsWith(ReadValue, "VERSION"))
        {
            ReadValue = HandleVersionsInLine(ReadValue);
            //Debug.Log("Version handled value: " + ReadValue);
        }

        if (SubLineStartsWith(ReadValue, "0x"))
        {
            //Debug.Log("AddNewHexInt: " + Line[(StartPosition + 2)..EndPosition].Trim());
            return int.Parse(ReadValue[2..].Trim(TrimCharacters), NumberStyles.AllowHexSpecifier);
        }
        else
        {
            if (Defines.TryGetValue(ReadValue, out int Value))
            {
                //Debug.Log("AddNewHexInt: " + Value);
                return Value;
            }
            else
            {
                Debug.LogError("Error while trying to read hexadecimal int, could not recognize it: " + ReadValue);
                Debug.LogError("Line: " + Line);
                return 0;
            }
        }
    }

    // WARNING: only use after setting UpdateSeekPositions once. Get the next argument from a line, like size, pad, attached weapon etc.
    // More specifically, it returns the substring between the opening bracket/comma and the next comma, and converts it to int.
    int AddNewIntOrHexInt()
    {
        UpdateSeekPositions();

        string ReadValue = Line[StartPosition..EndPosition];

        if (SubLineStartsWith(ReadValue, "VERSION"))
        {
            ReadValue = HandleVersionsInLine(ReadValue);
            //Debug.Log("Version handled value: " + ReadValue);
        }

        if (SubLineStartsWith(ReadValue, "0x"))
        {
            //Debug.Log("AddNewHexInt: " + Line[(StartPosition + 2)..EndPosition].Trim());
            return int.Parse(Line[(StartPosition + 2)..EndPosition].Trim(TrimCharacters), NumberStyles.AllowHexSpecifier);
        }
        else
        {
            if (Defines.TryGetValue(ReadValue, out int Value))
            {
                //Debug.Log("AddNewHexInt: " + Value);
                return Value;
            }
            else
            {
                //Debug.Log("AddNewIntOrHexInt: " + Line.Substring(StartPosition, EndPosition - StartPosition).Trim() + " in: " + Line);
                return int.Parse(ReadValue.Trim(TrimCharacters));
            }
        }
    }

    // WARNING: only use after setting UpdateSeekPositions once. Get the next argument from a line, like size, pad, attached weapon etc.
    // More specifically, it returns the substring between the opening bracket/comma and the next comma, and converts it to int.
    float AddNewFloat()
    {
        UpdateSeekPositions();
        //Debug.Log("AddNewInt: " + Line.Substring(StartPosition, EndPosition - StartPosition).Trim() + " in: " + Line);
        return float.Parse(Line[StartPosition..EndPosition].Trim(TrimCharacters));
    }
}