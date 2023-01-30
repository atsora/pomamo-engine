// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;
using Lemoine.Extensions.Plugin;
using Lemoine.Model;
using Pulse.Extensions.Plugin;

namespace Lemoine.Extensions.AutoReason
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public abstract class AutoReasonAutoConfigNoDefaultReasonPlugin<TConfiguration>
    : PluginWithAutoConfig<TConfiguration>, IPluginDll, IFlaggedPlugin
    where TConfiguration : AutoReasonConfiguration, new()
  {
    #region Members
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Applicable flags
    /// </summary>
    public virtual PluginFlag Flags => PluginFlag.AutoReason | PluginFlag.Analysis | PluginFlag.Web;

    /// <summary>
    /// Multiple configurations
    /// </summary>
    public override bool MultipleConfigurations => true;

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version { get { return 1; } }
    #endregion // Getters / Setters

    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonAutoConfigNoDefaultReasonPlugin<TConfiguration>).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    protected AutoReasonAutoConfigNoDefaultReasonPlugin ()
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
        CompleteInstall ();
        break;
      default:
        throw new InvalidOperationException ();
      }
    }

    /// <summary>
    /// Complete the installation if needed
    /// </summary>
    protected virtual void CompleteInstall () { }
    #endregion // Methods
  }
}
