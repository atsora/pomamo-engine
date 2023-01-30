// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Interface for converting between a string and a timespan
  /// in a given format
  /// </summary>
  public interface ITimespanStringConverter
  {    
    /// <summary>
    /// correct string for a timespan of null duration
    /// </summary>
    /// <returns></returns>
    string defaultString();
    
    /// <summary>
    /// Convert from duration in hours as a double to a string
    /// </summary>
    /// <param name="durationInHours"></param>
    /// <returns></returns>
    string ConvertToString(double? durationInHours);

    /// <summary>
    /// Convert a time span to a string
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    string ConvertToString (TimeSpan timeSpan);

    /// <summary>
    /// Convert from a HH:MM:ss string to a timespan
    /// </summary>
    /// <param name="durationStringMMHHss"></param>
    /// <returns></returns>
    TimeSpan? ConvertFromString(string durationStringMMHHss);
    
  }
}
