using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI.Extensions;
using UnityEngine.SceneManagement;
using JSAM;
using System.Text.RegularExpressions;
using System;

public class Cursor : MonoBehaviour
{
    public TextMeshProUGUI validityIndicator;
    public WordVerification verifier;
    WordDatabase database;
    public TileManager tileManager;
    public UILineTextureRenderer line;
    public SceneTransition transition;
    public Animator kogasaAnimation;
    public ShakeObjects kogasaShake;
    public TurnCounter turnCounter;
    public LetterVerification letterVerification;
    public ScrambleTiles scrambleTiles;
    [Header("Settings")]
    public bool titleScreen = false;
    [Header("Attributes")]
    public List<Tile> wordInProgress;
    public TilePosition cursorPosition;
    public List<string> letterInProgress;
    public bool playingSubmitAnimation = false;
    // Start is called before the first frame update
    public bool canScramble = true;

    public void TitleScreenStart()
    {
        if (titleScreen)
        {
            // todo: add a continue button later on and a clear progress, for now we'll erase
            PlayerPrefs.SetString("CurrentLevel", "NONE");
            PlayWordSound();
            if (CurrentWord() == "LETTERSMITH")
            {
                PlayerPrefs.SetString("NextScene", "VNScene");
                transition.NextScene();
            }
        }
    }
    void Start()
    {
        database = verifier.GetComponent<WordDatabase>();
        wordInProgress = new List<Tile>();
        letterInProgress = new List<string>();
    }
    void Update()
    {
        if (verifier == null)
            verifier = FindFirstObjectByType<WordVerification>(); // strangely doesn't work in Start
    }

    WordValidity ValidateWord()
    {
        if (wordInProgress == null || wordInProgress.Count == 0)
            return WordValidity.Invalid;

        //Build the string pattern
        char[] charBuilder = new char[wordInProgress.Count];
        bool hasRainbow = false;

        for (int i = 0; i < wordInProgress.Count; i++)
        {
            if (wordInProgress[i].type == TileType.Rainbow)
            {
                charBuilder[i] = '?';
                hasRainbow = true;
            }
            else
            {
                charBuilder[i] = wordInProgress[i].value[0];
            }
        }
        string pattern = new string(charBuilder);

        if (hasRainbow)
        {
            //Look for the best matching word
            string matchedWord = verifier.GetBestMatch(pattern, letterVerification.nextWord);

            if (matchedWord != null)
            {
                ApplyMatchToTiles(matchedWord);

                return verifier.ValidWord(matchedWord);
            }
            else
            {
                return WordValidity.Invalid;
            }
        }
        else
        {
            //Normal words
            return verifier.ValidWord(pattern);
        }
    }
    void ApplyMatchToTiles(string matchedWord)
    {
        for (int i = 0; i < wordInProgress.Count; i++)
        {
            if (wordInProgress[i].type == TileType.Rainbow)
            {
                // Update the tile to show the letter it matched
                wordInProgress[i].SetTileValue(matchedWord[i].ToString());
            }
        }
    }
    string SearchForWord(string originalWord, out List<int> wildcardValues, char[] candidateWord = null, WordValidity searchType = WordValidity.Valid)
    {
        bool skipIncrement = false;
        wildcardValues = new List<int>();

        if (originalWord.Length == letterVerification.nextWord.Length)
        {
            bool canMatchGoalWord = true;
            for (int i = 0; i < originalWord.Length; i++)
            {
                if (originalWord[i] == letterVerification.nextWord[i])
                {
                    continue;
                }
                else
                {
                    if (originalWord[i] == '?')
                    {
                        wildcardValues.Add(Extensions.alphabet.IndexOf(letterVerification.nextWord[i]));
                        continue;
                    }
                    else
                    {
                        canMatchGoalWord = false;
                        break;
                    }
                }
            }
            if (canMatchGoalWord)
            {
                return letterVerification.nextWord;
            }
            else
            {
                wildcardValues = new List<int>();
            }
        }

        candidateWord = originalWord.Replace('?', 'A').ToCharArray();

        // loop begins here

        while (true)
        {
            wildcardValues.Clear();

            for (int i = 0; i < originalWord.Length; i++)
            {
                if (originalWord[i] == '?')
                    wildcardValues.Add(Extensions.alphabet.IndexOf(candidateWord[i])); //
            }

            // increment, carry if over 26

            if (!skipIncrement)
            {
                wildcardValues[0]++;

                for (int i = 0; i < wildcardValues.Count; i++)
                {
                    // means we tried everything and got nothing
                    if (wildcardValues.Last() > 25)
                    {
                        wildcardValues[wildcardValues.Count - 1] = 25;
                        return null;
                    }

                    if (wildcardValues[i] > 25)
                    {
                        wildcardValues[i] = 0;
                        wildcardValues[i + 1]++;
                    }
                }
            }

            // convert back to string then call again if not valid

            int j = 0;

            for (int i = 0; i < originalWord.Length; i++)
            {
                if (originalWord[i] == '?')
                {
                    candidateWord[i] = Extensions.alphabet[wildcardValues[j]];
                    j++;
                }
            }

            if (verifier.ValidWord(Extensions.CharArrayToString(candidateWord)) == searchType)
            {
                // edit the tile value
                return Extensions.CharArrayToString(candidateWord);
            }
            else
            {
                continue;
                //// i think these are causing stack overflows, due to the recursive depth
                //// todo: look into rewriting to be non recursive using a stack?
                //return SearchForWord(originalWord, out wildcardValues, candidateWord, searchType);
            }
        }
    }

