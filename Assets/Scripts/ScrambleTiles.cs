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
            tileManager.DeselectAllTiles();
            cursor.ClearBoard();

            JSAM.AudioManager.PlaySound(LibrarySounds.Scramble);
        }
    }
    public void DelayedScrambleAllTiles(float delay)
    {
        Invoke("ScrambleAllTiles", delay);
    }
}
