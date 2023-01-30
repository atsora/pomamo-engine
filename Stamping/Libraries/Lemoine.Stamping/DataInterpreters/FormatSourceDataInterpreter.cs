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
  /// <see cref="IDataInterpreter"/> implementation formatting the source data
  /// </summary>
  public class FormatSourceDataInterpreter : IDataInterpreter
  {
    readonly ILog log = LogManager.GetLogger (typeof (FormatSourceDataInterpreter).FullName);

    /// <summary>
    /// Source key, for example CadName or FileName
    /// </summary>
    public string Source
    {
      get; set;
    } = "";

    /// <summary>
    /// Target key, for example JobName
    /// </summary>
    public string Target
    {
      get; set;
    } = "";

    /// <summary>
    /// How to format the source data
    /// </summary>
    public string Format
    {
      get; set;
    } = "?{0}?";

    /// <summary>
    /// Default constructor
    /// </summary>
    public FormatSourceDataInterpreter ()
    {
      this.Source = "";
    }

    /// <summary>
    /// Constructor
    /// </summary>
    [JsonConstructor]
    public FormatSourceDataInterpreter (string source)
    {
      this.Source = source;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public FormatSourceDataInterpreter (string source, string target)
    {
      this.Source = "";
      this.Target = target;
    }


    /// <summary>
    /// <see cref="IDataInterpreter"/>
    /// </summary>
    /// <param name="stampingData"></param>
    /// <returns></returns>
    public bool Interpret (StampingData stampingData)
    {
      if (string.IsNullOrEmpty (this.Source)) {
        log.Error ($"Interpret: source is null or empty => return false");
        return false;
      }
      if (string.IsNullOrEmpty (this.Target)) {
        log.Error ($"Interpret: target is null or empty => return false");
        return false;
      }
      if (string.IsNullOrEmpty (this.Format)) {
        log.Error ($"Interpret: format is empty for key={this.Target}");
        return false;
      }
      if (!stampingData.TryGet<string> (this.Source, out string? sourceData)) {
        log.Error ($"Interpret: {this.Source} is not in stampingData");
        return false;
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"Interpret: source={this.Source} format={this.Format} source={sourceData}");
      }

      stampingData.Add (this.Target, string.Format (this.Format, sourceData));
      return true;
    }
  }
}
