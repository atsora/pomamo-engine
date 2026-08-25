// Copyright (C) 2026 Atsora Solutions

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.AutoReason
{
  /// <summary>
  /// Auto machine state template extension that collects some delayed actions before applying them
  /// 
  /// See <see cref="ActionableAutoExtensions"/> for the methods that process the delayed actions
  /// </summary>
  public interface IActionableAutoStateTemplate
    : IActionableAutoExtension
    , IAutoStateTemplateExtension
  {
  }
}
