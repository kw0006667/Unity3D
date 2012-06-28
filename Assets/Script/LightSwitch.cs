using UnityEngine;
using System.Collections;

public class LightSwitch : MonoBehaviour {

    public float LightRan = 3;
    public float LightIntensity = 3.5f;

    private float LightRange = 30.0f;

    private GameObject Greta;
    private bool isLight = false;

	// Use this for initialization
	void Start () {
        Greta = GameObject.FindGameObjectWithTag("MainCamera");
        this.light.enabled = isLight;
        LightRan = 3.0f;
        LightIntensity = 3.5f;
        if (this.light.enabled)
        {
            this.light.range = LightRan;
            this.light.intensity = LightIntensity;
        }
	}
	
	// Update is called once per frame
	void Update () 
    {

        if (this.light.enabled)
        {
            this.light.range = LightRan;
            this.light.intensity = LightIntensity;
        }
        if (Distance())
        {
            isLight = true;
            this.light.enabled = isLight;
        }
        else
        {
            isLight = false;
            this.light.enabled = isLight;
            this.light.shadows = LightShadows.None;
        }
	}

    bool Distance()
    {
        Vector3 forward = Greta.transform.TransformDirection(Vector3.forward);
        Vector3 LightToGreta = this.transform.position - Greta.transform.position;

        float distance = Vector3.Distance(this.transform.position, Greta.transform.position);
        float distance_root2 = Mathf.Sqrt(forward.x * forward.x + forward.y * forward.y + forward.z * forward.z) * Mathf.Sqrt(distance);

        float dot = Vector3.Dot(forward, LightToGreta);
        if (dot/distance_root2 > -0.5f)
        {
            if (distance < LightRange)
                return true;
            else
                return false;
        }
        else
        {
            if (distance < 4)
                return true;
            else
                return false;
        }
    }
}
