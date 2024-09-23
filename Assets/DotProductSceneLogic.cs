    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class DotProductSceneLogic : MonoBehaviour {

      //Set these values in the Unity Inspector
      public Vector2 wallTail;
      public Vector2 wallHead;

      public Vector2 pathTail;
      public Vector2 pathHead;

      private void OnDrawGizmos() {
        int n = 10;
        Gizmos.color = Color.cyan;
        for (int i = -n; i < n; i++) {
          Gizmos.DrawLine(new Vector2(i, n), new Vector2(i, -n));
          Gizmos.DrawLine(new Vector2(n, i), new Vector2(-n, i));
        }

        Gizmos.color = Color.black;
        Gizmos.DrawLine(wallTail, wallHead);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(pathTail, pathHead);
        Gizmos.DrawSphere(pathHead, 0.15f);
        Vector2 intersect;
        bool collision = getLineIntersection(pathTail, pathHead, wallTail, wallHead, out intersect);

        if (!collision) {
          return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(intersect, .2f);

        //This is negative or posititive depending on orientation.
        float shortestDistToWall = distanceFromLine(wallTail, wallHead, pathHead);

        const float buffer = 1f;
        float ratio = buffer / Mathf.Abs(shortestDistToWall);

        float d = Vector2.Distance(pathHead, intersect);
        float newDistance = d - d * ratio;

        Debug.Log("d: " + d + " newDistance:" + newDistance);

        Vector2 newPos = pathHead + (pathTail - pathHead).normalized * newDistance;
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(newPos, .2f);



        //This is not necessary to calculate, just for debug:
        Vector2 perpendicular = (wallTail - wallHead).normalized;
        perpendicular = new Vector2(-perpendicular.y, perpendicular.x);

        Vector2 p3 = pathHead + perpendicular * shortestDistToWall;
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(p3, .12f);

    
      }

      //Version from here: https://www.youtube.com/watch?v=bvlIYX9cgls
      private bool getLineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 result) {
        //Notes:
        //Source: https://stackoverflow.com/a/73079842/602680
        //Use the vector representation of a line as shown. B = the base point, ie tail.
        //[x, y] = [m_x, m_y] * t + [b_x, b_y]

        //Vector2 pathMagnitude = pathTail - pathHead;
        //Vector2 wallMagnitude = wallTail - wallHead;

        //Now we are going to have two equations, one for each line.
        //(1)[x, y] = [m_x_1, m_y_1] * t + [b_x_1, b_y_1]
        //(2)[x, y] = [m_x_2, m_y_2] * s + [b_x_2, b_y_2]

        float x1 = p1.x;
        float y1 = p1.y;
        float x2 = p2.x;
        float y2 = p2.y;
        float x3 = p3.x;
        float y3 = p3.y;
        float x4 = p4.x;
        float y4 = p4.y;

        /*
        float a = (x4 - x3) * (y3 - y1) - (y4 - y3) * (x3 - x1);
        float b = (x4 - x3) * (y2 - y1) - (y4 - y3) * (x2 - x1);
        float c = (x2 - x1) * (y3 - y1) - (y2 - y1) * (x3 - x1);
        */
        //get magnitude values for P1P2
        float x21 = x2 - x1;
        float y21 = y2 - y1;

        //?
        float x31 = x3 - x1;
        float x43 = x4 - x3;

        //?
        float y31 = y3 - y1;
        float y43 = y4 - y3;

        float a = x43 * y31 - y43 * x31;
        float b = x43 * y21 - y43 * x21;
        float c = x21 * y31 - y21 * x31;

        if (b == 0) {
          //Lines are parallel and do not intersect
          if (a == 0) {
            //The lines are collinear
          }
          result = Vector2.zero;
          return false;
        }

        //The scalar that tells us how much to multiply the magnitude of the vector by to get our result.
        float alpha = a / b;

        //float x0 = x1 + alpha * (x2 - x1);
        //float y0 = y1 + alpha * (y2 - y1);

        //result = new Vector2(x0, y0);
        result = p1 + alpha * (p2 - p1);

        if (alpha > 0 && alpha <= 1) {
          float beta = c / b;
          if (beta > 0 && beta <= 1) {
            //If both scalars of the magnitude are between 0 and 1 they are between the two points
            //used to define both line segments
            return true;
          }
        }
        return false;
      }

      //From: https://stackoverflow.com/a/77928107/602680
      private float distanceFromLine(Vector2 tail, Vector2 head, Vector2 p) {
		//float val = (tail.y - p.y) * (head.x - tail.x) - (tail.x - p.x) * (head.y - tail.y);

		//For line p1p2, get the shortest distance to p3.
		float tailX = tail.x;
        float headX = head.x;
        float pX = p.x;
        float tailY = tail.y;
        float headY = head.y;
        float pY = p.y;
        float segmentDX = headX - tailX;
        float segmentDY = headY - tailY;
        float pTailDX = pX - tailX;
        float pTailDY = pY - tailY;
        float closest_distance = (segmentDX * pTailDY - pTailDX * segmentDY) / Mathf.Sqrt(segmentDX * segmentDX + segmentDY * segmentDY);
        return closest_distance;
      }

    }
