//-----------------------------------------
// Rolling
// National Taipei University of Education
// Digital Technology Design
// 
// Name : GameMenu
// Modified Data : 2011/11/29
// Modified By : Tim Chang
// Modified Content
//      1. Modify the GretaPlayer initialization.
//      2. Move the Start(), Awake() and Update() to the first of codes.
//-----------------------------------------

#region  References
using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System;
using System.IO;
#endregion

/// <summary>
/// Control all of the game including Display, Music, UI and Timer.
/// </summary>
public class GameMenu : MonoBehaviour
{
    #region public properties
    public Texture Radar;
    public Texture BGTexture;
    public GUISkin MainMenuGUI;
    public GUISkin MainMenuGUI_Slider;
    public float MenuMoveSpeed = 0.6f;      //The speed of menu fade in or out
    public AudioClip BackgroundMusic;
    public Camera senser;
    #endregion

    #region private properties
    private float addVaule = 0;             
    private float MenuSmoothMove = 0;       //SmoothMove by using Triangle function
    public GUITexture Background;
    private int MenuLayer = 0;
    private GameObject GretaPlayer;
    private GretaController Greta;
    private MouseOrbit Mouse;
    private int guidepth = -1;
    public Event e
    {
        private set;
        get;
    }
    #endregion

    //------------------- flag -----------------    
    public bool isOpenMenu          //If the menu has been opened or not
    {
        private set;
        get;
    }

    private bool isCursor = false;  //If the Cursor display or not

    //--------------------------
    private const string CONFIGFILE_NAME = "Quatily.config";
    private bool isFullscreen;
    private string isFS_Yes;
    private int SSAO;
    private SSAOEffect SSAOEFF;
    private string[] SSOA_Str = new string[] { "NONE", "Low", "Medium", "High" };
    private int Quality;
    private string[] Quality_Str = new string[] { "Fastest", "Fast", "Simple", "Good", "Beautiful", "Fantastic" };
    private int res_count = 0;
    private Resolution[] resolutions = Screen.resolutions;
    private int res_current;
    private int res_index;
    private int Optmize = 0;
    private string[] Optmize_str = { "No", "Yes" };
    private REPostProcessorEffect REPost;

    //-----------Music Initizlization--------------
    private int GameMusicVolume = 5;
    private int GameSoundVolume = 5;


    void Start()
    {
        //camera.backgroundColor = new Color(3, 3, 3, 0.2f);
        getResolution();
        SSAOEFF = gameObject.GetComponent<SSAOEffect>();
        REPost = gameObject.GetComponent<REPostProcessorEffect>();
        Mouse = gameObject.GetComponent<MouseOrbit>();
        GretaPlayer = GameObject.Find("Greta");
        Greta = GretaPlayer.GetComponentInChildren<GretaController>();
        LoadSetting();
        ApplySetting();
        Init_GUITBG();
        Background.color = new Color(0.5f, 0.5f, 0.5f, 0.4f);
        this.audio.clip = BackgroundMusic;
        this.audio.volume = PlayerPrefs.GetInt("Music") * 0.1f;
        this.audio.playOnAwake = true;
        this.audio.loop = true;
        this.audio.Play();
        
    }

    void Awake()
    {
        Background.enabled = false;
        SetCursor(false);
        isOpenMenu = false;
        e = Event.current;

        
    }

    void Update()
    {
        this.audio.volume = PlayerPrefs.GetInt("Music") * 0.1f;

        if (isOpenMenu)
        {
            MenuSmoothMove = Mathf.Sin(addVaule * Mathf.PI / 180);
            if (addVaule < 90)
                addVaule += MenuMoveSpeed;
        }
        else
        {
            MenuSmoothMove = Mathf.Sin(addVaule * Mathf.PI / 180);
            if (addVaule > 0)
                addVaule -= MenuMoveSpeed;
        }

    }


