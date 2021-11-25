using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//C'est en fait un graphe orienté
public class Road
{
    public HashSet<RoadPoint> points { get; private set; }
    private Dictionary<int, RoadPoint> idToPointDictionary;

    
    public Road()
    {
        points = new HashSet<RoadPoint>();
        idToPointDictionary = new Dictionary<int, RoadPoint>();
    }
    public int Length
    {
        get
        {
            return points.Count;
        }
    }
    private int GenerateID()
    {
        return Length;
    }
    public int GetClosestPointID(Vector2 position)
    {
        float minDistance = Mathf.Infinity;
        int minID = 0;
        foreach (RoadPoint p in points)
        {
            float dist = (position - p.position).magnitude;
            if(dist < minDistance)
            {
                minDistance = dist;
                minID = p.ID;
            }
        }
        
        return minID;
    }
    public int GetClosestPointIDAmongSet(Vector2 position, HashSet<int> IDSet)
    {
        float minDistance = Mathf.Infinity;
        int minID = -1;
        foreach (int ID in IDSet)
        {
            RoadPoint p = GetPointByID(ID);
            float dist = (position - p.position).magnitude;
            if (dist < minDistance)
            {
                minDistance = dist;
                minID = p.ID;
            }
        }
        if (minID == -1)
        {
            throw new System.Exception("Y A PAS DE VOISIN");
        }
        return minID;
    }
    public int GetClosestPointIDExceptSet(Vector2 position, HashSet<int> IDSet)
    {
        float minDistance = Mathf.Infinity;
        int minID = 0;
        HashSet<int> listToSearch = new HashSet<int>(idToPointDictionary.Keys);
        foreach(int id in IDSet)
        {
            listToSearch.Remove(id);
        }

        return GetClosestPointIDAmongSet(position, listToSearch) ;
    }
    public List<int> AddPoints(List<Vector2> _points, int linkTo)
    {
        List<int> pointsID = new List<int>();
        int lastID = AddPoint(_points[0], linkTo);
        pointsID.Add(lastID);
        for (int i = 1; i < _points.Count; i++)
        {
            lastID = AddPoint(_points[i], lastID);
            pointsID.Add(lastID);
        }
        return pointsID;
    }
    
    public int AddPoint(Vector2 point, int linkTo)
    {
        int ID = GenerateID();
        RoadPoint roadPoint = new RoadPoint(ID, point, new HashSet<int>());
        points.Add(roadPoint);
        idToPointDictionary.Add(ID, roadPoint);
        if (ExistsID(linkTo))
        {
            GetPointByID(linkTo).AddNeighbour(ID);
        }
        return ID;
    }
    public void Link(int from, int to)
    {
        GetPointByID(from).AddNeighbour(to);
    }
    public RoadPoint GetPointByID(int ID)
    {
        try
        {
            return idToPointDictionary[ID];
        }
        catch
        {
            throw new System.Exception(string.Format("There isn't any point with ID {0} in the road", ID));
        }
    }
    public bool ExistsID(int ID)
    {
        return idToPointDictionary.ContainsKey(ID);
    }
    public void RemovePoints(List<int> IDs)
    {
        foreach(int id in IDs)
        {
            RemovePoint(id);
        }
    }
    public void RemovePoint(int ID)
    {

        points.RemoveWhere(x => x.ID == ID);
        foreach(RoadPoint p in points)
        {
            if (p.neighbours.Contains(ID))
            {
                p.neighbours.Remove(ID);
            }
        }
        idToPointDictionary.Remove(ID);

    }
}
public struct RoadPoint
{

    public RoadPoint(int _ID, Vector2 _position, HashSet<int> _neighbours)
    {
        ID = _ID;
        position = _position;
        neighbours = _neighbours;
    }
    public int ID { get; private set; }
    public Vector2 position;
    public HashSet<int> neighbours;
    public void AddNeighbour(int ID)
    {
        neighbours.Add(ID);
    }
}