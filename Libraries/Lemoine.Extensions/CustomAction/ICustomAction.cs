// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;

using System.Collections.Generic;

namespace Lemoine.Extensions.CustomAction
{
  /// <summary>
  /// Basic interface for a custom action with a specific configuration
  /// </summary>
  public interface ICustomAction<IConfiguration>
  {
    /// <summary>
    /// Help message
    /// </summary>
    string Help { get; }

    /// <summary>
    /// Action done when validated
    /// This method is NOT called within a transaction
    /// </summary>
    /// <param name="configurations"></param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revision">Revision that is going to be applied when the function returns</param>
    void DoAction (IEnumerable<IConfiguration> configurations, ref IList<string> warnings, ref int revisionId);
  }

  /// <summary>
  /// Basic interface for a custom action with no specific configuration
  /// </summary>
  public interface ICustomAction: ICustomAction<Configuration.Implementation.EmptyConfiguration>
  {
  }
}
