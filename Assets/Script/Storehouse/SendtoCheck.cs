/*
 * file: SendtoCheck.cs
 * last update: 2011/5/18  
 * version : 1.0
*/

using UnityEngine;
using System.Collections;

public class SendtoCheck : MonoBehaviour
{
    public bool isEnter = false;
    
    public Transform tran;
    private StorehouseCheck StorehouseCheck_script;

    void OnTriggerEnter(Collider other)
    {

        if (string.Compare(other.tag, "Storehouse", true) == 0) 
            tran = other.transform;     
    }

    void OnTriggerExit(Collider other)
    {
        if (tran == other.transform)
            tran = transform;   // remove object
    }

    void OnTriggerStay(Collider other)
    {
        if (tran == transform)
            tran = other.transform;

        if (tran == other.transform)
        {
            //print(Vector3.Distance(tran.position, transform.position));
            
            if (Vector3.Distance(tran.position, transform.position) < transform.localScale.x / 2.0f)
            {
                tran.position = new Vector3(Mathf.Lerp(tran.position.x, transform.position.x, 0.06f), transform.position.y, Mathf.Lerp(tran.position.z, transform.position.z, 0.06f));
                
                isEnter = true;
                StorehouseCheck_script.CheckState();
            }
            else            
                isEnter = false;            
        }
    }

    public void Vt(Vector3 v1)
    {
        
    }

    

    void Start()
    {
        GameObject gameobject;
        gameobject = transform.parent.gameObject;
        StorehouseCheck_script = gameobject.GetComponent<StorehouseCheck>();
    }
	
}
