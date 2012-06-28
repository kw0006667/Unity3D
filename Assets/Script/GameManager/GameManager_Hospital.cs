using UnityEngine;
using System.Collections;

public class GameManager_Hospital : MonoBehaviour {

    public GUISkin GameManager;
    private GameObject Greta;

    private int GameMode;

    private float Greta_Position_X;
    private float Greta_Position_Y;
    private float Greta_Position_Z;

    private SSAOEffect SSOA;
    private int SSOA_mode = 0;
    private DisplayFPS DFPS;

    private string FILE_NAME;

    //---------Functions prototype--------------


	// Use this for initialization
	void Start () {
        //Screen.lockCursor = true;
        SSOA = gameObject.GetComponent<SSAOEffect>();
        DFPS = gameObject.GetComponent<DisplayFPS>();
        GameMode = PlayerPrefs.GetInt("GameMode");
        SSOA_mode = PlayerPrefs.GetInt("SSOA");
        FILE_NAME = PlayerPrefs.GetString("SaveFileName");
        
        
        if (GameMode == 2)
        {

            Greta = GameObject.FindGameObjectWithTag("Greta");

            Greta_Position_X = PlayerPrefs.GetFloat("loadP_X");
            Greta_Position_Y = PlayerPrefs.GetFloat("loadP_Y");
            Greta_Position_Z = PlayerPrefs.GetFloat("loadP_Z");

            Vector3 pos = new Vector3(Greta_Position_X, Greta_Position_Y + 0.5f, Greta_Position_Z);
            Greta.transform.position = pos;

            //print(Greta_Position_X.ToString() + ", " + Greta_Position_Y.ToString() + ", " + Greta_Position_Z.ToString());
        }
        if (SSOA_mode != 0)
        {
            //SSOA.active = true;
            SSOA.enabled = true;
            switch (SSOA_mode)
            {
                case 1:
                    SSOA.m_SampleCount = SSAOEffect.SSAOSamples.Low;
                    break;
                case 2:
                    SSOA.m_SampleCount = SSAOEffect.SSAOSamples.Medium;
                    break;
                case 3:
                    SSOA.m_SampleCount = SSAOEffect.SSAOSamples.High;
                    break;
                default:
                    break;
            }
        }
        else
            //SSOA.active = false;
            SSOA.enabled = false;
	}

    public string GetFILE_NAME()
    {
        return FILE_NAME;
    }

    void OnGUI()
    {

        if (GameManager)
            GUI.skin = GameManager;
        if (GameMode == 1)
        {
            //this.camera.enabled = false;
            GUI.color = new Color(0, 0, 0);
            //GUI.Label(new Rect(0, 0, 200, 100), "123", "Label");
            
        }
    }

    void Awake()
    {
        if (DFPS.GetFPS() > 70)
            Application.targetFrameRate = 70;
    }
	
	// Update is called once per frame
	void Update () {
        
	}
}
