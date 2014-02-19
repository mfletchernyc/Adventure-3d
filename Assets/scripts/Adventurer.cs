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
	public bool barbican;	// Camera and movement options change in the barbican.
	
	public AudioClip pickup, drop;
	public Material black, blue, green, lime, olive, purple, red, yellow;

	private Transform adventurer;
	private GameObject cam;
	private int defaultInventory;	// Children of the player object. Default is the camera.

	private GameObject dragon;		// Dragon ref for getting Dragon script.
	private Dragon Dragon;			// Dragon script ref for checking dragon position during teleportation.
	
	void Awake () {
		adventurer = gameObject.transform;
		cam = GameObject.Find("camera");
		defaultInventory = adventurer.childCount;
		barbican = false;
	}

	void Start () {
		dragon = GameObject.Find("Yorgle");
		Dragon = dragon.GetComponent<Dragon>();
	}

	void FixedUpdate () {	
		// Player rotation.
		float rotation = Input.GetAxis("Horizontal");
		adventurer.Rotate(0f, rotation * spin * Time.deltaTime, 0f);

		// Player forward/backward movement.
		CharacterController controller = GetComponent<CharacterController>();
		Vector3 direction = new Vector3(0f, 0f, Input.GetAxis("Vertical") * speed);
		direction = adventurer.TransformDirection(direction);
		controller.Move(direction * Time.deltaTime);

		// If player is in a dragon's belly, cancel movemement.
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

			// If player is already holding something, drop it.
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
			barbican = true;
		}

		// Maze teleporters.
		if (other.gameObject.tag == "Jump") {
			// Save the current player position.
			float advX = adventurer.position.x;
			float advZ = adventurer.position.z;

			// Teleporter name is "jump " + delta X + "x" + delta Z.
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
		}
	}
	
	void OnTriggerExit (Collider other) {
		// Raise the camera when entering a castle.
		if (other.name == "barbican") {
			MoveCamera(new Vector3 (0f, 15f, 0f));
			barbican = false;
		}
	}
	
	void MoveCamera(Vector3 position) {
		iTween.MoveBy(cam, position, 0.5f);
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
