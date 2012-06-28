using UnityEngine;
using System.Collections;
[RequireComponent(typeof(CharacterController))]
public class GretaController : MonoBehaviour
{
    public const string GRETANAME = "Greta";
    
    //-------------------------------------------------
    public Transform LeftWheel;
    public Transform RightWheel;

    private float WheelSpeed = 0;

    //-------------------------------------------------
    
    public float speed = 2.0F;                  //速度倍數
    public float Velocity = 0;                  //瞬間速度    
    public float jumpSpeed = 2.0F;              //跳躍速度
    public float gravity = 9.8F;                //重力(與全域系統無關)
    public float MaxVelocity = 5.0f;            //最大正向速度
    public float nMaxVelocity = -5.0f;          //最大反向速度    
    public float FrictionCoefficient = 0.15f;   //摩擦係數
    public float HandBrakeCoefficient = 0.1f;   //手煞車係數
    public Transform PlayCamera;                //觀看主角的主攝影機
    public float GretaWeight = 40;              //重量
    public bool isCanJump = false;              //是否可跳躍
    
    private Vector3 moveDirection = Vector3.zero;   //移動的方向
    private Vector3 LookDirection;                  //人物看得方向    
    private CharacterController controller;         //人物控制
    private float FrictionForce = 0;                //摩擦力
    

    //------------------- flag -----------------
    private bool isMouseRButton = false;        //是否按下滑鼠右鍵
    private bool isHandBrakeflag = false;       //是否按下滑鼠中鍵(手煞車)
    private bool isInclineRight = false;        //是否在斜坡上按下右鍵
    private bool isLevelRight = false;          //是否在平地上按下右鍵
    public bool isDebugMessage = false;         //是否開啟除錯訊息
    private bool isGameStop = false;            //遊戲是否暫停
    public bool IsGameStop()
    {
        return isGameStop;
    }
    

    //------------ incline --------------------
    private RaycastHit inclineHit;          //斜面法向量
    private float inclineAngle = 0;         //斜面與水平夾角
    private float inclineForce = 0;         //斜面正向力

    enum InclineDirection
    {
        level ,positive, negative
    }
    InclineDirection inclinedirection = InclineDirection.level;
    //--------------------------------------------

    public float pushPower = 2.0F;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.rigidbody;
        //


        if (body == null || body.isKinematic)
            return;

        if (hit.moveDirection.y < -0.3F)
            return;

        //Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        Vector3 pushDir = Vector3.zero;
        RaycastHit lHit;



        if (Mathf.Abs(hit.moveDirection.x) > Mathf.Abs(hit.moveDirection.z))
        {
            if (Mathf.Abs(hit.moveDirection.x) > 0.9f)
                pushDir = new Vector3(hit.moveDirection.x, 0, 0);
            //body.velocity = pushDir * pushPower * Mathf.Abs(Velocity) * 0.2f;
            //print(pushDir);
        }
        else if (Mathf.Abs(hit.moveDirection.x) < Mathf.Abs(hit.moveDirection.z))
        {
            if (Mathf.Abs(hit.moveDirection.z) > 0.9f)
                pushDir = new Vector3(0, 0, hit.moveDirection.z);
            //body.velocity = pushDir * pushPower * Mathf.Abs(Velocity) * 0.2f;
            //print(pushDir);
        }

        //pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        //Debug.DrawRay(body.transform.position, pushDir*1, Color.green);

        if (body.SweepTest(pushDir, out lHit, 0.2f))
            if (string.Compare("Object", 0, lHit.transform.name, 0, 6, true) == 0)
                return;

