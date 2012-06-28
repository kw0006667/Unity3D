//-----------------------------------------
// Rolling
// National Taipei University of Education
// Digital Technology Design
// 
// Name : Senser
// Modified Data : 2011/12/07
// Modified By : Tim Chang
// Modified Content

//-----------------------------------------

#region References
using UnityEngine;
using System.Collections.Generic;
#endregion

/// <summary>
/// Scan the distance between Greta to AI
/// </summary>
public class Senser : MonoBehaviour 
{
    public Transform AISenserCamera;
    public Camera RadarCamera;

    private List<GameObject> enermy;
    private GameObject gretaPlayer;
    private GretaController gretaScript;
    GameObject[] aiObjects;
    private bool isAnyAIinStay;
    private float minAIDistance = 0;
    private float senserRange = 15.0f;
    private float attactDistance = 5.0f;
    private Vector3 senserRadarMinAngles = new Vector3(0.0f, -135.0f, 0.0f);
    private float radarWidth = 192.0f;
    private float radarHeight = 192.0f;

	// Use this for initialization
	void Start () 
    {
        this.setRadarCameraSize(this.RadarCamera);
        gretaPlayer = GameObject.Find(GretaController.GRETANAME);
        gretaScript = gretaPlayer.GetComponentInChildren<GretaController>();
        aiObjects = GameObject.FindGameObjectsWithTag(testAI.AITAGNAME);
        foreach (var ai in aiObjects)
        {
            enermy.Add(ai);
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
        this.setRadarCameraSize(this.RadarCamera);
        this.minAIDistance = 10000;
        foreach (var en in aiObjects)
        {
            if (this.getaiDistance(en) < minAIDistance)
                minAIDistance = this.getaiDistance(en);
        }
        //Quaternion temprotation = AISenserCamera.rotation;

        //if (AISenserCamera.rotation.y - new Vector3(0.0f, (minAIDistance * 9 - 45), 0.0f).y <= senserRadarMinAngles.y)
        //{
        //    temprotation.y = senserRadarMinAngles.y;
        //    AISenserCamera.rotation = temprotation;
        //}
        //else if (AISenserCamera.rotation.y - new Vector3(0.0f, (minAIDistance * 9 - 45), 0.0f).y >= senserRadarMinAngles.y + 270)
        //{
        //    temprotation.y = senserRadarMinAngles.y + 270;
        //    AISenserCamera.rotation = temprotation;
        //}
        //else
        //{
        //    temprotation.y -= minAIDistance * 9 - 45;
        //    AISenserCamera.rotation = temprotation;
        //}

        if (!gretaScript.IsGameStop())
        {

            if (AISenserCamera.localEulerAngles.y - new Vector3(0.0f, (minAIDistance * 9.0f - 45), 0.0f).y <= senserRadarMinAngles.y)
            {
                AISenserCamera.localEulerAngles = senserRadarMinAngles;
            }
            else if (AISenserCamera.localEulerAngles.y - new Vector3(0.0f, (minAIDistance * 9.0f - 45), 0.0f).y >= senserRadarMinAngles.y + 270.0f)
            {
                AISenserCamera.localEulerAngles = AISenserCamera.localEulerAngles - senserRadarMinAngles + new Vector3(0.0f, 270.0f, 0.0f);
            }
            else
            {
                AISenserCamera.localEulerAngles = AISenserCamera.localEulerAngles - new Vector3(0.0f, minAIDistance * 9 - 45, 0.0f);
            }
        }     
	}

    //Be called once per frame for every Collider other that is touching the trigger.
    void OnTriggerStay(Collider col)
    {
        if (string.Equals(col.tag, testAI.AITAGNAME))
            this.isAnyAIinStay = true;
        else
            this.isAnyAIinStay = false;
    }


    #region Supoprt Methods

    /// <summary>
    /// Get the distance from Enermy to Player.
    /// </summary>
    /// <param name="ai">Enermy</param>
    /// <returns>distance</returns>
    private float getaiDistance(GameObject ai)
    {
        return Vector3.Distance(this.gretaPlayer.transform.position, ai.transform.position);
    }

    /// <summary>
    /// Set the radarCamera width and height.
    /// </summary>
    /// <param name="camera">Send the radar camear.</param>
    private void setRadarCameraSize(Camera camera)
    {
        Rect rect;
        if (Screen.fullScreen)
            rect = new Rect(0, 0, this.radarWidth / Screen.currentResolution.width, this.radarHeight / Screen.currentResolution.height);
        else
            rect = new Rect(0, 0, this.radarWidth / Screen.width, this.radarHeight / Screen.height);
        camera.rect = rect;
    }

    #endregion
}
