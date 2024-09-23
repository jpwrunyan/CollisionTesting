using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetectionPOC2Logic : MonoBehaviour {
	const float velocity = 1.5f;
	const float angluarVelocity = 180;
	[SerializeField]
	private GameObject testObject;

	[SerializeField]
	private Vector2 pathTail;

	[SerializeField]
	private Vector2 pathHead;

	[SerializeField]
	private Segment[] segments;

	// Start is called before the first frame update
	void Start() {
		//testObject.transform.forward = Vector3.up;

		LineRenderer lineRenderer = GetComponent<LineRenderer>();
		int n = segments.Length;
		Vector3[] positions = new Vector3[n + 1];
		positions[0] = new Vector3(segments[0].tail.x, segments[0].tail.y);
		for (int i = 0; i < n; i++) {
			positions[i + 1] = new Vector3(segments[i].head.x, segments[i].head.y);
		}
		lineRenderer.positionCount = positions.Length;
		lineRenderer.SetPositions(positions);
	}

	bool checkNextMove = false;

	// Update is called once per frame
	void Update() {
		//testObject.transform.position += Time.deltaTime * 1.5f * testObject.transform.forward;
		int speed = getInput();
		if (speed != 0) {
			Vector2 dest = testObject.transform.position + Time.deltaTime * speed * velocity * testObject.transform.up;
			if (checkNextMove) {
				Debug.LogWarning("check next move: " + testObject.transform.position + " dest: " + dest);
				checkNextMove = false;
			}
			int start = 0; // segments.Length - 1;
			for (int i = start; i < segments.Length; i++) {
				Vector2 segmentTail = segments[i].tail;
				Vector2 segmentHead = segments[i].head;

				Vector2 intersection;
				//It is possible to have more than one collision for close lines or large path.
				//However, I'm not particularly worried about this edge case.
				string s;
				bool collision = CollisionUtil.getOrientedLineIntersection(testObject.transform.position, dest, segmentTail, segmentHead, out intersection, out s);
				if (!string.IsNullOrEmpty(s)) {
					if (collision) {
						Debug.LogWarning(s + " move from: " + testObject.transform.position + " to dest: " + intersection);
						checkNextMove = true;
					} else {
						Debug.Log(s);
					}
				}
				if (collision) {
					//Gizmos.DrawWireSphere(intersection, .1f);
					dest = intersection;
					break;
				}
			}
			testObject.transform.position = dest;
		}
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.white;
		foreach (Segment segment in segments) {
			Gizmos.DrawLine(segment.tail, segment.head);
			//Gizmos.DrawSphere(segment.tail, .05f);
			//float d = Vector2.Distance(segment.tail, segment.head) - .1f;
			Vector2 tangent = (segment.head - segment.tail).normalized;
			Vector2 normal = new Vector2(tangent.y, -tangent.x);
			Gizmos.DrawLine(segment.head, segment.head - (tangent * .1f) + (normal * .1f));
		}

		Gizmos.color = Color.red;
		Gizmos.DrawLine(pathTail, pathHead);
		Gizmos.DrawSphere(pathTail, .05f);

		for (int i = segments.Length - 1; i < segments.Length; i++) {
			Vector2 head = segments[i].head;
			Vector2 tail = segments[i].tail;

			Vector2 intersection;
			//It is possible to have more than one collision for close lines or large path.
			//However, I'm not particularly worried about this edge case.
			string s;
			bool collision = CollisionUtil.getOrientedLineIntersection(pathTail, pathHead, tail, head, out intersection, out s);
			if (s != "") {
				//Debug.Log(s);
			}
			if (collision) {
				Gizmos.DrawWireSphere(intersection, .1f);
			}
		}

		

		//Debug.Log("orientationValue: " + orientationValue);
	}

	
	private int getInput() {
		int rotation = 0;
		if (Input.GetKey(KeyCode.LeftArrow)) {
			rotation = 1;
		} else if (Input.GetKey(KeyCode.RightArrow)) {
			rotation = -1;
		}
		
		int speed = 0;
		
		if (Input.GetKey(KeyCode.UpArrow)) {
			speed = 1;
		} else if (Input.GetKey(KeyCode.DownArrow)) {
			speed = -1;
		}
		
		/*
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			speed = 1;
		} else if (Input.GetKeyDown(KeyCode.DownArrow)) {
			speed = -1;
		}
		*/

		if (rotation != 0) {
			testObject.transform.Rotate(Vector3.forward, rotation * angluarVelocity * Time.deltaTime);
		}
		/*
		if (speed != 0) {
			//moveEntity(player, player.transform.forward, speed * velocity * Time.deltaTime);

			//player.transform.position += player.transform.forward * speed * velocity * Time.deltaTime;

		}
		*/
		return speed;
	}

	[Serializable]
	public struct Segment {
		public Vector2 tail;
		public Vector2 head;
	}
}
