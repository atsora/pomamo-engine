// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Linq;
using System.IO;
using Pulse.Business.Computer;
using Lemoine.Core.Hosting;
using Lemoine.Core.Hosting.LctrChecker;
using Lemoine.ModelDAO.LctrChecker;

namespace Lemoine.Business.Computer
{
  /// <summary>
  /// Method to test computers
  /// </summary>
  public static class ComputerTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ComputerTest).FullName);

    /// <summary>
    /// Check if the local computer is lctr.
    /// 
    /// If the database connection is not ok, then it is checked if the directory l_ctr\pfrdata exists
    /// </summary>
    /// <param name="checkInDatabase">check also the lctr from database</param>
    /// <returns></returns>
    public static bool IsLctr (bool checkInDatabase = true)
    {
      var lctrChecker = new CheckDatabaseLctrChecker (new DatabaseLctrChecker (), null);

      return lctrChecker.IsLctr ();
    }

  }
}
