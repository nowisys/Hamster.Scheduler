using Hamster.Scheduler.Repository;

namespace NCM.Service.Scheduler.Repository
{
  public interface IRepoService<TData> : IJsonRepository<TData>, IXmlRepository<TData>
  {
  }
}
