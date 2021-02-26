using System;
using RockPaperScissorsGame.Client.Helpers.Abstract;

namespace RockPaperScissorsGame.Client.Helpers.Implementations
{
    public class UserInput : IUserInput
    {
        public string ReadString(string message, string tryAgainMessage, uint minLength, string exitWord = null)
        {
            Console.WriteLine(message);
            var userInput = Console.ReadLine()?.Trim();

            while (userInput?.Length < minLength)
            {
                Console.WriteLine(tryAgainMessage);
                userInput = Console.ReadLine()?.Trim();

                if (userInput != null &&exitWord != null && userInput.Equals(exitWord, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
            }

            return userInput;
        }

        public bool ReadString(string message, string keyword, bool ignoreCase)
        {
            Console.WriteLine(message);
            var userInput = Console.ReadLine();

            if (ignoreCase)
            {
                return keyword.Equals(userInput, StringComparison.OrdinalIgnoreCase);
            }

            return keyword == userInput;
        }
    }
}
