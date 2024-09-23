using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneSetupSceneLogic : MonoBehaviour {

	[SerializeField]
	private Material wallMaterial;

    // Start is called before the first frame update
    void Start() {
		Vector2[] points = new Vector2[] {
			new Vector2(-2, 0),
			new Vector2(0, 1),
			new Vector2(2, 0),
			new Vector2(3, 0),
			new Vector2(2, -1),
			new Vector2(-2, -2)
		};

		ZoneRegion zr = new ZoneRegion();
		zr.points = points;
		
		/*
		ZoneRegion.Segment[] segments = new ZoneRegion.Segment[] {
			new ZoneRegion.Segment(new Vector2(-1, 0), new Vector2(1, 0)),
			new ZoneRegion.Segment(new Vector2(2, 0), new Vector2(-1, 0))
		};
		*/
		initialRender(zr.getSegments());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void initialRender(ZoneRegion.Segment[] segments) {
		const float height = 0.5f;
		for (int i = 0; i < segments.Length; i++) {
			Vector2 heading = (segments[i].head - segments[i].tail).normalized;
			float d = Vector2.Distance(segments[i].tail, segments[i].head);
			Vector3 pos = new Vector3(segments[i].tail.x, height / 2, segments[i].tail.y);

			GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
			wall.name = "Segment " + i;
			wall.transform.parent = gameObject.transform;

			//Both of these transforms work. The one to use depends on the material.

			//wall.transform.forward = new Vector3(heading.x, 0, heading.y);
			//wall.transform.localScale = new Vector3(0.25f, 0.5f, d);
			//wall.transform.localPosition = pos + wall.transform.forward * (d / 2);

			wall.transform.right = new Vector3(heading.x, 0, heading.y);
			wall.transform.Rotate(Vector3.right, 90);
			wall.transform.localScale = new Vector3(d, 0.1f, height);
			wall.transform.localPosition = pos + wall.transform.right * (d / 2);

			wall.GetComponent<Renderer>().material = wallMaterial;
			wall.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		}
		

		//GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Quad);
		//wall.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

	}
}
