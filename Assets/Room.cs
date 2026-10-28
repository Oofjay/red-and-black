using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Room : MonoBehaviour
{
    public int ID { get; set; }
    public string NumberString => (ID).ToString("X3");
    public int MinimumBrightness { get; set; }
    public int MaximumBrightness { get; set; }
    public List<ObjectiveCondition> ConnectedObjectiveConditions { get; set; } = new();

    public void Init(RoomData InData)
    {
        ID = InData.ID;
        name = "Room" + NumberString;
        MinimumBrightness = InData.MinimumBrightness;
        MaximumBrightness = InData.MaximumBrightness;

        Mesh RoomMesh = InData.RoomMesh;
        GetComponent<MeshFilter>().mesh = RoomMesh;
        GetComponent<MeshCollider>().sharedMesh = RoomMesh;

        Material[] MeshMaterials = new Material[RoomMesh.subMeshCount];
        for (int i = 0; i < RoomMesh.subMeshCount; ++i)
        {
            Material NewMaterial = DataHolder.Materials[InData.TextureIDs[i]];
            MeshMaterials[i] = NewMaterial;
        }
        GetComponent<MeshRenderer>().materials = MeshMaterials;
    }
}
