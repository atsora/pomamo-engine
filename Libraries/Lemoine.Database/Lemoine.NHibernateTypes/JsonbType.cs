// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
#if NETCOREAPP3_1_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization;
#else // !NETCOREAPP3_1_OR_GREATER
using Newtonsoft.Json;
#endif // !NETCOREAPP3_1_OR_GREATER
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

    #region AbstractType implementation
    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public override string Name => "Jsonb";
    #endregion AbstractType implementation
  }
}
