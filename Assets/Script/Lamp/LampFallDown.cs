using UnityEngine;
using System.Collections;

public class LampFallDown : MonoBehaviour {

    public GameObject chandelier;
    public GameObject line;
    private bool isFallDown;
    private Light[] lamplights;
    private Rigidbody rigid;
    private float addValue = 0.55f;

	// Use this for initialization
	void Start () {
        isFallDown = false;
        lamplights = chandelier.GetComponentsInChildren<Light>();
        
        //rigid = chandelier.GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {

        if (isFallDown)
        {
            foreach (Light lamp in lamplights)
                lamp.enabled = false;
            chandelier.AddComponent<Rigidbody>();

            if (line.transform.localScale.z < 18.5)
            {
                line.transform.localScale += new Vector3(0, 0, addValue);
            }
        }
	
	}

    void OnMouseDown()
    {
        if (!isFallDown)
        {
            isFallDown = true;
        }
        
    }
}
