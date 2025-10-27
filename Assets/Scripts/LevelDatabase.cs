using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using System;

[Serializable]
public struct LevelData
{
    //public int turnLimit; // todo: add this after the jam for "endless" mode?
    public TextAsset letterFile;
    public TextAsset winDialogue;
    public int maxTurns;
    public List<TileTypes> tileTypesAvailable;
    public bool playTutorial;
}
public class LevelDatabase : SerializedMonoBehaviour
{
    public Dictionary<string, LevelData> nameToLevelData;

    // load data

    public LevelData GetCurrentLevel()
    {
        return nameToLevelData[PlayerPrefs.GetString("CurrentLevel")];
    }
}
