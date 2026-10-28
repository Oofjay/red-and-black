using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using static UnityEngine.Mesh;
using UnityEngine.Rendering;

public class HelperFunctions
{

    public static Mesh CreateMeshWithSubMeshes(List<Vector3> Vertices, List<Color> Colours, List<Vector2> UVs, List<int> TriangleIndices, List<int> IndexOffsets)
    {
        //Profiler.BeginSample("CreateMeshWithSubmeshes");
        // Make the mesh
        MeshDataArray DataArray = AllocateWritableMeshData(1);
        MeshData Data = DataArray[0];

        NativeArray<VertexAttributeDescriptor> VertexAttributes = new(3, Allocator.Temp); // Make an array of 3 attribute descriptors (vertices, colors, UVs).
        VertexAttributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position, dimension: 3, stream: 0);
        VertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Color, dimension: 4, stream: 1);
        VertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2, stream: 2);
        Data.SetVertexBufferParams(Vertices.Count, VertexAttributes);
        VertexAttributes.Dispose();

        NativeArray<Vector3> MeshVertices = Data.GetVertexData<Vector3>(0);
        NativeArray<Color> MeshColours = Data.GetVertexData<Color>(1);
        NativeArray<Vector2> MeshUVs = Data.GetVertexData<Vector2>(2);
        Data.SetIndexBufferParams(TriangleIndices.Count, IndexFormat.UInt32);
        NativeArray<int> MeshTriangleIndices = Data.GetIndexData<int>();

        // Convert from List to NativeArray garbage
        /*for (int i = 0; i < Vertices.Count; ++i)
        {
            MeshVertices[i] = Vertices[i];
            MeshColours[i] = Colours[i];
            MeshUVs[i] = UVs[i];
        }
        for (int i = 0; i < TriangleIndices.Count; ++i)
        {
            MeshTriangleIndices[i] = TriangleIndices[i];
        }*/

        MeshVertices.CopyFrom(Vertices.ToArray());
        MeshColours.CopyFrom(Colours.ToArray());
        MeshUVs.CopyFrom(UVs.ToArray());
        MeshTriangleIndices.CopyFrom(TriangleIndices.ToArray());

        int NumberOfSubmeshes = IndexOffsets.Count;
        Data.subMeshCount = NumberOfSubmeshes;
        for (int i = 0; i < NumberOfSubmeshes; ++i)
        {
            int IndexStart = IndexOffsets[i];
            int IndexCount;
            if (i == NumberOfSubmeshes - 1) // We're at the last submesh
            {
                IndexCount = TriangleIndices.Count - IndexStart; // The number of triangle indices in this submesh is the total amount of indices minus the current start index
            }
            else
            {
                IndexCount = IndexOffsets[i + 1] - IndexStart; // The number of triangle indices in this submesh is the next start index minus the current start index
            }
            SubMeshDescriptor SubMesh = new(IndexStart, IndexCount);
            //SubMesh.baseVertex = IndexStart;
            Data.SetSubMesh(i, SubMesh);
        }

        Mesh NewMesh = new();
        ApplyAndDisposeWritableMeshData(DataArray, NewMesh);
        NewMesh.RecalculateBounds();
        //Profiler.EndSample();
        return NewMesh;
    }

}
