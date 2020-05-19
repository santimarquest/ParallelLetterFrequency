using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public static class ParallelLetterFrequency
{
    public static Dictionary<char, int> Calculate(IEnumerable<string> texts)
    {
      var result = new List<Task<Dictionary<char, int>>>();
      
        foreach (var text in texts)
        {
            result.Add(CalculateFrequency(text.Trim()));
        }

        var dictionaries = Task.WhenAll(result);
        return MergeDictionary(dictionaries.Result);
    }


    public static Dictionary<char, int> Calculate2(IEnumerable<string> texts)
    {
        var result = new Dictionary<char, int>();
        var partialResult = new List<Dictionary<char, int>>();
        var textResult = new Dictionary<char, int>();

        var observableTexts = texts.ToObservable();

        observableTexts.Subscribe
           (
               text => {
                   textResult = CalculateFrequency(text.Trim()).Result;
                   partialResult.Add(textResult);
                   },
               ex => Console.WriteLine("OnError: {0}", ex),
               () => result = MergeDictionary(partialResult.ToArray())
           );

        return result;
    }

    private static Dictionary<char, int> MergeDictionary(Dictionary<char, int>[] dictionaries)
    {
        var dictionaryResult = new Dictionary<char, int>();
        foreach (var dictionary in dictionaries)
        {
            foreach (var item in dictionary)
            {
                var lower = char.ToLower(item.Key);
                if (!dictionaryResult.ContainsKey(lower))
                {
                    dictionaryResult.Add(lower, item.Value);
                }
                else
                {
                    dictionaryResult[lower] += item.Value;
                }
            }
           
        }

        return dictionaryResult;
    }

    private static async Task<Dictionary<char, int>> CalculateFrequency(string text)
    {

        var t = await Task.Factory.StartNew(() =>
        {
            return CalculateFrequencyPerText(text);
        });
        return t;
    }

    private static Dictionary<char, int> CalculateFrequencyPerText(string text)
    {
        var lineResult = new Dictionary<char, int>();

        foreach (var c in text)
        {
            if (Char.IsLetter(c))
            {
                var lower = Char.ToLower(c);
                if (!lineResult.ContainsKey(lower))
                {
                    lineResult.Add(lower, 1);
                }
                else
                {
                    lineResult[lower]++;
                }
            }
        }

        return lineResult;
    }
}