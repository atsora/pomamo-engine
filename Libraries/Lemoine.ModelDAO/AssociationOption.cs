// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Available options for some kind of MachineAssociations
  /// </summary>
  [Flags]
  public enum AssociationOption
  {
    /// <summary>
    /// Associate to related slot
    /// </summary>
    AssociateToSlotOption = 1,
    /// <summary>
    /// The data was automatically detected on the machine
    /// </summary>
    Detected = 2, // 1 << 1
    /// <summary>
    /// Do not try to merge the data on the left
    /// </summary>
    NoLeftMerge = 4, // 1 << 2
    /// <summary>
    /// Do not try to merge the data on the right
    /// </summary>
    NoRightMerge = 8, // 1 << 3
    /// <summary>
    /// Do not try to split the analysis by period of time when a change is really required
    /// </summary>
    NotByPeriod = 16, // 1 << 4
    /// <summary>
    /// Do not consider the end of the association may be changed by some 'stop' rule
    /// </summary>
    NoStop = 32, // 1 << 5
    /// <summary>
    /// Force the assocation to be fully synchronous
    /// </summary>
    Synchronous = 64, // 1 << 6
    /// <summary>
    /// Track any slot change that could impact the modification
    /// </summary>
    TrackSlotChanges = 128, // 1 << 7
    /// <summary>
    /// Adapt a progressive strategy (VS a more aggressive strategy with a possibly later cancel process)
    /// 
    /// For example for the DynamicEnd process, if the dynamic end is now known yet, it is applied
    /// at once up to the specific end and then it may be cancelled if the dynamic end is before 
    /// that end
    /// </summary>
    ProgressiveStrategy = 256, // 1 << 8
    /// <summary>
    /// The dynamic end must be absolutely before the real end so that the modification applies
    /// 
    /// If this option is set, the modification may be cancelled 
    /// (is cancelled for ReasonMachineAssociation) if the dynamic end is found
    /// after the specific end.
    /// 
    /// If this option is not set, the modification is applied up to the specific end
    /// even if the dynamic end is after it.
    /// </summary>
    DynamicEndBeforeRealEnd = 512, // 1 << 9
    /// <summary>
    /// Use the association during a post-processing phase, to force the data processing to be completed
    /// </summary>
    FinalProcess = 1024, // 1 << 10
    /// <summary>
    /// Skip the compatibility check
    /// </summary>
    NoCompatibilityCheck = 2048, // 1 << 11
    /// <summary>
    /// Track only a dynamic end
    /// </summary>
    TrackDynamicEnd = 4096, // 1 << 12
  };
  
  /// <summary>
  /// Extensions to AssociationOption
  /// </summary>
  public static class AssociationOptionMethodExtensions
  {
    /// <summary>
    /// Check if other is included into t
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool HasFlag (this AssociationOption t, AssociationOption other)
    {
      return other == (t & other);
    }
    
    /// <summary>
    /// Add an option to another one (which may be null)
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static AssociationOption Add (this AssociationOption t, AssociationOption? other)
    {
      if (other.HasValue) {
        return t | other.Value;
      }
      else {
        return t;
      }
    }
    
    /// <summary>
    /// Remove an option
    /// </summary>
    /// <param name="t"></param>
    /// <param name="toRemove"></param>
    /// <returns></returns>
    public static AssociationOption Remove (this AssociationOption t, AssociationOption toRemove)
    {
      return t & ~toRemove;
    }
  }
}
