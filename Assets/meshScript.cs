using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Collections;
using System.IO;
using System.Text;

public class meshScript : MonoBehaviour
{

    void Start()
    {
        // programatically create meshfilter and meshrenderer and add to gameobject this script is attached to.
        GameObject go = gameObject; // GameObject.Find("GameObjectDp");
        MeshFilter meshFilter = (MeshFilter)go.AddComponent(typeof(MeshFilter));
        MeshRenderer renderer = go.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
    }
 
    public void createMeshGeometry(List<Vector3> vertices, List<int> indices)
    {
        // Mesh mesh = GetComponent<MeshFilter>().mesh;
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        // mesh.Clear();
        mesh.SetVertices(vertices);
     
        // https://docs.unity3d.com/ScriptReference/MeshTopology.html
        mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);   // MeshTopology.Points  MeshTopology.LineStrip   MeshTopology.Lines 
        mesh.RecalculateBounds();

    }

   
}