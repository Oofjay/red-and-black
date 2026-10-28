using SimpleFileBrowser;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DataHolder : MonoBehaviour
{
    string SourcePath; // the path to the /src directory in the decomp project.
    string BuildPath; // the path to the /build directory in the decomp project.
    public static Dictionary<string, int> ModelNumbers;
    public static List<string> LanguageFileConstants; // index is the ID of the language bank, string value is the filename constant
    public static List<string> FileNames; // index is the filenumber, the string values are the file path
    public static Dictionary<string, int> FileNumbers = new(); // key is filename constant, value is file number

    public static List<ModelData> PropDataList;
    public static List<ModelData> CharacterDataList;
    public static Dictionary<string, PropModel> PropModels = new(); // A list of all the prop 3D models. Key is the file name.
    public static Dictionary<string, PropModel> GunModels = new(); // A list of all the gun 3D models. Key is the file name.
    public static Dictionary<string, PropModel> CharacterModels = new(); // A list of all the character 3D models. Key is the file name.
    public static Dictionary<int, Material> Materials = new(); // A list of all the game's textures. Key is the texture number.
    public static List<AnimationSequence> Animations = new();

    public static List<SoundEffect> SoundEffects;
    public static List<MusicTrack> MusicTracks;
    public static List<AIList> GlobalAILists; // Default AI lists that can be used in all levels.
    public static List<AICommand> AICommands;
    public static List<ScreenImage> ScreenImages;
    public static List<Stage> Stages;

    public static Pad PadPreset;
    public static ModelData RandomHeadData;
    public static PropModel RandomHeadModel;

    public static List<PadData> Clipboard = new();

    public Dictionary<string, Shader> Shaders = new();
    public Shader NormalGeometryShader;
    public Shader TransparentGeometryShader;
    public Shader TopFlagGeometryShader;
    public Shader PortalShader;
    public Shader CollisionTypeShader;

    public static Material PortalMaterial;
    public static Material CollisionMaterial;

    public static Camera MainCameraInstance;
    public static SelectionManager SelectionManagerInstance;
    public static Raycaster RaycasterInstance;

    public Canvas MainCanvas;
    public static ObjectInspector ObjectInspectorInstance;
    public static PromptWindow PromptWindowInstance;
    public GameObject SetupSettingsInstance;

    //public static LanguageFileHolder Language = new();

    public static string LevelFileName = ""; // The decomp file name of the setup, like ame, ear, ark

    public static List<Room> Rooms = new();
    public static List<Portal> Portals = new();
    public static List<BackgroundLight> BackgroundLights = new();
    public static List<Collision> Collisions = new();

    public static List<Objective> Objectives = new();
    public static List<Pad> Pads = new();
    public static List<Pad> CoverPads = new();
    public static List<Waypoint> Waypoints = new();
    public static List<Waygroup> Waygroups = new();
    public static List<Prop> Props = new();
    public static List<Prop> VisibleProps = new();
    public static List<Prop> Characters = new();
    public static List<Prop> Tags = new();
    public static List<IntroSetting> IntroSettings = new();
    public static List<GuardPath> Paths = new();
    public static List<AIList> AILists = new();
    public static List<AIList> CharacterAILists = new();
    public static List<AIList> CutsceneAILists = new();
    public static List<AIList> BackgroundAILists = new();
    public static List<AIList> EnvironmentalAILists = new();
    public static List<VisibilityCommand> VisibilityCommands = new();

    public GameObject RoomType;
    public GameObject PortalType;
    public GameObject BackgroundLightType;
    public GameObject CollisionType;
    public GameObject PadType;

    private bool HasLoadedStage = false;

    private void Awake()
    {
        MainCameraInstance = FindFirstObjectByType<Camera>();

        ObjectInspectorInstance = MainCanvas.transform.Find("ObjectInspector").GetComponent<ObjectInspector>();
        PromptWindowInstance = MainCanvas.transform.Find("Prompt").GetComponent<PromptWindow>();

        SelectionManagerInstance = GetComponentInChildren<SelectionManager>();

        PadPreset = FindFirstObjectByType<Pad>();
        PadPreset.Number = 9000;

        PortalMaterial = new(PortalShader);
        CollisionMaterial = new(TopFlagGeometryShader);

#if UNITY_EDITOR

        LoadProject("C:\\perfect-dark");
        //LoadStage(Stages[31]);

        LevelFileName = "ear";

        LoadBackgroundFromBinary("C:\\Users\\Oofjay\\Desktop\\Temp\\ear_files\\bg_ear.bgf");
        LoadPadsFromBinary("C:\\Users\\Oofjay\\Desktop\\Temp\\ear_files\\bg_ear_pads.pad");
        LoadSetupFromBinary("C:\\Users\\Oofjay\\Desktop\\Temp\\ear_files\\UsetupearZ.set");
        LoadCollisionFromBinary("C:\\Users\\Oofjay\\Desktop\\Temp\\ear_files\\bg_ear_tiles.clp");

        DropdownOptions.RefreshSetupOptions();

        HasLoadedStage = true;

#else

        ShowLoadProjectFileWindow();

#endif
    }

    public void ShowLoadProjectFileWindow()
    {
        StartCoroutine(ShowLoadProjectFileWindowCoroutine());
    }

    public void ShowImportSetupFileWindow()
    {
        StartCoroutine(ShowImportSetupFileWindowCoroutine());
    }

    public void ShowImportBackgroundFileWindow()
    {
        StartCoroutine(ShowImportBackgroundFileWindowCoroutine());
    }

    public void ShowImportCollisionFileWindow()
    {
        StartCoroutine(ShowImportCollisionFileWindowCoroutine());
    }

    public void ShowExportPadsFileWindow()
    {
        StartCoroutine(ShowExportPadsFileWindowCoroutine());
    }

    public void ShowExportSetupFileWindow()
    {
        StartCoroutine(ShowExportSetupFileWindowCoroutine());
    }

    IEnumerator ShowLoadProjectFileWindowCoroutine()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, false, null, null, "Load decomp project", "Load folder");
        if (FileBrowser.Success && FileBrowser.Result.Length != 0)
        {
            LoadProject(FileBrowser.Result[0]);
        }
        else
        {
            Debug.Log("Exited load dialog without selecting file");
        }
    }

    IEnumerator ShowImportSetupFileWindowCoroutine()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("C setup files", ".c"), new FileBrowser.Filter("Binary setup files", ".set"));
        FileBrowser.SetDefaultFilter(".set");
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Import setup file", "Import");

        if (!FileBrowser.Success || FileBrowser.Result.Length == 0)
        {
            Debug.Log("Exited load dialog without selecting file");
            yield break;
        }

        string SetupFilePath = FileBrowser.Result[0];
        string SetupExtension = Path.GetExtension(SetupFilePath);

        FileBrowser.SetFilters(true, new FileBrowser.Filter("Binary pad files", ".pad"));
        FileBrowser.SetDefaultFilter(".pad");
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Import pad file", "Import");

        if (!FileBrowser.Success || FileBrowser.Result.Length == 0)
        {
            Debug.Log("Exited load dialog without selecting file");
            yield break;
        }

        string PadsFilePath = FileBrowser.Result[0];

        bool LoadSuccess = LoadPadsFromBinary(PadsFilePath);

        if (!LoadSuccess)
        {
            UnloadPads();
            yield break;
        }

        HasLoadedStage = true;

        if (SetupExtension == ".set") // Binary
        {
            LoadSuccess = LoadSetupFromBinary(SetupFilePath);
        }
        else if (SetupExtension == ".c") // C file
        {
            //LoadSetupFromCode(SetupFilePath);
        }

        if (!LoadSuccess)
        {
            UnloadSetup();
            yield break;
        }

        DropdownOptions.RefreshSetupOptions();

        if (SetupSettingsInstance.activeSelf)
        {
            SetupSettingsInstance.SetActive(false);
            SetupSettingsInstance.SetActive(true);
        }
    }

    IEnumerator ShowImportBackgroundFileWindowCoroutine()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Binary background files", ".seg", ".bgf"));
        FileBrowser.SetDefaultFilter(".seg");
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Import background file", "Import");

        if (!FileBrowser.Success || FileBrowser.Result.Length == 0)
        {
            Debug.Log("Exited load dialog without selecting file");
            yield break;
        }

        string BackgroundFilePath = FileBrowser.Result[0];

        bool LoadSuccess = LoadBackgroundFromBinary(BackgroundFilePath);

        if (!LoadSuccess)
        {
            UnloadBackground();
            yield break;
        }

        HasLoadedStage = true;

        DropdownOptions.RefreshBackground();

        if (SetupSettingsInstance.activeSelf)
        {
            SetupSettingsInstance.SetActive(false);
            SetupSettingsInstance.SetActive(true);
        }
    }

    IEnumerator ShowImportCollisionFileWindowCoroutine()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Binary collision files", ".clp"));
        FileBrowser.SetDefaultFilter(".clp");
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Import collision file", "Import");

        if (!FileBrowser.Success || FileBrowser.Result.Length == 0)
        {
            Debug.Log("Exited load dialog without selecting file");
            yield break;
        }

        string CollisionFilePath = FileBrowser.Result[0];

        bool LoadSuccess = LoadCollisionFromBinary(CollisionFilePath);

        if (!LoadSuccess)
        {
            UnloadCollision();
            yield break;
        }

        HasLoadedStage = true;
    }

    IEnumerator ShowExportPadsFileWindowCoroutine()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Binary pad files", ".pad"));
        FileBrowser.SetDefaultFilter(".pad");
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Files, false, null, null, "Export pad file", "Export");

        if (!FileBrowser.Success || FileBrowser.Result.Length == 0)
        {
            Debug.Log("Exited load dialog without selecting file");
            yield break;
        }

        string PadsFilePath = FileBrowser.Result[0];

        BinaryFileWriter BinaryFileWriterInstance = new();
        bool SaveSuccess = BinaryFileWriterInstance.WritePads(PadsFilePath);

        if (!SaveSuccess)
        {
            Debug.Log("Pads save false");
            yield break;
        }
    }

    IEnumerator ShowExportSetupFileWindowCoroutine()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("C setup files", ".c"), new FileBrowser.Filter("Binary setup files", ".set"));
        FileBrowser.SetDefaultFilter(".set");
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Files, false, null, null, "Export setup file", "Export");

        if (!FileBrowser.Success || FileBrowser.Result.Length == 0)
        {
            Debug.Log("Exited load dialog without selecting file");
            yield break;
        }

        string SetupFilePath = FileBrowser.Result[0];

        BinaryFileWriter BinaryFileWriterInstance = new();
        bool SaveSuccess = BinaryFileWriterInstance.WriteSetup(SetupFilePath);

        if (!SaveSuccess)
        {
            Debug.Log("Setup save false");
            yield break;
        }

        BinaryFileWriterInstance.WriteAIListDebugFile(SetupFilePath);
    }

    void LoadProject(string Path)
    {
        if (!Directory.Exists(Path + "\\src"))
        {
            Debug.LogError("Invalid decompilation project, did not find source folder");
            return;
        }

        if (HasLoadedStage)
        {
            UnloadStage();
        }

        SourcePath = Path + "\\src";
        BuildPath = Path + "\\build";

        FileReader FileReaderInstance = new();

        GameBitFlagHolder.ClearBitFlagCollection();
        ConstantHolder.ClearConstantCollection();

        FileNames = FileReaderInstance.GetFileData(SourcePath);
        FileReaderInstance.GetFileNumbersAndPropData(SourcePath);
        FileNumbers = FileReaderInstance.FileNumbers;
        LanguageFileConstants = FileReaderInstance.LanguageFileConstants;
        PropDataList = FileReaderInstance.PropDataList;
        CharacterDataList = FileReaderInstance.CharacterDataList;

        FileReaderInstance.GetImportantNumbers(SourcePath);
        ModelNumbers = FileReaderInstance.ModelNumbers;

        FileReaderInstance.GetLanguageFiles(SourcePath);
        FileReaderInstance.GetConstants(SourcePath);

        ImportTextures();
        Import3DModels();

        Animations = FileReaderInstance.GetAnimations(SourcePath);
        SoundEffects = FileReaderInstance.GetSoundEffects(SourcePath);
        MusicTracks = FileReaderInstance.GetMusicTracks(SourcePath);
        ScreenImages = FileReaderInstance.GetScreenImages(SourcePath);
        AICommands = FileReaderInstance.GetAICommands(SourcePath);
        Stages = FileReaderInstance.GetStages(SourcePath);

        FileReaderInstance.GetGlobalAILists(SourcePath);
        GlobalAILists = FileReaderInstance.AILists;

        DropdownOptions.RefreshGameOptions();
    }

    /*
    public void LoadStage(Stage InStage)
    {
        if (HasLoadedStage)
        {
            UnloadStage();
        }

        // TEMPORARY - until I get a better way to load the language level files
        if (GetFileNameFromConstant(InStage.SetupFile).StartsWith("mp_"))
        {
            LevelFileName = GetFileNameFromConstant(InStage.SetupFile)[8..^2];
        }
        else
        {
            LevelFileName = GetFileNameFromConstant(InStage.SetupFile)[5..^2];
        }

        FileReader FileReaderInstance = new();
        BinaryFileReader BinaryFileReaderInstance = new();

        string BGFilePath = SourcePath + "\\assets\\ntsc-final\\files\\bgdata\\" + GetFileNameFromConstant(InStage.BackgroundFile);
        bool Success = LoadBackgroundFromBinary(BGFilePath);

        if (Success == false)
        {
            UnloadStage();
            return;
        }

        string CollisionFilePath = SourcePath + "\\assets\\ntsc-final\\tiles\\" + GetFileNameFromConstant(InStage.CollisionFile);
        JSONTileObject TileContainer = FileReaderInstance.ReadCollision(CollisionFilePath);

        Dictionary<string, int> HardcodedRoomNames = new()
        {
            { "ROOM_DISH_FIRINGRANGE", 0x0A },
            { "ROOM_DISH_HOLO1", 0x16 },
            { "ROOM_DISH_HOLO2", 0x17 },
            { "ROOM_DISH_HOLO3", 0x18 },
            { "ROOM_DISH_HOLO4", 0x19 },
            { "ROOM_DISH_DEVICELAB", 0x1E },
        };

        foreach (KeyValuePair<string, List<TileProperties>> CurrentRoom in TileContainer.rooms)
        {
            int CollisionRoomID;

            if (int.TryParse(CurrentRoom.Key[^4..], System.Globalization.NumberStyles.HexNumber, null, out int Result))
            {
                if (Result == 0) { continue; } // Room ID 0 is not used

                CollisionRoomID = Result;
            }
            else if (HardcodedRoomNames.TryGetValue(CurrentRoom.Key, out int HardcodedID))
            {
                CollisionRoomID = HardcodedID;
            }
            else
            {
                Debug.LogError("Collision room ID not recognized: " + CurrentRoom.Key);
                return;
            }

            Room CollisionRoom = Rooms[CollisionRoomID - 1];
            GameObject NewCollision = Instantiate(CollisionType, Vector3.zero, Quaternion.identity);

            NewCollision.GetComponent<Collision>().Init(CurrentRoom.Value, CollisionRoom);
            Collisions.Add(NewCollision.GetComponent<Collision>());

            if (EditModeBar.CurrentMode != EditMode.Collision)
            {
                NewCollision.SetActive(false);
            }
        }

        string PadsFilePath = SourcePath + "\\assets\\ntsc-final\\pads\\" + GetFileNameFromConstant(InStage.PadsFile);
        FileReaderInstance.ReadPads(PadsFilePath);

        foreach (PadProperties CurrentPad in FileReaderInstance.PadContainer.pads)
        {
            Vector3 PadPosition = new(CurrentPad.pos[0], CurrentPad.pos[1], -CurrentPad.pos[2]);
            GameObject NewPad = Instantiate(PadType, PadPosition, Quaternion.identity);
            NewPad.GetComponent<Pad>().Init(CurrentPad);
            Pads.Add(NewPad.GetComponent<Pad>());
        }

        foreach (WaygroupProperties CurrentWaygroup in FileReaderInstance.PadContainer.waygroups)
        {
            Waygroups.Add(new Waygroup(CurrentWaygroup));
        }
        foreach (Waygroup CurrentWaygroup in Waygroups)
        {
            foreach (WaygroupNeighbour CurrentNeighbour in CurrentWaygroup.Neighbours)
            {
                CurrentNeighbour.LinkedWaygroup = Waygroups[CurrentNeighbour.NeighbourID];
            }
        }

        foreach (WaypointProperties CurrentWaypoint in FileReaderInstance.PadContainer.waypoints)
        {
            Pad MatchingPad = Pads[int.Parse(CurrentWaypoint.pad[^4..], System.Globalization.NumberStyles.AllowHexSpecifier)];// GameObject.Find(CurrentWaypoint.pad).GetComponent<Pad>();
            MatchingPad.AddWaypoint(CurrentWaypoint);
            Waypoints.Add(MatchingPad.PadWaypoint);
        }

        foreach (Waypoint CurrentWaypoint in Waypoints)
        {
            if (CurrentWaypoint.WaygroupID != -1)
            {
                CurrentWaypoint.ParentWaygroup = Waygroups[CurrentWaypoint.WaygroupID];
                CurrentWaypoint.ParentWaygroup.Waypoints.Add(CurrentWaypoint);
            }

            foreach (WaypointNeighbour CurrentNeighbour in CurrentWaypoint.Neighbours)
            {
                CurrentNeighbour.LinkedWaypoint = Waypoints[CurrentNeighbour.NeighbourID];
            }
        }

        foreach (CoverPadProperties CurrentCoverPad in FileReaderInstance.PadContainer.cover)
        {
            Vector3 CoverPadPosition = new(CurrentCoverPad.pos[0], CurrentCoverPad.pos[1], -CurrentCoverPad.pos[2]);
            GameObject NewCoverPad = Instantiate(CoverPadType, CoverPadPosition, Quaternion.identity);
            NewCoverPad.GetComponent<Pad>().Init(CurrentCoverPad);
            CoverPads.Add(NewCoverPad.GetComponent<Pad>());
        }

        string SetupFilePath = BuildPath + "\\ntsc-final\\assets\\files\\" + GetFileNameFromConstant(InStage.SetupFile);
        Success = LoadSetupFromBinary(SetupFilePath);

        if (Success == false)
        {
            UnloadStage();
            return;
        }

        Objectives = FileReaderInstance.Objectives;
        IntroSettings = FileReaderInstance.IntroSettings;
        Props = FileReaderInstance.Props;
        VisibleProps = FileReaderInstance.VisibleProps;
        Characters = FileReaderInstance.Characters;
        Paths = FileReaderInstance.Paths;
        Tags = FileReaderInstance.Tags;
        AILists = FileReaderInstance.AILists;

        DropdownOptions.RefreshSetupOptions();

        if (SetupSettingsInstance.activeSelf)
        {
            SetupSettingsInstance.SetActive(false);
            SetupSettingsInstance.SetActive(true);
        }

        HasLoadedStage = true;

        BroadcastMessage("UpdateUI", SendMessageOptions.DontRequireReceiver); // Update all the editor windows to display all imported data
    }*/

    bool LoadBackgroundFromBinary(string FilePath)
    {
        BinaryFileReader BinaryFileReaderInstance = new();

        bool Success = BinaryFileReaderInstance.ReadLevelBG(FilePath);

        if (Success == false)
        {
            Debug.LogError("Failed to import background file");
            return false;
        }

        if (HasLoadedStage)
        {
            UnloadBackground();
        }

        foreach (RoomData CurrentData in BinaryFileReaderInstance.RoomDataList)
        {
            GameObject NewRoom = Instantiate(RoomType, CurrentData.Position, Quaternion.identity);
            NewRoom.GetComponent<Room>().Init(CurrentData);
            Rooms.Add(NewRoom.GetComponent<Room>());
        }
        foreach (PortalData CurrentData in BinaryFileReaderInstance.PortalDataList)
        {
            GameObject NewPortal = Instantiate(PortalType, Vector3.zero, Quaternion.identity);
            NewPortal.GetComponent<Portal>().Init(CurrentData);
            Portals.Add(NewPortal.GetComponent<Portal>());

            if (EditModeBar.CurrentMode != EditMode.Portals)
            {
                NewPortal.SetActive(false);
            }
        }
        foreach (LightData CurrentData in BinaryFileReaderInstance.LightDataList)
        {
            GameObject NewLight = Instantiate(BackgroundLightType, Vector3.zero, Quaternion.identity);
            NewLight.GetComponent<BackgroundLight>().Init(CurrentData);
            BackgroundLights.Add(NewLight.GetComponent<BackgroundLight>());

            if (EditModeBar.CurrentMode != EditMode.BackgroundLights)
            {
                NewLight.SetActive(false);
            }
        }
        foreach (VisibilityCommandData CurrentData in BinaryFileReaderInstance.VisibilityDataList)
        {
            VisibilityCommand NewCommand = new()
            {
                Type = CurrentData.ID
            };

            if (CurrentData.Arguments.Count != 0)
            {
                NewCommand.Arguments = new();
            }

            foreach (int Argument in CurrentData.Arguments)
            {
                NewCommand.Arguments.Add(Argument);
                /*switch (CurrentData.ID)
                {
                    case 0x01:
                        //Debug.Log("Number" + (Argument - 1).ToString("X") + " " + CurrentData.ID.ToString("X"));
                        NewCommand.Arguments.Add(Argument);
                        break;
                    case 0x14:
                    case 0x20:
                    case 0x24:
                    case 0x25:
                    case 0x26:
                    case 0x27:
                    case 0x2A:
                        //Debug.Log("Room " + Argument.ToString("X") + " " + Rooms.Count.ToString("X") + " " + CurrentData.ID.ToString("X"));
                        NewCommand.Arguments.Add(Rooms[Argument]);
                        break;
                    case 0x1F:
                    case 0x22:
                    case 0x23:
                    case 0x29:
                        //Debug.Log("Portal" + (Argument - 1).ToString("X") + " " + Portals.Count.ToString("X") + " " + CurrentData.ID.ToString("X"));
                        NewCommand.Arguments.Add(Portals[Argument]);
                        break;
                }*/
            }

            VisibilityCommands.Add(NewCommand);
        }

        return true;
    }

    bool LoadPadsFromBinary(string FilePath)
    {
        BinaryFileReader BinaryFileReaderInstance = new();
        bool Success = BinaryFileReaderInstance.ReadPads(FilePath);

        if (Success == false)
        {
            Debug.Log("Pads load false");
            return false;
        }

        if (HasLoadedStage)
        {
            UnloadPads();
        }

        foreach (PadData CurrentPad in BinaryFileReaderInstance.PadDataList)
        {
            GameObject NewPadObject = Instantiate(PadType, CurrentPad.Position, Quaternion.identity);

            Pad NewPad = NewPadObject.GetComponent<Pad>();
            NewPad.Init(CurrentPad);
            Pads.Add(NewPad);
        }

        /*foreach (Waygroup CurrentWaygroup in BinaryFileReaderInstance.Waygroups)
        {
            Waygroups.Add(CurrentWaygroup);
        }
        foreach (Waygroup CurrentWaygroup in Waygroups)
        {
            foreach (WaygroupNeighbour CurrentNeighbour in CurrentWaygroup.Neighbours)
            {
                CurrentNeighbour.LinkedWaygroup = Waygroups[CurrentNeighbour.NeighbourID];
            }
        }*/

        Waypoints = BinaryFileReaderInstance.Waypoints;
        Waygroups = BinaryFileReaderInstance.Waygroups;

        /*foreach (Waypoint CurrentWaypoint in BinaryFileReaderInstance.Waypoints)
        {
            Pad MatchingPad = Pads[CurrentWaypoint.ParentPadID];
            MatchingPad.AddWaypoint(CurrentWaypoint);
            Waypoints.Add(MatchingPad.PadWaypoint);
        }

        foreach (Waypoint CurrentWaypoint in Waypoints)
        {
            if (CurrentWaypoint.WaygroupID != -1)
            {
                CurrentWaypoint.ParentWaygroup = Waygroups[CurrentWaypoint.WaygroupID];
            }

            foreach (WaypointNeighbour CurrentNeighbour in CurrentWaypoint.Neighbours)
            {
                CurrentNeighbour.LinkedWaypoint = Waypoints[CurrentNeighbour.NeighbourID];
            }
        }*/

        foreach (PadData CurrentCoverPad in BinaryFileReaderInstance.CoverPadDataList)
        {
            GameObject NewCoverPad = Instantiate(PadType, CurrentCoverPad.Position, Quaternion.identity);
            NewCoverPad.GetComponent<Pad>().Init(CurrentCoverPad);
            CoverPads.Add(NewCoverPad.GetComponent<Pad>());
        }

        return true;
    }

    bool LoadSetupFromBinary(string FilePath)
    {
        BinaryFileReader BinaryFileReaderInstance = new();
        bool Success = BinaryFileReaderInstance.ReadSetup(FilePath);

        if (Success == false)
        {
            Debug.Log("Setup load false");
            return false;
        }

        if (HasLoadedStage)
        {
            UnloadSetup();
        }

        IntroSettings = BinaryFileReaderInstance.IntroSettings;
        Objectives = BinaryFileReaderInstance.Objectives;
        Props = BinaryFileReaderInstance.Props;
        VisibleProps = BinaryFileReaderInstance.VisibleProps;
        Characters = BinaryFileReaderInstance.Characters;
        Paths = BinaryFileReaderInstance.Paths;
        Tags = BinaryFileReaderInstance.Tags;
        AILists = BinaryFileReaderInstance.AILists;

        Briefing.StageTitle = BinaryFileReaderInstance.StageTitle;
        Briefing.AgentBriefing = BinaryFileReaderInstance.AgentBriefing;
        Briefing.SpecialAgentBriefing = BinaryFileReaderInstance.SpecialAgentBriefing;
        Briefing.PerfectAgentBriefing = BinaryFileReaderInstance.PerfectAgentBriefing;

        BinaryFileReaderInstance.ReadAndWriteAIListDebugFile(FilePath);

        return true;
    }

    void LoadSetupFromCode(string FilePath)
    {

    }

    bool LoadCollisionFromBinary(string FilePath)
    {
        BinaryFileReader BinaryFileReaderInstance = new();
        bool Success = BinaryFileReaderInstance.ReadCollision(FilePath);

        if (Success == false)
        {
            Debug.Log("Collision load false");
            return false;
        }

        if (HasLoadedStage)
        {
            UnloadCollision();
        }

        foreach (CollisionData CurrentCollision in BinaryFileReaderInstance.CollisionDataList)
        {
            GameObject NewCollision = Instantiate(CollisionType, Vector3.zero, Quaternion.identity);

            NewCollision.GetComponent<Collision>().Init(CurrentCollision);
            Collisions.Add(NewCollision.GetComponent<Collision>());

            if (EditModeBar.CurrentMode != EditMode.Collision)
            {
                NewCollision.SetActive(false);
            }
        }

        return true;
    }

    void UnloadStage()
    {
        UnloadBackground();
        UnloadCollision();
        UnloadPads();
        UnloadSetup();

        HasLoadedStage = false;
    }

    void UnloadSetup()
    {
        Briefing.StageTitle = null;
        Briefing.AgentBriefing = null;
        Briefing.SpecialAgentBriefing = null;
        Briefing.PerfectAgentBriefing = null;

        Props.Clear();
        VisibleProps.Clear();
        Characters.Clear();
        Tags.Clear();
        Objectives.Clear();
        Paths.Clear();
        IntroSettings.Clear();
        AILists.Clear();
        CharacterAILists.Clear();
        BackgroundAILists.Clear();
        CutsceneAILists.Clear();
        EnvironmentalAILists.Clear();

        GameBitFlagHolder.ClearStageFlags();
        GameBitFlagHolder.ClearLockFlags();
    }

    void UnloadPads()
    {
        foreach (Pad CurrentPad in Pads)
        {
            Destroy(CurrentPad.gameObject);
        }
        Pads.Clear();
        foreach (Pad CurrentCoverPad in CoverPads)
        {
            Destroy(CurrentCoverPad.gameObject);
        }
        CoverPads.Clear();

        Waypoints.Clear();
        Waygroups.Clear();
    }

    void UnloadBackground()
    {
        foreach (Room CurrentRoom in Rooms)
        {
            Destroy(CurrentRoom.gameObject);
        }
        Rooms.Clear();
        foreach (Portal CurrentPortal in Portals)
        {
            Destroy(CurrentPortal.gameObject);
        }
        Portals.Clear();
        foreach (BackgroundLight CurrentLight in BackgroundLights)
        {
            Destroy(CurrentLight.gameObject);
        }
        BackgroundLights.Clear();

        VisibilityCommands.Clear();
    }

    void UnloadCollision()
    {
        foreach (Collision CurrentCollision in Collisions)
        {
            Destroy(CurrentCollision.gameObject);
        }
        Collisions.Clear();
    }

    void Import3DModels()
    {
        if (PropModels.Count > 0) { PropModels.Clear(); }
        if (GunModels.Count > 0) { GunModels.Clear(); }
        if (CharacterModels.Count > 0) { CharacterModels.Clear(); }

        string[] Files = Directory.GetFiles(SourcePath + "\\assets\\ntsc-final\\files\\props");
        foreach (string FilePath in Files)
        {
            BinaryFileReader BinaryFileReaderInstance = new();
            bool Succeeded = BinaryFileReaderInstance.Read3DModel(FilePath);
            if (Succeeded)
            {
                PropModels.Add(Path.GetFileName(FilePath), BinaryFileReaderInstance.ModelData);
            }
        }

        Files = Directory.GetFiles(SourcePath + "\\assets\\ntsc-final\\files\\guns");
        foreach (string FilePath in Files)
        {
            BinaryFileReader BinaryFileReaderInstance = new();
            bool Succeeded = BinaryFileReaderInstance.Read3DModel(FilePath);
            if (Succeeded)
            {
                GunModels.Add(Path.GetFileName(FilePath), BinaryFileReaderInstance.ModelData);
            }
        }

        Files = Directory.GetFiles(SourcePath + "\\assets\\ntsc-final\\files\\chrs");
        foreach (string FilePath in Files)
        {
            BinaryFileReader BinaryFileReaderInstance = new();
            bool Succeeded = BinaryFileReaderInstance.Read3DModel(FilePath);
            if (Succeeded)
            {
                CharacterModels.Add(Path.GetFileName(FilePath), BinaryFileReaderInstance.ModelData);
            }
        }

        // Set up a dummy head model for the random head option. This will use the first head model available, but will still carry the random head ID (FF) under the hood.
        ModelData FirstHeadData = CharacterDataList.Find(x => x.IsHead);
        if (FirstHeadData != null)
        {
            RandomHeadData = new()
            {
                IsHead = true,
            };

            PropModel FirstHeadModel = CharacterModels[GetFileNameFromConstant(FirstHeadData.FileName)];

            RandomHeadModel = new();
            RandomHeadModel.Meshes.AddRange(FirstHeadModel.Meshes);
            RandomHeadModel.Nodes.AddRange(FirstHeadModel.Nodes);
            RandomHeadModel.TextureIDs.AddRange(FirstHeadModel.TextureIDs);
        }
    }

    void ImportTextures()
    {
        string[] Files = Directory.GetFiles(SourcePath + "\\assets\\ntsc-final\\textures");

        if (Materials.Count > 0) { Materials.Clear(); }

        foreach (string FileName in Files)
        {
            if (Path.GetExtension(FileName) != ".bin")
            {
                continue;
            }

            if (int.TryParse(Path.GetFileNameWithoutExtension(FileName), System.Globalization.NumberStyles.HexNumber, null, out int TextureNumber) == false)
            {
                Debug.LogError($"Texture at {FileName} does not have a proper number identifier");
                continue;
            }

            BinaryFileReader BinaryFileReaderInstance = new();
            Texture2D NewTexture = BinaryFileReaderInstance.ReadTexture(FileName);

            Material NewMaterial = new(NormalGeometryShader) { name = NewTexture.name };
            NewMaterial.SetTexture("_MainTex", NewTexture);
            Materials.Add(TextureNumber, NewMaterial);
        }
    }

    public Pad AddPad(Vector3 Position)
    {
        GameObject NewPadObject = Instantiate(PadType, Position, Quaternion.identity);

        Pad NewPad = NewPadObject.GetComponent<Pad>();
        NewPad.InitNew();
        Pads.Add(NewPad);

        DropdownOptions.Pads.Add(NewPad.NumberString);

        return NewPad;
    }

    public void DeletePad(Pad InPad)
    {
        foreach (GuardPath CurrentPath in InPad.ConnectedPaths)
        {
            CurrentPath.Pads.Remove(InPad);
        }

        foreach (IntroSetting CurrentIntro in InPad.ConnectedIntroSettings)
        {
            CurrentIntro.IntroPad = null;
        }

        if (InPad.PadProp != null)
        {
            DeleteProp(InPad.PadProp, false);
        }

        Pads.RemoveAt(InPad.Number);

        for (int i = InPad.Number; i < Pads.Count; ++i)
        {
            Pad CurrentPad = Pads[i];
            CurrentPad.Number -= 1;
            CurrentPad.name = "Pad" + CurrentPad.NumberString;
        }

        Destroy(InPad.gameObject);

        DropdownOptions.RefreshObjects();

        SelectionManagerInstance.ClearSelectedObjects();
    }

    public void DeleteObject(List<GameObject> InObjects)
    {
        if (EditModeBar.CurrentMode == EditMode.Objects)
        {
            foreach (GameObject CurrentObject in InObjects)
            {
                Pad CurrentPad = CurrentObject.GetComponent<Pad>();

                foreach (GuardPath CurrentPath in CurrentPad.ConnectedPaths)
                {
                    CurrentPath.Pads.Remove(CurrentPad);
                }

                foreach (IntroSetting CurrentIntro in CurrentPad.ConnectedIntroSettings)
                {
                    CurrentIntro.IntroPad = null;
                }

                if (CurrentPad.PadProp != null)
                {
                    DeleteProp(CurrentPad.PadProp, false);
                }

                Pads.RemoveAt(CurrentPad.Number);

                Destroy(CurrentObject);
            }
        }

        for (int i = 0; i < Pads.Count; ++i)
        {
            Pad CurrentPad = Pads[i];
            CurrentPad.Number = i;
            CurrentPad.name = "Pad" + CurrentPad.NumberString;
        }

        DropdownOptions.RefreshObjects();

        SelectionManagerInstance.ClearSelectedObjects();
    }

    public Pad AddCoverPad(Vector3 Position)
    {
        GameObject NewCoverPadObject = Instantiate(PadType, Position, Quaternion.identity);

        Pad NewCoverPad = NewCoverPadObject.GetComponent<Pad>();
        NewCoverPad.IsCoverPad = true;
        NewCoverPad.InitNew();
        CoverPads.Add(NewCoverPad);

        DropdownOptions.CoverPads.Add(NewCoverPad.NumberString);

        return NewCoverPad;
    }

    public void DeleteCoverPad(Pad InPad)
    {
        CoverPads.RemoveAt(InPad.Number);

        for (int i = InPad.Number; i < CoverPads.Count; ++i)
        {
            Pad CurrentCoverPad = CoverPads[i];
            CurrentCoverPad.Number -= 1;
            CurrentCoverPad.name = "CoverPad" + CurrentCoverPad.NumberString;
        }

        DropdownOptions.RefreshCoverPads();

        Destroy(InPad.gameObject);

        SelectionManagerInstance.ClearSelectedObjects();
    }

    public static Prop AddProp(PropType InType)
    {
        Prop NewProp = new()
        {
            InternalID = Props.Count,

            Type = InType,
            Scale = 256,
            MaxDamage = 1000,

            IsGlobal = true,
        };

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
                NewProp.TagID = Tags.Count - 1;
                break;
            default:
                VisibleProps.Add(NewProp);
                break;
        }

        DropdownOptions.RefreshObjects();

        return NewProp;
    }

    public void AddProp(Vector3 Position, ModelData InData)
    {
        Prop NewProp = new()
        {
            InternalID = Props.Count,

            Type = PropType.StandardObject,
            Scale = 256,
            MaxDamage = 1000,

            Data = InData,

            IsGlobal = false,
        };

        NewProp.Model = PropModels[NewProp.Data.FilePath];

        Props.Add(NewProp);
        VisibleProps.Add(NewProp);

        GameObject NewPadObject = Instantiate(PadType, Position, Quaternion.identity);

        Pad NewPad = NewPadObject.GetComponent<Pad>();
        NewPad.InitNew();

        NewPad.HasBoundingBoxData = true;
        NewProp.AddFlags(GameBitFlagHolder.GetBitFlag("OBJFLAG", 0x00000020));
        NewProp.AddFlags(GameBitFlagHolder.GetBitFlag("OBJFLAG", 0x00000040));
        NewProp.AddFlags(GameBitFlagHolder.GetBitFlag("OBJFLAG", 0x00000080));
        NewPad.ChangeScale(-80, 80, -80, 80, -80, 80);

        Pads.Add(NewPad);

        DropdownOptions.Pads.Add(NewPad.NumberString);

        NewProp.ParentPad = NewPad;
        NewPad.AddProp(NewProp);

        DropdownOptions.RefreshObjects();
    }

    public void AddCharacter(Vector3 Position, ModelData InData)
    {
        Prop NewProp = new()
        {
            InternalID = Props.Count,

            Type = PropType.Character,
            Scale = 256,
            MaxDamage = 1000,

            Data = InData,

            IsGlobal = false,
        };

        IEnumerable<int> CharacterIDs = Characters.Select(x => x.CharacterID);

        NewProp.CharacterID = Enumerable.Range(0, int.MaxValue)
            .Except(CharacterIDs)
            .FirstOrDefault();

        NewProp.Body = CharacterModels[NewProp.Data.FilePath];
        NewProp.HeadData = RandomHeadData;
        NewProp.Head = RandomHeadModel;

        Props.Add(NewProp);
        Characters.Add(NewProp);

        GameObject NewPadObject = Instantiate(PadType, Position, Quaternion.identity);

        Pad NewPad = NewPadObject.GetComponent<Pad>();
        NewPad.InitNew();

        Pads.Add(NewPad);

        DropdownOptions.Pads.Add(NewPad.NumberString);

        NewProp.ParentPad = NewPad;
        NewPad.AddProp(NewProp);

        DropdownOptions.RefreshObjects();
    }

    public void AddTag(List<GameObject> InObjects)
    {
        foreach (GameObject CurrentObject in InObjects)
        {
            if (CurrentObject.TryGetComponent(out Pad CurrentPad) && CurrentPad.PadProp != null)
            {
                Prop CurrentProp = CurrentPad.PadProp;

                if (CurrentProp.AttachedTag != null) { continue; }

                Prop NewTag = new()
                {
                    InternalID = Props.Count,

                    Type = PropType.Tag,
                };


                IEnumerable<int> TagIDs = Tags.Select(x => x.TagID);

                NewTag.TagID = Enumerable.Range(0, int.MaxValue)
                    .Except(TagIDs)
                    .FirstOrDefault();

                Props.Add(NewTag);
                Tags.Add(NewTag);

                NewTag.TaggedProp = CurrentProp;
                CurrentProp.AttachedTag = NewTag;

                DropdownOptions.RefreshObjects();
            }
        }
    }

    public void DeleteProp(List<GameObject> InObjects)
    {
        foreach (GameObject CurrentObject in InObjects)
        {
            Pad CurrentPad = CurrentObject.GetComponent<Pad>();

            if (CurrentPad.PadProp != null)
            {
                DeleteProp(CurrentPad.PadProp, false);
            }
        }

        DropdownOptions.RefreshObjects();

        SelectionManagerInstance.ClearSelectedObjects();
    }

    public static void DeleteProp(Prop InProp, bool RefreshDropdowns = true)
    {
        if (InProp.IsAttachedToOtherProp || InProp.IsAttachedToCharacter)
        {
            if (InProp.Type == PropType.Rename)
            {
                InProp.ParentProp.RenameObject = null;
            }
            else
            {
                InProp.ParentProp.SubProps.Remove(InProp);
            }
        }

        if (InProp.Type == PropType.LiftDoor)
        {
            InProp.LiftStopLift.LiftStops.Remove(InProp);
        }

        List<Prop> SubProps = new();
        SubProps.AddRange(InProp.SubProps);

        foreach (Prop CurrentSubProp in SubProps)
        {
            DeleteProp(CurrentSubProp, false);
        }

        List<Prop> LiftStops = new();
        LiftStops.AddRange(InProp.LiftStops);

        foreach (Prop CurrentLiftStop in LiftStops)
        {
            DeleteProp(CurrentLiftStop, false);
        }

        foreach (BitFlag CurrentLockFlag in InProp.LockBits)
        {
            CurrentLockFlag.ConnectedProps.Remove(InProp);
        }
        foreach (BitFlag CurrentLockFlag in InProp.DoorFlags)
        {
            CurrentLockFlag.ConnectedProps.Remove(InProp);
        }

        foreach (ObjectiveCondition CurrentCondition in InProp.ConnectedObjectiveConditions)
        {
            CurrentCondition.TagID = 0;
            CurrentCondition.ObjectiveTag = null;
        }

        if (InProp.RenameObject != null) { DeleteProp(InProp.RenameObject, false); }
        Debug.Log(InProp.AttachedTag == null);
        if (InProp.LinkedDoor != null) { InProp.LinkedDoor.LinkedDoor = null; }
        if (InProp.LinkedWeapon != null) { InProp.LinkedWeapon.LinkedWeapon = null; }
        if (InProp.Chair != null) { InProp.Chair.ConnectedCharacter = null; }
        if (InProp.TaggedProp != null) { InProp.TaggedProp.AttachedTag = null; }
        if (InProp.AttachedTag != null) { InProp.AttachedTag.TaggedProp = null; }
        if (InProp.AIList != null) { InProp.AIList.ConnectedProps.Remove(InProp); }

        if (InProp.ParentPad != null && !InProp.IsAttachedToOtherProp && !InProp.IsAttachedToCharacter)
        {
            InProp.ParentPad.DetachProp();
        }

        switch (InProp.Type)
        {
            case PropType.DoorScale:
            case PropType.LinkGuns:
            case PropType.LiftDoor:
            case PropType.Rename:
            case PropType.TrainingHolographRequirement:
                break;
            case PropType.Character:
                Characters.Remove(InProp);
                break;
            case PropType.Tag:
                Tags.Remove(InProp);
                break;
            default:
                VisibleProps.Remove(InProp);
                break;
        }

        Props.Remove(InProp);

        if (RefreshDropdowns) { Debug.Log("REFRESHDROPDOWNS"); DropdownOptions.RefreshObjects(); }

        for (int i = InProp.InternalID; i < Props.Count; ++i)
        {
            Props[i].InternalID -= 1;
        }

        //SelectionManagerInstance.ClearSelectedObjects();
    }

    public void Copy(List<GameObject> InObjects)
    {
        Clipboard.Clear();
        Clipboard.Capacity = InObjects.Count;

        foreach (GameObject CurrentObject in InObjects)
        {
            if (CurrentObject.TryGetComponent(out Pad CurrentPad))
            {
                if (!CurrentPad.IsCoverPad)
                {
                    PadData NewData = new()
                    {
                        IsCoverPad = false,

                        Position = CurrentPad.Position,
                        Direction = CurrentPad.Direction,
                        Up = CurrentPad.Up,
                        Normal = CurrentPad.Normal,
                        XMin = CurrentPad.XMin,
                        XMax = CurrentPad.XMax,
                        YMin = CurrentPad.YMin,
                        YMax = CurrentPad.YMax,
                        ZMin = CurrentPad.ZMin,
                        ZMax = CurrentPad.ZMax,

                        IntegerPosition = CurrentPad.IntegerPosition,

                        DirectionAlignToX = CurrentPad.DirectionAlignToX,
                        DirectionAlignToY = CurrentPad.DirectionAlignToY,
                        DirectionAlignToZ = CurrentPad.DirectionAlignToZ,
                        DirectionAlignInverted = CurrentPad.DirectionAlignInverted,
                        UpAlignToX = CurrentPad.UpAlignToX,
                        UpAlignToY = CurrentPad.UpAlignToY,
                        UpAlignToZ = CurrentPad.UpAlignToZ,
                        UpAlignInverted = CurrentPad.UpAlignInverted,

                        HasBoundingBoxData = CurrentPad.HasBoundingBoxData,

                        AIWaitsForLift = CurrentPad.AIWaitsForLift,
                        AIRidesLift = CurrentPad.AIRidesLift,
                        AIWalksDirectlyTo = CurrentPad.AIWalksDirectlyTo,
                        AIDrops = CurrentPad.AIDrops,
                        AICrouches = CurrentPad.AICrouches,

                        AIMovesWhileFalling = CurrentPad.AIMovesWhileFalling,
                        AIDucks = CurrentPad.AIDucks,
                        LiftNumber = CurrentPad.LiftNumber,

                        PadProp = CurrentPad.PadProp != null ? new Prop(CurrentPad.PadProp) : null,
                    };

                    Clipboard.Add(NewData);
                }
                else
                {
                    PadData NewData = new()
                    {
                        IsCoverPad = true,

                        Position = CurrentPad.Position,
                        Direction = CurrentPad.Direction,

                        CoverFlags = CurrentPad.CoverFlags,
                    };

                    Clipboard.Add(NewData);
                }
            }
        }
    }

    public void Paste(Vector3 Position)
    {
        foreach (PadData CurrentData in Clipboard)
        {
            if (!CurrentData.IsCoverPad)
            {
                CurrentData.Number = Pads.Count;
                CurrentData.Position = Position;

                GameObject NewPadObject = Instantiate(PadType, Position, Quaternion.identity);

                Pad NewPad = NewPadObject.GetComponent<Pad>();
                NewPad.Init(CurrentData);
                Pads.Add(NewPad);

                if (CurrentData.PadProp != null)
                {
                    Prop PadProp = new(CurrentData.PadProp)
                    {
                        ParentPad = NewPad,
                        InternalID = Props.Count
                    };

                    Props.Add(PadProp);
                    if (PadProp.Type == PropType.Character) { Characters.Add(PadProp); }
                    else { VisibleProps.Add(PadProp); }

                    foreach (Prop CurrentSubProp in PadProp.SubProps)
                    {
                        CurrentSubProp.InternalID = Props.Count;

                        Props.Add(CurrentSubProp);
                        if (CurrentSubProp.Type == PropType.Character) { Characters.Add(CurrentSubProp); }
                        else { VisibleProps.Add(CurrentSubProp); }
                    }

                    if (PadProp.RenameObject != null)
                    {
                        PadProp.RenameObject.InternalID = Props.Count;

                        Props.Add(PadProp.RenameObject);
                    }

                    NewPad.AddProp(CurrentData.PadProp);
                }
            }
            else
            {
                CurrentData.Number = CoverPads.Count;
                CurrentData.Position = Position;

                GameObject NewPadObject = Instantiate(PadType, Position, Quaternion.identity);

                Pad NewPad = NewPadObject.GetComponent<Pad>();
                NewPad.Init(CurrentData);
                CoverPads.Add(NewPad);
            }
        }

        DropdownOptions.RefreshObjects();
        DropdownOptions.RefreshCoverPads();
    }

    public void Clone(List<GameObject> InObjects)
    {
        List<PadData> ClonedObjects = new();

        foreach (GameObject CurrentObject in InObjects)
        {
            if (CurrentObject.TryGetComponent(out Pad CurrentPad))
            {
                if (!CurrentPad.IsCoverPad)
                {
                    PadData NewData = new()
                    {
                        IsCoverPad = false,

                        Position = CurrentPad.Position,
                        Direction = CurrentPad.Direction,
                        Up = CurrentPad.Up,
                        Normal = CurrentPad.Normal,
                        XMin = CurrentPad.XMin,
                        XMax = CurrentPad.XMax,
                        YMin = CurrentPad.YMin,
                        YMax = CurrentPad.YMax,
                        ZMin = CurrentPad.ZMin,
                        ZMax = CurrentPad.ZMax,

                        IntegerPosition = CurrentPad.IntegerPosition,

                        DirectionAlignToX = CurrentPad.DirectionAlignToX,
                        DirectionAlignToY = CurrentPad.DirectionAlignToY,
                        DirectionAlignToZ = CurrentPad.DirectionAlignToZ,
                        DirectionAlignInverted = CurrentPad.DirectionAlignInverted,
                        UpAlignToX = CurrentPad.UpAlignToX,
                        UpAlignToY = CurrentPad.UpAlignToY,
                        UpAlignToZ = CurrentPad.UpAlignToZ,
                        UpAlignInverted = CurrentPad.UpAlignInverted,

                        HasBoundingBoxData = CurrentPad.HasBoundingBoxData,

                        AIWaitsForLift = CurrentPad.AIWaitsForLift,
                        AIRidesLift = CurrentPad.AIRidesLift,
                        AIWalksDirectlyTo = CurrentPad.AIWalksDirectlyTo,
                        AIDrops = CurrentPad.AIDrops,
                        AICrouches = CurrentPad.AICrouches,

                        AIMovesWhileFalling = CurrentPad.AIMovesWhileFalling,
                        AIDucks = CurrentPad.AIDucks,
                        LiftNumber = CurrentPad.LiftNumber,

                        PadProp = CurrentPad.PadProp != null ? new Prop(CurrentPad.PadProp) : null,
                    };

                    ClonedObjects.Add(NewData);
                }
                else
                {
                    PadData NewData = new()
                    {
                        IsCoverPad = true,

                        Position = CurrentPad.Position,
                        Direction = CurrentPad.Direction,

                        CoverFlags = CurrentPad.CoverFlags,
                    };

                    ClonedObjects.Add(NewData);
                }
            }
        }

        foreach (PadData CurrentData in ClonedObjects)
        {
            if (!CurrentData.IsCoverPad)
            {
                CurrentData.Number = Pads.Count;
                CurrentData.Position += new Vector3(0f, 0f, 50.0f);

                GameObject NewPadObject = Instantiate(PadType, CurrentData.Position, Quaternion.identity);

                Pad NewPad = NewPadObject.GetComponent<Pad>();
                NewPad.Init(CurrentData);
                Pads.Add(NewPad);

                if (CurrentData.PadProp != null)
                {
                    Prop PadProp = new(CurrentData.PadProp)
                    {
                        ParentPad = NewPad,
                        InternalID = Props.Count
                    };

                    Props.Add(PadProp);
                    if (PadProp.Type == PropType.Character) { Characters.Add(PadProp); }
                    else { VisibleProps.Add(PadProp); }

                    foreach (Prop CurrentSubProp in PadProp.SubProps)
                    {
                        CurrentSubProp.InternalID = Props.Count;

                        Props.Add(CurrentSubProp);
                        if (CurrentSubProp.Type == PropType.Character) { Characters.Add(CurrentSubProp); }
                        else { VisibleProps.Add(CurrentSubProp); }
                    }

                    if (PadProp.RenameObject != null)
                    {
                        PadProp.RenameObject.InternalID = Props.Count;

                        Props.Add(PadProp.RenameObject);
                    }

                    NewPad.AddProp(CurrentData.PadProp);
                }
            }
            else
            {
                CurrentData.Number = CoverPads.Count;
                CurrentData.Position += new Vector3(0f, 0f, 50.0f);

                GameObject NewPadObject = Instantiate(PadType, CurrentData.Position, Quaternion.identity);

                Pad NewPad = NewPadObject.GetComponent<Pad>();
                NewPad.Init(CurrentData);
                CoverPads.Add(NewPad);
            }
        }

        DropdownOptions.RefreshObjects();
        DropdownOptions.RefreshCoverPads();
    }

    public static void DeleteObjective(Objective InObjective)
    {
        // Remove all the references to this objective
        foreach (ObjectiveCondition CurrentCondition in InObjective.Conditions)
        {
            switch (CurrentCondition.Type)
            {
                case ConditionType.DestroyObject:
                case ConditionType.CollectObject:
                case ConditionType.DepositObject:
                case ConditionType.HolographObject:
                    if (CurrentCondition.ObjectiveTag != null) { CurrentCondition.ObjectiveTag.ConnectedObjectiveConditions.Remove(CurrentCondition); }
                    break;
                case ConditionType.CompletionFlags:
                case ConditionType.FailureFlags:
                    if (CurrentCondition.StageFlag != null) { CurrentCondition.StageFlag.ConnectedObjectiveConditions.Remove(CurrentCondition); }
                    break;
                case ConditionType.EnterRoom:
                    CurrentCondition.ObjectiveRoom.ConnectedObjectiveConditions.Remove(CurrentCondition);
                    break;
            }
        }

        Objectives.Remove(InObjective);

        // Reorder the remaining objectives, so the ID's match up and there isn't a hole
        for (int i = InObjective.ID + 1; i < Objectives.Count; ++i)
        {
            Objective CurrentObjective = Objectives[i];
            CurrentObjective.ID -= 1;
        }

        DropdownOptions.RefreshObjectives();
    }

    public static AIList AddAIList()
    {
        int ID = 0x1000;
        int Index = 0;

        foreach (AIList CurrentAIList in BackgroundAILists)
        {
            //Debug.Log(ID + " " + CurrentAIList.ID);
            if (ID > 0x10FF) // Maximum number of AI lists
            {
                return null;
            }
            else if (ID == CurrentAIList.ID)
            {
                ++ID;
                ++Index;
                continue;
            }
            else
            {
                break;
            }
        }

        AIList NewAIList = new()
        {
            ID = ID,
            Type = AIListType.Background,
            DisplayName = "New script",
        };

        AILists.Add(NewAIList);
        AILists.Sort((x, y) => x.ID.CompareTo(y.ID));
        BackgroundAILists.Insert(Index, NewAIList);
        DropdownOptions.RefreshAILists();

        return NewAIList;
    }

    public static void DeleteAIList(AIList InAIList)
    {
        // Remove all connections (parameters, AILists & Props that use this AIList)

        switch (InAIList.Type)
        {
            case AIListType.Character:
                CharacterAILists.Remove(InAIList);
                break;
            case AIListType.Cutscene:
                CutsceneAILists.Remove(InAIList);
                break;
            case AIListType.Background:
                BackgroundAILists.Remove(InAIList);
                break;
            case AIListType.Environmental:
                EnvironmentalAILists.Remove(InAIList);
                break;
        }

        foreach (Prop CurrentProp in InAIList.ConnectedProps)
        {
            CurrentProp.AIList = null;
        }

        AILists.Remove(InAIList);

        DropdownOptions.RefreshAILists();
    }

    public static AICommand GetAICommand(string CommandName)
    {
        for (int i = 0; i < AICommands.Count; ++i)
        {
            AICommand CurrentCommand = AICommands[i];
            if (CurrentCommand.Name == CommandName)
            {
                return CurrentCommand;
            }
        }

        return null;
    }

    public static Room GetRoom(int RoomID)
    {
        if (RoomID == 0)
        {
            return null;
        }

        return Rooms[RoomID - 1];
    }

    public string GetFileNameFromConstant(string InConstant)
    {
        int FileNumber = FileNumbers[InConstant];
        return FileNames[FileNumber];
    }
}

