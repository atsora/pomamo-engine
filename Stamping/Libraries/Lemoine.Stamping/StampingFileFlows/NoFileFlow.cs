// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;

namespace Lemoine.Stamping.StampingFileFlows
{
  /// <summary>
  /// NoFileFlow
  /// </summary>
  public class NoFileFlow: IStampingFileFlow
  {
    readonly ILog log = LogManager.GetLogger (typeof (NoFileFlow).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public NoFileFlow ()
    {
    }

    /// <summary>
    /// <see cref="IStampingFileFlow"/>
    /// </summary>
    public string InputFilePath => "";

    /// <summary>
    /// <see cref="IStampingFileFlow"/>
    /// </summary>
    public string OutputFilePath => "";

    /// <summary>
    /// <see cref=""/>
    /// </summary>
    public string StamperInputFilePath => throw new NotImplementedException ();

    public string StamperOutputFilePath => throw new NotImplementedException ();

    public Task OnFailureAsync ()
    {
      return Task.CompletedTask;
    }

    public Task OnSuccessAsync ()
    {
      return Task.CompletedTask;
    }
  }
}
