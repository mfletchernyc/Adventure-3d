using UnityEngine;
using System.Collections;

public class Chalice : MonoBehaviour {

	void Awake () {
		MovieTexture texture = (MovieTexture)renderer.material.mainTexture;
		texture.loop = true;
		texture.Play();
	}
}
