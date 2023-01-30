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
using Npgsql;

namespace Lemoine.Stamping.DataInterpreters
{
  /// <summary>
  /// Try to interpret some data using a regex.
  /// In case the regex is not valid, use the input format.
  /// 
  /// Implements <see cref="IDataInterpreter"/>
  /// </summary>
  public class RegexFallbackInterpreter: IDataInterpreter
  {
    readonly ILog log = LogManager.GetLogger (typeof (RegexFallbackInterpreter).FullName);

    readonly RegexInterpreter m_regexInterpreter = new RegexInterpreter ();
    IEnumerable<string>? m_targets = null;

    /// <summary>
    /// Source key, for example CadName or FileName
    /// </summary>
    public string Source
    {
      get => m_regexInterpreter.Source;
      set { m_regexInterpreter.Source = value; }
    }

    /// <summary>
    /// Associated regex
    /// 
    /// <see cref="RegexInterpreter"/>
    /// </summary>
    [JsonIgnore (Condition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull)]
    public string Regex
    {
      get { return m_regexInterpreter.Regex; }
      set { m_regexInterpreter.Regex = value; m_targets = null; }
    }

    /// <summary>
    /// How to format the source data in case the regex fails
    /// </summary>
    [JsonIgnore (Condition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull)]
    public string Format {
      get; set;
    } = "?{0}?";

    /// <summary>
    /// Constructor with no data 
    /// </summary>
    public RegexFallbackInterpreter ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    [JsonConstructor]
    public RegexFallbackInterpreter (string source)
    {
      this.Source = source;
    }

    /// <summary>
    /// <see cref="IDataInterpreter"/>
    /// </summary>
    /// <param name="stampingData"></param>
    /// <returns></returns>
    public bool Interpret (StampingData stampingData)
    {
      if (m_regexInterpreter.Interpret (stampingData)) {
        return true;
      }
      else {
        if (m_targets is null) {
          var targets = new List<string> ();
          var targetRegex = new Regex (@"\?<(\w+)>");
          var targetMatches = targetRegex.Matches (this.Regex);
          foreach (Match targetMatch in targetMatches) {
            var target = targetMatch.Groups[1].Value;
            if (log.IsDebugEnabled) {
              log.Debug ($"Interpret: extract target={target} from regex={this.Regex}");
            }
            targets.Add (target);
          }
          m_targets = targets;
        }
        bool anySuccess = false;
        bool anyError = false;
        foreach (var target in m_targets) {
          var formatSourceDataInterpreter = new FormatSourceDataInterpreter (this.Source) {
            Target = target,
            Format = this.Format
          };
          if (!formatSourceDataInterpreter.Interpret (stampingData)) {
            log.Error ($"Interpret: format data interpreter error with source={this.Source} target={target}");
            anyError = true;
          }
          else {
            anySuccess = true;
          }
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"Interpret: fallback with anySuccess={anySuccess} anyError={anyError}");
        }
        return anySuccess && !anyError;
      }
    }
  }
}
