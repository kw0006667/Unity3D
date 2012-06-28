using UnityEngine;
using System.Collections;

public class medicinecabinetCrashAI : MonoBehaviour
{

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider ai)
    {
        if (ai.tag.Equals("AI"))
        {
            testAI testAIScript = ai.GetComponent<testAI>();
            testAIScript.enabled = false;
            ai.rigidbody.freezeRotation = false; 
            Destroy(ai);
        }
    }
}
