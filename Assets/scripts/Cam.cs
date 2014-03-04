using UnityEngine;
using System.Collections;

public class Cam : MonoBehaviour {

	private Transform cam;
	private GameObject sensor;			
	private GameObject adventurer;
	private Adventurer Adventurer;

	private float positionY;	// usual:  4	limit: 6(?)
	private float positionZ;	// usual: -5	limit: -.75
	private float rotationX;	// usual: 15	limit: 50
	
	private float distance;		// Distance from adventurer to sensor for raycasting.

	void Awake () {
		cam = gameObject.transform;

		// Store normal camera placement. 
		positionY = cam.localPosition.y;
		positionZ = cam.localPosition.z;
		rotationX = cam.eulerAngles.x;
	}
	
	void Start () {
		sensor = GameObject.Find("sensor");
		adventurer = GameObject.Find("adventurer");
		Adventurer = adventurer.GetComponent<Adventurer>();
		distance = Vector3.Distance(sensor.transform.position, adventurer.transform.position);
	}
	
	void Update () {
		// Keep the camera from moving into objects.
		Vector3 direction = sensor.transform.TransformDirection(Vector3.back);
		float advRotation = adventurer.transform.eulerAngles.y;

		if (!Adventurer.barbican) {
			// iTween movement interferes with barbican movement.
			if (Physics.Raycast(adventurer.transform.position, direction, distance)) {
				cam.localPosition = new Vector3(0f, 6f, -.75f);
				//iTween.MoveTo(gameObject, iTween.Hash("position", new Vector3(0f, 6f, -.75f), "time", 0.5f, "islocal", true));
				cam.eulerAngles = new Vector3(50f, advRotation, 0f);
			} else {
				cam.localPosition = new Vector3(0f, positionY, positionZ);
				//iTween.MoveTo(gameObject, iTween.Hash("position", new Vector3(0f, positionY, positionZ), "time", 0.5f, "islocal", true));
				cam.eulerAngles = new Vector3(rotationX, advRotation, 0f);
			}
		}
	}
}