// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table Service
  /// </summary>
  public interface IService: IUpdater, IDataWithIdentifiers
  {
    /// <summary>
    /// Service name
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Service program name (case insensitive)
    /// </summary>
    string Program { get; set; }
    
    /// <summary>
    /// Is the service a Lemoine service ?
    /// </summary>
    bool Lemoine { get; set; }
    
    /// <summary>
    /// Computer on which the service is installed
    /// </summary>
    IComputer Computer { get; set; }
  }
}
