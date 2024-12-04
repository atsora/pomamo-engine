// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lemoine.IO
{
  /// <summary>
  /// Extension to System.IO.Path
  /// </summary>
  public class Path
  {
    /// <summary>
    /// Check if two paths are equal
    /// </summary>
    /// <param name="path1"></param>
    /// <param name="path2"></param>
    /// <returns></returns>
    public static bool IsSame (string path1, string path2)
    {
      var p1 = System.IO.Path.GetFullPath (path1);
      var p2 = System.IO.Path.GetFullPath (path2);
      var caseSensitive = IsCaseSensitive ();
      return string.Equals (p1, p2, caseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Check if a path is case sensitive
    /// </summary>
    /// <returns></returns>
    public static bool IsCaseSensitive ()
    {
#if NET48_OR_GREATER || NETCOREAPP
      if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)
        || RuntimeInformation.IsOSPlatform (OSPlatform.OSX)) { // HFS+ (the Mac file-system) is usually configured to be case insensitive.
        return false;
      }
      else if (RuntimeInformation.IsOSPlatform (OSPlatform.Linux)) {
        return true;
      }
      else if (Environment.OSVersion.Platform == PlatformID.Unix) {
        return true;
      }
      else {
        // A default.
        return false;
      }
#else // not NET48 or NETCOREAPP
      return false;
#endif
    }
  }
}