    void OnGUI()
    {
        GUI.depth = this.guidepth;
        e = Event.current;
        if (e.isMouse)
            if (e.button.Equals(1))
                SetCursor(true);
        if (e.type.Equals(EventType.MouseUp) && !isOpenMenu)
            SetCursor(false);



        if (MainMenuGUI)
            GUI.skin = MainMenuGUI;

        GUI.skin.horizontalSlider = MainMenuGUI_Slider.horizontalSlider;
        GUI.skin.horizontalSliderThumb = MainMenuGUI_Slider.horizontalSliderThumb;
        
        
        Background.color = new Color(0.5f, 0.5f, 0.5f, MenuSmoothMove * 0.5f);
        
        if (GUI.Button(new Rect(0.0f, Screen.height - 472 + (Screen.height + 472) * (1- MenuSmoothMove), 192, 48), "Display", "MenuButton"))
        {
            MenuLayer = 1;
        }

        if (GUI.Button(new Rect(0.0f, Screen.height - 402 + (Screen.height + 402) * (1 - MenuSmoothMove), 192, 48), "Music", "MenuButton"))
        {
            MenuLayer = 2;
        }

        if (GUI.Button(new Rect(0.0f, Screen.height - 332 + (Screen.height + 332) * (1 - MenuSmoothMove), 192, 48), "Exit Game", "MenuButton"))
        {
            Time.timeScale = 1.0f;
            Greta.LetGameStop(false);
            Mouse.SetGameStop(false);
            Application.LoadLevel("StartMenu");
        }

        if (GUI.Button(new Rect(0.0f, Screen.height - 262  + (Screen.height + 262) * (1 - MenuSmoothMove), 192, 48), "Back", "MenuButton"))
        {
            Time.timeScale = 1.0f;
            Greta.LetGameStop(false);
            Mouse.SetGameStop(false);
            MenuLayer = 0;
            isOpenMenu = false;
        }

        GUI.DrawTexture(new Rect(0, Screen.height - 192, 192, 192), Radar, ScaleMode.StretchToFill, true);
        if (GUI.Button(new Rect(0, Screen.height - 55.5f, 55.5f, 55.5f), "", "MainMenu"))
        {
            Background.enabled = true;
            isOpenMenu = true;
            Greta.LetGameStop(true);
            Mouse.SetGameStop(true);
            Time.timeScale = 0.0000001f;
            if (e.isMouse)
                if (e.button.Equals(1))
                    SetCursor(true);
            if (e.type.Equals(EventType.MouseUp) && !isOpenMenu)
                SetCursor(false);
        }

        if (isOpenMenu)
        {
            
            if (MenuLayer == 0)
            {
                //Map
            }

            
            #region Display Windows
            if (MenuLayer == 1)
            {
                GUI.Label(new Rect(Screen.width / 2 - 220, Screen.height / 2 - 200, 200, 32), "Resolution", "Option_Title");
                GUI.Label(new Rect(Screen.width / 2 + 32, Screen.height / 2 - 200, 128, 32), resolutions[res_index].width.ToString() + " X " + resolutions[res_index].height.ToString(), "Label");
                if (GUI.Button(new Rect(Screen.width / 2, Screen.height / 2 - 200, 32, 32), "", "ArrowButtonStyle_Left"))
                {
                    if (res_index > 0)
                        res_index--;
                    else
                        res_index = res_count;
                }
                if (GUI.Button(new Rect(Screen.width / 2 + 160, Screen.height / 2 - 200, 32, 32), "", "ArrowButtonStyle_Right"))
                {
                    if (res_index < res_count -1)
                        res_index++;
                    else
                        res_index = 0;
                }
                GUI.Label(new Rect(Screen.width / 2 - 220, Screen.height / 2 - 130, 200, 32), "Fullscreen", "Option_Title");
                GUI.Label(new Rect(Screen.width / 2 + 32, Screen.height / 2 - 130, 128, 32), isFS_Yes, "Label");
                if (GUI.Button(new Rect(Screen.width / 2, Screen.height / 2 - 130, 32, 32), "", "ArrowButtonStyle_Left"))
                {
                    if (isFullscreen)
                    {
                        isFullscreen = false;
                        isFS_Yes = "NO";
                    }
                    else
                    {
                        isFullscreen = true;
                        isFS_Yes = "YES";
                    }
                }
                if (GUI.Button(new Rect(Screen.width / 2 + 160, Screen.height / 2 - 130, 32, 32), "", "ArrowButtonStyle_Right"))
                {
                    if (isFullscreen)
                    {
                        isFullscreen = false;
                        isFS_Yes = "NO";
                    }
                    else
                    {
                        isFullscreen = true;
                        isFS_Yes = "YES";
                    }
                }
                GUI.Label(new Rect(Screen.width / 2 - 220, Screen.height / 2 - 60, 200, 32), "SSAO", "Option_Title");
                GUI.Label(new Rect(Screen.width / 2 + 32, Screen.height / 2 - 60, 128, 32), SSOA_Str[SSAO], "Label");
                if (GUI.Button(new Rect(Screen.width / 2, Screen.height / 2 - 60, 32, 32), "", "ArrowButtonStyle_Left"))
                {
                    if (SSAO > 0)
                        SSAO--;
                    else
                        SSAO = 3;
                }
                if (GUI.Button(new Rect(Screen.width / 2 + 160, Screen.height / 2 - 60, 32, 32), "", "ArrowButtonStyle_Right"))
                {
                    if (SSAO < 3)
                        SSAO++;
                    else
                        SSAO = 0;
                }
                //Optmize Setting
                GUI.Label(new Rect(Screen.width / 2 - 220, Screen.height / 2 + 10, 200, 32), "Optmization", "Option_Title");
                GUI.Label(new Rect(Screen.width / 2 + 32, Screen.height / 2 + 10, 128, 32), Optmize_str[Optmize], "Label");
                if (GUI.Button(new Rect(Screen.width / 2, Screen.height / 2 + 10, 32, 32), "", "ArrowButtonStyle_Left"))
                {
                    if (Optmize == 0)
                        Optmize = 1;
                    else if (Optmize == 1)
                        Optmize = 0;

                }
                if (GUI.Button(new Rect(Screen.width / 2 + 160, Screen.height / 2 + 10, 32, 32), "", "ArrowButtonStyle_Right"))
                {
                    if (Optmize == 0)
                        Optmize = 1;
                    else if (Optmize == 1)
                        Optmize = 0;
                }

                //Quality Setting
                GUI.Label(new Rect(Screen.width / 2 - 220, Screen.height / 2 + 80, 200, 32), "Quality Level", "Option_Title");
                GUI.Label(new Rect(Screen.width / 2 + 32, Screen.height / 2 + 80, 128, 32), Quality_Str[Quality], "Label");
                if (GUI.Button(new Rect(Screen.width / 2, Screen.height / 2 + 80, 32, 32), "", "ArrowButtonStyle_Left"))
                {
                    if (Quality > 0)
                        Quality--;

                }
                if (GUI.Button(new Rect(Screen.width / 2 + 160, Screen.height / 2 + 80, 32, 32), "", "ArrowButtonStyle_Right"))
                {
                    if (Quality < 5)
                        Quality++;
                }
                if (GUI.Button(new Rect(Screen.width / 2 + 112, Screen.height / 2 + 150, 64, 32), "Apply", "Option_Button"))
                {
                    ApplySetting();
                    ApplySetting();
                }
                if (GUI.Button(new Rect(Screen.width / 2 + 16, Screen.height / 2 + 150, 64, 32), "Back", "Option_Button"))
                {
                    MenuLayer = 0;
                }
            }
            #endregion

            #region Music Windows
            if (MenuLayer == 2)
            {
                GUI.Label(new Rect(Screen.width / 2 - 220, Screen.height / 2 - 200, 200, 32), "Music Volume", "Option_Title");
                GameMusicVolume = (int)GUI.HorizontalSlider(new Rect(Screen.width / 2, Screen.height / 2 - 189, 128, 32), GameMusicVolume, 0, 10);
                //GUI.Label(new Rect(Screen.width / 2 + 135, Screen.height / 2 - 200, 32, 32), GameMusicVolume.ToString(), "Label");

                GUI.Label(new Rect(Screen.width / 2 - 220, Screen.height / 2 - 130, 200, 32), "Sound Volume", "Option_Title");
                GameSoundVolume = (int)GUI.HorizontalSlider(new Rect(Screen.width / 2, Screen.height / 2 - 119, 128, 32), GameSoundVolume, 0, 10);
                //GUI.Label(new Rect(Screen.width / 2 + 135, Screen.height / 2 - 130, 32, 32), GameSoundVolume.ToString(), "Label");

                if (GUI.Button(new Rect(Screen.width / 2 + 112, Screen.height / 2 + 150, 64, 32), "Apply", "Option_Button"))
                {
                    ApplySetting();
                }

                if (GUI.Button(new Rect(Screen.width / 2 + 16, Screen.height / 2 + 150, 64, 32), "Back", "Option_Button"))
                {
                    MenuLayer = 0;
                }
            }
            #endregion
        }
    }

