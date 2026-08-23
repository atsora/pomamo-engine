// Copyright (C) 2026 Atsora Solutions

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.AutoReason.ActionableAutoReasonExtension
{
  /// <summary>
  /// Extension of the auto-reason service with a date/time state
  /// </summary>
  public interface IDateTimeStateAutoExtension
    : Lemoine.Extensions.IExtension
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
