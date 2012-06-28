using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        //string message = "x = " + this.transform.forward.x + ", y = " + this.transform.forward.y + ", z = " + this.transform.forward.z;
        print("x = " + this.transform.forward.x + ", y = " + this.transform.forward.y + ", z = " + this.transform.forward.z);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        GetInput();
    }

    void GetInput()
    {
        if (Input.GetKey(KeyCode.W))
            this.rigidbody.AddForce(this.transform.forward * rigidbody.mass * 2);

        if (Input.GetKey(KeyCode.S))
            this.rigidbody.AddRelativeForce(this.transform.forward * rigidbody.mass * 2);
    }
}
