using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PropModelPicker : MonoBehaviour
{
    public TMP_InputField SearchDropdown;
    public ScrollableList ModelList;
    public SceneView ModelView;

    public GameObject ModelSample;
    private MeshFilter SampleMeshFilter;
    private MeshRenderer SampleMeshRenderer;

    public GameObject InputBlocker;
    public DataHolder DataHolderInstance;

    bool NewPropMode = false;
    Vector3 NewPropPosition;

    public Pad CurrentPad;

    private List<ModelData> PropDataList;

    ModelMode CurrentMode;

    public enum ModelMode
    {
        Prop,
        Character,
        Head
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SampleMeshFilter = ModelSample.GetComponent<MeshFilter>();
        SampleMeshRenderer = ModelSample.GetComponent<MeshRenderer>();

        ModelList.ListItemSelected += OnModelSelected;

        //SceneView ModelView = transform.Find("Controls/3DModels/ModelView/ModelViewImage").GetComponent<SceneView>();
        ModelView.ModelSample = ModelSample;

        DataHolder.SelectionManagerInstance.SelectionChanged += OnSelectionChanged;
    }

    private void OnEnable()
    {
        InputBlocker.SetActive(true);
    }

    private void OnDisable()
    {
        InputBlocker.SetActive(false);

        SearchDropdown.SetTextWithoutNotify(string.Empty);
        ModelList.Clear();
        SampleMeshFilter.sharedMesh = null;
    }

    public void EnterPad(Pad InPad, ModelMode InModelMode)
    {
        NewPropMode = false;
        CurrentMode = InModelMode;

        gameObject.SetActive(true);
        CurrentPad = InPad;

        List<string> ModelNames = new();
        List<object> ModelObjects = new();

        PropDataList = (CurrentMode == ModelMode.Character || CurrentMode == ModelMode.Head) ? DataHolder.CharacterDataList : DataHolder.PropDataList;

        foreach (ModelData CurrentData in PropDataList)
        {
            if ((CurrentMode != ModelMode.Head && CurrentData.IsHead) || (CurrentMode == ModelMode.Head && !CurrentData.IsHead)) { continue; }
            ModelNames.Add(CurrentData.FilePath);
            ModelObjects.Add(CurrentData);
        }

        ModelList.AddToList(ModelNames, ModelObjects);
    }

    public void CreateNewProp(Vector3 Position, ModelMode InModelMode)
    {
        NewPropMode = true;
        CurrentMode = InModelMode;

        gameObject.SetActive(true);
        NewPropPosition = Position;

        List<string> ModelNames = new();
        List<object> ModelObjects = new();

        PropDataList = CurrentMode == ModelMode.Character ? DataHolder.CharacterDataList : DataHolder.PropDataList;

        foreach (ModelData CurrentData in PropDataList)
        {
            if (CurrentData.IsHead) { continue; }
            ModelNames.Add(CurrentData.FilePath);
            ModelObjects.Add(CurrentData);
        }

        ModelList.AddToList(ModelNames, ModelObjects);
    }

    public void OnSearchQueryEntered(string Input)
    {
        List<string> ModelNames = new();
        List<object> ModelObjects = new();

        if (string.IsNullOrEmpty(Input))
        {
            foreach (ModelData CurrentData in PropDataList)
            {
                if ((CurrentMode != ModelMode.Head && CurrentData.IsHead) || (CurrentMode == ModelMode.Head && !CurrentData.IsHead)) { continue; }
                ModelNames.Add(CurrentData.FilePath);
                ModelObjects.Add(CurrentData);
            }
        }
        else
        {
            foreach (ModelData CurrentData in PropDataList)
            {
                if (CurrentData.FilePath?.Contains(Input, System.StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    if ((CurrentMode != ModelMode.Head && CurrentData.IsHead) || (CurrentMode == ModelMode.Head && !CurrentData.IsHead)) { continue; }
                    ModelNames.Add(CurrentData.FilePath);
                    ModelObjects.Add(CurrentData);
                }
            }
        }

        ModelList.AddToList(ModelNames, ModelObjects);
    }

    public void OnModelSelected(ListItem SelectedItem)
    {
        ModelView.ResetView();
        if (CurrentMode == ModelMode.Character || CurrentMode == ModelMode.Head) { ModelSample.transform.Rotate(0, 180, 0); }

        ModelData CurrentData = (ModelData)SelectedItem.AssociatedObject;
        PropModel CurrentModel;

        if (CurrentMode == ModelMode.Character || CurrentMode == ModelMode.Head)
        {
            CurrentModel = DataHolder.CharacterModels[CurrentData.FilePath];
        }
        else
        {
            CurrentModel = DataHolder.PropModels[CurrentData.FilePath];
        }

        SampleMeshFilter.mesh = CurrentModel.Meshes[0];
        Mesh CurrentMesh = SampleMeshFilter.mesh;

        Material[] MeshMaterials = new Material[CurrentMesh.subMeshCount];
        for (int i = 0; i < CurrentMesh.subMeshCount; ++i)
        {
            if (DataHolder.Materials.TryGetValue(CurrentModel.TextureIDs[i], out Material NewMaterial))
            {
                MeshMaterials[i] = NewMaterial;
            }
            else
            {
                Debug.LogWarning($"Could not find material of texture ID {CurrentModel.TextureIDs[i]} in model {SelectedItem.Value}");
                MeshMaterials[i] = DataHolder.Materials[0];
            }
        }
        SampleMeshRenderer.materials = MeshMaterials;
    }

    public void OnModelSubmitted()
    {
        if (ModelList.SelectedItem == null)
        {
            return;
        }

        ModelData SelectedData = (ModelData)ModelList.SelectedItem.AssociatedObject;

        if (!NewPropMode)
        {
            if (CurrentMode == ModelMode.Head)
            {
                CurrentPad.ChangeHead(SelectedData);
            }
            else
            {
                CurrentPad.ChangeModel(SelectedData);
            }
        }
        else
        {
            if (CurrentMode == ModelMode.Character)
            {
                DataHolderInstance.AddCharacter(NewPropPosition, SelectedData);
            }
            else
            {
                DataHolderInstance.AddProp(NewPropPosition, SelectedData);
            }

            gameObject.SetActive(false);
        }
    }

    public void OnSelectionChanged(List<GameObject> PreviousObjects)
    {
        gameObject.SetActive(false);
    }
}
