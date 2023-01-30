// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Extensions.Database
{
  /// <summary>
  /// Extension to XML serialization
  /// </summary>
  public interface IXmlSerializationExtension: Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Get extra types for XML Serialization
    /// </summary>
    /// <returns></returns>
    Type[] GetExtraTypes ();
  }
}
