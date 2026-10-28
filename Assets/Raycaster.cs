using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum RaycastMode
{
    None,
    Pad,
    Room,
    Portal,
    Collision,
    BackgroundLight,
    WaypointLink
};

public class Raycaster : MonoBehaviour
{
    Camera CameraObject;
    EditorCamera EditorCameraInstance;

    public RaycastMode Mode;

    SelectionManager SelectionManagerInstance;
    public ContextMenu ContextMenuInstance;

    private void Start()
    {
        DataHolder.RaycasterInstance = this;

        CameraObject = GetComponent<Camera>();
        EditorCameraInstance = GetComponent<EditorCamera>();
        Mode = RaycastMode.Pad;

        SelectionManagerInstance = DataHolder.SelectionManagerInstance;
    }

    // Update is called once per frame
    void Update()
    {
        Ray InputRay = CameraObject.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonUp(0))
        {
            if (SelectionManagerInstance.IsDraggingGizmoXAxis)
            {
                SelectionManagerInstance.SetDragXAxis(false);
            }
            else if (SelectionManagerInstance.IsDraggingGizmoYAxis)
            {
                SelectionManagerInstance.SetDragYAxis(false);
            }
            else if (SelectionManagerInstance.IsDraggingGizmoZAxis)
            {
                SelectionManagerInstance.SetDragZAxis(false);
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (Mode == RaycastMode.None)
            {
                return;
            }

            /*PointerEventData EventData = new(EventSystem.current) { position = Input.mousePosition };
            List<RaycastResult> RaycastResults = new();
            EventSystem.current.RaycastAll(EventData, RaycastResults);

            foreach (RaycastResult Result in RaycastResults)
            {
                if (Result.gameObject.name == "SceneWindow")
                {
                    continue;
                }
                else // Found a UI element, so we don't raycast, because it's supposed to be blocking it.
                {
                    return;
                }
            }*/

            if (EventSystem.current.IsPointerOverGameObject()) // Is the mouse over a UI element?
            {
                return;
            }

            string ObjectLayerMask = string.Empty;

            switch (Mode)
            {
                case RaycastMode.Pad:
                    ObjectLayerMask = "Pad";
                    break;
                case RaycastMode.Room:
                    ObjectLayerMask = "Room";
                    break;
                case RaycastMode.Portal:
                    ObjectLayerMask = "Portal";
                    break;
                case RaycastMode.BackgroundLight:
                    ObjectLayerMask = "BackgroundLight";
                    break;
            }

            if (Mode == RaycastMode.WaypointLink)
            {
                if (Physics.Raycast(InputRay, out RaycastHit Hit, float.PositiveInfinity) && Hit.transform.TryGetComponent(out Pad HitPad) && HitPad.PadWaypoint != null)
                {
                    DataHolder.ObjectInspectorInstance.LinkWaypoint(HitPad);
                    Mode = RaycastMode.Pad;
                }
                else
                {
                    Debug.Log("Found no waypoint");
                }
            }
            else if (Mode == RaycastMode.Collision)
            {
                if (Physics.Raycast(InputRay, out RaycastHit Hit, float.PositiveInfinity, LayerMask.GetMask("Collision")) /* && EventSystem.current.IsPointerOverGameObject() == false */
                    && Hit.transform.TryGetComponent(out Collision HitCollision))
                {
                    Tile HitTile = HitCollision.GetTile(Hit.triangleIndex * 3);
                    DataHolder.ObjectInspectorInstance.EnterCollision(HitCollision, HitTile);
                }
                else
                {
                    DataHolder.ObjectInspectorInstance.Clear();
                }
            }
            else // Regular selectable object
            {
                if (Physics.Raycast(InputRay, out RaycastHit GizmoHit, float.PositiveInfinity, LayerMask.GetMask("Gizmo")))
                {
                    if (GizmoHit.transform.gameObject == SelectionManagerInstance.GizmoXAxis)
                    {
                        SelectionManagerInstance.SetDragXAxis(true);
                        return;
                    }
                    else if (GizmoHit.transform.gameObject == SelectionManagerInstance.GizmoYAxis)
                    {
                        SelectionManagerInstance.SetDragYAxis(true);
                        return;
                    }
                    else if (GizmoHit.transform.gameObject == SelectionManagerInstance.GizmoZAxis)
                    {
                        SelectionManagerInstance.SetDragZAxis(true);
                        return;
                    }
                }

                if (Physics.Raycast(InputRay, out RaycastHit Hit, float.PositiveInfinity, LayerMask.GetMask(ObjectLayerMask, "Selected")))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        if (Hit.transform.gameObject.layer == LayerMask.NameToLayer("Selected"))
                        {
                            return;
                        }
                        SelectionManagerInstance.SelectAdditionalObject(Hit.transform.gameObject);
                    }
                    else
                    {
                        SelectionManagerInstance.SelectNewObject(Hit.transform.gameObject);
                    }
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        return;
                    }
                    else
                    {
                        SelectionManagerInstance.ClearSelectedObjects();
                        DataHolder.ObjectInspectorInstance.Clear();
                    }
                }
            }
        }
        else if (Input.GetMouseButtonUp(1))
        {
            if (Mode == RaycastMode.None || EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            
            string ObjectLayerMask = string.Empty;

            switch (Mode)
            {
                case RaycastMode.Pad:
                    ObjectLayerMask = "Pad";
                    break;
                case RaycastMode.Room:
                    ObjectLayerMask = "Room";
                    break;
                case RaycastMode.Portal:
                    ObjectLayerMask = "Portal";
                    break;
                case RaycastMode.BackgroundLight:
                    ObjectLayerMask = "BackgroundLight";
                    break;
            }

            if (Mode == RaycastMode.Collision)
            {
                if (Physics.Raycast(InputRay, out RaycastHit Hit, float.PositiveInfinity, LayerMask.GetMask("Collision")) /* && EventSystem.current.IsPointerOverGameObject() == false */
                    && Hit.transform.TryGetComponent(out Collision HitCollision))
                {
                    Tile HitTile = HitCollision.GetTile(Hit.triangleIndex * 3);
                    DataHolder.ObjectInspectorInstance.EnterCollision(HitCollision, HitTile);
                }
                else
                {
                    DataHolder.ObjectInspectorInstance.Clear();
                }
            }
            else // Regular selectable object
            {
                if (EditorCameraInstance.IsPanning)
                {
                    return;
                }

                if (Physics.Raycast(InputRay, out RaycastHit Hit, float.PositiveInfinity, LayerMask.GetMask(ObjectLayerMask, "Selected")))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        if (Hit.transform.gameObject.layer == LayerMask.NameToLayer("Selected"))
                        {
                            return;
                        }
                        SelectionManagerInstance.SelectAdditionalObject(Hit.transform.gameObject);
                    }
                    else
                    {
                        SelectionManagerInstance.SelectNewObject(Hit.transform.gameObject);
                    }
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        return;
                    }
                    else
                    {
                        SelectionManagerInstance.ClearSelectedObjects();
                        DataHolder.ObjectInspectorInstance.Clear();
                    }
                }

                if (Physics.Raycast(InputRay, out RaycastHit ContextMenuHit, float.PositiveInfinity, LayerMask.GetMask(ObjectLayerMask, "Room", "Selected")))
                {
                    ContextMenuInstance.Display(Input.mousePosition, ContextMenuHit.point);
                }
            }
        }
    }
}
