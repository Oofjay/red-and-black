using System.Collections.Generic;
using UnityEngine;

public class DropdownOptions
{
    public static List<string> Pads = new();
    public static List<string> PadsWithNone = new();
    public static List<string> PadsWithPreset = new(); // includes the static option "pad preset"
    public static List<string> CoverPads = new();
    public static List<string> Waygroups = new();
    public static List<string> Props = new();
    public static List<string> PropsWithNone = new();
    public static List<string> Characters = new();
    public static List<string> CharactersWithPresets = new(); // includes static preset like current character, player character etc.
    public static List<string> DoorsWithNone = new();
    public static List<string> WeaponsWithNone = new();
    public static List<string> Tags = new();
    public static List<string> TagsWithNone = new();
    public static List<string> TagsAndCharacterPresets = new();

    public static List<string> Objectives = new();

    public static List<string> AILists = new();
    public static List<string> AssignableAILists = new();
    public static List<string> AICommands = new();

    public static List<string> Paths = new();

    public static List<string> Rooms = new();
    public static List<string> Portals = new();

    public static List<string> LanguageFiles = new();
    public static List<string> LanguageStrings = new();

    public static List<string> StageFlags = new();
    public static List<string> LockFlags = new();
    public static List<string> StageFlagsWithNone = new();
    public static List<string> LockFlagsWithNone = new();

    public static List<string> PropModels = new();
    public static List<string> BodyModels = new();
    public static List<string> HeadModels = new();

    public static List<string> Animations = new();

    public static List<string> SoundEffects = new();
    public static List<string> MusicTracks = new();

    public static List<string> ScreenImages = new();

    public static List<string> Stages = new();

    public static List<string> VisibilityCommands = new();

    public static Dictionary<string, List<string>> Constants = new()
    {
        { "DOORTYPE", new List<string>() },
        { "AMMOTYPE", new List<string>() },
        { "TEAM", new List<string>() },
        { "SQUADRON", new List<string>() },
        { "PADEFFECT", new List<string>() },
        { "OUTFIT", new List<string>() },
        { "HITPART", new List<string>() },
        { "GOPOSFLAG", new List<string>() },
        { "QUADRANT", new List<string>() },
        { "DIFF", new List<string>() },
        { "OPERATOR", new List<string>() },
        { "BANK", new List<string>() },
        { "COLOR", new List<string>() },
        { "CHANNEL", new List<string>() },
        { "XREASON", new List<string>() },
        { "LIGHTOP", new List<string>() },
        { "QUIP", new List<string>() },
        { "MA", new List<string>() },
        { "MODELPART", new List<string>() },
        { "GAMEFILEFLAG", new List<string>() },
        { "SPECIALDIE", new List<string>() },
        { "CIPQUIP", new List<string>() },
        { "AIENVCMD", new List<string>() },
        { "IDLEACTION", new List<string>() },
        { "PRESETANIM", new List<string>() },
        { "DRCAROLLIMAGE", new List<string>() },
        { "WEAPON", new List<string>() },
        { "CUTSCENEWEAPON", new List<string>() },
        { "TVPROGRAM", new List<string>() },
        { "PSTYPE", new List<string>() },
        { "TITLEAIMODE", new List<string>() },
        { "HOVTYPE", new List<string>() }
    };

    public static void RefreshAll()
    {
        RefreshGameOptions();
        RefreshSetupOptions();
    }

    public static void RefreshGameOptions()
    {
        RefreshAICommands();
        RefreshLanguageFiles();
        RefreshSetupBitFlags();
        RefreshModels();
        RefreshSounds();
        RefreshAnimations();
        RefreshScreenImages();
        RefreshStages();
        RefreshConstants();
        RefreshVisibilityCommands();
    }

    public static void RefreshSetupOptions()
    {
        RefreshObjects();
        RefreshWaygroups();
        RefreshCoverPads();
        RefreshBackground();
        RefreshObjectives();
        RefreshAILists();
        RefreshLanguageStrings();
        RefreshPaths();
        RefreshSetupBitFlags();
    }

