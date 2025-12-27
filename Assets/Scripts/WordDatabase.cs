using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
public class WordDatabase : MonoBehaviour
{
    public TextAsset wordList;
    public TextAsset bonusWordList;
    public Dictionary<string, string[]> validWords; // (e.g. key is A or first letter of word search is A then get list of words starting with A)
    public Dictionary<string, string[]> bonusWords;

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

    public List<string> CombinedList()
    {
        return validWords["default"].Concat(bonusWords["default"]).ToList();
    }

    [Button]
    void InitializeWordList()
    {
        validWords = new Dictionary<string, string[]>();
        bonusWords = new Dictionary<string, string[]>();

        validWords.Add("default", wordList.text.ToUpper().Split(',').Where(x => x.Length > 0).ToArray());
        bonusWords.Add("default", bonusWordList.text.ToUpper().Split(',').Where(x => x.Length > 0).ToArray());

        foreach(char letter in Extensions.alphabet)
        {
            validWords.Add(letter.ToString(), validWords["default"].Where(x => x[0] == letter).ToArray());
            bonusWords.Add(letter.ToString(), bonusWords["default"].Where(x => x[0] == letter).ToArray());
        }
    }

    public string GetRandomValidWord()
    {
        return validWords["default"][Random.Range(0, validWords.Count - 1)];
    }
    public string GetRandomBonusWord()
    {
        return bonusWords["default"][Random.Range(0, bonusWords.Count - 1)];
    }

}
