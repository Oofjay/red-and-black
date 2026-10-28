using UnityEngine;

public class BackgroundLight : MonoBehaviour
{
    public int ID;
    public virtual string NumberString => ID.ToString("X4"); // Same as Number, but converted to hexadecimal and string.

    public Room LightRoom;
    public Color32 Colour;
    public int Size;
    public int RingSize;
    public Vector3 Direction;

    LineRenderer Line;
    Bounds PortalBounds;

    public void Init(LightData InData)
    {
        ID = InData.ID;
        name = "BackgroundLight" + NumberString;

        LightRoom = DataHolder.Rooms[InData.Room - 1];
        Colour = InData.Colour;
        Size = InData.Size;
        RingSize = InData.RingSize;
        Direction = InData.Direction;

        MeshFilter Filter = GetComponent<MeshFilter>();
        MeshRenderer Renderer = GetComponent<MeshRenderer>();
        Filter.mesh = InData.LightMesh;
        Renderer.material = new Material(DataHolder.PortalMaterial) { color = Colour };
        GetComponent<MeshCollider>().sharedMesh = InData.LightMesh;

        PortalBounds = Renderer.bounds;

        Line = GetComponent<LineRenderer>();
        Line.positionCount = 2;

        Vector3[] LineVertices = new Vector3[2] { PortalBounds.center, PortalBounds.center + InData.Direction * 70 };
        Line.SetPositions(LineVertices);

        AnimationCurve Curve = new();
        Curve.AddKey(0f, 1f);
        Curve.AddKey(0.75f, 0.75f);
        Curve.AddKey(1f, 0f);

        Line.widthCurve = Curve;
    }

    public void ChangeNormal(Vector3 InDirection)
    {
        Direction = InDirection;

        Vector3[] LineVertices = new Vector3[2] { PortalBounds.center, PortalBounds.center + InDirection * 70 };
        Line.SetPositions(LineVertices);
    }

    public void ChangeColour(Color InColour)
    {
        Colour = InColour;
        MeshRenderer Renderer = GetComponent<MeshRenderer>();
        Renderer.material.color = Colour;
    }
}
