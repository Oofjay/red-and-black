using System.Collections.Generic;


public class TempAIList
{
    public int ID;
    public int Pointer;
    public int NewPointer;
}

public class PDTexture 
{
    public int ID { get; set; }
    public TexturePixelFormat PixelFormat { get; set; }
    public int DetailLevels { get; set; }
}

public class BoundingBox
{
    public int XMin, YMin, ZMin;
    public int XMax, YMax, ZMax;
}

public class ROMVersions
{
    public static Version CurrentVersion { get; set; } = Version.NTSCFinal;
    public static string CurrentVersionName
    {
        get { return Names[(int)CurrentVersion]; }
    }

    private static List<string> Names { get; } = new()
    {
        "VERSION_NTSC_BETA",
        "VERSION_NTSC_1_0",
        "VERSION_NTSC_FINAL",
        "VERSION_PAL_BETA",
        "VERSION_PAL_FINAL",
        "VERSION_JPN_FINAL"
    };

    public enum Version
    {
        NTSCBeta,
        NTSC1_0,
        NTSCFinal,
        PALBeta,
        PALFinal,
        JPNFinal,
    }

    public static string GetVersionName(Version InVersion)
    {
        return Names[(int)InVersion];
    }

    public static int GetVersionIndex(string VersionConstant)
    {
        return Names.IndexOf(VersionConstant);
    }

    public static int GetCurrentVersionIndex()
    {
        return (int)CurrentVersion;
    }
}

public class ScreenImageCommand
{
    public string Name { get; set; }
}

public class ScreenImage
{
    public string Name { get; set; }
    public string Constant { get; set; }
    public string DisplayName { get; set; }
    public List<ScreenImageCommand> Commands { get; set; }
}

public class AnimationSequence
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string FileName { get; set; }
    public int NumberOfFrames { get; set; }
    public int BytesPerFrame { get; set; }
    public int Unknown08 { get; set; }
    public int Unknown0A { get; set; }
    public bool Loops { get; set; }
    public bool HasAbsoluteTranslation { get; set; }
    public bool HasGuardRepeatPoints { get; set; }
    public bool HasCameraRepeatPoints { get; set; }
}

public class SoundEffect
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string FileName { get; set; }

    public bool IsMP3 { get; set; }
}

public class SoundEffectInstance
{
    public enum MP3Priority
    {
        Low,
        Medium,
        High
    }

    public SoundEffect SoundEffectID { get; set; }

    public MP3Priority Priority { get; set; }
    public bool Unknown0x2000 { get; set; }
    public bool Unknown0x4000 { get; set; }
}

public class MusicTrack
{
    public string Name { get; set; }
    public string FileName { get; set; }
}

public class Stage
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string LightingCoronaSize { get; set; }
    public int LIA { get; set; }
    public int LIW { get; set; }
    public int LIH { get; set; }
    public int Unknown1 { get; set; }
    public string BackgroundFile { get; set; }
    public string CollisionFile { get; set; }
    public string PadsFile { get; set; }
    public string SetupFile { get; set; }
    public string MultiplayerSetupFile { get; set; }
    public float Unknown2 { get; set; }
    public float Visibility { get; set; }
    public float Unknown3 { get; set; }
    public int Unknown4 { get; set; }
    public int Unknown5 { get; set; }
    public int Unknown6 { get; set; }
    public int Unknown7 { get; set; }
    public int Unknown8 { get; set; }
    public int Unknown9 { get; set; }
    public int Unknown10 { get; set; }
    public int Unknown11 { get; set; }
    public int Unknown12 { get; set; }
}

