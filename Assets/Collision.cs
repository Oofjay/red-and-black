using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public int ID;

    public int MinTriangleIndex;
    public int MaxTriangleIndex;

    public bool Floor1;
    public bool Floor2;
    public bool Wall;
    public bool BlockSight;
    public bool BlockGunfire;
    public bool LiftFloor;
    public bool Ladder;
    public bool RampWall;
    public bool Slope;
    public bool Underwater;
    public bool Flag0400;
    public bool AICrouches;
    public bool AIDucks;
    public bool AlwaysAscend;
    public bool DieOnTouch;
    public bool PlayerOnlyLadder;

    public int FloorType;

    public Color32 FloorColour;
    public Color32 TypeColour;
}

public class Collision : MonoBehaviour
{
    public Room CollisionRoom;
    private List<Tile> Tiles = new();

    private readonly Dictionary<string, int> FloorTypes = new()
    {
        { "default", 0 },
        { "wood", 1 },
        { "stone", 2 },
        { "carpet", 3 },
        { "metal", 4 },
        { "mud", 5 },
        { "water", 6 },
        { "dirt", 7 },
        { "snow", 8 },
    };

    public void Init(List<TileProperties> TilePropertyList, Room InRoom)
    {
        name = "Collision" + InRoom.NumberString;

        CollisionRoom = InRoom;

        if (TilePropertyList.Count == 0)
        {
            return;
        }

        List<Vector3> Vertices = new();
        List<Color32> Colours = new();
        List<int> TriangleIndices = new();

        int TileIndex = 0;

        foreach (TileProperties CurrentProperties in TilePropertyList)
        {
            byte Red = (byte)((CurrentProperties.floorcolour >> 8 & 0x00F) * 17);
            byte Green = (byte)((CurrentProperties.floorcolour >> 4 & 0x00F) * 17);
            byte Blue = (byte)((CurrentProperties.floorcolour & 0x00F) * 17);
            Color32 FloorColour = new Color32(Red, Green, Blue, 255);

            int TriangleCount = CurrentProperties.vertices.Count - 2;

            Tiles.Add(new()
            {
                ID = TileIndex,
                MinTriangleIndex = TriangleIndices.Count,
                MaxTriangleIndex = TriangleIndices.Count + TriangleCount * 3 - 1,
                Floor1 = CurrentProperties.flag0001,
                Floor2 = CurrentProperties.flag0002,
                Wall = CurrentProperties.flag0004,
                BlockSight = CurrentProperties.flag0008,
                BlockGunfire = CurrentProperties.flag0010,
                LiftFloor = CurrentProperties.flag0020,
                Ladder = CurrentProperties.ladder,
                RampWall = CurrentProperties.flag0080,
                Slope = CurrentProperties.flag0100,
                Underwater = CurrentProperties.underwater,
                Flag0400 = CurrentProperties.flag0400,
                AICrouches = CurrentProperties.aibotcrouch,
                AIDucks = CurrentProperties.aibotduck,
                AlwaysAscend = CurrentProperties.flag2000,
                DieOnTouch = CurrentProperties.die,
                PlayerOnlyLadder = CurrentProperties.climbableledge,

                FloorType = FloorTypes[CurrentProperties.floortype],

                FloorColour = FloorColour
            });

            int VertexOffset = Vertices.Count;

            foreach (Vector3 CurrentVertex in CurrentProperties.vertices)
            {
                Vertices.Add(new Vector3(CurrentVertex.x, CurrentVertex.y, -CurrentVertex.z));
                Colours.Add(FloorColour);
            }

            for (int i = 0; i < TriangleCount; ++i)
            {
                TriangleIndices.Add(2 + i + VertexOffset);
                TriangleIndices.Add(1 + i + VertexOffset);
                TriangleIndices.Add(0 + VertexOffset);
            }

            ++TileIndex;
        }

        Mesh NewMesh = new();
        NewMesh.vertices = Vertices.ToArray();
        NewMesh.triangles = TriangleIndices.ToArray();
        NewMesh.colors32 = Colours.ToArray();

        GetComponent<MeshFilter>().mesh = NewMesh;
        GetComponent<MeshCollider>().sharedMesh = NewMesh;
        GetComponent<MeshRenderer>().material = DataHolder.CollisionMaterial;
    }

    public void Init(CollisionData InData)
    {
        CollisionRoom = InData.CollisionRoom;
        Tiles = InData.Tiles;

        name = "Collision" + CollisionRoom.NumberString;

        GetComponent<MeshFilter>().mesh = InData.CollisionMesh;
        GetComponent<MeshCollider>().sharedMesh = InData.CollisionMesh;
        GetComponent<MeshRenderer>().material = DataHolder.CollisionMaterial;
    }

    public Tile GetTile(int TriangleIndex)
    {
        foreach (Tile CurrentTile in Tiles)
        {
            if (TriangleIndex >= CurrentTile.MinTriangleIndex && TriangleIndex <= CurrentTile.MaxTriangleIndex)
            {
                return CurrentTile;
            }
        }

        Debug.LogError($"Tile index {TriangleIndex} is out of range for room {CollisionRoom.ID}");
        return null;
    }

    public void SelectTile(Tile InTile)
    {
        Mesh CollisionMesh = GetComponent<MeshFilter>().mesh;

        int[] TriangleIndices = CollisionMesh.triangles[InTile.MinTriangleIndex..(InTile.MaxTriangleIndex + 1)];
        Color32[] Colours = CollisionMesh.colors32;

        foreach (int CurrentIndex in TriangleIndices)
        {
            Colours[CurrentIndex] = new Color32(245, 194, 66, 255);
        }

        CollisionMesh.colors32 = Colours;
        GetComponent<MeshFilter>().mesh = CollisionMesh;
    }

    public void UnselectTile(Tile InTile)
    {
        Mesh CollisionMesh = GetComponent<MeshFilter>().mesh;

        int[] TriangleIndices = CollisionMesh.triangles[InTile.MinTriangleIndex..(InTile.MaxTriangleIndex + 1)];
        Color32[] Colours = CollisionMesh.colors32;

        foreach (int CurrentIndex in TriangleIndices)
        {
            Colours[CurrentIndex] = InTile.FloorColour;
        }

        CollisionMesh.colors32 = Colours;
        GetComponent<MeshFilter>().mesh = CollisionMesh;
    }
}
