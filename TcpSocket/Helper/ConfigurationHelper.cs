using System.IO;
using System.Text;
using Helper.Statics;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TcpSocket.Helper
{
    public class ConfigurationHelper
    {
        private const string JsonFile = "appsettings.json";

        private static readonly string CurrentPath = Path.Combine(AppStatics.ExeDirectory, JsonFile);


        private const string BusinessConfig = "BusinessConfig";
        

        private const string DEFAULT_THEME_URI = "DefaultThemeURI";
        private const string IMAGE_DIR = "ImageDir";

        private const string Is_LOGGING = "IsLogging";

        private const string TRUE = "True";

        private IConfigurationRoot _configurationRoot;

        private ConfigurationHelper()
        {
            _configurationRoot = new ConfigurationBuilder()
                .SetBasePath(AppStatics.ExeDirectory)
                .AddJsonFile(JsonFile, optional: false, true)
                .Build();
        }

        private static ConfigurationHelper _instance = new ConfigurationHelper();

        public static ConfigurationHelper Instance => _instance;

        #region Public Method

        public string GetBusinessConfig(string key) => _configurationRoot.GetSection($"{BusinessConfig}:{key}").Value!;

        public JObject GetJObject()
        {
            var jsonString = File.ReadAllText(CurrentPath, Encoding.UTF8);
            var jsonObject = JObject.Parse(jsonString);

            return jsonObject;
        }

        public void WriteJsonToFile(JObject jObject)
        {
            var convertString = JsonConvert.SerializeObject(jObject, Formatting.Indented);
            File.WriteAllText(CurrentPath, convertString);

            string originalPath = string.Empty;
            var dirInfo = Directory.GetParent(AppStatics.ExeDirectory);
            if (dirInfo != null)
            {
                if ((dirInfo = dirInfo.Parent) != null)
                {
                    if ((dirInfo = dirInfo.Parent) != null)
                    {
                        originalPath = Path.Combine(dirInfo.ToString(), JsonFile);
                    }
                }
            }

            if (!string.IsNullOrEmpty(originalPath) && File.Exists(originalPath))
            {
                File.WriteAllText(originalPath, convertString);
            }
        }

        #endregion

        #region 读配置

        public bool IsOnlyOne()
        {
            return Helper.Equals(TRUE, this.GetBusinessConfig(Constants.ONLY_ONE_PROCESS));
        }

        public bool IsAutoStart()
        {
            return Helper.Equals(TRUE, this.GetBusinessConfig(Constants.AUTO_START));
        }
        
        public bool IsBackgroundSwitch()
        {
            return Helper.Equals(TRUE, this.GetBusinessConfig(Constants.BACKGROUND_SWITCH));
        }

        public bool IsLogging(string name)
        {
            return Helper.Equals(TRUE, this.GetBusinessConfig($"{Is_LOGGING}:{name}"));
        }

        public string GetDefaultThemeURI()
        {
            return this.GetBusinessConfig(DEFAULT_THEME_URI);
        }

        public string GetImageDir()
        {
            return this.GetBusinessConfig(IMAGE_DIR);
        }

        #endregion

        #region 写配置

        public void SetOnlyOne(JObject jsonObject, bool value)
        {
            jsonObject[BusinessConfig][Constants.ONLY_ONE_PROCESS] = value;
        }

        public void SetAutoStart(JObject jsonObject, bool value)
        {
            jsonObject[BusinessConfig][Constants.AUTO_START] = value;
        }
        
        public void SetBackgroundSwitch(JObject jsonObject, bool value)
        {
            jsonObject[BusinessConfig][Constants.BACKGROUND_SWITCH] = value;
        }

        public void SetLogging(JObject jsonObject, string name, bool value)
        {
            jsonObject[BusinessConfig][Is_LOGGING][name] = value;
        }


        public void SetDefaultThemeURI(JObject jsonObject, string uri)
        {
            if (!string.IsNullOrEmpty(uri))
            {
                jsonObject[BusinessConfig][DEFAULT_THEME_URI] = uri;
            }
        }

        public void SetImageURI(JObject jsonObject, string uri)
        {
            if (!string.IsNullOrEmpty(uri))
            {
                jsonObject[BusinessConfig][IMAGE_DIR] = uri;
            }
        }

        #endregion
    }
}