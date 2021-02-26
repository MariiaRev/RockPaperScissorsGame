using System;

namespace RockPaperScissorsGame.Server.Exceptions
{
    public class ServiceException : Exception
    {
        public ServiceException()
        {
            
        }

        public ServiceException(string message) : base(message)
        {
            
        }

        public ServiceException(string message, Exception innerException) : base(message, innerException)
        {
            
        }
    }
}