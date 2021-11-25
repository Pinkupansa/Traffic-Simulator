using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class PathRenderer : MonoBehaviour
{
    private GameObject roadRoot;
    [SerializeField] private GameObject roadPrefab;
    [SerializeField] private GameObject pointPrefab;
    private GameObject pointsRoot;
    [SerializeField] private bool renderPoints;
    private const float BASE_LENGTH = 3.1f;
    public void RenderPath(Road road)
    {
        if(roadRoot != null)
        {
            Destroy(roadRoot);
        }
        roadRoot = new GameObject("RoadRoot");

        foreach (RoadPoint point in road.points)
        {
            IEnumerable<Vector2> neighbours = point.neighbours.Select(x => road.GetPointByID(x).position);
            foreach(Vector2 npos in neighbours)
            {
                Vector2 linkPos = (point.position + npos) / 2f;
                GameObject linkInstance = Instantiate(roadPrefab, linkPos, Quaternion.identity);
                Vector2 linkVector = npos - point.position;
                linkInstance.transform.right = linkVector;
                linkInstance.transform.localScale = new Vector3(linkVector.magnitude/BASE_LENGTH, transform.localScale.y, transform.localScale.z);
                linkInstance.transform.SetParent(roadRoot.transform);
            }
        }
        
        if(renderPoints)
        {
            RenderPoints(road);
        }
    }
    private void RenderPoints(Road road)
    {
        if (pointsRoot != null)
        {
            Destroy(pointsRoot);

        }
        pointsRoot = new GameObject("test");
        foreach(RoadPoint p in road.points)
        {
            GameObject test = Instantiate(pointPrefab, p.position, Quaternion.identity);
            test.transform.SetParent(pointsRoot.transform);
        }
    }

}
