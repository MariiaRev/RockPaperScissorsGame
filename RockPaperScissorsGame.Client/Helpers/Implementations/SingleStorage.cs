using RockPaperScissorsGame.Client.Helpers.Abstract;

namespace RockPaperScissorsGame.Client.Helpers.Implementations
{
    public class SingleStorage<T> : ISingleStorage<T>
    {
        private readonly T[] _data = new T[1];
        public void Update(T item)
        {
            _data[0] = item;
        }
    }
}
