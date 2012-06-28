//-----------------------------------------
// Rolling
// National Taipei University of Education
// Digital Technology Design
// 
// Name : testAI
// Modified Data : 2011/11/23
// Modified By : Tim Chang
// Modified Content
//      1. Add heading comments and region area.
//      2. Add class introduction.
//-----------------------------------------

#region References

using UnityEngine;
using System.Collections;
using System.IO;

#endregion

/// <summary>
/// AI
/// </summary>

public class testAI : MonoBehaviour {

    public int CurrentIdleMode = 0;
    public float ResponseTime = 1.0f;

    private float BufferTimer = 0;    
    private bool isAttackBufferflag = false;
    
    //print angle
    public float angle;
    //print bool isTrigger
    public bool trigger = false;
    public string GretaName ="";

    public Vector3[] IdleMoveMap = new Vector3[] { Vector3.right, Vector3.forward, -Vector3.right, -Vector3.forward };
    private Transform target;
    public float ThreatenDistance = 20.0f;
    public float AttackDistance = 12.0f;    
    public float ChaseSpeed = 100.0f;         //controller enemy of chase speed    
    public float WalkingAnimationSpeed;
    public float RunningAnimationSpeed;

    private float CurrentDistance;
    private Vector3 CurrentDirection;
    private Vector3 gravity = -Vector3.up;    

    private float PlayTime; //save current play time
    private Vector3 OriginPosition;
    private float IdleModeTime = 0;
    public float SearchTime = 0;
    private float TransformActionTime = 3.0f;

    public float  MaxSearchAngel = 65;

    private float findSpeed = 2.0f;
    

    private RaycastHit hit;
    public RaycastHit[] Allhit;
    private bool isAttackFlag = false;

    //public bool isSearchFlag = true;
    private Vector3 FindTargetPosition = Vector3.zero;
    enum Status
    {
        idle = 1, Thraten, Attack
    }
    private Status statusMode = Status.idle;

    private Seeker seeker;
    
    private Vector3[] waypointPosition;
    private bool isSearch = false;
    private int CurrentPath = 0;

    //private float CollisionBufferTime = 1;

    private GretaController Gretacontroller;

    private Transform CurrentCollisionObject;

    //How many distance between AI from Greta that would be died.
    public float DeadDistance = 1.0f;
    public Texture DeadBackground;
    public GUISkin gskin;
    private MouseOrbit mouse;
    private bool dead = false;

    public const string AITAGNAME = "AI";

    void Awake()
    {
        PlayTime = 0;
        OriginPosition = transform.position;
        seeker = GetComponent<Seeker>();
        target = GameObject.FindGameObjectWithTag("Greta").transform;
        Gretacontroller = GameObject.Find("Greta").GetComponent<GretaController>();
        //Gretacontroller = GameObject.FindGameObjectWithTag("Greta").GetComponent<GretaController>();
        mouse = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<MouseOrbit>();
    }

    float patrolDistance(Vector3 v1, Vector3 v2)
    {
        float distance = 0;
        distance = Vector3.Distance(v1, v2);
        return distance;
    }

    void FixedUpdate()
    {
        if (isDead())
            dead = true;
        else
            dead = false;

        PlayTime = Time.time;
        CurrentDirection = target.position + new Vector3(0, 0.0f, 0) - transform.position;
        CurrentDirection.y = 0;
        Debug.DrawRay(transform.position, (target.position + new Vector3(0, 0.0f, 0) - transform.position).normalized * AttackDistance, Color.black);
        //Debug.DrawRay(transform.position,transform.TransformDirection(Vector3.forward));
        CurrentDistance = CurrentDirection.magnitude;

        if (Vector3.Angle(CurrentDirection, transform.TransformDirection(Vector3.forward)) < MaxSearchAngel)
        {
            //print("angle = " + Vector3.Angle(CurrentDirection, transform.TransformDirection(Vector3.forward)));

            if (Physics.Raycast(transform.position, (target.position + new Vector3(0, 0.0f, 0) - transform.position).normalized, out hit, AttackDistance))
            {
                GretaName = hit.collider.name;
                if (hit.collider.tag == "Greta" && !isAttackFlag)
                {
                    angle = hit.distance;
                    isAttackFlag = true;
                    isAttackBufferflag = true;
                }
            }
            else
                angle = 0;
        }

        if (CurrentDistance > ThreatenDistance)    //not enter threaten area.
        {
            Idle();

            isAttackFlag = false;
            isSearch = false;
            isAttackBufferflag = false;
            BufferTimer = 0;
        }

        else if (CurrentDistance <= ThreatenDistance && !isAttackFlag)  // target enter threaten area , start threaten.
        {
            //-------------------------
            Vector3 dirction = target.position + new Vector3(0, 0.0f, 0) - transform.position;
            bool flag = true;
            Allhit = Physics.RaycastAll(transform.position, dirction, dirction.magnitude);
            for (int i = 0; i < Allhit.Length; i++)
            {
                //print(Allhit[i].transform.tag);
                if (Allhit[i].transform.tag == "ceiling")
                {
                    flag = false;
                }
            }

            if (flag)
                Thraten();
            else
                Idle();
            //-------------------------

        }

        if (isAttackFlag) // target enter attack area , start attack.
        {
            if (!isAttackBufferflag)
                Attack();
            else
            {
                BufferTimer += Time.deltaTime;
                if (BufferTimer > ResponseTime)
                {
                    BufferTimer = 0;
                    isAttackBufferflag = false;
                }
            }
        }
	}

