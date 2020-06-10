using System.Collections.Generic;
using NCM.Service.Scheduler.Repository;

namespace Hamster.Scheduler.Repository
{
  public interface IRepository<TKey, TData>
  {
    IList<TKey> GetKeys();
    IList<TData> GetItems();
    IList<TData> GetItems(int offset, int count);
    TData Get(TKey key);

    void Add(TData data);
    void Update(TKey key, TData item);
    void Remove(TKey key);

    IRepoService<TData> GetService();
  }
}
