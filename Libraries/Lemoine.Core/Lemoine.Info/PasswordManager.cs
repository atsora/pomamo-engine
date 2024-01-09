// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NETSTANDARD

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Info
{
  /// <summary>
  /// Password managerc
  /// </summary>
  public static class PasswordManager
  {
    readonly static ILog log = LogManager.GetLogger (typeof (PasswordManager).FullName);

#if ATSORA
    const string SALT = "atso";
    const string ADMIN_HASHED_PASSWORD = "/IPiHry3H1unO9XSr64wncNpuQGlSLLNa3dFOAG5oYY=";
    const string SUPERADMIN_HASHED_PASSWORD = "P4pBCfPb5A6sqkC0l6wkQ6s2gXpiA+i/1WlnaseXTQA=";
#elif LEMOINE
    const string SALT = "LemoinePulse";
    const string ADMIN_HASHED_PASSWORD = "YB09HQHiW1aXSC9GbGT2ObiX/bfuee+dNt0GFU/YQO8=";
    const string SUPERADMIN_HASHED_PASSWORD = "xLXTQUWhgbxh4wMk8hCbxLQKOtu9lGgIVtd0Enbe1Bo=";
#else
    const string SALT = "salt";
    const string ADMIN_HASHED_PASSWORD = "KL5SstCEoc3YHdhTme0O9oG0c4E7sJ5m12KmlTkBbx4=";
    const string SUPERADMIN_HASHED_PASSWORD = "NQV6wyBS5RND/ITr4WwN1GmoQnL8U45qOe/u2B32bOE=";
#endif

    /// <summary>
    /// Check if the password is a valid admin password
    /// </summary>
    /// <param name="password">not null</param>
    /// <returns></returns>
    public static bool IsAdmin (string password)
    {
      Debug.Assert (null != password);

      return ADMIN_HASHED_PASSWORD.Equals (Lemoine.Model.Password.Hash (password, SALT));
    }

    /// <summary>
    /// Check if the password is a valid superadmin password
    /// </summary>
    /// <param name="password">not null</param>
    /// <returns></returns>
    public static bool IsSuperAdmin (string password)
    {
      Debug.Assert (null != password);

      return SUPERADMIN_HASHED_PASSWORD.Equals (Lemoine.Model.Password.Hash (password, SALT));
    }

  }
}

#endif // NETSTANDARD