    // Refreshes the options of the dropdowns that hold values from constants.h.
    public static void RefreshConstants()
    {
        foreach (KeyValuePair<string, List<Constant>> KVP in ConstantHolder.ConstantCollection)
        {
            List<string> OptionList = GetConstantOptions(KVP.Key);

            if (OptionList == null)
            {
                continue;
            }

            OptionList.Clear();
            List<Constant> CurrentConstantList = KVP.Value;

            foreach (Constant CurrentConstant in CurrentConstantList)
            {
                OptionList.Add(CurrentConstant.DisplayName ?? CurrentConstant.Name);
            }
        }

        // This is for the AI command set_cutscene_weapon, which has a specific option to use the current weapon or a fallback.
        // Other weapon dropdowns shouldn't use that option.
        Constants["CUTSCENEWEAPON"] = new(Constants["WEAPON"]);
        Constants["WEAPON"].Remove("WEAPON_CONTINUESAME");
    }

    public static List<string> GetConstantOptions(string ConstantType)
    {
        return Constants.GetValueOrDefault(ConstantType);
    }

    public static int GetConstantIndex(string ConstantType, string ConstantName)
    {
        return Constants[ConstantType].IndexOf(ConstantName);
    }

    public static int GetConstantIndex(string ConstantName)
    {
        string ConstantType = ConstantName[..ConstantName.IndexOf('_')];

        return Constants[ConstantType].IndexOf(ConstantName);
    }

    public static int GetConstantIndex(Constant InConstant)
    {
        if (InConstant == null)
        {
            return 0;
        }

        string ConstantType = InConstant.Name[..InConstant.Name.IndexOf('_')];

        return Constants[ConstantType].IndexOf(InConstant.Name);
    }

    public static Constant GetConstant(string ConstantType, int Index)
    {
        return ConstantHolder.ConstantCollection[ConstantType][Index];
    }

    public static void RefreshObjects()
    {
        Pads.Clear();
        PadsWithNone.Clear();
        Props.Clear();
        PropsWithNone.Clear();
        Characters.Clear();
        CharactersWithPresets.Clear();
        DoorsWithNone.Clear();
        WeaponsWithNone.Clear();
        Tags.Clear();
        TagsWithNone.Clear();
        TagsAndCharacterPresets.Clear();

        PadsWithNone.Add("None");
        PadsWithPreset.Add("Pad preset");
        PropsWithNone.Add("None");
        DoorsWithNone.Add("None");
        WeaponsWithNone.Add("None");
        TagsWithNone.Add("None");

        foreach (Pad IteratedPad in DataHolder.Pads)
        {
            Pads.Add(IteratedPad.NumberString);
        }

        PadsWithNone.AddRange(Pads);
        PadsWithPreset.AddRange(Pads);

        foreach (Prop IteratedProp in DataHolder.Props)
        {
            string PropName;
            switch (IteratedProp.Type)
            {
                case PropType.DoorScale:
                case PropType.LinkGuns:
                case PropType.LiftDoor:
                case PropType.Rename:
                case PropType.Tag:
                case PropType.TrainingHolographRequirement:
                case PropType.Character:
                    break;
                case PropType.Door:
                    PropName = IteratedProp.GetName();
                    Props.Add(PropName);
                    DoorsWithNone.Add(PropName);
                    break;
                case PropType.Weapon:
                    PropName = IteratedProp.GetName();
                    Props.Add(PropName);
                    WeaponsWithNone.Add(PropName);
                    break;
                default:
                    PropName = IteratedProp.GetName();
                    Props.Add(PropName);
                    break;
            }
        }

        // Tags are processed separately because DataHolder.Tags is sorted by tag ID, while DataHolder.Props isn't
        foreach (Prop IteratedTag in DataHolder.Tags)
        {
            Tags.Add( $"{IteratedTag.TagID:X2} - {IteratedTag?.TaggedProp?.GetName() ?? "No prop"}");
        }

        TagsWithNone.AddRange(Tags);
        PropsWithNone.AddRange(Props);

        // Process a separate character list that includes all the presets (like player character, target character).
        foreach (Prop IteratedCharacter in DataHolder.Characters)
        {
            if (!IteratedCharacter.IsAbstract)
            {
                Characters.Add(IteratedCharacter.GetName());
            }
            else
            {
                TagsAndCharacterPresets.Add(IteratedCharacter.GetName());
            }
            CharactersWithPresets.Add(IteratedCharacter.GetName());
        }

        TagsAndCharacterPresets.AddRange(Tags);
    }

