using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ZoneRegion {
	
	private Segment[] segments;

	private Rect _bounds;

	private Vector2[] _points;
	public Vector2[] points {
		get => _points;
		set { 
			if (value.Length < 3) {
				throw new ArgumentException("ZoneRegion.points must have a length of at least 3.");
			}
			_points = value; 
			validate(); 
		}
	}

	private void validate() {
		_points = sortPoints(_points);
		_bounds = getBounds(_points);
	}

	private static Rect getBounds(Vector2[] points) {
		float minX, minY, maxX, maxY;
		minX = maxX = points[0].x;
		minY = maxY = points[0].y;
		for (int i = 1; i < points.Length; i++) {
			Vector2 p = points[i];
			if (p.x < minX) {
				minX = p.x;
			} else if (p.x > maxX) {
				maxX = p.x;
			}
			if (p.y < minY) {
				minY = p.y;
			} else if (p.y > maxY) {
				maxY = p.y;
			}
		}
		return new Rect(minX, minY, maxX - minX, maxY - minY);
	}

	private static Vector2 getCentroid(Vector2[] points) {
		Vector2 centroid = new Vector2();
		for (int i = 0; i < points.Length; i++) {
			centroid += points[i];
		}
		centroid /= points.Length;
		return centroid;
	}

	private static Vector2[] sortPoints(Vector2[] points) {
		Vector2 center = getCentroid(points);

		Array.Sort(points, (a, b) => {
			if (a.x - center.x >= 0 && b.x - center.x < 0) {
				return -1;
			} else if (a.x - center.x < 0 && b.x - center.x >= 0) {
				return 1;
			} else if (a.x - center.x == 0 && b.x - center.x == 0) {
				if (a.y - center.y >= 0 || b.y - center.y >= 0) {
					return a.y > b.y ? 1 : -1;
				}
				return b.y > a.y ? -1 : 1;
			}

			// compute the cross product of vectors (center -> a) x (center -> b)
			float det = (a.x - center.x) * (b.y - center.y) - (b.x - center.x) * (a.y - center.y);
			if (det < 0) {
				return -1;
			} else if (det > 0) {
				return 1;
			}

			// points a and b are on the same line from the center
			// check which point is closer to the center
			float d1 = (a.x - center.x) * (a.x - center.x) + (a.y - center.y) * (a.y - center.y);
			float d2 = (b.x - center.x) * (b.x - center.x) + (b.y - center.y) * (b.y - center.y);
			return d1 > d2 ? -1 : 1;
		});
		return points;
	}

	[Serializable]
	public struct Segment {

		//TODO: add info about whether the start and/or end are passable (convex with their neighbor)
		//This will be used to determine whether movement continues or stops once passed by player.

		public Vector2 tail;
		public Vector2 head;

		public Vector2 prevTail; //The tail of the previous segment to this one. It's head being this tail.
		public Vector2 nextHead; //The head of the segment following this one. It's tail being this head.

		public Segment(Vector2 tail, Vector2 head, Vector2 prevTail, Vector2 nextHead) {
			this.tail = tail;
			this.head = head;
			this.prevTail = prevTail;
			this.nextHead = nextHead;
		}
	}

	public void test() {
		//Test 1:
		/*
		points = new Vector2[] {
			new Vector2(76, 113),
			new Vector2(225, 113),
			new Vector2(225, 225),
			new Vector2(375, 225),
			new Vector2(375, 338.5f),
			new Vector2(76, 338.5f)
			//new Vector2(76, 113)
		};
		*/

		//Negative area (clockwise render):
		
		points = new Vector2[] {
			new Vector2(-2, 0),
			new Vector2(0, 1),
			new Vector2(2, 0),
			new Vector2(3, 0),
			new Vector2(2, -1),
			new Vector2(-2, -2)
		};
		

		/*
		//Postive area (counter-clockwise render)
		points = new Vector2[] {
			new Vector2(-2, 0),
			new Vector2(0, 2),
			new Vector2(2, 0),
			//new Vector2(3, 0),
			//new Vector2(0, 1),
			//new Vector2(-2, 0)
		};
		*/

		//sqaure:
		/*
		points = new Vector2[] {
			new Vector2(0, 0),
			new Vector2(2, 0),
			new Vector2(2, 2),
			new Vector2(0, 2)
		};
		*/

		/*
		points = new Vector2[] {
			new Vector2(0, 0),
			new Vector2(2, 0),
			//new Vector2(2, 2),
			new Vector2(0, 2)
		};
		*/

		Debug.Log("Area: " + getArea());
	}

	

	public Segment[] getSegments() {
		if (segments == null) {
			segments = new Segment[points.Length];
			int n = points.Length - 1;
			//string s = "n: " + n + "\n";

			for (int i = 0; i < n; i++) {
				segments[i] = new Segment();
				segments[i].tail = points[i];
				segments[i].head = points[i + 1];
			}
			segments[n] = new Segment();
			segments[n].tail = points[n];
			segments[n].head = points[0];

			//This can be done more efficiently, but for current readability use a second loop.
			//Iterate over each segment and set up its next/prev pos.
			n = segments.Length;
			for (int i = 0; i < n; i++) {
				int prevTailIndex = i - 1;
				int nextHeadIndex = i + 2;

				if (prevTailIndex < 0) {
					prevTailIndex += n;
				}
				if (nextHeadIndex >= n) {
					nextHeadIndex -= n;
				}
				segments[i].prevTail = segments[prevTailIndex].tail;
				segments[i].nextHead = segments[nextHeadIndex].head;
			}


		}
		return segments;
	}

	//Source: https://www.youtube.com/watch?v=8MpiVWWqXAo
	public float getArea() {
		float sum = 0;
		int n = points.Length - 1;
		//string s = "n: " + n + "\n";

		for (int i = 0; i < n; i++) {
			sum += points[i].x * points[i + 1].y - points[i + 1].x * points[i].y;
		}
		//close the last point with the first
		sum += points[n].x * points[0].y - points[0].x * points[n].y;
		//Debug.Log(s + "sum: " + (sum / 2));
		//sum += points[n].x * points[0].y - points[0].x * points[n].y;

		Debug.LogWarning("Area sum is: " + (sum / 2));

		return sum / 2;
	}
	//Area: 50624.5
	//If the area is positive it is clockwise? Need to test against the 3d coord system.
	//Note: in 3d space, where y is mapped to z, positive is actually counter-clockwise
}