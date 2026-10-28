using System.Collections.Generic;
using System.Linq;

public enum AIListType
{
    Global,        // 0x00XX
    Character,     // 0x04XX
    Cutscene,      // 0x0CXX
    Background,    // 0x10XX
    Environmental  // 0x14XX
}

public class AIListUsage
{
    public AICommandInstance CommandInstance;
    public int ParameterID;
}

public class Label
{
    public int ID;
    public string Name;
    public string DisplayString => $"{ID:X2}: {Name}";
}

public class ParameterType
{
    public string ClassType;
    public string Name;
    public string ConstantName;
}

public class AICommand
{
    public int ID;

    public string Name; // Used in decomp code
    public string DisplayName;
    public string Description;

    public string NumberString => ID.ToString("X3");
    public string DisplayString => NumberString + " " + DisplayName;

    public List<ParameterType> ParameterTypes = new();

    public bool CharacterOnly; // Is this usable in 0x04XX AI lists only?
}

public class AICommandInstance
{
    public AIList ParentAIList;
    public AICommand Command;
    public List<object> Parameters = new();
}

public class AIList
{
    public int ID;
    public string NumberString => ID.ToString("X4");
    public string Name;
    public string DisplayName;
    public string Constant; // Used only for global AI lists. These use a C constant (e.g. "GAILIST_IDLE") instead of an ID.

    public AIListType Type;

    public List<AICommandInstance> Commands = new();
    public List<Label> Labels = new();
    public List<string> LabelOptions = new();

    public List<Prop> ConnectedProps { get; set; } = new();

    public Label GetNewOrExistingLabel(int LabelNumber)
    {
        Label FoundLabel = Labels.Find(x => x.ID == LabelNumber);

        if (FoundLabel != null)
        {
            return FoundLabel;
        }
        else
        {
            return CreateNewLabel(LabelNumber);
        }
    }

    object GetDefaultParameter(ParameterType InType)
    {
        return InType.ClassType switch
        {
            "System.Int32" or "System.Int16" or "Int24" => 0,
            "System.Byte" => (byte)0,
            "Label" => Labels.FirstOrDefault(),
            "Character" => DataHolder.Characters.FirstOrDefault(),
            "AIList" => DataHolder.GlobalAILists.FirstOrDefault(),
            "Animation" => DataHolder.Animations.FirstOrDefault(),
            "Model" => DataHolder.PropDataList.FirstOrDefault(),
            "Pad" => DataHolder.Pads.FirstOrDefault(),
            "Path" => DataHolder.Paths.FirstOrDefault(),
            "Room" => DataHolder.Rooms.FirstOrDefault(),
            "System.Boolean" => false,
            "AimEntity" => 0,
            "Prop" => DataHolder.Tags.FirstOrDefault(),
            "Objective" => DataHolder.Objectives.FirstOrDefault(),
            "Stage" => DataHolder.Stages.FirstOrDefault(),
            "Body" => DataHolder.CharacterDataList.Find(x => x.IsHead == false),
            "Head" => DataHolder.CharacterDataList.Find(x => x.IsHead == true),
            "LanguageString" => LanguageFileHolder.GetLevelLanguageFile().LanguageStrings.FirstOrDefault(),
            "SoundEffect" => DataHolder.SoundEffects.FirstOrDefault(),
            "ScreenImage" => DataHolder.ScreenImages.FirstOrDefault(),
            "MusicTrack" => DataHolder.MusicTracks.FirstOrDefault(),
            "Portal" => DataHolder.Portals.FirstOrDefault(),
            "Quip" => 0,
            "SpecialSoundBank" => 0,
            "Constant" => ConstantHolder.GetConstantList(InType.ConstantName).FirstOrDefault(),
            "BitFlag" => new BitFlagCollection(),
            _ => null,
        };
    }

    public AICommandInstance AddEmptyCommand(AICommand CommandType)
    {
        AICommandInstance NewCommandInstance = new()
        {
            ParentAIList = this,
            Command = CommandType
        };

        Commands.Add(NewCommandInstance);

        return NewCommandInstance;
    }

    public AICommandInstance InsertCommand(int Index, AICommand CommandType)
    {
        AICommandInstance NewCommandInstance = new()
        {
            ParentAIList = this,
            Command = CommandType,
        };

        foreach (ParameterType CurrentParameter in CommandType.ParameterTypes)
        {
            NewCommandInstance.Parameters.Add(GetDefaultParameter(CurrentParameter));
        }

        Commands.Insert(Index, NewCommandInstance);

        return NewCommandInstance;
    }