        body.velocity = pushDir * pushPower * Mathf.Abs(Velocity) * 0.2f ;
    }

    public void LetGameStop(bool stop)
    {
        isGameStop = stop;
    }
    
    
    void Update()
    {
        Application.targetFrameRate = 70;
        controller = GetComponent<CharacterController>();
        
        //---------------handle mouse message-----------------
        if (Input.GetMouseButtonDown(2))
        {
            isHandBrakeflag = true;                   
        }

        if (Input.GetMouseButtonUp(2))
        {
            isHandBrakeflag = false;                   
        }

        if (Input.GetMouseButtonDown(1))
        {
            isMouseRButton = true;
        }

        if (Input.GetMouseButtonUp(1))
        {            
            isMouseRButton = false;            
        }
        
        //---------------------------------------------------

        //----------handle handbrake--------------------------
        if (isHandBrakeflag)
        {
            if (Velocity < -HandBrakeCoefficient)
                Velocity += HandBrakeCoefficient;
            else if (Velocity > HandBrakeCoefficient)
                Velocity -= HandBrakeCoefficient;
            else
                Velocity = 0;
        }
        //--------------------------------------------

        if (controller.isGrounded && isGameStop != true)                              //On the ground to move     
        {
            float Velocity_temp = Input.GetAxis("Vertical") * 0.1f;
            float Velocity_H_temp = Input.GetAxis("Mouse X") * 0.02f;
            
            if (Velocity_temp > 0)
            {
                if (!animation.IsPlaying("Walk_Forward"))
                {
                    animation["Walk_Forward"].wrapMode = WrapMode.Once;

                    animation["Walk_Forward"].speed = 1.4f;
                    animation.Play("Walk_Forward");
                }
            }
            if (Velocity_temp < 0)
            {
                if (!animation.IsPlaying("Walk_Back"))
                {
                    animation["Walk_Back"].wrapMode = WrapMode.Once;
                    animation["Walk_Back"].speed = 1.4f;
                    animation.Play("Walk_Back");
                }
            }
            if (Velocity_temp == 0 && Velocity_H_temp == 0)
            {
                //animation["idle"].wrapMode = WrapMode.Loop;
                //animation.CrossFade("idle");
                if (!animation.IsPlaying("idle") && !animation.IsPlaying("Walk_Forward") && !animation.IsPlaying("Walk_Back") && !animation.IsPlaying("TurnRight") && !animation.IsPlaying("TurnLeft"))
                {
                    animation.Play("HandOnWheel");
                    animation.CrossFade("idle");
                }
               // animation["HandOnWheel"].wrapMode = WrapMode.Loop;
               //animation.CrossFade("HandOnWheel");
            }
            if (Velocity_H_temp > 0.03f)
            {
                if (!isMouseRButton)
                {
                    if (!animation.IsPlaying("TurnRight"))
                    {
                        animation["TurnRight"].wrapMode = WrapMode.Once;
                        animation["TurnRight"].speed = 1.0f;
                        animation.Play("TurnRight");
                    }
                }
            }
            if (Velocity_H_temp < -0.03f)
            {
                if (!isMouseRButton)
                {
                    if (!animation.IsPlaying("TurnLeft"))
                    {
                        animation["TurnLeft"].wrapMode = WrapMode.Once;
                        animation["TurnLeft"].speed = 1.0f;
                        animation.Play("TurnLeft");
                    }
                }
            }

            Velocity += Velocity_temp;
           
            if (Input.GetButton("Jump") && isCanJump)            //handle jumping
                moveDirection.y = jumpSpeed;
            //正向力    N = m*g                                inclineAngle = 斜面夾角
            //斜面     Fi = m*g*sin(inclineAngle)              
            //動摩擦力  fk = u * N * cos(inclineAngle)         u = 摩擦係數   與移動方向相反
            //淨力     F = fk +fi 
            
            //動摩擦力 = FrictionCoefficient * GretaWeight * gravity * Mathf.Cos(inclineAngle * Mathf.PI / 180);
            //假若Velocity 為正,動摩擦力為負

            //斜面力 = GretaWeight * gravity * Mathf.Sin(inclineAngle * Mathf.PI / 180);
            //假若斜面為人物面前,力量為負    在人物背後,力量為正

            InclineForce();                 //handle InclineForce
            MoveFriction();                 //The impact of FrictionCoefficient
            Velocity += (inclineForce + FrictionForce);   //淨力F = fk +fi
            //------------------debug message-------------------
            if (isDebugMessage)
            {
                print("inclineForce = " + inclineForce);
                print("FrictionForce = " + FrictionForce);
                print("inclinedirection = " + inclinedirection);
            }
            //--------------------------------------------------
            //-------微調速度-------
            if (Velocity > MaxVelocity)
                Velocity = MaxVelocity;
            else if (Velocity < nMaxVelocity)
                Velocity = nMaxVelocity;
            else if (Velocity < 0.1f && Velocity > -0.1f)
                Velocity = 0.0f;
            //---------------------------
            
            
            
            moveDirection = new Vector3(0, 0, Velocity) * speed;
        }

        
        if (!isMouseRButton)
        {
            LookDirection = transform.position - PlayCamera.transform.position; //take the vector between camera and object 
            LookDirection.y = 0;
            //---------根據斜面方向,判斷Greta看得方向---------
            if (inclinedirection == InclineDirection.positive)
                LookDirection.y = -Mathf.Sin(inclineAngle * Mathf.PI / 180) * LookDirection.magnitude;
            else if (inclinedirection == InclineDirection.negative)
                LookDirection.y = Mathf.Sin(inclineAngle * Mathf.PI / 180) * LookDirection.magnitude;
            //-----------------------------------------------   
            transform.rotation = Quaternion.LookRotation(LookDirection);    //handle greta look direction           
            
        }
        //--------------------------------有按"右鍵"觀察人物時，Greta看得方向修正-------------------------------------------        
        //-----------------假如在斜面上持續按右鍵觀察且遇到平地，修正人物看的方向------------------
        if (isInclineRight && inclinedirection == InclineDirection.level)
        {
            isInclineRight = false;
            Vector3 direction = LookDirection;
            direction.y = 0;
            transform.rotation = Quaternion.LookRotation(direction);    //handle greta look direction
        }
        //----------------------------------------------------------------------

        //-----------------假如在平面上持續按右鍵觀察且遇到上下坡，修正人物看的方向------------------
        if (isLevelRight && inclinedirection != InclineDirection.level)
        {
            isLevelRight = false;
            Vector3 direction = LookDirection;
            direction.y = 0;
            //---------根據斜面方向,判斷Greta看得方向---------
            if (inclinedirection == InclineDirection.positive)
                direction.y = -Mathf.Sin(inclineAngle * Mathf.PI / 180) * direction.magnitude;
            else if (inclinedirection == InclineDirection.negative)
                direction.y = Mathf.Sin(inclineAngle * Mathf.PI / 180) * direction.magnitude;
            //-----------------------------------------------   
            transform.rotation = Quaternion.LookRotation(direction);    //handle greta look direction
        }
        //----------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        WheelSpeed += Velocity;
        LeftWheel.localRotation = Quaternion.Euler(-WheelSpeed, 180, 180);
        RightWheel.localRotation = Quaternion.Euler(-WheelSpeed, 180, 180);

        moveDirection.y -= gravity *0.015f;                    //The impact of gravity 
        controller.Move(transform.TransformDirection( moveDirection) * Time.deltaTime); //handle move

        
    }

    void InclineForce()
    {
        Ray ray = new Ray(transform.position, -Vector3.up);
        //---------debug normal vector ----------
        Debug.DrawRay(ray.origin, ray.direction, Color.black);

        //--------------------------get inclineAngle--------------------
        if (Physics.Raycast(ray, out inclineHit, 2))
        {
            //---------debug normal vector ----------
            Debug.DrawRay(inclineHit.point, inclineHit.normal * 5, Color.blue);

            inclineAngle = Vector3.Angle(-ray.direction, inclineHit.normal);
            if (isDebugMessage)
                print("inclineAngle = " + inclineAngle);
            if (inclineAngle == 0)
            {
                //------------判別是否在斜坡上按右鍵，如果有開啟修正---------------
                if (inclinedirection != InclineDirection.level && isMouseRButton)                
                    isInclineRight = true;
                //-----------------------------------------------------------
                inclinedirection = InclineDirection.level;
                inclineForce = 0;
                return;
            }
            //------------判別是否在平地上按右鍵，如果有開啟修正---------------
            else
                if (inclinedirection == InclineDirection.level && isMouseRButton)
                    isLevelRight = true;
            //-----------------------------------------------------------
        }
        //----------------------------------------------------------------        
        ray = new Ray(transform.position - new Vector3(0, 0.4f, 0), transform.TransformDirection(Vector3.forward));
        Debug.DrawRay(ray.origin, ray.direction, Color.yellow);
        
        //--------------------------handle negative incline---------------------------------------
        if (Physics.Raycast(ray, out inclineHit, 1) && inclineHit.collider.tag == "Finish")       
        {
            inclinedirection = InclineDirection.negative;
            inclineForce = -GretaWeight * gravity * Mathf.Sin(inclineAngle * Mathf.PI / 180) / 1000;
            return;
        }
        //--------------------------handle positive incline---------------------------------------           
        else if (Physics.Raycast(ray.origin, -ray.direction, out inclineHit, 1) && inclineHit.collider.tag == "Finish")
        {
            Debug.DrawRay(ray.origin, -ray.direction, Color.yellow);         
            inclinedirection = InclineDirection.positive;
            inclineForce = GretaWeight * gravity * Mathf.Sin(inclineAngle * Mathf.PI / 180) / 1000;            
        }
        
    }

    void MoveFriction()         //處理摩擦力,與速度相反
    {
        if (Velocity > 0)
            FrictionForce = -FrictionCoefficient * GretaWeight * gravity * Mathf.Cos(inclineAngle * Mathf.PI / 180) / 1000;
        else if (Velocity < 0)
            FrictionForce = FrictionCoefficient * GretaWeight * gravity * Mathf.Cos(inclineAngle * Mathf.PI / 180) / 1000;
    }
}