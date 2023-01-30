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
  /// Migration 140: add a comparison operator between 2 integers even if they are null
  /// </summary>
  [Migration(140)]
  public class AddEqualEvenNullComparison: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddEqualEvenNullComparison).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"create or replace function equalEvenNull(num1 int,num2 int) returns bool as
$$
begin
if num1=num2 
then
  return true;
elsif num1 is null and num2 is null  
then
  return true;
else return false;
end if;
end;
$$ language 'plpgsql';");

      Database.ExecuteNonQuery (@"DROP OPERATOR IF EXISTS === (integer, integer);");
      Database.ExecuteNonQuery (@"CREATE OPERATOR === (
    leftarg = integer,
    rightarg = integer,
    procedure = equalEvenNull,
    commutator = ===
);");
      
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"DROP OPERATOR IF EXISTS === (integer, integer);");
    }
  }
}