using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Database.Models;
using WebAPI.Response;
using WebAPI.Cookie;
using WebAPI.Request;
using WebAPI.Security;
using WebAPI.Http;
using WebAPI.Database;
using static WebAPI.Security.Token;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WebAPI.Controllers.Editor;

[ApiController]
[Route("[controller]")]
public class LoginController(ILogger<Log> logger, ResponseService responseService, Cookies cookies) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpPost(Name = "UpdateOsuMode")]
    public async Task<ActionResult> ExecuteLoginAsync([FromBody] Request.UpdateOsuModeRequest request)
    {
        // 检查用户Token是否有效并从中获取信息
        if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var userId, out var mailAddr, out var token))
        {
            logger.LogWarning("{CurrentTime} UpdateOsuMode 中递交了无效的Token。", $"[{GetCurrentTime}]");
            HttpContext.Response.Cookies.Append("token", "", cookies.Expire); // 强制登出
            return responseService.Response(HttpStatusCodes.Forbidden, "Invalid request.");
        }

        // 获取用户信息
        try
        {
            // 检查模式
            var validModes = new HashSet<string> { "osu", "fruit", "taiko", "mania" };
            if (string.IsNullOrEmpty(request.Mode) || !validModes.Contains(request.Mode))
            {
                return responseService.Response(HttpStatusCodes.BadRequest,
                    string.IsNullOrEmpty(request.Mode) ? "Please provide the game mode." : "Please provide a valid game mode.");
            }

            (var user, var qq, var osu, var discord, var badges) = await ControllerUtils.GetFullUserInfoAsync(userId);
            if (user is null)
            {
                logger.LogWarning("{CurrentTime} UpdateOsuMode 失败，用户不存在。", $"[{GetCurrentTime}]");
                HttpContext.Response.Cookies.Append("token", "", cookies.Expire); // 强制登出
                return responseService.Response(HttpStatusCodes.Unauthorized, "User logged in but not found in database.");
            }
            if (osu is null)
            {
                logger.LogWarning("{CurrentTime} UpdateOsuMode 失败，用户未绑定至osu!账户。", $"[{GetCurrentTime}]");
                return responseService.Response(HttpStatusCodes.BadRequest, "Your account is currently not linked to osu! account.");
            }

            if (!await DB.UpdateUserOsuDefaultMode(user.ID, request.Mode))
            {
                logger.LogWarning("{CurrentTime} UpdateOsuMode 失败。", $"[{GetCurrentTime}]");
                return responseService.Response(HttpStatusCodes.InternalServerError, "An error has occurred.");
            }

            // 成功
            return responseService.Response(HttpStatusCodes.Ok, "Successfully unlinked.");
        }
        catch (Exception ex)
        {
            logger.LogWarning("{CurrentTime} UpdateOsuMode 失败。错误信息：{Message}", $"[{GetCurrentTime}]", ex.Message);
            return responseService.Response(HttpStatusCodes.InternalServerError, "An error has occurred.");
        }
    }
}
