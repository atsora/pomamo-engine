// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Properties that characterizes a plugin
  /// </summary>
  [Flags]
  public enum PluginFlag
  {
    /// <summary>
    /// The plugin is applicable on all the services
    /// </summary>
    All = int.MaxValue,

    /// <summary>
    /// The plugin is applicable on none of the services (neither loaded)
    /// 
    /// To be used as a filter when all the plugins must be loaded
    /// </summary>
    None = 0,

    /// <summary>
    /// Load the plugin even in case of no database connection
    /// </summary>
    NHibernateExtension = 1,

    /// <summary>
    /// The plugin is applicable on the alert service (events)
    /// </summary>
    Alert = 2,
    /// <summary>
    /// The plugin is applicable on the analysis service
    /// </summary>
    Analysis = 4,
    /// <summary>
    /// The plugin is applicable on the auto-reason service
    /// </summary>
    AutoReason = 8,
    /// <summary>
    /// The plugin is applicable on the web service
    /// </summary>
    Web = 16,
    /// <summary>
    /// The plugin is applicable on the cnc service
    /// </summary>
    Cnc = 32,
    /// <summary>
    /// The plugin is applicable on the cnc data service
    /// </summary>
    CncData = 64,
    /// <summary>
    /// The plugin is applicable in the operation explorer application
    /// </summary>
    OperationExplorer = 128,
    /// <summary>
    /// The plugin is applicable in the configuration applications (Lem_Settings, Lem_Configuration)
    /// </summary>
    Config = 256,

    /// <summary>
    /// This plugin implements an event
    /// (applicable to the alert service and without a database connection because of the NHibernateExtension)
    /// </summary>
    Event = NHibernateExtension | Alert,
  }
  
  /// <summary>
     /// Description of IPlugin.
     /// </summary>
  public interface IPlugin : IVersionable
  {
    /// <summary>
    /// Identifying Name
    /// </summary>
    string IdentifyingName { get; }

    /// <summary>
    /// Version of the plugin
    /// </summary>
    int NumVersion { get; set; }

    /// <summary>
    /// Plugin flag if available
    /// </summary>
    PluginFlag? Flag { get; set; }
  }
}
