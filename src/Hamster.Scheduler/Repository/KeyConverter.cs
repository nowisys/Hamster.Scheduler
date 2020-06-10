using System;

namespace Hamster.Scheduler.Repository
{
  public class KeyConverter<TKey> : IKeyConverter<TKey>
  {
    public virtual string ToString(TKey key)
    {
      return Convert.ToString(key);
    }

    public virtual TKey FromString(string key)
    {
      return (TKey)Convert.ChangeType(key, typeof(TKey));
    }
  }
}
