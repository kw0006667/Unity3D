using UnityEngine;
using System.Collections;

class Timer : MonoBehaviour
{
    private static float times;
    public static float deltaTime { get { return Time.deltaTime/times; } }
    
    private float t;
    private float s;

    
    void Start()
    {
        s = Time.time;
    }

    void Update()
    {
        t = Time.time - s;

        times = t / 0.016f;

        s = Time.time;

    }

    void OnGUI()
    {
        //GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 25, 200, 50), times.ToString() + " = " + t.ToString() + " / 0.016");
    }


}
