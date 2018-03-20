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
        public const ushort ResetPasswordSuccess = 2103;

        public const ushort ConfirmEmail = 2150;
        public const ushort ConfirmEmailFailed = 2151;
        public const ushort ConfirmEmailSuccess = 2152;
        public const ushort RequestNewEmailConfirmationCode = 2153;

        //Spawners & Spawning
        public const ushort RegisterSpawner = 2200;
        public const ushort RegisterSpawnerFailed = 2201;
        public const ushort RegisterSpawnerSuccess = 2202;
        public const ushort RegisterSpawnedProcess = 2205;
        public const ushort RegisterSpawnedProcessSuccess = 2206;
        public const ushort RegisterSpawnedProcessFailed = 2207;
        public const ushort RequestSpawnFromClientToMaster = 2250;
        public const ushort RequestSpawnFromClientToMasterFailed = 2251;
        public const ushort RequestSpawnFromClientToMasterSuccess = 2252;
        public const ushort RequestSpawnFromMasterToSpawner = 2253;
        public const ushort RequestSpawnFromMasterToSpawnerFailed = 2254;
        public const ushort RequestSpawnFromMasterToSpawnerSuccess = 2255;
        public const ushort KillSpawn = 2260;
        public const ushort NotifySpawnerKilledProcess = 2261;
        public const ushort SpawnStatusChanged = 2270;
        public const ushort CompleteSpawnProcess = 2280;
        public const ushort CompleteSpawnProcessFailed = 2281;
        public const ushort CompleteSpawnProcessSuccess = 2282;

        //Rooms
        public const ushort RegisterRoom = 2300;
        public const ushort RegisterRoomFailed = 2301;
        public const ushort RegisterRoomSuccess = 2302;
    }
}