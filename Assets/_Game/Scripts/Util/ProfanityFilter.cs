using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class ProfanityFilter
{
    public const string profanityListPath = "Assets/_Game/Resources/ProfanityList.txt";

    public const string replacement = "$#%!";

    public static string ReplaceProfanity(string message)
    {
        string[] profanityWords = System.IO.File.ReadAllLines(profanityListPath);
        string result = message;

        foreach (var profanity in profanityWords)
        {
            result = Regex.Replace(result, profanity, replacement, RegexOptions.IgnoreCase);
        }

        return result;
    }
}
