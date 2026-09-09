// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Pulse.Extensions.Web.Reason
{
  /// <summary>
  /// An additional option a reason carries
  ///
  /// Options are read only: an extension reads them and never writes them. They are what
  /// tells which implementation a reason belongs to - an extension does not necessarily
  /// read every option that is declared
  /// </summary>
  public interface IReasonConfigOption
  {
    /// <summary>
    /// Name of the option, which identifies it
    ///
    /// See <see cref="IReasonConfigAvailableOption.Name"/>
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Values of the option, never null
    /// </summary>
    IEnumerable<string> Values { get; }
  }

  /// <summary>
  /// An option a <see cref="IReasonConfigExtension"/> declares, so that a configuration
  /// screen may propose it without knowing the extension
  ///
  /// Any extension may declare a new one; whether an implementation reads it is up to it
  /// </summary>
  public interface IReasonConfigAvailableOption
  {
    /// <summary>
    /// Name of the option, which identifies it
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Description of the option
    ///
    /// nullable
    /// </summary>
    string Description { get; }

    /// <summary>
    /// The values the option accepts, never null
    /// </summary>
    IEnumerable<string> Values { get; }
  }
}
