namespace Hamster.Scheduler.Repository
{
  public interface IKeyConverter<TKey>
  {
    string ToString(TKey key);
    TKey FromString(string key);
  }
}
