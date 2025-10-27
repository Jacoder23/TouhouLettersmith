using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;
using TMPro;
public class TurnCounter : MonoBehaviour
{
    public LevelDatabase database;
    [SerializeField] private int maxTurns = 999;
    public int turns = 1;
    public TextMeshProUGUI indicator;
    public void Start()
    {
        if (PlayerPrefs.GetString("CurrentLevel").Length != 0)
        {
            maxTurns = database.GetCurrentLevel().maxTurns;
            indicator.text = (maxTurns - turns) + " TURNS LEFT";
        }
        turns = 0;
    }

    public void Turn()
    {
        turns++;

        if(turns >= maxTurns)
        {
            // todo: lose
        }

        indicator.text = (maxTurns - turns) + " TURNS LEFT";
    }
}
