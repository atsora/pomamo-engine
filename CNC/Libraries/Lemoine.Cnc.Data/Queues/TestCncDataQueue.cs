using System;
using System.Collections.Generic;
using System.Text;

namespace Lemoine.Cnc.Data.Queues
{
  /// <summary>
  /// Basic implementation for tests
  /// </summary>
  public class TestCncDataQueue : Queue<ExchangeData>, ICncDataQueue
  {
    public int MachineId { get; set; }
    public int MachineModuleId { get; set; }

    public void Close ()
    {
    }

    public void Delete ()
    {
    }

    public void Dispose ()
    {
    }

    public IList<ExchangeData> Peek (int nbElements)
    {
      var element = this.Peek ();
      return new List<ExchangeData> {
        element
      };
    }

    public void UnsafeDequeue ()
    {
      this.Dequeue ();
    }

    public void UnsafeDequeue (int n)
    {
      for (var i = 0; i < n; ++i) {
        this.UnsafeDequeue ();
      }
    }

    public bool VacuumIfNeeded ()
    {
      return true;
    }
  }
}
