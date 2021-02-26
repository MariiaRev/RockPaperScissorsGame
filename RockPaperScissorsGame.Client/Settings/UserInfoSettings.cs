namespace RockPaperScissorsGame.Client.Settings
{
    public class UserInfoSettings
    {
        public uint LoginMinLength { get; set; }
        public uint PasswordMinLength { get; set; }

        /// <summary>
        /// The number of authorization attempts after which it is temporarily blocked.
        /// </summary>
        public int AuthorizationAttemptsMax { get; set; }

        /// <summary>
        /// Time to temporarily block authorization in seconds.
        /// </summary>
        public int AuthorizationBlockingTime { get; set; }
    }
}
