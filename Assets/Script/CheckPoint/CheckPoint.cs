using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;

public class CheckPoint : MonoBehaviour {
    private GameObject player;
    private bool isTrigger;
    private string NowScene;
    private float checkpoint_x;
    private float checkpoint_y;
    private float checkpoint_z;

    private string FILE_NAME;       // The file name of saving.

	// Use this for initialization
	void Start () 
    {
        player = GameObject.FindGameObjectWithTag("Greta");
        FILE_NAME = PlayerPrefs.GetString("SaveFileName");
	}
	
	// Update is called once per frame
    void Update()
    {

    }
    /// <summary>
    /// Save the position of Greta.
    /// </summary>
    void SaveGretaPosition()
    {
        NowScene = Application.loadedLevelName;
        checkpoint_x = transform.position.x;
        checkpoint_y = transform.position.y;
        checkpoint_z = transform.position.z;

        using (FileStream fs = new FileStream(FILE_NAME, FileMode.OpenOrCreate))
        {
            BinaryWriter w = new BinaryWriter(fs);
            w.Write(NowScene);
            w.Write(checkpoint_x);
            w.Write(checkpoint_y);
            w.Write(checkpoint_z);

            w.Close();
            fs.Close();
        }
    }

    void OnTriggerEnter(Collider MainPlayer)
    {
        if (MainPlayer.tag == "Greta")
        {
            SaveGretaPosition();        // Save the position of Greta.
        }
    }
}
