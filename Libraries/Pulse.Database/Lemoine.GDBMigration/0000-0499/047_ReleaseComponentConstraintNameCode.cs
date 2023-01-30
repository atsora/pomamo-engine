// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 047: release one constraint in table Component
  /// - on table component, name or code or type must be set
  /// </summary>
  [Migration(47)]
  public class ReleaseComponentConstraintNameCode: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ReleaseComponentConstraintNameCode).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.RemoveConstraint (TableName.COMPONENT,
                                 "component_name_code");
      Database.AddCheckConstraint ("component_name_code_type",
                                   TableName.COMPONENT,
                                   "((componentname IS NOT NULL) OR (componentcode IS NOT NULL) OR (componenttypeid <> 1))");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveConstraint (TableName.COMPONENT,
                                 "component_name_code_type");
      Database.AddCheckConstraint ("component_name_code",
                                   TableName.COMPONENT,
                                   "((componentname IS NOT NULL) OR (componentcode IS NOT NULL))");
    }
  }
}
