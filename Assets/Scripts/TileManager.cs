using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KaimiraGames;
using jcdr;
using JSAM;

public class TileManager : MonoBehaviour
{
    public Tile tilePrefab;
    public Transform grid;
    public Cursor cursor;
    public WordDatabase database;
    public ToggleFont fontSettings;
    public LetterVerification letter;
    public LevelDatabase level;
    public OnLoss loss;

    [Header("Settings")]
    public bool titleScreen = false;
    public int gridSize;
    public float spaceBetweenTiles;
    public int randomLetterQueueMinLength = 20;
    public int maxRainbowTiles = 2;

    [Header("Word Spawn Rates")]
    public float chanceOfAnyGoalWord = 0.2f;
    public float chanceOfBonusWords = 0.1f;
    public float chanceOfCurrentGoalWord = 0.5f;
    public float chanceOfNextGoalWord = 0.5f;
    public float chanceOfActualRandomLetter = 0.5f;
    // otherwise random word

    [Header("Word Spawn Rates (SCRAMBLE)")]
    public float chanceOfAnyGoalWordScramble = 0.2f;
    public float chanceOfBonusWordsScramble = 0.1f;
    public float chanceOfCurrentGoalWordScramble = 0.5f;
    public float chanceOfNextGoalWordScramble = 0.5f;
    public float chanceOfActualRandomLetterScramble = 0.5f;

    [Header("Tile Spawn Rates")] // changes per level
    public float chanceOfFireTile = 0.03f;
    public float chanceOfBombTile = 0.01f;
    public float chanceOfStoneTile = 0.01f;
    public float chanceOfDrunkenTile = 0.02f;

    [Header("Preview")]
    public string randomLetterQueue = "";

    public List<Tile> instantiatedTiles;
    public int rainbowTileSpawnQueue;
    string[][] currentBoardState; // encode in each string extra data like what about the other things
    // format: X010101 with each 0/1 showing if it's a special tile type or not, theoretically allows combinations of tile types but that's out of scope
    
    string[][] blankBoardState;
    WeightedList<string> weightedRandomAlphabet;

    Vector2 offset;
    bool nextTileRainbow;
    void Start()
    {
        if (titleScreen)
            return;

        AudioManager.StopAllMusic();
        AudioManager.StopAllSounds();

        //AudioManager.PlayMusic(LibraryMusic.KogasaTheme);

        chanceOfFireTile = level.GetCurrentLevel().chanceOfFireTile;
        chanceOfBombTile = level.GetCurrentLevel().chanceOfBombTile;
        chanceOfStoneTile = level.GetCurrentLevel().chanceOfStoneTile;
        chanceOfDrunkenTile = level.GetCurrentLevel().chanceOfDrunkenTile;

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

        InitialBoardState();
    }

    void InitialBoardState()
    {
        // idk whats going on here, this was never an issue before
        if(database.validWords.Count == 0)
        {
            Invoke("InitialBoardState", 0.1f);
            return;
        }

        PopulateTileGrid();

        currentBoardState = GetBoardStateFromTiles();

        Extensions.DebugLog2DJaggedArray(currentBoardState);
    }
    bool lost = false;
    void Update()
    {
        if (database == null)
            database = FindFirstObjectByType<WordDatabase>(); // strangely doesn't work in Start

        if (instantiatedTiles.Count > 0)
        {
            if (instantiatedTiles.TrueForAll(x => x.type == TileType.Fire || x.type == TileType.Stone) && !lost)
            {
                // lose
                loss.Lose();
                lost = true;
            }
        }
    }

    Vector2 TilePositionToLocalPosition(TilePosition pos, int gridSize)
    {
        return new Vector2(pos.x * spaceBetweenTiles, (float)(gridSize - pos.y) * spaceBetweenTiles) - offset;
    }

