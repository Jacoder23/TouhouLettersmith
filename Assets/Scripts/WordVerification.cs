using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using System;

public enum WordValidity
{
    Invalid,
    Valid,
    Bonus
}

public class WordVerification : MonoBehaviour
{
    public WordDatabase database;

    public WordValidity ValidWord(string word)
    {
        if(word == null)
            return WordValidity.Invalid;

        //Debug.Log("Validating " + word + " with sublist of key " + word[0].ToString());

        if (database.bonusWords[word.Substring(0,Math.Min(word.Length, 3))].Contains(word))
            return WordValidity.Bonus;
        else if (database.validWords[word.Substring(0, Math.Min(word.Length, 3))].Contains(word))
            return WordValidity.Valid;
        else
            return WordValidity.Invalid;
    }

    // just for curiosity's sake
    [Button]
    public string LongestWord()
    {
        string longestWord = database.CombinedList().OrderByDescending(s => s.Length).First();
        Debug.Log(longestWord);
        return longestWord;
    }
}
