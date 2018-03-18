namespace Utils
{
    public static class MessageTags
    {
        //Auth & Security
        public const ushort RequestAesKey = 1500;
        public const ushort RequestAesKeyResponse = 1501;
        public const ushort LogIn = 2000;
        public const ushort LoginFailedResponse = 2001;
        public const ushort LoginSuccessResponse = 2002;
        public const ushort RegisterAccount = 2050;
        public const ushort RegisterAccountFailedResponse = 2051;
        public const ushort RegisterAccountSuccessResponse = 2052;
        public const ushort RequestPasswordResetCode = 2100;
        public const ushort RequestPasswordReset = 2101;
        public const ushort RequestPasswordResetFailedResponse = 2102;
        public const ushort RequestPasswordResetSuccessResponse = 2103;
        public const ushort ConfirmEmail = 2150;
        public const ushort ConfirmEmailFailed = 2151;
        public const ushort ConfirmEmailSuccess = 2152;
        public const ushort RequestNewEmailConfirmationCode = 2153;
    }
}
