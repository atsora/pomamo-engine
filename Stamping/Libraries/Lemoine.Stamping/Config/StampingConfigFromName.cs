// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Stamping.Config
{
  /// <summary>
  /// StampingConfigFromName
  /// </summary>
  public class StampingConfigFromName: IStampingConfigFactory
  {
    readonly ILog log = LogManager.GetLogger (typeof (StampingConfigFromName).FullName);

    /// <summary>
    /// Name of the stamping config
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    public StampingConfigFromName (string name)
    {
      this.Name = name;
    }

    /// <summary>
    /// <see cref="IStampingConfigFactory"/>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="Exception"></exception>
    /// <exception cref="NullReferenceException"></exception>
    public StampingConfig CreateStampingConfig ()
    {
      var name = this.Name;

      try {
        var connectionString = Lemoine.Info.GDBConnectionParameters.GetGDBConnectionString ("Lemoine.Stamping");

        // Check name against SQL injection
        var validationRegex = new Regex (@"^[-_A-Za-z0-9]+$");
        if (!validationRegex.IsMatch (name)) {
          log.Error ($"CreateStampingConfig: name={name} is not valid");
          throw new ArgumentException ("Name with invalid characters", "name");
        }
        var query = $@"select stampingconfig::text from stampingconfigbyname where stampingconfigname = '{name}';";
        var json = Lemoine.GDBUtils.ConnectionTools.ExecuteScalar<string> (connectionString, query);
        if (json is null) {
          log.Error ($"CreateStampingConfig: there is no stampingconfig with name {name}");
          throw new Exception ($"No stamping config with name {name}");
        }
        try {
          var stampingConfig = System.Text.Json.JsonSerializer.Deserialize<StampingConfig> (json);
          if (stampingConfig is null) {
            throw new NullReferenceException ("stampingConfig is null");
          }
          else {
            return stampingConfig;
          }
        }
        catch (Exception ex) {
          log.Error ($"CreateStampingConfig: invalid json={json}", ex);
          throw;
        }
      }
      catch (Exception ex) {
        log.Error ("CreateStampingConfig: exception", ex);
        throw;
      }
    }
  }
}