public class VisibilityCommand
{
    public static List<int> CommandTypes = new()
    {
        { 0x00 },
        { 0x01 },
        { 0x02 },
        { 0x03 },
        { 0x04 },
        { 0x05 },
        { 0x06 },
        { 0x14 },
        { 0x1E },
        { 0x1F },
        { 0x20 },
        { 0x21 },
        { 0x22 },
        { 0x23 },
        { 0x24 },
        { 0x25 },
        { 0x26 },
        { 0x27 },
        { 0x28 },
        { 0x29 },
        { 0x2A },
        { 0x50 },
        { 0x51 },
        { 0x52 },
        { 0x5A },
        { 0x5B },
        { 0x5C }
    };

    public static Dictionary<int, string> CommandNames = new()
    {
        { 0x00, "End" },
        { 0x01, "Push value to stack" },
        { 0x02, "Pop value from stack" },
        { 0x03, "Logical AND" },
        { 0x04, "Logical OR" },
        { 0x05, "Logical NOT" },
        { 0x06, "Logical XOR" },
        { 0x14, "Push if player is within rooms" },
        { 0x1E, "Set result to true" },
        { 0x1F, "Set result to true if portal is in FoV" },
        { 0x20, "Show room if result is true" },
        { 0x21, "Set result to false" },
        { 0x22, "Set result to true if room is visible through portal" }, // ?
        { 0x23, "Set result to false if room not visible through portal" }, // ?
        { 0x24, "Disable room" },
        { 0x25, "Disable room range" },
        { 0x26, "Load room" }, // doesn't do anything?
        { 0x27, "Load room range" }, // doesn't do anything?
        { 0x28, "Set room tests disabled" },
        { 0x29, "Push if portal is open" },
        { 0x2A, "Set room's unknown value" }, // has something to do with snaking (not sure what that is)
        { 0x50, "Branch" },
        { 0x51, "Throw" },
        { 0x52, "Catch" },
        { 0x5A, "If pop result is true" },
        { 0x5B, "Else (unused)" },
        { 0x5C, "End if" }
    };

    //public string Name;
    public int Type;
    public string NumberString => Type.ToString("X2");
    public string DisplayString => NumberString + " " + CommandNames[Type];

    public List<int> Arguments;
    //public int Length { get; set; }
}

// This class is used for BitFlags. This can be used to house the static BitFlags of the decomp project, or the stage/lock flags of the setup.
public class BitFlagCollection : List<BitFlag>
{
    public BitFlag GetBitFlag(string Constant)
    {
        return Find(x => x.Constant == Constant);
    }

    public BitFlag GetBitFlag(int Bits)
    {
        return Find(x => x.Bits == Bits);
    }

    // This function returns all BitFlags that match with the constants.h constants. Use this to fill a prop/paths BitFlagList.
    public BitFlagCollection GetBitFlags(List<string> Constants)
    {
        BitFlagCollection NewBitFlagList = new();

        foreach (string Constant in Constants)
        {
            BitFlag NewBitFlag = GetBitFlag(Constant);

            if (NewBitFlag != null)
            {
                NewBitFlagList.Add(GetBitFlag(Constant));
            }
        }

        return NewBitFlagList;
    }

    // This function returns all BitFlags that match with the entered bit field. Use this to fill a prop/paths BitFlagList.
    public BitFlagCollection GetBitFlags(int Bits)
    {
        BitFlagCollection NewBitFlagList = new();

        foreach(BitFlag CurrentBitFlag in this)
        {
            if ((CurrentBitFlag.Bits & Bits) != 0)
            {
                NewBitFlagList.Add(CurrentBitFlag);
            }
        }

        return NewBitFlagList;
    }

    public int GetBits()
    {
        int Bits = 0;

        foreach (BitFlag CurrentBitFlag in this)
        {
            Bits |= CurrentBitFlag.Bits;
        }

        return Bits;
    }

    public bool HasBits(int Bits)
    {
        return Exists(x => x.Bits == Bits);
    }

    public bool HasBitFlag(BitFlag InBitFlag)
    {
        return Contains(InBitFlag);
    }

    public bool HasBitFlag(string Constant)
    {
        return Exists(x => x.Constant == Constant);
    }
}

