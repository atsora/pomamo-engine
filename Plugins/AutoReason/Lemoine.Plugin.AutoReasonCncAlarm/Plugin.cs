// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;
using Lemoine.Extensions.Plugin;

namespace Lemoine.Plugin.AutoReasonCncAlarm
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin : Extensions.AutoReason.AutoReasonAutoConfigNoDefaultReasonPlugin<Configuration>, IPluginDll
  {
    #region Getters / Setters
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name { get { return "Auto-reason for stops around some specific cnc alarms"; } }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description
    {
      get
      {
        return "Associate automatically a specified reason when no activity is detected around some specific cnc alarms";
      }
    }

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version { get { return 1; } }
    #endregion // Getters / Setters

    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Plugin () : base ()
    { }
    #endregion // Constructors
  }
}
