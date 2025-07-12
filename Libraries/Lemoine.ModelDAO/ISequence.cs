// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024-2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lemoine.Model
{
  /// <summary>
  /// List of supported types for the Kind property of the sequence
  /// </summary>
  public enum SequenceKind
  {
    /// <summary>
    /// Machining
    /// </summary>
    Machining,
    /// <summary>
    /// Stop (M0,...)
    /// </summary>
    Stop,
    /// <summary>
    /// Optional stop (M1, /M0)
    /// </summary>
    OptionalStop,
    /// <summary>
    /// Non-machining sequence
    /// </summary>
    NonMachining,
    /// <summary>
    /// Pallet change.
    /// 
    /// Automatic if the pallet is ready. Else the machine stops.
    /// 
    /// Usually triggered by M60
    /// </summary>
    AutoPalletChange,
  }

  /// <summary>
  /// Feed unit
  /// </summary>
  [Flags]
  public enum MachiningUnit
  {
    /// <summary>
    /// Unknown
    /// 
    /// For FeedRate, default, that means unit/min
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// mm and mm/min
    /// </summary>
    Mm = 1,
    /// <summary>
    /// Inches and IPM
    /// </summary>
    In = 2,
    /// <summary>
    /// Revolution per minute
    /// </summary>
    RevolutionMin = 4,
    /// <summary>
    /// Inverse time
    /// 
    /// G93: the operation must be completed in (1/F number) of minutes
    /// </summary>
    InverseTime = 8,
  }

  /// <summary>
  /// In a segment, small angle changes are dismissed.
  /// 
  /// A segment is made of:
  /// - an initial vector (FV)
  /// - a length (L)
  /// - a terminal vector (LV)
  /// - if applicable, an angle change (A) at the end of the segment
  /// - the name of the axes that changed direction (DC) at the end of the segment
  /// </summary>
  public class Segment
  {
    /// <summary>
    /// First/initial vector, if configured to record it
    /// </summary>
    public double[] FV { get; set; } = null;

    /// <summary>
    /// Last/terminal vector, if configured to record it
    /// </summary>
    public double[] LV { get; set; } = null;

    /// <summary>
    /// Angle at the end of the segment
    /// </summary>
    public double? A { get; set; }

    /// <summary>
    /// Direction change: list of axis names for which the direction changed at the end of the segment
    /// </summary>
    public IList<string> DC { get; set; } = null;

    /// <summary>
    /// Length of the segment
    /// </summary>
    public double? L { get; set; }

    /// <summary>
    /// Distance by axis
    /// </summary>
    public double[] D { get; set; } = null;
  }

  /// <summary>
  /// Sequence path
  /// </summary>
  public class MachiningPath
  {
    /// <summary>
    /// Used path function. For example:
    /// <item>L: Linear for Heidenhain</item>
    /// <item>G1: Linear for standard g-codes</item>
    /// <item>C: arc for Heidenhain</item>
    /// 
    /// Not null or empty
    /// </summary>
    public string Function { get; set; }

    /// <summary>
    /// Additional parameters of the machining function or macro
    /// </summary>
    public string Params { get; set; } = null;

    /// <summary>
    /// Is it a rapid traverse?
    /// 
    /// If null, then this does not corresponds to a machining path
    /// If false, this is a machining path
    /// </summary>
    public bool? Rapid { get; set; }

    /// <summary>
    /// Feedrate
    /// </summary>
    public double? Feed { get; set; }

    /// <summary>
    /// Machining unit for distance and feedrate
    /// </summary>
    public MachiningUnit? Unit { get; set; }

    /// <summary>
    /// Distance
    /// </summary>
    public double? Distance { get; set; }

    /// <summary>
    /// Spindle speed
    /// </summary>
    public double? SpindleSpeed { get; set; }

    /// <summary>
    /// Segments
    /// 
    /// In a segment, small angle changes are dismissed.
    /// 
    /// A segment is made of:
    /// - an initial vector (FV)
    /// - a length (L)
    /// - a terminal vector (LV)
    /// - if applicable, an angle change (A) at the end of the segment
    /// - the name of the axes that changed direction (DC) at the end of the segment
    /// </summary>
    public IList<Segment> Segments { get; set; } = null;

    /// <summary>
    /// Number of times an axis direction changes
    /// </summary>
    public int? DirectionChanges { get; set; } = null;

    /// <summary>
    /// Angles (that are greater than a min angle in configuration)
    /// </summary>
    public IList<double> Angles { get; set; } = null;

    /// <summary>
    /// Computed time
    /// </summary>
    public double? Time { get; set; } = null;

    // TODO: tool properties ?
  }

  /// <summary>
  /// Sequence detail
  /// </summary>
  public class SequenceDetail
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public SequenceDetail () { }

    /// <summary>
    /// Version of the sequence detail data structure
    /// </summary>
    public int Version { get; set; } = 4;

    /// <summary>
    /// Sequence paths
    /// </summary>
    public IList<MachiningPath> Paths { get; set; } = new List<MachiningPath> ();

    /// <summary>
    /// Machining unit for distance (first one)
    /// </summary>
    public MachiningUnit? Unit { get; set; }

    /// <summary>
    /// Machining time in seconds
    /// </summary>
    public double? MachiningTime { get; set; }

    /// <summary>
    /// Rapid time in seconds
    /// </summary>
    public double? RapidTime { get; set; }

    /// <summary>
    /// Non machining time in seconds
    /// </summary>
    public double? NonMachiningTime { get; set; }

    /// <summary>
    /// Distance
    /// </summary>
    public double? Distance { get; set; }

    /// <summary>
    /// Rapid distance
    /// </summary>
    public double? RapidDistance { get; set; }

    /// <summary>
    /// Number of times an axis direction changes
    /// </summary>
    public int? DirectionChanges { get; set; } = null;

    // TODO: tool properties? Tool feed?
  }

  /// <summary>
  /// Model of table Sequence
  /// 
  /// It represents a specific machining sequence inside an operation.
  /// A sequence is characterized by a tool, some machining parameters
  /// like the tolerance, the stock...
  /// </summary>
  public interface ISequence : IVersionable, IDisplayable, IDataWithIdentifiers, IComparable, Lemoine.Collections.IDataWithId, ISerializableModel
  {
    /// <summary>
    /// Reference to the CAD Model table
    /// </summary>
    ICadModel CadModel { get; set; }

    /// <summary>
    /// Parent operation. Rather go through path.
    /// </summary>
    IOperation Operation { get; set; }

    /// <summary>
    /// Parent path
    /// </summary>
    IPath Path { get; set; }

    /// <summary>
    ///  order of the sequence in the parent operation
    /// </summary>
    int Order { get; set; }

    /// <summary>
    /// Name of the sequence
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Description of the sequence
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// Tool number
    /// </summary>
    string ToolNumber { get; set; }

    /// <summary>
    /// Reference to the used tool
    /// </summary>
    ITool Tool { get; set; }

    /// <summary>
    /// Estimated time
    /// </summary>
    TimeSpan? EstimatedTime { get; set; }

    /// <summary>
    /// Estimated time in seconds
    /// </summary>
    double? EstimatedTimeSeconds { get; set; }

    /// <summary>
    /// Should the sequence be only considered if the machine mode is flagged this way?
    /// </summary>
    bool AutoOnly { get; set; }

    /// <summary>
    /// Sequence Frequency in operation/path
    /// </summary>
    int Frequency { get; set; }

    /// <summary>
    /// Operation step if known
    /// </summary>
    int? OperationStep { get; set; }

    /// <summary>
    /// Sequence Kind
    /// </summary>
    SequenceKind Kind { get; set; }

    /// <summary>
    /// Sequence Detail (nullable)
    /// </summary>
    SequenceDetail Detail { get; set; }

    /// <summary>
    /// Set of stamps (and then ISO files) that are associated to this sequence
    /// </summary>
    ICollection<IStamp> Stamps { get; }

    /// <summary>
    /// Set of stamping values that are associated to this sequence
    /// </summary>
    ICollection<IStampingValue> StampingValues { get; }
  }

  /// <summary>
  /// Extensions to <see cref="ISequence"/>
  /// </summary>
  public static class SequenceExtensions
  {
    /// <summary>
    /// Get the associated <see cref="ISequenceOperationModel"/>
    /// </summary>
    /// <param name="sequence"></param>
    /// <returns></returns>
    public static IEnumerable<ISequenceOperationModel> GetSequenceOperationModels (this ISequence sequence)
    {
      return sequence.Operation.Revisions
        .SelectMany (x => x.OperationModels.SelectMany (y => y.SequenceOperationModels))
        .Where (x => x.Sequence.Id == sequence.Id)
        .Distinct ();
    }
    
    /// <summary>
    /// Get the associated <see cref="IOperationModel"/>
    /// </summary>
    /// <param name="sequence"></param>
    /// <returns></returns>

    public static IEnumerable<IOperationModel> GetOperationModels (this ISequence sequence)
    {
      return sequence.Operation.Revisions
        .SelectMany (x => x.OperationModels.Where (y => y.SequenceOperationModels.Any (z => z.Sequence.Id == sequence.Id)))
        .Distinct ();
    }
  }
}
