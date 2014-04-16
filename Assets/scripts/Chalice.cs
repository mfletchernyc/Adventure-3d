using UnityEngine;
using System.Collections;

// Adventure reinterpretation for Unity
// M. Fletcher 2014
// http://en.wikipedia.org/wiki/Adventure_(1979_video_game)

public class Chalice : MonoBehaviour {

	void Awake () {
		MovieTexture texture = (MovieTexture)renderer.material.mainTexture;
		texture.loop = true;
		texture.Play();
	}
}
