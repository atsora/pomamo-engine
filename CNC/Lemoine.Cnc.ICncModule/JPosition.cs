// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Position that can be serialized in Json
  /// </summary>
  public class JPosition
    : IEquatable<JPosition>
    , IPosition
  {
    readonly ILog log = LogManager.GetLogger (typeof (JPosition).FullName);

    /// <summary>
    /// X position
    /// </summary>
    [DefaultValue (null)]
    public double? X { get; set; } = null;

    /// <summary>
    /// Y position
    /// </summary>
    [DefaultValue (null)]
    public double? Y { get; set; } = null;

    /// <summary>
    /// Z position
    /// </summary>
    [DefaultValue (null)]
    public double? Z { get; set; } = null;

    /// <summary>
    /// U position
    /// </summary>
    [DefaultValue (null)]
    public double? U { get; set; } = null;

    /// <summary>
    /// V position
    /// </summary>
    [DefaultValue (null)]
    public double? V { get; set; } = null;

    /// <summary>
    /// W position
    /// </summary>
    [DefaultValue (null)]
    public double? W { get; set; } = null;

    /// <summary>
    /// A position
    /// </summary>
    [DefaultValue (null)]
    public double? A { get; set; } = null;

    /// <summary>
    /// B position
    /// </summary>
    [DefaultValue (null)]
    public double? B { get; set; } = null;

    /// <summary>
    /// C position
    /// </summary>
    [DefaultValue (null)]
    public double? C { get; set; } = null;

    /// <summary>
    /// UTC Date/time of the position
    /// 
    /// It is used to compute the feedrate
    /// </summary>
    public DateTime Time { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Default constructor
    /// </summary>
    public JPosition ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public JPosition (double x, double y, double z)
    {
      this.X = x;
      this.Y = y;
      this.Z = z;
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="p"></param>
    public JPosition (Position p)
    {
      this.X = p.X;
      this.Y = p.Y;
      this.Z = p.Z;
      if (0 != p.U) {
        this.U = p.U;
      }
      if (0 != p.V) {
        this.V = p.V;
      }
      if (0 != p.W) {
        this.W = p.W;
      }
      if (0 != p.A) {
        this.A = p.A;
      }
      if (0 != p.B) {
        this.B = p.B;
      }
      if (0 != p.C) {
        this.C = p.C;
      }
      this.Time = p.Time;
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="p"></param>
    public JPosition (IPosition p)
    {
      this.X = p.GetAxisValue ("X");
      this.Y = p.GetAxisValue ("Y");
      this.Z = p.GetAxisValue ("Z");
      this.U = p.GetAxisValue ("U");
      this.V = p.GetAxisValue ("V");
      this.W = p.GetAxisValue ("W");
      this.A = p.GetAxisValue ("A");
      this.B = p.GetAxisValue ("B");
      this.C = p.GetAxisValue ("C");
      this.Time = p.Time;
    }

    /// <summary>
    /// <see cref="Object.ToString" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      return $"[JPosition{GetStringValue (this.X, "X")}{GetStringValue (this.Y, "Y")}{GetStringValue (this.Z, "Z")}{GetStringValue (this.U, "U")}{GetStringValue (this.V, "V")}{GetStringValue (this.W, "W")}{GetStringValue (this.A, "A")}{GetStringValue (this.B, "B")}{GetStringValue (this.C, "C")}]";
    }

    string GetStringValue (double? v, string s)
    {
      return v.HasValue
        ? $" {s}={v.Value}"
        : "";
    }

    /// <summary>
    /// Get double value (distance from origin)
    /// </summary>
    /// <returns></returns>
    public double GetDoubleValue ()
    {
      return Math.Sqrt ((this.X ?? 0 + this.U ?? 0) * (this.X ?? 0 + this.U ?? 0)
                       + (this.Y ?? 0 + this.V ?? 0) * (this.Y ?? 0 + this.V ?? 0)
                       + (this.Z ?? 0 + this.W ?? 0) * (this.Z ?? 0 + this.W ?? 0));
    }

    /// <summary>
    /// <see cref="IEquatable{T}"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals (object obj)
    {
      return Equals (obj as JPosition);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals (JPosition other)
    {
      return other != null &&
             X == other.X &&
             Y == other.Y &&
             Z == other.Z &&
             U == other.U &&
             V == other.V &&
             W == other.W &&
             A == other.A &&
             B == other.B &&
             C == other.C &&
             Time == other.Time;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode ()
    {
      var hashCode = -1385684202;
      hashCode = hashCode * -1521134295 + X.GetHashCode ();
      hashCode = hashCode * -1521134295 + Y.GetHashCode ();
      hashCode = hashCode * -1521134295 + Z.GetHashCode ();
      hashCode = hashCode * -1521134295 + U.GetHashCode ();
      hashCode = hashCode * -1521134295 + V.GetHashCode ();
      hashCode = hashCode * -1521134295 + W.GetHashCode ();
      hashCode = hashCode * -1521134295 + A.GetHashCode ();
      hashCode = hashCode * -1521134295 + B.GetHashCode ();
      hashCode = hashCode * -1521134295 + C.GetHashCode ();
      hashCode = hashCode * -1521134295 + Time.GetHashCode ();
      return hashCode;
    }

    /// <summary>
    /// Conversion operator
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static implicit operator JPosition (Position p)
    {
      return new JPosition (p);
    }

    /// <summary>
    /// Conversion operator
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static implicit operator Position (JPosition p)
    {
      return new Position (p);
    }

    #region IPosition implementation
    /// <summary>
    /// <see cref="IPosition"/>
    /// </summary>
    /// <param name="axisName"></param>
    /// <returns></returns>
    public double? GetAxisValue (string axisName)
    {
      switch (axisName.ToUpperInvariant ()) {
      case "X": return this.X;
      case "Y": return this.Y;
      case "Z": return this.Z;
      case "U": return this.U;
      case "V": return this.V;
      case "W": return this.W;
      case "A": return this.A;
      case "B": return this.B;
      case "C": return this.C;
      default: throw new ArgumentException ("Invalid axis name", "axisName");
      }
    }

    /// <summary>
    /// <see cref="IPosition"/>
    /// </summary>
    /// <param name="v"></param>
    /// <param name="axisName"></param>
    public void SetAxisValue (double v, string axisName)
    {
      switch (axisName.ToUpperInvariant ()) {
      case "X":
        this.X = v;
        break;
      case "Y":
        this.Y = v;
        break;
      case "Z":
        this.Z = v;
        break;
      case "U":
        this.U = v;
        break;
      case "V":
        this.V = v;
        break;
      case "W":
        this.W = v;
        break;
      case "A":
        this.A = v;
        break;
      case "B":
        this.B = v;
        break;
      case "C":
        this.C = v;
        break;
      default:
        throw new ArgumentException ("Invalid axis name", "axisName");
      }
    }
    #endregion // IPosition implementation
  }
}
