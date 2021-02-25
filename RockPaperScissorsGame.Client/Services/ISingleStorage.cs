namespace RockPaperScissorsGame.Client.Services
{
    public interface ISingleStorage<T>
    {
        public void Update(T item);
    }
}
