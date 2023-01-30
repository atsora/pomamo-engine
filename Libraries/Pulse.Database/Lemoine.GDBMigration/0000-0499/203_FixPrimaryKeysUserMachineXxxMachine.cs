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
  /// Migration 203:
  /// </summary>
  [Migration(203)]
  public class FixPrimaryKeysUserMachineXxxMachine: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixPrimaryKeysUserMachineXxxMachine).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"ALTER TABLE usermachineassociationmachine DROP CONSTRAINT IF EXISTS usermachineassociationmachine_pkey;");
      Database.ExecuteNonQuery (@"ALTER TABLE usermachineassociationmachine
  ADD CONSTRAINT usermachineassociationmachine_pkey PRIMARY KEY(modificationid, machineid);");

      Database.ExecuteNonQuery (@"ALTER TABLE usermachineslotmachine DROP CONSTRAINT IF EXISTS usermachineslotmachine_pkey;");
      Database.ExecuteNonQuery (@"ALTER TABLE usermachineslotmachine DROP CONSTRAINT IF EXISTS usermachineslotmachine_usermachineslotid_machineid_unique;");
      Database.ExecuteNonQuery (@"ALTER TABLE usermachineslotmachine
  ADD CONSTRAINT usermachineslotmachine_pkey PRIMARY KEY(usermachineslotid, machineid);");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
