using UnityEngine;
using System.Collections;

public class OpenDoor : MonoBehaviour
{

    public float MinDistance = 1.5f;
    public float OpeningSpeed = 1.0f;

    public bool isCanOpen = true;

    private bool isOpen = false;
    private bool isTrigger = false;
    private bool onMouseClick = false;
    private bool isMotion = false;
    private bool isFront = true;
    private bool isOpenIn;

    private float Rotate_X = 0;
    private float Rotate_Y = 0;
    private float Rotate_Z = 0;
    private float AngleCount = 0;
    private float TriggerDistance = 10.0f;
    private GameObject player;
    private GameMenu gm;

    public Texture[] DoorBlinTexture = new Texture[2];

    private AstarPath ASPath;
    private Event e;


    //----Function Prototype----------

    /// <summary>
    /// The distance between Door and Player
    /// </summary>
    /// <returns>distance</returns>
    float Trigger()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        return distance;
    }

    bool isTriggerDistance()
    {
        return (TriggerDistance >= Vector3.Distance(transform.position, player.transform.position));
    }

    //void OnMouseDown()
    //{
    //    if (!isCanOpen)
    //        if (Trigger() <= MinDistance)
    //            onMouseClick = true;
    //}

    /// <summary>
    /// If you Click Mouse Left Button, you will call this function.
    /// </summary>
    void MouseLeftButton()
    {
        if (!isCanOpen)
            if (Trigger() <= MinDistance)
                onMouseClick = true;
    }

    public void opDoor(bool op)
    {
        if (op)
        {
            onMouseClick = op;
        }
    }

    /// <summary>
    /// judge the player whether is front of the door or not
    /// </summary>
    void ForB()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 toOther = transform.position - player.transform.position;

        if (Vector3.Dot(forward, toOther) > 0)
            isFront = false;
        else
            isFront = true;

    }


    // Use this for initialization
    void Start()
    {
        Rotate_X = transform.localEulerAngles.x;
        Rotate_Y = transform.localEulerAngles.y;
        Rotate_Z = transform.localEulerAngles.z;

        player = GameObject.FindGameObjectWithTag("Greta");

        ASPath = GameObject.Find("AstarPathing01").GetComponent<AstarPath>();
        //gm = GameObject.Find("Person/Main Camera").GetComponent<GameMenu>();
        //gm = Camera.current.GetComponent<GameMenu>();
        gm = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<GameMenu>();

        OpeningSpeed = 1.75f;

        //if (DoorBlinTexture[2])
        //this.renderer.material.SetTexture("_MainTex", DoorBlinTexture[2]);

    }

    bool isEmptyDoorMaterial()
    {
        if (isCanOpen)
            return false;
        if (DoorBlinTexture.Length <= 0)
            return false;
        return true;
    }

    private bool isButtonUp = true;

    private void isRightButtonUp(bool t)
    {
        isButtonUp = t;
    }
    

    void OnGUI()
    {
        e = Event.current;
        if (e.isMouse)
        {
            if (e.button.Equals(0) && !gm.isOpenMenu)
                MouseLeftButton();
            if (e.button.Equals(1) && !gm.isOpenMenu)
            {
                if (!isCanOpen && isTriggerDistance() && DoorBlinTexture.Length >= 2)
                {
                    this.renderer.material.SetTexture("_MainTex", DoorBlinTexture[1]);
                    print("Highlight");
                }
            }
        }

        if (e.type.Equals(EventType.MouseDown))
            isRightButtonUp(false);
        if (e.type.Equals(EventType.MouseUp))
            isRightButtonUp(true);
    }

    // Update is called once per frame
    void Update()
    {



        if (isEmptyDoorMaterial() && isButtonUp)
        {
            this.renderer.material.SetTexture("_MainTex", DoorBlinTexture[0]);
        }

        ForB();

        //whether player is front of the door MinDistance or not
        if (Trigger() <= MinDistance || isMotion)
        {

            if (!isOpen || isMotion)
            {
                if (isEmptyDoorMaterial() && Trigger() <= MinDistance)
                {
                    this.renderer.material.SetTexture("_MainTex", DoorBlinTexture[1]);
                }
                if (isFront)
                {
                    if (onMouseClick)
                    {
                        isMotion = true;
                        Vector3 rotate;
                        rotate = this.transform.localEulerAngles;
                        rotate.y += OpeningSpeed;
                        this.transform.localEulerAngles = rotate;

                        AngleCount += OpeningSpeed;


                        if (AngleCount >= 90)
                        {
                            rotate = this.transform.localEulerAngles;
                            rotate.y = Rotate_Y + 90;
                            this.transform.localEulerAngles = rotate;
                            isOpen = true;
                            onMouseClick = false;
                            AngleCount = 0;
                            isMotion = false;
                            isOpenIn = true;
                        }
                    }
                }
                else
                {
                    if (onMouseClick)
                    {
                        isMotion = true;
                        Vector3 rotate;
                        rotate = transform.localEulerAngles;
                        rotate.y -= OpeningSpeed;
                        transform.localEulerAngles = rotate;

                        AngleCount -= OpeningSpeed;

                        if (AngleCount <= -90)
                        {
                            rotate = transform.localEulerAngles;
                            rotate.y = Rotate_Y - 90;
                            transform.localEulerAngles = rotate;
                            isOpen = true;
                            onMouseClick = false;
                            AngleCount = 0;
                            isMotion = false;
                            isOpenIn = false;
                        }
                    }
                }
            }
        }
        else
        {
            if (isOpen && (!isMotion))
            {
                if (isOpenIn)
                {
                    Vector3 rotate;
                    rotate = transform.localEulerAngles;
                    rotate.y -= OpeningSpeed;
                    transform.localEulerAngles = rotate;

                    AngleCount -= OpeningSpeed;

                    if (AngleCount <= -90)
                    {
                        rotate = transform.localEulerAngles;
                        rotate.x = Rotate_X;
                        rotate.y = Rotate_Y;
                        rotate.z = Rotate_Z;
                        transform.localEulerAngles = rotate;
                        isOpen = false;
                        AngleCount = 0;
                        isMotion = true;
                    }
                }
                else
                {
                    Vector3 rotate;
                    rotate = transform.localEulerAngles;
                    rotate.y += OpeningSpeed;
                    transform.localEulerAngles = rotate;

                    AngleCount += OpeningSpeed;

                    if (AngleCount >= 90)
                    {
                        rotate = transform.localEulerAngles;
                        rotate.y = Rotate_Y;
                        transform.localEulerAngles = rotate;
                        isOpen = false;
                        AngleCount = 0;
                        isMotion = true;
                    }
                }
            }
        }
    }



}