    void Update()
    {
        if (statusMode == Status.idle)
        {
            //if (patrolDistance(IdleMoveMap[0], IdleMoveMap[1]) < 0.14f)
            if (IdleMoveMap[CurrentIdleMode].magnitude < 0.14f)
            {
                animation.Play("idle");
            }
            //if (!animation.IsPlaying("Walking"))
            else
            {
                animation["Walking"].wrapMode = WrapMode.Once;
                animation["Walking"].speed = WalkingAnimationSpeed;
                animation.Play("Walking");
            }
        }
        
        if (statusMode == Status.Attack)
        {
            if (!animation.IsPlaying("Running"))
            {
                animation["Running"].wrapMode = WrapMode.Once;
                animation["Running"].speed = RunningAnimationSpeed;
                animation.Play("Running");
            }
        }
    }

    /// <summary>
    /// If Greta is died or not.
    /// </summary>
    /// <returns></returns>
    bool isDead()
    {
        float distance = Vector3.Distance(target.position + new Vector3(0, 0.0f, 0), this.transform.position);

        if (distance < DeadDistance)
            return true;
        else
            return false;
    }

    void loadRecord()
    {
        if (File.Exists(PlayerPrefs.GetString("SaveFileName")))
        {
            using (FileStream fs = new FileStream(PlayerPrefs.GetString("SaveFileName"), FileMode.Open, FileAccess.Read))
            {
                BinaryReader r = new BinaryReader(fs);
                PlayerPrefs.SetString("loadScene", r.ReadString());
                PlayerPrefs.SetFloat("loadP_X", r.ReadSingle());
                PlayerPrefs.SetFloat("loadP_Y", r.ReadSingle());
                PlayerPrefs.SetFloat("loadP_Z", r.ReadSingle());
                r.Close();
            }
            PlayerPrefs.SetInt("GameMode", 2);
        }
        else
            PlayerPrefs.SetInt("GameMode", 1);
    }

    void OnGUI()
    {
        if (gskin)
            GUI.skin = gskin;

        if (dead)
        {
            Time.timeScale = 0.000001f;
            Gretacontroller.LetGameStop(true);
            mouse.SetGameStop(true);

            
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), DeadBackground, ScaleMode.StretchToFill, true);

