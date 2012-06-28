using UnityEngine;
using System.Collections;

public class OpenLight : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.name == "Spot light")
            col.gameObject.GetComponentInChildren<Light>().shadows = LightShadows.Hard;
    }
}