    // todo: post-jam v1.1.0
    // scramble using random path
    // slower fire spread
    // fix beer activating fire
    // item tutorial
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
                var tilePosition = new TilePosition(j,i);
                tile.transform.localPosition = TilePositionToLocalPosition(tilePosition, gridSize);
                tile.position = new TilePosition(j,i);
                tile.SetTileValue(".");
                // todo: logic for tile type selection
                tile.ChangeTileType(TileType.Normal);
                instantiatedTiles.Add(tile);
            }
        }

        ScrambleAllTiles();

        fontSettings.UpdateFonts();
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

    // todo: convert to a pre calc lookup table

    public List<TilePosition> GetAllValidTiles(TilePosition origin)
    {
        List<TilePosition> validTiles = new List<TilePosition>();
        //Debug.Log("Origin: " + origin);
        for(int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                if (new TilePosition(j, i).Equals(origin))
                    continue;

                if(Extensions.ValidTileDestination(origin, new TilePosition(j,i), gridSize))
                {
                    validTiles.Add(new TilePosition(j,i));
                    //Debug.Log(new TilePosition(j, i));
                }
            }
        }
        return validTiles;
    }

    public void ScrambleAllTiles()
    {
        foreach (var tile in instantiatedTiles)
            tile.SetTileValue(".");

        // random walk
        var scrambledTiles = new List<Tile>();

        Tile origin = instantiatedTiles[UnityEngine.Random.Range(0, instantiatedTiles.Count - 1)];
        origin.SetTileValue(QueuedRandomLetterOfTheAlphabet(true));
        scrambledTiles.Add(origin);
        //Debug.Log("Origin: " + origin.position + " (" + origin.value + ")");

        for (int i = 0; i < instantiatedTiles.Count - 1; i++)
        {
            var validTiles = GetAllValidTiles(origin.position);
            var nonScrambledTiles = instantiatedTiles.Except(scrambledTiles).Select(x => x.position).ToList();
            validTiles = validTiles.Intersect(nonScrambledTiles).ToList();
            if (validTiles.Count > 0)
            {
                // select random tile
                TilePosition originTilePosition = validTiles[UnityEngine.Random.Range(0, validTiles.Count - 1)];
                origin = instantiatedTiles[Extensions.GetIndexFromTilePosition(originTilePosition.x, originTilePosition.y, gridSize)];
                origin.SetTileValue(QueuedRandomLetterOfTheAlphabet(true));
                scrambledTiles.Add(origin);
                //Debug.Log("Destination: " + origin.position + " (" + origin.value + ")");
            }
            else
            {
                //Debug.Log("No non-scrambled valid tiles");
                TilePosition originTilePosition = nonScrambledTiles[UnityEngine.Random.Range(0, nonScrambledTiles.Count() - 1)]; // get a random tile that has yet to be scrambled
                origin = instantiatedTiles[Extensions.GetIndexFromTilePosition(originTilePosition.x, originTilePosition.y, gridSize)];
                origin.SetTileValue(QueuedRandomLetterOfTheAlphabet(true));
                scrambledTiles.Add(origin);
                //Debug.Log("Origin: " + origin.position + " (" + origin.value + ")");
            }
        }

        DeselectAllTiles();
    }

    public void DeselectAllTiles()
    {
        foreach (var tile in instantiatedTiles)
        {
            tile.selected = false;
        }
    }

    void UpdateFireTiles()
    {
        // only change the type to fire AFTER we've selected them 
        var newFireTiles = new List<Tile>();
        foreach (var tile in instantiatedTiles.Where(x => !x.newTile))
        {
            if (tile.type == TileType.Fire)
            {
                var tilesToBurn = FirePattern(tile.position);
                if (tilesToBurn != null)
                {
                    var tileOrder = new int[tilesToBurn.Count()];

                    for (int i = 0; i < tilesToBurn.Count(); i++)
                    {
                        tileOrder[i] = i;
                    }

                    SimpleExtensions.Shuffle(tileOrder);

                    // burn up to two random tiles
                    for (int i = 0; i < tilesToBurn.Count; i++)
                    {
                        if (i == 2) // 2 already burnt
                            break;

                        TilePosition t = tilesToBurn[tileOrder[i]];
                        var tileToBurn = instantiatedTiles[Extensions.GetIndexFromTilePosition(t.x, t.y, gridSize)];

                        if (tileToBurn.type == TileType.Bomb)
                            ActivateBomb(tileToBurn.position);
                        else
                            newFireTiles.Add(tileToBurn);
                    }
                }
            }
        }

        foreach(var tile in newFireTiles)
        {
            AudioManager.PlaySound(LibrarySounds.Burn);
            tile.ChangeTileType(TileType.Fire); // a delay would help w the effect here
        }
    }
    // currently unused
    public void UpdateSpecialTiles()
    {
        foreach(var tile in instantiatedTiles)
        {
            switch (tile.type)
            {
                case TileType.Normal:
                    break;
                case TileType.Rainbow:
                    break;
                case TileType.Fire:
                    break;
                case TileType.Bomb:
                    break;
                case TileType.Stone:
                    break;
                case TileType.Drunken:
                    break;
            }
        }
    }

    void ActivateBomb(TilePosition origin)
    {
        // play bomb sound
        AudioManager.PlaySound(LibrarySounds.Explosion);
        var tilesToBurn = FirePattern(origin);
        instantiatedTiles[Extensions.GetIndexFromTilePosition(origin.x, origin.y, gridSize)].ChangeTileType(TileType.Normal);
        if (tilesToBurn != null)
        {
            foreach (var t in tilesToBurn)
            {
                var tileToBurn = instantiatedTiles[Extensions.GetIndexFromTilePosition(t.x, t.y, gridSize)];

                if (tileToBurn.type == TileType.Stone)
                    tileToBurn.ChangeTileType(TileType.Normal);
                else if (tileToBurn.type == TileType.Bomb)
                    ActivateBomb(t);
                else
                    tileToBurn.ChangeTileType(TileType.Fire);
            }
        }
    }

    //List<TilePosition> BombPattern(TilePosition origin)
    //{
    //    return null;
    //}

    List<TilePosition> FirePattern(TilePosition origin)
    { // i miss bitboards

        if (origin == null)
            return null;
        else if (origin.x == -1 || origin.y == -1)
            return null;

        var destinations = new List<TilePosition>();
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                var destination = new TilePosition(j,i);
                bool withinRangeX = Mathf.Abs(destination.x - origin.x) < 2;
                bool withinRangeY = Mathf.Abs(destination.y - origin.y) < 2;
                bool diagonal = (destination.x + 1 == origin.x && destination.y + 1 == origin.y) || (destination.x - 1 == origin.x && destination.y + 1 == origin.y) || (destination.x + 1 == origin.x && destination.y - 1 == origin.y) || (destination.x - 1 == origin.x && destination.y - 1 == origin.y);

                if (withinRangeX && withinRangeY && !diagonal)
                {
                    destinations.Add(destination);
                }
            }
        }
        Debug.Log(destinations.Count());
        return destinations;
    }
    public Tile GetTileInstanceFromPosition(TilePosition position)
    {
        var instance = instantiatedTiles.Find(tile => tile.position.Equals(position));
        if(instance == null)
        {
            Debug.LogError("No tile found with position: (" + position.x + ", " + position.y + ")");
        }
        return instantiatedTiles.Find(tile => tile.position.Equals(position));
    }
    public TilePosition ShiftTilePositionDown(TilePosition position, int amountToMove)
    {
        position.y += amountToMove;
        return position;
    }

    public void RemoveSelectedTiles()
    {
        foreach (Tile tile in instantiatedTiles)
        {
            tile.newTile = false;
            if (tile.type == TileType.Bomb && tile.selected)
                ActivateBomb(tile.position);
        }

        var transientTiles = new List<Tile>();
        var transientTileIndexes = new List<int>();
        for (int i = 0; i < instantiatedTiles.Count; i++) // does it matter if this is going up to down or down to up?
        {
            if (instantiatedTiles[i].selected)
            {
                transientTiles.Add(instantiatedTiles[i]);
                transientTileIndexes.Add(i);
                instantiatedTiles[i].Fall(instantiatedTiles[i].transform.localPosition);
                instantiatedTiles[i].transform.localPosition = new Vector2(instantiatedTiles[i].transform.localPosition.x, instantiatedTiles[i].transform.localPosition.y + spaceBetweenTiles);
            }
        }

        // randomize BEFORE the shift (and saving of the board state) because the tile location doesnt actually physically change to keep the list in order
        foreach (var tile in transientTiles)
        {
            tile.RandomizeTileValue(); // doesn't use the scramble weights and so is better
        }

        var updatedBoardState = GetBoardStateFromTiles();

        var timesColumnsHaveBeenShifted = new int[gridSize];
        foreach (var tile in transientTiles)
        {
            // todo: i don't think tile positions includes the position of the original selected tile
            List<TilePosition> tilePositions = new List<TilePosition>(); // no need to record origin and destination, we only work one tile at a time so the origin is always one row above (unless its at the top so we use the other fall)
            updatedBoardState = ShiftColumnAndReinsertTile(instantiatedTiles.IndexOf(tile), updatedBoardState, out tilePositions);
            tile.selected = false; //IMPORTANT

            // if this particular column has been shifted 0 times before this shift then only label y 0 as new, shifted 1 before then up to y 1, etc.
            foreach (var movedTile in tilePositions)
            {
                //Debug.Log(movedTile.x + ", " + movedTile.y);
                var instance = GetTileInstanceFromPosition(movedTile);
                instance.Fall(TilePositionToLocalPosition(movedTile, gridSize));
                if (movedTile.y == timesColumnsHaveBeenShifted[tile.position.x]) // todo: find way of finding the "new" tiles without excluding the ones ending up below y 0
                {
                    instance.newTile = true;
                    instance.transform.localPosition = TilePositionToLocalPosition(ShiftTilePositionDown(movedTile, -gridSize), gridSize); // move down from off screen
                }
                else if(!instance.newTile)
                {
                    instance.transform.localPosition = TilePositionToLocalPosition(ShiftTilePositionDown(movedTile, -1), gridSize);
                }
                SetTilesToBoardState(updatedBoardState);
            }
            timesColumnsHaveBeenShifted[tile.position.x] += 1; // order is important, dont put before the check
        }

        //SetTilesToBoardState(updatedBoardState);

        DeselectAllTiles();

        // todo: add this back later if needed
        //UpdateSpecialTiles();

        Invoke("UpdateFireTiles", 1.5f); // unhardcode this

        var newTiles = instantiatedTiles.Where(x => x.newTile).ToList();
        var tileOrder = new int[newTiles.Count()];

        for (int i = 0; i < newTiles.Count(); i++)
        {
            tileOrder[i] = i;
        }

        SimpleExtensions.Shuffle(tileOrder);

        for (int i = 0; i < newTiles.Count(); i++)
        {
            // random order for the spawning
            newTiles[tileOrder[i]].ChangeTileType(GetNextRandomTileType());
        }

        // moved up so we could use the new tile setting to stop new fire tiles from immediately spreading
        //foreach (Tile tile in instantiatedTiles)
        //{
        //    tile.newTile = false;
        //}

        rainbowTileSpawnQueue = 0;
    }

    public string[][] ShiftColumnAndReinsertTile(int tileIndex, string[][] boardState, out List<TilePosition> tilePositions)
    {
        tilePositions = new List<TilePosition>();
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

        bool tileTypeChanged = false;
        
        if (instantiatedTiles[tileIndex].selected && tileIndex >= gridSize && instantiatedTiles[tileIndex].type == TileType.Normal)
        {
            var above = instantiatedTiles[tileIndex - gridSize];
            if (above.type != instantiatedTiles[tileIndex].type)
                tileTypeChanged = true;
        }

        foreach (var index in indexesToShift)
        {
            // if the og tileIndex ever gets its tile type changed then we need to know so we don't reset it back to being normal tile value
            // otherwise reset to use up the special tile
            TilePosition tempPos = new TilePosition(0,0);
            if (index.Item2 == -1)
            {
                boardState[0][index.Item1] = tileValue;
                tempPos.x = index.Item1;
                tempPos.y = 0;
            }
            else
            {
                var currentTile = instantiatedTiles[Extensions.GetIndexFromTilePosition(index.Item1, index.Item2, gridSize)];
                if (!currentTile.selected)
                {
                    if (currentTile.type != TileType.Normal)
                    {
                        instantiatedTiles[Extensions.GetIndexFromTilePosition(index.Item1, index.Item2 + 1, gridSize)].ChangeTileType(currentTile.type);
                    }
                    currentTile.ChangeTileType(instantiatedTiles[Extensions.GetIndexFromTilePosition(index.Item1, index.Item2 - 1, gridSize)].type);
                }
                else
                { // here's the issue, the tile type is reset to normal but its actually a rainbow tile falling in
                  // currently multiple in same column is fine only issue is back to the tile directly below
                  // multi and directly: removes the above
                  // multiple: fine
                  // directly below: fine
                  // selected special: fine
                  //instantiatedTiles[Extensions.GetIndexFromTilePosition(index.Item1, index.Item2 + 1, gridSize)].ChangeTileType(currentTile.type);
                  //this line is our culprit for the "multi directly below" issue
                  // BECAUSE IT WAS SELECTED FUCK, it must be on the second go around
                  // valdidated the second go around theory
                  // we need to deselect tiles inbetween shifts
                  // works now
                    currentTile.ChangeTileType(instantiatedTiles[Extensions.GetIndexFromTilePosition(index.Item1, index.Item2 - 1, gridSize)].type);
                }

                boardState[index.Item2 + 1][index.Item1] = boardState[index.Item2][index.Item1];
                tempPos.x = index.Item1;
                tempPos.y = index.Item2;
            }
            tilePositions.Add(tempPos);
        }
        instantiatedTiles[Extensions.GetIndexFromTilePosition(tilePosition.x, 0, gridSize)].ChangeTileType(TileType.Normal); // done last

        if (!tileTypeChanged)
            instantiatedTiles[tileIndex].ChangeTileType(TileType.Normal);

        return boardState;
    }

    // todo: add sounds when a tile type appears
    TileType GetNextRandomTileType()
    {
        if (level.GetCurrentLevel().tileTypesAvailable.Contains(TileType.Rainbow))
        {
            if (rainbowTileSpawnQueue > 0 && instantiatedTiles.Count(x => x.type == TileType.Rainbow) < maxRainbowTiles)
            {
                rainbowTileSpawnQueue--;
                return TileType.Rainbow;
            }
        }

        bool onlySpawnIfNoFireOnBoard =  instantiatedTiles.Count(x => x.type == TileType.Fire) == 0;

        if (UnityEngine.Random.Range(0.0f, 1.0f) < chanceOfFireTile && level.GetCurrentLevel().tileTypesAvailable.Contains(TileType.Fire) && onlySpawnIfNoFireOnBoard)
        {
            AudioManager.PlaySound(LibrarySounds.Burn);
            return TileType.Fire;
        }

        bool onlySpawnIfLittleFireOnBoard = instantiatedTiles.Count(x => x.type == TileType.Fire) < 5; // unhardcode this

        if (UnityEngine.Random.Range(0.0f, 1.0f) < chanceOfBombTile && level.GetCurrentLevel().tileTypesAvailable.Contains(TileType.Bomb) && onlySpawnIfLittleFireOnBoard)
            return TileType.Bomb;

        if (UnityEngine.Random.Range(0.0f, 1.0f) < chanceOfStoneTile && level.GetCurrentLevel().tileTypesAvailable.Contains(TileType.Stone))
            return TileType.Stone;

        if (UnityEngine.Random.Range(0.0f, 1.0f) < chanceOfDrunkenTile && level.GetCurrentLevel().tileTypesAvailable.Contains(TileType.Drunken))
            return TileType.Drunken;

        return TileType.Normal;
    }

    // todo: this would be much improved if we could evaluate from a board if a word is possible to form but that'd require a lot of work
    string GetRandomWord(bool scramble)
    {
        if (scramble)
        {
            if (UnityEngine.Random.Range(0f, 1f) < chanceOfAnyGoalWordScramble * (1f - (letter.nextWord.Length * 2f) / 100f)) // reduced by N*2 % where N is the length of the current goal word because ts gets impossible at long goal words
                return letter.letter[UnityEngine.Random.Range(0, letter.letter.Count)];
            else if (UnityEngine.Random.Range(0f, 1f) < chanceOfBonusWordsScramble * (1f - (letter.nextWord.Length) * 3f / 100f)) // reduced by N*3 % where N is the length of the current goal word because ts gets impossible at long goal words
                return database.GetRandomBonusWord();
            else if (UnityEngine.Random.Range(0f, 1f) < chanceOfCurrentGoalWordScramble * (1f + (letter.nextWord.Length * 7f) / 100f)) // increased by N*7 % where N is the length of the current goal word because ts gets impossible at long goal words
                return letter.nextWord;
            else if (UnityEngine.Random.Range(0f, 1f) < chanceOfNextGoalWordScramble * (1f + (letter.nextWord.Length * 3f) / 100f)) // increased by N*3 % where N is the length of the current goal word because ts gets impossible at long goal words
                return letter.GetNextGoalWord();
            else
                return database.GetRandomValidWord();
        }
        else
        {
            if (UnityEngine.Random.Range(0f, 1f) < chanceOfAnyGoalWord * (1f - (letter.nextWord.Length * 2f) / 100f)) // reduced by N*2 % where N is the length of the current goal word because ts gets impossible at long goal words
                return letter.letter[UnityEngine.Random.Range(0, letter.letter.Count)];
            else if (UnityEngine.Random.Range(0f, 1f) < chanceOfBonusWords * (1f - (letter.nextWord.Length) * 3f / 100f)) // reduced by N*3 % where N is the length of the current goal word because ts gets impossible at long goal words
                return database.GetRandomBonusWord();
            else if (UnityEngine.Random.Range(0f, 1f) < chanceOfCurrentGoalWord * (1f + (letter.nextWord.Length * 7f) / 100f)) // increased by N*7 % where N is the length of the current goal word because ts gets impossible at long goal words
                return letter.nextWord;
            else if (UnityEngine.Random.Range(0f, 1f) < chanceOfNextGoalWord * (1f + (letter.nextWord.Length * 3f) / 100f)) // increased by N*3 % where N is the length of the current goal word because ts gets impossible at long goal words
                return letter.GetNextGoalWord();
            else
                return database.GetRandomValidWord();
        }
    }
    public string QueuedRandomLetterOfTheAlphabet(bool scramble = false) // not all that random
    {
        while (randomLetterQueue.Length < randomLetterQueueMinLength)
        {
            randomLetterQueue += GetRandomWord(scramble);
        }

        if (UnityEngine.Random.Range(0f, 1f) < chanceOfActualRandomLetter * (1f - (letter.nextWord.Length * 15f)/100f)) // reduced by N*15% where N is the length of the current goal word because ts gets impossible at long goal words
            return WeightedRandomLetterOfTheAlphabet(); // moved here so it can show up inside of words instead of just inbetween them

        string randomLetter = randomLetterQueue.Substring(0,1);
        if (randomLetter == "Q" && randomLetterQueue.Substring(0, 2) == "QU")
        {
            randomLetter = randomLetterQueue.Substring(0, 2);
            randomLetterQueue = randomLetterQueue.Substring(2, randomLetterQueue.Length - 2);
        }
        else
        {
            if (randomLetter == "Q")
                randomLetter = "QU";
            randomLetterQueue = randomLetterQueue.Substring(1, randomLetterQueue.Length - 1);
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
            new WeightedListItem<string>("QU", 1),
            new WeightedListItem<string>("R", 7),
            new WeightedListItem<string>("S", 7),
            new WeightedListItem<string>("T", 9),
            new WeightedListItem<string>("U", 10),
            new WeightedListItem<string>("V", 2),
            new WeightedListItem<string>("W", 5),
            new WeightedListItem<string>("X", 2),
            new WeightedListItem<string>("Y", 3),
            new WeightedListItem<string>("Z", 1),
        };
        weightedRandomAlphabet = new(weightedAlphabet);
    }
}
