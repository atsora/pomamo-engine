// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Plugin;
using System.Linq;
using System.Globalization;
using Lemoine.Model;
using System.Diagnostics;
using Lemoine.Core.Log;
using Pulse.Extensions.Plugin;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Extensions.AutoReason
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public abstract class AutoReasonManualConfigPlugin<TConfiguration> : PluginWithConfig<TConfiguration>, IPluginDll, IFlaggedPlugin
    where TConfiguration : AutoReasonConfiguration, new ()
  {
    #region Members
    string m_defaultReasonTranslationKey;
    string m_defaultReasonTranslationValue;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Applicable flags
    /// </summary>
    public virtual PluginFlag Flags
    {
      get
      {
        return PluginFlag.AutoReason | PluginFlag.Analysis | PluginFlag.Web;
      }
    }

    /// <summary>
    /// Multiple configurations
    /// </summary>
    public override bool MultipleConfigurations => true;

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version { get { return 1; } }
    #endregion // Getters / Setters

    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonAutoConfigPlugin<TConfiguration>).FullName);

    #region Constructors
    /// <summary>
    /// Constructor with the default reason
    /// </summary>
    /// <param name="defaultReasonTranslationKey">not null or empty</param>
    /// <param name="defaultReasonTranslationValue">not null or empty</param>
    public AutoReasonManualConfigPlugin (string defaultReasonTranslationKey, string defaultReasonTranslationValue)
    {
      Debug.Assert (!string.IsNullOrEmpty (defaultReasonTranslationKey));
      Debug.Assert (!string.IsNullOrEmpty (defaultReasonTranslationValue));

      m_defaultReasonTranslationKey = defaultReasonTranslationKey;
      m_defaultReasonTranslationValue = defaultReasonTranslationValue;
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    AutoReasonManualConfigPlugin ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Install from a specific version
    /// (create or update tables if necessary, ...)
    /// This method is called within a transaction
    /// </summary>
    /// <param name="version"></param>
    protected override void InstallVersion (int version)
    {
      switch (version) {
        case 1: // First installation
          AddDefaultReason ();
          CompleteInstall ();
          break;
        default:
          throw new InvalidOperationException ();
      }
    }

    void AddDefaultReason ()
    {
      if (!string.IsNullOrEmpty (m_defaultReasonTranslationKey)
        && !string.IsNullOrEmpty (m_defaultReasonTranslationValue)) {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginTransaction ("AutoReason.Plugin.Install.1")) {
            AddReason (m_defaultReasonTranslationKey, m_defaultReasonTranslationValue);
            transaction.Commit ();
          }
        }
      }
    }

    /// <summary>
    /// Complete the installation if needed
    /// </summary>
    protected virtual void CompleteInstall () { }

    /// <summary>
    /// Add a translation if it does not exist yet
    /// </summary>
    /// <param name="translationKey"></param>
    /// <param name="translationValue"></param>
    /// <returns></returns>
    protected ITranslation AddTranslation (string translationKey, string translationValue)
    {
      return ConfigRequests.AddTranslation (translationKey, translationValue);
    }

    /// <summary>
    /// Add a reason group if it does not exist yet
    /// </summary>
    /// <param name="translationKey">not null or empty</param>
    /// <param name="translationValue">not null</param>
    /// <returns></returns>
    protected IReasonGroup AddReasonGroup (string translationKey, string translationValue)
    {
      return ConfigRequests.AddReasonGroup (translationKey, translationValue);
    }

    /// <summary>
    /// Add a reason
    /// </summary>
    /// <param name="translationKey">not null or empty</param>
    /// <param name="translationValue">not null</param>
    /// <returns></returns>
    protected IReason AddReason (string translationKey, string translationValue)
    {
      return ConfigRequests.AddReason (translationKey, translationValue);
    }

    /// <summary>
    /// Method to create the reason
    /// </summary>
    /// <param name="translationKey">not null or empty</param>
    /// <param name="reasonGroup">not null</param>
    /// <returns></returns>
    protected virtual IReason CreateReason (string translationKey, IReasonGroup reasonGroup)
    {
      return ConfigRequests.CreateReason (translationKey, reasonGroup);
    }
    #endregion // Methods
  }
}
