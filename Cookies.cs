namespace desu_life_web_backend;

public static partial class Cookies
{
    public static CookieOptions Default { get; set; }
    public static CookieOptions Expire { get; set; }
    public static CookieOptions Login { get; set; }
    public static CookieOptions Verity { get; set; }

    static Cookies()
    {
        Default = new()
        {
            Expires = DateTime.Now.AddMinutes(60),
            HttpOnly = true // no JavaScript access
        };

        Expire = new()
        {
            Expires = DateTime.Now.AddDays(-1),
            HttpOnly = true
        };

        Login = new()
        {
            Expires = DateTime.Now.AddMinutes(60), // same as JWT expire time
            HttpOnly = true
        };

        Verity = new()
        {
            Expires = DateTime.Now.AddMinutes(60),
            HttpOnly = true
        };
    }

}
