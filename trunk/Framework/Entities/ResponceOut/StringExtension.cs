using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Entities
{
    public static class StringExtension
    {
        public static bool IsValidJson(this string text)
        {
            text = text.Trim();
            if ((text.StartsWith("{") && text.EndsWith("}")) || //For object
                (text.StartsWith("[") && text.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(text);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    Console.Write(jex.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
