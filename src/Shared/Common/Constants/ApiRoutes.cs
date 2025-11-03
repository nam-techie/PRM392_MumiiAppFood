namespace Mumii.Shared.Common.Constants;

/// <summary>
/// Định nghĩa các API routes cho toàn bộ hệ thống
/// </summary>
public static class ApiRoutes
{
    public const string BaseUrl = "/api";
    
    /// <summary>
    /// Routes cho Auth Service
    /// </summary>
    public static class Auth
    {
        public const string Base = $"{BaseUrl}/auth";
        public const string Register = $"{Base}/register";
        public const string Login = $"{Base}/login";
        public const string GoogleLogin = $"{Base}/google";
        public const string RefreshToken = $"{Base}/refresh";
        public const string Logout = $"{Base}/logout";
        public const string Profile = $"{Base}/profile";
        public const string ChangePassword = $"{Base}/change-password";
        public const string ForgotPassword = $"{Base}/forgot-password";
        public const string ResetPassword = $"{Base}/reset-password";
    }
    
    /// <summary>
    /// Routes cho Discovery Service
    /// </summary>
    public static class Discovery
    {
        public const string Base = $"{BaseUrl}/restaurants";
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string Delete = $"{Base}/{{id}}";
        public const string Search = $"{Base}/search";
        public const string Nearby = $"{Base}/nearby";
    }
    
    /// <summary>
    /// Routes cho Social Service
    /// </summary>
    public static class Social
    {
        public const string Base = $"{BaseUrl}/posts";
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string Delete = $"{Base}/{{id}}";
        public const string React = $"{Base}/{{id}}/react";
        public const string Comments = $"{Base}/{{id}}/comments";
        public const string CreateComment = $"{Base}/{{id}}/comments";
    }
    
    /// <summary>
    /// Routes cho Health Check
    /// </summary>
    public static class Health
    {
        public const string Base = $"{BaseUrl}/health";
        public const string Ready = $"{Base}/ready";
        public const string Live = $"{Base}/live";
    }
}
