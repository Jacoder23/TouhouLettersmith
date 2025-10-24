using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KaimiraGames;
public class TileManager : MonoBehaviour
{
    public Tile tilePrefab;
    public Transform grid;
    public Cursor cursor;
    public WordDatabase database;

    [Header("Settings")]
    public int gridSize;
    public float spaceBetweenTiles;
    public int randomLetterQueueMinLength = 20;
    public float chanceOfBonusWords = 0.1f;
    public float chanceOfActualRandomLetter = 0.5f;

    [Header("Preview")]
    public string randomLetterQueue = "";

    List<Tile> instantiatedTiles;
    string[][] currentBoardState; // encode in each string extra data like what about the other things
    // format: X010101 with each 0/1 showing if it's a special tile type or not, theoretically allows combinations of tile types but that's out of scope
    
    string[][] blankBoardState;
    WeightedList<string> weightedRandomAlphabet;

    Vector2 offset;
    void Start()
    {
        offset = new Vector2((gridSize - 1) * spaceBetweenTiles / 2f, (gridSize + 1) * spaceBetweenTiles / 2f);

        InitWeightedAlphabet();

        instantiatedTiles = new List<Tile>();

        blankBoardState = new string[gridSize][];

        for (int i = 0; i < blankBoardState.Length; i++)
        {
            blankBoardState[i] = new string[gridSize];
            for(int j = 0; j < blankBoardState[i].Length; j++)
            {
                blankBoardState[i][j] = j % 2 == 0 ? "A" : "B";
            }
        }

        PopulateTileGrid();

        currentBoardState = GetBoardStateFromTiles();

        Extensions.DebugLog2DJaggedArray(currentBoardState);
    }

    Vector2 TilePositionToLocalPosition(TilePosition pos, int gridSize)
    {
        return new Vector2(pos.x * spaceBetweenTiles, (float)(gridSize - pos.y) * spaceBetweenTiles) - offset;
    }

