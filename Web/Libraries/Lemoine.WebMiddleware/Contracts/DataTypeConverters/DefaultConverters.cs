// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.WebMiddleware.Contracts.DataTypeConverters
{
  public static class DefaultConverters
  {
    internal static bool BoolConverter (string str)
    {
      string[] BooleanTrueStrings = { bool.TrueString, "1" };
      string[] BooleanFalseStrings = { bool.FalseString, "0" };

      if (string.IsNullOrEmpty (str)) {
        throw new FormatException ($"String \"{str}\" does not convert into a boolean.");
      }

      if (BooleanTrueStrings.Contains (str, StringComparer.InvariantCultureIgnoreCase)) {
        return true;
      }

      if (BooleanFalseStrings.Contains (str, StringComparer.InvariantCultureIgnoreCase)) {
        return false;
      }

      throw new FormatException ($"String \"{str}\" does not convert into a boolean.");
    }

    internal static T EnumConverter<T> (string str) where T : struct
    {
      if (Enum.TryParse (str, true, out T result)) {
        return result;
      }
      throw new FormatException ($"String \"{str}\" does not convert into Enum of type {typeof (T).Name}.");
    }
  }
}
