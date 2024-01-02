// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Newtonsoft.Json;
using NHibernate.SqlTypes;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Use Dictionary as jsonb object in SQL.
  /// </summary>
  [Serializable]
  public class JsonAsDictionaryType : SimpleGenericType<IDictionary<string, object>>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (JsonAsDictionaryType).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public JsonAsDictionaryType ()
      : base (new SqlType (DbType.Object), NpgsqlTypes.NpgsqlDbType.Jsonb, true)
    { }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// <see cref="SimpleGenericType{T}"/>
    /// </summary>
    /// <param name="x">not null</param>
    /// <param name="y">not null</param>
    /// <returns></returns>
    public override bool TestEquality (IDictionary<string, object> x, IDictionary<string, object> y)
    {
      Debug.Assert (null != x);
      Debug.Assert (null != y);

      if (x.Count != y.Count) {
        return false;
      }
      if (x.Keys.Except (y.Keys).Any ()) {
        return false;
      }
      if (y.Keys.Except (x.Keys).Any ()) {
        return false;
      }
      foreach (var xi in x) {
        if (!object.Equals (xi.Value, y[xi.Key])) {
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Deserialize a dictionary
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override IDictionary<string, object> Get (object v)
    {
      try {
        return Deserialize (v.ToString ());
      }
      catch (Exception ex) {
        string message = $"Data {v} is not a Json";
        log.Error ($"Get: exception, {message}", ex);
        throw new FormatException (message, ex);
      }
    }

    /// <summary>
    /// Serialize a dictionary
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override object Set (IDictionary<string, object> v)
    {
      var json = JsonConvert.SerializeObject (v);
      return json;
    }

    /// <summary>
    /// Deep copy implementation
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override IDictionary<string, object> DeepCopyValue (IDictionary<string, object> v)
    {
      var dictionary = new Dictionary<string, object> ();
      foreach (var key in v.Keys) {
        dictionary[key] = v[key];
      }
      return dictionary;
    }

    IDictionary<string, object> Deserialize (string json)
    {
      return JsonConvert.DeserializeObject<IDictionary<string, object>> (json);
    }
    #endregion // Methods

    #region AbstractType implementation
    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public override string Name
    {
      get { return "JsonAsDictionary"; }
    }
    #endregion // AbstractType implementation
  }
}
