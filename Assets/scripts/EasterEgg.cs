using UnityEngine;
using System.Collections;

public class EasterEgg : MonoBehaviour {

	void Awake () {
		// Easter egg uses a video texture.
		MovieTexture texture = (MovieTexture)renderer.material.mainTexture;
		texture.loop = true;
		texture.Play();
	}
}
