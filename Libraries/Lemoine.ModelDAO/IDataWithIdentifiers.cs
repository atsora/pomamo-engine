// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// DataWithIdentifiers model interface
  /// 
  /// A DataWithIndentifiers contains a list of identifiers
  /// that may be to retrieve a specific item
  /// </summary>
  public interface IDataWithIdentifiers
  {
    /// <summary>
    /// Default list of possible identifiers
    /// </summary>
    string[] Identifiers { get; }
  }
}
