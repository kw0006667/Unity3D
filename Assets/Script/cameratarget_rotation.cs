using UnityEngine;
using System.Collections;

public class cameratarget_rotation : MonoBehaviour {

    public Transform greta;
    private float _y;
    private Quaternion rotate;
	// Use this for initialization
	void Start () {
        _y = greta.rotation.y;
        
        rotate.y = _y;
        rotate.x = 0.0f;
        rotate.z = 0.0f;
        rotate.w = 0.0f;

        transform.rotation = rotate;
        

	}
	
	// Update is called once per frame
	void Update () {
        _y = greta.rotation.y;

        rotate.y = _y;
        rotate.x = 0.0f;
        rotate.z = 0.0f;
        rotate.w = 0.0f;

        transform.rotation = rotate;
	}
}
