// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.I18N;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 002: deprecated
  /// </summary>
  [Migration(2)]
  public class AddRowNotDefined: Migration
  {
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
