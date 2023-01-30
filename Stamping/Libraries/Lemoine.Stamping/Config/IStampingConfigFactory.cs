// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.Stamping.Config
{
  /// <summary>
  /// Interface to implement to create a StampingConfig
  /// </summary>
  public interface IStampingConfigFactory
  {
    /// <summary>
    /// Create a stamping config
    /// </summary>
    /// <returns></returns>
    StampingConfig CreateStampingConfig ();
  }
}
