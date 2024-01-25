using Microsoft.AspNetCore.Mvc;

namespace desu_life_web_backend;

public static partial class ReturnRequests
{
    public enum Enums
    {
        None = 0,
        Ok = 200,
        Created = 201,
        Accepted = 202,
        NoContent = 204,
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        MethodNotAllowed = 405,
        Conflict = 409,
        InternalServerError = 500
    }

    public static ActionResult<SystemMsg> Request(Enums request, string message)
    {
        var systemMsg = new SystemMsg
        {
            Status = Enum.GetName(typeof(Enums), request) ?? "unknown",
            Msg = message
        };

        return new ObjectResult(systemMsg)
        {
            StatusCode = (int)request
        };
    }
}
