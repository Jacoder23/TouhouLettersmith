using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public Image image;

    public string value;
    bool selected;

    public TileManager tileManager;
    public Cursor cursor;
    // store tile value on its own along with bools for if its on fire, etc.
    void Start()
    {
        // placeholder
        SetTileValue(tileManager.WeightedRandomLetterOfTheAlphabet());
    }

    void Update()
    {
        //if(selected)
        // todo: add effect
        if(selected)
            image.color = Color.gray;
        else
            image.color = Color.white;
    }

    public void SetTileValue(string text)
    {
        value = text;
        textMeshPro.text = value;
    }

    public void Toggle()
    {
        selected = cursor.ToggleTile(this);
    }

    public void ToggleIfMouseHeld()
    {
        if (Input.GetMouseButton(0))
            Toggle(); // todo: stop the flicker
    }

    public void GameFeelMouseEnter()
    {
        LeanTween.scale(this.gameObject, new Vector3(1.1f, 1.1f, 1.1f), 0.25f).setEaseOutQuad();
        LeanTween.rotateLocal(this.gameObject, new Vector3(0f, 0f, -6f), 0.25f).setEaseOutQuad(); // todo: don't hardcode this so it can be a player setting
    }

    public void GameFeelMouseExit()
    {
        LeanTween.scale(this.gameObject, new Vector3(1f, 1f, 1f), 0.25f).setEaseOutQuad();
        LeanTween.rotateLocal(this.gameObject, new Vector3(0f, 0f, 0f), 0.25f).setEaseOutQuad();
    }
}
