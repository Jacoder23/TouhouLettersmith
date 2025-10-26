using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LetterVerification : MonoBehaviour
{
    public LevelDatabase database;
    public TextAsset letterFile; // csv
    public List<string> letter;
    public string nextWord;
    // todo: load specific text asset in player prefs
    void Start()
    {
        if (PlayerPrefs.GetString("CurrentLevel").Length != 0)
        {
            letterFile = database.GetCurrentLevel().letterFile;
            letter = letterFile.text.ToUpper().Split(',').ToList();
            nextWord = letter[0];
        }
    }
}
