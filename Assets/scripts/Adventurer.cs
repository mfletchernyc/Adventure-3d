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

	public Material blue, green, lime, olive, purple, red, yellow;

	private Transform adventurer;
	private Vector3 direction = Vector3.zero;	// Container for reading input for forward/backward movement.
	private int defaultInventory = 3;			// Children of the player object. Default is camera and two lights.
	
	void Awake () {
		adventurer = gameObject.transform;
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
	}

//	void OnControllerColliderHit (ControllerColliderHit other) {
//		// Here there be dragons...
//		if (other.transform.tag == "Dragon") {
//			other.transform.SendMessage("Chomp");
//		}
//	}

	void DropObject () {
		foreach (Transform child in adventurer) {
			if (child.CompareTag("Item")) {
				audio.PlayOneShot(drop);
				child.transform.parent = GameObject.Find("items").transform;
			}
		}
	}
}
