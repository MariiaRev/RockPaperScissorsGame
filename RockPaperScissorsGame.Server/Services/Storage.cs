using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using RockPaperScissorsGame.Server.Models;

namespace RockPaperScissorsGame.Server.Services
{
    public class Storage<T>: IStorage<T> where T : class
    {
        private readonly ConcurrentDictionary<int, T> _data = new ConcurrentDictionary<int, T>();

        public int Count()
        {
            return _data.Count();
        }

        public Task<int> CountAsync()
        {
            return Task.FromResult(Count());
        }

        public T Get(int id)
        {
            return _data.TryGetValue(id, out var item) ? item : default;
        }

        public Task<T> GetAsync(int id)
        {
            return Task.FromResult(Get(id));
        }

        public IEnumerable<ItemWithId<T>> GetAll()
        {
            return _data.Select(item => new ItemWithId<T> { Id = item.Key, Item = item.Value });
        }

        public Task<IEnumerable<ItemWithId<T>>> GetAllAsync()
        {
            return Task.FromResult(GetAll());
        }

        public int? Add(T item, int? id = null, IEqualityComparer<T> comparer = null)
        {
            if (comparer == null &&
                _data.Values.Contains(item))
                return null;

            if (comparer != null &&
                _data.Values.Contains(item, comparer))
                return null;

            if (id == null)
            {
                id = _data.Keys.Any() ? _data.Keys.Max() + 1 : 1;
            }

            if (_data.TryAdd((int)id, item))
            {
                return (int)id;
            }

            return null;
        }

        public Task<int?> AddAsync(T item, int? id = null, IEqualityComparer<T> comparer = null)
        {
            return Task.FromResult(Add(item, id, comparer));
        }

        public void AddOrUpdate(int id, T item)
        {
            _data[id] = item;
        }

        public Task AddOrUpdateAsync(int id, T item)
        {
            AddOrUpdate(id, item);
            return Task.CompletedTask;
        }

        public bool Delete(int id)
        {
            return _data.Remove(id, out _);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return Task.FromResult(Delete(id));
        }
    }
}
