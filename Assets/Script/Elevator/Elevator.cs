using UnityEngine;
using System.Collections;

public class Elevator : MonoBehaviour
{
    //---------------public--------------------
    public CharacterController Person;          //人物控制器
    public float ElevatorSpeed = 0.2f;          //電梯移動的速度
    public float ElevatorHeight = 4.4f;         //電梯移動的範圍(樓下到樓上的距離)
    
    //---------------private-------------------
    private float addVaule = 0;                             //增加變量，用於調整電梯的移動
    private float ElevatorSmoothMove = 0;                   //利用三角函式使電梯出現光滑移動   
    private Vector3 ElevatorOriginPosition;                 //電梯起始位置
    private Vector3 PersonCurrentPosition = Vector3.zero;    //人物按下電梯裝置那刻的位置
    private float PersonOrigin_y = 0;
    private float ElevatorSearcharea = 2.5f;                //以電梯物件為中心，搜索範圍內的玩家，因此開啟電梯選單

    //------------------- flag ----------------- 
    private bool isOpenMenu = false;                        //是否開啟電梯控制選單
    public bool isDebugMessage = false;                     //是否開啟除錯訊息

    enum ElevatorState
    {
        Up , Down , Stop
    }
    private ElevatorState Elevatorstate = ElevatorState.Stop;   //判斷現在電梯狀態

    void OnGUI()
    {
        if (isOpenMenu)
        {
            if (GUI.Button(new Rect(Screen.width * 0.5f, Screen.height * 0.4f, Screen.width * 0.15f, Screen.height * 0.1f), "Up") && Elevatorstate == ElevatorState.Stop)
            {
                PersonOrigin_y = Person.transform.position.y;
                Elevatorstate = ElevatorState.Up;
                if(isDebugMessage)
                    print("Elevator is up !!");
            }
            if (GUI.Button(new Rect(Screen.width * 0.5f, Screen.height * 0.6f, Screen.width * 0.15f, Screen.height * 0.1f), "Down") && Elevatorstate == ElevatorState.Stop)
            {
                PersonOrigin_y = Person.transform.position.y - ElevatorHeight;
                Elevatorstate = ElevatorState.Down;
                if (isDebugMessage)
                    print("Elevator is down !!");
            }
            
        }
    }

    void Start()
    {
        ElevatorOriginPosition = transform.position;        
    }

	void Update ()
    {
        //-------------------進入電梯感應範圍開啟選單------------------------
        if (Vector3.Distance(Person.transform.position, collider.transform.position) <= Person.radius + ElevatorSearcharea)
            isOpenMenu = true;

        else
            isOpenMenu = false;
        //----------------------------------------------------------------

        //----------利用三角函式，從 OriginPosition 移動到OriginPosition + ElevatorHeight----------------        
        ElevatorHandle();     
        //---------------------------------------------------------------------------
	}

    void ElevatorHandle()
    {        
        if (Elevatorstate != ElevatorState.Stop)
        {
            PersonCurrentPosition = Person.transform.position;
            PersonCurrentPosition.y = PersonOrigin_y;
            if (Elevatorstate == ElevatorState.Up)
            {
                ElevatorSmoothMove = Mathf.Sin(addVaule * Mathf.PI / 180);
                if (addVaule < 90)
                    addVaule += ElevatorSpeed;
                else
                {
                    Elevatorstate = ElevatorState.Stop;
                    return;
                }
            }

            else if (Elevatorstate == ElevatorState.Down)
            {
                ElevatorSmoothMove = Mathf.Sin(addVaule * Mathf.PI / 180);
                if (addVaule > 0)
                    addVaule -= ElevatorSpeed;
                else
                {
                    Elevatorstate = ElevatorState.Stop;
                    return;
                }
            }

            Person.transform.position = PersonCurrentPosition + new Vector3(0, ElevatorHeight * ElevatorSmoothMove, 0);
            transform.position = ElevatorOriginPosition + new Vector3(0, ElevatorHeight * ElevatorSmoothMove, 0);
        }        
    }

}