            if (GUI.Button(new Rect(Screen.width / 2 - 250, Screen.height / 2, 192, 48), "Continue", "MenuButton"))
            {
                dead = false;
                Time.timeScale = 1.0f;
                Gretacontroller.LetGameStop(false);
                mouse.SetGameStop(false);
                loadRecord();
                Application.LoadLevel(PlayerPrefs.GetString("loadScene"));
            }
            if (GUI.Button(new Rect(Screen.width / 2 + 50, Screen.height / 2, 192, 48), "Exit Game", "MenuButton"))
            {
                Time.timeScale = 1.0f;
                dead = false;
                Gretacontroller.LetGameStop(false);
                mouse.SetGameStop(false);
                Application.LoadLevel("StartMenu");
            }
        }

    }

    void HandleWallCollision(Vector3 target, float ChaseSpeed)
    {
        CurrentDirection = target - transform.position;
        CurrentDirection.y = 0;
       
        if (Physics.Raycast(transform.position, CurrentDirection, out hit, CurrentDirection.magnitude) && hit.transform.tag != "Greta")
        {
            if (!isSearch || CurrentPath > waypointPosition.Length - 1)
            {
                isSearch = true;
                seeker.StartPath(transform.position, target);
            }

            Vector3 pos = waypointPosition[CurrentPath] - transform.position;
            pos.y = 0;
            CurrentPath++;
            rigidbody.rotation = Quaternion.LookRotation(pos);
            rigidbody.velocity = pos.normalized * Time.deltaTime * ChaseSpeed;            
            return;
        }

        else
        {
            isSearch = false;
            rigidbody.rotation = Quaternion.LookRotation(CurrentDirection);
            rigidbody.velocity = CurrentDirection.normalized * Time.deltaTime * ChaseSpeed;
        }
    }

    void Attack()                   //handle Attack state
    {
        
        statusMode = Status.Attack;
        //-----------debug test---------------
        //print("發現目標!!!");

        //renderer.material.color = Color.red;
        //------------------------------------

        //isSearchFlag = true;
        trigger = true;
        HandleWallCollision(target.position, ChaseSpeed * 2);        
    }

    void Thraten()                  //handle Thraten state
    {  
        statusMode = Status.Thraten;
        //-----------debug test---------------
        //print("警戒中!!");
        //renderer.material.color = Color.yellow;
        //------------------------------------
        
        //if (isSearchFlag)
        //{        
        FindTargetPosition = target.position;
            //SearchTime = 0;
            //isSearchFlag = false;
        //}
        //SearchTime += Time.deltaTime;    
        
        Vector3 distance = FindTargetPosition - transform.position;//計算與發出聲音的地方位置(FindTargetPosition)之間的距離,並在追逐後開始往回走
        distance.y = 0;

        if (Gretacontroller.Velocity < findSpeed && Gretacontroller.Velocity > -findSpeed)
        {
            Idle();
            return;
        }

        else if (distance.magnitude < AttackDistance && !Physics.Raycast(transform.position, distance, distance.magnitude))
        {            
            isAttackFlag = true;
            isAttackBufferflag = true;
        }

        else
        {
            Debug.DrawRay(transform.position, distance, Color.black);
            rigidbody.rotation = Quaternion.LookRotation(distance);            
        }
        //HandleWallCollision(FindTargetPosition, ChaseSpeed);        

        //if (distance.magnitude < 8 || SearchTime > 5)//找尋一段時間會重新再確認發出聲音的座標
            //isSearchFlag = true;

        //_IdleMode = IdleMode.back;  //切換為back模式,若離開Attack狀態,將在Idle狀態回到原始位置(OriginPosition)
    }

    void Idle()                     //handle idle state
    {  
        statusMode = Status.idle;
        
        //if (PlayTime - IdleModeTime > TransformActionTime && _IdleMode != IdleMode.back)    //一段時間(TransformActionTime)轉換成下一個移動模式
        if (PlayTime - IdleModeTime > TransformActionTime)    //一段時間(TransformActionTime)轉換成下一個移動模式
        {
            IdleModeTime = PlayTime;            //記錄目前的遊戲時間(PlayTime),用於判斷是否切換下一模式用
            if (CurrentIdleMode != IdleMoveMap.Length - 1)
                CurrentIdleMode++;
            else
                CurrentIdleMode = 0;            
        }

        //if (_IdleMode == IdleMode.back)
        //{
        //    Vector3 distance = OriginPosition - transform.position;//計算與起始位置(OriginPosition)之間的距離,並在追逐後開始往回走
        //    distance.y = 0;
        //    HandleWallCollision(OriginPosition, ChaseSpeed);
        //    if (distance.magnitude < 3)//若回到原點將模式切回移動模式
        //    {
        //        CurrentIdleMode = 0;
        //        IdleModeTime = 0;
        //    }
        //    return;
        //} 

        transform.rotation = Quaternion.LookRotation(IdleMoveMap[CurrentIdleMode]);
        rigidbody.velocity = IdleMoveMap[CurrentIdleMode];
    }

    //public Status getCurrentStatus()
    //{
    //    return statusMode;
    //}

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ThreatenDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, AttackDistance);
    }

    public void PathComplete(Vector3[] newPoints)
    {
        waypointPosition = newPoints;
        CurrentPath = 0;
        print(waypointPosition.Length);
        if (waypointPosition.Length == 1)
        {
            isAttackFlag = false;
            isSearch = false;
        }

        //print(waypointPosition.ToString());
    }

    public void PathError()
    {
        print("超出搜尋範圍");
        isAttackFlag = false;
        isSearch = false;
    }

    

}
