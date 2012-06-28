using UnityEngine;
using System.Collections;

public class StairRotate : MonoBehaviour
{
    private float Speed = 5.0f;
    private float AddValue = 0.0f;
    private int Active = 0;
    private bool isOpen = false;
    private bool isOpening = false;
    private bool isClosing = false;
    private bool isMotion = false;
    private float tempRotate = 0.0f;


    public void SetOpen()
    {
        GetRotate_X();
        isOpening = true;
    }

    public void SetClose()
    {
        GetRotate_X();
        isClosing = true;
    }

    public bool GetIsOpen()
    {
        return isOpen;
    }

    public bool GetIsMotion()
    {
        return isMotion;
    }

    void GetRotate_X()
    {
        tempRotate = this.transform.localEulerAngles.x;
    }

    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (isOpening)
        {
            isMotion = true;
            Vector3 rotate;

            rotate = this.transform.localEulerAngles;
            rotate.x -= Speed;
            this.transform.localEulerAngles = rotate;  

            AddValue += Speed;

            if (AddValue >= 180)
            {
                rotate = this.transform.localEulerAngles;
                rotate.x = tempRotate - 180.0f;
                this.transform.localEulerAngles = rotate;
                AddValue = 0;
                isOpening = false;
                isOpen = true;
                isMotion = false;
            }
        }

        if (isClosing)
        {
            isMotion = true;
            Vector3 rotate;
            rotate = this.transform.localEulerAngles;
            rotate.x += Speed;
            this.transform.localEulerAngles = rotate;

            AddValue += Speed;

            if (AddValue >= 180)
            {
                rotate = this.transform.localEulerAngles;
                rotate.x = tempRotate + 180.0f;
                this.transform.localEulerAngles = rotate;
                AddValue = 0;
                isClosing = false;
                isOpen = false;
                isMotion = false;
            }
        }
    }
}
