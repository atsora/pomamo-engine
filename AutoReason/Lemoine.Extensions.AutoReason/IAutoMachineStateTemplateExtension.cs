// Copyright (C) 2026 Atsora Solutions

using Lemoine.Model;
using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.AutoReason
{
  /// <summary>
  /// Extension to create automatically some machine state templates
  /// 
  /// It is run by the auto-reason service, just like <see cref="IAutoReasonExtension"/>
  /// </summary>
  public interface IAutoMachineStateTemplateExtension
    : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Associated machine
    /// </summary>
    IMonitoredMachine Machine { get; }

    /// <summary>
    /// Machine state template that is applied by this extension
    /// 
    /// Not null once Initialize returned true
    /// </summary>
    IMachineStateTemplate MachineStateTemplate { get; }

    /// <summary>
    /// Optional machine state template to apply once the dynamic end is reached
    /// </summary>
    IMachineStateTemplate NextMachineStateTemplate { get; }

    /// <summary>
    /// Initialize the extension with a machine
    /// 
    /// The plugin is de-activated if false is returned
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="caller"></param>
    /// <returns>Return if the plugin should be activated</returns>
    bool Initialize (IMonitoredMachine machine, Lemoine.Threading.IChecked caller);

    /// <summary>
    /// Check the data
    /// 
    /// One or several transactions may be created in this method
    /// </summary>
    void RunOnce ();
  }
}
