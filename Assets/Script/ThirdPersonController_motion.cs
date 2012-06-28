using UnityEngine;
using System.Collections;

//[RequireComponent(typeof(CharacterController))]

public class ThirdPersonController_motion : MonoBehaviour
{

    public AnimationClip idleAnimation;
    public AnimationClip walkAnimation_foward;
    public AnimationClip walkAnimation_back;
    public AnimationClip runningAnimation_foward;
    public AnimationClip runningAnimation_back;
    public AnimationClip turnleftAnimation;
    public AnimationClip turnrightAnimation;
    public AnimationClip jumpAnimation;
    
    public float walkMaxAnimationSpeed = 0.75f;
    public float runMaxAnimationSpeed = 1.0f;
    public float jumpAnimationSpeed = 1.15f;
    public float turnMaxAnimationSpeed = 0.75f;

    private Animation _animation;

    public float speed = 5.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 9.8F;

    public float MaxVelocity = 5.0f;                
    public float nMaxVelocity = -5.0f;              
    public float Friction = 0.03f;
    public float Velocity = 0;

    public Transform camera;
    public bool canJump = false;

    private Vector3 moveDirection = Vector3.zero;
    private bool isMouseRButton = false;
    private Quaternion rotation;

    enum CharacterState
    {
        Idle = 0,
        Walking_Foward = 1,
        Walking_Back = 2,
        Running_Foward = 3,
        Running_Back = 4,
        TurnLeft = 5,
        TurnRight = 6,
        Jumping = 7,
    }

    private CharacterState _characterState;

    void Update()
    {       
        if (Input.GetMouseButtonDown(2))
        {
            Velocity = 0;                   //Hand brake
        }

        if (Input.GetMouseButtonDown(1))
        {
            isMouseRButton = true;
        }
        if (Input.GetMouseButtonUp(1))
        {            
            isMouseRButton = false;            
        }


        CharacterController controller = GetComponent<CharacterController>();
        if (controller.isGrounded)                              //On the ground to move     
        {

            Velocity += Input.GetAxis("Vertical") * 0.2f;

            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Velocity);
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;

            if (Input.GetButton("Jump") && canJump)
            {
                moveDirection.y = jumpSpeed;
                _characterState = CharacterState.Jumping;
            }

        }

        MoveFriction();                 //The impact of friction

        // Change the CharacterState by Velocity Speed
        _characterState = CharacterState.Idle;

        if (Velocity > 0)
        {
            if (Velocity >= 5)
                _characterState = CharacterState.Running_Foward;
            else
                _characterState = CharacterState.Walking_Foward;
        }
        else
        {
            if (Velocity <= -5)
                _characterState = CharacterState.Running_Back;
            else
                _characterState = CharacterState.Walking_Back;
        }

        //Animation sector
        if (_animation)
        {
            if (_characterState == CharacterState.Jumping)
            {
                _animation[jumpAnimation.name].speed = jumpAnimationSpeed;
                _animation[jumpAnimation.name].wrapMode = WrapMode.ClampForever;
                _animation.CrossFade(jumpAnimation.name);
            }
            else
            {
                if (_characterState == CharacterState.Idle)
                {
                    _animation.CrossFade(idleAnimation.name);
                }
                else
                {
                    if (_characterState == CharacterState.Walking_Foward)
                    {
                        _animation[walkAnimation_foward.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0f, walkMaxAnimationSpeed);
                        _animation.CrossFade(walkAnimation_foward.name);
                    }
                    else if (_characterState == CharacterState.Walking_Back)
                    {
                        _animation[walkAnimation_back.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0f, walkMaxAnimationSpeed);
                        _animation.CrossFade(walkAnimation_back.name);
                    }
                    else if (_characterState == CharacterState.Running_Foward)
                    {
                        _animation[runningAnimation_foward.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0f, runMaxAnimationSpeed);
                        _animation.CrossFade(runningAnimation_foward.name);
                    }
                    else if (_characterState == CharacterState.Running_Back)
                    {
                        _animation[runningAnimation_back.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0f, runMaxAnimationSpeed);
                        _animation.CrossFade(runningAnimation_back.name);
                    }
                    else if (_characterState == CharacterState.TurnLeft)
                    {
                        _animation[turnleftAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0f, turnMaxAnimationSpeed);
                        _animation.CrossFade(turnleftAnimation.name);
                    }
                    else if (_characterState == CharacterState.TurnRight)
                    {
                        _animation[turnrightAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0f, turnMaxAnimationSpeed);
                        _animation.CrossFade(turnrightAnimation.name);
                    }
                }
            }
        }

        

        if (!isMouseRButton)
        {
            Vector3 direction;
            direction = transform.position - camera.transform.position; //take the vector between camera and object 
            direction.y = 0;
            rotation = Quaternion.LookRotation(direction);              //handle move direction          
            transform.rotation = rotation;
        }

        moveDirection.y -= gravity * Time.deltaTime;                    //The impact of gravity 
        controller.Move(moveDirection * Time.deltaTime);                //handle move

    }

    void MoveFriction()
    {

        Friction = Time.deltaTime;

        if (Velocity > MaxVelocity)
            Velocity = MaxVelocity;        

        if (Velocity > 0)
            Velocity -= Friction;        

        else if (Velocity < Friction && Velocity > -Friction)
            Velocity = 0;
        
        else if (Velocity < nMaxVelocity)
            Velocity = nMaxVelocity;
        
        else
            Velocity += Friction;        
    }
}