// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Stamping.StampLineCreators
{
  /// <summary>
  /// <see cref="ILineFormatter"/> implementation using string.Format
  /// </summary>
  public class StringFormatLineFormatter: ILineFormatter
  {
    readonly ILog log = LogManager.GetLogger (typeof (StringFormatLineFormatter).FullName);

    /// <summary>
    /// Is the variable optional ?
    /// </summary>
    public bool OptionalVariable { get; set; } = false;

    /// <summary>
    /// Format to use
    /// 
    /// {0} is replaced by the variable and {1} by the value (stampId)
    /// </summary>
    public string Format { get; set; } = "";

    /// <summary>
    /// Maximum number of fractional digits to write into the program
    /// 
    /// Default: 5
    /// </summary>
    public int FractionalDigits { get; set; } = 5;

    /// <summary>
    /// <see cref="ILineFormatter"/>
    /// </summary>
    /// <param name="variable"></param>
    /// <param name="stampId"></param>
    /// <returns></returns>
    public string CreateLine (string variable, object v)
    {
      if (string.IsNullOrWhiteSpace (this.Format)) {
        log.Error ($"CreateLine: Format={this.Format} is invalid");
        throw new Exception ("Invalid line format");
      }

      if (v is double d) { // Use an invariant culture for a double
        d = Math.Round (d, this.FractionalDigits);
        return CreateLine (variable, d.ToString (System.Globalization.CultureInfo.InvariantCulture));
      }

      return string.Format (this.Format, variable, v);
    }
  }
}
