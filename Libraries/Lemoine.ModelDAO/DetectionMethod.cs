// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// All detection methods
  /// 
  /// Deprecated. Use plugins instead now
  /// </summary>
  [Flags]
  public enum DetectionMethod
  {
    /// <summary>
    /// No detection
    /// </summary>
    None = 0,
    /// <summary>
    /// Stamp (default)
    /// </summary>
    StartCycleStamp = 1,
    /// <summary>
    /// Strictly positive (deprecated)
    /// </summary>
    StartCycleStrictlyPositive = 2, // 1 << 1
    /// <summary>
    /// Operation code (deprecated)
    /// </summary>
    StartCycleOperationCode = 4, // 1 << 2
    /// <summary>
    /// Stamp (default)
    /// </summary>
    EndCycleStamp = 8, // 1 << 3
    /// <summary>
    /// Strictly positive (deprecated)
    /// </summary>
    EndCycleStrictlyPositive = 16, // 1 << 4
    /// <summary>
    /// Operation code (deprecated)
    /// </summary>
    EndCycleOperationCode = 32, // 1 << 5
    /// <summary>
    /// A null value corresponds to a cycle end (deprecated)
    /// </summary>
    EndCycleZero = 64, // 1 << 6
    /// <summary>
    ///  (deprecated)
    /// <item>Between 1 and 9, it corresponds to a cycle end with the specified quantity</item>
    /// <item>If &gt; 10, it corresponds to a stamp</item>
    /// </summary>
    EndCycleQuantityOrStamp = 128, // 1 << 7
    /// <summary>
    ///  (deprecated)
    /// 10 * stamp + quantity
    /// 
    /// If the quantity is strictly positive, it corresponds to a cycle end with the specified quantity
    /// </summary>
    EndCycleStampQuantity = 256, // 1 << 8
    /// <summary>
    ///  (deprecated)
    /// 10 * stamp + operation code
    /// 
    /// If the quantity is strictly positive, it corresponds to a cycle end with the specified quantity
    /// </summary>
    EndCycleOperationCodeQuantity = 512, // 1 << 9
    /// <summary>
    /// Sequence stamp (deprecated)
    /// </summary>
    SequenceStamp = 1024, // 1 << 10
    /// <summary>
    /// A variable change means a cycle starts (deprecated)
    /// </summary>
    ChangeIsStartCycle = 2048, // 1 << 11
    /// <summary>
    /// A variable change means a cycle ends (deprecated)
    /// </summary>
    ChangeIsStopCycle = 4096, // 1 << 12 (deprecated)
    /// <summary>
    /// Contains a sequence milestone
    /// </summary>
    SequenceMilestone = 8192, // 1 << 13 (deprecated)
    /// <summary>
    /// The sequence milestone is set added to the sequence stamp variable as a decimal part
    /// </summary>
    SequenceMilestoneWithStamp = 16384, // 1 << 14 (deprecated)
    /// <summary>
    /// Use the cnc variable set directly to get the stamps from the machine and not individual values
    /// </summary>
    CncVariableSet = 32768, // 1 << 15
    // Other types to could be implemented (but not necessary any more with the plugins): ToolNumber, OperationCodeDotSequenceOrder, SequenceOrder, MachineModeCode...
    /// <summary>
    /// Default: combination of all stamp values
    /// </summary>
    Default = DetectionMethod.SequenceStamp | DetectionMethod.EndCycleStamp | DetectionMethod.StartCycleStamp | DetectionMethod.SequenceMilestoneWithStamp,
    /// <summary>
    /// Filter for the Start cycle valid values
    /// </summary>
    StartCycleFilter = DetectionMethod.StartCycleStamp | DetectionMethod.StartCycleStrictlyPositive | DetectionMethod.StartCycleOperationCode | DetectionMethod.ChangeIsStartCycle,
    /// <summary>
    /// Filter for the end cycle valid values
    /// </summary>
    EndCycleFilter = DetectionMethod.EndCycleStamp | DetectionMethod.EndCycleStrictlyPositive | DetectionMethod.EndCycleOperationCode
      | DetectionMethod.EndCycleZero | DetectionMethod.EndCycleQuantityOrStamp | DetectionMethod.EndCycleStampQuantity | DetectionMethod.EndCycleOperationCodeQuantity
      | DetectionMethod.ChangeIsStopCycle,
    /// <summary>
    /// Filter for sequence valid values
    /// </summary>
    SequenceFilter = DetectionMethod.SequenceStamp | DetectionMethod.SequenceMilestoneWithStamp,
    /// <summary>
    /// Filter for milestone valid values
    /// </summary>
    MilestoneFilter = DetectionMethod.SequenceMilestone,
  }
  
  /// <summary>
  /// Start cycle detection method (deprecated)
  /// 
  /// Deprecated. Use plugins instead now
  /// </summary>
  public enum StartCycleDetectionMethod
  {
    /// <summary>
    /// No start cycle detection
    /// </summary>
    None = 0,
    /// <summary>
    /// Stamp (default)
    /// </summary>
    Stamp = 1,
    /// <summary>
    /// Strictly positive
    /// </summary>
    StrictlyPositive = 2, // 1 << 1
    /// <summary>
    /// Operation code
    /// </summary>
    OperationCode = 4, // 1 << 2
    /// <summary>
    /// A variable change means a cycle starts
    /// </summary>
    ChangeIsStartCycle = 2048, // 1 << 11
  }

  /// <summary>
  /// End cycle detection method (deprecated)
  /// 
  /// Deprecated: use plugins instead now
  /// </summary>
  public enum CycleDetectionMethod
  {
    /// <summary>
    /// No end cycle detection
    /// </summary>
    None = 0,
    /// <summary>
    /// Stamp (default)
    /// </summary>
    Stamp = 8, // 1 << 3
    /// <summary>
    /// Strictly positive
    /// </summary>
    StrictlyPositive = 16, // 1 << 4
    /// <summary>
    /// Operation code
    /// </summary>
    OperationCode = 32, // 1 << 5
    /// <summary>
    /// A null value corresponds to a cycle end
    /// </summary>
    Zero = 64, // 1 << 6
    /// <summary>
    /// <item>Between 1 and 9, it corresponds to a cycle end with the specified quantity</item>
    /// <item>If &gt; 10, it corresponds to a stamp</item>
    /// </summary>
    QuantityOrStamp = 128, // 1 << 7
    /// <summary>
    /// 10 * stamp + quantity
    /// 
    /// If the quantity is strictly positive, it corresponds to a cycle end with the specified quantity
    /// </summary>
    StampQuantity = 256, // 1 << 8
    /// <summary>
    /// 10 * stamp + operation code
    /// 
    /// If the quantity is strictly positive, it corresponds to a cycle end with the specified quantity
    /// </summary>
    OperationCodeQuantity = 512, // 1 << 9
    /// <summary>
    /// A variable change means a cycle ends
    /// </summary>
    ChangeIsStopCycle = 4096, // 1 << 12
  }
  
  /// <summary>
  /// Sequence detection method (deprecated)
  /// 
  /// Deprecated: use plugins instead now
  /// </summary>
  public enum SequenceDetectionMethod
  {
    /// <summary>
    /// No sequence detection
    /// </summary>
    None = 0,
    /// <summary>
    /// Stamp (default)
    /// </summary>
    Stamp = 1024, // 1 << 10
    // Other types that could be implemented (but not necessary any more with the plugins): ToolNumber, OperationCodeDotSequenceOrder, SequenceOrder, MachineModeCode...
  }
  
  /// <summary>
  /// Extensions to DetectionMethod
  /// </summary>
  public static class DetectionMethodExtensions
  {
    /// <summary>
    /// Check if other is included into t
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool HasFlag (this DetectionMethod t, DetectionMethod other)
    {
      return other == (t & other);
    }
    
  }
}
