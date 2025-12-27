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
        if (string.IsNullOrEmpty(word))
            return WordValidity.Invalid;
        int len = word.Length;

        //Debug.Log("Validating " + word + " with sublist of key " + word[0].ToString());

        if (database.bonusWordsByLength.ContainsKey(len) && database.bonusWordsByLength[len].Contains(word))
            return WordValidity.Bonus;
        if (database.validWordsByLength.ContainsKey(len) && database.validWordsByLength[len].Contains(word))
            return WordValidity.Valid;
        
        return WordValidity.Invalid;
    }

    public string GetBestMatch(string pattern, string goalWord = "")
    {
        int len = pattern.Length;

        if (!string.IsNullOrEmpty(goalWord) && goalWord.Length == len)
        {
            if (IsMatch(goalWord, pattern)) return goalWord;
        }

        if (database.bonusWordsByLength.ContainsKey(len))
        {
            foreach (var candidate in database.bonusWordsByLength[len])
            {
                if (IsMatch(candidate, pattern)) return candidate;
            }
        }

        if (database.validWordsByLength.ContainsKey(len))
        {
            foreach (var candidate in database.validWordsByLength[len])
            {
                if (IsMatch(candidate, pattern)) return candidate;
            }
        }

        return null;
    }
    //Checks if a word matches a pattern using the wildcard '?'
    private bool IsMatch(string word, string pattern)
    {
        for (int i = 0; i < word.Length; i++)
        {
            if (pattern[i] != '?' && pattern[i] != word[i])
                return false;
        }
        return true;
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
