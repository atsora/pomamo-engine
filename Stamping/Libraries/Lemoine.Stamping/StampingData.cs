// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Stamping
{
  public enum GCodeGroup
  {
  }

  /// <summary>
  /// Stamping data
  /// </summary>
  public sealed class StampingData : IEnumerable<KeyValuePair<string, object>>
  {
    readonly ILog log = LogManager.GetLogger (typeof (StampingData).FullName);

    readonly IDictionary<string, object> m_data = new Dictionary<string, object> ();
    string? m_source = null;

    /// <summary>
    /// Read position
    /// </summary>
    public int ReadPosition { get; set; } = 0;

    /// <summary>
    /// Write position
    /// </summary>
    public int WritePosition { get; set; } = 0;

    /// <summary>
    /// Source path
    /// </summary>
    public string? Source
    {
      get => m_source;
      set {
        m_source = value;
        try {
          Add ("FileName", System.IO.Path.GetFileName (value));
        }
        catch (Exception ex) {
          log.Info ($"Source.set: the FileName was not set in the stamping data source={value}", ex);
        }
      }
    }

    /// <summary>
    /// File name
    /// </summary>
    public string? FileName => GetString ("FileName");

    /// <summary>
    /// Destination path
    /// </summary>
    public string? Destination { get; set; } = null;

    /// <summary>
    /// Current tool number
    /// </summary>
    public string? ToolNumber
    {
      get { return GetString ("ToolNumber"); }
      set { Add ("ToolNumber", value); }
    }

    /// <summary>
    /// Associated cad name
    /// </summary>
    public string? CadName
    {
      get => GetString ("CadName");
      set {
        Add ("CadName", value);
      }
    }

    /// <summary>
    /// Associated cam system
    /// </summary>
    public string? CamSystem
    {
      get => GetString ("CamSystem");
      set {
        Add ("CamSystem", value);
      }
    }

    /// <summary>
    /// Associated operation
    /// </summary>
    public IOperation? Operation
    {
      get => Get<IOperation> ("Operation");
      set {
        Add ("Operation", value);
      }
    }

    /// <summary>
    /// Associated path
    /// </summary>
    public IPath? OperationPath
    {
      get => Get<IPath> ("OperationPath");
      set {
        Add ("OperationPath", value);
      }
    }

    /// <summary>
    /// Active sequence
    /// </summary>
    public ISequence? Sequence
    {
      get => Get<ISequence> ("Sequence");
      set {
        Add ("Sequence", value);
      }
    }

    /// <summary>
    /// Sequence duration
    /// </summary>
    public TimeSpan? SequenceDuration
    {
      get => Get<TimeSpan> ("SequenceDuration");
      set {
        Add ("SequenceDuration", value);
      }
    }

    /// <summary>
    /// Stamping config name
    /// </summary>
    public string? StampingConfigName
    {
      get => GetString ("StampingConfigName");
      set {
        Add ("StampingConfigName", value);
      }
    }

    /// <summary>
    /// Length unit
    /// 
    /// Only use here: In and Mm
    /// </summary>
    public MachiningUnit? Unit
    {
      get => Get<MachiningUnit?> ("Unit");
      set {
        if (!value.Equals (MachiningUnit.Mm) && !value.Equals (MachiningUnit.In)) {
          log.Warn ($"Unit.set: Set unit {value} is not Mm or In, which is not recommended");
        }
        Add ("Unit", value);
      }
    }

    /// <summary>
    /// Set the length unit to be Mm
    /// </summary>
    public void SetLengthUnitMm ()
    {
      this.Unit = MachiningUnit.Mm;
    }

    /// <summary>
    /// Set the length unit to be In
    /// </summary>
    public void SetLengthUnitIn ()
    {
      this.Unit = MachiningUnit.In;
    }

    /// <summary>
    /// Get a not nullable length unit. If not set, Unknown is returned
    /// </summary>
    /// <returns></returns>
    public MachiningUnit GetLengthUnit ()
    {
      return this.Unit ?? Lemoine.Model.MachiningUnit.Unknown;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public StampingData ()
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="initialData"></param>
    public StampingData (IEnumerable<KeyValuePair<string, object>> initialData)
    {
      foreach (var kv in initialData) {
        m_data[kv.Key] = kv.Value;
      }
    }

    /// <summary>
    /// Try to get a specific data from a specified key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool TryGet<T> (string key, out T? result)
    {
      if (m_data.TryGetValue (key, out var v)) {
        if (v is null) {
          log.Error ($"TryGet: {key} returned null");
          result = default (T);
          return false;
        }
        else {
          result = (T)v;
          return true;
        }
      }
      else {
        result = default (T);
        return false;
      }
    }

    /// <summary>
    /// Get the data for a specified key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T? Get<T> (string key)
    {
      if (TryGet<T> (key, out var v)) {
        return v;
      }
      else {
        return default (T?);
      }
    }

    /// <summary>
    /// Get a string
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string? GetString (string key)
    {
      if (TryGet<string> (key, out var v)) {
        return v;
      }
      else {
        return null;
      }
    }

    /// <summary>
    /// Add a new data
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    public void Add (string key, object? v)
    {
      if (v is null) {
        m_data.Remove (key);
      }
      else {
        m_data[key] = v;
      }
    }

    /// <summary>
    /// Remove a data
    /// </summary>
    /// <param name="key"></param>
    public void Remove (string key)
    {
      m_data.Remove (key);
    }

    /// <summary>
    /// <see cref="IEnumerable{T}"/>
    /// </summary>
    /// <returns></returns>
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator ()
    {
      return m_data.GetEnumerator ();
    }

    /// <summary>
    /// <see cref="IEnumerable{T}"/>
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator ()
    {
      return m_data.GetEnumerator ();
    }
  }
}
