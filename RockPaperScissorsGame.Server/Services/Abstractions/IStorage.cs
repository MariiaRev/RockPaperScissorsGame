using System.Collections.Generic;
using System.Threading.Tasks;
using RockPaperScissorsGame.Server.Models;

namespace RockPaperScissorsGame.Server.Services.Abstractions
{
    public interface IStorage<T> where T: class
    {
        int Count();
        Task<int> CountAsync();

        T Get(int id);
        Task<T> GetAsync(int id);

        IEnumerable<ItemWithId<T>> GetAll();
        Task<IEnumerable<ItemWithId<T>>> GetAllAsync();

        int? Add(T item, int? id = null, IEqualityComparer<T> comparer = null);
        Task<int?> AddAsync(T item, int? id = null, IEqualityComparer<T> comparer = null);

        void AddOrUpdate(int id, T item);
        Task AddOrUpdateAsync(int id, T item);

        bool Delete(int id);
        Task<bool> DeleteAsync(int id);
    }
}
