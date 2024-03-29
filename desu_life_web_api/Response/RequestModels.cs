﻿namespace WebAPI.Request;

public class LoginRequest
{
    public string? MailAddress { get; set; }
    public string? Password { get; set; }
}

public class ChangePasswordRequest
{
    public string? NewPassword { get; set; }
}

public class ResetPasswordRequest
{
    public string? MailAddress { get; set; }
    public string? NewPassword { get; set; }
    public string? Token { get; set; }
}

public class RegistrationRequest
{
    public string? Username { get; set; }
    public string? MailAddress { get; set; }
    public string? Password { get; set; }
    public string? Token { get; set; }
}

public class UpdateOsuModeRequest
{
    public string? Mode { get; set; }
}
