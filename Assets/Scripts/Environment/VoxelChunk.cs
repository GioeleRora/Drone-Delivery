using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class VoxelChunk : MonoBehaviour
{
    public ComputeShader marchingCubesShader;
    
    [Header("Chunk Settings")]
    public int numPointsPerAxis = 32;
    public float boundsSize = 32f;
    public float isoLevel = 0f;

    struct Triangle
    {
        public Vector3 vertexC;
        public Vector3 vertexB;
        public Vector3 vertexA;
    }

    private ComputeBuffer triangleBuffer;
    private ComputeBuffer pointsBuffer;
    private ComputeBuffer triCountBuffer;

    public void GenerateMesh(float[] densityMap)
    {
        int numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxels * 5;

        // Inizializza i buffer per la GPU
        if (triangleBuffer == null || triangleBuffer.count != maxTriangleCount)
        {
            ReleaseBuffers();
            triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
            pointsBuffer = new ComputeBuffer(numPoints, sizeof(float) * 4);
            triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        }

        // Creiamo l'array di punti (xyz = posizione locale, w = densità)
        Vector4[] points = new Vector4[numPoints];
        float pointSpacing = boundsSize / (numPointsPerAxis - 1);
        
        for (int z = 0; z < numPointsPerAxis; z++)
        {
            for (int y = 0; y < numPointsPerAxis; y++)
            {
                for (int x = 0; x < numPointsPerAxis; x++)
                {
                    int idx = x + numPointsPerAxis * (y + numPointsPerAxis * z);
                    Vector3 pos = new Vector3(x, y, z) * pointSpacing - (Vector3.one * boundsSize / 2f);
                    points[idx] = new Vector4(pos.x, pos.y, pos.z, densityMap[idx]);
                }
            }
        }

        pointsBuffer.SetData(points);
        triangleBuffer.SetCounterValue(0);

        int kernel = marchingCubesShader.FindKernel("March");
        marchingCubesShader.SetBuffer(kernel, "points", pointsBuffer);
        marchingCubesShader.SetBuffer(kernel, "triangles", triangleBuffer);
        marchingCubesShader.SetInt("numPointsPerAxis", numPointsPerAxis);
        marchingCubesShader.SetFloat("isoLevel", isoLevel);

        int numThreads = 8;
        int threadGroups = Mathf.CeilToInt(numPointsPerAxis / (float)numThreads);
        marchingCubesShader.Dispatch(kernel, threadGroups, threadGroups, threadGroups);

        // Recupera il numero di triangoli generati
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];

        // Recupera i triangoli dalla GPU
        Triangle[] tris = new Triangle[numTris];
        triangleBuffer.GetData(tris, 0, 0, numTris);

        // Costruisci la Mesh di Unity
        Vector3[] vertices = new Vector3[numTris * 3];
        int[] meshTriangles = new int[numTris * 3];

        for (int i = 0; i < numTris; i++)
        {
            // Invertiamo l'ordine (A, C, B) per avere le normali rivolte verso l'esterno
            vertices[i * 3 + 0] = tris[i].vertexA;
            vertices[i * 3 + 1] = tris[i].vertexC;
            vertices[i * 3 + 2] = tris[i].vertexB;

            meshTriangles[i * 3 + 0] = i * 3 + 0;
            meshTriangles[i * 3 + 1] = i * 3 + 1;
            meshTriangles[i * 3 + 2] = i * 3 + 2;
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    private void OnDestroy()
    {
        ReleaseBuffers();
    }

    private void ReleaseBuffers()
    {
        if (triangleBuffer != null) triangleBuffer.Release();
        if (pointsBuffer != null) pointsBuffer.Release();
        if (triCountBuffer != null) triCountBuffer.Release();
    }
}
