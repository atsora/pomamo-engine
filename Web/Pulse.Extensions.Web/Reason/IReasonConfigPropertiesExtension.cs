// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Pulse.Extensions.Web.Reason
{
  /// <summary>
  /// Extension that tells which properties of a reason and of a reason group the
  /// configuration exposes to a client
  ///
  /// It is deliberately not a capability of <see cref="IReasonConfigExtension"/>: what an
  /// implementation is able to write and what a deployment wants to put in front of a user
  /// are two different things. A site that only lets its users rename a reason and change
  /// its color exposes the name and the color, whatever the extensions can do
  ///
  /// A caller takes the union of what the extensions expose
  /// </summary>
  public interface IReasonConfigPropertiesExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialize the extension. Return true if the extension is active, else false
    /// </summary>
    /// <returns>the extension is active</returns>
    bool Initialize ();

    /// <summary>
    /// The properties of a reason to expose, for example name, detailsRequired, options
    /// </summary>
    IEnumerable<string> ReasonProperties { get; }

    /// <summary>
    /// The properties of a reason group to expose, for example name, color
    /// </summary>
    IEnumerable<string> ReasonGroupProperties { get; }
  }
}
