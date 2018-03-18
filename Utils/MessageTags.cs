namespace Utils
{
    public static class MessageTags
    {
        //Auth & Security
        public const ushort RequestAesKey = 1500;
        public const ushort RequestAesKeyResponse = 1501;

        public const ushort RequestPermissionLevel = 1550;
        public const ushort RequestPermissionLevelResponse = 1551;

        public const ushort LogIn = 2000;
        public const ushort LoginFailedResponse = 2001;
        public const ushort LoginSuccessResponse = 2002;

        public const ushort RegisterAccount = 2050;
        public const ushort RegisterAccountFailed = 2051;
        public const ushort RegisterAccountSuccess = 2052;
        public const ushort RequestPasswordResetCode = 2100;

        public const ushort ResetPassword = 2101;
        public const ushort ResetPasswordFailed = 2102;
        public const ushort ResetPasswordPasswordSuccess = 2103;

        public const ushort ConfirmEmail = 2150;
        public const ushort ConfirmEmailFailed = 2151;
        public const ushort ConfirmEmailSuccess = 2152;
        public const ushort RequestNewEmailConfirmationCode = 2153;

        //Spawners & Spawning
        public const ushort RegisterSpawner = 2200;
        public const ushort RegisterSpawnerFailed = 2201;
        public const ushort RegisterSpawnerSuccess = 2202;
        public const ushort RequestSpawn = 2250;
        public const ushort SpawnStatusChanged = 2251;
        public const ushort KillSpawn = 2252;
        public const ushort RequestClientSpawn = 2253;
        public const ushort RequestClientSpawnFailed = 2254;
        public const ushort RequestClientSpawnSuccess = 2255;
    }
}
