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
    public List<TileType> tileTypesAvailable;
    public bool playTutorial;
    [Header("Spawn Rates")]
    public float chanceOfFireTile;
    public float chanceOfBombTile;
    public float chanceOfStoneTile;
    public float chanceOfDrunkenTile;
    //public bool spawnSpecialTilesOnFirstScreen; // todo: bring this back, but JSAM gets weird and glitches out so turn off sounds there
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
