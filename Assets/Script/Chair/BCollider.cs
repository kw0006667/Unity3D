using UnityEngine;
using System.Collections;

public class BCollider : MonoBehaviour {

    private BoxCollider[] bc;

	// Use this for initialization
	void Start () {

        bc = this.GetComponentsInChildren<BoxCollider>();

        foreach (BoxCollider b in bc)
        {
            if (b != null)
            {
                b.size += new Vector3(0, 0.05f, 0);
                b.center += new Vector3(0, 0.025f, 0);
            }
        }
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
