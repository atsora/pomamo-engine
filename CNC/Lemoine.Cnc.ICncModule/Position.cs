// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Position
  /// </summary>
  public struct Position
    : IConvertible
    , IPosition
    , IEquatable<Position>
  {
    /// <summary>
    /// X position
    /// </summary>
    public double X { get; set; }
    
    /// <summary>
    /// Y position
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Z position
    /// </summary>
    public double Z { get; set; }

    /// <summary>
    /// U position
    /// </summary>
    public double U { get; set; }

    /// <summary>
    /// V position
    /// </summary>
    public double V { get; set; }

    /// <summary>
    /// W position
    /// </summary>
    public double W { get; set; }

    /// <summary>
    /// A position
    /// </summary>
    public double A { get; set; }

    /// <summary>
    /// B position
    /// </summary>
    public double B { get; set; }

    /// <summary>
    /// C position
    /// </summary>
    public double C { get; set; }

    /// <summary>
    /// UTC Date/time of the position
    /// 
    /// It is used to compute the feedrate
    /// </summary>
    public DateTime Time { get; set; }

    static readonly ILog log = LogManager.GetLogger(typeof (Position).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public Position(double x, double y, double z)
    {
      this.X = x;
      this.Y = y;
      this.Z = z;
      this.U = 0;
      this.V = 0;
      this.W = 0;
      this.A = 0;
      this.B = 0;
      this.C = 0;
      this.Time = DateTime.UtcNow;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="p"></param>
    public Position (JPosition p)
    {
      this.X = p.X ?? 0.0;
      this.Y = p.Y ?? 0.0;
      this.Z = p.Z ?? 0.0;
      this.U = p.U ?? 0.0;
      this.V = p.V ?? 0.0;
      this.W = p.W ?? 0.0;
      this.A = p.A ?? 0.0;
      this.B = p.B ?? 0.0;
      this.C = p.C ?? 0.0;
      this.Time = p.Time;
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="p"></param>
    public Position (IPosition p)
    {
      this.X = p.GetAxisValue ("X") ?? 0.0;
      this.Y = p.GetAxisValue ("Y") ?? 0.0;
      this.Z = p.GetAxisValue ("Z") ?? 0.0;
      this.U = p.GetAxisValue ("U") ?? 0.0;
      this.V = p.GetAxisValue ("V") ?? 0.0;
      this.W = p.GetAxisValue ("W") ?? 0.0;
      this.A = p.GetAxisValue ("A") ?? 0.0;
      this.B = p.GetAxisValue ("B") ?? 0.0;
      this.C = p.GetAxisValue ("C") ?? 0.0;
      this.Time = p.Time;
    }

    /// <summary>
    /// <see cref="Object.ToString" />
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      string xyz = string.Format(" X={0} Y={1} Z={2}", this.X, this.Y, this.Z);
      string uvw = "";
      if ( (this.U != 0.0) || (this.V != 0.0) || (this.W != 0.0)) {
        uvw = string.Format(" U={0} V={1} W={2}", this.U, this.V, this.W);
      }
      string abc = "";
      if ( (this.A != 0.0) || (this.B != 0.0) || (this.C != 0.0)) {
        abc = string.Format(" A={0} B={1} C={2}", this.A, this.B, this.C);
      }
      return string.Format("[Position{0}{1}{2}]", xyz, uvw, abc);
    }

    double GetDoubleValue ()
    {
      return Math.Sqrt ((this.X + this.U) * (this.X + this.U)
                       + (this.Y + this.V) * (this.Y + this.V)
                       + (this.Z + this.W) * (this.Z + this.W));
    }

    /// <summary>
    /// <see cref="IConvertible.GetTypeCode" />
    /// </summary>
    /// <returns></returns>
    public TypeCode GetTypeCode ()
    {
      return TypeCode.Object;
    }

    bool IConvertible.ToBoolean (IFormatProvider provider)
    {
      if ((this.X != 0.0) || (this.Y != 0.0) || (this.Z != 0.0)
          || (this.U != 0.0) || (this.V != 0.0) || (this.W != 0.0)
          || (this.A != 0.0) || (this.B != 0.0) || (this.C != 0.0)) {
        return true;
      }
      else {
        return false;
      }
    }

    byte IConvertible.ToByte(IFormatProvider provider)
    {
      return Convert.ToByte(GetDoubleValue());
    }

    char IConvertible.ToChar(IFormatProvider provider)
    {
      return Convert.ToChar(GetDoubleValue());
    }

    DateTime IConvertible.ToDateTime(IFormatProvider provider)
    {
      return Convert.ToDateTime(GetDoubleValue());
    }

    decimal IConvertible.ToDecimal(IFormatProvider provider)
    {
      return Convert.ToDecimal(GetDoubleValue());
    }

    double IConvertible.ToDouble(IFormatProvider provider)
    {
      return GetDoubleValue();
    }

    short IConvertible.ToInt16(IFormatProvider provider)
    {
      return Convert.ToInt16(GetDoubleValue());
    }

    int IConvertible.ToInt32(IFormatProvider provider)
    {
      return Convert.ToInt32(GetDoubleValue());
    }

    long IConvertible.ToInt64(IFormatProvider provider)
    {
      return Convert.ToInt64(GetDoubleValue());
    }

    sbyte IConvertible.ToSByte(IFormatProvider provider)
    {
      return Convert.ToSByte(GetDoubleValue());
    }

    float IConvertible.ToSingle(IFormatProvider provider)
    {
      return Convert.ToSingle(GetDoubleValue());
    }

    string IConvertible.ToString(IFormatProvider provider)
    {
      return this.ToString ();
    }

    object IConvertible.ToType(Type conversionType, IFormatProvider provider)
    {
      return Convert.ChangeType(GetDoubleValue(),conversionType);
    }

    ushort IConvertible.ToUInt16(IFormatProvider provider)
    {
      return Convert.ToUInt16(GetDoubleValue());
    }

    uint IConvertible.ToUInt32(IFormatProvider provider)
    {
      return Convert.ToUInt32(GetDoubleValue());
    }

    ulong IConvertible.ToUInt64(IFormatProvider provider)
    {
      return Convert.ToUInt64(GetDoubleValue());
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
      case "X":
        return this.X;
      case "Y":
        return this.Y;
      case "Z":
        return this.Z;
      case "U":
        return this.U;
      case "V":
        return this.V;
      case "W":
        return this.W;
      case "A":
        return this.A;
      case "B":
        return this.B;
      case "C":
        return this.C;
      default:
        throw new ArgumentException ("Invalid axis name", "axisName");
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

    /// <summary>
    /// <see cref="IEquatable{T}"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals (Position other)
    {
      return other.X == this.X && other.Y == this.Y && other.Z == this.Z && other.A == this.A && other.B == this.B && other.C == this.C && other.U == this.U && other.V == this.V && other.W == this.W;
    }
    #endregion // IPosition implementation
  }
}
