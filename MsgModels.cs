using Flurl;
using Newtonsoft.Json;
using NullValueHandling = Newtonsoft.Json.NullValueHandling;

namespace desu_life_web_backend
{
    public class SystemMsg
    {
        public string? Status { get; set; }

        public string? Msg { get; set; }
    }
}
