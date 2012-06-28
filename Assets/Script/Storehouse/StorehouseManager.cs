/*
 * file: StorehouseManager.cs
 * last update: 2011/5/16  
 * version : 1.1
 * 
 * Ver1.0 -> Ver1.1說明:
 * 更改物件獲得方式程式寫法，未來不必輸入total，命名也不必統一化(但命名還是要完整有意義)

 * 
 * 想法：
 * 透過vector3陣列，一開始儲存所有物件的原始位置
 * 利用RestartObject()函式呼叫回復起始位置(用於開關門時)
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StorehouseManager : MonoBehaviour
{    
    public bool isRestart = false;

    private List<Vector3> OriginPosition = new List<Vector3>();
    private Transform[] trans;

    void Start()
    {
        trans = GetComponentsInChildren<Transform>();

        for (int i = 1; i < trans.Length; i++)        
            OriginPosition.Add(trans[i].position);
        
    }

    public void RestartObject()
    {
        for (int i = 1; i < trans.Length; i++)        
            trans[i].position = OriginPosition[i - 1];
        
    }

    void Update()
    {
        if (isRestart)
        {
            for (int i = 1; i < trans.Length; i++)
                trans[i].position = OriginPosition[i - 1];

            isRestart = false;
        }
    }
}
