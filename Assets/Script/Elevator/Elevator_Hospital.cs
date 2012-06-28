//-----------------------------------------
// Rolling
// National Taipei University of Education
// Digital Technology Design
// 
// Name : Elevator_Hospital
// Modified Data : 2011/11/30
// Modified By : Tim Chang
// Modified Content
//      1. Modify Direction 'S' state.Down logic.
//-----------------------------------------

#region References
using UnityEngine;
using System.Collections;
#endregion

/// <summary>
/// All Implementations of state and controller.
/// </summary>
public class Elevator_Hospital : MonoBehaviour
{

    public int Director;
    public float FloorHeight = 1.0f;
    public float DoorWidth = 0.15f;
    public float Speed = 0.05f;
    public float DoorSpeed = 0.05f;

    private State _state;
    private float DistanceAway = 5.0f;
    private int Current_floor = 3;
    private float AddValue;
    private bool isOpenDoor = false;
    private bool isCloseDoor = false;
    private bool isOpen = false;
    public bool isIn = false;
    private string[] DirectorStr = { "N", "E", "W", "S" };

    //-----GameObjects-------
    private GameObject Elevator_Door_L;
    private GameObject Elevator_Door_R;
    private GameObject Elevator_Out_L;
    private GameObject Elevator_Out_R;
    private GameObject Greta;

    public enum State
    {
        Up, Down, Stop,
    }

    public State GetState()
    {
        return _state;
    }

    public void SetState(State s)
    {
        switch (s)
        {
            case State.Down:
                _state = State.Down;
                break;
            case State.Up:
                _state = State.Up;
                break;
            default:
                break;
        }
    }

    public int GetCurrentFloor()
    {
        return Current_floor;
    }

    public bool GetDoorOpen()
    {
        return isOpen;
    }

    public void SetDoorOpen(bool TF)
    {
        isOpenDoor = TF;
    }

    public void SetDoorClose(bool TF)
    {
        isCloseDoor = TF;
    }

    float Distance()
    {
        float distance = Vector3.Distance(Elevator_Door_L.transform.position, Greta.transform.position);

        return distance;
    }

    // Use this for initialization
    void Start()
    {
        Elevator_Door_L = GameObject.FindGameObjectWithTag("Elevator_L" + DirectorStr[Director]);
        Elevator_Door_R = GameObject.FindGameObjectWithTag("Elevator_R" + DirectorStr[Director]);
        Elevator_Out_L = GameObject.FindGameObjectWithTag("Elevator_L" + DirectorStr[Director] + GetCurrentFloor().ToString());
        Elevator_Out_R = GameObject.FindGameObjectWithTag("Elevator_R" + DirectorStr[Director] + GetCurrentFloor().ToString());
        Greta = GameObject.FindGameObjectWithTag("Greta");
        _state = State.Stop;
    }

