using UnityEngine;
using System.Collections;
[RequireComponent(typeof(CharacterController))]
public class testAI_1204 : MonoBehaviour {

    public Vector3[] IdleMoveMap = new Vector3[] { Vector3.right, Vector3.forward, -Vector3.right, -Vector3.forward };
    public Transform target;
    public float ThreatenDistance = 20.0f;
    public float AttackDistance = 12.0f;    
    public float ChaseSpeed = 10.0f;         //controller enemy of chase speed    

    private CharacterController controller;

    private float CurrentDistance;
    private Vector3 CurrentDirection;
    private Vector3 gravity = -Vector3.up;    

    private float PlayTime; //save current play time
    private Vector3 OriginPosition;
    private float IdleModeTime = 0;
    private float TransformActionTime = 5.0f;

    public float AddAngel = 1;
    public float MaxSearchAngel = 45;
    private float CurrentAngel = 0;

    private RaycastHit hit;
    private bool isAttackFlag = false;

    enum IdleMode                   
    {
        one = 1, two, three, four, back //用於Idle狀態判別用
    }
    private IdleMode _IdleMode = IdleMode.one;

    void Awake()
    {
        PlayTime = 0;
        OriginPosition = transform.position;
        controller = GetComponent<CharacterController>();
    }    

    // Update is called once per frame
	void Update () 
    {
        CurrentAngel += AddAngel;
        if (CurrentAngel > MaxSearchAngel)
            AddAngel = -AddAngel;                   

        else if (CurrentAngel < -MaxSearchAngel)
            AddAngel = -AddAngel;
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(Mathf.Sin(CurrentAngel * Mathf.PI / 180), 0, Mathf.Cos(CurrentAngel * Mathf.PI / 180))) * AttackDistance, Color.yellow);

        CurrentDirection = target.position - transform.position;
        Debug.DrawRay(transform.position, CurrentDirection);
        CurrentDistance = CurrentDirection.magnitude;
        PlayTime = Time.time;                                       //save current play time

        //-------------------------------
        if (Physics.Raycast(transform.position, transform.TransformDirection(new Vector3(Mathf.Sin(CurrentAngel * Mathf.PI / 180), 0, Mathf.Cos(CurrentAngel * Mathf.PI / 180))), out hit, AttackDistance))
            if (hit.transform.tag == "Player")
                isAttackFlag = true;
        //--------------------------------
        if (controller.isGrounded)
        {
            if (CurrentDistance > ThreatenDistance)    //not enter threaten area.
            {
                Idle();
                isAttackFlag = false;                
            }

            else if (CurrentDistance <= ThreatenDistance && !isAttackFlag)  // target enter threaten area , start threaten.
            {                
                Thraten();                
                //transform.LookAt(target);
            }

            else if (isAttackFlag) // target enter attack area , start attack.
            {
                Attack();                                
            }
        }

        controller.SimpleMove(gravity);     //handle gravity
	}

    void Attack()                   //handle Attack state
    {
        print("發現目標!!!");
        renderer.material.color = Color.red;
        transform.rotation = Quaternion.LookRotation(CurrentDirection - new Vector3(0, CurrentDirection.y, 0));
        controller.SimpleMove(CurrentDirection / CurrentDistance * ChaseSpeed);
    }

    void Thraten()                  //handle Thraten state
    {
        print("警戒中!!");
        renderer.material.color = Color.yellow;
        transform.rotation = Quaternion.LookRotation(CurrentDirection - new Vector3(0, CurrentDirection.y, 0));
        //緩慢向發出聲音的地方移動
        controller.SimpleMove(transform.TransformDirection(Vector3.forward) * Time.deltaTime * ChaseSpeed);

        _IdleMode = IdleMode.back;  //切換為back模式,若離開Attack狀態,將在Idle狀態回到原始位置(OriginPosition)
    }


    void Idle()                     //handle idle state
    {
        renderer.material.color = Color.blue;

        if (PlayTime - IdleModeTime > TransformActionTime && _IdleMode != IdleMode.back)    //一段時間(TransformActionTime)轉換成下一個移動模式
        {
            IdleModeTime = PlayTime;            //記錄目前的遊戲時間(PlayTime),用於判斷是否切換下一模式用
            if (_IdleMode != IdleMode.four)
                _IdleMode++;
            else
                _IdleMode = IdleMode.one;            
        }

        switch (_IdleMode)      //(_IdleMode)四種移動模式 + 追逐後回到原點模式
        {
            case IdleMode.one:  //Mode 1:                
                //transform.eulerAngles = Vector3.RotateTowards(new Vector3(0, 0, 0), new Vector3(0,Mathf.Cos(Time.time), 0), 45*Mathf.PI/180, 1) * 100;
                transform.rotation = Quaternion.LookRotation(IdleMoveMap[0]);
                controller.SimpleMove(IdleMoveMap[0]);
                break;

            case IdleMode.two:  //Mode 2:
                transform.rotation = Quaternion.LookRotation(IdleMoveMap[1]);
                controller.SimpleMove(IdleMoveMap[1]);
                break;

            case IdleMode.three://Mode 3:
                transform.rotation = Quaternion.LookRotation(IdleMoveMap[2]);
                controller.SimpleMove(IdleMoveMap[2]);
                break;

            case IdleMode.four: //Mode 4:
                transform.rotation = Quaternion.LookRotation(IdleMoveMap[3]);
                controller.SimpleMove(IdleMoveMap[3]);
                break;

            case IdleMode.back:
                Vector3 distance = OriginPosition - transform.position;//計算與起始位置(OriginPosition)之間的距離,並在追逐後開始往回走
                distance.y = 0;
                transform.rotation = Quaternion.LookRotation(distance);
                controller.SimpleMove(distance * Time.deltaTime * ChaseSpeed);
                if (distance.magnitude < 3)//若回到原點將模式切回四種移動模式
                {
                    _IdleMode = 0;
                    IdleModeTime = 0;
                }
                break;
        }    

    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ThreatenDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, AttackDistance);
    }

}
