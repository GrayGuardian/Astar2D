
using System;
using System.Collections.Generic;
using UnityEngine;
public class AstarPointInfo
{
    public float x;
    public float y;
    public int g;
    public int h;
    public int f;
    public AstarPointInfo parent;
}
public class Astar
{
    /// <summary>
    /// 寻路粒子细度 越小越精准 同时运算越慢
    /// </summary>
    public const float OFFSET = 0.7f;
    public Vector2[] getAstarPoints(Vector2[] mapPoints, Vector2[][] barriersPoints, Vector2 startPoint, Vector2 targetPoint)
    {
        List<AstarPointInfo> openList = new List<AstarPointInfo>();
        List<AstarPointInfo> closeList = new List<AstarPointInfo>();
        List<Vector2> result = new List<Vector2>();

        openList.Add(new AstarPointInfo() { x = startPoint.x, y = startPoint.y, g = 0 });

        int result_index = -1;
        do
        {
            AstarPointInfo currentPoint = openList[openList.Count - 1];
            openList.RemoveAt(openList.Count - 1);

            closeList.Add(currentPoint);
            Vector2[] surroundPointArr = surroundPoint(new Vector2(currentPoint.x, currentPoint.y));
            for (int i = 0; i < surroundPointArr.Length; i++)
            {

                Vector2 item = surroundPointArr[i];
                if (validPos(mapPoints, barriersPoints, item) &&
                    existList(item, closeList.ToArray()) == -1 &&
                    validPos(mapPoints, barriersPoints, new Vector2(item.x, currentPoint.y)) &&
                    validPos(mapPoints, barriersPoints, new Vector2(currentPoint.x, item.y)))
                {

                    //g 到父节点的位置
                    //如果是上下左右位置的则g等于10，斜对角的就是14
                    int g = currentPoint.g + ((currentPoint.x - item.x) * (currentPoint.y - item.y) == 0 ? 10 : 14);
                    if (existList(item, openList.ToArray()) == -1)  //如果不在开启列表中
                    {
                        //计算H，通过水平和垂直距离进行确定
                        int h = (int)(Mathf.Abs(targetPoint.x - item.x) * 10 + Mathf.Abs(targetPoint.y - item.y) * 10);
                        AstarPointInfo val = new AstarPointInfo() { x = item.x, y = item.y, h = h, g = g, f = h + g, parent = currentPoint };
                        openList.Add(val);
                    }
                    else
                    {
                        //存在在开启列表中，比较目前的g值和之前的g的大小
                        int index = existList(item, openList.ToArray());
                        //如果当前点的g更小
                        if (g < openList[index].g)
                        {
                            openList[index].parent = currentPoint;
                            openList[index].g = g;
                            openList[index].f = g + openList[index].h;
                        }
                    }
                }
            }
            //如果开启列表空了，没有通路，结果为空
            if (openList.Count == 0)
            {
                break;
            }
            openList.Sort((a, b) => { return b.f - a.f; }); //这一步是为了循环回去的时候，找出 F 值最小的, 将它从 "开启列表" 中移掉

            result_index = existList(targetPoint, openList.ToArray());

        } while (result_index == -1);
        //判断结果列表是否为空
        if (result_index == -1)
        {
            result = new List<Vector2>() { };
        }
        else
        {
            AstarPointInfo currentObj = openList[result_index];
            do
            {
                result.Insert(0, new Vector2(currentObj.x, currentObj.y));
                currentObj = currentObj.parent;
            } while (currentObj.x != startPoint.x || currentObj.y != startPoint.y);

            result.Insert(0, startPoint);
            result.RemoveAt(result.Count - 1);
            result.Add(targetPoint);
        }
        return result.ToArray();
    }

    /// <summary>
    /// 判断坐标是否是有效坐标
    /// </summary>
    public bool validPos(Vector2[] mapPoints, Vector2[][] barriersPoints, Vector2 point)
    {
        if (!pointInPolygon(point, mapPoints))
        {
            return false;
        }
        for (int i = 0; i < barriersPoints.Length; i++)
        {
            Vector2[] barrierPoints = barriersPoints[i];
            if (pointInPolygon(point, barrierPoints))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 获取周围八个点的值
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public Vector2[] surroundPoint(Vector2 point)
    {
        float x = point.x;
        float y = point.y;
        return new Vector2[] {
            new Vector2(x-OFFSET,y-OFFSET),
            new Vector2(x,y-OFFSET),
            new Vector2(x+OFFSET,y-OFFSET),
            new Vector2(x+OFFSET,y),
            new Vector2(x+OFFSET,y+OFFSET),
            new Vector2(x,y+OFFSET),
            new Vector2(x-OFFSET,y+OFFSET),
            new Vector2(x-OFFSET,y)
        };
    }

    /// <summary>
    /// 判断点是否存在在列表中，是的话返回的是序列号
    /// </summary>
    /// <param name="point"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    public int existList(Vector2 point, AstarPointInfo[] list)
    {
        for (int i = 0; i < list.Length; i++)
        {
            if (point.x == list[i].x && point.y == list[i].y)
            {
                return i;
            }
            float len = Mathf.Abs(point.x - list[i].x) + Mathf.Abs(point.y - list[i].y);
            if (len < OFFSET)
            {
                return i;
            }

        }
        return -1;
    }

    /// <summary>
    /// 判断点是否在多边形内 通过夹角合是否大于360来判断
    /// </summary>
    /// <returns></returns>
    public bool pointInPolygon(Vector2 pos, Vector2[] points)
    {
        int len = points.Length;
        float angleSum = 0;
        for (int i = 0; i < len; i++)
        {
            Vector2 pos1 = points[i];
            Vector2 pos2 = i + 1 >= len ? points[0] : points[i + 1];
            Vector2 v1 = pos1 - pos;
            Vector2 v2 = pos2 - pos;

            float angle = getVectorAngle(v1, v2);
            angleSum = angleSum + angle;
        }
        return Math.Round(angleSum) >= 360;
    }

    /// <summary>
    /// 获取两个向量夹角(存在正负)
    /// </summary>
    public float getVectorAngle(Vector2 v1, Vector2 v2)
    {
        float cos = (v1.x * v2.x + v1.y * v2.y) / (Mathf.Sqrt(Mathf.Pow(v1.x, 2) + Mathf.Pow(v1.y, 2)) * Mathf.Sqrt(Mathf.Pow(v2.x, 2) + Mathf.Pow(v2.y, 2)));
        float angle = 180 * Mathf.Acos(cos) / Mathf.PI;
        if (v1.x * v2.y - v1.y * v2.x < 0)
        {
            angle = -angle;
        }
        angle = float.Equals(angle, float.NaN) ? 360 : angle;
        return angle;
    }

}
