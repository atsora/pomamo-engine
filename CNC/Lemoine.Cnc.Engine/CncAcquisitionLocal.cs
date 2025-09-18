// Copyright (C) 2025 Atsora Solutions

#if !NET40

using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.CncEngine
{
  /// <summary>
  /// Local implementation of <see cref="Lemoine.Model.ICncAcquisition"/> to be used when there is no database connection
  /// </summary>
  public class CncAcquisitionLocal : Lemoine.Model.ICncAcquisition
  {
    public int Id => 0;
    public string Name { get; set; }
    public string ConfigFile { get; set; }
    public string ConfigPrefix { get; set; }
    public string ConfigParameters { get; set; }
    public IDictionary<string, string> ConfigKeyParams { get; set; }
    public string ConfigKeyParamsJson { get; set; }
    public bool UseProcess { get; set; } = false;
    public bool StaThread { get; set; }
    public bool UseCoreService { get; set; }
    public TimeSpan Every { get; set; } = TimeSpan.FromSeconds (2);
    public TimeSpan NotRespondingTimeout { get; set; } = TimeSpan.FromSeconds (30);
    public TimeSpan SleepBeforeRestart { get; set; } = TimeSpan.FromSeconds (10);

    public ICollection<IMachineModule> MachineModules => new List<IMachineModule> ();

    public IComputer Computer { get; set; }
    public CncModuleLicense License { get; set; }

    public string SelectionText => "LocalTest";

    public int Version => 0;

    public void Unproxy ()
    {
    }
  }
}

#endif // !NET40