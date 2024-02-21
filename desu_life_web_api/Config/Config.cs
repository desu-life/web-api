using System.IO;
using WebAPI.Serializer;
using LanguageExt.UnitsOfMeasure;
using Tomlyn.Model;

namespace WebAPI
{
    public class Config
    {
        private static Base? inner;

        public static Base? Inner
        {
            get => inner;
            set
            {
                inner ??= value;
            }
        }

        public class API : ITomlMetadataProvider
        {
            public string? ApiHost { get; set; }
            public int ApiPort { get; set; }
            public bool UseSSL { get; set; }
            TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
        }

        public class Mail : ITomlMetadataProvider
        {
            public string? SmtpHost { get; set; }
            public int SmtpPort { get; set; }
            public string? UserName { get; set; }
            public string? PassWord { get; set; }
            TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
        }

        public class Database : ITomlMetadataProvider
        {
            public string? Type { get; set; }
            public string? Host { get; set; }
            public int Port { get; set; }
            public string? DB { get; set; }
            public string? User { get; set; }
            public string? Password { get; set; }
            TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
        }

        public class OSU : ITomlMetadataProvider
        {
            public int ClientId { get; set; }
            public string? ClientSecret { get; set; }
            public string? AuthorizeUrl { get; set; }
            public string? RedirectUrl { get; set; }
            public string? APIBaseUrl { get; set; }
            public string? TokenUrl { get; set; }
            TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
        }

        public class Discord : ITomlMetadataProvider
        {
            public string? ClientId { get; set; }
            public string? ClientSecret { get; set; }
            public string? AuthorizeUrl { get; set; }
            public string? RedirectUrl { get; set; }
            public string? APIBaseUrl { get; set; }
            public string? TokenUrl { get; set; }
            TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
        }

        public class Base : ITomlMetadataProvider
        {
            public bool Dev { get; set; }
            public API? Api { get; set; }
            public OSU? Osu { get; set; }
            public Database? Database { get; set; }
            public Mail? Mail { get; set; }
            public Discord? Discord { get; set; }

            TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
            public static Base Default()
            {
                return new Base()
                {
                    Dev = true,
                    Api = new()
                    {
                        ApiHost = "localhost",
                        ApiPort = 5500,
                        UseSSL = false
                    },
                    Osu = new()
                    {
                        ClientId = 0,
                        ClientSecret = "",
                        TokenUrl = "",
                        APIBaseUrl = "",
                        RedirectUrl = "",
                        AuthorizeUrl = ""
                    },
                    Database = new()
                    {
                        Type = "mysql",
                        Host = "",
                        Port = 3306,
                        DB = "kanonbot",
                        User = "",
                        Password = ""
                    },
                    Mail = new()
                    {
                        SmtpHost = "localhost",
                        SmtpPort = 587,
                        UserName = "",
                        PassWord = ""
                    },
                };
            }
            public void Save(string path)
            {
                using var f = new StreamWriter(path);
                f.Write(this.ToString());
            }

            public override string ToString()
            {
                return Toml.Serialize(this);
            }

            public string ToJson()
            {
                return Json.Serialize(this);
            }
        }

        public static Base Load(string path)
        {
            string c;
            using (var f = File.OpenText(path))
            {
                c = f.ReadToEnd();
            }
            return Toml.Deserialize<Base>(c);
        }
    }
}
