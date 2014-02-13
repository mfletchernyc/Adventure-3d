using UnityEngine;
using System.Collections;

// Adventure reinterpretation for Unity
// M. Fletcher 2014
// http://en.wikipedia.org/wiki/Adventure_(1979_video_game)

public class CastleGate : MonoBehaviour {

	void OnTriggerEnter (Collider other) {
		if (this.name.Replace("castle gate","") == other.renderer.material.name.Replace("(Instance)","")) {
			iTween.MoveBy(gameObject, iTween.Hash("y", 12, "time", 4));
		}
	}
}