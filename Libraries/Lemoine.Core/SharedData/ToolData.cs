// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Core.SharedData
{
  /// <summary>
  /// Different ways to describe the life of a tool
  /// Used also for the compensation numbers
  /// </summary>
  public enum ToolUnit
  {
    /// <summary>
    /// Unknown
    /// </summary>
    Unknown = 0,
    
    /// <summary>
    /// Tool life is described in terms of seconds
    /// </summary>
    TimeSeconds = 1,
    
    /// <summary>
    /// Tool life is described in terms of parts produced
    /// Only one time per program is counted, even if the tool is used several times
    /// within this program
    /// </summary>
    Parts = 2,
    
    /// <summary>
    /// Tool life is described in terms of number of times the tool is used
    /// It is possible to count several times per program
    /// </summary>
    NumberOfTimes = 3,
    
    /// <summary>
    /// Tool life is described in terms of wear
    /// </summary>
    Wear = 4,
    
    /// <summary>
    /// Tool life is described in terms of a distance (millimeters)
    /// </summary>
    DistanceMillimeters = 5,
    
    /// <summary>
    /// Tool life is described in terms of a distance (inches)
    /// </summary>
    DistanceInch = 6,
    
    /// <summary>
    /// Number of cycles a tool is used
    /// </summary>
    NumberOfCycles = 7
  }

  /// <summary>
  /// Different ways to count the life of a tool
  /// </summary>
  public enum ToolLifeDirection
  {
    /// <summary>
    /// The value summarizing the tool life is decreasing.
    /// We count the rest of the life until it expires.
    /// </summary>
    Down = -1,
    
    /// <summary>
    /// Unknown direction
    /// </summary>
    Unknown = 0,
    
    /// <summary>
    /// The value summarizing the tool life is increasing.
    /// We count how much of something the tool did from its registration.
    /// </summary>
    Up = 1
  }

  /// <summary>
  /// A specific position may have different states regarding the tool
  /// A tool may be available, temporary unavailable (busy) or definitely unavailable
  /// </summary>
  public enum ToolState {
    /// <summary>
    /// Unknown state
    /// </summary>
    Unknown = 0,
    
    /// <summary>
    /// AVAILABLE FOR VARIOUS REASONS NOT DESCRIBED HERE
    /// </summary>
    Available = 100,
    
    /// <summary>
    /// Available by being new and not used yet
    /// </summary>
    New = 101,
    
    /// <summary>
    /// Available by being not used and not new
    /// </summary>
    NotUsedNotNew = 102,
    
    /// <summary>
    /// TEMPORARY UNAVAILABLE FOR VARIOUS REASONS NOT DESCRIBED HERE
    /// </summary>
    TemporaryUnavailable = 200,
    
    /// <summary>
    /// Temporary unavailable by being used by the current machine
    /// </summary>
    Used = 201,
    
    /// <summary>
    /// Temporary unavailable by being reserved by another machine
    /// </summary>
    Reserved = 202,
    
    /// <summary>
    /// Temporary unavailable by being used by another machine
    /// </summary>
    Busy = 203,
    
    /// <summary>
    /// Temporary unavailable by being measured
    /// </summary>
    Measurement = 204,
    
    /// <summary>
    /// Temporary unavailable by being reconditioned
    /// </summary>
    Reconditioning = 205,
    
    /// <summary>
    /// Temporary unavailable by not being appropriate for the current operation
    /// </summary>
    NotAppropriate = 206,
    
    /// <summary>
    /// DEFINITELY UNAVAILABLE FOR VARIOUS REASONS NOT DESCRIBED HERE
    /// </summary>
    DefinitelyUnavailable = 300,
    
    /// <summary>
    /// Definitely unavailable by being expired 
    /// </summary>
    Expired = 301,
    
    /// <summary>
    /// Definitely unavailable by being broken 
    /// </summary>
    Broken = 302,
    
    /// <summary>
    /// Definitely unavailable by not being registered (= not managed)
    /// </summary>
    NotRegistered = 303,
    
    /// <summary>
    /// Other
    /// </summary>
    Other = 400,
    
    /// <summary>
    /// Not used at all
    /// </summary>
    Unused = 401
  }
}
