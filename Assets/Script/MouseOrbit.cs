using UnityEngine;
using System.Collections;

class MouseOrbit : MonoBehaviour
{

    public Transform target;
    public Transform player;
    public Transform Radar;
    public Camera radarcam;
    public LayerMask hitLayer;
    public Vector3 targetOffset = Vector3.zero;

    public float distance = 2.0f;
    public float xSpeed = 250.0f;
    public float ySpeed = 120.0f;
    public float yMinlimit = -20.0f;
    public float yMaxlimit = 80.0f;
    public float ylimit = 20.0f;

    public LayerMask lineOfSightMask = 0;
    public float closerRadius = 0.2f;
    public float closerSnapLag = 0.2f;

    private float x = 0.0f;
    private float y = 0.0f;
    private float x_temp = 0.0f;
    private float yMaxl, yMinl, dis;
    private float currentDistance = 10.0f;
    private float distanceVelocity = 0.0f;

    private int count = 0;
    public float maxHeight = 0.4f;
    private bool isMouseRButton = false;
    private bool isGameStop = false;
    private Quaternion rotation_temp;
    private Vector3 position_temp;
    //public string status;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        
        yMaxl = yMaxlimit;
        yMinl = yMinlimit;
        dis = distance;

        //radarcam.enabled = true;

        // Make the rigid body not change rotation
        if (rigidbody)
            rigidbody.freezeRotation = true;
    }

    public void SetGameStop(bool Stop)
    {
        isGameStop = Stop;
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            x_temp = x;
            isMouseRButton = true;
        }
        if (Input.GetButtonUp("Fire2"))
        {
            x = x_temp;
            isMouseRButton = false;
        }
        
        
        if (target && player && isGameStop != true)
        {
            
            
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            if (isMouseRButton)
                y = ClampAngle(y, yMinl, yMaxl);
            if (!isMouseRButton)
                y = ClampAngle(y, ylimit, ylimit);
            /*dis = distance;
            yMaxl = yMaxlimit;
            yMinl = yMinlimit;
             */
            Quaternion rotation = Quaternion.Euler(y, x, 0);
            //Vector3 targetPos = target.position + targetOffset;
            Vector3 targetPos = target.position + transform.TransformDirection(targetOffset);
            Vector3 direction = rotation * -Vector3.forward;

            

            //Vector3 position = rotation * new Vector3(0.0f, 0.0f, -dis) + target.position;


            float targetDistance = AdjustLineOfSight(ref targetPos, direction);
            currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref distanceVelocity, closerSnapLag * 0.3f);

            transform.rotation = rotation;
            transform.position = targetPos + direction * currentDistance;

            if (Radar != null)
            {
                Radar.position = player.position;
                radarcam.transform.rotation = Quaternion.Euler(90, x, 0);
            }
        }
    }

    float AdjustLineOfSight(ref Vector3 targetPos, Vector3 direction)
    {
        //RaycastHit hit;
        //if (Physics.Raycast(target, direction, out hit, distance, lineOfSightMask.value))
        //    return hit.distance - closerRadius;
        //else
        //    return distance;
        RaycastHit hit;
        if (Physics.Raycast(targetPos, direction, out hit, distance, lineOfSightMask.value))
        {
            if (!isMouseRButton)
            {
                //print("dis = " + hit.distance);
                float t = hit.distance - closerRadius;

                targetPos += new Vector3(0, Mathf.Lerp(maxHeight, 0, Mathf.Clamp(t, 0.0f, 1.0f)), 0);
                //print("y = " + targetPos.y);
                //y = Mathf.Lerp(1, 0, Mathf.Clamp(t, 0.0f, 1.0f));
                //y = Mathf.Lerp(45, ylimit, Mathf.Clamp(t, 0.0f, 1.0f));
                //print("y = " + y);
                return hit.distance - closerRadius;
            }
            else
            {
                return hit.distance - closerRadius;
            }
        }
        else
            return distance;
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}


