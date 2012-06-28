using UnityEngine;
using System.Collections;

public class Compass : MonoBehaviour
{
    public GameObject Greta;
    public Rect rec;

    public GUITexture COMPASSTexture;
    public Material COMPASSMtl;
    
	
    void Start () 
    {
        Greta = GameObject.Find("Greta");
        
            COMPASSTexture.transform.position = Vector3.zero;
            COMPASSTexture.transform.localScale = Vector3.zero;
            COMPASSTexture.renderer.material = COMPASSMtl;
            COMPASSTexture.renderer.material.color = new Color(1, 1, 1, 1);
            rec = new Rect(-5, 20, 200, 200);
            COMPASSTexture.pixelInset = rec;
       
	}
	
	void Update () 
    {        
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x,Greta.transform.rotation.eulerAngles.y,0));
        //COMPASSTexture.pixelInset = new Rect(20, Screen.height - 110, 100, 100);
        //rec = new Rect(5, 20, 200, 200);
        COMPASSTexture.transform.position = Vector3.zero;
        COMPASSTexture.transform.localScale = Vector3.zero;
        COMPASSTexture.renderer.material = COMPASSMtl;
        COMPASSTexture.renderer.material.color = new Color(1, 1, 1, 1);
        rec = new Rect(-5, 20, 200, 200);
        COMPASSTexture.pixelInset = rec;
        //this.camera.rect = rec;
	}

    void OnGUI()
    {
        
    }
}
