using UnityEngine;
using System.Collections;

public class GameStart : MonoBehaviour {
    //public GUISkin GameGUI;
    public MovieTexture moTexture;
    public GameObject moviePlane;

    private int GameMode;
    public float SceneMoveSpeed = 0.5f;
    public float MovieLength = 26.0f;
    //private GameObject MainCamera;

    private float SceneSmoothMove = 0.0f;
    private float addValue = 0.0f;
    private float TitleMoveSpeed = 0.6f;
    private bool isTitleFadeIn;
    private float TitleSmoothMove = 0.0f;
    private string NextScene = "Hospital";
    
    private bool isSceneFadeIn = true;
    private bool isSceneCross = false;

	// Use this for initialization
	void Start () {
        GameMode = PlayerPrefs.GetInt("GameMode");
        //MainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        
        //Application.LoadLevel("Hospital");

        isTitleFadeIn = true;
        StatusPause = false;
        addStatusTime = 0.0f;

        moviePlane.renderer.material.mainTexture = moTexture;
        moviePlane.renderer.material.color = new Color(0, 0, 0, 0);
        moTexture.Play();
	}


    void OnGUI()
    {
        //if (GameGUI)
        //    GUI.skin = GameGUI;

        //GUI.color = new Color(1, 1, 1, SceneSmoothMove);
        //GUI.Label(new Rect((Screen.width / 2 - 100), (Screen.height / 2 - 50), 200, 100), "Hospital", "SceneName");

        if (addValue >= 90)
            isSceneFadeIn = false;

        if (addValue <= 0 && isSceneCross == true)
            FadeOut();
    }

    private bool StatusPause;
    private float addStatusTime;

	// Update is called once per frame
	void Update () 
    {
        if (Input.GetButtonDown("Fire2") || Input.GetButtonDown("Fire1") || Input.GetMouseButtonDown(1) || Input.GetAxis("Vertical") != 0)
        {
            Application.LoadLevel(NextScene);
        }

        moviePlane.renderer.material.color = new Color(TitleSmoothMove * 0.3f, TitleSmoothMove * 0.3f, TitleSmoothMove * 0.3f);
        if (isTitleFadeIn)
        {
            TitleSmoothMove = Mathf.Sin(addValue * Mathf.PI / 180);
            if (addValue < 90)
                addValue += TitleMoveSpeed;
            else
                StatusPause = true;
        }
        else
        {
            TitleSmoothMove = Mathf.Sin(addValue * Mathf.PI / 180);
            if (addValue > 0)
                addValue -= TitleMoveSpeed;
            else
                Application.LoadLevel(NextScene);
        }

        if (StatusPause)
        {
            if (addStatusTime <= MovieLength)
                addStatusTime += Time.deltaTime;
            else
            {
                isTitleFadeIn = false;

            }
        }
        
	}

    void FadeOut()
    {
        //yield return new WaitForSeconds(3.0f);
        Application.LoadLevel("Hospital");
    }
}
