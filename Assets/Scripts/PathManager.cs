using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[RequireComponent(typeof(PathRenderer))]
public class PathManager : MonoBehaviour
{
    public static PathManager current;
    public Road road { get; private set; }
    [SerializeField] private float pathSmoothness;
    [SerializeField] private float clampDistance;
    private RoadPoint pointToLinkTo;
    private Vector2 currentRoadDirection;
    private bool roadHasVector;
    private void Start()
    {
        if(current == null)
        {
            current = this;
        }
        road = new Road();
        roadHasVector = false;
    }
    
    public void SetLink(Vector2 position)
    {
        pointToLinkTo = road.GetPointByID(road.GetClosestPointID(position));
        GetComponent<PathRenderer>().RenderPath(road);

        Vector2 roadNextPointPosition = Vector2.one;
        if(pointToLinkTo.neighbours.Count > 0)
        {
            roadNextPointPosition = road.GetPointByID(pointToLinkTo.neighbours.ToList()[0]).position;
            
            currentRoadDirection = (roadNextPointPosition - pointToLinkTo.position).normalized;
        }
        else
        {
            HashSet<int> exceptSet = new HashSet<int>();
            exceptSet.Add(pointToLinkTo.ID);
            roadNextPointPosition = road.GetPointByID(road.GetClosestPointIDExceptSet(position, exceptSet)).position;
            currentRoadDirection = -(roadNextPointPosition - pointToLinkTo.position).normalized;
        }
        
        
    }
    
    public void CreatePoint(Vector2 position)
    {
        
        if(road.Length == 0)
        {
            int ID = road.AddPoint(position, -1);
            pointToLinkTo = road.GetPointByID(ID);
           
        }
        else
        {

            RoadPoint closestPoint = road.GetPointByID(road.GetClosestPointID(position));
            Vector2 controlPoint = roadHasVector ? pointToLinkTo.position + currentRoadDirection * Vector2.Distance(pointToLinkTo.position, position) / 2f : (pointToLinkTo.position + position) / 2f;
            Debug.DrawLine(pointToLinkTo.position, controlPoint, Color.red, 2);
            if (Vector2.Distance(closestPoint.position, position) < clampDistance)
            {
                Vector2 tangent = GetTangent(closestPoint.ID);
                if(Vector2.Dot(tangent, pointToLinkTo.position - closestPoint.position) <= 0)
                {
                    tangent = -tangent;
                }
                Vector2 controlPoint2 = closestPoint.position + tangent * Vector2.Distance(pointToLinkTo.position, position) / 2f;
                Debug.DrawLine(closestPoint.position, controlPoint2, Color.red, 2);
               
                List<int> addedPointsID = road.AddPoints(Beziers.BeziersCurve(new Vector2[4] { pointToLinkTo.position, controlPoint, controlPoint2, closestPoint.position }, 1 / (pathSmoothness * Vector2.Distance(pointToLinkTo.position, position))), pointToLinkTo.ID);
                currentRoadDirection = (road.GetPointByID(addedPointsID[addedPointsID.Count - 1]).position - current.road.GetPointByID(addedPointsID[addedPointsID.Count - 2]).position).normalized;
                pointToLinkTo = road.GetPointByID(addedPointsID[addedPointsID.Count - 1]);
                roadHasVector = true;
                road.Link(pointToLinkTo.ID, closestPoint.ID);
                pointToLinkTo = closestPoint;
            }
            else
            {
                
                List<int> addedPointsID = road.AddPoints(Beziers.BeziersCurve(new Vector2[3] { pointToLinkTo.position, controlPoint, position }, 1 / (pathSmoothness * Vector2.Distance(pointToLinkTo.position, position))), pointToLinkTo.ID);
                currentRoadDirection = (road.GetPointByID(addedPointsID[addedPointsID.Count - 1]).position - current.road.GetPointByID(addedPointsID[addedPointsID.Count - 2]).position).normalized;
                pointToLinkTo = road.GetPointByID(addedPointsID[addedPointsID.Count - 1]);
                roadHasVector = true;

            }






        }
        GetComponent<PathRenderer>().RenderPath(road);
      
    }
    public void RemovePoints(List<int> IDs)
    {
        road.RemovePoints(IDs);
        GetComponent<PathRenderer>().RenderPath(road);
    }
    private Vector2 GetTangent(int ID)
    {
        RoadPoint point = road.GetPointByID(ID);
        if(point.neighbours.Count > 0)
        {
            return (road.GetPointByID(point.neighbours.ToList()[0]).position - point.position).normalized;
        }
        else
        {
            RoadPoint n = new RoadPoint();
            foreach(RoadPoint p in road.points)
            {
                if(p.neighbours.Contains(ID))
                {
                    n = p;
                    break;
                }
            }
            return GetTangent(n.ID);
        }
    }
}
