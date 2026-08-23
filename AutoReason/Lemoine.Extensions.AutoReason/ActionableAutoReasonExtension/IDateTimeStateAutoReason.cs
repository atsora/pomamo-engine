// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.AutoReason.ActionableAutoReasonExtension
{
  /// <summary>
  /// Auto-reason with a date/time state
  /// </summary>
  public interface IDateTimeStateAutoReason
    : IDateTimeStateAutoExtension
    , IAutoReasonExtension
  {
  }
}
