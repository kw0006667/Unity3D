using UnityEngine;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System;

public class StartMenuGUI : MonoBehaviour
{

    public GUISkin gSkin;
    public GUISkin gSkin_slider;
    public Texture2D background;
    public Texture TitlePic;
    public AudioClip btnHover;
    public AudioClip btnClick;
    public float LabelWidth = 100.0f;
    public float LabelHeight = 50.0f;
    public MovieTexture moTexture;
    public GameObject plane;

    private GUIStyle backgroundStyle = new GUIStyle();


    private int GameMode = 0;    //GameMode enum { 0 = Exit, 1 = New Game, 2 = Load Game}
    private float ScreenW;
    private float ScreenH;
    private int MenuLayer = 1;
    private int LoadFileChoice;

    private float TitleSmoothMove = 0.0f;
    private float addValue = 0.0f;
    private float TitleMoveSpeed = 0.6f;
    private bool isTitleFadeIn;

    private const string FILE_NAME = "Save";
    private string RECORD_NAME;
    private int RECORD_NUM = 1;
    private string loadScene = "";
    private float loadPosition_X;
    private float loadPosition_Y;
    private float loadPosition_Z;

    private string trainStationSceneName = "TrainStation";

    //----------Button Rect-----------------------
    private bool Hover = false;
    private Rect NewButtonRect = new Rect((Screen.width / 2) + 250, (Screen.height / 2) - 150, 200, 50);
    private Rect LoadButtonRect = new Rect((Screen.width / 2) + 250, (Screen.height / 2) - 50, 200, 50);
    private Rect OptionButtonRect = new Rect((Screen.width / 2) + 250, (Screen.height / 2) + 50, 200, 50);
    private Rect CreditsButtonRect = new Rect((Screen.width / 2) + 250, (Screen.height / 2) + 150, 200, 50);
    private Rect ExitButtonRect = new Rect((Screen.width / 2) + 250, (Screen.height / 2) + 250, 200, 50);
    private Rect DisplayButtonRect = new Rect((Screen.width / 2) + 250, (Screen.height / 2) - 150, 200, 50);
    private Rect MusicButtonRect = new Rect((Screen.width / 2) + 250, (Screen.height / 2) - 50, 200, 50);
    private Rect BackButtonRect = new Rect((Screen.width / 2) + 250, (Screen.height / 2) + 150, 200, 50);

    //---------Title Rect------------------------
    private Rect TitleRect = new Rect();

    //----------Window Initialization-------------
    private const string CONFIGFILE_NAME = "Quatily.config";
    private bool isFullscreen = true;
    private string isFS_Yes = "NO";
    private int SSOA = 0;
    private string[] SSOA_Str = new string[] { "NONE", "Low", "Medium", "High" };
    private int Quality = 5;
    private string[] Quality_Str = new string[] { "Fastest", "Fast", "Simple", "Good", "Beautiful", "Fantastic" };
    private int res_count = -1;
    private Resolution[] resolutions;
    private int res_current;
    private int res_index;
    private int Optmize = 0;
    private string[] Optmize_str = { "No", "Yes" };

    //-----------Music Initizlization--------------
    private int GameMusicVolume = 5;
    private int GameSoundVolume = 5;

    //-----------Windows Set----------------------
    private float windowsRectLeft = 10f;
    private float windowsRectTop = 10f;
    private float windowsRectWidth = 300f;
    private float windowsRectHeight = 300f;
    private Rect WindowsRect = new Rect(10, 10, 300, 300);
    private bool isshowWindowA = false;
    private string AdvanceString = "";

    void AdvanceWindow(int windowID)
    {
        if (GUI.Button(new Rect(windowsRectLeft + 20, windowsRectTop + 20, 150, 25), "Go to TrainStation!"))
        {
            isshowWindowA = false;
            GameMode = 1;
            PlayerPrefs.SetInt("GameMode", 1);
            Application.LoadLevel(trainStationSceneName);
        }
        AdvanceString = GUI.TextField(new Rect(windowsRectLeft + 20, windowsRectHeight - 45, 260, 25), AdvanceString, 260);
    }


