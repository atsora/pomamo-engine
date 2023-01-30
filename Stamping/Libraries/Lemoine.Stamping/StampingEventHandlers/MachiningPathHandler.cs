// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Stamping.StampingEventHandlers
{
  /// <summary>
  /// Machining path handler
  /// 
  /// Add a machining path to a sequence when detected
  /// </summary>
  public class MachiningPathHandler
    : IStampingEventHandler
  {
    readonly ILog log = LogManager.GetLogger (typeof (MachiningPathHandler).FullName);

    string? m_activePathFunction = null;
    string? m_pathFunction = null;
    string? m_activeParameter = null;
    string? m_parameter = null;
    bool? m_activeRapidTraverse = null;
    bool? m_rapidTraverse = null;
    bool? m_activeNonMachining = null;
    bool? m_nonMachining = null;
    double? m_activeFeedRate = null;
    double? m_feedRate = null;
    MachiningUnit? m_activeUnit = null;
    MachiningUnit? m_unit = null;
    TimeSpan? m_activeTime = null;
    TimeSpan? m_time = null;
    double? m_activeDistance = null;
    double? m_distance = null;
    double? m_activeSpindleSpeed = null;
    double? m_spindleSpeed = null;
    int? m_activeDirectionChanges = null;
    bool m_directionChange = false;
    IList<double>? m_activeAngles = null;
    double? m_angle = null;

    /// <summary>
    /// Constructor
    /// </summary>
    public MachiningPathHandler (IStamper stamper, StampingData stampingData)
    {
      this.Stamper = stamper;
      this.StampingData = stampingData;
    }

    StampingData StampingData { get; }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public IStampingEventHandler? Next { get; set; }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public IStamper Stamper { get; }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void NotifyNewBlock (bool edit, int level)
    {
      try {
        var newPath = CheckNewPath ();

        if (!object.Equals (m_activePathFunction, m_pathFunction)) {
          m_activePathFunction = m_pathFunction;
          m_activeParameter = null;
        }
        if (!string.IsNullOrEmpty (m_parameter)) {
          m_activeParameter = m_parameter;
          m_parameter = null;
        }
        m_activeRapidTraverse = m_rapidTraverse;
        m_activeNonMachining = m_nonMachining;
        if (m_nonMachining.HasValue) {
          // Not to keep this property more than in one block in memory
          m_nonMachining = null;
          m_pathFunction = null;
        }
        if (m_activeNonMachining ?? false) {
          m_activeFeedRate = null;
          m_activeSpindleSpeed = null;
          m_activeRapidTraverse = null;
        }
        else if (m_activeRapidTraverse ?? false) {
          m_activeFeedRate = null;
          m_activeSpindleSpeed = null;
        }
        else {
          m_activeFeedRate = m_feedRate;
          m_activeSpindleSpeed = m_spindleSpeed;
        }
        m_activeUnit = m_unit;
        if (m_time.HasValue) {
          if (m_activeTime.HasValue) {
            m_activeTime = m_activeTime.Value.Add (m_time.Value);
          }
          else {
            m_activeTime = m_time;
          }
          m_time = null;
        }
        if (m_distance.HasValue) {
          if (m_activeDistance.HasValue) {
            m_activeDistance = m_activeDistance.Value + m_distance.Value;
          }
          else {
            m_activeDistance = m_distance;
          }
          m_distance = null;
        }
        if (m_directionChange) {
          m_activeDirectionChanges = (m_activeDirectionChanges ?? 0) + 1;
          m_directionChange = false;
        }
        if (m_angle.HasValue) {
          if (m_activeAngles is null) {
            m_activeAngles = new List<double> { m_angle.Value };
          }
          else { // m_activeAngles not null
            m_activeAngles.Add (m_angle.Value);
          }
          m_angle = null;
        }
      }
      catch (Exception ex) {
        log.Fatal ($"NotifyNewBlock: unexpected error", ex);
        throw;
      }

      this.Next?.NotifyNewBlock (edit, level);
    }

    bool xor (bool p, bool q) => (p || q) && !(p && q);

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    /// <param name="comment"></param>
    public void SetComment (string comment)
    {
      this.Next?.SetComment (comment);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SetData (string key, object v)
    {
      switch (key) {
      case "PathFunction":
        m_pathFunction = (string)v;
        break;
      case "PathParameter":
        m_parameter = (string)v;
        break;
      case "PathRapidTraverse":
        m_rapidTraverse = (bool)v;
        break;
      case "PathNonMachining":
        m_nonMachining = (bool)v;
        break;
      case "PathFeedRate":
        m_feedRate = (double)v;
        break;
      case "Unit":
        m_unit = (MachiningUnit)v;
        break;
      case "PathSpindleSpeed":
        m_spindleSpeed = (double)v;
        break;
      case "Distance":
        m_distance = (m_distance ?? 0.0) + (double)v;
        break;
      case "DirectionChange":
        m_directionChange = true;
        break;
      case "Angle":
        m_angle = (double)v;
        break;
      }
      this.Next?.SetData (key, v);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    /// <param name="duration"></param>
    public void SetMachiningTime (TimeSpan duration)
    {
      if (log.IsTraceEnabled) {
        log.Trace ($"SetMachiningTime: duration={duration}");
      }
      m_time = (m_time ?? TimeSpan.FromSeconds (0)).Add (duration);
      this.Next?.SetMachiningTime (duration);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SetNextToolNumber (string toolNumber)
    {
      this.Next?.SetNextToolNumber (toolNumber);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartCycle ()
    {
      this.Next?.StartCycle ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartSequence (SequenceKind sequenceKind)
    {
      this.Next?.StartSequence (sequenceKind);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StopCycle ()
    {
      this.Next?.StopCycle ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartProgram (bool edit, int level)
    {
      this.Next?.StartProgram (edit, level);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void EndProgram (bool edit, int level, bool endOfFile)
    {
      if (0 == level) {
        CheckNewPath ();
      }

      this.Next?.EndProgram (edit, level, endOfFile);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void ResumeProgram (bool edit, int level)
    {
      this.Next?.ResumeProgram (edit, level);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SuspendProgram (bool optional = false, string details = "")
    {
      this.Next?.SuspendProgram (optional, details);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void TriggerMachining ()
    {
      this.Next?.TriggerMachining ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void TriggerToolChange (string toolNumber = "")
    {
      this.Next?.TriggerToolChange ();
    }

    bool CheckNewPath ()
    {
      if (this.StampingData.Sequence is not null) {
        if (!string.IsNullOrEmpty (m_activePathFunction)) {
          bool newPath = false;
          var pathFunctionUpdate = !object.Equals (m_activePathFunction, m_pathFunction);
          newPath |= pathFunctionUpdate;

          var parameterUpdate = !object.Equals (m_activeParameter, m_parameter);
          newPath |= parameterUpdate;

          var rapidTraverseUpdate = xor (m_activeRapidTraverse ?? false, m_rapidTraverse ?? false);
          newPath |= rapidTraverseUpdate;

          var nonMachiningUpdate = xor (m_activeNonMachining ?? false, m_nonMachining ?? false); // TODO: ...
          newPath |= nonMachiningUpdate;

          var isRapidTraverse = m_activeRapidTraverse ?? false;
          var isNonMachining = m_activeNonMachining ?? false;
          if (!isRapidTraverse && !isNonMachining) {
            var feedRateUpdate = !object.Equals (m_activeFeedRate, m_feedRate);
            newPath |= feedRateUpdate;

            var unitUpdate = !object.Equals (m_activeUnit, m_unit);
            newPath |= unitUpdate;

            var spindleSpeedUpdate = !object.Equals (m_activeSpindleSpeed, m_spindleSpeed);
            newPath |= spindleSpeedUpdate;
          }

          if (newPath) {
            if (this.StampingData.Sequence.Detail is null) {
              this.StampingData.Sequence.Detail = new SequenceDetail ();
            }
            if (m_activeUnit is null) {
              m_activeUnit = this.StampingData.Unit;
            }
            var unit = isNonMachining ? null : m_activeUnit;
            var spindleSpeed = isNonMachining ? null : m_activeSpindleSpeed;
            bool? rapid = isNonMachining ? null : isRapidTraverse;
            var detail = this.StampingData.Sequence.Detail;
            var machiningPath = new MachiningPath () {
              Function = m_activePathFunction,
              Params = m_activeParameter,
              Rapid = rapid,
              Feed = m_activeFeedRate,
              Unit = unit,
              Distance = m_activeDistance,
              SpindleSpeed = spindleSpeed,
              DirectionChanges = m_activeDirectionChanges,
              Angles = m_activeAngles,
            };
            detail.Paths.Add (machiningPath);
            if (m_activeTime.HasValue) {
              if (isRapidTraverse) { // Rapid traverse only
                detail.RapidTime = detail.RapidTime ?? 0.0 + m_activeTime.Value.TotalSeconds;
              }
              else if (isNonMachining) {
                detail.NonMachiningTime = detail.NonMachiningTime ?? 0.0 + m_activeTime.Value.TotalSeconds;
              }
              else {
                detail.MachiningTime = detail.MachiningTime ?? 0.0 + m_activeTime.Value.TotalSeconds;
              }
            }
            if (m_activeUnit.HasValue && !isNonMachining) {
              if (!detail.Unit.HasValue || detail.Unit.Value.Equals (MachiningUnit.Unknown)) {
                if (m_activeUnit.Value.HasFlag (MachiningUnit.In)) {
                  detail.Unit = MachiningUnit.In;
                }
                else if (m_activeUnit.Value.HasFlag (MachiningUnit.Mm)) {
                  detail.Unit = MachiningUnit.Mm;
                }
              }
              else if (m_activeUnit.Value.HasFlag (MachiningUnit.In) && (detail.Unit.Value.Equals (MachiningUnit.Mm))) { // From Mm to In
                if (detail.Distance.HasValue) {
                  detail.Distance = Lemoine.Conversion.Converter.ConvertToInches (detail.Distance.Value);
                }
                if (detail.RapidDistance.HasValue) {
                  detail.RapidDistance = Lemoine.Conversion.Converter.ConvertToInches (detail.RapidDistance.Value);
                }
                detail.Unit = MachiningUnit.In;
              }
              else if (m_activeUnit.Value.HasFlag (MachiningUnit.Mm) && (detail.Unit.Value.Equals (MachiningUnit.In))) { // From In to Mm
                if (detail.Distance.HasValue) {
                  detail.Distance = Lemoine.Conversion.Converter.ConvertToMetric (detail.Distance.Value, false);
                }
                if (detail.RapidDistance.HasValue) {
                  detail.RapidDistance = Lemoine.Conversion.Converter.ConvertToMetric (detail.RapidDistance.Value, false);
                }
                detail.Unit = MachiningUnit.Mm;
              }
            }
            if (m_activeDistance.HasValue) {
              if (isRapidTraverse) { // Rapid traverse only
                detail.RapidDistance = (detail.RapidDistance ?? 0.0) + m_activeDistance.Value;
              }
              else {
                detail.Distance = (detail.Distance ?? 0.0) + m_activeDistance.Value;
              }
            }
            if (m_activeDirectionChanges.HasValue) {
              detail.DirectionChanges = (detail.DirectionChanges ?? 0) + m_activeDirectionChanges.Value;
            }
            m_activeParameter = null;
            m_activeNonMachining = false;
            m_activeTime = null;
            m_activeDistance = null;
            m_activeAngles = null;
            m_activeDirectionChanges = null;
            return true;
          }
          return false;
        }
        else { // No active path function
          return false;
        }
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"CheckNewPath: no active sequence");
        }
        return false;
      }
    }
  }
}
