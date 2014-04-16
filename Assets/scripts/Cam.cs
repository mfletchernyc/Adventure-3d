using UnityEngine;
using System.Collections;

// Adventure reinterpretation for Unity
// M. Fletcher 2014
// http://en.wikipedia.org/wiki/Adventure_(1979_video_game)

public class Cam : MonoBehaviour {

	// Check for objects between the camera and player. Move the camera if necessary.

	// The map area has default camera positions. 'Hero' looks best cinematically.
	// 'Safe' prevents any collisions with area-specific objects. When the camera's
	// view of the player becomes obstructed, it will move towards the safe position
	// (and vice versa). Special areas (like barbicans) change these positions.

	public float defaultHeroY, defaultHeroZ;
	public float defaultSafeY, defaultSafeZ;
	
	private Transform cam;
	private GameObject adventurer;
	
	private Transform target;			// The camera actually looks at a point above the player.
	private GameObject sensor;			// Raycasts to check camera position for obstruction.
	private float distance;				// Distance from adventurer to sensor for raycasting.
	private Vector3 direction;			// Direction from adventurer to sensor for raycasting.
	private bool obstruction;			// Flag for camera obstruction.
	private bool transition;			// Flag for camera switching position between areas.
	
	private float heroY, heroZ;
	private float safeY, safeZ;
	private float deltaY, deltaZ;
	
	void Awake () {
		cam = gameObject.transform;
		
		heroY = defaultHeroY;
		heroZ = defaultHeroZ;
		safeY = defaultSafeY;
		safeZ = defaultSafeZ;

		FindCamDeltas();
	}
	
	void Start () {
		sensor = GameObject.Find("sensor");						// Maintains current default camera position.
		target = GameObject.Find("camera target").transform;	// Looking at a point above and in front of the player looks better.
		adventurer = GameObject.Find("adventurer");
	}
	
	void Update () {
		// Rays cast from inside an object don't register hits. Camera obstruction is checked
		// by casting a ray from the player position to a sensor in the camera's position.
		// This avoids camera bounce and accounts for the camera target not being the player.

		if (!transition) {
			float updatedY, updatedZ;
			Vector3 previousPosition;

			// Reset the sensor to current camera position
			sensor.transform.localPosition = cam.localPosition;
			sensor.transform.LookAt(adventurer.transform);

			if (ObstructedSensor()) { 
				// Move the sensor to alt position to clear the obstruction.
				updatedY = sensor.transform.localPosition.y + deltaY;
				updatedZ = sensor.transform.localPosition.z + deltaZ;

				// Move the sensor toward (or into) alternate position.
				if (updatedY < safeY || updatedZ + deltaZ < safeZ) {
					sensor.transform.localPosition = new Vector3(0f, updatedY, updatedZ);
				} else { sensor.transform.localPosition = new Vector3(0f, safeY, safeZ); }
			}

			else {
				// Move the camera toward the default position (if necessary and possible).
				previousPosition = sensor.transform.localPosition;

				updatedY = sensor.transform.localPosition.y - deltaY;
				updatedZ = sensor.transform.localPosition.z - deltaZ;

				// Move the sensor toward (or into) default position.
				if (updatedY > heroY || updatedZ + deltaZ > heroZ) {
					sensor.transform.localPosition = new Vector3(0f, updatedY, updatedZ);
				} else { sensor.transform.localPosition = new Vector3(0f, heroY, heroZ); }

				// Restore sensor position if new position will cause bouncing.
				sensor.transform.LookAt(adventurer.transform);
				if (ObstructedSensor()) {
					sensor.transform.localPosition = previousPosition;
				}
			}
		}

			// Put the camera in position.
			cam.localPosition = sensor.transform.localPosition;
			cam.LookAt(target);
	}

	public void MoveCamera (string message) {
		transition = true;

		if (message == "exit") {
			heroY = defaultHeroY;
			heroZ = defaultHeroZ;
			safeY = defaultSafeY;
			safeZ = defaultSafeZ;
		}

		else {
			// Special area name is "cam " + heroY + "x" + heroY + "x" + safeY + "x" + safeZ.
			string[] camPositions = message.Replace("cam ", "").Split('x');
			float newHeroY, newHeroZ, newSafeY, newSafeZ;

			if (float.TryParse(camPositions[0], out newHeroY)) { heroY = newHeroY; }
			if (float.TryParse(camPositions[1], out newHeroZ)) { heroZ = newHeroZ; }
			if (float.TryParse(camPositions[2], out newSafeY)) { safeY = newSafeY; }
			if (float.TryParse(camPositions[3], out newSafeZ)) { safeZ = newSafeZ; }
		}

		// Without iTween: cam.localPosition = new Vector3(0f, heroY, heroZ);
		iTween.MoveTo(sensor, iTween.Hash(
			"position", new Vector3(0f, heroY, heroZ), "time", 0.5f, "islocal", true,
			"onComplete", "CameraIsMoved", "onCompleteTarget", gameObject

		));
		cam.LookAt(target);
		
		sensor.transform.localPosition = cam.localPosition;
		sensor.transform.LookAt(adventurer.transform);
	}

	void CameraIsMoved () {
		// Moving the camera with iTween interferes with camera adjustments in Update().
		transition = false;
	}

	void FindCamDeltas() {
		deltaY = (safeY - heroY) / 30f;
		deltaZ = (safeZ - heroZ) / 30f;
	}

	public void CamTestScript (string msg) {
		Debug.Log("Message from Adventurer.cs: " + msg);
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
