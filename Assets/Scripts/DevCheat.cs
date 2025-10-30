using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System;
using JSAM;
using Sirenix.OdinInspector;

public class DevCheat : MonoBehaviour
{
    public OnWin win;
    List<string> keys = new List<string>();
    private void Update()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown("l"))
                keys.Add("L");
            else if (Input.GetKeyDown("o"))
                keys.Add("O");
            else if (Input.GetKeyDown("v"))
                keys.Add("V");
            else if (Input.GetKeyDown("e"))
                keys.Add("E");
            else if (Input.GetKeyDown("1"))
                keys.Add("1");
            else if (Input.GetKeyDown("4"))
                keys.Add("4");
            else if (Input.GetKeyDown("3"))
                keys.Add("3");

            if(string.Join(string.Empty, keys) == "LOVE143")
            {
                win.Win();
            }
        }
    }
}
