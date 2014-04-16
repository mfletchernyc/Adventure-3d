using UnityEngine;
using System.Collections;

// Adventure reinterpretation for Unity
// M. Fletcher 2014
// http://en.wikipedia.org/wiki/Adventure_(1979_video_game)

public class Magnet : MonoBehaviour {

	public float range;		// Distance at which the magnet attracts items.

	/*

	Magnet order of attraction:
	1. yellow key
	2. black key
	3. sword
	4. bridge
	5. chalice

	Magnet attracts only one object at a time.

	If a higher-ranked object is in the room, magnet will not attract any
	other object, even if the higher-ranked object is held by the player.

	Should the magnet be rotated on the X axis (X and Z axes) for better placement of attracted objects?

	Bridge may need coordinates for placement when stuck to magnet.
	
	When an item being attracted by the magnet hits the player, the player
	picks up that item, even if already carrying another item.
	
	*/

	void Update () {
		// Is an item currently stuck to the magnet? If so, break.
		// Find all objects in range that are children of items.
		// Rank eligible found items.
		// Attract highest-ranked found item.
	}
}