    void PopulateTileGrid()
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                var tile = Instantiate(tilePrefab);
                tile.tileManager = this;
                tile.cursor = cursor;
                tile.transform.parent = grid;
                var tilePosition = new TilePosition();
                tilePosition.x = j;
                tilePosition.y = i;
                tile.transform.localPosition = TilePositionToLocalPosition(tilePosition, gridSize);
                tile.position = new TilePosition();
                tile.position.x = j;
                tile.position.y = i;
                tile.FallFromSky(tile.transform.localPosition);
                tile.RandomizeTileValue();
                instantiatedTiles.Add(tile);
            }
        }
    }

    string[][] GetBoardStateFromTiles()
    {
        string[][] boardState = Extensions.Copy2DJaggedArray(blankBoardState);

        for (int i = 0; i < instantiatedTiles.Count; i++)
        {
            // [row][num in row]
            int currentRow = Mathf.FloorToInt((float)i / (float)gridSize);
            int currentNinRow = i - currentRow * gridSize;
            boardState[currentRow][currentNinRow] = instantiatedTiles[i].value;
        }

        return boardState;
    }

    void SetTilesToBoardStateWithAnimation(string[][] boardState, int[] indexesToMakeFall, Vector2[] destinations) 
    {
        for (int i = 0; i < boardState.Length; i++)
        {
            var rowState = boardState[i];
            for (int j = 0; j < rowState.Length; j++)
            {
                var tileState = rowState[j];
                var index = Extensions.GetIndexFromTilePosition(j, i, gridSize);
                if (indexesToMakeFall.Contains(index))
                {
                    var destination = destinations[Array.IndexOf(indexesToMakeFall, index)];
                    if(index < gridSize) // means this is the top row todo: check if this actually works
                        instantiatedTiles[index].FallFromSky(destination);
                    else
                        instantiatedTiles[index].Fall(destination);
                }
                else
                {
                    // do nothing? i think
                    // ill keep this here if i think of something, i feel like im missing something
                }
                //instantiatedTiles[i * gridSize + j].SetTileValue(tileState);
            }
        }

        // TODO: now sort the instantiated list by position, right to left, up to down, like reading
    }
    void SetTilesToBoardState(string[][] boardState)
    {
        for(int i = 0; i < boardState.Length; i++)
        {
            var rowState = boardState[i];
            for (int j = 0; j < rowState.Length; j++)
            {
                var tileState = rowState[j];
                instantiatedTiles[i * gridSize + j].SetTileValue(tileState);
            }
        }
    }

    public void ScrambleAllTiles()
    {
        foreach (var tile in instantiatedTiles)
        {
                tile.FallFromSky(tile.transform.localPosition);
            tile.RandomizeTileValue();
        }
    }

    public void DeselectAllTiles()
    {
        foreach (var tile in instantiatedTiles)
        {
            tile.selected = false;
        }
    }

    public void RemoveSelectedTiles()
    {
        var transientTiles = new List<Tile>();
        var transientTileIndexes = new List<int>();
        var destinationTileIndexes = new List<int>();
        for (int i = 0; i < instantiatedTiles.Count; i++) // does it matter if this is going up to down or down to up?
        {
            if (instantiatedTiles[i].selected)
            {
                transientTiles.Add(instantiatedTiles[i]);
                transientTileIndexes.Add(i);
            }
        }

        // randomize BEFORE the shift (and saving of the board state) because the tile location doesnt actually physically change to keep the list in order
        foreach (var tile in transientTiles)
        {
            tile.RandomizeTileValue();
        }

        var updatedBoardState = GetBoardStateFromTiles();

        foreach (var tile in transientTiles)
        {
            int destinationTileIndex = -1;
            updatedBoardState = ShiftColumnAndReinsertTile(instantiatedTiles.IndexOf(tile), updatedBoardState, out destinationTileIndex);
            destinationTileIndexes.Add(destinationTileIndex);
        }

        // get destinations

        // TODO: then animate this tile falling/update

        //SetTilesToBoardStateWithAnimation(updatedBoardState, transientTileIndexes.ToArray(), destinationTileIndexes.Select(x => TilePositionToLocalPosition(Extensions.GetTilePositionFromIndex(x, gridSize),gridSize)).ToArray()); // then convert via select linq and get pos from destination then the usual number figuring out

        SetTilesToBoardState(updatedBoardState);

        DeselectAllTiles();
    }

    public string[][] ShiftColumnAndReinsertTile(int tileIndex, string[][] boardState, out int newTileIndex)
    {
        newTileIndex = -1;
        var tilePosition = Extensions.GetTilePositionFromIndex(tileIndex, gridSize);
        var tileValue = boardState[tilePosition.y][tilePosition.x];

        var indexesToShift = new List<(int,int)>();

        for (int i = boardState.Length - 1; i >= 0; i--) // go backwards so as to move the bottom ones first by putting them at the top of the list
        {
            for (int j = boardState.Length - 1; j >= 0; j--)
            {
                if(j == tilePosition.x && i < tilePosition.y)
                {
                    indexesToShift.Add((j,i));
                }
            }
        }

        indexesToShift.Add((tilePosition.x, -1)); // add og to the top

        foreach (var index in indexesToShift)
        {
            if (index.Item2 == -1)
            {
                boardState[0][index.Item1] = tileValue;
                newTileIndex = Extensions.GetIndexFromTilePosition(0, index.Item1, gridSize);
            }
            else
            {
                boardState[index.Item2 + 1][index.Item1] = boardState[index.Item2][index.Item1];
            }
        }

        return boardState;
    }

    string GetRandomWord()
    {
        if (UnityEngine.Random.Range(0, 1) < chanceOfActualRandomLetter)
            return WeightedRandomLetterOfTheAlphabet();

        if (UnityEngine.Random.Range(0, 1) < chanceOfBonusWords)
            return database.GetRandomBonusWord();
        else
            return database.GetRandomValidWord();
    }

    // todo: fix as it seems to change the entire randomletterqueue every time, though still works decently
    public string QueuedRandomLetterOfTheAlphabet() // not all that random
    {
        while(randomLetterQueue.Length < randomLetterQueueMinLength)
        {
            randomLetterQueue += GetRandomWord();
        }

        string randomLetter = randomLetterQueue.Substring(0,1);
        if (randomLetter == "Q" && randomLetterQueue.Substring(0, 2) == "QU")
        {
            randomLetter = randomLetterQueue.Substring(0, 2);
            randomLetterQueue = randomLetterQueue.Substring(2, randomLetterQueue.Length - 3);
        }
        else
        {
            if (randomLetter == "Q")
                randomLetter = "QU";
            randomLetterQueue = randomLetterQueue.Substring(1, randomLetterQueue.Length - 2);
        }
        return randomLetter;
    }

    // just faffed about with the wikipedia letter frequency list
    
    public string WeightedRandomLetterOfTheAlphabet()
    {
        return weightedRandomAlphabet.Next();
    }
    void InitWeightedAlphabet()
    {
        List<WeightedListItem<string>> weightedAlphabet = new()
        {
            new WeightedListItem<string>("A", 12),
            new WeightedListItem<string>("B", 5),
            new WeightedListItem<string>("C", 5),
            new WeightedListItem<string>("D", 6),
            new WeightedListItem<string>("E", 12),
            new WeightedListItem<string>("F", 5),
            new WeightedListItem<string>("G", 5),
            new WeightedListItem<string>("H", 7),
            new WeightedListItem<string>("I", 10),
            new WeightedListItem<string>("J", 3),
            new WeightedListItem<string>("K", 3),
            new WeightedListItem<string>("L", 5),
            new WeightedListItem<string>("M", 5),
            new WeightedListItem<string>("N", 5),
            new WeightedListItem<string>("O", 10),
            new WeightedListItem<string>("P", 7),
            new WeightedListItem<string>("QU", 2),
            new WeightedListItem<string>("R", 7),
            new WeightedListItem<string>("S", 7),
            new WeightedListItem<string>("T", 9),
            new WeightedListItem<string>("U", 10),
            new WeightedListItem<string>("V", 2),
            new WeightedListItem<string>("W", 5),
            new WeightedListItem<string>("X", 2),
            new WeightedListItem<string>("Y", 3),
            new WeightedListItem<string>("Z", 2),
        };
        weightedRandomAlphabet = new(weightedAlphabet);
    }
}
