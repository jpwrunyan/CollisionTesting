using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionUtil {


	/// <summary>
	/// From: https://stackoverflow.com/a/77928107/602680
	/// </summary>
	/// <param name="tail"></param>
	/// <param name="head"></param>
	/// <param name="p"></param>
	/// <returns></returns>
	public static float distanceFromLine(Vector2 tail, Vector2 head, Vector2 p) {
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

	/// <summary>
	///Version from here: https://www.youtube.com/watch?v=bvlIYX9cgls 
	/// </summary>
	/// <param name="p1"></param>
	/// <param name="p2"></param>
	/// <param name="p3"></param>
	/// <param name="p4"></param>
	/// <param name="result"></param>
	/// <returns></returns>
	public static bool getLineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 result) {
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
		
		if (b == 0) {
			if (a == 0) {
				//The lines are collinear
				result = Vector2.one;
			} else {
				//Lines are parallel and do not intersect
				result = Vector2.zero;
			}
			return false;
		}

		//The scalar that tells us how much to multiply the magnitude of the vector by to get our result.
		//Note that either alpha or beta can be used on either segment to find the intersection point.
		float alpha = a / b;

		//float x0 = x1 + alpha * (x2 - x1);
		//float y0 = y1 + alpha * (y2 - y1);

		//result = new Vector2(x0, y0);
		result = p1 + alpha * (p2 - p1);

		if (alpha > 0 && alpha <= 1) {
			float c = x21 * y31 - y21 * x31;
			float beta = c / b;
			if (beta > 0 && beta <= 1) {
				//If both scalars of the magnitude are between 0 and 1 they are between the two points
				//used to define both line segments
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// This version will check if the pathTail is oriented clockwise with the line segment it is tested against.
	/// If the orientation is not clockwise, it will be considered a non-impact (for closed polygon borders).
	/// If the orientation is 0, that means the start point is *on* the line segment, and this will be considered an impact.
	/// Version from here: https://www.youtube.com/watch?v=bvlIYX9cgls 
	/// </summary>
	/// <param name="pathTail"></param>
	/// <param name="pathHead"></param>
	/// <param name="segTail"></param>
	/// <param name="segHead"></param>
	/// <param name="result"></param>
	/// <returns></returns>
	public static bool getOrientedLineIntersection(Vector2 pathTail, Vector2 pathHead, Vector2 segTail, Vector2 segHead, out Vector2 result, out string debug) {
		debug = "";

		//Notes:
		//Source: https://stackoverflow.com/a/73079842/602680
		//Use the vector representation of a line as shown. B = the base point, ie tail.
		//[x, y] = [m_x, m_y] * t + [b_x, b_y]

		//Vector2 pathMagnitude = pathTail - pathHead;
		//Vector2 wallMagnitude = wallTail - wallHead;

		//Now we are going to have two equations, one for each line.
		//(1)[x, y] = [m_x_1, m_y_1] * t + [b_x_1, b_y_1]
		//(2)[x, y] = [m_x_2, m_y_2] * s + [b_x_2, b_y_2]

		
		float pathTailX = pathTail.x;
		float pathTailY = pathTail.y;
		float pathHeadX = pathHead.x;
		float pathHeadY = pathHead.y;
		float segTailX = segTail.x;
		float segTailY = segTail.y;
		float segHeadX = segHead.x;
		float segHeadY = segHead.y;

		/*
        float a = (x4 - x3) * (y3 - y1) - (y4 - y3) * (x3 - x1);
        float b = (x4 - x3) * (y2 - y1) - (y4 - y3) * (x2 - x1);
        float c = (x2 - x1) * (y3 - y1) - (y2 - y1) * (x3 - x1);
        */
		//get magnitude values for P1P2
		float pathHeadMinusPathTailX = pathHeadX - pathTailX;
		float pathHeadMinusPathTailY = pathHeadY - pathTailY;

		//?
		float segTailMinusPathTailX = segTailX - pathTailX;
		float segTailMinusPathTailY = segTailY - pathTailY;

		//?
		float segHeadMinusSegTailX = segHeadX - segTailX;
		float segHeadMinusSegTailY = segHeadY - segTailY;

		//float orientationValue = (segTailMinusPathTailY) * (segHeadMinusSegTailX) - (segTailMinusPathTailX) * (segHeadMinusSegTailY);

		

		float crossProduct = segHeadMinusSegTailX * segTailMinusPathTailY - segHeadMinusSegTailY * segTailMinusPathTailX;
		float b = segHeadMinusSegTailX * pathHeadMinusPathTailY - segHeadMinusSegTailY * pathHeadMinusPathTailX;

		//float crossproduct = (pathTail.y - segTail.y) * (segHead.x - segTail.x) - (pathTail.x - segTail.x) * (segHead.y - segTail.y);



		//If crossProduct < 0, then orientation doesn't pass. Ignore this boundary; not an impact
		//If crossProduct == 0 and b == 0, it's collinear; not an impact
		//If crossProduct == 0 and b > 0, then
		//						it's an impact at pathTail if pathTail is on segment
		//If crossProduct == 0 and b < 0, then it's starting at the line and moving away; not an impact
		//If crossProduct > 0, then test intersection
		// To successfully compensate for floating point math, we need a suitably small number to substitute for 0.
		if (Mathf.Abs(crossProduct) < 1.0E-06) {
			//Tantamount to crossProduct == 0
			//The start point lies on the segment line.
			debug += "orientation of start point (crossProduct): " + crossProduct + " within e " + float.Epsilon + " - orientation of b: " + b;
			if (b > 0) {
				//Reference: https://stackoverflow.com/questions/328107/how-can-you-determine-a-point-is-between-two-other-points-on-a-line-segment
				//float pathTailMinusSegTailX = pathTailX - segTailX;
				//float pathTailMinusSegTailY
				//if (pathTailX - segHeadX)
				//float d2 = segTailMinusPathTailX * segTailMinusPathTailX + segTailMinusPathTailY * segTailMinusPathTailY;
				//Debug.Log("d2: " + d2);

				//crossproduct = (pathTail.y - segTail.y) * (segHead.x - segTail.x) - (pathTail.x - segTail.x) * (segHead.y - segTail.y)


				//float dotproduct = (pathTail.x - segTail.x) * (segHead.x - segTail.x) + (pathTail.y - segTail.y) * (segHead.y - segTail.y);
				float dotproduct = -(segTailMinusPathTailX * segHeadMinusSegTailX + segTailMinusPathTailY * segHeadMinusSegTailY);


				//float squaredlengthba = (segHead.x - segTail.x) * (segHead.x - segTail.x) + (segHead.y - segTail.y) * (segHead.y - segTail.y);
				float segLenSqr = segHeadMinusSegTailX * segHeadMinusSegTailX + segHeadMinusSegTailY * segHeadMinusSegTailY;
					
				debug += "\ndoes not lie on segment if - dotproduct: " + dotproduct + " squaredLengthBA: " + segLenSqr + " either dotproduct < 0 || dotproduct > squaredlengthba: " + (dotproduct < 0) + " || " + (dotproduct > segLenSqr);
				if (dotproduct < 0 || dotproduct > segLenSqr) {
					debug += " - no collision";
					result = pathHead;
					return false;
				} else {
					debug += " - collision";
					result = pathTail;
					return true;
				}
			} else {
				//If b == 0, the path is collinear with the line; not an impact.
				//If b < 0, then the path is moving away from the line; not an impact
				result = pathHead;
				return false;
			}
			
		} else if (crossProduct < 0) {
			//Only test clockwise orientation.
			//This is not an impact since the start of the path is on the "wrong side" of the wall.
			result = pathHead;
			return false;
		} else {
			//crossProduct > 0
			//Test for traditional segment intersection:
			//The scalar that tells us how much to multiply the magnitude of the vector by to get our result.
			//Note that either alpha or beta can be used on either segment to find the intersection point.
			debug += "orientation of start point (crossProduct): " + crossProduct + " > 0 - orientation of b: " + b;
			float alpha = crossProduct / b;

			if (alpha > 0 && alpha <= 1) {
				float c = pathHeadMinusPathTailX * segTailMinusPathTailY - pathHeadMinusPathTailY * segTailMinusPathTailX;
				float beta = c / b;
				if (beta > 0 && beta <= 1) {
					//If both scalars of the magnitude are between 0 and 1 they are between the two points
					//used to define both line segments
					result = pathTail + alpha * (pathHead - pathTail);
					return true;
				}
			}
			result = pathHead;
			return false;
		}
	}
}
