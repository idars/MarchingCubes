using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;

public class quadScript : MonoBehaviour
{
    // member variables, accessible from any function
    Slice[] _slices;
    int _numSlices;
    int _minIntensity;
    int _maxIntensity;
    // int _sliceNum;
    float _iso;
    // float _brightness;
    int _xdim;
    int _ydim;
    int _zdim;
    // Vector3 _voxelSize;

    // member variables used for generating meshes (to make it easier for us)
    List<Vector3> _vertices;
    List<int> _indices;
    int _i;

    // Start is called once when the application is run
    void Start()
    {
        print("void Start was called");
        readDicom();
        sliceIsoSliderChange(0.5f);
        setSlice(0);
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Reads scanned images (slices) from a dicom file
    void readDicom()
    {
        Slice.initDicom();
        string dicomfilepath = @"C:\Users\143362\Documents\Scans\head";

        _numSlices = Slice.getnumslices(dicomfilepath);
        _slices = new Slice[_numSlices];

        float min = 0;
        float max = 0;
        Slice.getSlices(dicomfilepath, _numSlices, out _slices, out min, out max);

        SliceInfo info = _slices[0].sliceInfo;

        _minIntensity = (int)min;
        _maxIntensity = (int)max;
        _iso = (_minIntensity + _maxIntensity) / 2;
        _xdim = info.Rows;
        _ydim = info.Columns;
        _zdim = _numSlices;
        print("Number of slices read:" + _numSlices);
    }

    // Displays slice z in the preview screen
    void setSlice(int z)
    {
        if (z < 0 || z > _numSlices) z = 0;

        var texture = new Texture2D(_xdim, _ydim, TextureFormat.RGB24, false); // garbage collector will tackle that it is new'ed 

        for (int y = 0; y < _ydim; y++)
        {
            for (int x = 0; x < _xdim; x++)
            {
                float v = pixelval(new Vector3(x, y, z));
                v = interpolant(_minIntensity, _maxIntensity, v); // Normalizing v to fit within min and max density
                texture.SetPixel(x, y, new UnityEngine.Color(v, v, v));
            }
        }

        texture.filterMode = FilterMode.Point; // nearest neigbor interpolation is used. (alternative is FilterMode.Bilinear)
        texture.Apply(); // Apply all SetPixel calls
        GetComponent<Renderer>().material.mainTexture = texture;
    }

    // Returns the perceived density of (x, y) in slice z
    float pixelval(Vector3 p)
    {
        if (p.z < 0 || p.z > _numSlices)
        {
            return 0f;
        }
        else
        {
            Slice s = _slices[(int)p.z];
            ushort[] image_data = s.getPixels();
            int index = _xdim * (int)p.x + (int)p.y; // width * row + column
            return image_data[index]; // Implicit cast from ushort (16-bit int) to float
        }
    }

    // Sets the position of the first slider
    // This slider controls which slice is being shown
    void slicePosSliderChange(float val)
    {
        print("slicePosSliderChange:" + val);

        // Get value relative to the number of slices
        float depth = val * _numSlices;

        // Cast to integer in order to view specific slice
        setSlice((int)depth);
    }

    // Set the position of the second slider
    // This slider controls the isovalue
    // It won't have any impact on the preview screen, but it controls what densities should be displayed when generating a mesh
    void sliceIsoSliderChange(float val)
    {
        print("sliceIsoSliderChange:" + val);

        // Set iso between min and max intensity, assuming val is between min and max
        _iso = (val * (_maxIntensity - _minIntensity)) + _minIntensity;
    }

    // Currently using this button to generate the mesh
    void button1Pushed()
    {
        print("button1Pushed");

        MakeContour();
    }

    void button2Pushed()
    {
        print("button2Pushed");
    }
    
    // Obsolete methods

    //void drawIsoline()
    //{
    //    meshScript mscript = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();
    //    List<Vector3> vertices = new List<Vector3>();
    //    List<int> indices = new List<int>();

    //    // Detect and create isoline
    //    for (int y = 0; y < ydim - 1; y++)
    //    {
    //        for (int x = 0; x < xdim - 1; x++)
    //        {
    //            makeLine(vertices,
    //                     heightmap[x, y],
    //                     heightmap[x + 1, y],
    //                     heightmap[x, y + 1],
    //                     heightmap[x + 1, y + 1],
    //                     x,
    //                     y);
    //        }
    //    }

    //    int i = 0;
    //    foreach (Vector3 vertex in vertices)
    //    {
    //        indices.Add(i);
    //        i++;
    //    }

    //    mscript.createMeshGeometry(vertices, indices);
    //}

    //void makeLine(List<Vector3> vertices, float a, float b, float c, float d, int x, int y)
    //{
    //    // Draws a part of an isoline for a given area
    //    // Input: pixelval for 4 pixels forming a square, x and y coordinates of first pixel

    //    bool tl = a > isolevel;
    //    bool tr = b > isolevel;
    //    bool bl = c > isolevel;
    //    bool br = d > isolevel;

    //    // Use the XOR operator to decide if isolevel intersects between the given pixels
    //    bool top = tl ^ tr;
    //    bool left = tl ^ bl;
    //    bool right = tr ^ br;
    //    bool bottom = bl ^ br;

    //    if (left || top || right || bottom)
    //    {
    //        // Transform value set from [0, 100] to [-0.5, 0.5]
    //        float dx = (float)(x - 50) / 100;
    //        float dy = (float)(y - 50) / 100;
    //        float half = 0.5f / 100;
    //        float whole = 1f / 100;

    //        // Draw line depending on where (between which pixels) the isolevel intersects
    //        if (left && bottom)
    //        {
    //            vertices.Add(new Vector3(dx, dy + half, 0));
    //            vertices.Add(new Vector3(dx + half, dy + whole, 0));
    //        }
    //        else if (bottom && right)
    //        {
    //            vertices.Add(new Vector3(dx + half, dy + whole, 0));
    //            vertices.Add(new Vector3(dx + whole, dy + half, 0));
    //        }
    //        else if (left && right)
    //        {
    //            vertices.Add(new Vector3(dx, dy + half, 0));
    //            vertices.Add(new Vector3(dx + whole, dy + half, 0));
    //        }
    //        else if (left && top)
    //        {
    //            vertices.Add(new Vector3(dx, dy + half, 0));
    //            vertices.Add(new Vector3(dx + half, dy, 0));
    //        }
    //        else if (top && bottom)
    //        {
    //            vertices.Add(new Vector3(dx + half, dy, 0));
    //            vertices.Add(new Vector3(dx + half, dy + whole, 0));
    //        }
    //        else if (top && right)
    //        {
    //            vertices.Add(new Vector3(dx + half, dy, 0));
    //            vertices.Add(new Vector3(dx + whole, dy + half, 0));
    //        }
    //    }
    //}

    void MakeContour()
    {
        meshScript mscript = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();
        _vertices = new List<Vector3>();
        _indices = new List<int>();
        _i = 0;
        
        // for (int z = 0; z < _zdim - 1; z++)
        // for a test run we can do 1/4th of the figure
        for (int z = 0; z < _zdim / 4; z++)
        {
            for (int y = 0; y < _ydim - 1; y++)
            {
                for (int x = 0; x < _xdim - 1; x++)
                {
                    Vector3 p = new Vector3(x, y, z);
                    doCube(p);
                }
            }
        }

        // mscript.createMeshGeometry(vertices, indices);
        MeshToFile("mesh.obj", _vertices, _indices);
    }

    void doCube(Vector3 p)
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

    void doTetrahedron(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        // Midpoint vertices
        // Vector3 p12 = (p1 + p2) / 2;
        // Vector3 p13 = (p1 + p3) / 2;
        // Vector3 p14 = (p1 + p4) / 2;
        // Vector3 p23 = (p2 + p3) / 2;
        // Vector3 p24 = (p2 + p4) / 2;
        // Vector3 p34 = (p3 + p4) / 2;

        // Interpolated vertices
        Vector3 p12 = Vector3.Lerp(p1, p2, interpolant(p1, p2, _iso));
        Vector3 p13 = Vector3.Lerp(p1, p3, interpolant(p1, p3, _iso));
        Vector3 p14 = Vector3.Lerp(p1, p4, interpolant(p1, p4, _iso));
        Vector3 p23 = Vector3.Lerp(p2, p3, interpolant(p2, p3, _iso));
        Vector3 p24 = Vector3.Lerp(p2, p4, interpolant(p2, p4, _iso));
        Vector3 p34 = Vector3.Lerp(p3, p4, interpolant(p3, p4, _iso));

        bool b1 = isAbove(p1, _iso);
        bool b2 = isAbove(p2, _iso);
        bool b3 = isAbove(p3, _iso);
        bool b4 = isAbove(p4, _iso);

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
    }

    void makeTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        _vertices.Add(p1);
        _vertices.Add(p2);
        _vertices.Add(p3);
        _indices.Add(_i++);
        _indices.Add(_i++);
        _indices.Add(_i++);
    }

    void makeQuadrilateral(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        makeTriangle(p1, p2, p3);
        makeTriangle(p1, p3, p4);
    }

    // Returns true if the density at p is higher than the given value
    bool isAbove(Vector3 p, float val)
    {
        return pixelval(p) >= val;
    }

    // Returns the difference between p1 and p2 relative to the given value
    //
    // Example:  0.4 ----+---- 0.5   -->   0 ----+---- 1
    //                  val = 0.45              val = 0.5
    //
    // I haven't checked for values outside the [p1, p2] interval
    float interpolant(Vector3 p1, Vector3 p2, float val)
    {
        float v1 = pixelval(p1);
        float v2 = pixelval(p2);
        return interpolant(v1, v2, val);
    }

    float interpolant(float v1, float v2, float val)
    {
        return (val - v1) / (v2 - v1);
    }

    // Saves a generated mesh to an obj file containing a list of vertices and indices
    void MeshToFile(string filename, List<Vector3> vertices, List<int> indices)
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
