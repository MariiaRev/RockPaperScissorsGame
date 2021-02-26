namespace RockPaperScissorsGame.Client.Helpers.Abstract
{
    public interface IUserInput
    {
        string ReadString(string message, string tryAgainMessage, uint minLength, string exitWord = null);

        bool ReadString(string message, string keyword, bool ignoreCase);
    }
}
