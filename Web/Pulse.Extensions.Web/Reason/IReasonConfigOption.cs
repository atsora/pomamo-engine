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
  ///
  /// The texts (<see cref="Display"/>, <see cref="Description"/> and those of the choices) are
  /// in the UI culture of the request (<see cref="System.Globalization.CultureInfo.CurrentUICulture"/>),
  /// evaluated when they are read: an implementation translates them there, and does not keep
  /// them from a previous read
  /// </summary>
  public interface IReasonConfigAvailableOption
  {
    /// <summary>
    /// Name of the option, which identifies it
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Short label of the option, in the UI culture, not null
    ///
    /// The name when there is no better label
    /// </summary>
    string Display { get; }

    /// <summary>
    /// Help text of the option, in the UI culture
    ///
    /// It does not quote the values, which have their own label: see <see cref="Choices"/>
    ///
    /// nullable
    /// </summary>
    string Description { get; }

    /// <summary>
    /// The values the option accepts, with their labels, never null
    ///
    /// Empty when the accepted values are not listed
    /// </summary>
    IEnumerable<IReasonConfigOptionChoice> Choices { get; }

    /// <summary>
    /// The option may be set at the creation of a reason
    ///
    /// When false, the option is only declared so that a configuration screen may present it
    /// on the reasons that carry it, these reasons being configured another way (manually for
    /// example). A creation with this option is refused
    /// </summary>
    bool Creatable { get; }
  }

  /// <summary>
  /// A value a <see cref="IReasonConfigAvailableOption"/> accepts
  /// </summary>
  public interface IReasonConfigOptionChoice
  {
    /// <summary>
    /// The value, as <see cref="IReasonConfigOption.Value"/> carries it, not null and not empty
    /// </summary>
    string Value { get; }

    /// <summary>
    /// Label of the value, in the UI culture, not null
    ///
    /// For example the name of a machine state template for its id. The value when there is
    /// no better label
    /// </summary>
    string Display { get; }

    /// <summary>
    /// Help text of the value, in the UI culture
    ///
    /// nullable
    /// </summary>
    string Description { get; }
  }
}