    // Update is called once per frame
    void Update()
    {
        //isInBox();
        if (_state == State.Down)
        {
            if (AddValue < FloorHeight)
            {
                
                if (isIn)
                {
                    Greta.transform.position -= new Vector3(0.0f, Speed, 0.0f);
                    this.transform.position -= new Vector3(0.0f, Speed, 0.0f);
                }
                else
                {
                    this.transform.position -= new Vector3(0.0f, Speed, 0.0f);
                }
                AddValue += Speed;
            }
            else
            {
                AddValue = 0;
                Current_floor--;
                Elevator_Out_L = GameObject.FindGameObjectWithTag("Elevator_L" + DirectorStr[Director] + GetCurrentFloor().ToString());
                Elevator_Out_R = GameObject.FindGameObjectWithTag("Elevator_R" + DirectorStr[Director] + GetCurrentFloor().ToString());
                _state = State.Stop;
                isOpenDoor = true;
            }
        }
        else if (_state == State.Up)
        {
            if (AddValue < FloorHeight)
            {
                if (isIn)
                {
                    Greta.transform.position += new Vector3(0.0f, Speed, 0.0f);
                    this.transform.position += new Vector3(0.0f, Speed, 0.0f);
                }
                else
                {
                    gameObject.transform.position += new Vector3(0.0f, Speed, 0.0f);
                    //this.transform.position += new Vector3(0.0f, Speed, 0.0f);
                }
                
                AddValue += Speed;
            }
            else
            {
                AddValue = 0;
                Current_floor++;
                Elevator_Out_L = GameObject.FindGameObjectWithTag("Elevator_L" + DirectorStr[Director] + GetCurrentFloor().ToString());
                Elevator_Out_R = GameObject.FindGameObjectWithTag("Elevator_R" + DirectorStr[Director] + GetCurrentFloor().ToString());
                _state = State.Stop;
                isOpenDoor = true;
            }
        }

        if (isOpenDoor)
        {
            if (AddValue < DoorWidth)
            {
                if (Director == 0)
                {
                    Elevator_Out_L.transform.localPosition += new Vector3(DoorSpeed * 0.333f, 0.0f, 0.0f);
                    Elevator_Out_R.transform.localPosition -= new Vector3(DoorSpeed * 0.333f, 0.0f, 0.0f);
                    Elevator_Door_L.transform.localPosition += new Vector3(DoorSpeed, 0.0f, 0.0f);
                    Elevator_Door_R.transform.localPosition -= new Vector3(DoorSpeed, 0.0f, 0.0f);
                }
                else if (Director == 1)
                {
                    Elevator_Out_L.transform.localPosition -= new Vector3(0.0f, 0.0f, DoorSpeed * 0.333f);
                    Elevator_Out_R.transform.localPosition += new Vector3(0.0f, 0.0f, DoorSpeed * 0.333f);
                    Elevator_Door_L.transform.localPosition -= new Vector3(0.0f, 0.0f, DoorSpeed);
                    Elevator_Door_R.transform.localPosition += new Vector3(0.0f, 0.0f, DoorSpeed);
                }
                else if (Director == 2)
                {
                    Elevator_Out_L.transform.localPosition += new Vector3(0.0f, 0.0f, DoorSpeed * 0.333f);
                    Elevator_Out_R.transform.localPosition -= new Vector3(0.0f, 0.0f, DoorSpeed * 0.333f);
                    Elevator_Door_L.transform.localPosition += new Vector3(0.0f, 0.0f, DoorSpeed);
                    Elevator_Door_R.transform.localPosition -= new Vector3(0.0f, 0.0f, DoorSpeed);
                }
                else if (Director == 3)
                {
                    Elevator_Out_L.transform.localPosition -= new Vector3(DoorSpeed * 0.333f, 0.0f, 0.0f);
                    Elevator_Out_R.transform.localPosition += new Vector3(DoorSpeed * 0.333f, 0.0f, 0.0f);
                    Elevator_Door_L.transform.localPosition += new Vector3(DoorSpeed, 0.0f, 0.0f);
                    Elevator_Door_R.transform.localPosition -= new Vector3(DoorSpeed, 0.0f, 0.0f);
                }
                AddValue += DoorSpeed;
            }
            else
            {
                AddValue = 0;
                isOpen = true;
                isOpenDoor = false;
            }
        }

        if (isOpen)
        {
            if (Distance() >= DistanceAway)
                isCloseDoor = true;
        }

        if (isCloseDoor)
        {
            if (AddValue < DoorWidth)
            {
                if (Director == 0)  // || Director == 3
                {
                    Elevator_Out_L.transform.localPosition -= new Vector3(DoorSpeed * 0.333f, 0.0f, 0.0f);
                    Elevator_Out_R.transform.localPosition += new Vector3(DoorSpeed * 0.333f, 0.0f, 0.0f);
                    Elevator_Door_L.transform.localPosition -= new Vector3(DoorSpeed, 0.0f, 0.0f);
                    Elevator_Door_R.transform.localPosition += new Vector3(DoorSpeed, 0.0f, 0.0f);
                }
                else if (Director == 1)
                {
                    Elevator_Out_L.transform.localPosition += new Vector3(0.0f, 0.0f, DoorSpeed * 0.333f);
                    Elevator_Out_R.transform.localPosition -= new Vector3(0.0f, 0.0f, DoorSpeed * 0.333f);
                    Elevator_Door_L.transform.localPosition += new Vector3(0.0f, 0.0f, DoorSpeed);
                    Elevator_Door_R.transform.localPosition -= new Vector3(0.0f, 0.0f, DoorSpeed);
                }
                else if (Director == 2)
                {
                    Elevator_Out_L.transform.localPosition -= new Vector3(0.0f, 0.0f, DoorSpeed * 0.333f);
                    Elevator_Out_R.transform.localPosition += new Vector3(0.0f, 0.0f, DoorSpeed * 0.333f);
                    Elevator_Door_L.transform.localPosition -= new Vector3(0.0f, 0.0f, DoorSpeed);
                    Elevator_Door_R.transform.localPosition += new Vector3(0.0f, 0.0f, DoorSpeed);
                }
                else if (Director == 3)
                {
                    Elevator_Out_L.transform.localPosition += new Vector3(DoorSpeed * 0.333f, 0.0f, 0.0f);
                    Elevator_Out_R.transform.localPosition -= new Vector3(DoorSpeed * 0.333f, 0.0f, 0.0f);
                    Elevator_Door_L.transform.localPosition -= new Vector3(DoorSpeed, 0.0f, 0.0f);
                    Elevator_Door_R.transform.localPosition += new Vector3(DoorSpeed, 0.0f, 0.0f);
                }
                AddValue += DoorSpeed;
            }
            else
            {
                AddValue = 0;
                isOpen = false;
                isCloseDoor = false;
            }
        }
    }

    void OnTriggerEnter(Collider Object)
    {
        if (Object.tag.Equals("Greta"))
        {
            print("in in in in in ");
            isIn = true;
        }
    }

    void OnTriggerExit(Collider Object)
    {
        if (Object.tag.Equals("Greta"))
        {
            print("out out out out ");
            isIn = false;
        }
    }

}
