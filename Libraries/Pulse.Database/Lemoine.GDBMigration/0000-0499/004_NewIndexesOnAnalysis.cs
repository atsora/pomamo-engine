// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 004: deprecated
  /// </summary>
  [Migration(04)]
  public class NewIndexesOnAnalysis: GDBMigration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (NewIndexesOnAnalysis).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
