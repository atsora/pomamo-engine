// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Extensions.Interfaces;
using Lemoine.Extensions.Plugin;
using Lemoine.Model;

namespace Lemoine.Extensions.CustomAction
{
  /// <summary>
  /// AutoCustomActionGuiBuilder
  /// </summary>
  public class AutoCustomActionGuiBuilder<TCustomAction, TConfiguration>
    : AutoGuiBuilderWithConfigurationLoader<TCustomAction, TConfiguration>
    , IPluginCustomActionControl
    , ICustomActionGuiBuilder
    where TCustomAction : IConfiguration, ICustomAction<TConfiguration>, new()
    where TConfiguration : IConfiguration, new()
  {
    readonly ILog log = LogManager.GetLogger (typeof (AutoCustomActionGuiBuilder<TCustomAction, TConfiguration>).FullName);

    readonly IList<IPluginInstance> m_associations = new List<IPluginInstance> ();

    #region Getters / Setters
    /// <summary>
    /// Configuration context
    /// </summary>
    IEnumerable<IPluginInstance> ConfigurationContexts
    {
      get { return m_associations; }
    }

    /// <summary>
    /// Help
    /// </summary>
    public override string Help
    {
      get {
        TCustomAction customAction = GetConfiguration ();
        if (null != customAction) {
          return customAction.Help;
        }
        else {
          log.Error ($"Help.get: customAction is null");
          return base.Help;
        }
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public AutoCustomActionGuiBuilder ()
    {
    }

    /// <summary>
    /// Associated title
    /// </summary>
    public string Title => "Custom action";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="association"></param>
    public void AddConfigurationContext (IPluginInstance association)
    {
      m_associations.Add (association);
    }

    #region IPluginConfigurationControl implementation
    /// <summary>
    /// Run the action
    /// </summary>
    /// <param name="warnings"></param>
    /// <param name="revision"></param>
    public void DoAction (ref IList<string> warnings, ref int revisionId)
    {
      TCustomAction customAction = GetConfiguration ();
      var configurations = LoadConfigurations ();
      customAction.DoAction (configurations, ref warnings, ref revisionId);
    }

    /// <summary>
    /// Load the configurations
    /// </summary>
    IEnumerable<TConfiguration> LoadConfigurations ()
    {
      IList<TConfiguration> configurations = new List<TConfiguration> ();

      foreach (var association in m_associations) {
        string parameters = association.InstanceParameters;
        TConfiguration configuration;
        try {
          configuration = this.ConfigurationLoader.LoadConfiguration (parameters);
          IEnumerable<string> errors;
          if (!configuration.IsValid (out errors)) {
            log.WarnFormat ("LoadConfiguration: " +
                            "the configuration is not valid, skip this instance");
          }
          else {
            configurations.Add (configuration);
          }
        }
        catch (Exception) {
          log.ErrorFormat ("LoadConfiguration: " +
                           "parse error for {0}, skip this instance",
                           parameters);
        }
      }

      return configurations;
    }
    #endregion // IPluginConfigurationControl implementation

    #endregion // Constructors

  }
}
