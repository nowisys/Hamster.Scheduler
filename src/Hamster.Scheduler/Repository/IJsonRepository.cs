namespace Hamster.Scheduler.Repository
{
  public interface IJsonRepository<TData>
  {
    string[] GetKeys();
    
    TData[] GetItems(int offset, int count);
    
    TData Get(string key);
    
    void Remove(string key);
    
    void Update(string key, TData item);
  }
}
