using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoundedCube : MonoBehaviour
{

    public int xSize, ySize, zSize;
    public int roundness;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;


    private Vector3[] normals;

    void Awake()
    {
        Generate();
    }

    private void Generate()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.name = "Procedural Cube";

        CreateVertecies();
        CreateTriangles();
        CreateColliders();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
    }

    void OnDrawGizmos2()
    {
        if(vertices == null)
            return;

        for(int i = 0; i < vertices.Length; i++)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(vertices[i], 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(vertices[i], normals[i]);
        }
    }

    void CreateVertecies()
    {
        var verticesInCube = (xSize + 1) * (ySize + 1) * (zSize + 1);
        var vertcesInInnerCube = (xSize - 1) * (ySize - 1) * (zSize - 1);
        vertices = new Vector3[verticesInCube - vertcesInInnerCube];
        normals = new Vector3[vertices.Length];

        int v = 0;
        for(int y = 0; y <= ySize; y++)
        {
            for(int x = 0; x <= xSize; x++)
            {
                SetVertex(v++, x, y, 0);
            }
            for(int z = 1; z <= zSize; z++)
            {
                SetVertex(v++, xSize, y, z);
            }
            for(int x = xSize - 1; x >= 0; x--)
            {
                SetVertex(v++, x, y, zSize);
            }
            for(int z = zSize - 1; z >= 1; z--)
            {
                SetVertex(v++, 0, y, z);
            }
        }
        for(int z = 1; z < zSize; z++)
        {
            for(int x = 1; x < xSize; x++)
            {
                SetVertex(v++, x, ySize, z);
            }
        }
        for(int z = 1; z < zSize; z++)
        {
            for(int x = 1; x < xSize; x++)
            {
                SetVertex(v++, x, 0, z);
            }
        }
    }

    void CreateTriangles()
    {
        int quads = (xSize * ySize + zSize * xSize + zSize * ySize) * 2;
        triangles = new int[quads * 6];
        int ring = (xSize + zSize) * 2;
        int t = 0, v = 0;

        for(int y = 0; y < ySize; y++, v++)
        {
            for(int q = 0; q < ring - 1; q++, v++)
            {
                SetQuad(ref t, v, v + 1, v + ring, v + ring + 1);
            }
            SetQuad(ref t, v, v - ring + 1, v + ring, v + 1);
        }

        CreateTopFaces(ref t, ring);
        CreateBotFace(ref t, ring);
    }

    void CreateTopFaces(ref int t, int ring)
    {
        //First Edge
        int v = ring * ySize;
        for(int x = 0; x < xSize - 1; x++, v++)
        {
            SetQuad(ref t, v, v + 1, v + ring - 1, v + ring);
        }
        SetQuad(ref t, v, v + 1, v + ring - 1, v + 2);

        v = ring * (ySize + 1);
        var vLeftEdge = v - 1;
        var vRightEdge = ring * ySize + xSize + 1;

        for(int i = 1; i < zSize - 1; i++, vLeftEdge--, vRightEdge++, v++)
        {
            SetQuad(ref t, vLeftEdge, v, vLeftEdge - 1, v + xSize - 1);

            for(int j = 1; j < xSize - 1; j++, v++)
            {
                SetQuad(ref t, v, v + 1, v + xSize - 1, v + xSize);
            }

            SetQuad(ref t, v, vRightEdge, v + xSize - 1, vRightEdge + 1);
        }

        var vTop = vLeftEdge - 1;
        SetQuad(ref t, vLeftEdge, v, vTop, vTop - 1);
        vTop--;
        for(int i = 1; i < xSize - 1; i++, vTop--, v++)
        {
            SetQuad(ref t, v, v + 1, vTop, vTop - 1);
        }
        SetQuad(ref t, v, vRightEdge, vTop, vTop - 1);
    }

    void CreateBotFace(ref int t, int ring)
    {
        var vStart = ring * (ySize + 1) + (zSize - 1) * (xSize - 1);
        var v = vStart;
        SetQuad(ref t, ring - 1, v, 0, 1);
        for(int i = 1; i < xSize - 1; i++, v++)
        {
            SetQuad(ref t, v, v + 1, i, i + 1);
        }
        SetQuad(ref t, v, xSize + 1, xSize - 1, xSize);

        var vLeft = ring - 1;
        var vRight = xSize + 1;
        v = vStart;
        for(int i = 1; i < zSize - 1; i++, vLeft--, vRight++, v++)
        {
            SetQuad(ref t, vLeft - 1, v + xSize - 1, vLeft, v);
            for(int j = 1; j < xSize - 1; j++, v++)
            {
                SetQuad(ref t, v + xSize - 1, v + xSize, v, v + 1);
            }
            SetQuad(ref t, v + xSize - 1, vRight + 1, v, vRight);
        }

        var vBot = vLeft - 1;
        SetQuad(ref t, vBot, vBot - 1, vLeft, v);
        vBot--;
        for(int i = 1; i < xSize - 1; i++, v++, vBot--)
        {
            SetQuad(ref t, vBot, vBot - 1, v, v + 1);
        }
        SetQuad(ref t, vBot, vBot - 1, v, vBot - 2);
    }

    private void CreateColliders()
    {
        AddBoxCollider(xSize, ySize - roundness * 2, zSize - roundness * 2);
        AddBoxCollider(xSize - roundness * 2, ySize, zSize - roundness * 2);
        AddBoxCollider(xSize - roundness * 2, ySize - roundness * 2, zSize);

        Vector3 min = Vector3.one * roundness;
        Vector3 half = new Vector3(xSize, ySize, zSize) * 0.5f; 
        Vector3 max = new Vector3(xSize, ySize, zSize) - min;

        AddCapsuleCollider(0, half.x, min.y, min.z);
        AddCapsuleCollider(0, half.x, min.y, max.z);
        AddCapsuleCollider(0, half.x, max.y, min.z);
        AddCapsuleCollider(0, half.x, max.y, max.z);

        AddCapsuleCollider(1, min.x, half.y, min.z);
        AddCapsuleCollider(1, min.x, half.y, max.z);
        AddCapsuleCollider(1, max.x, half.y, min.z);
        AddCapsuleCollider(1, max.x, half.y, max.z);

        AddCapsuleCollider(2, min.x, min.y, half.z);
        AddCapsuleCollider(2, min.x, max.y, half.z);
        AddCapsuleCollider(2, max.x, min.y, half.z);
        AddCapsuleCollider(2, max.x, max.y, half.z);
    }

    private void AddCapsuleCollider(int direction, float x, float y, float z)
    {
        CapsuleCollider c = gameObject.AddComponent<CapsuleCollider>();
        c.center = new Vector3(x, y, z);
        c.direction = direction;
        c.radius = roundness;
        c.height = c.center[direction] * 2f;
    }

    private void AddBoxCollider(float x, float y, float z)
    {
        BoxCollider c = gameObject.AddComponent<BoxCollider>();
        Vector3 center = new Vector3();
        center.x = (float)(xSize) / 2;
        center.y = (float)(ySize) / 2;
        center.z = (float)(zSize) / 2;
        c.center = center;
        c.size = new Vector3(x, y, z);
    }

    private void SetVertex(int i, int x, int y, int z)
    {
        Vector3 inner = new Vector3(x, y, z);
        vertices[i] = new Vector3(x, y, z);

        if(x < roundness)
        {
            inner.x = roundness;
        }
        else if(x > xSize - roundness)
        {
            inner.x = xSize - roundness;
        }

        if(y < roundness)
        {
            inner.y = roundness;
        }
        else if(y > ySize - roundness)
        {
            inner.y = ySize - roundness;
        }

        if(z < roundness)
        {
            inner.z = roundness;
        }
        else if(z > zSize - roundness)
        {
            inner.z = zSize - roundness;
        }

        normals[i] = (vertices[i] - inner).normalized;
        vertices[i] = inner + normals[i] * roundness;
    }

    private void SetQuad(ref int i, int v00, int v10, int v01, int v11)
    {
        triangles[i] = v00;
        triangles[i + 1] = v01;
        triangles[i + 2] = v10;

        triangles[i + 3] = v10;
        triangles[i + 4] = v01;
        triangles[i + 5] = v11;

        i += 6;
    }
}
