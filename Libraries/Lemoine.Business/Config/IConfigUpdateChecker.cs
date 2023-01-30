// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Business.Config
{
  /// <summary>
  /// Interface for classes that check there is no config update
  /// 
  /// Check there was no update of the config.
  /// The method <see cref="Lemoine.Threading.IAdditionalChecker.Check"/> returns true in case the condition could not be checked
  /// </summary>
  public interface IConfigUpdateChecker: Lemoine.Threading.IAdditionalChecker
  {
    /// <summary>
    /// Check there was no update of the config (without considering any timer)
    /// 
    /// true is returned in case the condition could not be checked
    /// </summary>
    /// <returns></returns>
    bool CheckNoConfigUpdate ();
  }
}
