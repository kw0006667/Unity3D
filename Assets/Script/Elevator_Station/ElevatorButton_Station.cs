using UnityEngine;
using System.Collections;

public class ElevatorButton_Station : MonoBehaviour
{
    public int CurrentFloor;

    private GameObject Greta;
    private ElevatorManager_Station manager;

    #region 獲得與Greta間距離
    float getDistance()
    {
        return Vector3.Distance(transform.position, Greta.transform.position);
    }
    #endregion

    #region Initialization
    void Start()
    {
        Greta = GameObject.Find("Greta");
        manager = transform.parent.GetComponent<ElevatorManager_Station>();
        if (manager == null)
            manager = transform.parent.parent.GetComponent<ElevatorManager_Station>();
    }
    #endregion

    #region Mouse Event (MouseDown)
    IEnumerator OnMouseDown()
    {
        if (getDistance() < 2.0f && !manager.isWait)
        {
            if (!manager.doorRunning && !manager.ElevatorRunning)
            {
                if (!manager.isEnter)
                {
                    if (manager.GetCurrentFloor() == CurrentFloor)
                    {
                        if (manager.doorstate == ElevatorManager_Station.DoorState.close)
                            manager.SetdoorState(ElevatorManager_Station.DoorState.open);
                    }
                    else
                    {
                        if (manager.doorstate == ElevatorManager_Station.DoorState.open)
                        {
                            manager.SetdoorState(ElevatorManager_Station.DoorState.close);

                            manager.isWait = true;
                            yield return new WaitForSeconds(5.0f);
                            manager.isWait = false;
                        }

                        if (manager.GetCurrentFloor() == 1)
                            manager.SetFloor(ElevatorManager_Station.ElevatorFloor.two);
                        else
                            manager.SetFloor(ElevatorManager_Station.ElevatorFloor.one);
                    }
                }
                else
                {
                    if (manager.doorstate == ElevatorManager_Station.DoorState.open)
                    {
                        manager.SetdoorState(ElevatorManager_Station.DoorState.close);
                        manager.isWait = true;
                        yield return new WaitForSeconds(5.0f);
                        manager.isWait = false;
                    }

                    if (manager.GetCurrentFloor() == 1)
                        manager.SetFloor(ElevatorManager_Station.ElevatorFloor.two);
                    else
                        manager.SetFloor(ElevatorManager_Station.ElevatorFloor.one);
                }
            }
        }
    }
    #endregion
    
}
