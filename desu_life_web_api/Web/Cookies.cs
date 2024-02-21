namespace desu_life_web_api.Cookie;

public partial class Cookies
{
    public CookieOptions Default => new()
    {
        Expires = DateTime.Now.AddMinutes(60),
        HttpOnly = true // no JavaScript access
    };

    public CookieOptions Expire => new()
    {
        Expires = DateTime.Now.AddDays(-1),
        HttpOnly = true
    };

    public CookieOptions Login => new()
    {
        Expires = DateTime.Now.AddMinutes(60), // same as JWT expire time
        HttpOnly = true
    };

    public CookieOptions Verity => new()
    {
        Expires = DateTime.Now.AddMinutes(60),
        HttpOnly = true
    };
}