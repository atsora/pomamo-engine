// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// DTO for ReasonSource
  /// </summary>
  public class ReasonSourceDTO
  {
    /// <summary>
    /// Default flag
    /// </summary>
    public bool Default { get; set; }

    /// <summary>
    /// Auto flag
    /// </summary>
    public bool Auto { get; set; }

    /// <summary>
    /// Manual flag
    /// </summary>
    public bool Manual { get; set; }

    /// <summary>
    /// The auto-number is unsafe
    /// </summary>
    public bool UnsafeAutoReasonNumber { get; set; }

    /// <summary>
    /// The extra manual flag is unsafe
    /// </summary>
    public bool UnsafeManualFlag { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public ReasonSourceDTO (ReasonSource reasonSource)
    {
      this.Default = reasonSource.HasFlag (ReasonSource.Default);
      this.Auto = reasonSource.HasFlag (ReasonSource.Auto);
      this.Manual = reasonSource.HasFlag (ReasonSource.Manual);
      this.UnsafeAutoReasonNumber = reasonSource.HasFlag (ReasonSource.UnsafeAutoReasonNumber);
      this.UnsafeManualFlag = reasonSource.HasFlag (ReasonSource.UnsafeManualFlag);
    }
  }
}
