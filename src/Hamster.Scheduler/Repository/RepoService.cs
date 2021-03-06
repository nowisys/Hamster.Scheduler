﻿using System.Linq;

namespace Hamster.Scheduler.Repository
{
  public class RepoService<TKey, TData> : IRepoService<TData>
  {
    private readonly IKeyConverter<TKey> converter;
    private readonly IRepository<TKey, TData> repository;

    public RepoService(IRepository<TKey, TData> repository)
      : this(repository, new KeyConverter<TKey>())
    {

    }

    public RepoService(IRepository<TKey, TData> repository, IKeyConverter<TKey> converter)
    {
      this.repository = repository;
      this.converter = converter;
    }
    
    public virtual string[] GetKeys()
    {
      return (from k in repository.GetKeys()
              select converter.ToString(k)).ToArray();
    }

    public virtual TData[] GetItems(int offset, int count)
    {
      return repository.GetItems(offset, count).ToArray();
    }

    public virtual TData Get(string key)
    {
      return repository.Get(converter.FromString(key));
    }

    public virtual void Remove(string key)
    {
      repository.Remove(converter.FromString(key));
    }

    public virtual void Update(string key, TData item)
    {
      if (key == null)
      {
        repository.Add(item);
      }
      else
      {
        repository.Update(converter.FromString(key), item);
      }
    }
  }
}
