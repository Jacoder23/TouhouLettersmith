using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI.Extensions;
using UnityEngine.SceneManagement;
using JSAM;
using System.Text.RegularExpressions;

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
        // todo: rainbow
        // just loop for each rainbow, go through 26 letters and nest/recurse to try every combination
        // this may wreck computers with enough rainbow tiles
        if (wordInProgress == null)
            return WordValidity.Invalid;

        if (wordInProgress.Count == 0)
            return WordValidity.Invalid;

        if (wordInProgress.Any(x => x.type == TileType.Rainbow))
        {
            var word = new List<string>();
            int numOfWildcards = 0;
            foreach (var tile in wordInProgress)
            {
                if (tile.type == TileType.Rainbow)
                {
                    word.Add("?");
                    numOfWildcards++;
                }
                else
                {
                    word.Add(tile.value);
                }
            }

            var wildcards = new List<int>();
            var validity = verifier.ValidWord(SearchForValidWord(string.Join(string.Empty, word), out wildcards));

            int i = 0;

            foreach (var tile in wordInProgress)
            {
                if (tile.type == TileType.Rainbow)
                {
                    tile.SetTileValue(Extensions.alphabet[wildcards[i]].ToString());
                    i++;
                }
            }
            return validity;
        }
        else
        {
            return verifier.ValidWord(string.Join("", wordInProgress.Select(x => x.value).ToArray()));
        }
    }

    // todo: if its the same length as the goal word then try to manually calc if its possible, get the difference between the two strings and if the only difference is the ? then its a match
    string SearchForValidWord(string originalWord, out List<int> wildcardValues, char[] candidateWord = null)
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

        if(candidateWord == null)
        {
            skipIncrement = true;
            candidateWord = originalWord.Replace('?', 'A').ToCharArray();
        }

        for (int i = 0; i < originalWord.Length; i++)
        {
            if (originalWord[i] == '?')
                wildcardValues.Add(Extensions.alphabet.IndexOf(candidateWord[i]));
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
            if (originalWord[i] == '?') {
                candidateWord[i] = Extensions.alphabet[wildcardValues[j]];
                j++;
            }
        }

        if (verifier.ValidWord(Extensions.CharArrayToString(candidateWord)) != WordValidity.Invalid)
        {
            // edit the tile value
            return Extensions.CharArrayToString(candidateWord);
        }
        else
        {
            return SearchForValidWord(originalWord, out wildcardValues, candidateWord);
        }

        // base 26 its a base 26 number
        // an int[]
        // if all the tiles are rainbow then just select one word of that length
        // break upon finding one should cut down on that 8 trillion
    }

    string CurrentWord()
    {
        // display with ?
        if (wordInProgress.Any(x => x.type == TileType.Rainbow))
        {
            return string.Join("", wordInProgress.Select(x => x.value).ToArray());
        }
        else
        {
            return string.Join("", wordInProgress.Select(x => x.value).ToArray());
        }
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
            cursorPosition = new TilePosition();

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
                    scrambleTiles.DelayedScrambleAllTiles(1.5f + 0.05f);
            }
            else if (validity == WordValidity.Bonus)
            {
                tileManager.rainbowTileSpawnQueue++;
                kogasaAnimation.Play("KogasaSpecialHit");
                Invoke("ClearBoard", 3f);
                if (wordInProgress.Any(x => x.type == TileType.Drunken))
                    scrambleTiles.DelayedScrambleAllTiles(3f + 0.05f);
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
