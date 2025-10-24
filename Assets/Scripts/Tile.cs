using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class TilePosition
{
    public int x = -1;
    public int y = -1; // when negative you can start anywhere
}
public class Tile : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public Image image;

    public TileManager tileManager;
    public Cursor cursor;

    [Header("Attributes")]
    public string value;
    public bool selected;
    public TilePosition position;
    // store tile value on its own along with bools for if its on fire, etc.

    private void Start()
    {
        transform.localScale = Vector3.one;
    }

    public void RandomizeTileValue()
    {
        //SetTileValue(tileManager.WeightedRandomLetterOfTheAlphabet());
        SetTileValue(tileManager.QueuedRandomLetterOfTheAlphabet());
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
        selected = cursor.ToggleTileWithoutIslands(this);
    }

    public void ToggleIfMouseHeld()
    {
        if (Input.GetMouseButton(0))
            Toggle();
    }
    public void ToggleIfValidTile()
    {
        if (Extensions.ValidTileDestination(cursor.cursorPosition, position, tileManager.gridSize))
        {
            Toggle();
        }
    }
    public void ToggleIfMouseHeldAndValidTile()
    {
        if (Extensions.ValidTileDestination(cursor.cursorPosition, position, tileManager.gridSize))
        {
            if (Input.GetMouseButton(0))
                Toggle();
        }
    }

    public void GameFeelMouseEnter()
    {
        //Debug.Log(position.x + ", " + position.y);
        LeanTween.scale(this.gameObject, new Vector3(1.1f, 1.1f, 1.1f), 0.25f).setEaseOutQuad();
        LeanTween.rotateLocal(this.gameObject, new Vector3(0f, 0f, -6f), 0.25f).setEaseOutQuad(); // todo: don't hardcode this so it can be a player setting
    }

    public void GameFeelMouseExit()
    {
        LeanTween.scale(this.gameObject, new Vector3(1f, 1f, 1f), 0.25f).setEaseOutQuad();
        LeanTween.rotateLocal(this.gameObject, new Vector3(0f, 0f, 0f), 0.25f).setEaseOutQuad();
    }
}
