// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Lemoine.Core.Log;
using NHibernate.SqlTypes;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Map a list of strings to a PostgreSQL text[] column
  ///
  /// The list is mutable: a modification of its items makes the entity dirty
  /// </summary>
  [Serializable]
  public class StringListAsTextArrayType : SimpleGenericType<IList<string>>
  {
    static readonly ILog log = LogManager.GetLogger<StringListAsTextArrayType> ();

    /// <summary>
    /// Constructor
    /// </summary>
    public StringListAsTextArrayType ()
      : base (new SqlType (DbType.Object), NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text, true)
    {
    }

    /// <summary>
    /// Compare the items of the lists, in order
    /// </summary>
    /// <param name="x">not null</param>
    /// <param name="y">not null</param>
    /// <returns></returns>
    public override bool TestEquality (IList<string> x, IList<string> y) => x.SequenceEqual (y, StringComparer.Ordinal);

    /// <summary>
    /// Convert the database value (a string[]) into a list, that can be modified
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override IList<string> Get (object v)
    {
      if (v is IEnumerable<string> strings) {
        return strings.ToList ();
      }
      log.Error ($"Get: unexpected type {v?.GetType ()} for a text array");
      throw new InvalidCastException ($"Unexpected type {v?.GetType ()} for a text array");
    }

    /// <summary>
    /// Convert the list into a string[]
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override object Set (IList<string> v) => v.ToArray ();

    /// <summary>
    /// Deep copy implementation
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override IList<string> DeepCopyValue (IList<string> v) => v.ToList ();

    /// <summary>
    /// A null value in database is returned as an empty list
    /// </summary>
    /// <returns></returns>
    protected override object GetNullValue () => new List<string> ();

    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public override string Name => "StringListAsTextArray";
  }
}
