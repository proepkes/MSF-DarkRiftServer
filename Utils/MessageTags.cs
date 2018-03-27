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
        public const ushort RegisterSpawner = 2200; //1.) Spawner.RegisterTo -> Master
        public const ushort RegisterSpawnerFailed = 2201;
        public const ushort RegisterSpawnerSuccess = 2202;
        public const ushort RequestSpawnFromClientToMaster = 2250; //2.) Client.RequestSpawnTo -> Master
        public const ushort RequestSpawnFromClientToMasterFailed = 2251;
        public const ushort RequestSpawnFromClientToMasterSuccess = 2252;
        public const ushort RequestSpawnFromMasterToSpawner = 2253; //3.) Master.CreateTaskFor -> Spawner
        public const ushort RequestSpawnFromMasterToSpawnerFailed = 2254;
        public const ushort RequestSpawnFromMasterToSpawnerSuccess = 2255;
        public const ushort RegisterSpawnedProcess = 2256; //4.) Spawn.RegisterProcessTo -> Master
        public const ushort RegisterSpawnedProcessSuccess = 2257;
        public const ushort RegisterSpawnedProcessFailed = 2258;
        public const ushort SpawnStatusChanged = 2259; //5.) Notification of Process-status
        public const ushort CompleteSpawnProcess = 2260; //6.) Spawn.NotifyCompleteTo -> Master
        public const ushort CompleteSpawnProcessFailed = 2261;
        public const ushort CompleteSpawnProcessSuccess = 2262;
        public const ushort GetFinalizationData = 2263; //7.) Client -> GetSpawnDataFromMaster
        public const ushort GetFinalizationDataFailed = 2264; 
        public const ushort GetFinalizationDataSuccess = 2265; 
        public const ushort KillSpawn = 2270;
        public const ushort NotifySpawnerKilledProcess = 2271;

        //Rooms
        public const ushort RegisterRoom = 2300;
        public const ushort RegisterRoomFailed = 2301;
        public const ushort RegisterRoomSuccess = 2302;
        public const ushort GetRoomAccess = 2303;
        public const ushort GetRoomAccessFailed = 2304;
        public const ushort GetRoomAccessSuccess = 2305;
        public const ushort ProvideRoomAccessCheck = 2350;
        public const ushort ProvideRoomAccessCheckFailed = 2351;
        public const ushort ProvideRoomAccessCheckSuccess = 2352;
        public const ushort ValidateRoomAccess = 2353;
        public const ushort ValidateRoomAccessFailed = 2354;
        public const ushort ValidateRoomAccessSuccess = 2355;
        public const ushort AccessRoom = 2360; //Call from client

        //Game-reated
        public const ushort GetNetworkTime = 3000;
        public const ushort SpawnEntity = 3010;
        public const ushort DespawnPlayer = 3011;
        public const ushort NavigateTo = 3012;
        public const ushort ChangeState = 3013;
    }
}