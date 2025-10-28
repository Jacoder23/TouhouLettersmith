using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
public class LetterVerification : MonoBehaviour
{
    public LevelDatabase database;
    public TextAsset letterFile; // csv
    public List<string> letter;
    int wordIndex;
    public string nextWord;
    public bool inGameplayScene;
    public TextMeshProUGUI nextWordIndicator;
    public OnWin win;
    // todo: load specific text asset in player prefs
    void Awake()
    {
        if (PlayerPrefs.GetString("CurrentLevel").Length != 0)
        {
            letterFile = database.GetCurrentLevel().letterFile;
            letter = letterFile.text.ToUpper().Split(',').ToList();
            wordIndex = 0;
            nextWord = letter[wordIndex];
            if(inGameplayScene)
                nextWordIndicator.text = nextWord;
        }
    }

    public bool ContinueToNextWord()
    {
        wordIndex++;
        if (wordIndex < letter.Count)
        {
            if(wordIndex != letter.Count)
                nextWord = letter[wordIndex];

            if (inGameplayScene)
                nextWordIndicator.text = nextWord;

            return false;
        }
        else
        {
            win.Win();
            PlayerPrefs.SetString("NextScene", "VNScene");

            return true;
        }
    }
    public string GetNextGoalWord()
    {
        if (wordIndex + 1 < letter.Count)
            return letter[wordIndex + 1];
        else
            return nextWord;
    }
}
