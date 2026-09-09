// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Pulse.Extensions.Web.Reason
{
  /// <summary>
  /// A selection entry contributed to a reason by a <see cref="IReasonConfigExtension"/>:
  /// the reason may be selected on a machine mode and a machine planned state
  ///
  /// It carries ids and values only, never an entity: it may be cached
  /// </summary>
  public interface IReasonConfigSelection
  {
    /// <summary>
    /// The <see cref="IReasonConfigReason.ClassificationId"/> this entry applies to, not null
    /// </summary>
    string ClassificationId { get; }

    /// <summary>
    /// Id of the reasonselection row
    ///
    /// null when the entry comes from a plugin rather than from the table
    /// </summary>
    int? Id { get; }

    /// <summary>
    /// Name of the plugin the entry comes from
    ///
    /// null when the entry comes from a table
    /// </summary>
    string Plugin { get; }

    /// <summary>
    /// Id of the machine mode
    ///
    /// null: every machine mode applies, which a plugin that is not restricted to any of
    /// them returns
    /// </summary>
    int? MachineModeId { get; }

    /// <summary>
    /// Id of the machine planned state (= machine observation state)
    ///
    /// null: every machine planned state applies
    /// </summary>
    int? MachinePlannedStateId { get; }

    /// <summary>
    /// Must the operator enter a free detailed entry along with the reason?
    /// </summary>
    bool DetailsRequired { get; }

    /// <summary>
    /// Are additional details refused?
    /// </summary>
    bool? NoDetails { get; }

    /// <summary>
    /// Is the reason displayed in the second level, under its reason group?
    /// </summary>
    bool? AlwaysSecondLevel { get; }

    /// <summary>
    /// Id of the machine filter the entry is restricted to
    ///
    /// null: every machine applies
    /// </summary>
    int? MachineFilterId { get; }
  }
}
