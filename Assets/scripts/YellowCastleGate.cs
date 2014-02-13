using UnityEngine;
using System.Collections;

// Adventure reinterpretation for Unity
// M. Fletcher 2014
// http://en.wikipedia.org/wiki/Adventure_(1979_video_game)

public class YellowCastleGate : MonoBehaviour {

	void OnTriggerEnter (Collider other) {
		if (other.name == "yellow key") {
			iTween.MoveBy(gameObject, iTween.Hash("y", 12));
		}
	}
}