using System.Threading.Tasks;

namespace RockPaperScissorsGame.Client.Services
{
    public class SingleStorage<T> : ISingleStorage<T>
    {
        private readonly T[] _data = new T[1];
        public void Update(T item)
        {
            _data[0] = item;
        }

        public Task UpdateAsync(T item)
        {
            Update(item);
            return Task.CompletedTask;
        }
        public T Get()
        {
            return _data[0];
        }
    }
}
