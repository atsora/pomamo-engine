// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Newtonsoft.Json;
using NHibernate.SqlTypes;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Convert an Json to a serializable type using Newtonsoft
  /// 
  /// To use it in the mapping file:
  /// type="Lemoine.NHibernateTypes.JsonAsTNewtonsoft`1[[Lemoine.Model.CncAlarmSeverityPatternRules, Lemoine.ModelDAO]], Lemoine.Database"
  /// </summary>
  [Serializable]
  public class JsonAsTNewtonsoft<T> : SimpleGenericType<T>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (JsonAsTNewtonsoft<T>).FullName);

    #region Members
    #endregion Members

    #region Constructor
    /// <summary>
    /// Constructor
    /// </summary>
    public JsonAsTNewtonsoft () : base (new SqlType (DbType.Object), NpgsqlTypes.NpgsqlDbType.Jsonb, true)
    {
      
    }
    #endregion Constructor

    #region Methods
    JsonSerializerSettings GetSerializationSettings ()
    {
      return new JsonSerializerSettings ()
      {
        DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
        NullValueHandling = NullValueHandling.Ignore,
      };
    }

    /// <summary>
    /// Deserialize a dictionary
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override T Get (object v)
    {
      try {
        return JsonConvert.DeserializeObject<T> (v.ToString (), GetSerializationSettings ());
      }
      catch (Exception ex) {
        string message = $"Data {v} is not a Json";
        log.ErrorFormat ($"Get: {message}", ex);
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
      var json = JsonConvert.SerializeObject (v, GetSerializationSettings ());
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
        var serializationSettings = GetSerializationSettings ();
        return JsonConvert.DeserializeObject<T> (JsonConvert.SerializeObject (v, serializationSettings), serializationSettings);
      }
      catch (Exception ex) {
        log.Error ($"DeepCopyValue: got exception", ex);
        throw;
      }
    }
    #endregion // Methods

    #region AbstractType implementation
    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public override string Name
    {
      get { return "NewtonsoftJsonAs" + typeof (T).Name; }
    }
    #endregion AbstractType implementation
  }
}
