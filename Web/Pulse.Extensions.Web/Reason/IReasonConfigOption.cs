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
    /// Value of the option, not null and not empty
    ///
    /// An option is turned on by being carried, always with a value: a reason that does not
    /// carry it does not have it. An implementation refuses an option with no value
    ///
    /// One option carries one value: an option that would need several of them declares
    /// several options instead
    /// </summary>
    string Value { get; }
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
    ///
    /// Empty when the accepted values are not listed
    /// </summary>
    IEnumerable<string> Values { get; }

    /// <summary>
    /// The option may be set at the creation of a reason
    ///
    /// When false, the option is only declared so that a configuration screen may present it
    /// on the reasons that carry it, these reasons being configured another way (manually for
    /// example). A creation with this option is refused
    /// </summary>
    bool Creatable { get; }
  }
}
