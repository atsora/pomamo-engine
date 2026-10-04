// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Pomamo.CncModule
{
  /// <summary>
  /// Time-stamped set of data that was acquired by the modules with the attribute frequency="high"
  ///
  /// Immutable once created
  /// </summary>
  public sealed class HighFrequencySample
  {
    /// <summary>
    /// UTC date/time of the acquisition
    /// </summary>
    public DateTime DateTime { get; }

    /// <summary>
    /// Acquired data: key => value
    /// </summary>
    public IDictionary<string, object> Data { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dateTime">UTC date/time of the acquisition</param>
    /// <param name="data">not null</param>
    public HighFrequencySample (DateTime dateTime, IDictionary<string, object> data)
    {
      this.DateTime = dateTime;
      this.Data = data ?? throw new ArgumentNullException (nameof (data));
    }

    /// <summary>
    /// <see cref="Object.ToString"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString () => $"{this.DateTime:o}:{this.Data.Count} values";
  }
}
