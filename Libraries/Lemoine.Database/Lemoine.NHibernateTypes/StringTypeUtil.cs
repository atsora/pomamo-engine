// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Utilities to manage the string types
  /// </summary>
  internal static class StringTypeUtil
  {
    /// <summary>
    /// Escape the strings, for example when a single quote is used.
    /// 
    /// This is necessary since Npgsql 2.2.5.0
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    internal static string EscapeString (string s)
    {
      // Note: since the upgrade of Npgsql to 2.2.5.0, the single quote is not escaped correctly
      return s.Replace ("'", "''");
    }
  }
}
