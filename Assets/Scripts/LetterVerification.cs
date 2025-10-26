using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LetterVerification : MonoBehaviour
{
    public TextAsset letterFile; // csv
    public List<string> letter;
    public string nextWord;
    // todo: load specific text asset in player prefs
    void Start()
    {
        letter = letterFile.text.ToUpper().Split(',').ToList();
        nextWord = letter[0];
    }
}