    public static void RefreshCoverPads()
    {
        CoverPads.Clear();

        //CoverPads.Add("None"));

        foreach (Pad IteratedCoverPad in DataHolder.CoverPads)
        {
            CoverPads.Add(IteratedCoverPad.NumberString);
        }
    }

    public static void RefreshWaygroups()
    {
        Waygroups.Clear();

        foreach (Waygroup IteratedWaygroup in DataHolder.Waygroups)
        {
            Waygroups.Add(IteratedWaygroup.ID.ToString("X4"));
        }
    }

    public static void RefreshBackground()
    {
        Rooms.Clear();
        Portals.Clear();

        foreach (Room CurrentRoom in DataHolder.Rooms)
        {
            Rooms.Add(CurrentRoom.NumberString);
        }

        foreach (Portal CurrentPortal in DataHolder.Portals)
        {
            Portals.Add(CurrentPortal.NumberString);
        }
    }

    public static void RefreshObjectives()
    {
        Objectives.Clear();

        int Index = 1;
        foreach (Objective CurrentObjective in DataHolder.Objectives)
        {
            Objectives.Add($"{Index}: {CurrentObjective.DisplayText.Abbreviate()}");
            ++Index;
        }
    }

    public static void RefreshAILists()
    {
        AILists.Clear();
        AssignableAILists.Clear();

        foreach (AIList CurrentGlobalAIList in DataHolder.GlobalAILists)
        {
            AssignableAILists.Add($"{CurrentGlobalAIList.NumberString} {CurrentGlobalAIList.DisplayName}");
        }

        foreach (AIList CurrentAIList in DataHolder.AILists)
        {
            string Option = $"{CurrentAIList.NumberString} {CurrentAIList.DisplayName}";
            AILists.Add(Option);

            if (CurrentAIList.Type == AIListType.Character)
            {
                AssignableAILists.Add(Option);
            }
        }
    }

    public static void RefreshAICommands()
    {
        AICommands.Clear();

        foreach (AICommand CurrentCommand in DataHolder.AICommands)
        {
            AICommands.Add(CurrentCommand.DisplayString);
        }
    }

    public static void RefreshLanguageFiles()
    {
        LanguageFiles.Clear();

        foreach (LanguageBank CurrentLanguageFile in LanguageFileHolder.LanguageFiles)
        {
            LanguageFiles.Add(CurrentLanguageFile.Name);
        }
    }

    public static void RefreshLanguageStrings()
    {
        LanguageStrings.Clear();

        foreach (LanguageBank CurrentFile in LanguageFileHolder.LanguageFiles)
        {
            foreach (LanguageString CurrentLanguageString in CurrentFile.LanguageStrings)
            {
                LanguageStrings.Add(CurrentLanguageString.Abbreviate());
            }
        }
    }

    public static void RefreshPaths()
    {
        Paths.Clear();

        int PathIndex = 1;
        foreach (GuardPath CurrentPath in DataHolder.Paths)
        {
            Paths.Add("Path " + PathIndex.ToString());
            ++PathIndex;
        }
    }

