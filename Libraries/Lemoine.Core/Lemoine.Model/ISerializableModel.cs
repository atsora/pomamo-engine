// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Interface to make a model serializable.
  /// 
  /// It includes a few methods for example to unproxy some properties
  /// </summary>
  public interface ISerializableModel
  {
    /// <summary>
    /// Unproxy all the properties
    /// </summary>
    void Unproxy();
  }
}
