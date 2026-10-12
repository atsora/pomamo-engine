// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Pulse.Extensions.Web.Reason
{
  /// <summary>
  /// Names of the properties of a <see cref="IReasonConfigReason"/>, the way the schema names
  /// them
  ///
  /// They are what <see cref="IReasonConfigReason.ReadOnlyProperties"/> lists, and what an
  /// update passes to <see cref="IReasonConfigExtension.UpdateReasonAsync"/> to say which
  /// properties the client provided
  /// </summary>
  public static class ReasonConfigPropertyNames
  {
    /// <summary>
    /// <see cref="IReasonConfigReason.Name"/>
    /// </summary>
    public const string NAME = "name";

    /// <summary>
    /// <see cref="IReasonConfigReason.Code"/>
    /// </summary>
    public const string CODE = "code";

    /// <summary>
    /// <see cref="IReasonConfigReason.Description"/>
    /// </summary>
    public const string DESCRIPTION = "description";

    /// <summary>
    /// <see cref="IReasonConfigReason.Color"/>
    /// </summary>
    public const string COLOR = "color";

    /// <summary>
    /// <see cref="IReasonConfigReason.ReportColor"/>
    /// </summary>
    public const string REPORT_COLOR = "reportColor";

    /// <summary>
    /// <see cref="IReasonConfigReason.ReasonGroupId"/>
    /// </summary>
    public const string REASON_GROUP = "reasonGroup";

    /// <summary>
    /// <see cref="IReasonConfigReason.LinkDirection"/>
    /// </summary>
    public const string LINK_DIRECTION = "linkDirection";

    /// <summary>
    /// <see cref="IReasonConfigReason.DisplayPriority"/>
    /// </summary>
    public const string DISPLAY_PRIORITY = "displayPriority";

    /// <summary>
    /// <see cref="IReasonConfigReason.ProductionStateId"/>
    /// </summary>
    public const string PRODUCTION_STATE = "productionState";

    /// <summary>
    /// <see cref="IReasonConfigReason.Planned"/>
    /// </summary>
    public const string PLANNED = "planned";

    /// <summary>
    /// <see cref="IReasonConfigReason.DetailsRequired"/>
    /// </summary>
    public const string DETAILS_REQUIRED = "detailsRequired";

    /// <summary>
    /// <see cref="IReasonConfigReason.AlwaysSecondLevel"/>
    /// </summary>
    public const string ALWAYS_SECOND_LEVEL = "alwaysSecondLevel";

    /// <summary>
    /// <see cref="IReasonConfigReason.MaxDuration"/>
    /// </summary>
    public const string MAX_DURATION = "maxDuration";

    /// <summary>
    /// <see cref="IReasonConfigReason.Options"/>
    /// </summary>
    public const string OPTIONS = "options";
  }
}
