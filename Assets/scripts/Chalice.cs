using UnityEngine;
using System.Collections;

// Adventure reinterpretation for Unity
// M. Fletcher 2014
// http://en.wikipedia.org/wiki/Adventure_(1979_video_game)

public class Chalice : MonoBehaviour {

	public AudioClip win_good;
	public Material yellow;
	public Texture chalice;

	private Adventurer Adventurer;

	void Awake () {
		// Chalice uses a video texture.
		MovieTexture texture = (MovieTexture)renderer.material.mainTexture;
		texture.loop = true;
		texture.Play();
	}

	void Start () {
		Adventurer = GameObject.Find("adventurer").GetComponent<Adventurer>();

	}

	void RestoreScenery () {
		yellow.SetTexture("_MainTex", null);
	}

	void OnTriggerEnter (Collider other) {
		if (other.name == "victory" && !Adventurer.gameOver) { 
			Adventurer.gameOver = true;
			audio.PlayOneShot(win_good);

			yellow.SetTexture("_MainTex", chalice);

			Invoke("RestoreScenery", 4.3f);
		}
	}
}
