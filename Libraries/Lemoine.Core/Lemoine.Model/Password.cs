// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NETSTANDARD

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Lemoine.Model
{
  /// <summary>
  /// Methods to create or check a password
  /// </summary>
  public static class Password
  {
    readonly static ILog log = LogManager.GetLogger (typeof (Password).FullName);

    static readonly string HASH_SALT_KEY = "user.salt"; // Or primary password

    public static bool IsMatch (string x, string y)
    {
      if (string.IsNullOrEmpty (y)) {
        return false;
      }
      if (string.Equals (x, y)) {
        return true;
      }
      var salt = Lemoine.Info.ConfigSet.LoadAndGet (HASH_SALT_KEY, "");
      if (!string.IsNullOrEmpty (salt)) {
        var hashedx = Hash (x, salt);
        if (hashedx.Equals (y)) {
          return true;
        }
        var hashedy = Hash (y, salt);
        if (hashedy.Equals (x)) {
          return true;
        }
      }

      return false;
    }

      /// <summary>
      /// Hash if a salt was configured
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public static string HashIfConfigured (string s)
    {
      var salt = Lemoine.Info.ConfigSet.LoadAndGet (HASH_SALT_KEY, "");
      if (string.IsNullOrEmpty (salt)) {
        return s;
      }
      else {
        return Hash (s, salt);
      }
    }

    /// <summary>
    /// Hash a password
    /// </summary>
    /// <param name="s"></param>
    /// <param name="saltString"></param>
    /// <returns></returns>
    public static string Hash (string s, string saltString)
    {
      var salt = Convert.FromBase64String (saltString);
      return Convert.ToBase64String (KeyDerivation.Pbkdf2 (
       password: s,
        salt: salt,
        prf: KeyDerivationPrf.HMACSHA1,
        iterationCount: 10000,
        numBytesRequested: 256 / 8));
    }
  }
}

#endif // NETSTANDARD
