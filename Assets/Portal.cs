using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Portal : MonoBehaviour
{
    public int ID;
    public string NumberString => ID.ToString("X3");
    public Room FirstRoom;
    public Room SecondRoom;

    public string MeshName;

    public void Init(PortalData InData)
    {
        ID = InData.ID;
        name = "Portal" + NumberString;

        FirstRoom = DataHolder.Rooms[InData.FirstRoom - 1];
        SecondRoom = DataHolder.Rooms[InData.SecondRoom - 1];

        Mesh PortalMesh = InData.PortalMesh;

        MeshFilter Filter = GetComponent<MeshFilter>();
        MeshRenderer Renderer = GetComponent<MeshRenderer>();
        Filter.mesh = PortalMesh;
        Renderer.material = new Material(DataHolder.PortalMaterial);
        GetComponent<MeshCollider>().sharedMesh = PortalMesh;

        Plane NormalPlane = new(Filter.mesh.vertices[0], Filter.mesh.vertices[1], Filter.mesh.vertices[2]);
        Bounds PortalBounds = Renderer.bounds;

        LineRenderer Line = GetComponent<LineRenderer>();
        Line.positionCount = 2;

        Vector3[] LineVertices = new Vector3[2] { PortalBounds.center, PortalBounds.center + NormalPlane.normal * 70 };
        Line.SetPositions(LineVertices);

        AnimationCurve Curve = new();
        Curve.AddKey(0f, 1f);
        Curve.AddKey(0.75f, 0.75f);
        Curve.AddKey(1f, 0f);

        Line.widthCurve = Curve;
    }
}
