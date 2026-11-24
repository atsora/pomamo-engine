// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.Model
{
  /// <summary>
  /// Category of a non conformance reason (column nonconformancereasoncategory)
  /// </summary>
  public enum NonConformanceReasonCategory
  {
    /// <summary>
    /// The part is scrapped (default)
    /// </summary>
    Scrap = 1,
    /// <summary>
    /// The part can be fixed (re-machined...)
    /// </summary>
    Fixable = 2,
  }
}
