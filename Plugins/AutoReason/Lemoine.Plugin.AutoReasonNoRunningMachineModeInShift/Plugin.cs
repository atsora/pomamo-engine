// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Plugin;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Plugin.AutoReasonNoRunningMachineModeInShift
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin : Extensions.AutoReason.AutoReasonAutoConfigPlugin<Configuration>, IPluginDll
  {
    public static readonly string DEFAULT_REASON_TRANSLATION_KEY = "ReasonShiftNoRunningMachineModeInShift";
    
    #region Getters / Setters
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name { get { return "Auto-reason for a shift with no activity"; } }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description
    {
      get
      {
        return "Associate automatically a specified reason when no activity is detected on a whole shift";
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
    public Plugin () : base (DEFAULT_REASON_TRANSLATION_KEY, "No running machine mode in shift")
    { }
    #endregion // Constructors
  }
}
