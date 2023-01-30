// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#if NET6_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization;
#else // !NET6_0_OR_GREATER
using Newtonsoft.Json;
#endif // !NET6_0_OR_GREATER
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;

namespace Lemoine.Model
{
#if NET6_0_OR_GREATER
  /// <summary>
  /// Convert a Json to a string
  /// </summary>
  public class JsonToStringConverter : JsonConverter<string>
  {
    /// <summary>
    /// <see cref="JsonConverter{T}"/>
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns>nullable string</returns>
    public override string Read (ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      using (var doc = JsonDocument.ParseValue (ref reader)) {
        return doc.RootElement.GetRawText ();
      }
    }

    /// <summary>
    /// <see cref="JsonConverter{T}"/>
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write (Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
      writer.WriteRawValue (value);
    }
  }
#endif // NET6_0_OR_GREATER

  /// <summary>
  /// Scoped or Singleton service
  /// </summary>
  public class StampingService : IEquatable<StampingService>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (StampingService).FullName);

    /// <summary>
    /// Full type name of the singleton service name
    /// 
    /// not null
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Full type name of the singleton implementation
    /// 
    /// not null
    /// </summary>
    public string ImplementationName { get; set; } = "";

    /// <summary>
    /// Which service should be used for that name ?
    /// 
    /// not null
    /// </summary>
    public string DelegationName { get; set; } = "";

    /// <summary>
    /// Json
    /// 
    /// nullable
    /// </summary>
#if NET6_0_OR_GREATER
    [JsonConverter (typeof (JsonToStringConverter))]
#else // !NET6_0_OR_GREATER
    [JsonIgnore]
#endif // !NET6_0_OR_GREATER
    public string Json { get; set; } = null;

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
#if NET6_0_OR_GREATER
      var json = JsonSerializer.Serialize (this);
      return $"StampingService={json}";
#else // !NET6_0_OR_GREATER
      return "StampingService";
#endif // !NET6_0_OR_GREATER
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      StampingService other = obj as StampingService;
      if (null == other) {
        return false;
      }
      if (!object.Equals (this.Name, other.Name)
        || !object.Equals (this.ImplementationName, other.ImplementationName)
        || !object.Equals (this.DelegationName, other.DelegationName)) {
        return false;
      }
      if (string.IsNullOrEmpty (this.Json)) {
        return string.IsNullOrEmpty (other.Json);
      }
      else if (string.IsNullOrEmpty (other.Json)) {
        return false;
      }
      // Note: for the moment there is no good tool in System.Text.Json to compare two Json objects
      // Use Newtonsoft instead
      Newtonsoft.Json.Linq.JObject a, b;
      try {
        a = Newtonsoft.Json.Linq.JObject.Parse (this.Json);
      }
      catch (Exception ex) {
        log.Error ($"Equals: this.Json={this.Json} is not a valid json", ex);
        throw;
      }
      try {
        b = Newtonsoft.Json.Linq.JObject.Parse (other.Json);
      }
      catch (Exception ex) {
        log.Error ($"Equals: other.Json={other.Json} is not a valid json", ex);
        throw;
      }

      try {
        return Newtonsoft.Json.Linq.JToken.DeepEquals (a, b);
      }
      catch (Exception ex) {
        log.Error ($"Equals: DeepEquals {this.Json} VS {other.Json} returned an exception", ex);
        throw;
      }
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="other">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public virtual bool Equals (StampingService other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode ()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * Name.GetHashCode ();
        hashCode += 1000000009 * ImplementationName.GetHashCode ();
        hashCode += 1000000011 * DelegationName.GetHashCode ();
        if (null != this.Json) {
          var jsonObject = Newtonsoft.Json.Linq.JToken.Parse (this.Json);
          var jsonComparer = new Newtonsoft.Json.Linq.JTokenEqualityComparer ();
          hashCode += 1000000013 * jsonComparer.GetHashCode (jsonObject);
        }
      }
      return hashCode;
    }
  }

  /// <summary>
  /// StampingConfig
  /// </summary>
  public class StampingConfig : IEquatable<StampingConfig>
  {
    readonly ILog log = LogManager.GetLogger (typeof (StampingConfig).FullName);

    #region Getters / Setters
    /// <summary>
    /// Full type name of the stamping file flow
    /// </summary>
    public string StampingFileFlow { get; set; } = "";

    /// <summary>
    /// Full type name of the stamper
    /// </summary>
    public string Stamper { get; set; } = "";

    /// <summary>
    /// Full type name of the stamping parser
    /// </summary>
    public string StampingParser { get; set; } = "";

    /// <summary>
    /// Full type names of additional stamping service configurators
    /// </summary>
    public IList<string> Configurators { get; set; } = new List<string> ();

    /// <summary>
    /// Singleton definitions
    /// </summary>
    public IList<StampingService> Singletons { get; set; } = new List<StampingService> ();

    /// <summary>
    /// Scoped service definitions
    /// </summary>
    public IList<StampingService> ScopedServices { get; set; } = new List<StampingService> ();

    /// <summary>
    /// Full type names of the event handlers
    /// </summary>
    public IList<string> EventHandlers { get; set; } = new List<string> ();
    #endregion // Getters / Setters

    /// <summary>
    /// Constructor
    /// </summary>
    public StampingConfig ()
    {
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      var json =
#if NET6_0_OR_GREATER
        JsonSerializer.Serialize (this);
#else // !NET6_0_OR_GREATER
        Newtonsoft.Json.JsonConvert.SerializeObject (this);
#endif // !NET6_0_OR_GREATER
      return $"StampingConfig={json}";
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      StampingConfig other = obj as StampingConfig;
      if (null == other) {
        return false;
      }
      return object.Equals (this.StampingFileFlow, other.StampingFileFlow)
        && object.Equals (this.Stamper, other.Stamper)
        && object.Equals (this.StampingParser, other.StampingParser)
        && SetEqual (this.Configurators, other.Configurators)
        && SetEqual (this.Singletons, other.Singletons)
        && SetEqual (this.ScopedServices, other.ScopedServices)
        && this.EventHandlers.SequenceEqual (other.EventHandlers);
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="other">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public virtual bool Equals (StampingConfig other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode ()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * StampingFileFlow.GetHashCode ();
        hashCode += 1000000009 * Stamper.GetHashCode ();
        hashCode += 1000000011 * StampingParser.GetHashCode ();
        hashCode += 1000000013 * SetGetHashCode (this.Configurators);
        hashCode += 1000000015 * SetGetHashCode (this.Singletons);
        hashCode += 1000000017 * SetGetHashCode (this.ScopedServices);
        var m = 1000000017;
        foreach (var eventHandler in this.EventHandlers) {
          m += 2;
          hashCode += m * eventHandler.GetHashCode ();
        }
      }
      return hashCode;
    }

    bool SetEqual<T> (IList<T> a, IList<T> b)
    {
      if (a is null && b is null) {
        return true;
      }
      if (a is null || b is null) {
        // For Newtonsoft to avoid any dirty item
        if (a is null) {
          return !b.Any ();
        }
        else {
          return !a.Any ();
        }
      }
      return a.Count == b.Count
        && a.All (x => b.Any (y => object.Equals (x, y)));
    }

    int SetGetHashCode<T> (IEnumerable<T> items)
    {
      if (items is null) {
        return 0;
      }
      int hashCode = 266136261;
      foreach (var item in items) {
        hashCode ^= item.GetHashCode ();
      }
      return hashCode;
    }
  }

}