    public static void RefreshSetupBitFlags()
    {
        StageFlags.Clear();
        LockFlags.Clear();
        StageFlagsWithNone.Clear();
        LockFlagsWithNone.Clear();

        StageFlagsWithNone.Add("None");
        LockFlagsWithNone.Add("None");

        foreach (BitFlag CurrentBitFlag in GameBitFlagHolder.GetStageFlags)
        {
            StageFlags.Add(CurrentBitFlag.Name ?? CurrentBitFlag.Constant);
            StageFlagsWithNone.Add(CurrentBitFlag.Name ?? CurrentBitFlag.Constant);
        }

        foreach (BitFlag CurrentBitFlag in GameBitFlagHolder.GetLockFlags)
        {
            LockFlags.Add(CurrentBitFlag.Name ?? CurrentBitFlag.Constant);
            LockFlagsWithNone.Add(CurrentBitFlag.Name ?? CurrentBitFlag.Constant);
        }
    }

    public static void RefreshModels()
    {
        PropModels.Clear();
        BodyModels.Clear();
        HeadModels.Clear();

        foreach (ModelData CurrentModel in DataHolder.PropDataList)
        {
            PropModels.Add(CurrentModel.FileName);
        }

        HeadModels.Add("Random");

        foreach (ModelData CurrentModel in DataHolder.CharacterDataList)
        {
            if (CurrentModel.IsHead)
            {
                HeadModels.Add(CurrentModel.FileName);
            }
            else
            {
                BodyModels.Add(CurrentModel.FileName);
            }
        }
    }

    public static void RefreshAnimations()
    {
        Animations.Clear();

        foreach (AnimationSequence CurrentAnimation in DataHolder.Animations)
        {
            Animations.Add(CurrentAnimation.DisplayName ?? CurrentAnimation.Name);
        }
    }

    public static void RefreshScreenImages()
    {
        ScreenImages.Clear();

        foreach (ScreenImage CurrentImage in DataHolder.ScreenImages)
        {
            ScreenImages.Add(CurrentImage.DisplayName ?? CurrentImage.Name);
        }
    }

    public static void RefreshStages()
    {
        Stages.Clear();

        foreach (Stage CurrentStage in DataHolder.Stages)
        {
            Stages.Add(CurrentStage.Name);
        }
    }

    public static void RefreshSounds()
    {
        SoundEffects.Clear();
        MusicTracks.Clear();

        foreach (SoundEffect CurrentSoundEffect in DataHolder.SoundEffects)
        {
            SoundEffects.Add(CurrentSoundEffect.Name);
        }

        foreach (MusicTrack CurrentMusicTrack in DataHolder.MusicTracks)
        {
            MusicTracks.Add(CurrentMusicTrack.Name);
        }
    }

    public static void RefreshVisibilityCommands()
    {
        VisibilityCommands.Clear();

        foreach (KeyValuePair<int, string> CurrentCommand in VisibilityCommand.CommandNames)
        {
            VisibilityCommands.Add(CurrentCommand.Key.ToString("X2") + " " + CurrentCommand.Value);
        }
    }

    public static int FindLanguageStringIndex(LanguageString InLanguageString)
    {
        if (InLanguageString == null)
        {
            return 0;
        }
        return LanguageFileHolder.GetLevelLanguageFile().LanguageStrings.IndexOf(InLanguageString);
    }

    public static Pad FindPad(string Value)
    {
        if (Value == "None")
        {
            return null;
        }
        else if (Value == "Pad preset")
        {
            return DataHolder.PadPreset;
        }
        return DataHolder.Pads.Find(x => x.NumberString == Value);
    }

    public static int FindPadIndex(Pad InPad, bool WithNoneOrPreset)
    {
        if (InPad == null)
        {
            return 0;
        }
        else if (InPad == DataHolder.PadPreset)
        {
            return 0;
        }
        return Pads.IndexOf(InPad.NumberString) + (WithNoneOrPreset ? 1 : 0);
    }

    public static int FindWaygroup(string WaygroupID)
    {
        return int.Parse(WaygroupID, System.Globalization.NumberStyles.HexNumber);
    }

    public static int FindWaygroupIndex(int WaygroupID)
    {
        return Waygroups.IndexOf(WaygroupID.ToString("X4"));
    }

