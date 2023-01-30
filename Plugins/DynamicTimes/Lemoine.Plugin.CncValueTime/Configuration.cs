// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.CncValueTime
{
  public sealed class Configuration
    : Pulse.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Name prefix (mandatory)
    /// 
    /// The dynamic time is then suffixed by AfterStart, AfterEnd, BeforeStart or BeforeEnd
    /// </summary>
    [PluginConf ("Text", "Dynamic time prefix", Description = "Prefix to add to the dynamic time names", Optional = false)]
    public string NamePrefix
    {
      get; set;
    }

    /// <summary>
    /// Field (mandatory)
    /// </summary>
    [PluginConf ("Field", "Field", Description = "Field to match", Multiple = false, Optional = false)]
    public int FieldId { get; set; }

    /// <summary>
    /// Lambda condition
    /// </summary>
    [PluginConf ("Text", "Lambda condition", Description = "A lambda expression object => bool to define the condition", Optional = false)]
    public string LambdaCondition { get; set; }

    /// <summary>
    /// Optionally a cache time out
    /// </summary>
    [PluginConf ("DurationPicker", "Cache time out", Description = "Optionally a cache time out", Parameters = "00:00:01", Optional = true)]
    public TimeSpan? CacheTimeOut
    {
      get; set;
    }

    /// <summary>
    /// Optionally an applicable time span
    /// </summary>
    [PluginConf ("DurationPicker", "Applicable time span", Description = "Optionally an applicable time span", Parameters = "1.00:00:00", Optional = true)]
    public TimeSpan? ApplicableTimeSpan
    {
      get; set;
    }
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

      IList<string> errorList = new List<string> ();

      if (string.IsNullOrEmpty (this.NamePrefix)) {
        errorList.Add ("Invalid NamePrefix (empty)");
        result = false;
      }
      if (string.IsNullOrEmpty (this.LambdaCondition)) {
        errorList.Add ("Invalid lambda (empty)");
        result = false;
      }
      if (0 == this.FieldId) {
        errorList.Add ("Invalid field (empty)");
        result = false;
      }
      else {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var field = ModelDAOHelper.DAOFactory.FieldDAO
            .FindById (this.FieldId);
          if (null == field) {
            errorList.Add ($"Field id={this.FieldId} does not exist");
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