// This contains the decomp's BitFlags. It has a list of each type of BitFlags, which it reads from constants.h
public static class GameBitFlagHolder
{
    public static Dictionary<string, BitFlagCollection> BitFlagCollection = new()
    {
        {"STAGEFLAG", new BitFlagCollection()},
        {"LOCKFLAG", new BitFlagCollection()},
    };

    public static Dictionary<string, int> BitFlagSizes = new()
    {
        {"STAGEFLAG", 32},
        {"LOCKFLAG", 8},
    };

    public static void ClearBitFlagCollection()
    {
        BitFlagCollection = new()
        {
            {"STAGEFLAG", new BitFlagCollection()},
            {"LOCKFLAG", new BitFlagCollection()},
        };

        BitFlagSizes = new()
        {
            {"STAGEFLAG", 32},
            {"LOCKFLAG", 8},
        };
    }

    public static BitFlagCollection AddBitFlagList(string ListName)
    {
        BitFlagCollection NewList = new();
        BitFlagCollection.TryAdd(ListName, NewList);
        return NewList;
    }

    public static BitFlagCollection GetBitFlagList(string ListName)
    {
        return BitFlagCollection[ListName];
    }

    public static void AddBitFlag(string BitFlagType, BitFlag InBitFlag)
    {
        BitFlagCollection[BitFlagType].Add(InBitFlag);
    }

    public static BitFlag GetBitFlag(string BitFlagType, string BitFlagConstant)
    {
        return BitFlagCollection[BitFlagType].Find(x => x.Constant == BitFlagConstant);
    }

    public static BitFlag GetBitFlag(string BitFlagType, int Bits)
    {
        return BitFlagCollection[BitFlagType].Find(x => x.Bits == Bits);
    }

    public static BitFlag GetBitFlag(string BitFlagConstant)
    {
        string BitFlagType = BitFlagConstant[..BitFlagConstant.IndexOf('_')];

        return BitFlagCollection[BitFlagType].Find(x => x.Constant == BitFlagConstant);
    }

    public static BitFlagCollection GetBitFlags(string BitFlagType, List<string> BitFlagConstants)
    {
        BitFlagCollection NewCollection = new();

        foreach (BitFlag CurrentBitFlag in BitFlagCollection[BitFlagType])
        {
            if (BitFlagConstants.Contains(CurrentBitFlag.Constant))
            {
                NewCollection.Add(CurrentBitFlag);
            }
        }
        return NewCollection;
    }

    public static BitFlagCollection GetBitFlags(List<string> BitFlagConstants)
    {
        if (BitFlagConstants.Count == 0)
        {
            return new BitFlagCollection();
        }

        string BitFlagType = BitFlagConstants[0][..BitFlagConstants[0].IndexOf('_')];

        BitFlagCollection NewCollection = new();

        foreach (BitFlag CurrentBitFlag in BitFlagCollection[BitFlagType])
        {
            if (BitFlagConstants.Contains(CurrentBitFlag.Constant))
            {
                NewCollection.Add(CurrentBitFlag);
            }
        }
        return NewCollection;
    }

    public static BitFlagCollection GetBitFlags(string BitFlagType, int Bits)
    {
        BitFlagCollection NewCollection = new();

        foreach (BitFlag CurrentBitFlag in BitFlagCollection[BitFlagType])
        {
            if ((CurrentBitFlag.Bits & Bits) != 0)
            {
                NewCollection.Add(CurrentBitFlag);
            }
        }

        return NewCollection;
    }

    public static BitFlag CreateNewStageFlag()
    {
        BitFlag NewBitFlag = null;
        BitFlagCollection StageFlags = GetStageFlags;

        for (int CheckBits = 1; CheckBits != 0; CheckBits <<= 1)
        {
            if (StageFlags.HasBits(CheckBits))
            {
                continue;
            }
            else
            {
                NewBitFlag = new() { Bits = CheckBits, Name = $"Stage flag {StageFlags.Count + 1}", Constant = "STAGEFLAG_" + CheckBits.ToString("X") };
                StageFlags.Add(NewBitFlag);
                break;
            }
        }

        return NewBitFlag;
    }

