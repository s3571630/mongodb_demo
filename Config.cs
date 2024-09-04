using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Mongo
{
    public class Config
    {
        private static string RootPath;

        IConfiguration config;

        private Appsettings _appsettings { get; set; }
        public Appsettings appSettings => _appsettings;


        public Config(string basePath = null!)
        {
            RootPath = basePath ?? AppContext.BaseDirectory;

            _appsettings = (Appsettings)BuildNewSetting("AppSetting");

            if (!File.Exists(appSettings.SettingFileName)) Save<Appsettings>(appSettings);

            config = new ConfigurationBuilder()
                .SetBasePath(RootPath)
                .AddJsonFile(_appsettings.SettingFilePath, optional: false, reloadOnChange: true)
                .Build();

            _appsettings = config.Get<Appsettings>() ?? _appsettings;
        }

        public IISConfig BuildNewSetting(string type)
        {
            switch (type)
            {
                case "AppSetting":
                    return new Appsettings();
                default:
                    throw new ArgumentException("Setting Not Found");
            }
        }

        public void Save<T>(IISConfig settings)
        {
            File.WriteAllText(settings.SettingFileName,
                JsonSerializer.Serialize((T)settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                }));
        }


        public interface IISConfig
        {
            [JsonIgnore]
            public string SettingFileName { get; }
            [JsonIgnore]
            public string SettingFilePath { get; }
        }

        #region Appsettings 實體類別及參數設定

        public class Appsettings : IISConfig
        {
            [JsonIgnore]
            public string SettingFileName => "appsettings.json";
            [JsonIgnore]
            public string SettingFilePath => Path.Combine(RootPath, SettingFileName) ?? "";


            public Mongo Mongo { get; set; } = new Mongo();
            //public Redis Redis { get; set; } = new Redis();
            //public Oracle Oracle { get; set; } = new Oracle();
            //public SQLServer SQLServer { get; set; } = new SQLServer();
        }
        public class Mongo
        {
            public string ConnectionString { get; set; } = "";
        }

        //public class Redis
        //{
        //    public string RedisConnectionString { get; set; } = "";

        //    public string DefaultKey { get; set; } = "";
        //}
        //public class Oracle
        //{
        //    public string ConnectionString { get; set; } = "";
        //}
        //public class SQLServer
        //{
        //    public string ConnectionString { get; set; } = "";
        //}


        #endregion
    }

}