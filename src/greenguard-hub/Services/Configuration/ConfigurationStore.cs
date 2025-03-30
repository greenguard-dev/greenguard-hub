using nanoFramework.Json;
using System.IO;
using System.Text;

namespace greenguard_hub.Services.Configuration
{
    public class ConfigurationStore
    {
        private const string ConfigFile = "I:\\configuration.json";
        public bool IsConfigFileExisting => File.Exists(ConfigFile);

        public void ClearConfig()
        {
            if (File.Exists(ConfigFile))
            {
                File.Delete(ConfigFile);
            }
        }

        public Configuration GetConfig()
        {
            var json = new FileStream(ConfigFile, FileMode.Open);
            Configuration config = (Configuration)JsonConvert.DeserializeObject(json, typeof(Configuration));

            return config;
        }

        public bool WriteConfig(Configuration config)
        {
            try
            {
                var configJson = JsonConvert.SerializeObject(config);

                var json = new FileStream(ConfigFile, FileMode.Create);

                byte[] buffer = Encoding.UTF8.GetBytes(configJson);
                json.Write(buffer, 0, buffer.Length);
                json.Dispose();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
