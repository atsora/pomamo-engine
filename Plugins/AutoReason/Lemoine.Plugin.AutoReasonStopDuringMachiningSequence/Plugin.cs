// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;
using Lemoine.Extensions.Plugin;

namespace Lemoine.Plugin.AutoReasonStopDuringMachiningSequence
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin : Extensions.AutoReason.AutoReasonAutoConfigPlugin<Configuration>, IPluginDll
  {
    public static readonly string DEFAULT_REASON_TRANSLATION_KEY = "ReasonStopDuringMachiningSequence";
    
    #region Getters / Setters
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name { get { return "Auto-reason for a stop occurring in a machining sequence"; } }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description
    {
      get
      {
        return "Associate automatically a specified reason when no activity is detected in a machining sequence";
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
    public Plugin () : base (DEFAULT_REASON_TRANSLATION_KEY, "Stop during a machining sequence")
    { }
    #endregion // Constructors
  }
}
