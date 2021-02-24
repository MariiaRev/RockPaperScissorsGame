namespace RockPaperScissorsGame.Server.Models
{
    public class User
    {
        readonly string _login;
        readonly string _password;

        public User (string login, string password)
        {
            _login = login;
            _password = password;
        }

        public string GetLogin()
        {
            return _login;
        }

        public string GetPassword()
        {
            return _password;
        }

        public bool VerifyPassword(string password)
        {
            return password == _password;
        }
    }
}