    #region Support Methods

    /// <summary>
    /// Initialize GUI Background Texture
    /// </summary>
    void Init_GUITBG()
    {
        Background.texture = BGTexture;
        Background.transform.position = Vector3.zero;
        Background.transform.localScale = Vector3.zero;
        Background.pixelInset = new Rect(Screen.width / 2 - 256, Screen.height / 2 - 256, 512, 512);
    }

    /// <summary>
    /// If not find the config file, it will be called to create a new setting config file.
    /// </summary>
    void CreateSetting()
    {

        if (!File.Exists(CONFIGFILE_NAME))
        {
            using (StreamWriter sw = File.CreateText(CONFIGFILE_NAME))
            {
                sw.Write("Resolution = ");
                sw.WriteLine(res_index.ToString());
                sw.Write("isFullscreen = ");
                sw.WriteLine(isFullscreen.ToString());
                sw.Write("SSAO = ");
                sw.WriteLine(SSAO.ToString());
                sw.Write("Quatily = ");
                sw.WriteLine(Quality.ToString());
                sw.Write("Optimization = ");
                sw.WriteLine(Optmize.ToString());
                sw.Write("Music = ");
                sw.WriteLine(GameMusicVolume.ToString());
                sw.Write("Sound = ");
                sw.WriteLine(GameSoundVolume.ToString());
                sw.Close();
            }
        }
    }

