/*
 * file: StorehouseCheck.cs
 * last update: 2011/5/18  
 * version : 1.0 
 * 
 * 利用list儲存所有checkpoint的狀態，當全部  isEnter = true , isopen = true;
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StorehouseCheck : MonoBehaviour
{
    private class CheckData
    {
        public string ObjectName;        
        public SendtoCheck SendtoCheck_script;
        public CheckData(string name)
        {
            ObjectName = name;
            SendtoCheck_script = GameObject.Find(ObjectName).GetComponent<SendtoCheck>();
        }
    }
    private List <CheckData> Checkpoint = new List<CheckData>();

    public bool isOpen = false;

    public GameObject door;
    private OpenDoor door_script;

    public void CheckState()
    {
        //for (int i = 0; i < Checkpoint.Count; i++)
        //    if (!Checkpoint[i].SendtoCheck_script.isEnter)
        //        return;

        foreach (CheckData Ch in Checkpoint)
        {
            if (!Ch.SendtoCheck_script.isEnter)
                return;
        }

        isOpen = true;
    }

    void Start()
    {
        Transform[] trans;
        trans = GetComponentsInChildren<Transform>();
        for (int i = 1; i < trans.Length; i++)
        {
            //print(trans[i].name);
            Checkpoint.Add(new CheckData(trans[i].name));
        }

        door_script = door.GetComponent<OpenDoor>();
        
    }

	void Update ()
    {

        if (isOpen)
        {
            door_script.opDoor(true);
            door_script.isCanOpen = false;
            //------------通過倉庫番 , handle  event---------
        }
        else
        {
            door_script.isCanOpen = true;
        }
    }
}
