// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Plugin;
using Lemoine.Model;
using Lemoine.Database.Persistent;
using Pulse.Extensions.Plugin;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Plugin.AutoReasonBreak
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin : Extensions.AutoReason.AutoReasonAutoConfigPlugin<Configuration>, IPluginDll, IFlaggedPlugin
  {
    public static readonly string DEFAULT_REASON_TRANSLATION_KEY = "ReasonBreak";

    #region Members
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name { get { return "Auto-reason break"; } }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description
    {
      get
      {
        return "Associate automatically a specified reason around break times";
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
    public Plugin ()
      : base (DEFAULT_REASON_TRANSLATION_KEY, "Break")
    { }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Create the reason considering the id ReasonId.Break
    /// </summary>
    /// <param name="reasonTranslationKey"></param>
    /// <param name="reasonGroup"></param>
    protected override IReason CreateReason (string reasonTranslationKey, IReasonGroup reasonGroup)
    {
      if (reasonTranslationKey.Equals (DEFAULT_REASON_TRANSLATION_KEY, StringComparison.InvariantCultureIgnoreCase)) {
        using (var command = ModelDAOHelper.DAOFactory.GetConnection ().CreateCommand ()) {
          command.CommandText = string.Format (@"INSERT INTO Reason (reasonid, reasontranslationkey, reasongroupid)
VALUES ({0}, '{1}', {2})",
                                               (int)ReasonId.Break,
                                               reasonTranslationKey,
                                               reasonGroup.Id);
          command.ExecuteNonQuery ();
        }
        ModelDAOHelper.DAOFactory.Flush ();
        return ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Break);
      }
      else {
        return base.CreateReason (reasonTranslationKey, reasonGroup);
      }
    }
    #endregion // Methods
  }
}
