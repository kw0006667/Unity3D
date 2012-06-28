using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour {

    public GameObject bomb;

    private bool isMouseDown = false;
    private GameObject B;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        
        if (isMouseDown)
        {
            bomb.active = true;        
        }
	    
	}

    void OnMouseDown()
    {
        isMouseDown = true;
    }
}
