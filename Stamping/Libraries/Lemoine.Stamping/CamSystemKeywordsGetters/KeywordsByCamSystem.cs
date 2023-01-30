// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Stamping.CamSystemKeywordsGetters
{
  /// <summary>
  /// KeywordsByCamSystem
  /// </summary>
  public class KeywordsByCamSystem : ICamSystemKeywordsGetter
  {
    readonly ILog log = LogManager.GetLogger (typeof (KeywordsByCamSystem).FullName);

    /// <summary>
    /// List of data by cam system
    /// </summary>
    public IList<CamSystemKeywordsGetter> DataByCamSystem { get; set; } = new List<CamSystemKeywordsGetter> ();

    CamSystemKeywordsGetter? Get (string? camSystem) =>
      camSystem switch {
        null => null,
        _ => this.DataByCamSystem.FirstOrDefault (x => string.Equals (x.CamSystem, camSystem, StringComparison.CurrentCultureIgnoreCase))
      };

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetCadName (string? camSystem) => Get (camSystem)?.CadName ?? "";

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetComponentName (string? camSystem) => Get (camSystem)?.ComponentName ?? "";

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetDepth (string? camSystem) => Get (camSystem)?.Depth ?? "";

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetMinimumToolLength (string? camSystem) => Get (camSystem)?.MinimumToolLength ?? "";

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetOperationDurationHHMMSS (string? camSystem) => Get (camSystem)?.OperationDurationHHMMSS ?? "";

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetProgrammedFeedrate (string? camSystem) => Get (camSystem)?.ProgrammedFeedrate ?? "";

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetProgrammedSpindleSpeed (string? camSystem) => Get (camSystem)?.ProgrammedSpindleSpeed ?? "";

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetProjectName (string? camSystem) => Get (camSystem)?.ProjectName ?? "";

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetSequenceDurationHHMMSS (string? camSystem) => Get (camSystem)?.SequenceDurationHHMMSS ?? "";

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetStock (string? camSystem) => Get (camSystem)?.Stock ?? "";

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetStrategy (string? camSystem) => Get (camSystem)?.Strategy ?? "";

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetTolerance (string? camSystem) => Get (camSystem)?.Tolerance ?? "";

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetToolCode (string? camSystem) => Get (camSystem)?.ToolCode ?? "";

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetToolDiameter (string? camSystem) => Get (camSystem)?.ToolDiameter ?? "";

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetToolName (string? camSystem) => Get (camSystem)?.ToolName ?? "";

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetToolRadius (string? camSystem) => Get (camSystem)?.ToolRadius ?? "";

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetWidth (string? camSystem) => Get (camSystem)?.Width ?? "";


  }
}
