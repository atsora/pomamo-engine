// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;
using Migrator.Framework;

#if TEST
using NUnit.Framework;
#endif // TEST

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Extends class Migrator.Framework.Migration
  /// with some useful methods
  /// </summary>
  public abstract class GDBMigration: Migration
  {
    /// <summary>
    /// Add an index on the given columns
    /// </summary>
    /// <param name="table"></param>
    /// <param name="columns"></param>
    public void AddIndex (string table, IEnumerable <Column> columns)
    {
      string indexName = BuildIndexName (table, columns);
      List <string> columnsArray = new List <string> ();
      foreach (Column column in columns) {
        columnsArray.Add (column.Name);
      }
      string columnsStringList = String.Join (",",
                                              columnsArray.ToArray ());
      Database.ExecuteNonQuery (String.Format ("DROP INDEX IF EXISTS {0}",
                                               indexName));
      Database.ExecuteNonQuery (String.Format ("CREATE INDEX {1} " +
                                               "ON {0} " +
                                               "USING btree " +
                                               "({2});",
                                               table, indexName,
                                               columnsStringList));
    }
    
    /// <summary>
    /// Remove the index on the given columns
    /// </summary>
    /// <param name="table"></param>
    /// <param name="columns"></param>
    public void RemoveIndex (string table, IEnumerable <Column> columns)
    {
      string indexName = BuildIndexName (table, columns);
      Database.ExecuteNonQuery (String.Format ("DROP INDEX {0}",
                                               indexName));
    }
    
    /// <summary>
    /// Build the index name from the list of columns
    /// in a similar but slightly different way it is done in C++.
    /// 
    /// The returned values are not the same.
    /// </summary>
    /// <param name="table"></param>
    /// <param name="columns"></param>
    /// <returns></returns>
    private static string BuildIndexName (string table, IEnumerable <Column> columns)
    {
      char [] resultArray = new char [0];
      foreach (Column column in columns) {
        char [] columnArray = column.Name.ToLowerInvariant ().ToCharArray ();
        for (int i = 0;
             (i < resultArray.Length) && (i < columnArray.Length);
             ++i) {
          resultArray [i] += columnArray [i];
          while (resultArray [i] > 'z') {
            resultArray [i] -= (char) (26);
          }
        }
        if (columnArray.Length > resultArray.Length) {
          char [] newArray = (char[]) Array.CreateInstance (typeof (char), columnArray.Length);
          Array.Copy (resultArray, 0, newArray, 0, resultArray.Length);
          for (int i = resultArray.Length; i < columnArray.Length; ++i) {
            newArray [i] = columnArray [i];
          }
          resultArray = newArray;
        }
      }
      return table + "_" + new String (resultArray);
    }
  }
}
