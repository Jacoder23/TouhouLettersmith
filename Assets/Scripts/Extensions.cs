using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public static class Extensions
{
    // TODO: figure out one with a combined QU
    const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    // TODO: Make a version that just scrambles an alphabet, so that we can insert valid words into the mix.
    // And put weight on words other than XYZ and other uncommon letters, maybe base it off of a frequency list
    public static string RandomLetterOfTheAlphabet()
    {
        int random = Random.Range(0, 25);
        return alphabet.Substring(random, 1);
    }

    public static void DebugLog2DJaggedArray(string[][] array)
    {
        string output = "";
        for (int i = 0; i < array.Length; i++)
        {
            string[] row = array[i];
            output += string.Join(' ', row) + "\n";
        }
    }

    //https://stackoverflow.com/q/4670720
    public static string[][] Copy2DJaggedArray(this string[][] source)
    {
        string[][] destination = new string[source.Length][];
        // For each Row
        for (int y = 0; y < source.Length; y++)
        {
            // Initialize Array
            destination[y] = new string[source[y].Length];
            // For each Column
            for (int x = 0; x < destination[y].Length; x++)
            {
                destination[y][x] = source[y][x];
            }
        }
        return destination;
    }
}
