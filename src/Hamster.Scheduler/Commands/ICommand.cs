using System.Collections.Generic;

namespace Hamster.Scheduler.Commands
{
  public interface ICommand
  {
    void Invoke(IDictionary<string, object> parameters);
  }
}
