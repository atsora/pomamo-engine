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
  /// Migration 118: add a comparison operator between a boolean and an integer
  /// so that boolean values can be used in C++ / with ODBC
  /// </summary>
  [Migration(118)]
  public class AddIntBoolComparison: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddIntBoolComparison).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"create or replace function inttobool(num int,val bool) returns bool as
$$
begin
if num=0 and not val then
        return true;
elsif num<>0 and val then
        return true;
else return false;
end if;
end;
$$ language 'plpgsql';");
      Database.ExecuteNonQuery (@"create or replace function inttobool(val bool, num int) returns bool as
$$
begin
        return inttobool(num,val);
end;
$$ language 'plpgsql';");
      Database.ExecuteNonQuery (@"create or replace function notinttobool(val bool, num int) returns bool as
$$
begin
        return not inttobool(num,val);
end;
$$ language 'plpgsql';");
      Database.ExecuteNonQuery (@"create or replace function notinttobool(num int, val bool) returns bool as
$$
begin
        return not inttobool(num,val);
end;
$$ language 'plpgsql';");
      Database.ExecuteNonQuery (@"CREATE OPERATOR = (
    leftarg = integer,
    rightarg = boolean,
    procedure = inttobool,
    commutator = =,
    negator = !=
);");
      Database.ExecuteNonQuery (@"CREATE OPERATOR = (
    leftarg = boolean,
    rightarg = integer,
    procedure = inttobool,
    commutator = =,
    negator = !=
);");
      Database.ExecuteNonQuery (@"CREATE OPERATOR <> (
    leftarg = integer,
    rightarg = boolean,
    procedure = notinttobool,
    commutator = <>,
    negator = =
);");
      Database.ExecuteNonQuery (@"CREATE OPERATOR <> (
    leftarg = boolean,
    rightarg = integer,
    procedure = notinttobool,
    commutator = <>,
    negator = =
);");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"DROP OPERATOR IF EXISTS <> (boolean, integer);");
      Database.ExecuteNonQuery (@"DROP OPERATOR IF EXISTS <> (integer, boolean);");
      Database.ExecuteNonQuery (@"DROP OPERATOR IF EXISTS = (boolean, integer);");
      Database.ExecuteNonQuery (@"DROP OPERATOR IF EXISTS = (integer, boolean);");
      Database.ExecuteNonQuery (@"DROP FUNCTION IF EXISTS inttobool(int, bool) CASCADE");
      Database.ExecuteNonQuery (@"DROP FUNCTION IF EXISTS inttobool(bool, int) CASCADE;");
      Database.ExecuteNonQuery (@"DROP FUNCTION IF EXISTS notinttobool(val bool, num int) CASCADE");
      Database.ExecuteNonQuery (@"DROP FUNCTION IF EXISTS notinttobool(num int, val bool) CASCADE");
    }
  }
}
