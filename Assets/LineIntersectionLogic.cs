using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(LineRenderer))]
public class LineIntersectionLogic : MonoBehaviour, IPointerClickHandler {

	[System.Serializable]
	public struct LineSegment {
		public Vector2 start;
		public Vector2 end;
	}

	[SerializeField]
	private GameObject player;

	private GameObject indicator;
	private Vector3 playerDestination;

	[SerializeField]
	private List<Vector2> points;

	private List<LineRenderer> lineRenderers = new List<LineRenderer>();

	private LineRenderer lineRenderer;

	private Camera mainCamera;

    // Start is called before the first frame update
    void Start() {
		playerDestination = player.transform.localPosition;

		mainCamera = Camera.main;
		lineRenderer = GetComponent<LineRenderer>();
		int n = points == null ? 0 : points.Count;
		if (n < 2) {
			return;
		}
		Vector3[] positions = new Vector3[n];
		for (int i = 0; i < n; i++) {
			positions[i] = new Vector3(points[i].x, 0, points[i].y);
		}
		lineRenderer.positionCount = n;
		lineRenderer.SetPositions(positions);

	}

	private float rotation = 0;
	private float speed = 0;

    // Update is called once per frame
    void Update() {
		const float velocity = 140;
		const float angluarVelocity = 180;

		rotation = 0;
		if (Input.GetKey(KeyCode.LeftArrow)) {
			rotation = -1;
		} else if (Input.GetKey(KeyCode.RightArrow)) {
			rotation = 1;
		}
		speed = 0;
		if (Input.GetKey(KeyCode.UpArrow)) {
			speed = 1;
		} else if (Input.GetKey(KeyCode.DownArrow)) {
			speed = -1;
		}

		if (rotation != 0) {
			player.transform.Rotate(Vector3.up, rotation * angluarVelocity * Time.deltaTime);
		}
		if (speed != 0) {
			moveEntity(player, player.transform.forward, speed * velocity * Time.deltaTime);
		}

		//player.transform.localPosition = playerDestination;
	}

	private void moveEntity(GameObject entity, Vector3 heading3d, float distance) {
		//Distance must always be positive for this to work.
		if (distance < 0) {
			distance *= -1;
			heading3d *= -1;
		}
		Vector3 pp = entity.transform.localPosition;
		Vector3 np = pp + heading3d * distance;

		
		Vector2 currentPos = new Vector2(pp.x, pp.z);
		Vector2 heading = new Vector2(heading3d.x, heading3d.z);
		//Vector2 nextPos = new Vector2(np.x, np.z);
		Vector2 nextPos = currentPos + heading * distance;
				
		//3d
		//bool collision = checkWallCollision(playerPos, nextPos, out collisionPoint, out collisionAngle, out collisionDot);

		//2d
		Vector2 closestCollision = Vector2.zero;
		float closestDistanceSquared = float.MaxValue;
		//bool collision = checkWallCollision2d(p1a, p2a, out collisionPoint2, out collisionAngle, out collisionDot);

		bool collision = false;
		int n = points.Count - 1;
		
		
		
		const float r = 0.25f; //radius of the circle collider
		for (int i = 0; i < n; i++) {

			Vector2 a = points[i];
			Vector2 b = points[i + 1];


			//Check collision against parallel line translated by radius:
			/*
			Vector2 ab = b - a;
			//Vector2 perpendicular = Vector2.Perpendicular(ab);
			//Vector2 perpendicular = new Vector2(-ab.y, ab.x) * -1;
			Vector2 perpendicular = new Vector2(ab.y, -ab.x);

			int orientation = getOrientation(currentPos, a, b);
			
			a = a + perpendicular.normalized * (r * orientation);
			b = b + perpendicular.normalized * (r * orientation);
			*/

			Vector2 intersection;
			if (getLineIntersection(currentPos, nextPos, a, b, out intersection)) {
				//Temporary:
				Debug.Log("Boundary met: " + intersection + " line: " + points[i] + " " + points[i + 1]);
				
				float dx = intersection.x - currentPos.x;
				float dy = intersection.y - currentPos.y;

				float distanceSquared = dx * dx + dy * dy;
				if (distanceSquared < closestDistanceSquared) {
					closestCollision = intersection;
					closestDistanceSquared = distanceSquared;
				}
				collision = true;
			}
		}
		
		
		if (collision) {
			//2d conversion:
			//collisionPoint = new Vector3(collisionPoint2.x, 0, collisionPoint2.y);

			//entity.transform.localPosition = collisionPoint;

			//Buffer
			//This is a very hacky way to do it.
			//Vector2 modifiedCollisionPoint2 = closestCollision - heading * 0.01f;
			closestCollision -= heading * 0.01f;

			//entity.transform.localPosition = new Vector3(collisionPoint2.x, 0, collisionPoint2.y) - heading3d * 0.01f;
			//entity.transform.localPosition = new Vector3(modifiedCollisionPoint2.x, 0, modifiedCollisionPoint2.y);
			entity.transform.localPosition = new Vector3(closestCollision.x, 0, closestCollision.y);

			//float angle = Vector3.Angle(heading, collisionPoint);
			//Debug.Log("Angle: " + angle);

			//Pre-buffered result here:
			//entity.transform.localPosition = playerDestination = collisionPoint;

			//A dot of 0 means the lines are perpendicular (and crossing).
			//-1 means they are opposite direction
			//+1 means they are the same direction
		} else {
			entity.transform.localPosition = np;
		}

		//s += " player pos: " + player.transform.localPosition;
			
		
	}

