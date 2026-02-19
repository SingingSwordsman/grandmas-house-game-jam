using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class SmoothNormalsBaker : MonoBehaviour
{
    void Awake()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        BakeSmoothNormals(mesh);
    }

    void BakeSmoothNormals(Mesh mesh)
    {
        // Group vertices by position and average their normals
        var smoothNormalsMap = new Dictionary<Vector3, Vector3>();

        for (int i = 0; i < mesh.vertexCount; i++)
        {
            Vector3 pos = mesh.vertices[i];
            if (!smoothNormalsMap.ContainsKey(pos))
                smoothNormalsMap[pos] = Vector3.zero;
            smoothNormalsMap[pos] += mesh.normals[i];
        }

        // Normalize the averaged normals
        var keys = new List<Vector3>(smoothNormalsMap.Keys);
        foreach (var key in keys)
            smoothNormalsMap[key] = smoothNormalsMap[key].normalized;

        // Write into UV1
        var smoothNormals = new Vector3[mesh.vertexCount];
        for (int i = 0; i < mesh.vertexCount; i++)
            smoothNormals[i] = smoothNormalsMap[mesh.vertices[i]];

        mesh.SetUVs(1, smoothNormals);
    }
}