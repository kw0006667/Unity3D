using UnityEngine;
using System.Collections;

public class DisplayFPS : MonoBehaviour {

    public float updateInterval = 0.0f;

    private float accum = 0.0f; // FPS accumulated over the interval
    private float frames = 0f; // Frames drawn over the interval
    private float timeleft; // Left time for current interval 

	// Use this for initialization
	void Start () 
    {
        if (!guiText)
        {
            print("FramesPerSecond needs a GUIText component!");
            enabled = false;
            return;
        }
        timeleft = updateInterval;
        //guiText.transform.position = new Vector3(10.0f, Screen.height - 10, 10.0f);
	}
	
	// Update is called once per frame
	void Update () 
    {
	    timeleft -= Time.deltaTime;
        accum += Time.timeScale/Time.deltaTime;
        ++frames; 

        // Interval ended - update GUI text and start new interval
        if( timeleft <= 0.0 )
        {
            // display two fractional digits (f2 format)
            guiText.text = "FPS = " + Time.timeScale.ToString("f2") + " / " + Time.deltaTime.ToString() + " = " + (accum/frames).ToString("f2");
            timeleft = updateInterval;
            accum = 0.0f;
            frames = 0;
        }
	
	}

    public float GetFPS()
    {
        return (accum / frames);
    }
}
