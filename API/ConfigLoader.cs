using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;

namespace AtlantaSecurity.API
{
    internal static class ConfigLoader
    {
        public static string LoadDeepLApiKey()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "AtlantaSecurity.dbconfig.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException("Configuration file not found.", resourceName);
                }

                using (StreamReader reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    var config = JObject.Parse(json);
                    return config["DeepLApiKey"].ToString();
                }
            }
        }
    }
}
