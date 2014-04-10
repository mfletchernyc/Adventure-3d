using UnityEngine;
using System.Collections;

public class Cam : MonoBehaviour {

	// Check for objects between the camera and player. Move the camera if necessary.

	// Every map area has default and alt camera positions. Default looks best for the
	// area. Alt prevents any collisions with area-specific objects. (X is always 0.)

	private Transform cam;
	private GameObject adventurer;
	private Adventurer Adventurer;
	
	private Transform target;			// The camera actually looks at a point above the player.
	private GameObject sensor;			// Raycasts to check new camera position for obstruction.
	private float distance;				// Distance from adventurer to sensor for raycasting.
	private Vector3 direction;			// Direction from adventurer to sensor for raycasting.
	private bool obstruction;			// Flag for camera obstruction.
	private Vector3 previousPosition;	// Fallback position if moving the camera causes an obstruction.
	
	private float defaultY;		
	private float defaultZ;
	private float altY;
	private float altZ;
	private float deltaY;
	private float deltaZ;
	private float previousY;
	private float previousZ;
	
	void Awake () {
		cam = gameObject.transform;

		defaultY = 4f;
		defaultZ = -5f;
		altY = 6f;
		altZ = -.75;

		FindCamDeltas();
	}
	
	void Start () {
		sensor = GameObject.Find("sensor");						// Maintains current default camera position.
		target = GameObject.Find("camera target").transform;	// Looking at a point above and in front of the player looks better.
		adventurer = GameObject.Find("adventurer");
//		Adventurer = adventurer.GetComponent<Adventurer>();
	}
	
	void Update () {
		// Rays cast from inside an object don't register hits. Camera obstruction is checked
		// by casting a ray from the player position to a sensor in the camera's position.
		// This avoids camera bounce and accounts for the camera target not being the player.

		// Reset the sensor to current camera position
		sensor.transform.localPosition = cam.localPosition;
		sensor.transform.LookAt(adventurer.transform);

		if (ObstructedSensor()) { 
			// Move the sensor to alt position to clear the obstruction.
			previousY = sensor.transform.localPosition.y + deltaY;
			previousZ = sensor.transform.localPosition.z + deltaZ;

			if (previousY < altY || previousZ + deltaZ < altZ) {
				sensor.transform.localPosition = new Vector3(0f, previousY, previousZ);
			} else {
				sensor.transform.localPosition = new Vector3(0f, altY, altZ);
			}
		}

		else {
			// Move the camera toward the default position (if necessary and possible).
			previousPosition = sensor.transform.localPosition;

			previousY = sensor.transform.localPosition.y - deltaY;
			previousZ = sensor.transform.localPosition.z - deltaZ;

			if (previousY > defaultY || previousZ + deltaZ > defaultZ) {
				sensor.transform.localPosition = new Vector3(0f, previousY, previousZ);
			} else {
				sensor.transform.localPosition = new Vector3(0f, defaultY, defaultZ);
			}

			// Restore sensor position if new position will cause bouncing.
			sensor.transform.LookAt(adventurer.transform);
			if (ObstructedSensor()) {
				sensor.transform.localPosition = previousPosition;
			}
		}

		// Put the camera in position.
		cam.localPosition = sensor.transform.localPosition;
		cam.LookAt(target);
	}

	void FindCamDeltas() {
		deltaY = (altY - defaultY) / 25f;
		deltaZ = (altZ - defaultZ) / 25f;
	}

	bool ObstructedSensor() {
		distance = Vector3.Distance(sensor.transform.position, adventurer.transform.position);
		direction = sensor.transform.TransformDirection(Vector3.back) * distance;
		obstruction = Physics.Raycast(adventurer.transform.position, direction, distance);

		Color testRayColor = obstruction ? Color.red : Color.grey;
		Debug.DrawRay(adventurer.transform.position, direction, testRayColor);

		return obstruction;
	}
}




		// Use an OnTriggerEnter event to set local camera positions.





//		// Keep the camera from moving into objects.
//		Vector3 direction = sensor.transform.TransformDirection(Vector3.back);
//
//		if (!Adventurer.barbican) {
//			// iTween movement interferes with barbican movement.
//			if (Physics.Raycast(adventurer.transform.position, direction, distance)) {
//				cam.localPosition = new Vector3(0f, 6f, -.75f);
//				//iTween.MoveTo(gameObject, iTween.Hash("position", new Vector3(0f, 6f, -.75f), "time", 0.5f, "islocal", true));
//			} else {
//				cam.localPosition = new Vector3(0f, positionY, positionZ);
//				//iTween.MoveTo(gameObject, iTween.Hash("position", new Vector3(0f, positionY, positionZ), "time", 0.5f, "islocal", true));
//			}
//
//			cam.LookAt(target);
//		}