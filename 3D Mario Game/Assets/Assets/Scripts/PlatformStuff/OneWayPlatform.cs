using UnityEngine;
using System.Collections.Generic;


//NOT MY CODE
//USED FROM STACKOVERFLOW POST: https://answers.unity.com/questions/236313/one-way-collider.html

[RequireComponent(typeof(MeshCollider))]
public class OneWayPlatform : MonoBehaviour                   
{
    public bool topCollision = true;
    public float maxAngle = 45.0f;

    void Start()
    {
        float cos = Mathf.Cos(maxAngle);
        MeshCollider MC = GetComponent<MeshCollider>();
        if (MC == null)
        {
            Debug.LogError("PlatformCollision needs a MeshCollider");
            return;
        }
        Mesh M = new Mesh();
        Vector3[] verts = MC.sharedMesh.vertices;
        List<int> triangles = new List<int>(MC.sharedMesh.triangles);
        for (int i = triangles.Count - 1; i >= 0; i -= 3)
        {
            Vector3 P1 = transform.TransformPoint(verts[triangles[i - 2]]);
            Vector3 P2 = transform.TransformPoint(verts[triangles[i - 1]]);
            Vector3 P3 = transform.TransformPoint(verts[triangles[i]]);
            Vector3 faceNormal = Vector3.Cross(P3 - P2, P1 - P2).normalized;
            if ((topCollision && Vector3.Dot(faceNormal, Vector3.up) <= cos) ||
                 (!topCollision && Vector3.Dot(faceNormal, -Vector3.up) <= cos))
            {
                triangles.RemoveAt(i);
                triangles.RemoveAt(i - 1);
                triangles.RemoveAt(i - 2);
            }
        }
        M.vertices = verts;
        M.triangles = triangles.ToArray();
        MC.sharedMesh = M;
    }
}