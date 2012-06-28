using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Text.RegularExpressions;

public class Initialize : MonoBehaviour {

    public Texture LogoTexture;

    private float TitleSmoothMove = 0.0f;
    private float addValue = 0.0f;
    private float TitleMoveSpeed = 0.6f;
    private bool isTitleFadeIn;
    private string NextScene = "StartMenu";

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

    void getResolution()
    {
        foreach (Resolution res in resolutions)
            res_count++;
        res_current = res_count;
        res_index = res_current;
    }

	// Use this for initialization
	void Start () {
        Logo = new GameObject("LogoPost");
        Logo.AddComponent<GUITexture>();
        Logo.guiTexture.texture = LogoTexture;
        Logo.guiTexture.texture.wrapMode = TextureWrapMode.Clamp;
        Logo.transform.position = Vector3.zero;
        Logo.transform.localScale = Vector3.zero;
        Logo.guiTexture.pixelInset = new Rect( Screen.width / 2 - (Screen.height - 30) / 3 * 2 / 2, 15, (Screen.height - 30) / 3 * 2 - 0.6f, Screen.height - 30);
        Logo.guiTexture.color = new Color(0, 0, 0, 0);

        isTitleFadeIn = true;
        StatusPause = false;
        addStatusTime = 0.0f;
	
	}

    GameObject Logo;

    void OnGUI()
    {
        
    }

    void Awake()
    {
        resolutions = Screen.resolutions;
        getResolution();

        LoadSetting();
        ApplySetting();
    }

    private bool StatusPause;
    private float addStatusTime;
	
	// Update is called once per frame
	void Update () {
        Logo.guiTexture.pixelInset = new Rect(Screen.width / 2 - (Screen.height - 30) / 3 * 2 / 2, 15, (Screen.height - 30) / 3 * 2 - 0.6f, Screen.height - 30);

        Logo.guiTexture.color = new Color(0.5f, 0.5f, 0.5f, TitleSmoothMove * 0.5f);

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
            if (addStatusTime <= 3.0f)
                addStatusTime += Time.deltaTime;
            else
            {
                isTitleFadeIn = false;
               
            }
        }
	
	}
}