    //----Function Prototype------
    int CreateRecord()
    {
        RECORD_NUM = 1;
        while (true)
        {
            if (!File.Exists("Save" + RECORD_NUM.ToString() + ".data"))
            {
                RECORD_NAME = "Save" + RECORD_NUM.ToString() + ".data";
                PlayerPrefs.SetString("SaveFileName", RECORD_NAME);
                return 0;
            }
            RECORD_NUM++;
            if (RECORD_NUM > 3)
                return 1;
        }
    }



    void getResolution()
    {
        foreach (Resolution res in resolutions)
            res_count++;
        res_current = res_count;
        res_index = res_current;
    }

    bool isSaveFileExists()
    {
        RECORD_NUM = 1;
        while (true)
        {
            if (File.Exists(FILE_NAME + RECORD_NUM.ToString() + ".data"))
            {
                //RECORD_NAME = "Save" + RECORD_NUM.ToString() + ".data";
                //PlayerPrefs.SetString("SaveFileName", RECORD_NAME);
                return true;
            }
            RECORD_NUM++;
            if (RECORD_NUM > 3)
                return false;
        }
    }

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
                sw.WriteLine(SSOA.ToString());
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
                SSOA = Convert.ToInt32(setting[2]);
                Quality = Convert.ToInt32(setting[3]);
                Optmize = Convert.ToInt32(setting[4]);
                GameMusicVolume = Convert.ToInt32(setting[5]);
                GameSoundVolume = Convert.ToInt32(setting[6]);

