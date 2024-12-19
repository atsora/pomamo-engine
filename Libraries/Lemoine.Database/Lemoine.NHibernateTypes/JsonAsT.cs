// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using System.Text.Json;
using System.Text.Json.Serialization;
using NHibernate.SqlTypes;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Convert an Json to a serializable type using Newtonsoft for .Net 4.8 and System.Text.Json for .Net Core
  /// 
  /// To use it in the mapping file:
  /// type="Lemoine.NHibernateTypes.JsonAsT`1[[Lemoine.Model.CncAlarmSeverityPatternRules, Lemoine.ModelDAO]], Lemoine.Database"
  /// </summary>
  [Serializable]
  public class JsonAsT<T> : SimpleGenericType<T>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (JsonAsT<T>).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public JsonAsT () : base (new SqlType (DbType.Object), NpgsqlTypes.NpgsqlDbType.Jsonb, true)
    {

    }

    JsonSerializerOptions GetSerializerOptions () => new JsonSerializerOptions {
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault,
      PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Deserialize a dictionary
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override T Get (object v)
    {
      try {
        return JsonSerializer.Deserialize<T> (v.ToString (), GetSerializerOptions ());
      }
      catch (Exception ex) {
        string message = $"Data {v} is not a Json";
        log.Error ($"Get: {message},", ex);
        throw new FormatException (message, ex);
      }
    }

    /// <summary>
    /// Serialize a dictionary
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override object Set (T v)
    {
      var json = JsonSerializer.Serialize (v, GetSerializerOptions ());
      return json;
    }

    /// <summary>
    /// Deep copy implementation
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override T DeepCopyValue (T v)
    {
      // Serialize and deserialize
      try {
        var serializerOptions = GetSerializerOptions ();
        return JsonSerializer.Deserialize<T> (JsonSerializer.Serialize (v, serializerOptions), serializerOptions);
      }
      catch (Exception ex) {
        log.Error ("DeepCopyValue: got exception", ex);
        throw;
      }
    }

    #region AbstractType implementation
    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public override string Name
    {
      get { return "JsonAs" + typeof (T).Name; }
    }
    #endregion AbstractType implementation
  }
}
