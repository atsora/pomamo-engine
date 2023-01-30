// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Conversion
{
  /// <summary>
  /// Extension methods to <see cref="Type"/>
  /// </summary>
  public static class TypeExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (TypeExtensions).FullName);

    /// <summary>
    /// Test if a type is numeric
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    static public bool IsNumeric (this Type t)
    {
      switch (Type.GetTypeCode (t)) {
      case TypeCode.Byte:
      case TypeCode.SByte:
      case TypeCode.UInt16:
      case TypeCode.UInt32:
      case TypeCode.UInt64:
      case TypeCode.Int16:
      case TypeCode.Int32:
      case TypeCode.Int64:
      case TypeCode.Decimal:
      case TypeCode.Double:
      case TypeCode.Single:
        return true;
      default:
        return false;
      }
    }

  }
}