    public static BitFlag CreateNewLockFlag()
    {
        BitFlag NewBitFlag = null;
        BitFlagCollection LockFlags = GetLockFlags;

        for (int CheckBits = 1; CheckBits != 0; CheckBits <<= 1)
        {
            if (LockFlags.HasBits(CheckBits))
            {
                continue;
            }
            else
            {
                NewBitFlag = new() { Bits = CheckBits, Name = $"Lock flag {LockFlags.Count + 1}", Constant = "LOCKFLAG_" + CheckBits.ToString("X") };
                LockFlags.Add(NewBitFlag);
                break;
            }
        }

        return NewBitFlag;
    }

    public static void DeleteStageFlag(BitFlag InBitFlag)
    {
        foreach (ObjectiveCondition CurrentCondition in InBitFlag.ConnectedObjectiveConditions)
        {
            CurrentCondition.StageFlag = null;
        }
        foreach (AIListUsage CurrentUsage in InBitFlag.ConnectedAICommands)
        {
            BitFlagCollection Parameter = (BitFlagCollection)CurrentUsage.CommandInstance.Parameters[CurrentUsage.ParameterID];
            Parameter.Remove(InBitFlag);
        }

        BitFlagCollection StageFlags = GetStageFlags;

        StageFlags.Remove(InBitFlag);
        DropdownOptions.RefreshSetupBitFlags();
    }

    public static void DeleteLockFlag(BitFlag InBitFlag)
    {
        foreach (Prop CurrentTag in InBitFlag.ConnectedProps)
        {
            CurrentTag.LockBits.Remove(InBitFlag);
        }

        BitFlagCollection LockFlags = GetLockFlags;

        LockFlags.Remove(InBitFlag);
        DropdownOptions.RefreshSetupBitFlags();
    }

    public static void ClearStageFlags()
    {
        BitFlagCollection StageFlags = GetStageFlags;

        StageFlags.Clear();
    }

    public static void ClearLockFlags()
    {
        BitFlagCollection LockFlags = GetLockFlags;

        LockFlags.Clear();
    }

    public static BitFlagCollection GetStageFlags => GetBitFlagList("STAGEFLAG");
    public static BitFlagCollection GetLockFlags => GetBitFlagList("LOCKFLAG");
}

public class BitFlag
{
    public string Constant { get; set; } // For the in-built BitFlags, the constant from constants.h. For dynamic stage BitFlags, the constant at the top of the setup file.
    public int Bits { get; set; }
    public string Name { get; set; } // For in-built BitFlags, the name from BitFlags.json. For dynamic stage BitFlags, the name the user created.
    public string Description { get; set; } // Only for in-built BitFlags, the description from BitFlags.json.

    // Lock flags only.
    public List<Prop> ConnectedProps { get; set; } = new();

    // Stage flags only.
    public List<ObjectiveCondition> ConnectedObjectiveConditions { get; set; } = new();

    // Stage flags only. A list of command instances that use this bit flag, along with their parameter index.
    public List<AIListUsage> ConnectedAICommands { get; set; } = new();
}

public static class LanguageFileHolder
{
    public static List<LanguageBank> LanguageFiles { get; set; } = new();

    public static LanguageBank GetLanguageFile(string FileName)
    {
        return LanguageFiles.Find(x => x.Name == FileName);
    }

    public static LanguageBank GetLevelLanguageFile()
    {
        return LanguageFiles.Find(x => x.Name == DataHolder.LevelFileName);
    }
}

public class LanguageBank
{
    public int ID { get; set; }
    public string Name { get; set; }
    public List<LanguageString> LanguageStrings { get; set; }

