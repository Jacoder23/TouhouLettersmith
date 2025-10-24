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
}
