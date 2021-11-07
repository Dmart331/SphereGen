using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultObject : MonoBehaviour
{
    public int RingNumber;
    public int RingPosition;
    public int SectorNumber;
    public Vector3 CanonicalCenter;
    public List<Vector3> VertexPoints;
    public List<int> NeighborCells;
}
