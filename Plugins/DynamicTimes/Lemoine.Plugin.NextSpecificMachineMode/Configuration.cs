// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.NextSpecificMachineMode
{
  public sealed class Configuration
    : Pulse.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Name prefix (mandatory)
    /// </summary>
    [PluginConf ("Text", "Dynamic time name", Description = "Name of the dynamic time (mandatory)", Optional = false)]
    public string Name
    {
      get; set;
    }

    [PluginConf ("MachineMode", "End machine modes", Description = "List of end machine modes that trigger the dynamic end", Multiple = true, Optional = false)]
    public IEnumerable<int> EndMachineModeIds
    {
      get; set;
    } = new List<int> ();

    [PluginConf ("MachineMode", "Cancel machine modes", Description = "List of machine modes that cancel the dynamic end (then NoData is returned)", Multiple = true, Optional = true)]
    public IEnumerable<int> CancelMachineModeIds
    {
      get; set;
    } = new List<int> ();

    [PluginConf ("MachineMode", "Start-up machine modes", Description = "List of machine modes that define the start-up phase. The start-up phase is the period when the cancel machine modes are ignored", Multiple = true, Optional = true)]
    public IEnumerable<int> StartUpMachineModeIds
    {
      get; set;
    } = new List<int> ();
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration ()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public override bool IsValid (out IEnumerable<string> errors)
    {
      bool result = base.IsValid (out errors);

      var errorList = new List<string> ();

      if (string.IsNullOrEmpty (this.Name)) {
        errorList.Add ("No name defined");
        result = false;
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        if (!this.EndMachineModeIds.Any ()) {
          errorList.Add ("No end machine mode was set, although at least one is mandatory");
          result = false;
        }
        else {
          foreach (var endMachineModeId in this.EndMachineModeIds) {
            var endMachineMode = ModelDAOHelper.DAOFactory.MachineModeDAO
              .FindById (endMachineModeId);
            if (null == endMachineMode) {
              errorList.Add ($"End machine mode with id {endMachineModeId} does not exist");
              result = false;
            }
          }
        }
        foreach (var cancelMachineModeId in this.CancelMachineModeIds) {
          var cancelMachineMode = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById (cancelMachineModeId);
          if (null == cancelMachineMode) {
            errorList.Add ($"Cancel machine mode with id {cancelMachineModeId} does not exist");
            result = false;
          }
        }
        foreach (var startUpMachineModeId in this.EndMachineModeIds) {
          var startUpMachineMode = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById (startUpMachineModeId);
          if (null == startUpMachineMode) {
            errorList.Add ($"Start-up machine mode with id {startUpMachineModeId} does not exist");
            result = false;
          }
        }
      }

      errors = errors.Concat (errorList);
      return result;
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="Lemoine.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter"/>
    /// </summary>
    /// <returns></returns>
    protected override bool IsMachineFilterRequired ()
    {
      return false;
    }
  }
}