                sr.Close();
            }
        }
    }

    void WriteSetting()
    {
        using (StreamWriter sw = new StreamWriter(CONFIGFILE_NAME, false))
        {
            sw.Write("Resolution = ");
            sw.WriteLine(res_index.ToString());
            sw.Write("isFullscreen = ");
            sw.WriteLine(isFullscreen.ToString());
            sw.Write("SSAO = ");
            sw.WriteLine(SSOA.ToString());
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

    void ApplySetting()
    {
        Screen.SetResolution(resolutions[res_index].width, resolutions[res_index].height, isFullscreen);
        PlayerPrefs.SetInt("SSOA", SSOA);

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
        PlayerPrefs.SetInt("REPost", Optmize);
        PlayerPrefs.SetInt("Music", GameMusicVolume);
        PlayerPrefs.SetInt("Sound", GameSoundVolume);

        WriteSetting();
    }

    void loadRecord(int session)
    {
        using (FileStream fs = new FileStream(FILE_NAME + session.ToString() + ".data", FileMode.Open, FileAccess.Read))
        {
            BinaryReader r = new BinaryReader(fs);
            loadScene = r.ReadString();
            loadPosition_X = r.ReadSingle();
            loadPosition_Y = r.ReadSingle();
            loadPosition_Z = r.ReadSingle();

            PlayerPrefs.SetString("loadScene", loadScene);
            PlayerPrefs.SetFloat("loadP_X", loadPosition_X);
            PlayerPrefs.SetFloat("loadP_Y", loadPosition_Y);
            PlayerPrefs.SetFloat("loadP_Z", loadPosition_Z);
            RECORD_NAME = FILE_NAME + session.ToString() + ".data";
            PlayerPrefs.SetString("SaveFileName", RECORD_NAME);

            r.Close();
            fs.Close();
        }
        GameMode = 2;
        PlayerPrefs.SetInt("GameMode", 2);
    }

    public void OnGUI()
    {
        print(SSOA);
        if (gSkin)
            GUI.skin = gSkin;
        else
            Debug.Log("StartMenuGUI: GUI Skin object missing!");

        backgroundStyle.normal.background = background;

        GUI.skin.horizontalSliderThumb = gSkin_slider.horizontalSliderThumb;
        GUI.skin.horizontalSlider = gSkin_slider.horizontalSlider;

        ScreenW = Screen.width * 0.7f;
        ScreenH = Screen.height * 0.9f;

        Title.guiTexture.color = new Color(0.5f, 0.5f, 0.5f, TitleSmoothMove * 0.0f);

        GUI.DrawTexture(new Rect(Screen.width / 2 + 20, Screen.height / 2 - 320, 600, 150), TitlePic);
        bool overButton = false;

        #region StartMenu
        // StartMenu
        if (MenuLayer == 1)
        {
            //GUI.Label(new Rect( (Screen.width - ScreenW) / 2, (Screen.height - ScreenH) / 2, ScreenW, ScreenH), "", backgroundStyle);

            //GUI.Label(new Rect((Screen.width / 2) - 375, (Screen.height / 2) - 64 , LabelWidth, LabelHeight), "", "Title");

            SetTitleFadeIn(true);

            //ButtonHover(NewButtonRect);

            Rect ButtonRect = new Rect();

            ButtonRect = new Rect(NewButtonRect.x, Screen.height / 2 + 100, 200, 50);
            if (ButtonRect.Contains(Input.mousePosition))
            {
                overButton = true;
                if (!Hover)
                {
                    Hover = true;

                    audio.PlayOneShot(btnHover);
                }
            }

            ButtonRect = new Rect(LoadButtonRect.x, Screen.height / 2, LoadButtonRect.width, LoadButtonRect.height);
            if (ButtonRect.Contains(Input.mousePosition))
            {
                overButton = true;
                if (!Hover)
                {
                    Hover = true;
                    audio.PlayOneShot(btnHover);
                }
            }

            ButtonRect = new Rect(OptionButtonRect.x, Screen.height / 2 - 100, LoadButtonRect.width, LoadButtonRect.height);
            if (ButtonRect.Contains(Input.mousePosition))
            {
                overButton = true;
                if (!Hover)
                {
                    Hover = true;
                    audio.PlayOneShot(btnHover);
                }
            }

            ButtonRect = new Rect(CreditsButtonRect.x, Screen.height / 2 - 200, ExitButtonRect.width, ExitButtonRect.height);
            if (ButtonRect.Contains(Input.mousePosition))
            {
                overButton = true;
                if (!Hover)
                {
                    Hover = true;
                    audio.PlayOneShot(btnHover);
                }
            }

            ButtonRect = new Rect(ExitButtonRect.x, Screen.height / 2 - 300, ExitButtonRect.width, ExitButtonRect.height);
            if (ButtonRect.Contains(Input.mousePosition))
            {
                overButton = true;
                if (!Hover)
                {
                    Hover = true;
                    audio.PlayOneShot(btnHover);
                }
            }

            if (!overButton)
                Hover = false;
            if (GUI.Button(NewButtonRect, "", "NewButton"))
            {
                GameMode = 1;
                PlayerPrefs.SetInt("GameMode", 1);
                if (CreateRecord() == 0)
                    Application.LoadLevel("SceneCross");
                else if (CreateRecord() == 1)
                {
                    MenuLayer = 2;
                }
            }

            if (true)
            {
                //ButtonHover(LoadButtonRect);
                if (GUI.Button(LoadButtonRect, "", "LoadButton"))
                {
                    MenuLayer = 3;
                }
            }
            //ButtonHover(OptionButtonRect);
            if (GUI.Button(OptionButtonRect, "", "OptionButton"))
            {
                MenuLayer = 5;
            }

            //ButtonHover(OptionButtonRect);
            if (GUI.Button(CreditsButtonRect, "", "CreditsButton"))
            {
                MenuLayer = 8;
            }


            bool isWebPlayer = (Application.platform == RuntimePlatform.OSXWebPlayer || Application.platform == RuntimePlatform.WindowsWebPlayer);
            if (!isWebPlayer)
            {
                //ButtonHover(ExitButtonRect);
                if (GUI.Button(ExitButtonRect, "", "ExitButton"))
                {
                    GameMode = 0;
                    Application.Quit();
                }
            }
        }
        #endregion

        #region More than 3 savefiles
        //More than 3 savefiles       
        if (MenuLayer == 2)
        {
            //SetTitleFadeIn(false);

            //GUI.DrawTexture(new Rect(Screen.width / 2 - 256, Screen.height / 2 - 256, 512, 512), background, ScaleMode.StretchToFill, true);
            GUI.Label(new Rect((Screen.width / 2 + 270), (Screen.height / 2 - 50), 100, 32), "Record of more than three,", "Option_Title");
            GUI.Label(new Rect((Screen.width / 2 + 270), (Screen.height / 2 - 25), 100, 32), "if you want to replace the Record 1?", "Option_Title");

            if (GUI.Button(new Rect(Screen.width / 2 + 224, Screen.height / 2 + 61, 64, 32), "YES", "Option_Title"))
            {
                RECORD_NAME = "Save" + (1).ToString() + ".data";
                PlayerPrefs.SetString("SaveFileName", RECORD_NAME);
                GameMode = 1;
                PlayerPrefs.SetInt("GameMode", 1);
                Application.LoadLevel("SceneCross");
            }

            if (GUI.Button(new Rect(Screen.width / 2 + 352, Screen.height / 2 + 61, 64, 32), "NO", "Option_Title"))
            {
                MenuLayer = 1;
            }
        }
        #endregion

        #region Load Menu
        //Load Menu
        if (MenuLayer == 3)
        {
            SetTitleFadeIn(false);
            //GUI.DrawTexture(new Rect(Screen.width / 2 - 256, Screen.height / 2 - 256, 512, 512), background, ScaleMode.StretchToFill, true);
            if (File.Exists(FILE_NAME + "1.data"))
            {
                GUI.Label(new Rect((Screen.width / 2) + 250, (Screen.height / 2) - 100, 128, 32), "Record 1", "Label");
                if (GUI.Button(new Rect(Screen.width / 2 + 110, Screen.height / 2 - 100, 64, 32), "Load", "Option_Title"))
                {
                    LoadFileChoice = 1;
                    loadRecord(LoadFileChoice);
                    Application.LoadLevel(PlayerPrefs.GetString("loadScene"));
                }
                if (GUI.Button(new Rect(Screen.width / 2 + 450, Screen.height / 2 - 100, 64, 32), "Delete", "Option_Title"))
                {
                    LoadFileChoice = 1;
                    File.Delete(FILE_NAME + LoadFileChoice.ToString() + ".data");
                }
            }
            if (File.Exists(FILE_NAME + "2.data"))
            {
                GUI.Label(new Rect((Screen.width / 2) + 250, (Screen.height / 2), 128, 32), "Record 2", "Label");
                if (GUI.Button(new Rect(Screen.width / 2 + 110, Screen.height / 2, 64, 32), "Load", "Option_Title"))
                {
                    LoadFileChoice = 2;
                    loadRecord(LoadFileChoice);
                    Application.LoadLevel(PlayerPrefs.GetString("loadScene"));
                }
                if (GUI.Button(new Rect(Screen.width / 2 + 450, Screen.height / 2, 64, 32), "Delete", "Option_Title"))
                {
                    LoadFileChoice = 2;
                    File.Delete(FILE_NAME + LoadFileChoice.ToString() + ".data");
                }
            }
            if (File.Exists(FILE_NAME + "3.data"))
            {
                GUI.Label(new Rect((Screen.width / 2) + 250, (Screen.height / 2) + 100, 128, 32), "Record 3", "Label");
                if (GUI.Button(new Rect(Screen.width / 2 + 110, Screen.height / 2 + 100, 64, 32), "Load", "Option_Title"))
                {
                    LoadFileChoice = 3;
                    loadRecord(LoadFileChoice);
                    Application.LoadLevel(PlayerPrefs.GetString("loadScene"));
                }
                if (GUI.Button(new Rect(Screen.width / 2 + 450, Screen.height / 2 + 100, 64, 32), "Delete", "Option_Title"))
                {
                    LoadFileChoice = 3;
                    File.Delete(FILE_NAME + LoadFileChoice.ToString() + ".data");
                }
            }
            if (GUI.Button(new Rect((Screen.width / 2) + 280, (Screen.height / 2) + 200, 64, 32), "", "SubBackButton"))
                MenuLayer = 1;
        }
        #endregion

        #region Option Menu
        //Option Menu
        if (MenuLayer == 5)
        {
            //SetTitleFadeIn(true);
            Rect ButtonRect = new Rect();

            ButtonRect = new Rect(DisplayButtonRect.x, Screen.height / 2 + 100, DisplayButtonRect.width, DisplayButtonRect.height);
            if (ButtonRect.Contains(Input.mousePosition))
            {
                overButton = true;
                if (!Hover)
                {
                    Hover = true;
                    audio.PlayOneShot(btnHover);
                }
            }
            ButtonRect = new Rect(MusicButtonRect.x, Screen.height / 2, MusicButtonRect.width, MusicButtonRect.height);
            if (ButtonRect.Contains(Input.mousePosition))
            {
                overButton = true;
                if (!Hover)
                {
                    Hover = true;
                    audio.PlayOneShot(btnHover);
                }
            }
            ButtonRect = new Rect(BackButtonRect.x, Screen.height / 2 - 200, BackButtonRect.width, BackButtonRect.height);
            if (ButtonRect.Contains(Input.mousePosition))
            {
                overButton = true;
                if (!Hover)
                {
                    Hover = true;
                    audio.PlayOneShot(btnHover);
                }
            }

            if (!overButton)
                Hover = false;

            //GUI.Label(new Rect((Screen.width / 2) - 375, (Screen.height / 2) - 64, LabelWidth, LabelHeight), "", "Title");
            if (GUI.Button(DisplayButtonRect, "", "DisplayButton"))
            {
                SetTitleFadeIn(true);
                LoadSetting();
                MenuLayer = 6;

            }

            if (GUI.Button(MusicButtonRect, "", "MusicButton"))
            {
                SetTitleFadeIn(true);
                LoadSetting();
                MenuLayer = 7;
            }
            if (GUI.Button(BackButtonRect, "", "BackButton"))
                MenuLayer = 1;
        }

        //Display Menu
        if (MenuLayer == 6)
        {
            //SetTitleFadeIn(false);
            //GUI.DrawTexture(new Rect(Screen.width / 2 - 256, Screen.height / 2 - 256, 512, 512), background, ScaleMode.StretchToFill, true);

            GUI.Label(new Rect(Screen.width / 2 + 90, Screen.height / 2 - 160, 200, 32), "Resolution", "Option_Title");
            GUI.Label(new Rect(Screen.width / 2 + 370, Screen.height / 2 - 160, 128, 32), resolutions[res_index].width.ToString() + " X " + resolutions[res_index].height.ToString(), "Label");
            if (GUI.Button(new Rect(Screen.width / 2 + 330, Screen.height / 2 - 152, 16, 16), "", "ArrowButtonLeft"))
            {
                if (res_index > 0)
                    res_index--;
                else
                    res_index = res_count;
            }
            if (GUI.Button(new Rect(Screen.width / 2 + 520, Screen.height / 2 - 152, 16, 16), "", "ArrowButtonRight"))
            {
                if (res_index < res_count)
                    res_index++;
                else
                    res_index = 0;
            }
            GUI.Label(new Rect(Screen.width / 2 + 90, Screen.height / 2 - 90, 200, 32), "Fullscreen", "Option_Title");
            GUI.Label(new Rect(Screen.width / 2 + 370, Screen.height / 2 - 90, 128, 32), isFS_Yes, "Label");
            if (GUI.Button(new Rect(Screen.width / 2 + 330, Screen.height / 2 - 82, 16, 16), "", "ArrowButtonLeft"))
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
            if (GUI.Button(new Rect(Screen.width / 2 + 520, Screen.height / 2 - 82, 16, 16), "", "ArrowButtonRight"))
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
            GUI.Label(new Rect(Screen.width / 2 + 90, Screen.height / 2 - 20, 200, 32), "SSAO", "Option_Title");
            GUI.Label(new Rect(Screen.width / 2 + 370, Screen.height / 2 - 20, 128, 32), SSOA_Str[SSOA], "Label");
            if (GUI.Button(new Rect(Screen.width / 2 + 330, Screen.height / 2 - 12, 16, 16), "", "ArrowButtonLeft"))
            {
                if (SSOA > 0)
                    SSOA--;
                else
                    SSOA = 3;
            }
            if (GUI.Button(new Rect(Screen.width / 2 + 520, Screen.height / 2 - 12, 16, 16), "", "ArrowButtonRight"))
            {
                if (SSOA < 3)
                    SSOA++;
                else
                    SSOA = 0;
            }

            //Optmize Setting
            GUI.Label(new Rect(Screen.width / 2 + 90, Screen.height / 2 + 50, 200, 32), "Color Soften", "Option_Title");
            GUI.Label(new Rect(Screen.width / 2 + 370, Screen.height / 2 + 50, 128, 32), Optmize_str[Optmize], "Label");
            if (GUI.Button(new Rect(Screen.width / 2 + 330, Screen.height / 2 + 58, 16, 16), "", "ArrowButtonLeft"))
            {
                if (Optmize == 0)
                    Optmize = 1;
                else if (Optmize == 1)
                    Optmize = 0;

            }
            if (GUI.Button(new Rect(Screen.width / 2 + 520, Screen.height / 2 + 58, 16, 16), "", "ArrowButtonRight"))
            {
                if (Optmize == 0)
                    Optmize = 1;
                else if (Optmize == 1)
                    Optmize = 0;
            }
            //Quality Setting
            GUI.Label(new Rect(Screen.width / 2 + 90, Screen.height / 2 + 120, 200, 32), "Quality Level", "Option_Title");
            GUI.Label(new Rect(Screen.width / 2 + 370, Screen.height / 2 + 120, 128, 32), Quality_Str[Quality], "Label");
            if (GUI.Button(new Rect(Screen.width / 2 + 330, Screen.height / 2 + 128, 16, 16), "", "ArrowButtonLeft"))
            {
                if (Quality > 0)
                    Quality--;

            }
            if (GUI.Button(new Rect(Screen.width / 2 + 520, Screen.height / 2 + 128, 16, 16), "", "ArrowButtonRight"))
            {
                if (Quality < 5)
                    Quality++;
            }


            if (GUI.Button(new Rect(Screen.width / 2 + 450, Screen.height / 2 + 190, 64, 32), "", "ApplyButton"))
            {
                ApplySetting();
            }
            if (GUI.Button(new Rect(Screen.width / 2 + 356, Screen.height / 2 + 190, 64, 32), "", "SubBackButton"))
            {
                MenuLayer = 5;
            }
        }
        #endregion

        #region Music Menu
        if (MenuLayer == 7)
        {
            //SetTitleFadeIn(false);
            //GUI.DrawTexture(new Rect(Screen.width / 2 - 256, Screen.height / 2 - 256, 512, 512), background, ScaleMode.StretchToFill, true);

            GUI.Label(new Rect(Screen.width / 2 + 90, Screen.height / 2 - 160, 200, 32), "Music Volume", "Option_Title");
            GUI.Label(new Rect(Screen.width / 2 + 320, Screen.height / 2 - 149, 128, 32), "-------------", "Option_Title");
            GameMusicVolume = (int)GUI.HorizontalSlider(new Rect(Screen.width / 2 + 320, Screen.height / 2 - 149, 128, 32), GameMusicVolume, 0, 10);

            //GameMusicVolume = (int)GUILayout.HorizontalSlider(GameMusicVolume,0, 10);
            //GUI.Label(new Rect(Screen.width / 2 + 135, Screen.height / 2 - 200, 32, 32), GameMusicVolume.ToString(), "Label");

            GUI.Label(new Rect(Screen.width / 2 + 90, Screen.height / 2 - 90, 200, 32), "Sound Volume", "Option_Title");
            GUI.Label(new Rect(Screen.width / 2 + 320, Screen.height / 2 - 79, 128, 32), "-------------", "Option_Title");
            GameSoundVolume = (int)GUI.HorizontalSlider(new Rect(Screen.width / 2 + 320, Screen.height / 2 - 79, 128, 32), GameSoundVolume, 0, 10);
            //GUI.Label(new Rect(Screen.width / 2 + 135, Screen.height / 2 - 130, 32, 32), GameSoundVolume.ToString(), "Label");

            if (GUI.Button(new Rect(Screen.width / 2 + 450, Screen.height / 2 + 190, 64, 32), "", "ApplyButton"))
            {
                ApplySetting();
            }

            if (GUI.Button(new Rect(Screen.width / 2 + 356, Screen.height / 2 + 190, 64, 32), "", "SubBackButton"))
            {
                MenuLayer = 5;
            }
        }
        #endregion

        #region Credits Menu
        //Credits Menu
        if (MenuLayer == 8)
        {
            GUI.Label(new Rect(Screen.width / 2 + 180, Screen.height / 2 - 160, 300, 32), "國立台北教育大學 - 數位科技設計學系", "Option_Title");
            GUI.Label(new Rect(Screen.width / 2 + 230, Screen.height / 2 - 100, 200, 32), "指導老師：巴白山  老師", "Option_Title");
            GUI.Label(new Rect(Screen.width / 2 + 230, Screen.height / 2 - 20, 200, 32), "作者", "Option_Title");
            GUI.Label(new Rect(Screen.width / 2 + 210, Screen.height / 2 + 40, 250, 32), "Programing      張廷宇  黃御恩", "Option_Title");
            GUI.Label(new Rect(Screen.width / 2 + 210, Screen.height / 2 + 100, 250, 32), "Art Designer      王昶中  康學昕", "Option_Title");


            if (GUI.Button(new Rect((Screen.width / 2) + 300, (Screen.height / 2) + 200, 64, 32), "", "SubBackButton"))
                MenuLayer = 1;
        }

        #endregion

        #region Windows
        if (Input.GetKeyDown(KeyCode.Escape))
            isshowWindowA = false;
        if (Input.GetKeyDown(KeyCode.LeftControl))
            isshowWindowA = true;

        if (isshowWindowA)
        {
            GUI.Window(0, WindowsRect, AdvanceWindow, "Advance Window");
            if (Input.GetKeyUp(KeyCode.KeypadEnter))
                AdvanceString = "";
        }
        #endregion

    }

    void ButtonHover(Rect rect)
    {
        bool overButton = false;
        Rect ButtonRect = new Rect(rect.xMin, rect.yMin + 250.0f, rect.width, rect.height);
        if (ButtonRect.Contains(Input.mousePosition))
        {
            overButton = true;
            if (!Hover)
            {
                Hover = true;
                audio.PlayOneShot(btnHover);
            }
        }

        if (!overButton)
            Hover = false;
    }

    void SetTitleFadeIn(bool FadeIn)
    {
        if (FadeIn)
            isTitleFadeIn = FadeIn;
        else
            isTitleFadeIn = FadeIn;
    }

    GameObject Title;

    // Use this for initialization
    void Start()
    {
        Title = new GameObject("Title");
        Title.AddComponent<GUITexture>();
        Title.guiTexture.texture = TitlePic;
        Title.guiTexture.texture.wrapMode = TextureWrapMode.Clamp;
        Title.transform.position = Vector3.zero;
        Title.transform.localScale = Vector3.zero;
        Title.guiTexture.pixelInset = new Rect((Screen.width / 2) - 375, (Screen.height / 2), 512, 128);
        Title.guiTexture.color = new Color(0, 0, 0, 0);

        plane.renderer.material.mainTexture = moTexture;
        moTexture.loop = true;
        moTexture.Play();

    }
    void Awake()
    {
        resolutions = Screen.resolutions;
        getResolution();

        LoadSetting();
        ApplySetting();

    }

    // Update is called once per frame
    void Update()
    {
        NewButtonRect = new Rect((Screen.width / 2) + 250, (Screen.height / 2) - 150, 200, 50);
        LoadButtonRect = new Rect((Screen.width / 2) + 250, (Screen.height / 2) - 50, 200, 50);
        OptionButtonRect = new Rect((Screen.width / 2) + 250, (Screen.height / 2) + 50, 200, 50);
        CreditsButtonRect = new Rect((Screen.width / 2) + 250, (Screen.height / 2) + 150, 200, 50);
        ExitButtonRect = new Rect((Screen.width / 2) + 250, (Screen.height / 2) + 250, 200, 50);
        DisplayButtonRect = new Rect((Screen.width / 2) + 250, (Screen.height / 2) - 150, 200, 50);
        MusicButtonRect = new Rect((Screen.width / 2) + 250, (Screen.height / 2) - 50, 200, 50);
        BackButtonRect = new Rect((Screen.width / 2) + 250, (Screen.height / 2) + 150, 200, 50);
        TitleRect = new Rect((Screen.width / 2) - 375, (Screen.height / 2) - 64, 512, 128);
        Title.guiTexture.pixelInset = TitleRect;

        audio.volume = PlayerPrefs.GetInt("Sound") * 0.1f;


        if (isTitleFadeIn)
        {
            TitleSmoothMove = Mathf.Sin(addValue * Mathf.PI / 180);
            if (addValue < 90)
                addValue += TitleMoveSpeed;
        }
        else
        {
            TitleSmoothMove = Mathf.Sin(addValue * Mathf.PI / 180);
            if (addValue > 0)
                addValue -= TitleMoveSpeed;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            isshowWindowA = false;
        if (Input.GetKeyDown(KeyCode.LeftControl))
            isshowWindowA = true;
    }


}
