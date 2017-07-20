using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ExampleProject
{
    public static class JSonPrettyfierExtension
    {
        public static string ToPrettyJsonFromBase64(this string str)
        {
            //IdentityServer strips the trailing "=" which we need to fix to decodes it using Convert.FromBase64String(). It must be divisble by 4...
            var base64Str = str.PadRight(str.Length + (str.Length % 4), '=');

            //Now lets decode and display in pretty formattet JSON...
            byte[] decodedBytes = Convert.FromBase64String(base64Str);
            string decodedTxt = System.Text.Encoding.UTF8.GetString(decodedBytes);

            return ToPrettyJson(decodedTxt);
        }
        
        public static string ToPrettyJson(this string str)
        {
            return JsonConvert.SerializeObject(JsonConvert.DeserializeObject(str), Formatting.Indented);
        }
    }
}
