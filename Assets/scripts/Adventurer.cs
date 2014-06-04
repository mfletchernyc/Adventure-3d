using UnityEngine;
using System.Collections;

// Adventure reinterpretation for Unity
// M. Fletcher 2014
// http://en.wikipedia.org/wiki/Adventure_(1979_video_game)
//
// http://atariage.com/manual_thumbs.html?SoftwareLabelID=964

public class Adventurer : MonoBehaviour {

	public bool gameOver;
	public AudioClip pickup, drop;
	public Material black, blue, green, lime, olive, purple, red, yellow;

	private Transform adventurer;
	private CharacterController controller;

	private float spin;				// Speed multiplier for rotation.
	private float speed;			// Speed multiplier for forward/backward movement.
	private float acceleration;		// Player eases in and out of forward/backward movement.
	private float rebound;			// Testing the bounce when using bridge to leave the map.

	private bool bridge;			// Moving through the bridge changes some rules.
	private bool stuck;				// For example, you can get stuck in a wall...
	private int defaultInventory;	// Children of the player object; used to determine if an item is held.

	private Cam Cam;				// Cam script ref for telling cam about entering a new area.
	private Dragon Dragon;			// Dragon script ref for checking dragon position during teleportation.
	
	void Awake () {
		spin = 222f;
		speed = 66f;
		rebound = -20f;

		adventurer = gameObject.transform;
		defaultInventory = adventurer.childCount;
	}

	void Start () {
		Cam = GameObject.Find("camera").GetComponent<Cam>();
		Dragon = GameObject.Find("Yorgle").GetComponent<Dragon>();
	}

	void FixedUpdate () {
		// Player rotation and movement.
		adventurer.Rotate(0f, Input.GetAxis("Horizontal") * spin * Time.deltaTime, 0f);

		controller = GetComponent<CharacterController>();

		if (!gameOver && !stuck) {
			acceleration = Input.GetAxis("Vertical") * speed * Time.deltaTime;
			controller.Move(adventurer.TransformDirection(new Vector3(0f, 0f, acceleration)));
		}
	}
	
	void Update () {
		// Drop an object if possible.
		if (Input.GetButton("Jump") && !gameOver) { 
			DropObject();

			// The only way to get stuck is to pick up the bridge in a wall.
			if (stuck) { stuck = false; }
		}
	}

	void OnTriggerEnter (Collider other) {
		switch (other.gameObject.tag) {// Pick up items when running into them.
			case "Item":
				if (other.gameObject.name == "beam") {
					// Bridge is composed of multiple colliders.
					other = GameObject.Find("bridge").collider;

					// Picking up the bridge while inside it ends the magic.
					if (bridge) { 
						ExitBridge();

						// Picking up the bridge while using it to move through a wall stops movement.
						Collider[] entities = Physics.OverlapSphere(adventurer.position, 1f);
						
						for (int count = 0; count < entities.Length; count++) {
							if (entities[count].tag == "Wall") { stuck = true; }
						}
					}
				}

				audio.PlayOneShot(pickup);
				
				// If player is already holding something, drop it.
				if (adventurer.childCount > defaultInventory) { DropObject(); }
				
				// Pick up item and position according to direction of travel.
				other.transform.parent = adventurer;
				float distance = acceleration > 0 ? 2f : -1.5f;
				Vector3 itemPosition = other.transform.localPosition; 
				other.transform.localPosition = new Vector3(itemPosition.x, itemPosition.y, itemPosition.z + distance);
				
				break;

			// Set player color to match dominant local color.
			case "Threshold":
				adventurer.renderer.material = (Material)this.GetType().GetField(other.renderer.name.ToString()).GetValue(this);
				break;
				
			// Tell camera script to move the camera. Player is a more stable place to test for this.
			case "Cam":
				Cam.MoveCamera(other.name);
				break;

			// Magic bridge.
			case "Bridge":
				EnterBridge();
				break;				

			// Maze teleporters.
			case "Jump":
				// Save the current player position.
				float advX = adventurer.position.x;
				float advZ = adventurer.position.z;
				
				// Teleporter name is "jump " + deltaX + "," + deltaZ.
				string[] jumpDeltas = other.name.Replace("jump ", "").Split(',');
				float deltaX, deltaZ;
				if (float.TryParse(jumpDeltas[0], out deltaX)) { advX += deltaX; }
				if (float.TryParse(jumpDeltas[1], out deltaZ)) { advZ += deltaZ; }
				
				// Find everything in the player's sphere, and teleport any dragons or items along, too.
				Collider[] entities = Physics.OverlapSphere(adventurer.position, Dragon.interestRange);
				
				for (int count = 0; count < entities.Length; count++) {
				if (entities[count].tag == "Dragon" || (entities[count].tag == "Item" && entities[count].transform.parent != adventurer)) {
						float entityX = entities[count].transform.position.x + deltaX;
						float entityY = entities[count].transform.position.y;
						float entityZ = entities[count].transform.position.z + deltaZ;
						
						entities[count].transform.position = new Vector3(entityX, entityY, entityZ);
					}
				}
				
				// Teleport the player.
				adventurer.position = new Vector3(advX, 8f, advZ);
				break;
		}
	}

	void OnTriggerExit (Collider other) {
		switch (other.gameObject.tag) {
			case "Cam":
				Cam.MoveCamera("exit");
				break;
				
			case "Bridge":
				ExitBridge();
				break;
				
			case "Boundary":
				// Prevent the player from leaving the map by using the bridge.
				controller.Move(adventurer.TransformDirection(new Vector3(0f, 0f, speed * Time.deltaTime * rebound)));
				break;
		}
	}

	void DropObject () {
		foreach (Transform child in adventurer) {
			if (child.CompareTag("Item")) {
				audio.PlayOneShot(drop);
				child.transform.parent = GameObject.Find("items").transform;

				if (child.name == "bridge") {
					// If within the dropped bridge, reactivate the magic.
					Collider[] entities = Physics.OverlapSphere(adventurer.position, 1f);

					for (int count = 0; count < entities.Length; count++) {
						if (entities[count].tag == "Bridge") { EnterBridge(); }
					}
				}
			}
		}
	}

	void EnterBridge () {
		bridge = true;
		Physics.IgnoreLayerCollision(0, 2, true);
	}

	void ExitBridge () {
		bridge = false;
		Physics.IgnoreLayerCollision(0, 2, false);
	}
}