    string CurrentWord()
    {
        return string.Join("", wordInProgress.Select(x => x.value).ToArray());
    }

    void LateUpdate()
    {
        if (wordInProgress.Count == 0)
            validityIndicator.text = "";
    }

    void UpdateCursorPosition()
    {
        if (wordInProgress.Count > 0)
            cursorPosition = wordInProgress.LastOrDefault().position;
        else
            cursorPosition = new TilePosition(-1,-1);

        //Debug.Log(cursorPosition.x + ", " + cursorPosition.y);
    }

    bool win = false;

    public void SubmitWord()
    {
        PlayWordSound();
        var validity = ValidateWord();
        if (validity != WordValidity.Invalid)
        {
            canScramble = false;
            if (CurrentWord() == letterVerification.nextWord)
                win = letterVerification.ContinueToNextWord();

            letterInProgress.Add(CurrentWord());

            if (CurrentWord().Length >= 6)
            { // length of 6 tiles for a free rainbow tile, todo: unhardcode since there'd be an item that can reduce the req to 5
                tileManager.rainbowTileSpawnQueue++;
            }
            // todo: change animation depending on what's going on
            if (validity == WordValidity.Valid)
            {
                kogasaAnimation.Play("KogasaHit");
                Invoke("ClearBoard", 1.5f); // todo: unhardcode this? idk how useful itd be to expose to editor since the animation isnt gonna get longer or shorter
                if (wordInProgress.Any(x => x.type == TileType.Drunken))
                    scrambleTiles.DrunkenScrambleAllTiles(1.5f + 0.05f);
            }
            else if (validity == WordValidity.Bonus)
            {
                tileManager.rainbowTileSpawnQueue++;
                kogasaAnimation.Play("KogasaSpecialHit");
                Invoke("ClearBoard", 3f);
                if (wordInProgress.Any(x => x.type == TileType.Drunken))
                    scrambleTiles.DrunkenScrambleAllTiles(3f + 0.05f);
            }
            playingSubmitAnimation = true;

        }
    }

    void PlayWordSound()
    {
        switch (ValidateWord())
        {
            case WordValidity.Invalid:
                AudioManager.PlaySound(LibrarySounds.InvalidWord);
                break;
            case WordValidity.Valid:
                AudioManager.PlaySound(LibrarySounds.ValidWord);
                break;
            case WordValidity.Bonus:
                AudioManager.PlaySound(LibrarySounds.RainbowWord);
                break;
        }
    }

    public void ClearBoard()
    {
        canScramble = true;
        if (!win)
            turnCounter.Turn();
        wordInProgress.Clear();
        tileManager.RemoveSelectedTiles();
        UpdateCursorPosition();
        UpdateLineRenderer();
        playingSubmitAnimation = false;
    }
    public void ClearBoardWithoutTurnAdvance()
    {
        canScramble = true;
        wordInProgress.Clear();
        UpdateCursorPosition();
        UpdateLineRenderer();
        playingSubmitAnimation = false;
    }

    void UpdateLineRenderer()
    {
        if (wordInProgress.Count < 2)
            line.Points = new Vector2[] { Vector2.zero, Vector2.zero };
        else
            line.Points = wordInProgress.Select(x => (Vector2)x.transform.localPosition).ToArray();
    }

    public void AddTile(Tile tile)
    {
        if (wordInProgress.Count == 0 && !titleScreen)
            kogasaAnimation.Play("KogasaPrepare");

        wordInProgress.Add(tile);
        UpdateCursorPosition();
        UpdateLineRenderer();
        UpdateIndicator();
        //Debug.Log(string.Join(' ', wordInProgress.Select(x => x.value).ToArray()));
    }

    public void RemoveTile(Tile tile)
    {
        if (wordInProgress.Count == 1 && !titleScreen)
            kogasaAnimation.Play("KogasaDoNothing");

        wordInProgress.Remove(tile);
        UpdateCursorPosition();
        UpdateLineRenderer();
        UpdateIndicator();
        //Debug.Log(string.Join(' ', wordInProgress.Select(x => x.value).ToArray()));
    }

    public bool ToggleTile(Tile tile)
    {
        if (!wordInProgress.Contains(tile))
        {
            AddTile(tile);
            return true;
        }
        else
        {
            RemoveTile(tile);
            return false;
        }
    }
    // tile islands: when you use the same logic for finding valid tiles to turn on AND off and so can create isolated but selected tiles by traveling backwards without following the order of the tiles as they were originally selected
    public bool ToggleTileWithoutIslands(Tile tile)
    {
        if (!wordInProgress.Contains(tile))
        {
            AddTile(tile);
            return true;
        }
        else
        {
            if (wordInProgress.LastOrDefault() == tile)
            {
                RemoveTile(tile);
                return false;
            }
            return true;
        }
    }

    void UpdateIndicator()
    {
        var validity = ValidateWord(); // must come before the text is updated due to rainbow tiles
        validityIndicator.text = CurrentWord();

        if (validity == WordValidity.Invalid)
        {
            validityIndicator.color = Color.gray;
        }
        else if (validity == WordValidity.Valid)
        {
            validityIndicator.color = Color.white;
        }
        else if (validity == WordValidity.Bonus)
        {
            validityIndicator.color = Color.white;
            validityIndicator.text = "<rainb>" + CurrentWord() + "</rainb>";
        }
    }
}