    public void DeleteCommand(AICommandInstance CommandInstance)
    {
        // Remove all Prop references

        Commands.Remove(CommandInstance);
    }

    public Label CreateNewLabel(int ID)
    {
        Label NewLabel = new() { ID = ID, Name = "Label" };
        Labels.Add(NewLabel);
        LabelOptions.Add(NewLabel.DisplayString);

        return NewLabel;
    }

    public Label CreateNewLabel()
    {
        int Index = 0;
        foreach (Label CurrentLabel in Labels)
        {
            if (CurrentLabel.ID == Index)
            {
                ++Index;
                continue;
            }
            else
            {
                break;
            }
        }
        return CreateNewLabel(Index);
    }

    public void DeleteLabel(Label InLabel)
    {
        Labels.Remove(InLabel);
        RefreshLabelOptions();
    }

    public void RefreshLabelOptions()
    {
        LabelOptions.Clear();

        foreach (Label CurrentLabel in Labels)
        {
            LabelOptions.Add(CurrentLabel.DisplayString);
        }
    }

    public bool ChangeType(AIListType NewType)
    {
        AIListType OldType = Type;
        Type = NewType;

        int CurrentID = 0;
        int Index = 0;

        switch (NewType)
        {
            case AIListType.Character:
                CurrentID = 0x0401;

                foreach (AIList CurrentAIList in DataHolder.CharacterAILists)
                {
                    //Debug.Log(ID + " " + CurrentAIList.ID);
                    if (CurrentID > 0x04FF) // Maximum number of AI lists
                    {
                        return false;
                    }
                    else if (CurrentID == CurrentAIList.ID)
                    {
                        ++CurrentID;
                        ++Index;
                        continue;
                    }
                    else
                    {
                        DataHolder.CharacterAILists.Insert(Index, this);
                        break;
                    }
                }
                break;
            case AIListType.Cutscene:
                CurrentID = 0x0C00;

                foreach (AIList CurrentAIList in DataHolder.CutsceneAILists)
                {
                    //Debug.Log(ID + " " + CurrentAIList.ID);
                    if (CurrentID > 0x0CFF) // Maximum number of AI lists
                    {
                        return false;
                    }
                    else if (CurrentID == CurrentAIList.ID)
                    {
                        ++CurrentID;
                        ++Index;
                        continue;
                    }
                    else
                    {
                        DataHolder.CutsceneAILists.Insert(Index, this);
                        break;
                    }
                }
                break;
            case AIListType.Background:
                CurrentID = 0x1000;

                foreach (AIList CurrentAIList in DataHolder.BackgroundAILists)
                {
                    //Debug.Log(ID + " " + CurrentAIList.ID);
                    if (CurrentID > 0x10FF) // Maximum number of AI lists
                    {
                        return false;
                    }
                    else if (CurrentID == CurrentAIList.ID)
                    {
                        ++CurrentID;
                        ++Index;
                        continue;
                    }
                    else
                    {
                        DataHolder.BackgroundAILists.Insert(Index, this);
                        break;
                    }
                }
                break;
            case AIListType.Environmental:
                CurrentID = 0x1400;

                foreach (AIList CurrentAIList in DataHolder.EnvironmentalAILists)
                {
                    //Debug.Log(ID + " " + CurrentAIList.ID);
                    if (CurrentID > 0x14FF) // Maximum number of AI lists
                    {
                        return false;
                    }
                    else if (CurrentID == CurrentAIList.ID)
                    {
                        ++CurrentID;
                        ++Index;
                        continue;
                    }
                    else
                    {
                        DataHolder.EnvironmentalAILists.Insert(Index, this);
                        break;
                    }
                }
                break;
        }

        ID = CurrentID;

        switch (OldType)
        {
            case AIListType.Character:
                DataHolder.CharacterAILists.Remove(this);
                break;
            case AIListType.Cutscene:
                DataHolder.CutsceneAILists.Remove(this);
                break;
            case AIListType.Background:
                DataHolder.BackgroundAILists.Remove(this);
                break;
            case AIListType.Environmental:
                DataHolder.EnvironmentalAILists.Remove(this);
                break;
        }

        DataHolder.AILists.Sort((x, y) => x.ID.CompareTo(y.ID));
        DropdownOptions.RefreshAILists();

        return true;

    }
}
