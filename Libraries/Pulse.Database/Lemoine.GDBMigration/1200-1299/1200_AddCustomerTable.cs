// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Diagnostics;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1200: add a new table customer 
  /// </summary>
  [Migration (1200)]
  public class AddCustomerTable : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddCustomerTable).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.CUSTOMER)) {
        Database.AddTable (TableName.CUSTOMER,
                           new Column (ColumnName.CUSTOMER_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                           new Column ($"{TableName.CUSTOMER}version", DbType.Int32, ColumnProperty.NotNull, 1),
                           new Column ($"{TableName.CUSTOMER}name", DbType.String),
                           new Column ($"{TableName.CUSTOMER}code", DbType.String),
                           new Column ($"{TableName.CUSTOMER}externalcode", DbType.String, ColumnProperty.Unique));
        MakeColumnCaseInsensitive (TableName.CUSTOMER, $"{TableName.CUSTOMER}name");
        MakeColumnCaseInsensitive (TableName.CUSTOMER, $"{TableName.CUSTOMER}code");
        AddUniqueConstraint (TableName.CUSTOMER, new string[] { $"{TableName.CUSTOMER}name" });
        AddUniqueConstraint (TableName.CUSTOMER, new string[] { $"{TableName.CUSTOMER}code" });
        AddUniqueConstraint (TableName.CUSTOMER, new string[] { $"{TableName.CUSTOMER}externalcode" });
        Database.AddCheckConstraint ("customer_name_code",
          TableName.CUSTOMER,
          $"(({TableName.CUSTOMER}name IS NOT NULL) OR ({TableName.CUSTOMER}code IS NOT NULL))");
      }
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TableName.CUSTOMER)) {
        Database.RemoveTable (TableName.CUSTOMER);
      }
    }
  }
}
