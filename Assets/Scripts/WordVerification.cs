using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
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

        if (database.bonusWords.Contains(word))
            return WordValidity.Bonus;
        else if (database.validWords.Contains(word))
            return WordValidity.Valid;
        else
            return WordValidity.Invalid;
    }
}
