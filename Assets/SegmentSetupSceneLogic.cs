using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentSetupSceneLogic : MonoBehaviour {

	[SerializeField]
	private ZoneRegion.Segment testSegment;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.white;
		
		Gizmos.DrawLine(testSegment.prevTail, testSegment.tail);
		Gizmos.DrawLine(testSegment.tail, testSegment.head);
		Gizmos.DrawLine(testSegment.head, testSegment.nextHead);

		float buffer = .5f;
		
		Vector2 tail = testSegment.tail;
		Vector2 head = testSegment.head;

		Vector2 prevTail = testSegment.prevTail;
		Vector2 nextHead = testSegment.nextHead;

		Vector2 segmentTangent = (testSegment.head - testSegment.tail).normalized;
		//Vector2 segmentNormal = new Vector2(testSegment.head.y - testSegment.tail.y, -testSegment.head.x + testSegment.tail.x).normalized;
		Vector2 segmentNormal = new Vector2(segmentTangent.y, -segmentTangent.x);

		Vector2 segmentBufferMidpoint = ((testSegment.tail + testSegment.head) / 2) + buffer * segmentNormal;

		Gizmos.DrawWireSphere(segmentBufferMidpoint, buffer);

		//If the orientation value is positive, then there is a clockwise relationship between these points.
		//In other words, the previous segment creates a concave corner with the current segment.
		float orientationValue = (prevTail.y - head.y) * (tail.x - prevTail.x) - (prevTail.x - head.x) * (tail.y - prevTail.y);
		if (orientationValue > 0) {
			Vector2 prevSegmentBufferIntersection = getCornerBufferPoint(prevTail, tail, segmentBufferMidpoint, segmentTangent, buffer);
			/*
			float distanceFromPrevSegment = CollisionUtil.distanceFromLine(prevTail, tail, segmentBufferMidpoint);
			Debug.Log("distanceFromPrevSegment: " + distanceFromPrevSegment);
			
			Vector2 prevSegmentBufferIntersection;
			if (distanceFromPrevSegment != 0) {
				Vector2 prevSegmentTangent;
				Vector2 prevSegmentNormal;
				Vector2 prevSegmentClosestPoint;

				prevSegmentTangent = (tail - prevTail).normalized;
				prevSegmentNormal = new Vector2(prevSegmentTangent.y, -prevSegmentTangent.x);
				prevSegmentClosestPoint = segmentBufferMidpoint + distanceFromPrevSegment * prevSegmentNormal;
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(prevSegmentClosestPoint, .11f);

				Gizmos.color = Color.white;
				
				CollisionUtil.getLineIntersection(prevTail, tail, segmentBufferMidpoint, segmentBufferMidpoint - segmentTangent, out prevSegmentBufferIntersection);
				
				float ratio = buffer / distanceFromPrevSegment;
				float d = Vector2.Distance(segmentBufferMidpoint, prevSegmentBufferIntersection);
				float newDistance = d * ratio;

				Debug.Log("Ratio: " + ratio);
				if (ratio < 0) {
					prevSegmentBufferIntersection -= newDistance * segmentTangent;
				} else {
					prevSegmentBufferIntersection += newDistance * segmentTangent;
				}
			} else {
				//Edge case where the midpoint is actually on the previous segment.
				//I figured this part out by eye.
				float d = Vector2.Distance(segmentBufferMidpoint, tail);
				prevSegmentBufferIntersection = segmentBufferMidpoint + d * segmentTangent;

				//float validation = CollisionUtil.distanceFromLine(prevTail, tail, prevSegmentBufferIntersection);
				//Debug.Log("validation: " + validation + " buffer: " + buffer);
			}
			*/
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(prevSegmentBufferIntersection, buffer);
			//TODO: cache this point.
			//Have a flag for concave.
		} else {
			Debug.Log("orientation is negative: " + orientationValue);
		}

		orientationValue = (nextHead.y - head.y) * (tail.x - nextHead.x) - (nextHead.x - head.x) * (tail.y - nextHead.y);
		if (orientationValue > 0) {
			//Vector2 nextSegmentBufferIntersection;
			//CollisionUtil.getLineIntersection(head, nextHead, segmentBufferMidpoint, segmentBufferMidpoint + segmentTangent, out nextSegmentBufferIntersection);
			//Gizmos.DrawWireSphere(nextSegmentBufferIntersection, .1f);
			//TODO: cache this point.
			//Vector2 nextSegmentBufferIntersection = getCornerBufferPoint(head, nextHead, segmentBufferMidpoint, segmentTangent, buffer);
			Vector2 nextSegmentBufferIntersection = getCornerBufferPoint(head, nextHead, segmentBufferMidpoint, -segmentTangent, buffer);
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(nextSegmentBufferIntersection, buffer);
		}



		//float distance = Mathf.Abs(orientationValue / Vector2.Distance(prevTail, tail));

		//Debug.Log("orientation: " + orientationValue + " distance from buffer midpoint: " + distance);
	}

	/// <summary>
	/// Find the corner buffer with the sibling/adjacent segment.
	/// This point will be buffer distance away from both segments.
	/// </summary>
	/// <param name="tail">The tail of the adjacent segment</param>
	/// <param name="head">The head of the adjacent segment</param>
	/// <param name="segmentBufferMidpoint">The point that is buffer distance away from the current segment</param>
	/// <param name="segmentTangent">The direction of the segment (in relation to the adjacent segment)</param>
	/// <param name="buffer">The distance the corner buffer must also have from the adjacent segment</param>
	/// <returns></returns>
	private static Vector2 getCornerBufferPoint(Vector2 tail, Vector2 head, Vector2 segmentBufferMidpoint, Vector2 segmentTangent, float buffer) {
		float distanceFromPrevSegment = CollisionUtil.distanceFromLine(tail, head, segmentBufferMidpoint);
		Vector2 prevSegmentBufferIntersection;
		if (distanceFromPrevSegment == 0) {
			//Edge case where the midpoint is actually on the previous segment.
			//I figured this part out by eye.
			float d = Vector2.Distance(segmentBufferMidpoint, head);
			prevSegmentBufferIntersection = segmentBufferMidpoint + d * segmentTangent;

			//float validation = CollisionUtil.distanceFromLine(prevTail, tail, prevSegmentBufferIntersection);
			//Debug.Log("validation: " + validation + " buffer: " + buffer);
		} else { 
			CollisionUtil.getLineIntersection(tail, head, segmentBufferMidpoint, segmentBufferMidpoint - segmentTangent, out prevSegmentBufferIntersection);

			float ratio = buffer / distanceFromPrevSegment;
			float d = Vector2.Distance(segmentBufferMidpoint, prevSegmentBufferIntersection);
			float newDistance = d * ratio;

			//Debug.Log("Ratio: " + ratio);
			//Depending on which side of the adjacent line the segment midpoint falls, we either add or subtract its distance.
			//This is a bit of an edge-case.
			if (ratio < 0) {
				prevSegmentBufferIntersection -= newDistance * segmentTangent;
			} else {
				prevSegmentBufferIntersection += newDistance * segmentTangent;
			}
		}
		return prevSegmentBufferIntersection;
	}
}