using System;

namespace RockPaperScissorsGame.Client.Exceptions
{

    /// <summary>
    /// Exception is thrown when client received unexpected event from server
    /// </summary>
    public class UnexpectedBehaviorException : Exception
    {
        public UnexpectedBehaviorException()
        {
            
        }

        public UnexpectedBehaviorException(string message) : base(message)
        {
            
        }

        public UnexpectedBehaviorException(string message, Exception innerException) : base(message, innerException)
        {
            
        }
    }
}