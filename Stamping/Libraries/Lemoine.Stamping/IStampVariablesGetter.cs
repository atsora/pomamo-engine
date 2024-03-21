// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Stamping
{
  /// <summary>
  /// Interface to get the stamp variables
  /// 
  /// If it is not defined, an empty string is returned
  /// </summary>
  public interface IStampVariablesGetter
  {
    /// <summary>
    /// Get the sequence stamp variable
    /// </summary>
    /// <returns></returns>
    string SequenceStampVariable { get; }

    /// <summary>
    /// Get the start cycle stamp variable
    /// </summary>
    /// <returns></returns>
    string StartCycleStampVariable { get; }

    /// <summary>
    /// Get the stop cycle stamp variable
    /// </summary>
    /// <returns></returns>
    string StopCycleStampVariable { get; }

    /// <summary>
    /// Get the milestone stamp variable (if any)
    /// </summary>
    string MilestoneStampVariable { get; }
  }

  /// <summary>
  /// Extension class to <see cref="IStampVariablesGetter"/>
  /// </summary>
  public static class StampVariablesGetterExtension
  {
    /// <summary>
    /// Test if a string is one of the stamp variable
    /// </summary>
    /// <param name="stampVariablesGetter"></param>
    /// <param name="s"></param>
    /// <returns></returns>
    public static bool IsStampVariable (this IStampVariablesGetter stampVariablesGetter, string s)
    {
      if (string.IsNullOrEmpty (s.Trim ())) {
        return false;
      }
      if (string.Equals (stampVariablesGetter.SequenceStampVariable, s, StringComparison.InvariantCultureIgnoreCase)) {
        return true;
      }
      if (string.Equals (stampVariablesGetter.StartCycleStampVariable, s, StringComparison.InvariantCultureIgnoreCase)) {
        return true;
      }
      if (string.Equals (stampVariablesGetter.StopCycleStampVariable, s, StringComparison.InvariantCultureIgnoreCase)) {
        return true;
      }
      if (string.Equals (stampVariablesGetter.MilestoneStampVariable, s, StringComparison.InvariantCultureIgnoreCase)) {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Test if a string starts with one of the stamp variable
    /// </summary>
    /// <param name="stampVariablesGetter"></param>
    /// <param name="s"></param>
    /// <param name="sep">separator</param>
    /// <returns></returns>
    public static bool StartWithStampVariable (this IStampVariablesGetter stampVariablesGetter, string s, string sep = "")
    {
      var p = s.Trim ();
      if (string.IsNullOrEmpty (p)) {
        return false;
      }
      if (p.StartsWith (stampVariablesGetter.SequenceStampVariable + sep, StringComparison.InvariantCultureIgnoreCase)) {
        return true;
      }
      if (p.StartsWith (stampVariablesGetter.StartCycleStampVariable + sep, StringComparison.InvariantCultureIgnoreCase)) {
        return true;
      }
      if (p.StartsWith (stampVariablesGetter.StopCycleStampVariable + sep, StringComparison.InvariantCultureIgnoreCase)) {
        return true;
      }
      if (p.StartsWith (stampVariablesGetter.MilestoneStampVariable + sep, StringComparison.InvariantCultureIgnoreCase)) {
        return true;
      }
      return false;
    }
  }
}
