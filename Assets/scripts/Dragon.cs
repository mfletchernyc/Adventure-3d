using UnityEngine;
using System.Collections;

// Adventure reinterpretation for Unity
// M. Fletcher 2014
// http://en.wikipedia.org/wiki/Adventure_(1979_video_game)

public class Dragon : MonoBehaviour {
	
	public float interestRange;		// Distance at which dragon notices items or the adventurer.
	public float biteRange;			// Distance at which dragon can eat the adventurer.
	public float deathRange;		// Distance from the sword at which the dragon dies.
	public float speed;				// How fast is dragon?
	public float chompDuration;		// How long between chomping and eating?

	public AudioClip slay, chomp, death;

	private Transform dragon;
	private GameObject adventurer;
	private Adventurer Adventurer;

	private Transform alive;			// Default dragon appearance.
	private Transform dead;				// Dead dragon appearance.
	private Transform chomping;			// Attacking dragon appearance.
	private float chompTimer;			// How soon is now?
	
	private GameObject target;			// Object for chasing, attacking and guarding.
	private Vector3 targetPosition;		// Direction to look chasing and attacking.

	// Need items that frighten each dragon.
	// Need items that each dragon guards.

	void Awake () {
		dragon = gameObject.transform;

		// Dragon object contains the three poses.
		alive = dragon.FindChild("dragon");
		dead = dragon.FindChild("dragon_dead");
		chomping = dragon.FindChild("dragon_chomp");
	}
	
	void Start () {
		adventurer = GameObject.Find("adventurer");
		Adventurer = adventurer.GetComponent<Adventurer>();
	}

	void Update () {
		// Let the dragon do his thing (if he's alive).
		if (alive.gameObject.activeSelf) {
			// Detect anything interesting in interest range.
			Collider[] entities = Physics.OverlapSphere(dragon.position, interestRange);

			// RULES:
			// - Always run from the scary object.
			// - If there is only a preferred object, guard it.
			// - If the adventurer is in range of a preferred object, attack.
			
			for (int count = 0; count < entities.Length; count++) {
				if (entities[count].name == "adventurer" && !Adventurer.defeat) {
					if (Vector3.Distance(dragon.position, adventurer.transform.position) < biteRange) {
						Chomp();
					} else { 
						Chase(adventurer);
					}
				}

				if (entities[count].name == "sword") { 
					// Checking trigger collisions is flakey if player isn't holding the sword.
					if (Vector3.Distance(dragon.position, entities[count].transform.position) < deathRange) {
						Die();
					}
				}

				// Check for items that scare the dragon.
				// Check for items the dragon likes to guard.
			}
		}

		// Dragon is mid-attack; either kill or resume normal behavior.
		if (chomping.gameObject.activeSelf) {
			if (Time.time > chompTimer + chompDuration) {
				Pose(true, false, false);	// Can't kill, so resume normal behavior.

				if (Vector3.Distance(adventurer.transform.position, dragon.position) < biteRange) {
					Kill();
				}
			}
		}
	}
	
	void OnTriggerEnter (Collider other) {
		// This is flakey if player isn't holding the sword, so there's a backup in Update().
		if (other.name == "sword") { Die(); }
	}
	
	void Chase (GameObject target) {
		// Face the player, then move in that direction.
		targetPosition = new Vector3(target.transform.position.x, dragon.position.y, target.transform.position.z);
		dragon.LookAt(targetPosition);
		dragon.position = Vector3.MoveTowards(dragon.position, targetPosition, speed * Time.deltaTime);
	}
	
	void Chomp () {
		// Called from the adventurer script.
		if (alive.gameObject.activeSelf) { // Don't chomp if already chomping.
			audio.PlayOneShot(chomp);
			Pose(false, false, true);
			chompTimer = Time.time;
		}
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

	void Kill () {
		// Get in my belly!
		audio.PlayOneShot(death);
		adventurer.transform.parent = dragon;
		adventurer.transform.localPosition = new Vector3(0f, 8f, 0f);
		Adventurer.defeat = true;
	}

	void Die () {
		// Death greets me warm...
		if (!dead.gameObject.activeSelf) {
			audio.PlayOneShot(slay);
			Pose(false, true, false);
		}
	}

	void Pose (bool a, bool s, bool c) {
		// Crude animation by showing/hiding child models.
		alive.gameObject.SetActive(a);
		dead.gameObject.SetActive(s);
		chomping.gameObject.SetActive(c);
	}
}
