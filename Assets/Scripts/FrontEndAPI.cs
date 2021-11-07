using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/*
 * This interface is how the back end sends data to the front end for rendering. When the back end has parsed
 * the data and is ready to send it here, it should retrieve a data ingester with the appropriate method in
 * FrontEndAPI, and display each hex cell.
 *
 * Hiring manager's note: this is admittedly a clunkier way to do this than we'd use in a real project.
 * However, for purposes of this homework exercise, it has the advantage of making zero assumptions or
 * constraints on how the back end works, and allowing you a blank canvas to build the back end however
 * you think best.
 */
public interface FrontEndDataIngester {
    void DisplayHexCell(Vector3 canonicalCenter, List<Vector3> vertices);
}

/*
 * This class forms the graphical front end of our project. You'll need to get this to start itself up using
 * your knowledge of Unity set-up, and then examine the code to find out how to get a reference to it and call
 * the right API methods at the right time. Normally our code would provide a few more hints in the form of
 * inline comments, but an important skill you'll need is to examine and understand existing code.
 *
 * The additional help at the top of this file is because the data ingester pattern is a little out of the
 * ordinary for this situation. The rest is up to your skills and experience. We look forward to the next step!
 */
public class FrontEndAPI : MonoBehaviour, FrontEndDataIngester {
    private static readonly string CELLS_PARENT_NAME = "CellsParent";
    private static readonly string GAME_OBJECT_NAME = "HexCell";
    private static readonly string STEEL_MATERIAL_PATH = "steel_mat";
    private static readonly int[] HEX_TRIANGLES = new int[] { 0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5 };
    private static readonly int[] PENT_TRIANGLES = new int[] { 0, 1, 2, 0, 2, 3, 0, 3, 4 };
    private static readonly Vector2[] HEX_UVS = new Vector2[] {
    		new Vector2(0.5f, 0.9902f),
            new Vector2(0.0887f, 0.7474f),
            new Vector2(0.0887f, 0.2580f),
            new Vector2(0.5f, 0.0147f),
            new Vector2(0.9303f, 0.2580f),
            new Vector2(0.9303f, 0.7474f)
        };
    private static readonly Vector2[] PENT_UVS = new Vector2[] {
    		new Vector2(0.89f, 0.1920f),
            new Vector2(0.7982f, 0.1254f),
            new Vector2(0.8332f, 0.0172f),
            new Vector2(0.9468f, 0.0172f),
            new Vector2(0.9818f, 0.1254f)
        };

    private static FrontEndAPI _instance;

    public static FrontEndAPI shared {
        get { return _instance; }
    }

    private GameObject cellsParent = null;
    private Material steelMaterial = null;
    private Thread mainThread = null;

    private void Awake() {
        _instance = this;
        cellsParent = new GameObject(CELLS_PARENT_NAME);
        cellsParent.transform.position = Vector3.zero;
        steelMaterial = Resources.Load<Material>(STEEL_MATERIAL_PATH);
        mainThread = Thread.CurrentThread;
    }
    
    

    public FrontEndDataIngester GetDataIngester() {
        return this;
    }

    private void Update()
    {
        cellsParent.transform.Rotate(0, .03f,.03f);
    }

    public void DisplayHexCell(Vector3 canonicalCenter, List<Vector3> vertices) {
        if(Thread.CurrentThread != mainThread)
            throw new Exception("DisplayHexCell() can only be called on the main thread.");
        if(vertices.Count < 5 || vertices.Count > 6)
            throw new Exception("DisplayPolygon() was sent an invalid list of vertices.");
        GameObject cell = new GameObject(GAME_OBJECT_NAME);
        cell.transform.parent = cellsParent.transform;
        cell.transform.localPosition = canonicalCenter;
        cell.transform.localEulerAngles = Vector3.zero;
        cell.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        Vector3[] meshVertices = new Vector3[vertices.Count];
        for(int i = 0; i < vertices.Count; i++)
            meshVertices[i] = vertices[i] - canonicalCenter;
        mesh.vertices = meshVertices;
        mesh.triangles = (vertices.Count == 5) ? PENT_TRIANGLES : HEX_TRIANGLES;
        mesh.uv = (vertices.Count == 5) ? PENT_UVS : HEX_UVS;
        mesh.RecalculateNormals();
        cell.GetComponent<MeshFilter>().mesh = mesh;
        MeshRenderer renderer = cell.GetComponent<MeshRenderer>();
        if(renderer == null)
            renderer = cell.AddComponent<MeshRenderer>();
        renderer.material = steelMaterial;
    }
}
