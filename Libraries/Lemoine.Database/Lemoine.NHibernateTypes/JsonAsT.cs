// Copyright (C) 2009-2023 Lemoine Automation Technologies
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

#if NETCOREAPP3_1_OR_GREATER
    JsonSerializerOptions GetSerializerOptions () => new JsonSerializerOptions {
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault,
      PropertyNameCaseInsensitive = true,
    };
#else // !NETCOREAPP3_1_OR_GREATER
    JsonSerializerSettings GetSerializationSettings ()
    {
      return new JsonSerializerSettings () {
        DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore,
      };
    }
#endif // !NETCOREAPP3_1_OR_GREATER

    /// <summary>
    /// Deserialize a dictionary
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override T Get (object v)
    {
      try {
#if NETCOREAPP3_1_OR_GREATER
        return JsonSerializer.Deserialize<T> (v.ToString (), GetSerializerOptions ());
#else // !NETCOREAPP3_1_OR_GREATER
        return JsonConvert.DeserializeObject<T> (v.ToString (), GetSerializationSettings ());
#endif // !NETCOREAPP3_1_OR_GREATER
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
#if NETCOREAPP3_1_OR_GREATER
      var json = JsonSerializer.Serialize (v, GetSerializerOptions ());
#else // !NETCOREAPP3_1_OR_GREATER
      var json = JsonConvert.SerializeObject (v, GetSerializationSettings ());
#endif // !NETCOREAPP3_1_OR_GREATER
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
#if NETCOREAPP3_1_OR_GREATER
        var serializerOptions = GetSerializerOptions ();
        return JsonSerializer.Deserialize<T> (JsonSerializer.Serialize (v, serializerOptions), serializerOptions);
#else // !NETCOREAPP3_1_OR_GREATER
        var serializationSettings = GetSerializationSettings ();
        return JsonConvert.DeserializeObject<T> (JsonConvert.SerializeObject (v, serializationSettings), serializationSettings);
#endif // !NETCOREAPP3_1_OR_GREATER
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