    public LanguageString GetLanguageString(string StringID)
    {
        return LanguageStrings.Find(x => x.id == StringID);
    }
}

public class LanguageString
{
    public string id { get; set; }
    public string en { get; set; }
    public string gb { get; set; }
    public string jp { get; set; }
    public string it { get; set; }
    public string fr { get; set; }
    public string de { get; set; }
    public string es { get; set; }

    public int Number { get; set; }
    public LanguageBank ParentBank { get; set; }

    public string Abbreviate()
    {
        if (en == null)
        {
            return null;
        }

        string DisplayString = en;
        if (DisplayString.Length > 50)
        {
            DisplayString = DisplayString[..50];
        }
        DisplayString = DisplayString.Replace("\n", null);
        return DisplayString;
    }
}

public static class Briefing
{
    public static LanguageString StageTitle { get; set; }
    public static LanguageString AgentBriefing { get; set; }
    public static LanguageString SpecialAgentBriefing { get; set; }
    public static LanguageString PerfectAgentBriefing { get; set; }
}

public enum ConditionType
{
    DestroyObject,
    CompletionFlags,
    FailureFlags,
    CollectObject,
    DepositObject,
    HolographObject,
    EnterRoom,
    DepositObjectInRoom
}

public class ObjectiveCondition
{
    private static readonly string[] _ConditionNames =
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

    public int ID { get; set; }

    public Objective AttachedObjective { get; set; }

    public ConditionType Type { get; set; }
    public string ConditionName
    {
        get => _ConditionNames[(int)Type];
    }

    public int TagID { get; set; }
    public Prop ObjectiveTag { get; set; }
    public BitFlag StageFlag { get; set; }
    public Room ObjectiveRoom { get; private set; }
    public Pad ObjectivePad { get; set; }

    public void AddNewTag(Prop InTag)
    {
        if (ObjectiveTag != null)
        {
            ObjectiveTag.ConnectedObjectiveConditions.Remove(this);
        }

        ObjectiveTag = InTag;

        if (ObjectiveTag != null)
        {
            ObjectiveTag.ConnectedObjectiveConditions.Add(this);
        }
    }

    public void AddNewStageFlag(BitFlag InStageFlag)
    {
        if (StageFlag != null)
        {
            StageFlag.ConnectedObjectiveConditions.Remove(this);
        }

        StageFlag = InStageFlag;

        if (StageFlag != null)
        {
            InStageFlag.ConnectedObjectiveConditions.Add(this);
        }
    }

    public void AddNewRoom(Room InRoom)
    {
        if (ObjectiveRoom != null)
        {
            ObjectiveRoom.ConnectedObjectiveConditions.Remove(this);
        }

        ObjectiveRoom = InRoom;

        if (ObjectiveRoom != null)
        {
            ObjectiveRoom.ConnectedObjectiveConditions.Add(this);
        }
    }

    public void ChangeConditionType(ConditionType InType)
    {
        Type = InType;

        if (ObjectiveTag != null)
        {
            ObjectiveTag.ConnectedObjectiveConditions.Remove(this);
            ObjectiveTag = null;
        }
        else if (StageFlag != null)
        {
            StageFlag.ConnectedObjectiveConditions.Remove(this);
            StageFlag = null;
        }
        else if (ObjectiveRoom != null)
        {
            ObjectiveRoom.ConnectedObjectiveConditions.Remove(this);
            ObjectiveRoom = null;
        }
    }

    public void RemoveStageFlag()
    {
        StageFlag.ConnectedObjectiveConditions.Remove(this);
        StageFlag = null;
    }

    public void RemoveTag()
    {
        ObjectiveTag.ConnectedObjectiveConditions.Remove(this);
        ObjectiveTag = null;
    }

    public void RemoveRoom()
    {
        ObjectiveRoom.ConnectedObjectiveConditions.Remove(this);
        ObjectiveRoom = null;
    }
}

