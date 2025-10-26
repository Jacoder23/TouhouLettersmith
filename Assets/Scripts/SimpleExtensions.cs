using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// borrowing from my 2023 self - 2025
namespace jcdr
{
    public static class SimpleExtensions
    {
        // https://stackoverflow.com/questions/11743160/how-do-i-encode-and-decode-a-base64-string
        public static string Base64Encode(string plainText)
        {
            if(plainText == null)
                return null;

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            if (base64EncodedData == null)
                return null;

            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        // https://stackoverflow.com/questions/2729192/c-rotating-2d-arrays
        public static object[][] ArrayRotate(object[][] input)
        {
            int length = input[0].Length;
            object[][] retVal = new object[length][];
            for (int x = 0; x < length; x++)
            {
                retVal[x] = input.Select(p => p[x]).ToArray();
            }
            return retVal;
        }

        //https://stackoverflow.com/questions/333737/evaluating-string-342-yield-int-18
        public static double Evaluate(string expression)
        {
            var loDataTable = new DataTable();
            var loDataColumn = new DataColumn("Eval", typeof(double), expression);
            loDataTable.Columns.Add(loDataColumn);
            loDataTable.Rows.Add(0);
            return (double)(loDataTable.Rows[0]["Eval"]);
        }

        // https://stackoverflow.com/questions/16100/convert-a-string-to-an-enum-in-c-sharp
        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        //https://stackoverflow.com/questions/3932413/is-there-a-shorter-simpler-version-of-the-for-loop-to-anything-x-times
        public static void Times(this int count, Action<int> action)
        {
            for (int i = 0; i < count; i++)
                action(i);
        }

        // https://stackoverflow.com/questions/1206019/converting-string-to-title-case
        public static string ToTitleCase(this string s) =>
        CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLower());

        // https://stackoverflow.com/questions/3875312/clever-way-to-append-s-for-plural-form-in-net-syntactic-sugar
        /// <summary>
        /// Pluralize: takes a word, inserts a number in front, and makes the word plural if the number is not exactly 1.
        /// </summary>
        /// <example>"{n.Pluralize("maid")} a-milking</example>
        /// <param name="word">The word to make plural</param>
        /// <param name="number">The number of objects</param>
        /// <param name="pluralSuffix">An optional suffix; "s" is the default.</param>
        /// <param name="singularSuffix">An optional suffix if the count is 1; "" is the default.</param>
        /// <returns>Formatted string: "number word[suffix]", pluralSuffix (default "s") only added if the number is not 1, otherwise singularSuffix (default "") added</returns>
        internal static string Pluralize(this int number, string word, string pluralSuffix = "s", string singularSuffix = "")
        {
            return $@"{number} {word}{(number != 1 ? pluralSuffix : singularSuffix)}";
        }

        // https://stackoverflow.com/questions/943635/how-do-i-clone-a-range-of-array-elements-to-a-new-array?answertab=scoredesc#tab-top
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        // https://stackoverflow.com/questions/2714639/calculating-weighted-average-with-linq
        public static double WeightedAverage<T>(this IEnumerable<T> records, Func<T, double> value, Func<T, double> weight)
        {
            if (records == null)
                throw new ArgumentNullException(nameof(records), $"{nameof(records)} is null.");

            int count = 0;
            double valueSum = 0;
            double weightSum = 0;

            foreach (var record in records)
            {
                count++;
                double recordWeight = weight(record);

                valueSum += value(record) * recordWeight;
                weightSum += recordWeight;
            }

            if (count == 0)
            {
                return 0;
            }
                //throw new ArgumentException($"{nameof(records)} is empty.");

            if (count == 1)
                return value(records.Single());

            if (weightSum != 0)
                return valueSum / weightSum;
            else
            {
                return valueSum;
            }
                //throw new DivideByZeroException($"Division of {valueSum} by zero.");
        }
    }
}