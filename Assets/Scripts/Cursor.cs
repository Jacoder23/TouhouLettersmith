using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI.Extensions;
using UnityEngine.SceneManagement;
using JSAM;
public class Cursor : MonoBehaviour
{
    public TextMeshProUGUI validityIndicator;
    public WordVerification verifier;
    public TileManager tileManager;
    public UILineTextureRenderer line;
    public SceneTransition transition;
    public Animator kogasaAnimation;
    public ShakeObjects kogasaShake;
    [Header("Settings")]
    public bool titleScreen = false;
    [Header("Attributes")]
    public List<Tile> wordInProgress;
    public TilePosition cursorPosition;
    public List<string> letterInProgress;
    public bool playingSubmitAnimation = false;
    // Start is called before the first frame update

    public void TitleScreenStart()
    {
        if (titleScreen)
        {
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
        if (wordInProgress == null)
            return WordValidity.Invalid;

        if (wordInProgress.Count == 0)
            return WordValidity.Invalid;

        return verifier.ValidWord(string.Join("", wordInProgress.Select(x => x.value).ToArray()));
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
            cursorPosition = new TilePosition();

        //Debug.Log(cursorPosition.x + ", " + cursorPosition.y);
    }

    public void SubmitWord()
    {
        PlayWordSound();
        if (ValidateWord() != WordValidity.Invalid)
        {
            letterInProgress.Add(CurrentWord());
            // todo: change animation depending on what's going on
            kogasaAnimation.Play("KogasaHit");
            playingSubmitAnimation = true;
            Invoke("ClearBoard", 1.5f); // todo: unhardcode this? idk how useful itd be to expose to editor since the animation isnt gonna get longer or shorter
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
        validityIndicator.text = CurrentWord();
        var validity = ValidateWord();

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
