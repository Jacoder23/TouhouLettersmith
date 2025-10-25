using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LetterVerification : MonoBehaviour
{
    public TextAsset letterFile; // csv
    public List<string> letter;
    public string nextWord;
    // not a singleton unlike the word database/verification since the specific letter to write changes between scenes/levels
    void Awake()
    {
        letter = letterFile.text.ToUpper().Split(',').ToList();
        nextWord = letter[0];
    }
}
