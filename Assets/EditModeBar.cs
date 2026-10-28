using UnityEngine;
using UnityEngine.UI;

public enum EditMode
{
    None,
    Objects,
    Rooms,
    Portals,
    Collision,
    BackgroundLights
};

public class EditModeBar : MonoBehaviour
{
    public static EditMode CurrentMode = EditMode.None;
    System.Type SelectableType;

    Image ObjectModeButton;
    Image RoomModeButton;
    Image PortalModeButton;
    Image CollisionModeButton;
    Image BackgroundLightModeButton;

    // Start is called before the first frame update
    void Start()
    {
        ObjectModeButton = transform.Find("EditModeButtons/ObjectMode").GetComponent<Image>();
        RoomModeButton = transform.Find("EditModeButtons/RoomMode").GetComponent<Image>();
        PortalModeButton = transform.Find("EditModeButtons/PortalMode").GetComponent<Image>();
        CollisionModeButton = transform.Find("EditModeButtons/CollisionMode").GetComponent<Image>();
        BackgroundLightModeButton = transform.Find("EditModeButtons/BackgroundLightMode").GetComponent<Image>();

        CurrentMode = EditMode.Objects;
        SelectableType = typeof(Pad);

        ObjectModeButton.color = new Color32(215, 130, 4, 255);

        DataHolder.RaycasterInstance.Mode = RaycastMode.Pad;
    }

    private void DeactivateObjects(EditMode NewMode)
    {
        if (CurrentMode == EditMode.Portals)
        {
            foreach (Portal CurrentPortal in DataHolder.Portals)
            {
                CurrentPortal.gameObject.SetActive(false);
            }
        }
        else if (CurrentMode == EditMode.Collision)
        {
            foreach (Collision CurrentCollision in DataHolder.Collisions)
            {
                CurrentCollision.gameObject.SetActive(false);
            }
        }
        else if (CurrentMode == EditMode.BackgroundLights)
        {
            foreach (BackgroundLight CurrentLight in DataHolder.BackgroundLights)
            {
                CurrentLight.gameObject.SetActive(false);
            }
        }

        if (NewMode == EditMode.Portals)
        {
            foreach (Portal CurrentPortal in DataHolder.Portals)
            {
                CurrentPortal.gameObject.SetActive(true);
            }
        }
        else if (NewMode == EditMode.Collision)
        {
            foreach (Collision CurrentCollision in DataHolder.Collisions)
            {
                CurrentCollision.gameObject.SetActive(true);
            }
        }
        else if (NewMode == EditMode.BackgroundLights)
        {
            foreach (BackgroundLight CurrentLight in DataHolder.BackgroundLights)
            {
                CurrentLight.gameObject.SetActive(true);
            }
        }
    }

    public void EnterObjectMode()
    {
        if (CurrentMode == EditMode.Objects)
        {
            return;
        }

        PrepareForModeSwitch();

        DeactivateObjects(EditMode.Objects);

        CurrentMode = EditMode.Objects;
        SelectableType = typeof(Pad);

        ObjectModeButton.color = new Color32(215, 130, 4, 255);

        DataHolder.RaycasterInstance.Mode = RaycastMode.Pad;
    }

    public void EnterRoomMode()
    {
        if (CurrentMode == EditMode.Rooms)
        {
            return;
        }

        PrepareForModeSwitch();

        DeactivateObjects(EditMode.Rooms);

        CurrentMode = EditMode.Rooms;
        SelectableType = typeof(Room);

        RoomModeButton.color = new Color32(215, 130, 4, 255);

        DataHolder.RaycasterInstance.Mode = RaycastMode.Room;
    }

    public void EnterPortalMode()
    {
        if (CurrentMode == EditMode.Portals)
        {
            return;
        }

        PrepareForModeSwitch();

        DeactivateObjects(EditMode.Portals);

        CurrentMode = EditMode.Portals;
        SelectableType = typeof(Portal);

        PortalModeButton.color = new Color32(215, 130, 4, 255);

        DataHolder.RaycasterInstance.Mode = RaycastMode.Portal;
    }

    public void EnterCollisionMode()
    {
        if (CurrentMode == EditMode.Collision)
        {
            return;
        }

        PrepareForModeSwitch();

        DeactivateObjects(EditMode.Collision);

        CurrentMode = EditMode.Collision;
        SelectableType = typeof(Collision);

        CollisionModeButton.color = new Color32(215, 130, 4, 255);

        DataHolder.RaycasterInstance.Mode = RaycastMode.Collision;
    }

    public void EnterBackgroundLightMode()
    {
        if (CurrentMode == EditMode.BackgroundLights)
        {
            return;
        }

        PrepareForModeSwitch();

        DeactivateObjects(EditMode.BackgroundLights);

        CurrentMode = EditMode.BackgroundLights;
        SelectableType = typeof(BackgroundLight);

        BackgroundLightModeButton.color = new Color32(215, 130, 4, 255);

        DataHolder.RaycasterInstance.Mode = RaycastMode.BackgroundLight;
    }

    private void PrepareForModeSwitch()
    {
        DataHolder.SelectionManagerInstance.ClearSelectedObjects();

        ResetButtonColours();

        DataHolder.ObjectInspectorInstance.Clear();
    }

    protected void ResetButtonColours()
    {
        ObjectModeButton.color = new Color32(81, 81, 81, 255);
        RoomModeButton.color = new Color32(81, 81, 81, 255);
        PortalModeButton.color = new Color32(81, 81, 81, 255);
        CollisionModeButton.color = new Color32(81, 81, 81, 255);
        BackgroundLightModeButton.color = new Color32(81, 81, 81, 255);
    }
}
