// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lemoine.Stamping
{
  /// <summary>
  /// Interface to define the file flow and what action is taken in case of a stamping success and a stamping failure
  /// </summary>
  public interface IStampingFileFlow
  {
    /// <summary>
    /// Input file path if any
    /// </summary>
    public string InputFilePath { get; }

    /// <summary>
    /// Output file path if any
    /// </summary>
    public string OutputFilePath { get; }

    /// <summary>
    /// Path of the input file of the <see cref="IStamper"/>
    /// </summary>
    public string StamperInputFilePath { get; }

    /// <summary>
    /// Path of the output file of the <see cref="IStamper"/> if applicable
    /// </summary>
    public string StamperOutputFilePath { get; }

    /// <summary>
    /// Asynchronous method that is called on stamping success
    /// </summary>
    /// <returns></returns>
    public Task OnSuccessAsync ();

    /// <summary>
    /// Asynchronous method that is called on stamping failure
    /// </summary>
    /// <returns></returns>
    public Task OnFailureAsync ();
  }
}
