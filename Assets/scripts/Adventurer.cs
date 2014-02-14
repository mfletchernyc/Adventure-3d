using UnityEngine;
using System.Collections;

// Adventure reinterpretation for Unity
// M. Fletcher 2014
// http://en.wikipedia.org/wiki/Adventure_(1979_video_game)

public class Adventurer : MonoBehaviour {

	// Player control.
	public float spin;		// Speed pref for rotation.
	public float speed;		// Speed pref for forward/backward movement.
	public bool defeat;		// Prevents player from moving if eaten.
	
	public AudioClip pickup, drop;

	public Material black, blue, green, lime, olive, purple, red, yellow;

	private Transform adventurer;
	private GameObject advCamera;
	private Vector3 direction = Vector3.zero;	// Container for reading input for forward/backward movement.
	private int defaultInventory = 1;			// Children of the player object. Default is the camera. (Might add lights.)
	
	private float jumpX, jumpZ;		// Coordinates for maze transportation.
	private string[] jumpDeltas;	// Maze transporters carry jump distances in their names.
	private float deltaX, deltaZ;	// Variables for the transporters' jump distances.
	
	void Awake () {
		adventurer = gameObject.transform;
		advCamera = GameObject.Find("camera");
	}

	void FixedUpdate () {	
		// Player rotation.
		float rotation = Input.GetAxis("Horizontal");
		adventurer.Rotate(0f, rotation * spin * Time.deltaTime, 0f);
	
		// Player forward/backward movement.
		CharacterController controller = GetComponent<CharacterController>();
		direction = new Vector3(0f, 0f, Input.GetAxis("Vertical") * speed);
		direction = adventurer.TransformDirection(direction);
		controller.Move(direction * Time.deltaTime);

		// If in a dragon's belly, cancel movemement.
		if (defeat) { controller.Move(-direction * Time.deltaTime); }
	}
	
	void Update () {
		// Drop an object if possible.
		if (Input.GetButton("Jump")) { DropObject(); }
	}

	void OnTriggerEnter (Collider other) {
		// Pick up items when running into them.
		if (other.gameObject.tag == "Item") {
			audio.PlayOneShot(pickup);

			// If adventurer is already holding something, drop it.
			if (adventurer.childCount > defaultInventory) { DropObject(); }

			// Pick up and position object.
			other.transform.parent = adventurer;
			other.transform.localPosition = new Vector3(0f, 0f, 2.5f);
		}
		
		// Set player color to match dominant local color.
		if (other.gameObject.tag == "Threshold") {
			adventurer.renderer.material = (Material)this.GetType().GetField(other.renderer.name.ToString()).GetValue(this);
		}

		// Lower the camera when entering a castle.
		if (other.name == "barbican") {
			MoveCamera(new Vector3 (0f, -15f, 0f));
		}

		// Maze transporters.
		if (other.gameObject.tag == "Jump") {
			// Save the current position.
			jumpX = adventurer.position.x;
			jumpZ = adventurer.position.z;

			// Transporter name is "jump " + delta X + "x" + delta Z.
			jumpDeltas = other.name.Replace("jump ", "").Split('x');
			if (float.TryParse(jumpDeltas[0], out deltaX)) { jumpX += deltaX; }
			if (float.TryParse(jumpDeltas[1], out deltaZ)) { jumpZ += deltaZ; }

			adventurer.position = new Vector3(jumpX, 8, jumpZ);

			// If a dragon is chasing, transport it, too.

			// if (Vector3.Distance(adventurer.transform.position, dragon.position) < Dragon.interestRange) {
			// Need to check for all dragons tho.


			/*
			// Find everything in the player's sphere, and check each element for dragonness.
			entities = Physics.OverlapSphere(adventurer.position, Dragon.interestRange);
			
			for (int count = 0; count < entities.Length; count++) {
				if (entities[count].tag == "Dragon") {
					// Transport this dragon. How? Check all differences between player position and jumpX/Y/Z
				}
			}*/

			/* 

			OR

			// Find all dragons, and check distance.
			*/


			/*
			// For all teleports? [ gameObject vs. GameObject]
			void moveElement(gameObject element, Vector3 position) { }


			 */

			// if things take too long, pass the dragon transport to dragon.cs?
		}
	}
	
	void OnTriggerExit (Collider other) {
		// Raise the camera when entering a castle.
		if (other.name == "barbican") {
			MoveCamera(new Vector3 (0f, 15f, 0f));
		}
	}
	
	void MoveCamera(Vector3 position) {
		iTween.MoveBy(advCamera, position, 0.5f);
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
