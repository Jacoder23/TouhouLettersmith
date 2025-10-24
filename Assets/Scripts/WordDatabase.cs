using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class WordDatabase : MonoBehaviour
{
    public TextAsset wordList;
    public TextAsset bonusWordList;
    public List<string> validWords;
    public List<string> bonusWords;

    public static WordDatabase instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        //replace whitespace and/pr removing \n didn't work?? keeps saying aardvark is 9 characters?? just using substring now i give up
        validWords = wordList.text.ToUpper().Split(',').ToList();
        bonusWords = bonusWordList.text.ToUpper().Split(',').ToList();

        DontDestroyOnLoad(gameObject);
    }


    public string GetRandomValidWord()
    {
        return validWords[Random.Range(0, validWords.Count - 1)];
    }
    public string GetRandomBonusWord()
    {
        return bonusWords[Random.Range(0, bonusWords.Count - 1)];
    }

}
