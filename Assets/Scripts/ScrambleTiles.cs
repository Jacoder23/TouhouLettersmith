using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using JSAM;
public class ScrambleTiles : MonoBehaviour
{
    public TileManager tileManager;
    public Cursor cursor;

    public void ScrambleAllTiles()
    {
        if (cursor.canScramble)
        {
            tileManager.ScrambleAllTiles();
            cursor.ClearBoard();

            JSAM.AudioManager.PlaySound(LibrarySounds.Scramble);
        }
    }
    public void DelayedScrambleAllTiles(float delay)
    {
        Invoke("ScrambleAllTiles", delay);
    }

    public void ScrambleAllTilesWithoutTurnAdvance()
    {
        if (cursor.canScramble)
        {
            tileManager.ScrambleAllTiles();
            cursor.ClearBoardWithoutTurnAdvance();

            JSAM.AudioManager.PlaySound(LibrarySounds.Scramble);
        }
    }

    public void DrunkenScrambleAllTiles(float delay)
    {
        Invoke("ScrambleAllTilesWithoutTurnAdvance", delay);
    }
}
