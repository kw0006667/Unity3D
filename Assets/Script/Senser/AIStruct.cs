using UnityEngine;
using System.Collections;

public class IAIStruct 
{

    public string name { get; private set; }
    public Vector3 position { get; private set; }

    public IAIStruct(string _name, Vector3 _position)
    {
        this.name = _name;
        this.position = _position;
    }
}
