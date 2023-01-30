// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.AutoReason.ActionableAutoReasonExtension
{
  /// <summary>
  /// Auto-reason with a date/time state
  /// </summary>
  public interface IDateTimeStateAutoReason: IAutoReasonExtension
  {
    /// <summary>
    /// Value of the date/time state
    /// </summary>
    DateTime DateTime { get; }

    /// <summary>
    /// Update the date/time
    /// </summary>
    /// <param name="dateTime"></param>
    void UpdateDateTime (DateTime dateTime);

    /// <summary>
    /// Reset date/time (in case of a failure)
    /// </summary>
    /// <param name="dateTime"></param>
    void ResetDateTime (DateTime dateTime);
  }
}
