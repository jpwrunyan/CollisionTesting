using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetectionPOC1Logic : MonoBehaviour {

	[SerializeField]
	private Vector2 pathTail;

	[SerializeField]
	private Vector2 pathHead;

	[SerializeField]
	private Segment[] segments;

	// Start is called before the first frame update
	void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }

	private void OnDrawGizmos() {
		Gizmos.color = Color.white;
		foreach (Segment segment in segments) {
			Gizmos.DrawLine(segment.tail, segment.head);
			Gizmos.DrawSphere(segment.tail, .05f);
		}

		Gizmos.color = Color.red;
		Gizmos.DrawLine(pathTail, pathHead);
		Gizmos.DrawSphere(pathTail, .05f);

		Vector2 head = segments[0].head;
		Vector2 tail = segments[0].tail;


		float orientationValue = (tail.y - pathTail.y) * (head.x - tail.x) - (tail.x - pathTail.x) * (head.y - tail.y);

		if (orientationValue == 0) {
			//Tail (start point) is on the line. Thus collision point is here. No further processing.
		} else if (orientationValue > 0) {

		}


		Vector2 intersection;
		//bool collision = CollisionUtil.getLineIntersection(pathTail, pathHead, tail, head, out intersection);
		
		bool collision = CollisionUtil.getOrientedLineIntersection(pathTail, pathHead, tail, head, out intersection, out _);
		if (collision) {
			Gizmos.DrawWireSphere(intersection, .1f);
		}

		//Debug.Log("orientationValue: " + orientationValue);
	}

	[Serializable]
	public struct Segment {
		public Vector2 head;
		public Vector2 tail;
	}
}
