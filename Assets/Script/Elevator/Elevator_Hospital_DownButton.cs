using UnityEngine;
using System.Collections;

public class Elevator_Hospital_DownButton : MonoBehaviour {

    public int Director;

    private string[] DirectorStr = { "N", "E", "W", "S" };

    private GameObject Elevator;
    private Elevator_Hospital Elevator_script;

	// Use this for initialization
	void Start () 
    {
        Elevator = GameObject.FindGameObjectWithTag("Elevator_" + DirectorStr[Director]);
        //Elevator_script = FindObjectOfType(typeof(Elevator_Hospital)) as Elevator_Hospital;
        Elevator_script = Elevator.GetComponentInChildren<Elevator_Hospital>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    IEnumerator OnMouseDown()
    {
        if (Elevator_script.GetCurrentFloor() > 1)
        {
            if (Elevator_script.GetState() == Elevator_Hospital.State.Stop)
            {
                if (Elevator_script.GetDoorOpen())
                {
                    Elevator_script.SetDoorClose(true);
                    yield return new WaitForSeconds(5.0f);
                    if (!Elevator_script.GetDoorOpen())
                        Elevator_script.SetState(Elevator_Hospital.State.Down);
                }
            }
        }
    }
}
