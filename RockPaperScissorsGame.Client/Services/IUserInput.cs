using System;
using System.Collections.Generic;
using System.Text;

namespace RockPaperScissorsGame.Client.Services
{
    public interface IUserInput
    {
        string ReadString(string message, string tryAgainMessage, uint minLength, string exitWord = null);

        bool ReadString(string message, string keyword, bool ignoreCase);
    }
}
