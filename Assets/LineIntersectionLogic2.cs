using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LineIntersectionLogic2 : MonoBehaviour, IPointerClickHandler {
	const float velocity = 14;
	const float angluarVelocity = 180;

	[SerializeField]
	private Material wallMaterial;

	[SerializeField]
	private GameObject player;

	private Camera mainCamera;

	private ZoneRegion[] zoneRegions;

	// Start is called before the first frame update
	void Start() {

		//Debug.Log("test distance 1 for horizontal line1: " + distanceFromLine(new Vector2(-10, 2), new Vector2(17, 2), new Vector2(1, 1)));

		//Debug.Log("test distance 1 for horizontal line2: " + distanceFromLine(new Vector2(-10, 2), new Vector2(17, 2), new Vector2(1, 3)));

		//Debug.Log("test distance 1 for horizontal line3: " + distanceFromLine(new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 0)));

		mainCamera = Camera.main;

		ZoneRegion zr = new ZoneRegion();
		zr.test();

		//ZoneRegion zr2 = new ZoneRegion();
		//zr2.points = new Vector2[] {

		//}

		zoneRegions = new ZoneRegion[] { zr };

		initialRender();
	}

	// Update is called once per frame
	void Update() {
		int speed = getInput();

		//player.transform.position += player.transform.forward * speed * velocity * Time.deltaTime;
		if (speed != 0) {
			moveEntity(player, speed * velocity * Time.deltaTime);
		}
		
	}
	private void moveEntity(GameObject entity, float distance) {
		Vector3 heading = entity.transform.forward;
		Vector3 pos = entity.transform.localPosition;
		if (distance < 0) {
			distance *= -1;
			heading *= -1;
		}
		bool impact = false;
		Vector2 newPos = getMovement(new Vector2(pos.x, pos.z), new Vector2(heading.x, heading.z), distance, out impact);
		if (impact) {
			Debug.LogError("IMPACT WITH WALL " + newPos);
			pos = new Vector3(newPos.x, 0, newPos.y);

			//This bugger modification is so that the entity can't clip through the wall.
			//Its exact value has to be tweaked to work properly.
			//Visually, it is jittery.
			pos = new Vector3(newPos.x, 0, newPos.y) - (heading * 0.1f);
			//speed = 0;
		} else {
			pos = new Vector3(newPos.x, 0, newPos.y);
		}
		
		entity.transform.localPosition = pos;
		//entity.transform.localPosition = new Vector3(newPos.x, 0, newPos.y);
		//entity.transform.localPosition += entity.transform.forward * distance;
	}

	private Vector2 getMovement(Vector2 startPos, Vector2 heading, float distance, out bool tempBool) {
		List<ZoneRegion.Segment> segments = new List<ZoneRegion.Segment>();
		//Temp: get a list of all segments, period:
		foreach (ZoneRegion zr in zoneRegions) {
			foreach (ZoneRegion.Segment boundary in zr.getSegments()) {
				segments.Add(boundary);
			}
		}


		Vector2 pendingPos = startPos + heading * distance;
		string s = "";
		//Debug.Log("startPos: " + startPos + " pendingPos: " + pendingPos);
		s += "startPos: " + startPos + " pendingPos: " + pendingPos + "\n";

		//When an entity collides with a wall, calculate new position along that wall's vector.
		//If it collides with a second wall, calculate new position along second wall's vector.
		//Check if second modified position again collides with first wall. If so, stop.
		//This works with just two checks because movement isn't just converted to the vector of the wall.
		//It is proportionally converted so that the entity does not move in a direction farther from its chose direction.
		//So if an entity finds itself in a corner, it will freeze. That is the closest it can travel toward its desired direction.

		//First wall check:
		Vector2 closestImpact = pendingPos;
		float closestImpactDistSqr = float.MaxValue;
		ZoneRegion.Segment firstCollision = new ZoneRegion.Segment();
		foreach (ZoneRegion.Segment boundary in segments) {
			Vector2 p1 = startPos;
			Vector2 p2 = boundary.tail;
			Vector2 p3 = boundary.head;
			//From: https://www.geeksforgeeks.org/orientation-3-ordered-points/
			//float orientation = (p2.y - p1.y) * (p3.x - p2.x) - (p2.x - p1.x) * (p3.y - p2.y);
			//If orientation is: 
			// 0 --> p, q and r are collinear
			// >0 --> Clockwise
			// <0 --> Counterclockwise

			//Only check collisions on clockwise facing. In other words, if the object is *inside* the region, it will not collide.
			//Regions are intended to demarcate inaccessable sub-zones. They are not meant to create enclosed arenas.

			//TODO: implement orientation check.

			//TODO: can also implement check that if the entity is within a certain distance of the wall, it is considered a collision.

			Vector2 intersection;
			if (getLineIntersection(startPos, pendingPos, boundary.tail, boundary.head, out intersection)) {
				float dx = intersection.x - startPos.x;
				float dy = intersection.y - startPos.y;
				float distanceSqr = dx * dx + dy * dy;
				//If we're not worried about large velocities or tightly clustered walls, then we can skip this.
				//Just stop checking at the first found collision.
				if (distanceSqr < closestImpactDistSqr) {
					closestImpact = intersection;
					closestImpactDistSqr = distanceSqr;
					firstCollision = boundary;
				}
			}
		}

		if (closestImpactDistSqr == float.MaxValue) {
			tempBool = false;
			return pendingPos;
		} else {
			/*
			//Testing!
			//pendingPos -= heading * 0.05f;

			//There will be a boundary size to "push" the point away from the actual border of the line.
			//This will require figuring out how far away from the boundary the original point was.
			//Using this walk it back up the movement vector to where it is at this boundary distance.
			//Note, there is an edge case we're not testing for when the destination point crosses the boundary distance,
			//but does not actuall cross the line. We will rely on the *next* test to move the point from the offset.
			//If the boundary is too thick, this edge case will become noticeable.

			pendingPos = closestImpact;
			
			Vector2 boundaryVector = (firstCollision.end - firstCollision.start).normalized;

			float dot = Vector2.Dot(heading, boundaryVector);

			float remainingDistance = distance - Mathf.Sqrt(closestImpactDistSqr);
			Debug.Log("cutoff distance: " + remainingDistance);

			pendingPos = pendingPos + (boundaryVector * dot * remainingDistance);
			
			tempBool = true;
			return pendingPos;
			*/
		}

		/*
		pendingPos = closestImpact;


		//closestImpactDistSqr = distanceSqr;
		//firstCollision = boundary;
		Vector2 perpendicular = (firstCollision.end - firstCollision.start).normalized;
		float perpDistFromBoundary = Mathf.Abs(distanceFromLine(firstCollision.start, firstCollision.end, startPos));

		const float bufferDist = 0.2f;
		float impactDist = Mathf.Sqrt(closestImpactDistSqr);
		float bufferRatio = bufferDist / impactDist;

		pendingPos = startPos + (heading * bufferRatio * impactDist);
		*/


		/*
		//Now that we have an impact, align to the wall's vector and offset the pending position according to the dot product.
		//This "slides" the position along the wall based on its angle (perpendicular will have dot product 0 and no sliding will occur).
		pendingPos = closestImpact;
		Vector2 boundaryVector = (firstCollision.end - firstCollision.start).normalized;
		float dot = Vector2.Dot(heading, boundaryVector);
		float remainingDistance = distance - Mathf.Sqrt(closestImpactDistSqr);
		//remainingDistance *= dot;
		pendingPos = pendingPos + (boundaryVector * remainingDistance * dot);
		tempBool = true;
		*/

		float distFromLine = distanceFromLine(firstCollision.tail, firstCollision.head, startPos);
		float remainingDistance = distance - Mathf.Sqrt(closestImpactDistSqr);
		Vector2 boundaryVector = (firstCollision.head - firstCollision.tail).normalized;
		if (distFromLine < 0) {
			pendingPos = pendingPos + (boundaryVector * remainingDistance * -1);
		} else if (distFromLine > 0) {
			pendingPos = pendingPos + (boundaryVector * remainingDistance);
		} else {
			Debug.LogError("DIST FROM LINE IS 0!");
		}


		/*
		//----------------------------------------
		// Corner impact check!
		//----------------------------------------

		//Check if this pending pos collides with a second wall. If it does, simply stop.
		//We won't take into accound spiralling walls because it's too much of an edge case and
		//does not need to be handled with such precision during each frame update.
		//Logic is identical to above.
		
		bool secondCollision = false;
		//Check along this new mini-offset path. Starting from the impact and ending with the offset pending position.
		startPos = closestImpact;
		foreach (ZoneRegion.Segment boundary in segments) {
			if (boundary.Equals(firstCollision)) {
				continue;
			}
			
			Vector2 p1 = startPos;
			Vector2 p2 = boundary.start;
			Vector2 p3 = boundary.end;
			
			//TODO: implement orientation check.
			//TODO: can also implement check that if the entity is within a certain distance of the wall, it is considered a collision.

			Vector2 intersection;
			if (getLineIntersection(startPos, pendingPos, boundary.start, boundary.end, out intersection)) {
				float dx = intersection.x - startPos.x;
				float dy = intersection.y - startPos.y;
				float distanceSqr = dx * dx + dy * dy;
				//If we're not worried about large velocities or tightly clustered walls, then we can skip this.
				//Just stop checking at the first found collision.
				if (distanceSqr < closestImpactDistSqr) {
					closestImpact = intersection;
					closestImpactDistSqr = distanceSqr;
					firstCollision = boundary;
					secondCollision = true;
				}
			}
		}
		if (secondCollision) {
			pendingPos = closestImpact;
		}
		*/

		tempBool = true;
		
		return pendingPos;
	}


	//From: https://stackoverflow.com/a/77928107/602680
	private float distanceFromLine(Vector2 p1, Vector2 p2, Vector2 p3) {
		//For line p1p2, get the shortest distance to p3.
		float p1_0 = p1.x;
		float p2_0 = p2.x;
		float p3_0 = p3.x;
		float p1_1 = p1.y;
		float p2_1 = p2.y;
		float p3_1 = p3.y;
		float p2_0_minus_p1_0 = p2_0 - p1_0;
		float p2_1_minus_p1_1 = p2_1 - p1_1;
		float p3_0_minus_p1_0 = p3_0 - p1_0;
		float p3_1_minus_p1_1 = p3_1 - p1_1;
		float closest_distance = (p2_0_minus_p1_0 * p3_1_minus_p1_1 - p3_0_minus_p1_0 * p2_1_minus_p1_1) / Mathf.Sqrt(p2_0_minus_p1_0 * p2_0_minus_p1_0 + p2_1_minus_p1_1 * p2_1_minus_p1_1);
		return closest_distance;




		//closest_distance = (p2_0_minus_p1_0 * p3_1_minus_p1_1 - p3_0_minus_p1_0 * p2_1_minus_p1_1) / sqrt(p2_0_minus_p1_0 * p2_0_minus_p1_0 + p2_1_minus_p1_1 * p2_1_minus_p1_1)

		//float xba = b.x - a.x;
		//float yba = b.y - a.y;
		//float xpa = p.x - a.x;
		//float ypa = p.y - a.y;
		//float solution = xba * ypa - xpa * yba / Mathf.Sqrt(xba * xba + yba * yba);

		//return solution;

	}

	//Version from here: https://www.youtube.com/watch?v=bvlIYX9cgls
	private bool getLineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 result) {

		

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
		float x21 = x2 - x1;
		float x31 = x3 - x1;
		float x43 = x4 - x3;
		float y21 = y2 - y1;
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
		float alpha = a / b;
		float beta = c / b;
		if (alpha > 0 && alpha <= 1 && beta > 0 && beta <= 1) {
			//Two solutions possible.
			//x0 = x1 + alpha(x2 - x1) = x3 + beta(x4 - x3)
			//y0 = y1 + alpha(y2 - y1) = y3 + beta(y4 - y3)
			float x0 = x1 + alpha * (x2 - x1);
			float y0 = y1 + alpha * (y2 - y1);

			result = new Vector2(x0, y0);

			return true;
		} else {
			result = Vector2.zero;
			return false;
		}
	}

	/*
	private bool checkWallCollision(Vector2 p1, Vector2 p2, out Vector2 collisionPoint) {
		int n = points.Count - 1;
		string s = "";
		Vector2 intersection;
		//Just overload this method.

		const float r = 0.25f; //radius of the circle collider


		for (int i = 0; i < n; i++) {

			Vector2 a = points[i];
			Vector2 b = points[i + 1];

			Vector2 ab = b - a;
			//Vector2 perpendicular = Vector2.Perpendicular(ab);
			Vector2 perpendicular = new Vector2(-ab.y, ab.x) * -1;

			int orientation = getOrientation(p1, a, b);
			//Debug.Log("Check orientation: " + orientation);

			a = a + perpendicular.normalized * (r * orientation);
			b = b + perpendicular.normalized * (r * orientation);

			if (getLineIntersection(p1, p2, a, b, out intersection)) {
				//orientation = getOrientation(p1, a, b);
				//Debug.Log("orientation: " + orientation);

				//Temporary:
				Debug.Log("Boundary met: " + intersection + " line: " + points[i] + " " + points[i + 1]);
				//playerDestination = new Vector3(intersection.x, 0, intersection.y);
				//player.transform.localPosition = playerDestination;
				collisionPoint = intersection;
				return true;
			}



			//if (findSegmentIntersection(new Vector2(p1.x, p1.z), new Vector2(p2.x, p2.z), points[i], points[i + 1], out intersection)) {
			//	//Temporary:
			//	s += "Boundary met (method2): " + intersection + " line: " + points[i] + " " + points[i + 1] + "\n";
			//	//playerDestination = new Vector3(intersection.x, 0, intersection.y);
			//	//player.transform.localPosition = playerDestination;
			//}


		}
		if (s != "") {
			Debug.Log(s);

		}
		collisionPoint = Vector2.zero;
		return false;
	}
	*/

	public void OnPointerClick(PointerEventData eventData) {
		Ray ray = mainCamera.ScreenPointToRay(eventData.position);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			Vector3 worldPos = hit.point;
			player.transform.localPosition = transform.InverseTransformPoint(worldPos);
		}
	}

	//int speed = 0;
	private int getInput() {
		int rotation = 0;
		if (Input.GetKey(KeyCode.LeftArrow)) {
			rotation = -1;
		} else if (Input.GetKey(KeyCode.RightArrow)) {
			rotation = 1;
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
			player.transform.Rotate(Vector3.up, rotation * angluarVelocity * Time.deltaTime);
		}
		if (speed != 0) {
			//moveEntity(player, player.transform.forward, speed * velocity * Time.deltaTime);

			//player.transform.position += player.transform.forward * speed * velocity * Time.deltaTime;
			
		}
		return speed;
	}

	private void initialRender() {
		foreach (ZoneRegion zr in zoneRegions) {
			
		
			ZoneRegion.Segment[] segments = zr.getSegments();
			for (int i = 0; i < segments.Length; i++) {
				Vector2 heading = (segments[i].head - segments[i].tail).normalized;
				float d = Vector2.Distance(segments[i].tail, segments[i].head);
				Vector3 pos = new Vector3(segments[i].tail.x, 0, segments[i].tail.y);

				GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
				wall.name = "Segment " + i;
				wall.transform.parent = gameObject.transform;
				//wall.transform.forward = new Vector3(heading.x, 0, heading.y);
				//wall.transform.localScale = new Vector3(0.01f, 0.5f, d);
				//wall.transform.localPosition = pos + wall.transform.forward * (d / 2);
				
				wall.transform.right = new Vector3(heading.x, 0, heading.y);
				wall.transform.Rotate(Vector3.right, 90);
				wall.transform.localScale = new Vector3(d, 0.1f, 0.5f);
				wall.transform.localPosition = pos + wall.transform.right * (d / 2);

				wall.GetComponent<Renderer>().material = wallMaterial;
			}
		}

		//GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Quad);
		//wall.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

	}
}
