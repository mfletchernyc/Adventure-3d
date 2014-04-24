using UnityEngine;
using System.Collections;

// Adventure reinterpretation for Unity
// M. Fletcher 2014
// http://en.wikipedia.org/wiki/Adventure_(1979_video_game)

public class Magnet : MonoBehaviour {

	public float range;
	public float power;

	private Transform magnet;
	private Transform focus;
	private ArrayList items;
	private Transform adventurer;

	void Awake () {
		magnet = gameObject.transform;
	}

	void Start () {
		adventurer = GameObject.Find("adventurer").transform;
		focus = GameObject.Find("focus").transform;

		// Items in order of attraction.
		items = new ArrayList();
		items.Add("yellow key");
		items.Add("black key");
		items.Add("sword");
		items.Add("bridge");
		items.Add("chalice");
	}

	void FixedUpdate () {
		// Find anything attractable. Items all live in the 'Ignore Raycast' layer.
		Collider[] entities = Physics.OverlapSphere(magnet.position, range, 1 << 2);

		int itemIndex = items.Count; // Placeholder out of range of attractable items.

		// Magnet attracts only one object at a time based on order of attraction.
		for (int count = 0; count < entities.Length; count++) {
			if (entities[count].tag == "Item" && entities[count].name != "magnet") {
				int thisIndex = items.IndexOf(entities[count].name);
				if (thisIndex < itemIndex) { itemIndex = thisIndex; }
			}
		}

		// If magnet found something to attract, try to move it.
		if (itemIndex < items.Count) {
			GameObject item = GameObject.Find(items[itemIndex].ToString());

			if (item.transform.parent != adventurer) { // Can't attract an item being held.
				item.transform.LookAt(new Vector3(magnet.position.x, item.transform.position.y, magnet.position.z));
				Vector3 target = new Vector3(focus.position.x, item.transform.position.y, focus.position.z);
				item.transform.position = Vector3.MoveTowards(item.transform.position, target, power * Time.deltaTime);

				// If the highest-ranked object is already captured, magnet attracts nothing.
			}
		}
	}

	/*

	NOTES:

	Bridge may need coordinates for placement when stuck to magnet.
	
	When an item being attracted by the magnet hits the player, the player
	picks up that item, even if already carrying another item.
	
	*/
}
