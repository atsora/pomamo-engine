// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Pulse.Extensions.Web.Reason
{
  /// <summary>
  /// A default entry contributed to a reason by a <see cref="IReasonConfigExtension"/>: the
  /// reason is set by default on a machine mode and a machine planned state
  ///
  /// It carries ids and values only, never an entity: it may be cached
  /// </summary>
  public interface IReasonConfigDefault
  {
    /// <summary>
    /// The <see cref="IReasonConfigReason.ClassificationId"/> this entry applies to, not null
    /// </summary>
    string ClassificationId { get; }

    /// <summary>
    /// Id of the machinemodedefaultreason row
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
    /// Maximum duration the default reason applies to
    ///
    /// null: no limitation
    /// </summary>
    TimeSpan? MaxDuration { get; }

    /// <summary>
    /// Reason score
    /// </summary>
    double Score { get; }

    /// <summary>
    /// Id of the machine filter the entry is restricted to
    ///
    /// null: every machine applies
    /// </summary>
    int? MachineFilterId { get; }
  }
}
