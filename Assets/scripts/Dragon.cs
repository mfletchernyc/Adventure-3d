using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Adventure reinterpretation for Unity
// M. Fletcher 2014
// http://en.wikipedia.org/wiki/Adventure_(1979_video_game)

public class Dragon : MonoBehaviour {
	
	public float interestRange = 80f;	// Distance at which dragon notices items or the adventurer.

	public AudioClip slay, chomp, death;

	private Transform dragon;
	private GameObject adventurer;

	private Transform alive;		// Default dragon appearance.
	private Transform dead;			// Dead dragon appearance.
	private Transform chomping;		// Attacking dragon appearance.

	private Collider[] entities;	// Container for anything in the sphere of interest.
	private GameObject target;		// Object for chasing and attacking.
	private Vector3 view;			// Direction to look chasing and attacking.	
	
	private Dictionary<string, float> chompDuration = new Dictionary<string, float>() { { "Yorgle", 1.5f}, { "Grundle", 2.5f} };
	private Dictionary<string, float> chompTimer = new Dictionary<string, float>() { { "Yorgle", 0f}, { "Grundle", 0f} };


	void Awake () {
		dragon = gameObject.transform;
		alive = dragon.FindChild("dragon");
		dead = dragon.FindChild("dragon_dead");
		chomping = dragon.FindChild("dragon_chomp");

		// transform.parent.gameObject
		// gameObject.name
	}
	
	void Start () {
		adventurer = GameObject.Find("adventurer");
	}

	void Update () {
		// Let the dragon do his thing (if he's alive).
		if (alive.gameObject.activeSelf) {
			// Evaluate the local area, and figure out what to do.
			// RULES:
			// - Always run from the scary object.
			// - If there is only a preferred object, guard it.
			// - If the adventurer is in range of a preferred object, attack.

			// Detect anything interesting in interest range.
			entities = Physics.OverlapSphere(dragon.position, interestRange);

			for (int count = 0; count < entities.Length; count++) {
				if (entities[count].tag == "Item") {
					// Debug.Log(dragon.name + " detects " + entities[count].name);
				}
				
				if (entities[count].tag == "Player") {
					Chase(adventurer);
				}
			}
		}

		if (chomping.gameObject.activeSelf) {
			if (Time.time > chompTimer[dragon.name] + chompDuration[dragon.name]) {
				Pose(true, false, false);
			}
		}
	}
	
	void OnTriggerEnter (Collider other) {
		if (other.name == "sword") { 
			Die(); 
		} else {
			Debug.Log(dragon.name + " collides with " + other.name);
		}
	}
	
	void Chase (GameObject target) {
		view = new Vector3(target.transform.position.x, dragon.position.y, target.transform.position.z);
		dragon.LookAt(view);

		// if scary object: flee
		// if touching the player, attack
		// else move toward player
	}
	
	void Chomp () {
		// Only one attack at a time...
		if (alive.gameObject.activeSelf) {
			audio.PlayOneShot(chomp);
			Pose(false, false, true);

			chompTimer[dragon.name] = Time.time;
		}
	}
	
	void Kill (GameObject target) {
		// if adv is colliding with dragon, dead!
		// else, back to detection
	}

	void Guard (GameObject target) {
		// get guarded object position
		// if close to object, stop
		// else move toward that position
	}
	
	void Flee (GameObject target) {
		// get scary object position
		// move away from that position
	}

	void Die () {
		// Death greets me warm...
		if (!dead.gameObject.activeSelf) {
			audio.PlayOneShot(slay);
			Pose(false, true, false);
		}
	}

	void Pose (bool a, bool s, bool c) {
		alive.gameObject.SetActive(a);
		dead.gameObject.SetActive(s);
		chomping.gameObject.SetActive(c);
	}
}
