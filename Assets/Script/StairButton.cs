using UnityEngine;
using System.Collections;

public class StairButton : MonoBehaviour {

    public int Side;
    public int Director;
    public int Floor;
    public GameObject[] Block;

    private float MaxDistance = 3.0f;
    private const string STAIR_NAME = "Stair_";
    private string[] DirectorStr = { "N", "E", "W", "S" };
    private string[] SideStr = { "L", "R" };
    private GameObject[] stairs;
    private StairRotate[] Stairs_script;

	// Use this for initialization
	void Start () {
        stairs = GameObject.FindGameObjectsWithTag(STAIR_NAME + SideStr[Side] + DirectorStr[Director] + Floor.ToString());
        int i = 0;
        //foreach (GameObject obj in stairs)
        //{
        //    Stairs_script[i] = obj.GetComponentInChildren<StairRotate>();
        //    i++;
        //}

        //for (; i < stairs.Length; i++)
        //{
        //    Stairs_script[i] = stairs[i].GetComponentInChildren<StairRotate>();
        //}
        print(stairs.Length.ToString());
	}

    // Update is called once per frame
    void Update()
    {
        if (stairs[0].GetComponentInChildren<StairRotate>().GetIsOpen())
        {
            Block[0].active = false;
            Block[1].active = false;
        }
        else
        {
            Block[0].active = true;
            Block[1].active = true;
        }
	}

    void OnMouseDown()
    {
        if (!stairs[0].GetComponentInChildren<StairRotate>().GetIsMotion())
        {
            if (!stairs[0].GetComponentInChildren<StairRotate>().GetIsOpen())
            {
                foreach (GameObject obj in stairs)
                {
                    obj.GetComponentInChildren<StairRotate>().SetOpen();
                }
            }
            else
            {
                foreach (GameObject obj in stairs)
                {
                    obj.GetComponentInChildren<StairRotate>().SetClose();
                }
            }
        }
    }

}
