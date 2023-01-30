// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.ModelDAO.LctrChecker
{
  /// <summary>
  /// Check if a computer is lctr using the database
  /// </summary>
  public class DatabaseLctrChecker : IDatabaseLctrChecker
  {
    readonly ILog log = LogManager.GetLogger (typeof (DatabaseLctrChecker).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public DatabaseLctrChecker ()
    {
    }

    /// <summary>
    /// <see cref="ILctrChecker"/>
    /// </summary>
    /// <returns></returns>
    public bool IsLctr ()
    {
      using (var session = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var lctrComputer = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.ComputerDAO
          .GetLctr ();
        if (null == lctrComputer) {
          log.Fatal ("IsLctr: no lctr is defined in table computer => return false");
          return false;
        }
        var isLctr = lctrComputer.IsLocal ();
        if (log.IsDebugEnabled) {
          log.Debug ($"IsLctrFromDatabase: return {isLctr}");
        }
        return isLctr;
      }
    }
  }
}
