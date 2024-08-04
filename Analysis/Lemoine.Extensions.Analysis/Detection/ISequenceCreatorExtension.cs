// Copyright (C) 2024 Atsora Solutions

using Lemoine.Model;
using Pulse.Extensions.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Lemoine.Extensions.Analysis.Detection
{
  /// <summary>
  /// Extension to create sequences
  /// </summary>
  public interface ISequenceCreatorExtension
    : Lemoine.Extensions.IExtension
    , IInitializedByMachineExtension
  {
    /// <summary>
    /// Priority
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Create the sequences
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="match"></param>
    void CreateSequences (IOperation operation, Match match);
  }
}