	/*
	private bool checkWallCollision(Vector2 p1, Vector2 p2, float radius, Vector2 a, Vector2 b, out Vector2 collisionPoint, out float collisionAngle, out float collisionDot) {

		Vector2 ab = b - a;
		Vector2 perpendicular = Vector2.Perpendicular(ab).normalized;

		//Vector2 ap = a + perpendicular * radius;
		//Vector2 bp = b + perpendicular * radius;

		Vector2 ap = a;
		Vector2 bp = b;

		Vector2 intersection;

		if (getLineIntersection(p1, p2, ap, bp, out intersection, out collisionAngle, out collisionDot)) {
			//Temporary:
			//Debug.Log("Boundary met: " + intersection + " line: " + points[i] + " " + points[i + 1]);
			Debug.Log("Boundary met: " + intersection + " line: " + a + " " + b);
			//playerDestination = new Vector3(intersection.x, 0, intersection.y);
			//player.transform.localPosition = playerDestination;
			collisionPoint = new Vector2(intersection.x, intersection.y);
			return true;
		} else {
			collisionPoint = Vector2.zero;
			return false;
		}


		//To rotate a 2D vector by 90deg clockwise, you can multiply the X component of the vector by -1 and then swap the X and Y values

		//Need to account for whether above or below boundary.
		//Vector2 heading = p4 - p3;
		//heading = new Vector2(heading.y, -heading.x);
		//result = result - heading.normalized * 0.5f;

	}
	*/

	//From: https://www.geeksforgeeks.org/orientation-3-ordered-points/
	private int getOrientation(Vector2 p, Vector2 tail, Vector2 head) {
		// To find orientation of ordered triplet 
		// (p1, p2, p3). The function returns 
		// following values 
		// 0 --> p, q and r are collinear
		// 1 --> Clockwise
		// -1 --> Counterclockwise
		
			
		float val = (tail.y - p.y) * (head.x - tail.x) - (tail.x - p.x) * (head.y - tail.y);

		if (val == 0) return 0; // collinear

		//clockwise or counterclock wise
		return (val > 0) ? 1 : -1;
	}

	private bool checkWallCollision2d(Vector2 p1, Vector2 p2, out Vector2 collisionPoint, out float collisionAngle, out float collisionDot) {
		collisionAngle = 0;
		collisionDot = 0;
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

	private float getDistanceSquared(float x1, float y1, float x2, float y2) {
		float dx = x2 - x1;
		float dy = y2 - y1;
		return dx * dx + dy * dy;
	}

	private bool checkWallCollision(Vector3 p1, Vector3 p2, out Vector3 collisionPoint, out float collisionAngle, out float collisionDot) {
		collisionAngle = 0;
		collisionDot = 0;
		int n = points.Count - 1;
		string s = "";
		Vector2 intersection;
		//Just overload this method.
		Vector2 p1a = new Vector2(p1.x, p1.z);
		Vector2 p2a = new Vector2(p2.x, p2.z);
		for (int i = 0; i < n; i++) {
			if (getLineIntersection(p1a, p2a, points[i], points[i + 1], out intersection)) {
				//Temporary:
				Debug.Log("Boundary met: " + intersection + " line: " + points[i] + " " + points[i + 1]);
				//playerDestination = new Vector3(intersection.x, 0, intersection.y);
				//player.transform.localPosition = playerDestination;
				collisionPoint = new Vector3(intersection.x, 0, intersection.y);
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
		collisionPoint = Vector3.zero;
		return false;
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

			//Attempt to calculate a buffer.
			//To rotate a 2D vector by 90deg clockwise, you can multiply the X component of the vector by -1 and then swap the X and Y values

			//Need to account for whether above or below boundary.
			//Vector2 heading = p4 - p3;
			//heading = new Vector2(heading.y, -heading.x);
			//result = result - heading.normalized * 0.5f;

			

			return true;
		} else {
			result = Vector2.zero;
			return false;
		}
	}


	public void OnPointerClick(PointerEventData eventData) {
		Ray ray = mainCamera.ScreenPointToRay(eventData.position);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			/*
			if (indicator == null) {
				GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
				indicator = sphere;
			}
			*/
			Vector3 worldPos = hit.point;
			//indicator.transform.position = hit.point;

			player.transform.localPosition = transform.InverseTransformPoint(worldPos);
			//playerDestination = transform.InverseTransformPoint(worldPos);

		}

		
	}


	/*
	
	
	*/
}
