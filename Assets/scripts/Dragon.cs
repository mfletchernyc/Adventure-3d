using UnityEngine;
using System.Collections;

// Adventure reinterpretation for Unity
// M. Fletcher 2014
// http://en.wikipedia.org/wiki/Adventure_(1979_video_game)

public class Dragon : MonoBehaviour {
	
	public AudioClip slay, chomp, death;

	private Transform dragon;
	private GameObject adventurer;
	private Adventurer Adventurer;
	private GameObject target;

	private Transform alive;			// Default dragon appearance.
	private Transform dead;				// Dead dragon appearance.
	private Transform chomping;			// Attacking dragon appearance.

	private float speed;				// How fast is dragon?
	private float chompDuration;		// How long between chomping and eating?
	private float interestRange;		// Distance at which dragon notices items or the adventurer.
	private float biteRange;			// Distance at which dragon can eat the adventurer.
	public float deathRange;			// Distance from the sword at which the dragon dies.

	private float chompTimer;			// Delay between chomp and kill.
	private float fleeTimer;			// When player holds a scary object, dragon gets conflicted.
	private bool fleeing;				// Giving the dragon time to flee prevents bouncing.
	private float fleeDuration;			// How long between fleeing and attacking again?

	void Awake () {
		dragon = gameObject.transform;

		interestRange = 80f;
		biteRange = 15f;
		deathRange = 10f;
		fleeDuration = 1.5f;

		if (dragon.name == "Yorgle") {
			speed = 57f;
			chompDuration = 1.5f;
		} else {
			speed = 53f;
			chompDuration = 2f;
		}

		// Dragon contains the three poses. Used for getting the current dragon state.
		alive = dragon.FindChild("alive");
		dead = dragon.FindChild("dead");
		chomping = dragon.FindChild("chomping");
	}
	
	void Start () {
		adventurer = GameObject.Find("adventurer");
		Adventurer = adventurer.GetComponent<Adventurer>();

		// Create Yorgle's list of objects to guard.
		// Create Grundle's list of objects to flee.
	}

	void FixedUpdate () {
		// Let the dragon do his thing (if he's alive).

		// RULES:
		// - Always run from the scary object.
		// - If there is only a preferred object, guard it.
		// - If the adventurer is in range of a preferred object, attack.
		// - Go in search of a preferred object...when?

		if (alive.gameObject.activeSelf) {
			if (fleeing) {
				// Dragon flees for a while to prevent bouncing.
				dragon.Translate(Vector3.forward * speed * Time.deltaTime);

				if (Time.time > fleeTimer + fleeDuration) { fleeing = false; }
			}

			else {
				// Detect anything interesting. Adventurer and tems all live in the 'Ignore Raycast' layer.
				Collider[] entities = Physics.OverlapSphere(dragon.position, interestRange, 1 << 2);
				
				for (int count = 0; count < entities.Length; count++) {
					// In level one, difficulty easy, there's only one scary item.
					if (dragon.name == "Yorgle" && entities[count].name == "yellow key") {
						Flee(GameObject.Find("yellow key"));
						break;
					}

					// Need to add items dragons like to guard.

					else {
						if (entities[count].name == "adventurer" && !Adventurer.gameOver) {
							if (Vector3.Distance(dragon.position, adventurer.transform.position) < biteRange) {
								Chomp();
							} else { Chase(adventurer); }
						}

						if (entities[count].name == "sword") { 
							// Checking trigger collisions is flakey if player isn't holding the sword.
							if (Vector3.Distance(dragon.position, entities[count].transform.position) < deathRange) {
								Die();
							}
						}
					}
				}
			}
		}

		// Dragon is mid-attack; either kill or resume normal behavior.
		if (chomping.gameObject.activeSelf) {
			if (Time.time > chompTimer + chompDuration) {
				Pose("alive");	// Can't kill, so resume normal behavior.

				if (Vector3.Distance(adventurer.transform.position, dragon.position) < biteRange && !Adventurer.gameOver) {
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
		Vector3 targetPosition = new Vector3(target.transform.position.x, dragon.position.y, target.transform.position.z);
		dragon.LookAt(targetPosition);
		dragon.position = Vector3.MoveTowards(dragon.position, targetPosition, speed * Time.deltaTime);
	}
	
	void Chomp () {
		// Called from the adventurer script.
		if (alive.gameObject.activeSelf) { // Don't chomp if already chomping.
			audio.PlayOneShot(chomp);
			Pose("chomping");
			chompTimer = Time.time;
		}
	}

	void Guard (GameObject target) {
		// get guarded object position
		// if close to object, stop
		// else move toward that position
	}
	
	void Flee (GameObject target) {
		Vector3 targetPosition = new Vector3(target.transform.position.x, dragon.position.y, target.transform.position.z);
		dragon.LookAt(targetPosition);
		dragon.Rotate(0, 180, 0);
		dragon.Translate(Vector3.forward * speed * Time.deltaTime);

		fleeTimer = Time.time;
		fleeing = true;
	}

	void Kill () {
		// Get in my belly!
		audio.PlayOneShot(death);
		adventurer.transform.parent = dragon;
		adventurer.transform.localPosition = new Vector3(0f, 8f, 0f);
		Adventurer.gameOver = true;
	}

	void Die () {
		// Death greets me warm...
		if (alive.gameObject.activeSelf) { 
			audio.PlayOneShot(slay);
			Pose("dead");
		}
	}

	void Pose (string pose) {
		// Crude animation by showing/hiding child models.
		alive.gameObject.SetActive(false);
		dead.gameObject.SetActive(false);
		chomping.gameObject.SetActive(false);
		dragon.FindChild(pose).gameObject.SetActive(true);
	}
}
