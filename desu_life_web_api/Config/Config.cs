using System.IO;
using desu_life_web_api.Serializer;
using LanguageExt.UnitsOfMeasure;
using Tomlyn.Model;

namespace desu_life_web_api
{
    public class Config
    {
        public static Base? inner;

        public class API : ITomlMetadataProvider
        {
            public string? apiHost { get; set; }
            public int apiPort { get; set; }
            public bool useSSL { get; set; }
            TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
        }

        public class Mail : ITomlMetadataProvider
        {
            public string? smtpHost { get; set; }
            public int smtpPort { get; set; }
            public string? userName { get; set; }
            public string? passWord { get; set; }
            TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
        }

        public class Database : ITomlMetadataProvider
        {
            public string? type { get; set; }
            public string? host { get; set; }
            public int port { get; set; }
            public string? db { get; set; }
            public string? user { get; set; }
            public string? password { get; set; }
            TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
        }

        public class OSU : ITomlMetadataProvider
        {
            public int clientId { get; set; }
            public string? clientSecret { get; set; }
            public string? AuthorizeUrl { get; set; }
            public string? RedirectUrl { get; set; }
            public string? APIBaseUrl { get; set; }
            public string? TokenUrl { get; set; }
            TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
        }

        public class Discord : ITomlMetadataProvider
        {
            public string? clientId { get; set; }
            public string? clientSecret { get; set; }
            public string? AuthorizeUrl {  get; set; }
            public string? RedirectUrl { get; set; }
            public string? APIBaseUrl { get; set; }
            public string? TokenUrl { get; set; }
            TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
        }



        public class Base : ITomlMetadataProvider
        {
            public bool dev { get; set; }
            public API? api { get; set; }
            public OSU? osu { get; set; }
            public Database? database { get; set; }
            public Mail? mail { get; set; }
            public Discord? discord { get; set; }

            TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
            public static Base Default()
            {
                return new Base()
                {
                    dev = true,
                    api = new()
                    {
                        apiHost = "localhost",
                        apiPort = 5500,
                        useSSL = false
                    },
                    osu = new()
                    {
                        clientId = 0,
                        clientSecret = "",
                        TokenUrl = "",
                        APIBaseUrl = "",
                        RedirectUrl = "",
                        AuthorizeUrl = ""
                    },
                    database = new()
                    {
                        type = "mysql",
                        host = "",
                        port = 3306,
                        db = "kanonbot",
                        user = "",
                        password = ""
                    },
                    mail = new()
                    {
                        smtpHost = "localhost",
                        smtpPort = 587,
                        userName = "",
                        passWord = ""
                    },
                    discord = new()
                    {
                        clientId = "",
                        clientSecret = "",
                        TokenUrl = "",
                        APIBaseUrl = "",
                        RedirectUrl = "",
                        AuthorizeUrl = ""
                    }
                };
            }
            public void save(string path)
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


        public static Base load(string path)
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
