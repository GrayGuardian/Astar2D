using System.Security.Cryptography.X509Certificates;
using System.Data.SqlTypes;
using System.Data;
using UnityEngine;
[RequireComponent(typeof(PolygonCollider2D))]
public class Main : MonoBehaviour
{
    public Camera Camera;
    public Transform player;
    Vector2[] mapPoints = null;
    Vector2[][] barriersPoints;
    Astar astar;
    Transform posBox;
    private void Start()
    {
        // 获取地图数据
        PolygonCollider2D[] polygons = transform.GetComponentsInChildren<PolygonCollider2D>();
        barriersPoints = new Vector2[polygons.Length - 1][];
        for (int i = 0; i < polygons.Length; i++)
        {
            Vector2[] points = getPolygonPoints(polygons[i]);
            if (i == 0)
            {
                Debug.Log("地图多边形点数>>" + points.Length);
                mapPoints = points;
            }
            else
            {
                Debug.Log("障碍" + (i - 1) + "多边形点数>>" + points.Length);
                barriersPoints[i - 1] = points;
            }
        }

        player.localScale = Vector3.one * Astar.OFFSET;
        posBox = new GameObject("posBox").transform;

        astar = new Astar();

    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 startPos = player.position;
            Vector3 targetPos = Camera.ScreenToWorldPoint(Input.mousePosition);


            Debug.Log("startPos>>>>" + startPos + " targetPos>>>" + targetPos);
            //计算寻路路径
            Vector2[] poss = astar.getAstarPoints(mapPoints, barriersPoints, startPos, targetPos);
            Debug.Log("寻路路径数量>>>" + poss.Length);
            //打印寻路路径
            if (poss.Length > 0)
            {
                for (int i = 0; i < posBox.childCount; i++)
                {
                    Destroy(posBox.GetChild(i).gameObject);
                }
                foreach (var pos in poss)
                {
                    GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube1.transform.localScale = Vector3.one * Astar.OFFSET;
                    cube1.transform.position = new Vector3(pos.x, pos.y, 0);
                    cube1.transform.parent = posBox;
                }
                player.position = poss[poss.Length - 1];
            }


        }
    }

    private Vector2[] getPolygonPoints(PolygonCollider2D polygon)
    {
        Vector2[] points = polygon.points;
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = points[i] + (Vector2)polygon.transform.position;
        }
        return points;
    }

}
