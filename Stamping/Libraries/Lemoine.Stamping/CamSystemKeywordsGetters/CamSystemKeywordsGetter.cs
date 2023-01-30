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
  /// Basic <see cref="ICamSystemKeywordsGetter"/> implementation without considering the CAM System
  /// </summary>
  public class CamSystemKeywordsGetter: ICamSystemKeywordsGetter
  {
    readonly ILog log = LogManager.GetLogger (typeof (CamSystemKeywordsGetter).FullName);

    /// <summary>
    /// Optional field that can be used to match a given cam system
    /// </summary>
    public string CamSystem { get; set; } = "";

    /// <summary>
    /// CAD Name keyword
    /// </summary>
    public string CadName { get; set; } = "";

    /// <summary>
    /// Project name keyword
    /// </summary>
    public string ProjectName { get; set; } = "";

    /// <summary>
    /// Component name keyword
    /// </summary>
    public string ComponentName { get; set; } = "";

    /// <summary>
    /// Stock keyword
    /// </summary>
    public string Stock { get; set; } = "";

    /// <summary>
    /// Depth keyword
    /// </summary>
    public string Depth { get; set; } = "";

    /// <summary>
    /// Width keyword
    /// </summary>
    public string Width { get; set; } = "";

    /// <summary>
    /// Tolerance keyword
    /// </summary>
    public string Tolerance { get; set; } = "";

    /// <summary>
    /// Programmed feedrate keyword
    /// </summary>
    public string ProgrammedFeedrate { get; set; } = "";

    /// <summary>
    /// Spindle speed keyword
    /// </summary>
    public string ProgrammedSpindleSpeed { get; set; } = "";

    /// <summary>
    /// Tool code keyword
    /// </summary>
    public string ToolCode { get; set; } = "";

    /// <summary>
    /// Tool name keyword
    /// </summary>
    public string ToolName { get; set; } = "";

    /// <summary>
    /// Tool diameter keyword
    /// </summary>
    public string ToolDiameter { get; set; } = "";

    /// <summary>
    /// Tool radius keyword
    /// </summary>
    public string ToolRadius { get; set; } = "";

    /// <summary>
    /// Minimum tool length keyword
    /// </summary>
    public string MinimumToolLength { get; set; } = "";

    /// <summary>
    /// Strategy keyword
    /// </summary>
    public string Strategy { get; set; } = "";

    /// <summary>
    /// Operation duration in format hh:mm:ss keyword
    /// </summary>
    public string OperationDurationHHMMSS { get; set; } = "";

    /// <summary>
    /// Sequence duration in format hh:mm:ss keyword
    /// </summary>
    public string SequenceDurationHHMMSS { get; set; } = "";

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetCadName (string? camSystem) => this.CadName;

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetComponentName (string? camSystem) => this.ComponentName;

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetDepth (string? camSystem) => this.Depth;

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetMinimumToolLength (string? camSystem) => this.MinimumToolLength;

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetProgrammedFeedrate (string? camSystem) => this.ProgrammedFeedrate;

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetProgrammedSpindleSpeed (string? camSystem) => this.ProgrammedSpindleSpeed;

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetProjectName (string? camSystem) => this.ProjectName;

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetStock (string? camSystem) => this.Stock;

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetStrategy (string? camSystem) => this.Strategy;

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetTolerance (string? camSystem) => this.Tolerance;

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetToolCode (string? camSystem) => this.ToolCode;

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetToolDiameter (string? camSystem) => this.ToolDiameter;

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetToolName (string? camSystem) => this.ToolName;

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetToolRadius (string? camSystem) => this.ToolRadius;

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetWidth (string? camSystem) => this.Width;

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetOperationDurationHHMMSS (string? camSystem) => this.OperationDurationHHMMSS;

    /// <summary>
    /// <see cref="ICamSystemKeywordsGetter"/>
    /// </summary>
    public string GetSequenceDurationHHMMSS (string? camSystem) => this.SequenceDurationHHMMSS;
  }
}
