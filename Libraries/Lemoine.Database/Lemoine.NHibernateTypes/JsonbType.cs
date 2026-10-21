// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using NHibernate.SqlTypes;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Using string for Jsonb
  /// </summary>
  [Serializable]
  public class JsonbType : SimpleGenericType<string>
  {
    static readonly ILog log = LogManager.GetLogger<JsonbType> ();

    /// <summary>
    /// Constructor
    /// </summary>
    public JsonbType () : base (new SqlType (DbType.Object), NpgsqlTypes.NpgsqlDbType.Jsonb, true)
    {
    }

    /// <summary>
    /// Deserialize a dictionary
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override string Get (object v) => v.ToString ();

    /// <summary>
    /// Serialize a dictionary
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override object Set (string v) => v;

    /// <summary>
    /// Deep copy implementation
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override string DeepCopyValue (string v) => v;

    /// <summary>
    /// Compare the JSON contents, not the texts
    ///
    /// PostgreSQL returns a jsonb in its own normalized form (white spaces, order of the keys),
    /// which differs from the text the application sets. Comparing the texts would make
    /// the entity dirty, and updated at each flush, including in a read-only transaction
    /// </summary>
    /// <param name="x">not null</param>
    /// <param name="y">not null</param>
    /// <returns></returns>
    public override bool TestEquality (string x, string y) => Lemoine.Conversion.JsonComparison.AreEqual (x, y);

    #region AbstractType implementation
    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public override string Name => "Jsonb";
    #endregion AbstractType implementation
  }
}
