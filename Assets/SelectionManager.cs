using Assimp;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SelectionManager : MonoBehaviour
{
    public static List<GameObject> SelectedObjects { get; private set; } = new();
    public static GameObject CurrentSelectedObject => SelectedObjects.FirstOrDefault();
    public bool MultipleObjectsSelected => SelectedObjects.Count > 1;

    public event SelectionChange SelectionChanged;

    public GameObject Gizmo;
    public GameObject GizmoXAxis;
    public GameObject GizmoYAxis;
    public GameObject GizmoZAxis;

    public int GizmoSensitivity = 3;

    public bool IsDraggingGizmoXAxis { get; private set; }
    public bool IsDraggingGizmoYAxis { get; private set; }
    public bool IsDraggingGizmoZAxis { get; private set; }

    ObjectInspector ObjectInspectorInstance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GizmoXAxis.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.red);
        GizmoYAxis.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.green);
        GizmoZAxis.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.blue);

        ObjectInspectorInstance = DataHolder.ObjectInspectorInstance;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsDraggingGizmoXAxis)
        {
            float MouseX = Input.GetAxis("Mouse X");

            if (EditModeBar.CurrentMode == EditMode.Objects)
            {
                if (!MultipleObjectsSelected)
                {
                    Pad CurrentPad = CurrentSelectedObject.GetComponent<Pad>();

                    Vector3 NewPosition = new Vector3(CurrentPad.Position.x + MouseX * GizmoSensitivity, CurrentPad.Position.y, CurrentPad.Position.z);
                    CurrentPad.ChangePosition(NewPosition);
                    Gizmo.transform.position = CurrentPad.transform.position;
                }
                else
                {
                    foreach (GameObject CurrentObject in SelectedObjects)
                    {
                        Pad CurrentPad = CurrentObject.GetComponent<Pad>();

                        Vector3 NewPosition = new Vector3(CurrentPad.Position.x + MouseX * GizmoSensitivity, CurrentPad.Position.y, CurrentPad.Position.z);
                        CurrentPad.ChangePosition(NewPosition);
                    }

                    Vector3 GizmoPosition = Gizmo.transform.position;
                    GizmoPosition.x += MouseX * GizmoSensitivity;
                    Gizmo.transform.position = GizmoPosition;
                }

                ObjectInspectorInstance.UpdatePositionFields();
            }
        }
        else if (IsDraggingGizmoYAxis)
        {
            float MouseY = Input.GetAxis("Mouse Y");

            if (EditModeBar.CurrentMode == EditMode.Objects)
            {
                if (!MultipleObjectsSelected)
                {
                    Pad CurrentPad = CurrentSelectedObject.GetComponent<Pad>();

                    Vector3 NewPosition = new Vector3(CurrentPad.Position.x, CurrentPad.Position.y + MouseY * GizmoSensitivity, CurrentPad.Position.z);
                    CurrentPad.ChangePosition(NewPosition);
                    Gizmo.transform.position = CurrentPad.transform.position;
                }
                else
                {
                    foreach (GameObject CurrentObject in SelectedObjects)
                    {
                        Pad CurrentPad = CurrentObject.GetComponent<Pad>();

                        Vector3 NewPosition = new Vector3(CurrentPad.Position.x, CurrentPad.Position.y + MouseY * GizmoSensitivity, CurrentPad.Position.z);
                        CurrentPad.ChangePosition(NewPosition);
                    }

                    Vector3 GizmoPosition = Gizmo.transform.position;
                    GizmoPosition.y += MouseY * GizmoSensitivity;
                    Gizmo.transform.position = GizmoPosition;
                }

                ObjectInspectorInstance.UpdatePositionFields();
            }
        }
        else if (IsDraggingGizmoZAxis)
        {
            float MouseX = Input.GetAxis("Mouse X");

            if (EditModeBar.CurrentMode == EditMode.Objects)
            {
                if (!MultipleObjectsSelected)
                {
                    Pad CurrentPad = CurrentSelectedObject.GetComponent<Pad>();

                    Vector3 NewPosition = new Vector3(CurrentPad.Position.x, CurrentPad.Position.y, CurrentPad.Position.z + MouseX * GizmoSensitivity);
                    CurrentPad.ChangePosition(NewPosition);
                    Gizmo.transform.position = CurrentPad.transform.position;
                }
                else
                {
                    foreach (GameObject CurrentObject in SelectedObjects)
                    {
                        Pad CurrentPad = CurrentObject.GetComponent<Pad>();

                        Vector3 NewPosition = new Vector3(CurrentPad.Position.x, CurrentPad.Position.y, CurrentPad.Position.z + MouseX * GizmoSensitivity);
                        CurrentPad.ChangePosition(NewPosition);
                    }

                    Vector3 GizmoPosition = Gizmo.transform.position;
                    GizmoPosition.z += MouseX * GizmoSensitivity;
                    Gizmo.transform.position = GizmoPosition;
                }

                ObjectInspectorInstance.UpdatePositionFields();
            }
        }
    }

    public void SelectNewObject(GameObject NewObject)
    {
        ClearSelectionOutline();

        List<GameObject> PreviousObjects = SelectedObjects;

        SelectedObjects.Clear();
        SelectedObjects.Add(NewObject);

        NewObject.layer = 10;

        if (EditModeBar.CurrentMode == EditMode.Objects)
        {
            Gizmo.SetActive(true);
        }

        Gizmo.transform.position = NewObject.transform.position;

        SelectionChanged?.Invoke(PreviousObjects);
    }

    public void SelectAdditionalObject(GameObject NewObject)
    {
        List<GameObject> PreviousObjects = SelectedObjects;

        SelectedObjects.Add(NewObject);

        NewObject.layer = 10;

        if (EditModeBar.CurrentMode == EditMode.Objects)
        {
            Gizmo.SetActive(true);
        }

        Vector3 GizmoPosition = new();

        foreach (GameObject CurrentObject in SelectedObjects)
        {
            GizmoPosition += CurrentObject.transform.position;
        }

        GizmoPosition.x /= SelectedObjects.Count;
        GizmoPosition.y /= SelectedObjects.Count;
        GizmoPosition.z /= SelectedObjects.Count;

        Gizmo.transform.position = GizmoPosition;

        SelectionChanged?.Invoke(PreviousObjects);
    }

    public void ClearSelectedObjects()
    {
        List<GameObject> PreviousObjects = SelectedObjects;

        ClearSelectionOutline();

        Gizmo.SetActive(false);

        if (DataHolder.RaycasterInstance.Mode == RaycastMode.WaypointLink)
        {
            DataHolder.RaycasterInstance.Mode = RaycastMode.Pad;
        }

        SelectedObjects.Clear();

        SelectionChanged?.Invoke(PreviousObjects);
    }

    private void ClearSelectionOutline()
    {
        foreach (GameObject CurrentObject in SelectedObjects)
        {
            switch (EditModeBar.CurrentMode)
            {
                case EditMode.Objects:
                    CurrentObject.layer = LayerMask.NameToLayer("Pad");
                    break;
                case EditMode.Rooms:
                    CurrentObject.layer = LayerMask.NameToLayer("Room");
                    break;
                case EditMode.Portals:
                    CurrentObject.layer = LayerMask.NameToLayer("Portal");
                    break;
                case EditMode.BackgroundLights:
                    CurrentObject.layer = LayerMask.NameToLayer("BackgroundLight");
                    break;
            }
        }
    }

    public void SetDragXAxis(bool IsDragging)
    {
        if (IsDragging)
        {
            GizmoXAxis.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.yellow);
            IsDraggingGizmoXAxis = true;
        }
        else
        {
            GizmoXAxis.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.red);
            IsDraggingGizmoXAxis = false;
        }
    }

    public void SetDragYAxis(bool IsDragging)
    {
        if (IsDragging)
        {
            GizmoYAxis.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.yellow);
            IsDraggingGizmoYAxis = true;
        }
        else
        {
            GizmoYAxis.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.green);
            IsDraggingGizmoYAxis = false;
        }
    }
    public void SetDragZAxis(bool IsDragging)
    {
        if (IsDragging)
        {
            GizmoZAxis.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.yellow);
            IsDraggingGizmoZAxis = true;
        }
        else
        {
            GizmoZAxis.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.blue);
            IsDraggingGizmoZAxis = false;
        }
    }

    public delegate void SelectionChange(List<GameObject> PreviousObjects);
}
