using System;
using System.Collections.Generic;
using Microsoft.Scripting.Hosting;

namespace Hamster.Scheduler.Commands
{
  public class ScriptCommand : ICommand
  {
    private readonly ScriptScope scope;
    private readonly CompiledCode code;

    public ScriptCommand(CompiledCode code, ScriptScope scope)
    {
      if ((this.code = code) == null)
        throw new ArgumentNullException(nameof(code));

      if ((this.scope = scope) == null)
        throw new ArgumentNullException(nameof(scope));
    }

    public void Invoke(IDictionary<string, object> parameters)
    {
      ScriptScope local = scope.Engine.CreateScope();
      foreach (KeyValuePair<string, object> item in scope.GetItems())
      {
        local.SetVariable(item.Key, item.Value);
      }

      if (parameters != null)
      {
        foreach (KeyValuePair<string, object> item in parameters)
        {
          local.SetVariable(item.Key, item.Value);
        }
      }

      code.Execute(local);
    }
  }
}