    public static int FindPathIndex(GuardPath Path)
    {
        if (Path == null)
        {
            return 0;
        }
        return Paths.IndexOf(Path.ID.ToString());
    }

    public static Prop FindProp(int Index, bool WithNone)
    {
        if (WithNone && Index == 0)
        {
            return null;
        }
        else
        {
            return DataHolder.VisibleProps[Index - (WithNone ? 1 : 0)];
        }
    }

    public static int FindPropIndex(Prop InProp, bool WithNone)
    {
        if (InProp == null)
        {
            return 0;
        }
        return DataHolder.VisibleProps.FindIndex(x => x.InternalID == InProp.InternalID) + (WithNone ? 1 : 0);
    }

    public static Prop FindCharacter(string Value)
    {
        int Index = int.Parse(Value[..2], System.Globalization.NumberStyles.HexNumber);

        return DataHolder.Characters.Find(x => x.CharacterID == Index);
    }

    public static int FindCharacterIndex(Prop InProp, bool WithPresets)
    {
        if (InProp == null)
        {
            return 0;
        }

        return DataHolder.Characters.IndexOf(InProp) - (WithPresets ? 0 : 12);
    }

    public static int FindDoorIndex(Prop Door)
    {
        if (Door == null)
        {
            return 0;
        }
        return DoorsWithNone.IndexOf(Door.GetName());
    }

    public static int FindWeaponIndex(Prop Weapon)
    {
        if (Weapon == null)
        {
            return 0;
        }
        Debug.Log("Weapon name: " + Weapon.GetName());
        return WeaponsWithNone.IndexOf(Weapon.GetName());
    }

    public static Prop FindTag(int Index, bool WithNone)
    {
        if (WithNone && Index == 0)
        {
            return null;
        }
        else
        {
            return DataHolder.Tags[Index - (WithNone ? 1 : 0)];
        }
    }

    public static int FindTagIndex(Prop Tag, bool WithNone)
    {
        if (Tag == null)
        {
            return 0;
        }

        return DataHolder.Tags.IndexOf(Tag) + (WithNone ? 1 : 0);
    }

    public static int FindAIListIndex(AIList InAIList)
    {
        if (InAIList == null)
        {
            return 0;
        }

        return DataHolder.AILists.IndexOf(InAIList);
    }

    public static int FindAssignableAIListIndex(AIList InAIList)
    {
        if (InAIList == null)
        {
            return 0;
        }

        return AssignableAILists.FindIndex(x => x[..4] == InAIList.NumberString);
    }

    public static AIList FindAssignableAIList(string Value)
    {
        AIList GlobalAIList = DataHolder.GlobalAILists.Find(x => x.NumberString == Value[..4]);

        if (GlobalAIList != null)
        {
            return GlobalAIList;
        }
        else
        {
            return DataHolder.CharacterAILists.Find(x => x.NumberString == Value[..4]);
        }
    }

    public static ModelData FindPropModel(string Value)
    {
        return DataHolder.PropDataList.Find(x => x.FileName == Value);
    }

    public static int FindPropModelIndex(ModelData InModelData)
    {
        if (InModelData == null)
        {
            return 0;
        }

        return PropModels.IndexOf(InModelData.FileName);
    }

    public static int FindBodyModelIndex(ModelData InModelData)
    {
        if (InModelData == null)
        {
            return 0;
        }

        return BodyModels.IndexOf(InModelData.FileName);
    }

    public static int FindHeadModelIndex(ModelData InModelData)
    {
        Debug.Log(InModelData.FileName);
        if (InModelData == null || InModelData == DataHolder.RandomHeadData)
        {
            return 0;
        }

        return HeadModels.IndexOf(InModelData.FileName);
    }

    public static ModelData FindCharacterModel(string Value)
    {
        if (Value == "Random")
        {
            return DataHolder.RandomHeadData;
        }

        return DataHolder.CharacterDataList.Find(x => x.FileName == Value);
    }
}
