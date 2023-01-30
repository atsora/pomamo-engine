// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Lemoine.Core.Log;

namespace Lemoine.Stamping.DataInterpreters
{
  /// <summary>
  /// RegexInterpreter
  /// </summary>
  public class RegexInterpreter : IDataInterpreter
  {
    readonly ILog log = LogManager.GetLogger (typeof (RegexInterpreter).FullName);

    Regex? m_regex = null;

    /// <summary>
    /// Source key, for example CadName or FileName
    /// </summary>
    public string Source
    {
      get; set;
    } = "";

    /// <summary>
    /// Regex
    /// 
    /// If empty, consider the config value Stamping.Regex.{Source} instead
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull)]
    public string Regex
    {
      get => m_regex?.ToString () ?? "";
      set {
        if (string.IsNullOrEmpty (value)) {
          m_regex = null;
        }
        else {
          m_regex = new Regex (value, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
      }
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public RegexInterpreter ()
    {
      this.Source = "";
      this.Regex = "";
    }

    /// <summary>
    /// Constructor
    /// </summary>
    [JsonConstructor]
    public RegexInterpreter (string source)
    {
      this.Source = source;
      this.Regex = "";
    }

    /// <summary>
    /// <see cref="IDataInterpreter"/>
    /// </summary>
    /// <param name="stampingData"></param>
    /// <returns></returns>
    public virtual bool Interpret (StampingData stampingData)
    {
      if (string.IsNullOrEmpty (this.Source)) {
        log.Error ($"Interpret: Source is null or empty");
        throw new InvalidOperationException ("Source is not set");
      }

      Regex regex;
      if (m_regex is not null && !string.IsNullOrEmpty (this.Regex)) {
        regex = m_regex;
      }
      else { // Try to get it from ConfigSet
        var regexConfigKey = $"Stamping.Regex.{this.Source}";
        string regexConfigValue;
        try {
          regexConfigValue = Lemoine.Info.ConfigSet.Get<string> (regexConfigKey);
        }
        catch (Lemoine.Info.ConfigKeyNotFoundException ex) {
          log.Error ($"Interpret: no config key {regexConfigKey} was set, give up", ex);
          return false;
        }
        regex = new Regex (regexConfigValue, RegexOptions.Compiled | RegexOptions.IgnoreCase);
      }

      if (regex is null || string.IsNullOrEmpty (regex.ToString ())) {
        log.Error ($"Interpret: Regex is null or empty");
        throw new InvalidOperationException ("Regex is not set");
      }

      if (!stampingData.TryGet<string> (this.Source, out string? data)) {
        log.Error ($"Interpret: {this.Source} is not in stampingData");
        return false;
      }
      if (data is null || string.IsNullOrWhiteSpace (data)) {
        log.Error ($"Interpret: {this.Source}={data} is null or white space only");
        return false;
      }

      var match = regex.Match (data);
      if (!match.Success) {
        log.Error ($"Interpret: {data} did not match {m_regex}");
        return false;
      }
      foreach (Group group in match.Groups) {
        if (!int.TryParse (group.Name, out _)) {
          if (log.IsInfoEnabled) {
            log.Info ($"Interpret: add {group.Name}={group.Value}");
          }
          stampingData.Add (group.Name, group.Value);
        }
      }
      return true;
    }
  }
}
