using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShootLineRender : MonoBehaviour
{
    public Transform arrow;
    public Transform linePivot;
    public GameObject linePrefab; 

    private List<GameObject> lineR = new List<GameObject>();

    private Color lineColor = Color.white;

    private void Update()
    {
        CalculateLineBasedPhysics();
    }

    private void CalculateLineBasedPhysics()
    {
        Ray2D ray = new Ray2D(arrow.position, arrow.up);
        List<Vector2> listHitWalls = new List<Vector2>();
        List<Vector3> listHitWallsLocal = new List<Vector3>();

        listHitWalls.Add(ray.origin);
        listHitWalls.AddRange(RaycastRecursive(ray));

        listHitWalls.ForEach(v =>
        {
            listHitWallsLocal.Add(linePivot.InverseTransformPoint(v));
        });

        PrepareLines(listHitWallsLocal.Count);
        int countWall = 0;

        for (int i = 0; i < listHitWallsLocal.Count - 1; i++)
        {
            countWall++;
            if (countWall < 3)
                DrawTextureBasedLine(lineR[i], listHitWallsLocal[i], listHitWallsLocal[i + 1]);
        }
    }

    private List<Vector2> RaycastRecursive(Ray2D ray)
    {
        List<Vector2> list = new List<Vector2>();

        RaycastHit2D hitWall = Physics2D.Raycast(ray.origin, ray.direction, 1080, 1 << LayerMask.NameToLayer(ValueManager.LAYER_WALL_LINE));
        RaycastHit2D hitBall = Physics2D.Raycast(ray.origin, ray.direction, 1080, 1 << LayerMask.NameToLayer(ValueManager.LAYER_BUBBLE));

        if (hitBall.collider != null || ray.direction.Equals(Vector2.zero))
        {
            Debug.DrawLine(ray.origin, hitBall.point, Color.red);
            list.Add(hitBall.point);
            return list;
        }

        // 벽에 충돌한 경우
        if (hitWall.collider != null)
        {
            Debug.DrawLine(ray.origin, hitWall.point, Color.red);
            Vector2 oppositePoint = FindOppositePoint(ray.origin, hitWall.point);
            Vector2 dir = oppositePoint - hitWall.point;

            list.Add(hitWall.point);
            list.AddRange(RaycastRecursive(new Ray2D(hitWall.point + dir.normalized, dir)));
        }
        return list;
    }

    private Vector2 FindOppositePoint(Vector2 p, Vector2 pline)
    {
        Vector2 midPoint = new Vector2(p.x, pline.y);
        return midPoint * 2 - p;
    }

    private void PrepareLines(int count)
    {
        int needToAdd = count - lineR.Count;
        for (int i = 0; i < needToAdd; i++)
        {
            GameObject go = GameObject.Instantiate(linePrefab);
            go.transform.parent = linePivot;
            go.transform.localScale = Vector3.one;
            lineR.Add(go);
        }

        lineR.ForEach(obj =>
        {
            obj.SetActive(false);
        });
    }

    private void DrawTextureBasedLine(GameObject texture, Vector3 start, Vector3 end)
    {
        texture.transform.localPosition = (start + end) / 2;
        texture.transform.up = start - end;
        texture.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(33, (end - start).magnitude);
        texture.GetComponent<Image>().color = lineColor;
        texture.SetActive(true);
    }

    public void SetLineColor(Color color)
    {
        lineColor = color;
        lineR.ForEach(segment =>
        {
            if (segment.activeSelf)
            {
                segment.GetComponent<Image>().color = lineColor;
            }
        });
    }
}
