using System;

namespace RockPaperScissorsGame.Client.Exceptions
{
    /// <summary>
    /// Exception is thrown when server is down
    /// </summary>
    public class ConnectionException : Exception
    {
        public ConnectionException()
        {
            
        }

        public ConnectionException(string message) : base(message)
        {
            
        }

        public ConnectionException(string message, Exception innerException) : base(message, innerException)
        {
            
        }
    }
}