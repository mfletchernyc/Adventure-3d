using UnityEngine;
using System.Collections;

// Adventure reinterpretation for Unity
// M. Fletcher 2014
// http://en.wikipedia.org/wiki/Adventure_(1979_video_game)

public class Adventurer : MonoBehaviour {

	// Player control.
	public float spin;		// Speed pref for rotation.
	public float speed;		// Speed pref for forward/backward movement.
	public bool gameOver;	// Prevents player from moving if game over.
	
	public AudioClip pickup, drop;
	public Material black, blue, green, lime, olive, purple, red, yellow;

	private Transform adventurer;
	private CharacterController controller;
	
	private int defaultInventory;	// Children of the player object. Default is the camera.
	
	private Cam Cam;				// Cam script ref for telling cam about entering a new area.

	private GameObject dragon;		// Dragon ref for getting Dragon script on the next line...
	private Dragon Dragon;			// Dragon script ref for checking dragon position during teleportation.
	
	void Awake () {
		adventurer = gameObject.transform;
		defaultInventory = adventurer.childCount;
	}

	void Start () {
		dragon = GameObject.Find("Yorgle"); // All dragons use the same script.
		Dragon = dragon.GetComponent<Dragon>();

		Cam = GameObject.Find("camera").GetComponent<Cam>();
	}

	void FixedUpdate () {	
		// Player rotation and movement.
		adventurer.Rotate(0f, Input.GetAxis("Horizontal") * spin * Time.deltaTime, 0f);

		controller = GetComponent<CharacterController>();
		if (!gameOver) {
			controller.Move(adventurer.TransformDirection(new Vector3(0f, 0f, Input.GetAxis("Vertical") * speed)) * Time.deltaTime);
		}
	}
	
	void Update () {
		// Drop an object if possible.
		if (Input.GetButton("Jump")) { DropObject(); }
	}

	void OnTriggerEnter (Collider other) {
		switch (other.gameObject.tag) {
			// Pick up items when running into them.
			case "Item":
				audio.PlayOneShot(pickup);
				
				// If player is already holding something, drop it.
				if (adventurer.childCount > defaultInventory) { DropObject(); }
				
				// Pick up and position object.
				other.transform.parent = adventurer;
				other.transform.localPosition = new Vector3(0f, 0f, 2.5f);
				break;
			
			// Set player color to match dominant local color.
			case "Threshold":
				adventurer.renderer.material = (Material)this.GetType().GetField(other.renderer.name.ToString()).GetValue(this);
				break;
				
			// Tell camera script to move the camera. Player is a more stable place to test for this.
			case "Cam":
			Debug.Log("adv OnTriggerEnter 'Cam'");
				Cam.MoveCamera(other.name);
				break;

			// Maze teleporters.
			case "Jump":
				// Save the current player position.
				float advX = adventurer.position.x;
				float advZ = adventurer.position.z;
				
				// Teleporter name is "jump " + deltaX + "x" + deltaZ.
				string[] jumpDeltas = other.name.Replace("jump ", "").Split('x');
				float deltaX, deltaZ;
				if (float.TryParse(jumpDeltas[0], out deltaX)) { advX += deltaX; }
				if (float.TryParse(jumpDeltas[1], out deltaZ)) { advZ += deltaZ; }
				
				// Find everything in the player's sphere, and teleport any dragons along, too.
				Collider[] entities = Physics.OverlapSphere(adventurer.position, Dragon.interestRange);
				
				for (int count = 0; count < entities.Length; count++) {
					if (entities[count].tag == "Dragon") {
						float dragonX = entities[count].transform.position.x + deltaX;
						float dragonZ = entities[count].transform.position.z + deltaZ;
						entities[count].transform.position = new Vector3(dragonX, 0f, dragonZ);
					}
				}
				
				// Teleport the player.
				adventurer.position = new Vector3(advX, 8f, advZ);
				break;
		}
	}

	void OnTriggerExit (Collider other) {
		if (other.gameObject.tag == "Cam") {
			Debug.Log("adv OnTriggerExit 'Cam'");
			Cam.MoveCamera("exit");
		}
	}

	void DropObject () {
		foreach (Transform child in adventurer) {
			if (child.CompareTag("Item")) {
				audio.PlayOneShot(drop);
				child.transform.parent = GameObject.Find("items").transform;
			}
		}
	}
}
