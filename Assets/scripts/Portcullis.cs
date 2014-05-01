using UnityEngine;
using System.Collections;

// Adventure reinterpretation for Unity
// M. Fletcher 2014
// http://en.wikipedia.org/wiki/Adventure_(1979_video_game)

public class Portcullis : MonoBehaviour {

	void OnTriggerEnter (Collider other) {
		if (other.name.Split(' ').Length > 1) { // Only applies to the keys.
			if (this.name.Replace(" portcullis", "") == other.name.Split(' ')[0]) {
				iTween.MoveBy(gameObject, iTween.Hash("y", 12f, "time", 3f));
			}
		}
	}
}