public class Objective
{
    //public string Name;

    public int ID { get; set; }
    public LanguageString DisplayText { get; set; }
    public bool Agent { get; set; } = false;
    public bool SpecialAgent { get; set; } = false;
    public bool PerfectAgent { get; set; } = false;
    public bool PerfectDark { get; set; } = false;
    public List<ObjectiveCondition> Conditions { get; set; } = new();

    public void DeleteCondition(ObjectiveCondition InCondition)
    {
        switch (InCondition.Type)
        {
            case ConditionType.DestroyObject:
            case ConditionType.CollectObject:
            case ConditionType.DepositObject:
            case ConditionType.HolographObject:
                if (InCondition.ObjectiveTag != null) { InCondition.ObjectiveTag.ConnectedObjectiveConditions.Remove(InCondition); }
                break;
            case ConditionType.CompletionFlags:
            case ConditionType.FailureFlags:
                if (InCondition.StageFlag != null) { InCondition.StageFlag.ConnectedObjectiveConditions.Remove(InCondition); }
                break;
            case ConditionType.EnterRoom:
                InCondition.ObjectiveRoom.ConnectedObjectiveConditions.Remove(InCondition);
                break;
        }
        Conditions.Remove(InCondition);

        // Reorder the remaining objective conditions, so the ID's match up and there isn't a hole
        for (int i = InCondition.ID; i < Conditions.Count; ++i)
        {
            ObjectiveCondition CurrentCondition = Conditions[i];
            CurrentCondition.ID -= 1;
        }
    }
}

public enum IntroType
{
    Spawn,
    Weapon,
    Ammo,
    PlayerOutfit,
    Case,
    CaseRespawn,
    Hill
}

public class IntroSetting
{
    private Pad _Pad;

    public static readonly string[] IntroNames =
    {
        "Spawn",
        "Weapon",
        "Ammo",
        "Player outfit",
        "Case",
        "Case respawn",
        "Hill",
    };

    public IntroType Type { get; set; }

    // Spawn, case, case respawn and hill
    public Pad IntroPad {
        get { return _Pad; }
        set { 
            _Pad = value;
            if (_Pad != null ) _Pad.ConnectedIntroSettings.Add(this);
        }
    }

    public string IntroName
    {
        get => IntroNames[(int)Type];
    }

    // Intro weapon
    public Constant WeaponType { get; set; }
    public int UnknownWeaponInt { get; set; }

    // Ammo
    public Constant AmmoType { get; set; }
    public int AmmoAmount { get; set; }

    // Outfit
    public Constant Outfit { get; set; }

    // Case & case respawn
    public int CaseID { get; set; }
}

public class GuardPath
{
    public int ID;
    public string Name;
    public List<Pad> Pads { get; } = new();
    public bool IsCircularPath;
    public bool IsFlyingPath;

    public void AddPad(Pad InPad)
    {
        Pads.Add(InPad);
        InPad.ConnectedPaths.Add(this);
    }

    public void AddPadAtIndex(int Index, Pad InPad)
    {
        Pads.Insert(Index, InPad);
        InPad.ConnectedPaths.Add(this);
    }

    public void RemovePad(Pad InPad)
    {
        Pads.Remove(InPad);
        InPad.ConnectedPaths.Remove(this);
    }
}

public class ModelData
{
    public string FileName { get; set; } // the file name constant located in files.h
    public int FileNumber { get; set; } // the file number, corresponds to the number in list.c
    public string FilePath { get; set; }
    public int Scale { get; set; }
    public string ExplosionType { get; set; }
    public bool IsMale { get; set; }
    public bool IsHead { get; set; }
    public bool HeightCanVary { get; set; }
    public string BodyType { get; set; }
    public int Height { get; set; }
    public string HandFileName { get; set; }
    public float CharacterScale { get; set; }
    public float AnimationScale { get; set; }
    public int ModelDef { get; set; }
}