namespace Auth.Services.API.AuthRoutes
{
    public static class AuthApiRoutes
    {
        public const string Root = "api";
        public const string Version = "v1";
        public const string Base = Root + "/" + Version + "/auth";

        public static class Authentication
        {
            public const string Login = "login";
            public const string Register = "register";
            public const string VerifyEmailOtp = "verify-register-otp";
            public const string ForgotPassword = "forgot-password";
            public const string VerifyPasswordOtp = "verify-password-otp";
            public const string ResetPassword = "reset-password";
            public const string RefreshToken = "refresh-token";
            public const string Profile = "profile";
            public const string Health = "health";
            public const string TestEmail = "test-email";
        }
    }
}
