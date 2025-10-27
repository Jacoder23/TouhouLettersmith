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

        InitializeWordList();

        DontDestroyOnLoad(gameObject);
    }

    void InitializeWordList()
    {
        validWords = wordList.text.ToUpper().Split(',').ToList();
        bonusWords = bonusWordList.text.ToUpper().Split(',').ToList();
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
