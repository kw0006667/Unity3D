using UnityEngine;
using System.Collections;

class ThirdPersonController_New : MonoBehaviour
{

    public Camera cam;

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

    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float SpeedSmooth = 10.0f;
    public bool canJump = false;

    //move()
    public float MaxVelocity = 10.0f;
    public float nMaxVelocity = -10.0f;
    public float Friction = 0.02f;
    public float Velocity = 0;

    private Vector3 moveDirection = Vector3.zero;

    private float moveSpeed = 0.0f;

    private bool isMouseRButton = false;

    private float x = 0.0f;
    private float y = 0.0f;
    private float x_temp;
    private float y_temp;

    private float v;

    void Start()
    {
        //Time.maximumDeltaTime = 0.0111111f;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isMouseRButton = true;
        }
        if (Input.GetMouseButtonUp(1))
        {
            isMouseRButton = false;
        }

        CharacterController controller = GetComponent<CharacterController>();
        if (controller.isGrounded)
        {
            //We are grounded, so recalculate
            //move direction directly from axes
            Velocity += Input.GetAxis("Vertical") * 0.2f;
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Velocity);
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;

            Move();

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
            
            if (Input.GetButton("Jump") && canJump)
            {
                moveDirection.y = jumpSpeed;
                _characterState = CharacterState.Jumping;
            }
            
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

        Quaternion rotation;
        if (!isMouseRButton)
        {
            x -= Input.GetAxis("Mouse X") * 50 * 0.02f;
            y += Input.GetAxis("Mouse Y") * 120 * 0.02f;

            x_temp = x;
            y = ClampAngle(y, 0, 0);
            y_temp = y;
            rotation = Quaternion.Euler(0, -x - 180.0f, 0);

            transform.rotation = rotation;
        }
        if (isMouseRButton)
        {
            transform.rotation = Quaternion.Euler(0, -x_temp - 180.0f, 0);
        }

        // Apply gravity
        moveDirection.y -= gravity * Time.deltaTime;

        print(Velocity.ToString());
        // Move the controller
        controller.Move(moveDirection * Time.deltaTime);
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

    void Move()
    {
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
