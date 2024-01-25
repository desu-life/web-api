namespace desu_life_web_backend;

public static partial class Cookies
{
    public static CookieOptions Expire { get; set; }
    public static CookieOptions Login { get; set; }

    static Cookies()
    {
        Expire = new()
        {
            Expires = DateTime.Now.AddMinutes(-1),
            HttpOnly = true // no JavaScript access
        };

        Login = new()
        {
            Expires = DateTime.Now.AddMinutes(60), // same as JWT expire time
            HttpOnly = true // no JavaScript access
        };
    }

}
