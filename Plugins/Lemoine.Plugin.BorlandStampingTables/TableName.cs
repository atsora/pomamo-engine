// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemoine.Plugin.BorlandStampingTables
{
  /// <summary>
  /// Table name constants
  /// </summary>
  internal class TableName : Lemoine.GDBMigration.TableName
  {
    /// <summary>
    /// maintenanceaction table
    /// </summary>
    public static readonly string SFK_CAMSYSTEM = "sfkcamsystem";
    /// <summary>
    /// maintenanceactiontype table
    /// </summary>
    public static readonly string SFK_MACHTYPE = "sfkmachtype";
  }
}
