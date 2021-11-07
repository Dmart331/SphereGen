using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

enum Shape
{
    Hexagon = 0,
    Pentagon = 1
}

public class GetMeshDataFromServer : MonoBehaviour
{
    [SerializeField] private string url;
    private FrontEndAPI api;
    private List<ResultObject> ResultObjects = new List<ResultObject>();

    private void Start()
    {
        api = GetComponent<FrontEndAPI>();
        StartCoroutine(GetMeshData());
    }
    
    private IEnumerator GetMeshData() 
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success) 
        {
            Debug.LogError(www.error);
        }
        else 
        {
            string result = www.downloadHandler.text;
            SetUpMeshObject(result);
        }
    }

    private void SetUpMeshObject(string result)
    {
        string[] dataRows = SplitRows(result);
        AssignTokens(dataRows);
    }

    private string[] SplitRows(string result)
    {
        var r =  result.Split(
            new string[] { "\r\n", "\r", "\n" },
            StringSplitOptions.None
        ); 
        var rowsList = r.ToList();
        rowsList.RemoveAt(0);
        rowsList.RemoveAt(rowsList.Count - 1);
        return rowsList.ToArray();
    }
    
    private void AssignTokens(string[] row)
    {
        foreach (string token in row)
        {
            string[] tokens = SplitTokens(token);
            ResultObject ro = new ResultObject();
            SetUpResultsObject(ro, tokens);
            if (tokens.Length == 11)
            {
                CreatePolygon(tokens, ro, Shape.Hexagon);
            }
            else if (tokens.Length == 10)
            {
                CreatePolygon(tokens, ro, Shape.Pentagon);
            }
            else
            {
                Debug.LogError("Shape Not Recognized");
            }
            ResultObjects.Add(ro);
            api.GetDataIngester().DisplayHexCell(ro.CanonicalCenter, ro.VertexPoints);
        }
    }
    
    private string[] SplitTokens(string stringArray)
    {
        return stringArray.Split(' ');
    }
    
    private void SetUpResultsObject(ResultObject ro, string[] tokens)
    {
        ro.RingNumber = int.Parse(tokens[0]);
        ro.RingPosition = int.Parse(tokens[1]);
        ro.RingNumber = int.Parse(tokens[2]);
        var canCenter = tokens[3].Split(',');
        ro.CanonicalCenter = new Vector3(float.Parse(canCenter[0]), float.Parse(canCenter[1]), float.Parse(canCenter[2]));
    }
    
    private void CreatePolygon(string[] tokens, ResultObject ro, Shape shape)
    {
        bool isHexagon = shape == Shape.Hexagon;
        List<Vector3> vector3ListObject = new List<Vector3>();
        string[] vector3List = isHexagon ? HexagonCollection(tokens) : PentagonCollection(tokens);
        foreach (var st in vector3List)
        {
            AddToVectorList(st, vector3ListObject);
        }
        ro.VertexPoints = vector3ListObject;
        string[] neighborSet = NeighboringPolygonTokens(tokens, isHexagon);
        List<int> neighboringCells = new List<int>();
        AddToNeighborList(neighborSet, neighboringCells);
        ro.NeighborCells = neighboringCells;
    }
    
    private string[] HexagonCollection(string[] tokens)
    {
       return new string[6] {tokens[4], tokens[5], tokens[6], tokens[7], tokens[8], tokens[9]};
    }
    
    private string[] PentagonCollection(string[] tokens)
    {
        return new string[5] {tokens[4], tokens[5], tokens[6], tokens[7], tokens[8]};
    }
    
    private string[] NeighboringPolygonTokens(string[] tokens, bool isHexagon)
    {
        return isHexagon ? tokens[10].Split(',') : tokens[9].Split(',');
    }

    private void AddToNeighborList(string[] neighborSet, List<int> neighboringCells)
    {
        foreach (var s in neighborSet)
        {
            neighboringCells.Add(int.Parse(s));
        }
    }

    private void AddToVectorList(string st, List<Vector3> vector3ListObject)
    {
        string[] s = st.Split(',');
        Vector3 newV = new Vector3(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]));
        vector3ListObject.Add(newV);
    }




    
}
