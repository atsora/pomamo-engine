// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Stamping
{
  /// <summary>
  /// Get the string keywords from the CAM system that correspond in the iso file to some of the StampingData keys
  /// </summary>
  public interface ICamSystemKeywordsGetter
  {
    /// <summary>
    /// CAD name keyword
    /// </summary>
    /// <param name="camSystem">CAM system if known</param>
    string GetCadName (string? camSystem);

    /// <summary>
    /// Project/Job name keyword
    /// </summary>
    string GetProjectName (string? camSystem);

    /// <summary>
    /// Component name keyword
    /// </summary>
    string GetComponentName (string? camSystem);

    /// <summary>
    /// Stock keyword
    /// </summary>
    string GetStock (string? camSystem);

    /// <summary>
    /// Depth of cut keyword
    /// </summary>
    string GetDepth (string? camSystem);

    /// <summary>
    /// Width of cut keyword
    /// </summary>
    string GetWidth (string? camSystem);

    /// <summary>
    /// Tolerance keyword
    /// </summary>
    /// <param name="camSystem"></param>
    /// <returns></returns>
    string GetTolerance (string? camSystem);

    /// <summary>
    /// Programmed feedrate keyword
    /// </summary>
    /// <param name="camSystem"></param>
    /// <returns></returns>
    string GetProgrammedFeedrate (string? camSystem);

    /// <summary>
    /// Spindle speed keyword
    /// </summary>
    string GetProgrammedSpindleSpeed (string? camSystem);

    /// <summary>
    /// Tool code keyword
    /// </summary>
    /// <param name="camSystem"></param>
    /// <returns></returns>
    string GetToolCode (string? camSystem);

    /// <summary>
    /// Tool name keyword
    /// </summary>
    /// <param name="camSystem"></param>
    /// <returns></returns>
    string GetToolName (string? camSystem);

    /// <summary>
    /// Tool diameter keyword
    /// </summary>
    string GetToolDiameter (string? camSystem);

    /// <summary>
    /// Tool radius keyword
    /// </summary>
    string GetToolRadius (string? camSystem);

    /// <summary>
    /// Minimum tool length keyword
    /// </summary>
    /// <param name="camSystem"></param>
    /// <returns></returns>
    string GetMinimumToolLength (string? camSystem);

    /// <summary>
    /// Strategy keyword
    /// </summary>
    string GetStrategy (string? camSystem);

    /// <summary>
    /// Operation time in format hh:mm:ss
    /// </summary>
    /// <param name="camSystem"></param>
    /// <returns></returns>
    string GetOperationDurationHHMMSS (string? camSystem);

    /// <summary>
    /// Sequence time in format hh:mm:ss
    /// </summary>
    /// <param name="camSystem"></param>
    /// <returns></returns>
    string GetSequenceDurationHHMMSS (string? camSystem);
  }
}
