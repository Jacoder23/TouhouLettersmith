using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System;

public enum TileType
{
    Normal,
    Rainbow,
    Fire,
    Bomb,
    Stone,
    Drunken,
    SnowballRainbow // temporary for a turn
}

[Serializable]
public class TilePosition
{
    [ShowInInspector]
    public int x = -1;
    [ShowInInspector]
    public int y = -1; // when negative you can start anywhere

    // i know this is stupid
    public override bool Equals(object obj)
    {
        return obj is TilePosition position &&
               x == position.x &&
               y == position.y;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(x, y);
    }
}
public class Tile : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public Image image;

    public TileManager tileManager;
    public Cursor cursor;

    public Animator animator;

    [Header("Attributes")]
    public string value;
    public bool selected;
    public TilePosition position;
    public bool newTile = false;
    public TileType type;

    private void Start()
    {
        transform.localScale = Vector3.one;
    }
    public void Fall(Vector2 destination, float delay = 0f)
    {
        var d = UnityEngine.Random.Range(0f, 0.08f) + delay;
        LeanTween.moveLocal(gameObject, destination, 1f).setEaseOutQuint().setDelay(d); // TODO: unhardcode this timing
        // random so its not too orderly
    }
    public void RandomizeTileValue()
    {
        //SetTileValue(tileManager.WeightedRandomLetterOfTheAlphabet());
        SetTileValue(tileManager.QueuedRandomLetterOfTheAlphabet());
    }

    void Update()
    {
        //tile value is the same but displayed differently
        if (type == TileType.Rainbow)
            textMeshPro.text = "?";
        else
            textMeshPro.text = value;

        if(selected)
            image.color = Color.gray;
        else
            image.color = Color.white;
    }

    [Button]
    public void ChangeTileType(TileType newType)
    {
        type = newType;
        switch (type)
        {
            case TileType.Normal:
                animator.Play("Normal");
                break;
            case TileType.Rainbow:
                animator.Play("Rainbow");
                break;
            case TileType.Fire:
                animator.Play("Fire");
                break;
            case TileType.Bomb:
                animator.Play("Bomb");
                break;
            case TileType.Stone:
                animator.Play("Stone");
                break;
            case TileType.Drunken:
                animator.Play("Drunken");
                break;
            case TileType.SnowballRainbow:
                animator.Play("Rainbow");
                break;
        }
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
        if (cursor.playingSubmitAnimation)
            return;

        if (Input.GetMouseButton(0))
            Toggle();
    }
    public void ToggleIfValidTile()
    {
        if (cursor.playingSubmitAnimation)
            return;

        if (Extensions.ValidTileDestination(cursor.cursorPosition, position, tileManager.gridSize))
        {
            Toggle();
        }
    }
    public void ToggleIfMouseHeldAndValidTile()
    {
        if (cursor.playingSubmitAnimation)
            return;

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
