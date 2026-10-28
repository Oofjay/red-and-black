using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public class BinaryFileWriter
{
    private BetterBinaryWriter Writer;

    // The stream that we currently need to write to. Set this each time when you want switch between writing to the main file and data that needs to be compressed later,
    // since many methods write to CurrentStream and need access to the proper stream.
    private Stream CurrentStream;

    private FileStream MainFileStream;

    private string FileName = "";

    // How many lines of briefing and objectives are there in the file? This is to calculate the index of a certain prop among all objects,
    // so the index of a prop (among all PD object lines in the file) is BriefingLines + PropIndex.
    int BriefingLines = 0;
    int PropIndex = 0;

    List<Pad> Pads = DataHolder.Pads;
    List<Pad> CoverPads = DataHolder.CoverPads;
    List<Waypoint> Waypoints = DataHolder.Waypoints;
    List<Waygroup> Waygroups = DataHolder.Waygroups;

    List<Objective> Objectives = DataHolder.Objectives;
    List<Prop> Props = DataHolder.Props;
    List<IntroSetting> IntroSettings = DataHolder.IntroSettings;
    List<GuardPath> Paths = DataHolder.Paths;
    List<AIList> AILists = DataHolder.AILists;

    public bool WritePads(string FilePath)
    {
        FileName = Path.GetFileNameWithoutExtension(FilePath);
        MainFileStream = File.Open(FilePath, FileMode.Create);

        // -- Pads --

        MemoryStream PadList = new();
        SetAsCurrentStream(PadList);

        int NumPads = Pads.Count;
        int PadsStartPosition = 20 + NumPads * 2;

        short[] PadPointers;

        // If the end of the pad pointer list doesn't line up with a multiple of 4 bytes, make sure it does by padding it with enough 0's
        if (PadsStartPosition % 4 != 0)
        {
            PadsStartPosition += 2;
            PadPointers = new short[Pads.Count + 1];
            PadPointers[^1] = 0;
        }
        else
        {
            PadPointers = new short[Pads.Count];
        }

        for (int i = 0; i < NumPads; ++i)
        {
            PadPointers[i] = (short)(PadsStartPosition + PadList.Position);

            Pad CurrentPad = Pads[i];

            int PadInfo = 0;
            Vector3 PadPos = CurrentPad.Position;

            if (PadPos.x == Math.Floor(PadPos.x + 0.00001f) && PadPos.y == Math.Floor(PadPos.y + 0.00001f) && PadPos.z == Math.Floor(PadPos.z + 0.00001f))
            {
                PadInfo |= 0x00004000;
                CurrentPad.IntegerPosition = true;
            }

            if (CurrentPad.UpAlignToX) { PadInfo |= 0x00008000; }
            if (CurrentPad.UpAlignToY) { PadInfo |= 0x00010000; }
            if (CurrentPad.UpAlignToZ) { PadInfo |= 0x00020000; }
            if (CurrentPad.UpAlignInverted) { PadInfo |= 0x00040000; }
            if (CurrentPad.DirectionAlignToX) { PadInfo |= 0x00080000; }
            if (CurrentPad.DirectionAlignToY) { PadInfo |= 0x00100000; }
            if (CurrentPad.DirectionAlignToZ) { PadInfo |= 0x00200000; }
            if (CurrentPad.DirectionAlignInverted) { PadInfo |= 0x00400000; }
            if (CurrentPad.HasBoundingBoxData) { PadInfo |= 0x00800000; }
            if (CurrentPad.AIWaitsForLift) { PadInfo |= 0x01000000; }
            if (CurrentPad.AIRidesLift) { PadInfo |= 0x02000000; }
            if (CurrentPad.AIWalksDirectlyTo) { PadInfo |= 0x04000000; }
            if (CurrentPad.AIDrops) { PadInfo |= 0x08000000; }
            if (CurrentPad.AICrouches) { PadInfo |= 0x10000000; }
            if (CurrentPad.AIMovesWhileFalling) { PadInfo |= 0x20000000; }
            if (CurrentPad.AIDucks) { PadInfo |= 0x40000000; }

            PadInfo |= 0x00003FF0; // Room number, which is only read in runtime and just set to 1's in the ROM
            PadInfo |= CurrentPad.LiftNumber;

            Writer.Write(PadInfo);

            if (CurrentPad.IntegerPosition)
            {
                Writer.Write((short)PadPos.x);
                Writer.Write((short)PadPos.y);
                Writer.Write((short)-PadPos.z);
                Writer.Write((short)0);
            }
            else
            {
                Writer.Write(PadPos.x);
                Writer.Write(PadPos.y);
                Writer.Write(-PadPos.z);
            }

            if (!CurrentPad.UpAlignToX && !CurrentPad.UpAlignToY && !CurrentPad.UpAlignToZ)
            {
                Writer.Write(CurrentPad.Up.x);
                Writer.Write(CurrentPad.Up.y);
                Writer.Write(-CurrentPad.Up.z);
            }

            if (!CurrentPad.DirectionAlignToX && !CurrentPad.DirectionAlignToY && !CurrentPad.DirectionAlignToZ)
            {
                Writer.Write(CurrentPad.Direction.x);
                Writer.Write(CurrentPad.Direction.y);
                Writer.Write(-CurrentPad.Direction.z);
            }

            if (CurrentPad.HasBoundingBoxData)
            {
                Writer.Write(CurrentPad.XMin);
                Writer.Write(CurrentPad.XMax);
                Writer.Write(CurrentPad.YMin);
                Writer.Write(CurrentPad.YMax);
                Writer.Write(CurrentPad.ZMin);
                Writer.Write(CurrentPad.ZMax);
            }
        }

        // -- Waypoints --

        MemoryStream WaypointList = new();
        SetAsCurrentStream(WaypointList);

        MemoryStream WaypointNeighbourList = new();
        BetterBinaryWriter NeighbourWriter = new(WaypointNeighbourList);

        int NumWaypoints = Waypoints.Count;

        int WaypointPointer = 20 + PadPointers.Length * 2 + (int)PadList.Length;
        int CurrentNeighbourPointer = WaypointPointer + NumWaypoints * 16 + 16;

        for (int i = 0; i < NumWaypoints; ++i)
        {
            Waypoint CurrentWaypoint = Waypoints[i];

            Writer.Write(CurrentWaypoint.ParentPad.Number);
            Writer.Write(CurrentNeighbourPointer);
            CurrentNeighbourPointer += CurrentWaypoint.Neighbours.Count * 4 + 4;
            Writer.Write(CurrentWaypoint.WaygroupID);
            Writer.Write(0);

            foreach (WaypointNeighbour CurrentNeighbour in CurrentWaypoint.Neighbours)
            {
                int NeighbourNumber = CurrentNeighbour.LinkedWaypoint.ID;

                if (CurrentNeighbour.MoveOutwardsOnly) { NeighbourNumber |= 0x4000; }
                if (CurrentNeighbour.MoveInwardsOnly) { NeighbourNumber |= 0x8000; }

                NeighbourWriter.Write(NeighbourNumber);
            }

            NeighbourWriter.Write(0xFFFFFFFF);
        }

        Writer.Write(0xFFFFFFFF);
        Writer.Write(0);
        Writer.Write(0);
        Writer.Write(0);

        // -- Waygroups --

        int NumWaygroups = Waygroups.Count;

        int WaygroupPointer = WaypointPointer + (int)WaypointList.Length + (int)WaypointNeighbourList.Length;

        MemoryStream WaygroupWaypointList = new();
        SetAsCurrentStream(WaygroupWaypointList);

        int[] WaypointPointers = new int[NumWaygroups];
        int WaygroupWaypointsStart = WaygroupPointer + NumWaygroups * 12 + 12;

        for (int i = 0; i < NumWaygroups; ++i)
        {
            Waygroup CurrentWaygroup = Waygroups[i];

            WaypointPointers[i] = WaygroupWaypointsStart + (int)WaygroupWaypointList.Position;

            foreach (Waypoint CurrentWaypoint in CurrentWaygroup.Waypoints)
            {
                Writer.Write(CurrentWaypoint.ID);
            }
            Writer.Write(0xFFFFFFFF);
        }

        MemoryStream WaygroupNeighbourList = new();
        SetAsCurrentStream(WaygroupNeighbourList);

        int[] NeighbourPointers = new int[NumWaygroups];
        int WaygroupNeighboursStart = WaygroupPointer + NumWaygroups * 12 + 12 + (int)WaygroupWaypointList.Length;

        for (int i = 0; i < NumWaygroups; ++i)
        {
            Waygroup CurrentWaygroup = Waygroups[i];

            NeighbourPointers[i] = WaygroupNeighboursStart + (int)WaygroupNeighbourList.Position;

            foreach (WaygroupNeighbour CurrentNeighbour in CurrentWaygroup.Neighbours)
            {
                int NeighbourNumber = CurrentNeighbour.LinkedWaygroup.ID;

                if (CurrentNeighbour.MoveOutwardsOnly) { NeighbourNumber |= 0x4000; }
                if (CurrentNeighbour.MoveInwardsOnly) { NeighbourNumber |= 0x8000; }

                Writer.Write(NeighbourNumber);
            }
            Writer.Write(0xFFFFFFFF);
        }

        MemoryStream WaygroupList = new();
        SetAsCurrentStream(WaygroupList);

        for (int i = 0; i < NumWaygroups; ++i)
        {
            Waygroup CurrentWaygroup = Waygroups[i];

            Writer.Write(NeighbourPointers[i]);
            Writer.Write(WaypointPointers[i]);
            Writer.Write(0);
        }

        Writer.Write(0);
        Writer.Write(0);
        Writer.Write(0);

        // -- Cover pads --

        MemoryStream CoverPadList = new();
        SetAsCurrentStream(CoverPadList);

        int NumCoverPads = CoverPads.Count;

        int CoverPadPointer = WaygroupNeighboursStart + (int)WaygroupNeighbourList.Length;

        for (int i = 0; i < NumCoverPads; ++i)
        {
            Pad CurrentCoverPad = CoverPads[i];

            Writer.Write(CurrentCoverPad.Position.x);
            Writer.Write(CurrentCoverPad.Position.y);
            Writer.Write(-CurrentCoverPad.Position.z);

            Writer.Write(CurrentCoverPad.Direction.x);
            Writer.Write(CurrentCoverPad.Direction.y);
            Writer.Write(-CurrentCoverPad.Direction.z);

            Writer.Write(CurrentCoverPad.CoverFlags);
        }

        SetAsCurrentStream(MainFileStream);

        Writer.Write(NumPads);
        Writer.Write(NumCoverPads);
        Writer.Write(WaypointPointer);
        Writer.Write(WaygroupPointer);
        Writer.Write(CoverPadPointer);

        foreach (short CurrentPointer in PadPointers)
        {
            Writer.Write(CurrentPointer);
        }

        PadList.WriteTo(MainFileStream);
        WaypointList.WriteTo(MainFileStream);
        WaypointNeighbourList.WriteTo(MainFileStream);
        WaygroupList.WriteTo(MainFileStream);
        WaygroupWaypointList.WriteTo(MainFileStream);
        WaygroupNeighbourList.WriteTo(MainFileStream);
        CoverPadList.WriteTo(MainFileStream);

        if (MainFileStream.Position % 16 != 0)
        {
            int AmountOfZeroes = 16 - ((int)MainFileStream.Position % 16);

            for (int i = 0; i < AmountOfZeroes; ++i)
            {
                Writer.Write((byte)0);
            }
        }

        Writer.Dispose();
        NeighbourWriter.Dispose();
        MainFileStream.Dispose();

        return true;
    }

    public void WriteAIListDebugFile(string FilePath) // FOR TESTING ONLY
    {
        FileName = Path.GetFileNameWithoutExtension(FilePath);
        MainFileStream = File.Open("C:\\Users\\Oofjay\\Desktop\\Temp\\Setup files\\AI Lists\\" + FileName + ".bin", FileMode.Create);

        Writer = new(MainFileStream);

        foreach (AIList CurrentAIList in AILists)
        {
            Writer.Write("AILIST");
            Writer.Write((short)CurrentAIList.ID);

            foreach (AICommandInstance CurrentCommandInstance in CurrentAIList.Commands)
            {
                AICommand CurrentCommand = CurrentCommandInstance.Command;

                Writer.Write((short)CurrentCommand.ID);

                int ParameterID = 0;

                foreach (ParameterType CurrentParameterType in CurrentCommand.ParameterTypes)
                {
                    object CurrentParameter = null;

                    if (CurrentParameterType.ClassType != "null")
                    {
                        CurrentParameter = CurrentCommandInstance.Parameters[ParameterID];
                        ++ParameterID;
                    }

                    // Debug.Log($"List: {CurrentAIList.ID:X4}, {CurrentCommand.ID:X3}/{CurrentCommand.Name}: cmd {commandinstnum}, param {ParameterID}, type {CurrentParameterType.ClassType}");

                    switch (CurrentParameterType.ClassType)
                    {
                        case "System.Int32":
                            Writer.Write((int)CurrentParameter);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {(int)NewCommandInstance.Parameters[^1]}");
                            break;
                        case "Int24":
                            byte[] Bytes = BitConverter.GetBytes((int)CurrentParameter);

                            int param = (int)CurrentParameter;

                            Writer.Write(Bytes[2]);
                            Writer.Write(Bytes[1]);
                            Writer.Write(Bytes[0]);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {(int)NewCommandInstance.Parameters[^1]}");
                            break;
                        case "System.Int16":
                            int NumberParameter = (int)CurrentParameter;
                            Writer.Write((short)NumberParameter);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {(int)NewCommandInstance.Parameters[^1]}");
                            break;
                        case "System.Byte":
                            NumberParameter = (int)CurrentParameter;
                            Writer.Write((byte)NumberParameter);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {(int)NewCommandInstance.Parameters[^1]}");
                            break;
                        case "String":
                            byte[] DebugCommentArray = Encoding.UTF8.GetBytes((string)CurrentParameter);

                            Writer.Write(DebugCommentArray);
                            Writer.Write((byte)0);
                            //Debug.Log($"Parameter added to {NewAIList.ID} in {CurrentCommand.Name}: {DebugComment}");
                            break;
                        case "Label":
                            Label CurrentLabel = (Label)CurrentParameter;

                            Writer.Write((byte)CurrentLabel.ID);
                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {Parameter.ID}");
                            break;
                        case "Character":
                            if (CurrentParameter is Prop CurrentCharacter)
                            {
                                Writer.Write((byte)CurrentCharacter.CharacterID);
                            }
                            else
                            {
                                NumberParameter = (int)CurrentParameter;
                                Writer.Write((byte)NumberParameter);
                            }

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {chr?.GetName()}");
                            break;
                        case "AIList":
                            AIList ReferencedAIList = (AIList)CurrentParameter;
                            Writer.Write((short)ReferencedAIList.ID);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {AIListID}");
                            break;
                        case "Animation":
                            Writer.Write((short)DataHolder.Animations.IndexOf((AnimationSequence)CurrentParameter));

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {AnimationParameter.Name}");
                            break;
                        case "Model":
                            Writer.Write((short)DataHolder.PropDataList.IndexOf((ModelData)CurrentParameter));

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {AnimationParameter.Name}");
                            break;
                        case "Pad":
                            Pad CurrentPad = (Pad)CurrentParameter;
                            Writer.Write((short)CurrentPad.Number);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {newpad?.name}");
                            break;
                        case "Path":
                            GuardPath ParameterPath = (GuardPath)CurrentParameter;

                            if (ParameterPath == null)
                            {
                                Writer.Write((byte)0xFF);
                            }
                            else
                            {
                                Writer.Write((byte)ParameterPath.ID);
                            }

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {PathID}");
                            break;
                        case "Room":
                            // If the second parameter of action 0x005B is 1, the room parameter is not used.
                            // If it is 2, it's a pad, not a room (hardcoded to work only in G5 Building.)
                            if (CurrentCommand.ID == 0x005B)
                            {
                                int RoomOrPad = (int)CurrentCommandInstance.Parameters[1];

                                if (RoomOrPad == 1)
                                {
                                    Writer.Write((short)0);

                                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: No room");
                                }
                                else if (RoomOrPad == 2)
                                {
                                    Pad PadParameter = (Pad)CurrentParameter;
                                    Writer.Write((short)PadParameter.Number);

                                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {PadParameter.NumberString}");
                                }
                                else
                                {
                                    Room RoomParameter = (Room)CurrentParameter;
                                    Writer.Write(RoomParameter != null ? (short)RoomParameter.ID : (short)0);
                                }
                            }
                            else
                            {
                                Room RoomParameter = (Room)CurrentParameter;
                                Writer.Write(RoomParameter != null ? (short)RoomParameter.ID : (short)0);
                            }

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {RoomParameter?.NumberString}");
                            break;
                        case "System.Boolean":
                            Writer.Write((bool)CurrentParameter);
                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {NewBool}");
                            break;
                        case "AimEntity":
                            BitFlagCollection AimFlags = (BitFlagCollection)CurrentCommandInstance.Parameters[0];

                            if (AimFlags.HasBits(0x0004)) // Aim at character
                            {
                                Prop AimCharacter = (Prop)CurrentParameter;
                                Writer.Write((short)AimCharacter.CharacterID);
                                //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {AimCharacter?.CharacterID}");
                            }
                            else if (AimFlags.HasBits(0x0008)) // Aim at pad
                            {
                                Pad AimPad = (Pad)CurrentParameter;
                                Writer.Write((short)AimPad.Number);
                                //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {AimPad?.NumberString}");
                            }
                            else if (AimFlags.HasBits(0x0010)) // Aim at direction
                            {
                                int AimDirection = (int)CurrentParameter;
                                Writer.Write((short)AimDirection);
                                //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {AimDirection}");
                            }
                            else if (AimFlags.HasBits(0x0200)) // Aim at target
                            {
                                short PlaysWalkAnimation = System.Convert.ToInt16((bool)CurrentParameter);
                                Writer.Write(PlaysWalkAnimation);
                                //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {PlaysWalkAnimation}");
                            }
                            else
                            {
                                Writer.Write((short)0);
                                //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: null");
                            }
                            break;
                        case "Prop":
                            Prop ParameterTag = (Prop)CurrentParameter;

                            if (ParameterTag == null)
                            {
                                //Debug.LogWarning($"Prop parameter does not exist at: {CurrentStream.Position:X}");
                                Writer.Write((byte)0xFF);
                            }
                            else
                            {
                                Writer.Write((byte)ParameterTag.TagID);
                            }

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {TagParameter.TagID}");
                            break;
                        case "PropOrCharacter":
                            Prop PropParameter = (Prop)CurrentParameter;

                            if (PropParameter.IsAbstract) // Character preset
                            {
                                Writer.Write((byte)PropParameter.CharacterID);

                                //Debug.Log($"Parameter added to {InAIList.Name} in {CurrentCommand.Name}: {CharacterParameter.CharacterID}");
                            }
                            else // Prop tag
                            {
                                Writer.Write((byte)PropParameter.TagID);

                                //Debug.Log($"Parameter added to {InAIList.Name} in {CurrentCommand.Name}: {TagParameter.TagID}");
                            }
                            break;
                        case "Objective":
                            Objective ObjectiveParameter = (Objective)CurrentParameter;
                            Writer.Write((byte)ObjectiveParameter.ID);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {ObjectiveParameter.DisplayText?.en}");
                            break;
                        case "Stage":
                            Stage StageParameter = (Stage)CurrentParameter;
                            Writer.Write((byte)StageParameter.ID);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {StageParameter.Name}");
                            break;
                        case "Body":
                            Writer.Write((byte)DataHolder.CharacterDataList.IndexOf((ModelData)CurrentParameter));
                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {BodyData.FileName}");
                            break;
                        case "Head":
                            ModelData ModelDataParameter = (ModelData)CurrentParameter;

                            if (ModelDataParameter == DataHolder.RandomHeadData)
                            {
                                Writer.Write((byte)0xFF);
                                //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {HeadData.FileName}");
                            }
                            else
                            {
                                Writer.Write((byte)DataHolder.CharacterDataList.IndexOf(ModelDataParameter));
                            }
                            break;
                        case "LanguageString":
                            WriteShortLanguageString((LanguageString)CurrentParameter);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {LanguageStringParameter?.en}");
                            break;
                        case "SoundEffect":
                            int SoundEffectParameter = (int)CurrentParameter;

                            Writer.Write((short)SoundEffectParameter);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {DataHolder.SoundEffects.Find(x => x.Name == SoundEffectName).Name}");
                            break;
                        case "MusicTrack":
                            Writer.Write((byte)DataHolder.MusicTracks.IndexOf((MusicTrack)CurrentParameter));

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {DataHolder.MusicTracks.Find(x => x.Name == MusicTrackName).Name}");
                            break;
                        case "Portal":
                            Portal PortalParameter = (Portal)CurrentParameter;
                            Writer.Write((short)PortalParameter.ID);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: Portal");
                            break;
                        case "Quip":
                            int QuipParameter = (int)CurrentParameter;
                            Writer.Write((byte)QuipParameter);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {int.Parse(Quip[2..], NumberStyles.HexNumber)}");
                            break;
                        case "SpecialSoundBank":
                            int SoundBankParameter = (int)CurrentParameter;
                            Writer.Write((byte)SoundBankParameter);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {int.Parse(SoundBankProperty[2..], NumberStyles.HexNumber)}");
                            break;
                        case "Constant":
                            Constant ConstantParameter = (Constant)CurrentParameter;
                            string ConstantType = CurrentParameterType.ConstantName == "CUTSCENEWEAPON" ? "WEAPON" : CurrentParameterType.ConstantName;
                            int ConstantSize = ConstantHolder.ConstantSizes[ConstantType];

                            switch (ConstantSize)
                            {
                                case 8:
                                    Writer.Write((byte)(ConstantParameter != null ? ConstantParameter.Value : 0xFF));
                                    break;
                                case 16:
                                    Writer.Write((short)(ConstantParameter != null ? ConstantParameter.Value : 0xFFFF));
                                    break;
                                case 32:
                                    Writer.Write(ConstantParameter != null ? ConstantParameter.Value : 0xFFFFFFFF);
                                    break;
                                default:
                                    break;
                            }
                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {Constant.Name}");
                            break;
                        case "BitFlag":
                            BitFlagCollection BitFlags = new();

                            string BitFlagType = CurrentParameterType.ConstantName;

                            if (BitFlagType == "VARIABLE")
                            {
                                Writer.Write((int)CurrentParameter);

                                //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {BitFlags.Count}");
                                break;
                            }
                            else
                            {
                                BitFlagCollection BitFlagParameter = (BitFlagCollection)CurrentParameter;
                                int BitFlagSize = GameBitFlagHolder.BitFlagSizes[BitFlagType];

                                switch (BitFlagSize)
                                {
                                    case 8:
                                        Writer.Write((byte)BitFlagParameter.GetBits());
                                        break;
                                    case 16:
                                        Writer.Write((short)BitFlagParameter.GetBits());
                                        break;
                                    case 32:
                                        Writer.Write((int)BitFlagParameter.GetBits());
                                        break;
                                    default:
                                        break;
                                }
                            }
                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {BitFlags.Count}");
                            break;
                        case "null":
                            if (CurrentParameterType.ConstantName == null)
                            {
                                Writer.Write((byte)0);
                            }
                            else
                            {
                                Writer.Write(byte.Parse(CurrentParameterType.ConstantName));
                            }

                            break;
                        default:
                            break;
                    }
                }
            }
        }

        Writer.Dispose();
        MainFileStream.Dispose();
    }

    public bool WriteSetup(string FilePath)
    {
        FileName = Path.GetFileNameWithoutExtension(FilePath);
        MainFileStream = File.Open(FilePath, FileMode.Create);

        // -- Props --

        MemoryStream PropList = new();
        SetAsCurrentStream(PropList);

        int PropPointer = 32;

        if (Briefing.StageTitle != null)
        {
            Writer.Write(0x23);
            Writer.Write(0);
            WriteLanguageString(Briefing.StageTitle);
            Writer.Write(0);
        }
        if (Briefing.PerfectAgentBriefing != null)
        {
            Writer.Write(0x23);
            Writer.Write(1);
            WriteLanguageString(Briefing.PerfectAgentBriefing);
            Writer.Write(0);
        }
        if (Briefing.SpecialAgentBriefing != null)
        {
            Writer.Write(0x23);
            Writer.Write(2);
            WriteLanguageString(Briefing.SpecialAgentBriefing);
            Writer.Write(0);
        }
        if (Briefing.AgentBriefing != null)
        {
            Writer.Write(0x23);
            Writer.Write(3);
            WriteLanguageString(Briefing.AgentBriefing);
            Writer.Write(0);
        }

        BriefingLines += 4;

        foreach (Objective CurrentObjective in Objectives)
        {
            Writer.Write(0x17); // Start objective
            Writer.Write(CurrentObjective.ID);
            WriteLanguageString(CurrentObjective.DisplayText);

            int DifficultyBits = 0;
            if (CurrentObjective.Agent) { DifficultyBits |= 0x01; }
            if (CurrentObjective.SpecialAgent) { DifficultyBits |= 0x02; }
            if (CurrentObjective.PerfectAgent) { DifficultyBits |= 0x04; }
            if (CurrentObjective.PerfectDark) { DifficultyBits |= 0x08; }
            Writer.Write(DifficultyBits);

            foreach (ObjectiveCondition CurrentCondition in CurrentObjective.Conditions)
            {
                switch (CurrentCondition.Type)
                {
                    case ConditionType.DestroyObject:
                        Writer.Write(0x19);
                        Writer.Write(CurrentCondition.ObjectiveTag.TagID);
                        break;
                    case ConditionType.CompletionFlags:
                        Writer.Write(0x1A);
                        Writer.Write(CurrentCondition.StageFlag.Bits);
                        break;
                    case ConditionType.FailureFlags:
                        Writer.Write(0x1B);
                        Writer.Write(CurrentCondition.StageFlag.Bits);
                        break;
                    case ConditionType.CollectObject:
                        Writer.Write(0x1C);
                        Writer.Write(CurrentCondition.ObjectiveTag.TagID);
                        break;
                    case ConditionType.DepositObject:
                        Writer.Write(0x1D);
                        Writer.Write(CurrentCondition.ObjectiveTag.TagID);
                        break;
                    case ConditionType.HolographObject:
                        Writer.Write(0x1E);
                        Writer.Write(CurrentCondition.ObjectiveTag.TagID);
                        Writer.Write(0);
                        Writer.Write(0);
                        break;
                    case ConditionType.EnterRoom:
                        Writer.Write(0x20);
                        Writer.Write(CurrentCondition.ObjectiveRoom.ID + 1);
                        break;
                    case ConditionType.DepositObjectInRoom:
                        Writer.Write(0x21);
                        Writer.Write(CurrentCondition.ObjectiveTag.TagID);
                        Writer.Write(CurrentCondition.ObjectivePad.Number);
                        break;
                }

                ++BriefingLines;
            }

            Writer.Write(0x18); // End objective

            BriefingLines += 2;
        }

        for (PropIndex = 0; PropIndex < Props.Count; ++PropIndex)
        {
            Prop CurrentProp = Props[PropIndex];

            switch (CurrentProp.Type)
            {
                case PropType.Door:
                    WriteCommonPropAttributes(0x01, CurrentProp);
                    Writer.Write(CurrentProp.MaxFrac);
                    Writer.Write(CurrentProp.PerimFrac);
                    Writer.Write(CurrentProp.Acceleration);
                    Writer.Write(CurrentProp.Deceleration);
                    Writer.Write(CurrentProp.MaxSpeed);
                    Writer.Write((short)CurrentProp.DoorFlags.GetBits());
                    Writer.Write((short)CurrentProp.DoorType.Value);
                    Writer.Write(CurrentProp.LockBits.GetBits());
                    Writer.Write(CurrentProp.AutoCloseTime);
                    Writer.WriteZeroes(12);
                    Writer.Write(CurrentProp.unk88);
                    Writer.WriteZeroes(48);
                    Writer.Write(GetObjectOffset(CurrentProp.LinkedDoor));
                    Writer.WriteZeroes(4);
                    Writer.Write(CurrentProp.unkc4);
                    Writer.Write(0);
                    Writer.Write(0xFF000000);
                    Writer.WriteZeroes(12);
                    break;
                case PropType.DoorScale:
                    Writer.Write(0x02);
                    Writer.Write(CurrentProp.Scale);
                    break;
                case PropType.StandardObject:
                    WriteCommonPropAttributes(0x03, CurrentProp);
                    break;
                case PropType.Key:
                    WriteCommonPropAttributes(0x04, CurrentProp);
                    Writer.Write(CurrentProp.LockBits.GetBits());
                    break;
                case PropType.Camera:
                    WriteCommonPropAttributes(0x06, CurrentProp);
                    Writer.Write((short)CurrentProp.TargetPad.Number);
                    Writer.WriteZeroes(74);
                    Writer.Write(CurrentProp.Yaw);
                    Writer.WriteZeroes(8);
                    Writer.Write(CurrentProp.YawMaxSpeed);
                    Writer.Write(0);
                    Writer.Write(CurrentProp.MaxDistance);
                    Writer.Write(0);
                    break;
                case PropType.AmmoCrate:
                    WriteCommonPropAttributes(0x07, CurrentProp);
                    Writer.Write(CurrentProp.AmmoType.Value);// ConstantHolder.GetConstantIndex("AMMOTYPE", CurrentProp.AmmoType));
                    break;
                case PropType.Weapon:
                    WriteCommonPropAttributes(0x08, CurrentProp);
                    Writer.Write((byte)CurrentProp.WeaponType.Value);// ConstantHolder.GetConstantIndex("WEAPON", CurrentProp.WeaponType));
                    Writer.WriteZeroes(3);
                    Writer.Write(0x00FFFFFF);
                    Writer.Write(0);
                    break;
                case PropType.Character:
                    Writer.Write(0x09);
                    Writer.Write(CurrentProp.SpawnFlags.GetBits());
                    Writer.Write((short)CurrentProp.CharacterID);
                    Writer.Write((short)CurrentProp.ParentPad.Number);
                    Writer.Write((byte)DataHolder.CharacterDataList.IndexOf(CurrentProp.Data));

                    if (CurrentProp.HeadData == DataHolder.RandomHeadData) { Writer.Write((byte)0xFF); }
                    else { Writer.Write((byte)DataHolder.CharacterDataList.IndexOf(CurrentProp.HeadData)); }

                    Writer.Write((short)CurrentProp.AIList.ID);
                    Writer.Write((short)GetPadNumber(CurrentProp.PadPreset));
                    Writer.Write((short)GetPadNumber(CurrentProp.CharacterPreset));
                    Writer.Write((short)CurrentProp.HearScale);
                    Writer.Write((short)CurrentProp.ViewDistance);
                    Writer.Write(CurrentProp.Flags1.GetBits());
                    Writer.Write(CurrentProp.Flags2.GetBits());
                    Writer.Write((byte)CurrentProp.TeamFlags.GetBits());
                    Writer.Write((byte)CurrentProp.Squadron.Value);
                    Writer.Write((short)(CurrentProp.Chair?.TagID ?? -1));
                    Writer.Write(CurrentProp.ConvTalk);
                    Writer.Write((byte)CurrentProp.Tude);
                    Writer.Write((byte)CurrentProp.NaturalAnimation);
                    Writer.Write((byte)CurrentProp.YVisAngle);
                    Writer.Write((byte)CurrentProp.TeamScanDistance);
                    break;
                case PropType.SingleMonitor:
                    WriteCommonPropAttributes(0x0A, CurrentProp);
                    Writer.WriteZeroes(116);
                    Writer.Write((byte)0xFF);
                    Writer.Write((byte)0xFF);
                    Writer.Write((byte)0xFF);
                    Writer.Write((byte)CurrentProp.Image.Value);
                    break;
                case PropType.MultiMonitor:
                    WriteCommonPropAttributes(0x0B, CurrentProp);
                    Writer.WriteZeroes(464);
                    Writer.Write((byte)CurrentProp.Image1.Value);
                    Writer.Write((byte)CurrentProp.Image2.Value);
                    Writer.Write((byte)CurrentProp.Image3.Value);
                    Writer.Write((byte)CurrentProp.Image4.Value);
                    break;
                case PropType.Autogun:
                    WriteCommonPropAttributes(0x0D, CurrentProp);
                    Writer.Write(CurrentProp.TargetPad != null ? (short)CurrentProp.TargetPad.Number : (short)-1);
                    Writer.WriteZeroes(6);
                    Writer.Write(CurrentProp.unk64);
                    Writer.Write(CurrentProp.unk68);
                    Writer.WriteZeroes(20);
                    Writer.Write(CurrentProp.unk80);
                    Writer.Write(CurrentProp.unk84);
                    Writer.WriteZeroes(36);
                    break;
                case PropType.LinkGuns:
                    Writer.Write(0x0E);
                    Writer.Write((short)GetObjectOffset(CurrentProp.LinkedWeapon1));
                    Writer.Write((short)GetObjectOffset(CurrentProp.LinkedWeapon2));
                    break;
                case PropType.Debris:
                    WriteCommonPropAttributes(0x0F, CurrentProp);
                    break;
                case PropType.Hat:
                    WriteCommonPropAttributes(0x11, CurrentProp);
                    break;
                case PropType.LiftDoor:
                    Writer.Write(0x13);
                    Writer.Write(GetObjectOffset(CurrentProp.LiftStopDoor));
                    Writer.Write(GetObjectOffset(CurrentProp.LiftStopLift));
                    Writer.WriteZeroes(4);
                    Writer.Write(CurrentProp.StopNumber);
                    break;
                case PropType.AmmoCrateMulti:
                    WriteCommonPropAttributes(0x14, CurrentProp);
                    byte[] ByteArray = new byte[] {
                        0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00,
                        0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00,
                        0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00,
                        0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00
                    };
                    Writer.Write(ByteArray);
                    break;
                case PropType.Shield:
                    WriteCommonPropAttributes(0x15, CurrentProp);
                    Writer.Write(0x00010000);
                    Writer.WriteZeroes(8);
                    break;
                case PropType.Tag:
                    Writer.Write(0x16);
                    Writer.Write((short)CurrentProp.TagID);
                    Writer.Write((short)GetObjectOffset(CurrentProp.TaggedProp));
                    Writer.WriteZeroes(8);
                    break;
                case PropType.TrainingHolographRequirement:
                    Writer.Write(0x1E);
                    Writer.Write(CurrentProp.TagID);
                    Writer.WriteZeroes(8);
                    break;
                case PropType.GasBottle:
                    WriteCommonPropAttributes(0x24, CurrentProp);
                    break;
                case PropType.Rename:
                    Writer.Write(0x25);
                    Writer.Write(GetObjectOffset(CurrentProp.ParentProp));
                    Writer.Write(CurrentProp.WeaponType.Value);
                    WriteLanguageString(CurrentProp.OtherText1);
                    WriteLanguageString(CurrentProp.OtherText2);
                    WriteLanguageString(CurrentProp.MenuListText);
                    WriteLanguageString(CurrentProp.OtherText3);
                    WriteLanguageString(CurrentProp.PickUpText);
                    Writer.WriteZeroes(8);
                    break;
                case PropType.PadlockedDoor: // UNUSED / NOT IMPLEMENTED YET 
                    Writer.WriteZeroes(12);
                    break;
                case PropType.Glass:
                    WriteCommonPropAttributes(0x2A, CurrentProp);
                    Writer.WriteZeroes(4);
                    break;
                case PropType.Safe:
                    WriteCommonPropAttributes(0x2B, CurrentProp);
                    break;
                case PropType.SafeItem: // UNUSED / NOT IMPLEMENTED YET 
                    Writer.WriteZeroes(12);
                    break;
                case PropType.Camera2:
                    int ObjectInfo = 0x2E | (CurrentProp.Scale << 16);
                    Writer.Write(ObjectInfo);
                    Writer.Write(CurrentProp.XOffset);
                    Writer.Write(CurrentProp.YOffset);
                    Writer.Write(CurrentProp.ZOffset);
                    Writer.Write(CurrentProp.LateralRotation);
                    Writer.Write(CurrentProp.VerticalRotation);
                    Writer.Write(CurrentProp.ParentPad.Number);
                    break;
                case PropType.TintedGlass:
                    WriteCommonPropAttributes(0x2F, CurrentProp);
                    Writer.Write(CurrentProp.unk5c);
                    Writer.Write(0x0000FFFF);
                    Writer.Write(0);
                    break;
                case PropType.Lift:
                    WriteCommonPropAttributes(0x30, CurrentProp);
                    Writer.Write((short)GetPadNumber(CurrentProp.LiftPad1));
                    Writer.Write((short)GetPadNumber(CurrentProp.LiftPad2));  
                    Writer.Write((short)GetPadNumber(CurrentProp.LiftPad3));
                    Writer.Write((short)GetPadNumber(CurrentProp.LiftPad4));
                    Writer.Write(CurrentProp.LiftDoor1 != null ? GetObjectOffset(CurrentProp.LiftDoor1) : 0);
                    Writer.Write(CurrentProp.LiftDoor2 != null ? GetObjectOffset(CurrentProp.LiftDoor2) : 0);
                    Writer.Write(CurrentProp.LiftDoor3 != null ? GetObjectOffset(CurrentProp.LiftDoor3) : 0);
                    Writer.Write(CurrentProp.LiftDoor4 != null ? GetObjectOffset(CurrentProp.LiftDoor4) : 0);
                    Writer.WriteZeroes(8);
                    Writer.Write(CurrentProp.Acceleration);
                    Writer.Write(CurrentProp.MaxSpeed);
                    Writer.Write(CurrentProp.unk84);
                    Writer.WriteZeroes(12);
                    break;
                case PropType.ConditionalScenery:
                    Writer.Write(0x31);
                    Writer.Write(GetObjectOffset(CurrentProp.TriggerProp));
                    Writer.Write(GetObjectOffset(CurrentProp.UnexplodedProp));
                    Writer.Write(GetObjectOffset(CurrentProp.ExplodedProp));
                    Writer.WriteZeroes(4);
                    break;
                case PropType.BlockedPath:
                    Writer.Write(0x32);
                    Writer.Write(GetObjectOffset(CurrentProp.PathBlockingProp));
                    Writer.Write((short)CurrentProp.Waypoint1.ID);
                    Writer.Write((short)CurrentProp.Waypoint2.ID);
                    Writer.WriteZeroes(4);
                    break;
                case PropType.Hoverbike:
                    WriteCommonPropAttributes(0x33, CurrentProp);
                    Writer.Write((byte)1);
                    Writer.Write((byte)1);
                    Writer.WriteZeroes(130);
                    break;
                case PropType.Hoverprop:
                    WriteCommonPropAttributes(0x35, CurrentProp);
                    Writer.Write((byte)CurrentProp.HoverType.Value);
                    Writer.Write((byte)1);
                    Writer.WriteZeroes(62);
                    break;
                case PropType.Fan:
                    WriteCommonPropAttributes(0x36, CurrentProp);
                    Writer.WriteZeroes(8);
                    Writer.Write(CurrentProp.MaxSpeed);
                    Writer.WriteZeroes(4);
                    Writer.Write(CurrentProp.Acceleration);
                    Writer.Write(CurrentProp.On);
                    Writer.WriteZeroes(3);
                    break;
                case PropType.Hovercar:
                    WriteCommonPropAttributes(0x37, CurrentProp);
                    Writer.Write(CurrentProp.AIList.ID);
                    Writer.WriteZeroes(56);
                    break;
                case PropType.Effect:
                    Writer.Write(0x38);
                    Writer.Write(CurrentProp.Effect.Value);
                    Writer.Write(CurrentProp.ParentPad.Number);
                    break;
                case PropType.Hovercopter:
                    WriteCommonPropAttributes(0x39, CurrentProp);
                    Writer.Write(CurrentProp.AIList.ID);
                    Writer.WriteZeroes(136);
                    break;
                case PropType.Mine:
                    WriteCommonPropAttributes(0x3A, CurrentProp);
                    Writer.Write((byte)CurrentProp.WeaponType.Value);
                    Writer.WriteZeroes(3);
                    Writer.Write(1);
                    Writer.Write(0);
                    break;
                case PropType.EscalatorStep:
                    WriteCommonPropAttributes(0x3B, CurrentProp);
                    Writer.WriteZeroes(16);
                    break;
            }

            /*if (PropPointer + CurrentStream.Position >= 0xD18)
            {
                Debug.Log("BREAKER");
                Debug.Log(PropIndex + " " + CurrentProp.Type);
            }*/
        }

        Writer.Write(0x34); // End objects list

        MemoryStream IntroList = new();
        SetAsCurrentStream(IntroList);

        int IntroPointer = PropPointer + (int)PropList.Length;

        foreach (IntroSetting CurrentIntro in IntroSettings)
        {
            switch (CurrentIntro.Type)
            {
                case IntroType.Spawn:
                    Writer.Write(0x00);
                    Writer.Write(CurrentIntro.IntroPad.Number);
                    Writer.Write(0);
                    break;
                case IntroType.Weapon:
                    Writer.Write(0x01);
                    Writer.Write(CurrentIntro.WeaponType.Value);
                    Writer.Write(CurrentIntro.UnknownWeaponInt);
                    Writer.Write(0);
                    break;
                case IntroType.Ammo:
                    Writer.Write(0x02);
                    Writer.Write(CurrentIntro.AmmoType.Value);
                    Writer.Write(CurrentIntro.AmmoAmount);
                    Writer.Write(0);
                    break;
                case IntroType.PlayerOutfit:
                    Writer.Write(0x05);
                    Writer.Write(CurrentIntro.Outfit.Value);
                    break;
                case IntroType.Case:
                    Writer.Write(0x09);
                    Writer.Write(CurrentIntro.CaseID);
                    Writer.Write(CurrentIntro.IntroPad.Number);
                    break;
                case IntroType.CaseRespawn:
                    Writer.Write(0x0A);
                    Writer.Write(CurrentIntro.CaseID);
                    Writer.Write(CurrentIntro.IntroPad.Number);
                    break;
                case IntroType.Hill:
                    Writer.Write(0x0B);
                    Writer.Write(CurrentIntro.IntroPad.Number);
                    break;
            }
        }

        Writer.Write(0x0C); // End intro list

        MemoryStream PathPadList = new();
        SetAsCurrentStream(PathPadList);

        int PathPadsStartPosition = IntroPointer + (int)IntroList.Length;
        int[] PathPointers = new int[Paths.Count];

        for (int i = 0; i < Paths.Count; ++i)
        {
            GuardPath CurrentPath = Paths[i];

            PathPointers[i] = PathPadsStartPosition + (int)CurrentStream.Position;

            foreach (Pad CurrentPad in CurrentPath.Pads)
            {
                Writer.Write(CurrentPad.Number);
            }

            Writer.Write(0xFFFFFFFF);
        }

        MemoryStream AICommandList = new();
        SetAsCurrentStream(AICommandList);

        int AICommandsStartPosition = PathPadsStartPosition + (int)PathPadList.Length;
        int[] AIListPointers = new int[AILists.Count];

        int j = 0;

        foreach (AIList CurrentAIList in AILists)
        {
            AIListPointers[j] = AICommandsStartPosition + (int)CurrentStream.Position;

            foreach (AICommandInstance CurrentCommandInstance in CurrentAIList.Commands)
            {
                AICommand CurrentCommand = CurrentCommandInstance.Command;

                Writer.Write((short)CurrentCommand.ID);

                int ParameterID = 0;

                foreach (ParameterType CurrentParameterType in CurrentCommand.ParameterTypes)
                {
                    object CurrentParameter = null;

                    if (CurrentParameterType.ClassType != "null")
                    {
                        CurrentParameter = CurrentCommandInstance.Parameters[ParameterID];
                        ++ParameterID;
                    }

                    switch (CurrentParameterType.ClassType)
                    {
                        case "System.Int32":
                            Writer.Write((int)CurrentParameter);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {(int)NewCommandInstance.Parameters[^1]}");
                            break;
                        case "Int24":
                            byte[] Bytes = BitConverter.GetBytes((int)CurrentParameter);

                            int param = (int)CurrentParameter;

                            Writer.Write(Bytes[2]);
                            Writer.Write(Bytes[1]);
                            Writer.Write(Bytes[0]);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {(int)NewCommandInstance.Parameters[^1]}");
                            break;
                        case "System.Int16":
                            int NumberParameter = (int)CurrentParameter;
                            Writer.Write((short)NumberParameter);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {(int)NewCommandInstance.Parameters[^1]}");
                            break;
                        case "System.Byte":
                            NumberParameter = (int)CurrentParameter;
                            Writer.Write((byte)NumberParameter);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {(int)NewCommandInstance.Parameters[^1]}");
                            break;
                        case "String":
                            byte[] DebugCommentArray = Encoding.UTF8.GetBytes((string)CurrentParameter);

                            Writer.Write(DebugCommentArray);
                            Writer.Write((byte)0);
                            //Debug.Log($"Parameter added to {NewAIList.ID} in {CurrentCommand.Name}: {DebugComment}");
                            break;
                        case "Label":
                            Label CurrentLabel = (Label)CurrentParameter;

                            Writer.Write((byte)CurrentLabel.ID);
                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {Parameter.ID}");
                            break;
                        case "Character":
                            if (CurrentParameter is Prop CurrentCharacter)
                            {
                                Writer.Write((byte)CurrentCharacter.CharacterID);
                            }
                            else
                            {
                                NumberParameter = (int)CurrentParameter;
                                Writer.Write((byte)NumberParameter);
                            }

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {chr?.GetName()}");
                            break;
                        case "AIList":
                            AIList ReferencedAIList = (AIList)CurrentParameter;
                            Writer.Write((short)ReferencedAIList.ID);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {AIListID}");
                            break;
                        case "Animation":
                            Writer.Write((short)DataHolder.Animations.IndexOf((AnimationSequence)CurrentParameter));

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {AnimationParameter.Name}");
                            break;
                        case "Model":
                            Writer.Write((short)DataHolder.PropDataList.IndexOf((ModelData)CurrentParameter));

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {AnimationParameter.Name}");
                            break;
                        case "Pad":
                            Pad CurrentPad = (Pad)CurrentParameter;
                            Writer.Write((short)CurrentPad.Number);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {newpad?.name}");
                            break;
                        case "Path":
                            GuardPath ParameterPath = (GuardPath)CurrentParameter;

                            if (ParameterPath == null)
                            {
                                Writer.Write((byte)0xFF);
                            }
                            else
                            {
                                Writer.Write((byte)ParameterPath.ID);
                            }

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {PathID}");
                            break;
                        case "Room":
                            // If the second parameter of action 0x005B is 1, the room parameter is not used.
                            // If it is 2, it's a pad, not a room (hardcoded to work only in G5 Building.)
                            if (CurrentCommand.ID == 0x005B)
                            {
                                int RoomOrPad = (int)CurrentCommandInstance.Parameters[1];

                                if (RoomOrPad == 1)
                                {
                                    Writer.Write((short)0);

                                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: No room");
                                }
                                else if (RoomOrPad == 2)
                                {
                                    Pad PadParameter = (Pad)CurrentParameter;
                                    Writer.Write((short)PadParameter.Number);

                                    //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {PadParameter.NumberString}");
                                }
                                else
                                {
                                    Room RoomParameter = (Room)CurrentParameter;
                                    Writer.Write(RoomParameter != null ? (short)RoomParameter.ID : (short)0);
                                }
                            }
                            else
                            {
                                Room RoomParameter = (Room)CurrentParameter;
                                Writer.Write(RoomParameter != null ? (short)RoomParameter.ID : (short)0);
                            }

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {RoomParameter?.NumberString}");
                            break;
                        case "System.Boolean":
                            Writer.Write((bool)CurrentParameter);
                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {NewBool}");
                            break;
                        case "AimEntity":
                            BitFlagCollection AimFlags = (BitFlagCollection)CurrentCommandInstance.Parameters[0];

                            if (AimFlags.HasBits(0x0004)) // Aim at character
                            {
                                Prop AimCharacter = (Prop)CurrentParameter;
                                Writer.Write((short)AimCharacter.CharacterID);
                                //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {AimCharacter?.CharacterID}");
                            }
                            else if (AimFlags.HasBits(0x0008)) // Aim at pad
                            {
                                Pad AimPad = (Pad)CurrentParameter;
                                Writer.Write((short)AimPad.Number);
                                //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {AimPad?.NumberString}");
                            }
                            else if (AimFlags.HasBits(0x0010)) // Aim at direction
                            {
                                int AimDirection = (int)CurrentParameter;
                                Writer.Write((short)AimDirection);
                                //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {AimDirection}");
                            }
                            else if (AimFlags.HasBits(0x0200)) // Aim at target
                            {
                                short PlaysWalkAnimation = System.Convert.ToInt16((bool)CurrentParameter);
                                Writer.Write(PlaysWalkAnimation);
                                //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {PlaysWalkAnimation}");
                            }
                            else
                            {
                                Writer.Write((short)0);
                                //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: null");
                            }
                            break;
                        case "Prop":
                            Prop ParameterTag = (Prop)CurrentParameter;

                            if (ParameterTag == null)
                            {
                                Debug.LogWarning($"Prop parameter does not exist at: {CurrentStream.Position:X}");
                                Writer.Write((byte)0xFF);
                            }
                            else
                            {
                                Writer.Write((byte)ParameterTag.TagID);
                            }

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {TagParameter.TagID}");
                            break;
                        case "PropOrCharacter":
                            Prop PropParameter = (Prop)CurrentParameter;

                            if (PropParameter.IsAbstract) // Character preset
                            {
                                Writer.Write((byte)PropParameter.CharacterID);

                                //Debug.Log($"Parameter added to {InAIList.Name} in {CurrentCommand.Name}: {CharacterParameter.CharacterID}");
                            }
                            else // Prop tag
                            {
                                Writer.Write((byte)PropParameter.TagID);

                                //Debug.Log($"Parameter added to {InAIList.Name} in {CurrentCommand.Name}: {TagParameter.TagID}");
                            }
                            break;
                        case "Objective":
                            Objective ObjectiveParameter = (Objective)CurrentParameter;
                            Writer.Write((byte)ObjectiveParameter.ID);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {ObjectiveParameter.DisplayText?.en}");
                            break;
                        case "Stage":
                            Stage StageParameter = (Stage)CurrentParameter;
                            Writer.Write((byte)StageParameter.ID);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {StageParameter.Name}");
                            break;
                        case "Body":
                            Writer.Write((byte)DataHolder.CharacterDataList.IndexOf((ModelData)CurrentParameter));
                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {BodyData.FileName}");
                            break;
                        case "Head":
                            ModelData ModelDataParameter = (ModelData)CurrentParameter;

                            if (ModelDataParameter == DataHolder.RandomHeadData)
                            {
                                Writer.Write((byte)0xFF);
                                //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {HeadData.FileName}");
                            }
                            else
                            {
                                Writer.Write((byte)DataHolder.CharacterDataList.IndexOf(ModelDataParameter));
                            }
                            break;
                        case "LanguageString":
                            WriteShortLanguageString((LanguageString)CurrentParameter);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {LanguageStringParameter?.en}");
                            break;
                        case "SoundEffect":
                            int SoundEffectParameter = (int)CurrentParameter;

                            Writer.Write((short)SoundEffectParameter);


                            /*SoundEffectInstance SoundEffectParameter = (SoundEffectInstance)CurrentParameter;

                            SoundEffect CurrentSoundEffect = SoundEffectParameter.SoundEffectID;
                            Debug.Log("SOUNDEFFECT:" + SoundEffectParameter.SoundEffectID?.Name);
                            Debug.Log("SOUNDEFFECT:" + SoundEffectParameter.SoundEffectID?.ID);
                            Debug.Log(SoundEffectParameter.Unknown0x2000);
                            Debug.Log(SoundEffectParameter.Unknown0x4000);

                            short SoundEffectNumber = 0;
                            SoundEffectNumber &= (short)CurrentSoundEffect?.ID;

                            if (CurrentSoundEffect.IsMP3)
                            {
                                if (SoundEffectParameter.Unknown0x2000) { SoundEffectNumber |= 0x2000; }
                                if (SoundEffectParameter.Unknown0x4000) { SoundEffectNumber |= 0x4000; }

                                if (SoundEffectParameter.Priority == SoundEffectInstance.MP3Priority.Low) { SoundEffectNumber |= 0x1800; }
                                else if (SoundEffectParameter.Priority == SoundEffectInstance.MP3Priority.Medium) { SoundEffectNumber |= 0x1000; }
                                else if (SoundEffectParameter.Priority == SoundEffectInstance.MP3Priority.High) { SoundEffectNumber |= 0x0800; }
                            }

                            Writer.Write((short)SoundEffectNumber);*/

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {DataHolder.SoundEffects.Find(x => x.Name == SoundEffectName).Name}");
                            break;
                        case "MusicTrack":
                            Writer.Write((byte)DataHolder.MusicTracks.IndexOf((MusicTrack)CurrentParameter));

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {DataHolder.MusicTracks.Find(x => x.Name == MusicTrackName).Name}");
                            break;
                        case "Portal":
                            Portal PortalParameter = (Portal)CurrentParameter;
                            Writer.Write((short)PortalParameter.ID);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: Portal");
                            break;
                        case "Quip":
                            int QuipParameter = (int)CurrentParameter;
                            Writer.Write((byte)QuipParameter);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {int.Parse(Quip[2..], NumberStyles.HexNumber)}");
                            break;
                        case "SpecialSoundBank":
                            int SoundBankParameter = (int)CurrentParameter;
                            Writer.Write((byte)SoundBankParameter);

                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {int.Parse(SoundBankProperty[2..], NumberStyles.HexNumber)}");
                            break;
                        case "Constant":
                            Constant ConstantParameter = (Constant)CurrentParameter;
                            string ConstantType = CurrentParameterType.ConstantName == "CUTSCENEWEAPON" ? "WEAPON" : CurrentParameterType.ConstantName;
                            int ConstantSize = ConstantHolder.ConstantSizes[ConstantType];

                            switch (ConstantSize)
                            {
                                case 8:
                                    Writer.Write((byte)(ConstantParameter != null ? ConstantParameter.Value : 0xFF));
                                    break;
                                case 16:
                                    Writer.Write((short)(ConstantParameter != null ? ConstantParameter.Value : 0xFFFF));
                                    break;
                                case 32:
                                    Writer.Write(ConstantParameter != null ? ConstantParameter.Value : 0xFFFFFFFF);
                                    break;
                                default:
                                    Debug.LogError($"Constant {CurrentParameterType.ConstantName} has an incorrect constant size: {ConstantSize}");
                                    break;
                            }
                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {Constant.Name}");
                            break;
                        case "BitFlag":
                            BitFlagCollection BitFlags = new();

                            string BitFlagType = CurrentParameterType.ConstantName;

                            if (BitFlagType == "VARIABLE")
                            {
                                Writer.Write((int)CurrentParameter);

                                //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {BitFlags.Count}");
                                break;
                            }
                            else
                            {
                                BitFlagCollection BitFlagParameter = (BitFlagCollection)CurrentParameter;
                                int BitFlagSize = GameBitFlagHolder.BitFlagSizes[BitFlagType];

                                switch (BitFlagSize)
                                {
                                    case 8:
                                        Writer.Write((byte)BitFlagParameter.GetBits());
                                        break;
                                    case 16:
                                        Writer.Write((short)BitFlagParameter.GetBits());
                                        break;
                                    case 32:
                                        Writer.Write((int)BitFlagParameter.GetBits());
                                        break;
                                    default:
                                        Debug.LogError($"BitFlag {BitFlagType} has an incorrect constant size: {BitFlagSize}");
                                        break;
                                }
                            }
                            //Debug.Log($"Parameter added to {InAIList.Name} in {Command.Name}: {BitFlags.Count}");
                            break;
                        case "null":
                            if (CurrentParameterType.ConstantName == null)
                            {
                                Writer.Write((byte)0);
                            }
                            else
                            {
                                Writer.Write(byte.Parse(CurrentParameterType.ConstantName));
                            }

                            break;
                        default:
                            Debug.LogError($"Syntax error: Parameter {CurrentParameterType.ClassType} not recognized at: {CurrentStream.Position:X}");
                            break;
                    }

                    /*if (AICommandsStartPosition + CurrentStream.Position >= 0x8F65)
                    {
                        Debug.Log("BREAKPOINT");
                    }*/
                }
            }

            ++j;

            AlignToBytes(AICommandsStartPosition + (int)CurrentStream.Position, 4);
        }

        MemoryStream AIListList = new();
        SetAsCurrentStream(AIListList);

        int AIListPointer = AICommandsStartPosition + (int)AICommandList.Length;

        int AIListIndex = 0;

        foreach (AIList CurrentAIList in AILists)
        {
            int CurrentPointer = AIListPointers[AIListIndex];

            Writer.Write(CurrentPointer);
            Writer.Write(CurrentAIList.ID);

            ++AIListIndex;
        }

        Writer.Write(0);
        Writer.Write(0);

        MemoryStream PathList = new();
        SetAsCurrentStream(PathList);

        int PathPointer = AIListPointer + (int)AIListList.Length;

        for (int i = 0; i < Paths.Count; ++i)
        {
            int CurrentPointer = PathPointers[i];
            GuardPath CurrentPath = Paths[i];

            Writer.Write(CurrentPointer);
            Writer.Write((byte)CurrentPath.ID);

            int PathFlags = 0;
            if (CurrentPath.IsCircularPath) { PathFlags |= 1; }
            if (CurrentPath.IsFlyingPath) { PathFlags |= 2; }

            Writer.Write((byte)PathFlags);
            Writer.Write((short)0);
        }

        Writer.Write(0);
        Writer.Write(0);

        SetAsCurrentStream(MainFileStream);
        Writer.Write(0);
        Writer.Write(0);
        Writer.Write(0);
        Writer.Write(IntroPointer);
        Writer.Write(PropPointer);
        Writer.Write(PathPointer);
        Writer.Write(AIListPointer);
        Writer.Write(0);

        PropList.WriteTo(MainFileStream);
        IntroList.WriteTo(MainFileStream);
        PathPadList.WriteTo(MainFileStream);
        AICommandList.WriteTo(MainFileStream);
        AIListList.WriteTo(MainFileStream);
        PathList.WriteTo(MainFileStream);

        AlignToBytes((int)CurrentStream.Position, 16);

        Writer.Dispose();
        MainFileStream.Dispose();

        return true;
    }

    int GetObjectOffset(Prop OffsetProp)
    {
        if (OffsetProp == null)
        {
            return 0;
        }

        return (BriefingLines + Props.IndexOf(OffsetProp)) - (BriefingLines + PropIndex);
    }

    // Use this to write a pad in the file, this accounts for null pads (which are represented as -1 in the files).
    int GetPadNumber(Pad InPad)
    {
        return InPad != null ? InPad.Number : -1;
    }

    void WriteLanguageString(LanguageString InString)
    {
        if (InString == null)
        {
            Writer.Write(0xFFFFFFFF);
            return;
        }

        int LanguageNumber = InString.Number;
        LanguageNumber |= (InString.ParentBank.ID << 9);

        Writer.Write(LanguageNumber);
    }

    void WriteShortLanguageString(LanguageString InString)
    {
        if (InString == null)
        {
            Writer.Write((ushort)0x0002);
            return;
        }

        short LanguageNumber = (short)InString.Number;
        LanguageNumber |= (short)(InString.ParentBank.ID << 9);

        Writer.Write(LanguageNumber);
    }

    private void WriteCommonPropAttributes(int ObjectNumber, Prop InProp)
    {
        int ObjectInfo = ObjectNumber | (InProp.Scale << 16);
        Writer.Write(ObjectInfo);

        Writer.Write((short)DataHolder.PropDataList.IndexOf(InProp.Data));

        if (InProp.IsGlobal) 
        { 
            Writer.Write((short)-1);
        }
        else if (InProp.IsAttachedToCharacter) 
        { 
            Writer.Write((short)InProp.ParentProp.CharacterID); 
        }
        else if (InProp.IsAttachedToOtherProp) 
        { 
            Writer.Write((short)GetObjectOffset(InProp.ParentProp));
        }
        else 
        { 
            Writer.Write((short)InProp.ParentPad.Number); 
        }

        Writer.Write(InProp.Flags1.GetBits());
        Writer.Write(InProp.Flags2.GetBits());
        Writer.Write(InProp.Flags3.GetBits());

        Writer.WriteZeroes(56);
        Writer.Write(InProp.MaxDamage);
        Writer.WriteZeroes(8);
        Writer.Write(0x0FFF0000);
    }

    void AlignToBytes(int CurrentPosition, int AlignBy)
    {
        if (CurrentPosition % AlignBy != 0)
        {
            int AmountOfZeroes = AlignBy - (CurrentPosition % AlignBy);

            Writer.WriteZeroes(AmountOfZeroes);
        }
    }

    // Set CurrentStream and Reader with this function to make other functions write to the correct data.
    void SetAsCurrentStream(Stream InStream)
    {
        CurrentStream = InStream;
        Writer = new(InStream);
    }
}
