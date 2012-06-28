using UnityEngine;
using System.Collections;

public class Music : MonoBehaviour {
    public AudioClip BackgroundMusic;

    private float GameMusicVolume;
	// Use this for initialization
	void Start () {
        GameMusicVolume = PlayerPrefs.GetInt("Music") * 0.05f;
        
        this.audio.volume = GameMusicVolume;
        this.audio.clip = BackgroundMusic;
        this.audio.playOnAwake = true;
        this.audio.Play();
        //this.audio.PlayOneShot(BackgroundMusic);

	
	}
	
	// Update is called once per frame
	void Update () {
        GameMusicVolume = PlayerPrefs.GetInt("Music") * 0.05f;

        this.audio.volume = GameMusicVolume;
	}
}