    /// <summary>
    /// Load setting file to config system envirement if the file exist.
    /// </summary>
    void LoadSetting()
    {
        string temp = "";
        string[] setting = new string[7];
        if (!File.Exists(CONFIGFILE_NAME))
            CreateSetting();
        else
        {
            using (StreamReader sr = new StreamReader(CONFIGFILE_NAME))
            {
                int index = 0;
                temp = sr.ReadToEnd();
                foreach (Match m in Regex.Matches(temp, @"(= )(?<value>\w+)"))
                {
                    setting[index] = m.Groups["value"].Value;
                    print(m.Groups["value"].Value);
                    index++;

                }

                res_index = Convert.ToInt32(setting[0]);
                if (setting[1].Equals("True"))
                {
                    isFullscreen = true;
                    isFS_Yes = "YES";
                }
                else
                {
                    isFullscreen = false;
                    isFS_Yes = "NO";
                }
                SSAO = Convert.ToInt32(setting[2]);
                Quality = Convert.ToInt32(setting[3]);
                Optmize = Convert.ToInt32(setting[4]);
                GameMusicVolume = Convert.ToInt32(setting[5]);
                GameSoundVolume = Convert.ToInt32(setting[6]);

                sr.Close();
            }
        }
    }

    /// <summary>
    /// Save current system envirement setting to a config file.
    /// </summary>
    void WriteSetting()
    {
        using (StreamWriter sw = new StreamWriter(CONFIGFILE_NAME, false))
        {
            sw.Write("Resolution = ");
            sw.WriteLine(res_index.ToString());
            sw.Write("isFullscreen = ");
            sw.WriteLine(isFullscreen.ToString());
            sw.Write("SSAO = ");
            sw.WriteLine(SSAO.ToString());
            sw.Write("Quatily = ");
            sw.WriteLine(Quality.ToString());
            sw.Write("Optmize = ");
            sw.WriteLine(Optmize.ToString());
            sw.Write("Music = ");
            sw.WriteLine(GameMusicVolume.ToString());
            sw.Write("Sound = ");
            sw.WriteLine(GameSoundVolume.ToString());
            sw.Close();
        }
    }

    /// <summary>
    /// Apply current setting to system envirement
    /// </summary>
    void ApplySetting()
    {

        Screen.SetResolution(resolutions[res_index].width, resolutions[res_index].height, isFullscreen);
        PlayerPrefs.SetInt("SSOA", SSAO);

        if (SSAO != 0)
        {
            SSAOEFF.enabled = true;
            switch (SSAO)
            {
                case 1:
                    SSAOEFF.m_SampleCount = SSAOEffect.SSAOSamples.Low;
                    break;
                case 2:
                    SSAOEFF.m_SampleCount = SSAOEffect.SSAOSamples.Medium;
                    break;
                case 3:
                    SSAOEFF.m_SampleCount = SSAOEffect.SSAOSamples.High;
                    break;
                default:
                    break;
            }
        }
        else
            SSAOEFF.enabled = false;

        switch (Quality)
        {
            case 0:
                QualitySettings.currentLevel = QualityLevel.Fastest;
                break;
            case 1:
                QualitySettings.currentLevel = QualityLevel.Fast;
                break;
            case 2:
                QualitySettings.currentLevel = QualityLevel.Simple;
                break;
            case 3:
                QualitySettings.currentLevel = QualityLevel.Good;
                break;
            case 4:
                QualitySettings.currentLevel = QualityLevel.Beautiful;
                break;
            case 5:
                QualitySettings.currentLevel = QualityLevel.Fantastic;
                break;
            default:
                break;
        }
        Init_GUITBG();
        PlayerPrefs.SetInt("REPost", Optmize);
        PlayerPrefs.SetInt("Music", GameMusicVolume);
        PlayerPrefs.SetInt("Sound", GameSoundVolume);

        if (Optmize == 0)
            REPost.enabled = false;
        else if (Optmize == 1)
            REPost.enabled = true;
        WriteSetting();
    }

    void getResolution()
    {
        foreach (Resolution res in resolutions)
            res_count++;
        res_current = res_count;
        res_index = res_current;
    }

    void SetCursor(bool isTrue)
    {
        isCursor = isTrue;
        Screen.showCursor = isCursor;
        Screen.lockCursor = !isCursor;

    }

    #endregion
}
