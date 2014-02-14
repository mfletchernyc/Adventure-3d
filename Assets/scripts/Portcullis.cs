using UnityEngine;
using System.Collections;

// Adventure reinterpretation for Unity
// M. Fletcher 2014
// http://en.wikipedia.org/wiki/Adventure_(1979_video_game)

public class Portcullis : MonoBehaviour {

	void OnTriggerEnter (Collider other) {
		if (this.name.Replace("portcullis", "") == other.renderer.material.name.Replace("(Instance)", "")) {
			iTween.MoveBy(gameObject, iTween.Hash("y", 12f, "time", 3f));
		}
	}
}