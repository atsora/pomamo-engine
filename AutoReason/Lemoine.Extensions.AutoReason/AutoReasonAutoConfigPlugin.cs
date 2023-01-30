// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Plugin;
using System.Linq;
using System.Globalization;
using Lemoine.Model;
using System.Diagnostics;
using Lemoine.Extensions.Interfaces;
using Pulse.Extensions.Plugin;

namespace Lemoine.Extensions.AutoReason
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public abstract class AutoReasonAutoConfigPlugin<TConfiguration>
    : AutoReasonAutoConfigNoDefaultReasonPlugin<TConfiguration>, IPluginDll, IFlaggedPlugin
    where TConfiguration : AutoReasonConfiguration, new()
  {
    string m_defaultReasonTranslationKey;
    string m_defaultReasonTranslationValue;

    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonAutoConfigPlugin<TConfiguration>).FullName);

    /// <summary>
    /// Constructor with the default reason
    /// </summary>
    /// <param name="defaultReasonTranslationKey">not null or empty</param>
    /// <param name="defaultReasonTranslationValue">not null or empty</param>
    protected AutoReasonAutoConfigPlugin (string defaultReasonTranslationKey, string defaultReasonTranslationValue)
      : base ()
    {
      m_defaultReasonTranslationKey = defaultReasonTranslationKey;
      m_defaultReasonTranslationValue = defaultReasonTranslationValue;
    }

    /// <summary>
    /// Default constructor to use when no default reason must be set on the system
    /// </summary>
    protected AutoReasonAutoConfigPlugin ()
    {
    }

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
  }
}
