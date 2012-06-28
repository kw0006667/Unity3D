using UnityEngine;
using System.Collections;

public class Elevator_Hospital_OpenButton : MonoBehaviour {

    public int current_floor;
    public int Director;

    private string[] DirectorStr = { "N", "E", "W", "S" };

    private GameObject Elevator;
    private Elevator_Hospital Elevator_script;
    private GameObject Greta;

    public int GetCurrentFloor()
    {
        return current_floor;
    }

    float Distance()
    {
        float distance = Vector3.Distance(this.transform.position, Greta.transform.position);

        return distance;
    }

	// Use this for initialization
	void Start () 
    {
        Greta = GameObject.FindGameObjectWithTag("Greta");
        Elevator = GameObject.FindGameObjectWithTag("Elevator_" + DirectorStr[Director]);
        //Elevator_script = Elevator.GetComponent("Elevator_Hospital") as Elevator_Hospital;
        //Elevator_script = FindObjectOfType(typeof(Elevator_Hospital)) as Elevator_Hospital;
        Elevator_script = Elevator.GetComponentInChildren<Elevator_Hospital>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        
	}

    IEnumerator OnMouseDown()
    {
        if (Elevator_script.GetState() == Elevator_Hospital.State.Stop)
        {
            if (Elevator_script.GetCurrentFloor() == GetCurrentFloor())
            {
                if (!Elevator_script.GetDoorOpen())
                {
                    if (Distance() < 2.0f)
                    {
                        Elevator_script.SetDoorOpen(true);
                    }
                }
                else
                    Elevator_script.SetDoorClose(true);
            }
            else
            {
                if (Elevator_script.GetState() == Elevator_Hospital.State.Stop)
                {
                    if (Elevator_script.GetDoorOpen())
                    {
                        Elevator_script.SetDoorClose(true);
                        yield return new WaitForSeconds(5.0f);
                        if ((GetCurrentFloor() - Elevator_script.GetCurrentFloor()) > 0)
                            Elevator_script.SetState(Elevator_Hospital.State.Up);
                        else
                            Elevator_script.SetState(Elevator_Hospital.State.Down);
                    }
                    else
                    {
                        if ((GetCurrentFloor() - Elevator_script.GetCurrentFloor()) > 0)
                            Elevator_script.SetState(Elevator_Hospital.State.Up);
                        else
                            Elevator_script.SetState(Elevator_Hospital.State.Down);
                    }
                }
            }
        }
    }
}
