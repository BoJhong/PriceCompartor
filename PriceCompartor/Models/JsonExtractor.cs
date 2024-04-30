using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace PriceCompartor.Models
{
    public class JsonExtractor
    {
        public static List<T>? ExtractJson<T>(string textResponse)
        {
            // This pattern matches a string that starts with '{' and ends with '}'
            string pattern = @"\{[^{}]*\}";
            List<T> jsonObjects = new List<T>();

            MatchCollection matches = Regex.Matches(textResponse, pattern);
            foreach (Match match in matches)
            {
                string jsonString = match.Groups[0].Value;
                try
                {
                    // Validate if the extracted string is valid JSON
                    T jsonObj = JsonConvert.DeserializeObject<T>(jsonString)!;
                    jsonObjects.Add(jsonObj);
                }
                catch (JsonException)
                {
                    // Extend the search for nested structures
                    string extendedJsonString = ExtendSearch(textResponse, match.Index, match.Index + match.Length);
                    try
                    {
                        T? jsonObj = JsonConvert.DeserializeObject<T>(extendedJsonString);
                        if (jsonObj != null)
                        {
                            jsonObjects.Add(jsonObj);
                        }
                    }
                    catch (JsonException)
                    {
                        // Handle cases where the extraction is not valid JSON
                        continue;
                    }
                }
            }

            if (jsonObjects.Count > 0)
            {
                return jsonObjects;
            }
            else
            {
                return null; // Or handle this case as you prefer
            }
        }

        private static string ExtendSearch(string text, int startIndex, int endIndex)
        {
            // Extend the search to try to capture nested structures
            int nestCount = 0;
            for (int i = startIndex; i < text.Length; i++)
            {
                if (text[i] == '{')
                {
                    nestCount++;
                }
                else if (text[i] == '}')
                {
                    nestCount--;
                    if (nestCount == 0)
                    {
                        return text.Substring(startIndex, i - startIndex + 1);
                    }
                }
            }
            return text.Substring(startIndex, endIndex - startIndex);
        }
    }
}
