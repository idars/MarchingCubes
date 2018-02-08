using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;

public class quadScript : MonoBehaviour
{


    // member variables of quadScript, accessible from any function
    static int xdim = 100;
    static int ydim = 100;
    static int zdim = 100;
    float isolevel = 0f; // [0, 1]
    float[,] heightmap = new float[xdim, ydim]; // Twodimensional array containing pixelval for all pixels
    List<Vector3> vertices;
    List<int> indices;
    int i;


    // Start is called once when the application is run
    void Start()
    {
        print("void Start was called");
        setSlice(50);                     // shows a slice

        //  gets the mesh object and uses it to create a diagonal line
        meshScript mscript = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        vertices.Add(new Vector3(-0.5f, -0.5f, 0));
        vertices.Add(new Vector3(0.5f, 0.5f, 0));
        indices.Add(0);
        indices.Add(1);
        mscript.createMeshGeometry(vertices, indices);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void setSlice(int z)
    {
        var texture = new Texture2D(xdim, ydim, TextureFormat.RGB24, false);     // garbage collector will tackle that it is new'ed 

        for (int y = 0; y < ydim; y++)
        {
            for (int x = 0; x < xdim; x++)
            {
                float v = pixelval(new Vector3(x, y, z));
                heightmap[x, y] = v;
                texture.SetPixel(x, y, new UnityEngine.Color(v, v, v));
            }
        }

        texture.filterMode = FilterMode.Point;  // nearest neigbor interpolation is used.  (alternative is FilterMode.Bilinear)
        texture.Apply();  // Apply all SetPixel calls
        GetComponent<Renderer>().material.mainTexture = texture;
    }

    float pixelval(Vector3 p)
    {
        /* For a sphere
        // Returns a value between 0 and 1 based on the distance from the vector to the center of the sphere.
        double x = p.x - 50;
        double y = p.y - 50;
        double z = p.z - 50;
        return (float)Math.Sqrt(x * x + y * y + z * z) / 50;
        */
        if (p.x > 25 && p.x < 75)
        {
            if (p.y > 25 && p.y < 75)
            {
                if (p.z > 25 && p.z < 75)
                {
                    return 0;
                }
            }
        }
        return 1;

    }

    public void slicePosSliderChange(float val)
    {
        print("slicePosSliderChange:" + val);

        // Get value relative to the number of slices
        float depth = val * zdim;

        // Cast to integer in order to view specific slice
        setSlice((int)depth);
    }

    public void sliceIsoSliderChange(float val)
    {
        print("sliceIsoSliderChange:" + val);

        isolevel = val;
    }

    public void button1Pushed()
    {
        print("button1Pushed");

        MakeContour();
    }

    public void button2Pushed()
    {
        print("button2Pushed");
    }
    
    public void drawIsoline()
    {
        meshScript mscript = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        // Detect and create isoline
        for (int y = 0; y < ydim - 1; y++)
        {
            for (int x = 0; x < xdim - 1; x++)
            {
                makeLine(vertices,
                         heightmap[x, y],
                         heightmap[x + 1, y],
                         heightmap[x, y + 1],
                         heightmap[x + 1, y + 1],
                         x,
                         y);
            }
        }

        int i = 0;
        foreach (Vector3 vertex in vertices)
        {
            indices.Add(i);
            i++;
        }

        mscript.createMeshGeometry(vertices, indices);
    }

    void makeLine(List<Vector3> vertices, float a, float b, float c, float d, int x, int y)
    {
        // Draws a part of an isoline for a given area
        // Input: pixelval for 4 pixels forming a square, x and y coordinates of first pixel

        bool tl = a > isolevel;
        bool tr = b > isolevel;
        bool bl = c > isolevel;
        bool br = d > isolevel;

        // Use the XOR operator to decide if isolevel intersects between the given pixels
        bool top = tl ^ tr;
        bool left = tl ^ bl;
        bool right = tr ^ br;
        bool bottom = bl ^ br;

        if (left || top || right || bottom)
        {
            // Transform value set from [0, 100] to [-0.5, 0.5]
            float dx = (float)(x - 50) / 100;
            float dy = (float)(y - 50) / 100;
            float half = 0.5f / 100;
            float whole = 1f / 100;

            // Draw line depending on where (between which pixels) the isolevel intersects
            if (left && bottom)
            {
                vertices.Add(new Vector3(dx, dy + half, 0));
                vertices.Add(new Vector3(dx + half, dy + whole, 0));
            }
            else if (bottom && right)
            {
                vertices.Add(new Vector3(dx + half, dy + whole, 0));
                vertices.Add(new Vector3(dx + whole, dy + half, 0));
            }
            else if (left && right)
            {
                vertices.Add(new Vector3(dx, dy + half, 0));
                vertices.Add(new Vector3(dx + whole, dy + half, 0));
            }
            else if (left && top)
            {
                vertices.Add(new Vector3(dx, dy + half, 0));
                vertices.Add(new Vector3(dx + half, dy, 0));
            }
            else if (top && bottom)
            {
                vertices.Add(new Vector3(dx + half, dy, 0));
                vertices.Add(new Vector3(dx + half, dy + whole, 0));
            }
            else if (top && right)
            {
                vertices.Add(new Vector3(dx + half, dy, 0));
                vertices.Add(new Vector3(dx + whole, dy + half, 0));
            }
        }
    }

    public void MakeContour()
    {
        meshScript mscript = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();
        vertices = new List<Vector3>();
        indices = new List<int>();
        i = 0;
        
        for (int z = 0; z < zdim - 1; z++)
        {
            for (int y = 0; y < ydim - 1; y++)
            {
                for (int x = 0; x < xdim - 1; x++)
                {
                    Vector3 p = new Vector3(x, y, z);
                    doCube(p);
                }
            }
        }

        // mscript.createMeshGeometry(vertices, indices);
        MeshToFile("mesh.obj", vertices, indices);
    }

    public void doCube(Vector3 p)
    {
        //    p2 ---- p3
        //   / |     / |
        // p6 ---- p7  |
        // |  p0 --|- p1
        // | /     | /
        // p4 ---- p5
        Vector3 p0 = new Vector3(p.x, p.y, p.z);
        Vector3 p1 = new Vector3(p.x + 1, p.y, p.z);
        Vector3 p2 = new Vector3(p.x, p.y + 1, p.z);
        Vector3 p3 = new Vector3(p.x + 1, p.y + 1, p.z);
        Vector3 p4 = new Vector3(p.x, p.y, p.z + 1);
        Vector3 p5 = new Vector3(p.x + 1, p.y, p.z + 1);
        Vector3 p6 = new Vector3(p.x, p.y + 1, p.z + 1);
        Vector3 p7 = new Vector3(p.x + 1, p.y + 1, p.z + 1);

        doTetrahedron(p4, p6, p0, p7);
        doTetrahedron(p6, p0, p7, p2);
        doTetrahedron(p0, p7, p2, p3);
        doTetrahedron(p4, p5, p7, p0);
        doTetrahedron(p1, p7, p0, p3);
        doTetrahedron(p0, p5, p7, p1);
    }

    public void doTetrahedron(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        // Vector3 p12 = new Vector3((p1.x + p2.x) / 2, (p1.y + p2.y) / 2, (p1.z + p2.z) / 2);
        // Vector3 p13 = new Vector3((p1.x + p3.x) / 2, (p1.y + p3.y) / 2, (p1.z + p3.z) / 2);
        // Vector3 p14 = new Vector3((p1.x + p4.x) / 2, (p1.y + p4.y) / 2, (p1.z + p4.z) / 2);
        // Vector3 p23 = new Vector3((p2.x + p3.x) / 2, (p2.y + p3.y) / 2, (p2.z + p3.z) / 2);
        // Vector3 p24 = new Vector3((p2.x + p4.x) / 2, (p2.y + p4.y) / 2, (p2.z + p4.z) / 2);
        // Vector3 p34 = new Vector3((p3.x + p4.x) / 2, (p3.y + p4.y) / 2, (p3.z + p4.z) / 2);
        Vector3 p12 = (p1 + p2) / 2;
        Vector3 p13 = (p1 + p3) / 2;
        Vector3 p14 = (p1 + p4) / 2;
        Vector3 p23 = (p2 + p3) / 2;
        Vector3 p24 = (p2 + p4) / 2;
        Vector3 p34 = (p3 + p4) / 2;

        bool b1 = isAbove(p1, isolevel);
        bool b2 = isAbove(p2, isolevel);
        bool b3 = isAbove(p3, isolevel);
        bool b4 = isAbove(p4, isolevel);

        if ((b1 && b2 && b3 && b4) || (!b1 && !b2 && !b3 && !b4))
        {
            // Nothing
        }
        // In the following cases, the vertices are numbered counter-clockwise
        else if (!b1 && !b2 && !b3 && b4)
        {
            makeTriangle(p34, p24, p14);
        }
        else if (!b1 && !b2 && b3 && !b4)
        {
            makeTriangle(p23, p34, p13);
        }
        else if (!b1 && !b2 && b3 && b4)
        {
            makeQuadrilateral(p23, p24, p14, p13);
        }
        else if (!b1 && b2 && !b3 && !b4)
        {
            makeTriangle(p24, p23, p12);
        }
        else if (!b1 && b2 && !b3 && b4)
        {
            makeQuadrilateral(p14, p34, p23, p12);
        }
        else if (!b1 && b2 && b3 && !b4)
        {
            makeQuadrilateral(p24, p34, p13, p12);
        }
        else if (!b1 && b2 && b3 && b4)
        {
            makeTriangle(p14, p13, p12);
        }
        // Under here, the vertices are defined clockwise again
        else if (b1 && !b2 && !b3 && !b4)
        {
            makeTriangle(p12, p13, p14);
        }
        else if (b1 && !b2 && !b3 && b4)
        {
            makeQuadrilateral(p12, p13, p34, p24);
        }
        else if (b1 && !b2 && b3 && !b4)
        {
            makeQuadrilateral(p12, p23, p34, p14);
        }
        else if (b1 && !b2 && b3 && b4)
        {
            makeTriangle(p12, p23, p24);
        }
        else if (b1 && b2 && !b3 && !b4)
        {
            makeQuadrilateral(p13, p14, p24, p23);
        }
        else if (b1 && b2 && !b3 && b4)
        {
            makeTriangle(p13, p34, p23);
        }
        else if (b1 && b2 && b3 && !b4)
        {
            makeTriangle(p14, p24, p34);
        }

        /*
        if ((b1 && b2 && b3 && b4) || (!b1 && !b2 && !b3 && !b4))
        {
            // Nothing
        }
        else if ((b1 && b2 && b3 && !b4) || (!b1 && !b2 && !b3 && b4))
        {
            makeTriangle(p14, p24, p34);
        }
        else if ((b1 && b2 && !b3 && b4) || (!b1 && !b2 && b3 && !b4))
        {
            makeTriangle(p13, p34, p23);
        }
        else if ((b1 && b2 && !b3 && !b4) || (!b1 && !b2 && b3 && b4))
        {
            makeQuadrilateral(p13, p14, p24, p23);
        }
        else if ((b1 && !b2 && b3 && b4) || (!b1 && b2 && !b3 && !b4))
        {
            makeTriangle(p12, p23, p24);
        }
        else if ((b1 && !b2 && b3 && !b4) || (!b1 && b2 && !b3 && b4))
        {
            makeQuadrilateral(p12, p23, p34, p14);
        }
        else if ((b1 && !b2 && !b3 && b4) || (!b1 && b2 && b3 && !b4))
        {
            makeQuadrilateral(p12, p13, p34, p24);
        }
        else if ((b1 && !b2 && !b3 && !b4) || (!b1 && b2 && b3 && b4))
        {
            makeTriangle(p12, p13, p14);
        }
         */
    }

    public void makeTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Transform values if necessary
        // p1 = transformValues(p1);
        // p2 = transformValues(p2);
        // p3 = transformValues(p3);

        vertices.Add(p1);
        vertices.Add(p2);
        vertices.Add(p3);
        indices.Add(i++);
        indices.Add(i++);
        indices.Add(i++);
    }

    public void makeQuadrilateral(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        makeTriangle(p1, p2, p3);
        makeTriangle(p1, p3, p4);
    }

    public bool isAbove(Vector3 p, float isolevel)
    {
        return pixelval(p) >= isolevel;
    }

    public Vector3 transformValues(Vector3 p1)
    {
        Vector3 p2 = new Vector3();
        p2.x = (p1.x - 50) / 100;
        p2.y = (p1.y - 50) / 100;
        p2.z = (p1.z - 50) / 100;
        return p2;
    }

    // Save a generated mesh to an obj file containing a list of vertices and indices
    public void MeshToFile(string filename, List<Vector3> vertices, List<int> indices)
    {
        StreamWriter stream = new StreamWriter(filename);
        stream.WriteLine("g " + "Mesh");

        foreach (Vector3 v in vertices)
            stream.WriteLine(string.Format("v {0} {1} {2}", v.x, v.y, v.z));

        stream.WriteLine();
        for (int i = 0; i < indices.Count; i += 3)
            stream.WriteLine(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", indices[i] + 1, indices[i + 1] + 1, indices[i + 2] + 1));

        stream.Close();
        print("Mesh saved to file: " + filename);
    }

}
