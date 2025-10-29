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
    public bool notALevel; // equiv to null
    [Header("Spawn Rates")]
    public float chanceOfFireTile;
    public float chanceOfBombTile;
    public float chanceOfStoneTile;
    public float chanceOfDrunkenTile;
    [Header("Items")]
    public bool snowball;
    //public bool spawnSpecialTilesOnFirstScreen; // todo: bring this back, but JSAM gets weird and glitches out so turn off sounds there
}
public class LevelDatabase : SerializedMonoBehaviour
{
    public Dictionary<string, LevelData> nameToLevelData;
    public LevelData defaultLevel;

    // load data

    public LevelData GetCurrentLevel()
    {
        if (!nameToLevelData.ContainsKey(PlayerPrefs.GetString("CurrentLevel")))
        {
            LevelData levelData = defaultLevel;
            levelData.notALevel = true;
            return levelData;
        }
        else
        {
            return nameToLevelData[PlayerPrefs.GetString("CurrentLevel")];
        }
    }
}
