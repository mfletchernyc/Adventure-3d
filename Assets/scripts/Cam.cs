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
	// (and vice versa). The maze and barbicans change these positions. Currently
	// these special areas use only a single position. If this changes, refactor. 

	public float defaultHeroY, defaultHeroZ;
	public float defaultSafeY, defaultSafeZ;
	public float defaultTargetY, defaultTargetZ;
	
	private Transform cam;
	private GameObject adventurer;
	
	private Transform target;			// The camera doesn't actually look at the player, but at a point that makes nice cinematics.
	private GameObject sensor;			// The origin of cast rays that check camera position for obstruction.
	private bool transition;			// Prevents usual cam placement routine from fighting with iTween animations.
	
	private float heroY, heroZ;			// Cam position for best cinematics.
	private float safeY, safeZ;			// Cam position to avoid any obstructions.
	private float deltaY, deltaZ;		// Distance moved each update between hero and safe.
	private float targetY, targetZ;		// Cam target position also changes in special areas.
	
	void Awake () {
		cam = gameObject.transform;
		
		heroY = defaultHeroY;
		heroZ = defaultHeroZ;
		safeY = defaultSafeY;
		safeZ = defaultSafeZ;
		targetY = defaultTargetY;
		targetZ = defaultTargetZ;

		SetCamDeltas();
	}
	
	void Start () {
		sensor = GameObject.Find("sensor");
		target = GameObject.Find("camera target").transform;
		adventurer = GameObject.Find("adventurer");
	}
	
	void Update () {
		// Rays cast from inside an object don't register hits. Camera obstruction is checked
		// by casting a ray from the player position to a sensor in the camera's position.
		// This avoids camera bounce and accounts for the camera target not being the player.

		// Issue: The script assumes safe positions are greater than heroes. This is currently
		// true (or safe positions == heroes), but if that changes, it's time to refactor.

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
				if (updatedY < safeY || updatedZ < safeZ) { // Issue: assumes safe positions are greater than heroes.
					sensor.transform.localPosition = new Vector3(0f, updatedY, updatedZ);
				} else { sensor.transform.localPosition = new Vector3(0f, safeY, safeZ); }
			}

			else {
				// Move the camera toward the default position (if necessary and possible).
				previousPosition = sensor.transform.localPosition;

				updatedY = sensor.transform.localPosition.y - deltaY;
				updatedZ = sensor.transform.localPosition.z - deltaZ;

				// If necessary, move the sensor toward (or into) default position.
				if (updatedY > heroY || updatedZ > heroZ) { // Issue: assumes safe positions are greater than heroes.
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
			targetY = defaultTargetY;
			targetZ = defaultTargetZ;
			
			SetCamDeltas();
		}

		else {
			// Special area name is "cam " + specialY + "," + specialY + "," + targetY + "," + targetZ.
			string[] camPositions = message.Replace("cam ", "").Split(',');
			float specialY, specialZ, specialTargetY, specialTargetZ;
			
			if (float.TryParse(camPositions[0], out specialY)) { heroY = specialY; }
			if (float.TryParse(camPositions[1], out specialZ)) { heroZ = specialZ; }
			if (float.TryParse(camPositions[2], out specialTargetY)) { targetY = specialTargetY; }
			if (float.TryParse(camPositions[3], out specialTargetZ)) { targetZ = specialTargetZ; }
			safeY = specialY;
			safeZ = specialZ;
			
			SetCamDeltas();
		}

		iTween.MoveTo(sensor, iTween.Hash(
			"position", new Vector3(0f, heroY, heroZ), "time", 0.75f, "islocal", true,
			"onComplete", "CameraIsMoved", "onCompleteTarget", gameObject
		));

		iTween.MoveTo(GameObject.Find("camera target"), iTween.Hash(
			"position", new Vector3(0f, targetY, targetZ), "time", 0.75f, "islocal", true
		));
		
		sensor.transform.localPosition = cam.localPosition;
	}

	void CameraIsMoved () { 
		transition = false; // Moving the cam with iTween conflicts with Update().
	}

	void SetCamDeltas () {
		deltaY = (safeY - heroY) / 20f;
		deltaZ = (safeZ - heroZ) / 20f;
	}

	bool ObstructedSensor() {
		float distance = Vector3.Distance(sensor.transform.position, adventurer.transform.position);
		Vector3 direction = sensor.transform.TransformDirection(Vector3.back) * distance;

		return Physics.Raycast(adventurer.transform.position, direction, distance);
	}
}
