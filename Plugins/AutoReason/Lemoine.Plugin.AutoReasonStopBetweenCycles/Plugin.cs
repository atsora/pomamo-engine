// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;
using Lemoine.Extensions.Plugin;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.AutoReasonStopBetweenCycles
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin : Extensions.AutoReason.AutoReasonAutoConfigPlugin<Configuration>, IPluginDll
  {
    public static readonly string DEFAULT_REASON_TRANSLATION_KEY = "ReasonStopBetweenCycles";
    
    #region Getters / Setters
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name { get { return "Auto-reason for stops between cycles"; } }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description
    {
      get
      {
        return "Associate automatically a specified reason between cycles when no activity is detected";
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
    public Plugin () : base (DEFAULT_REASON_TRANSLATION_KEY, "Stop between cycles")
    { }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Complete the installation if needed
    /// </summary>
    protected override void CompleteInstall ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("AutoReason.Plugin.Install.2")) {
          AddReason (DEFAULT_REASON_TRANSLATION_KEY + "Extended", "Extended stop between cycles");
          transaction.Commit ();
        }
      }
    }
    #endregion Methods
  }
}
