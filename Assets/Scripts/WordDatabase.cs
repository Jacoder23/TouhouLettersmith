using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
public class WordDatabase : MonoBehaviour
{
    public TextAsset wordList;
    public TextAsset bonusWordList;
    //Store the valid words in hash sets grouped by their length to speed up lookups
    public Dictionary<int, HashSet<string>> validWordsByLength;
    public Dictionary<int, HashSet<string>> bonusWordsByLength;

    //Duplicate dictionaries to avoid flattening the hashsets every time
    private List<string> _allValidWords;
    private List<string> _allBonusWords;

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
        return _allValidWords.Concat(_allBonusWords).ToList();
    }


    [Button]
    void InitializeWordList()
    {
        validWordsByLength = new Dictionary<int, HashSet<string>>();
        bonusWordsByLength = new Dictionary<int, HashSet<string>>();

        //Valid words
        _allValidWords = wordList.text.ToUpper().Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
        foreach (string word in _allValidWords)
        {
            if (!validWordsByLength.ContainsKey(word.Length))
                validWordsByLength[word.Length] = new HashSet<string>();

            validWordsByLength[word.Length].Add(word);
        }

        //Bonus words
        _allBonusWords = bonusWordList.text.ToUpper().Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
        foreach (string word in _allBonusWords)
        {
            if (!bonusWordsByLength.ContainsKey(word.Length))
                bonusWordsByLength[word.Length] = new HashSet<string>();

            bonusWordsByLength[word.Length].Add(word);
        }

    }

    public string GetRandomValidWord()
    {
        if (_allValidWords == null || _allValidWords.Count == 0) return "ERROR";
        return _allValidWords[Random.Range(0, _allValidWords.Count)];
    }
    //Should work now
    public string GetRandomBonusWord()
    {
        if (_allBonusWords == null || _allBonusWords.Count == 0) return "ERROR";
        return _allBonusWords[Random.Range(0, _allBonusWords.Count)];
    }

}
