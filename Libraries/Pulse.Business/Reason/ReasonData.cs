// Copyright (C) 2024 Atsora Solutions

using Lemoine.Business.Extension;
using Lemoine.Core.Log;
using Pulse.Extensions.Business.Reason;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pulse.Business.Reason
{
  /// <summary>
  /// Helping class to manage a reason data
  /// </summary>
  public class ReasonData
  {
    static ILog log = LogManager.GetLogger<ReasonData> ();

    /// <summary>
    /// Deserialize a value
    /// </summary>
    /// <param name="json">not null</param>
    /// <returns></returns>
    public static async Task<IDictionary<string, object>> DeserializeAsync (string json)
    {
      Debug.Assert (null != json);
      var extensionRequest = new GlobalExtensions<IReasonDataExtension> (x => x.Initialize ());
      var extensions = await Lemoine.Business.ServiceProvider.GetAsync (extensionRequest);
      return Deserialize (json, extensions);
    }

    /// <summary>
    /// Deserialize a value
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <returns></returns>
    public static IDictionary<string, object> Deserialize (string json)
    {
      try {
        var extensionRequest = new GlobalExtensions<IReasonDataExtension> (x => x.Initialize ());
        var extensions = Lemoine.Business.ServiceProvider.Get (extensionRequest);
        var a = JsonSerializer.Deserialize<IDictionary<string, JsonElement>> (json);
        var d = new Dictionary<string, object> ();
        foreach (var kv in a) {
          if (TryDeserializeItem (kv, extensions, out var v)) {
            d[kv.Key] = v;
          }
        }
        return d;
      }
      catch (Exception ex) {
        log.Error ("Deserialize: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Deserialize a value if the extensions are known
    /// </summary>
    /// <param name="json">not null</param>
    /// <param name="extensions"></param>
    /// <returns></returns>
    public static IDictionary<string, object> Deserialize (string json, IEnumerable<IReasonDataExtension> extensions)
    {
      Debug.Assert (null != json);
      try {
        var a = JsonSerializer.Deserialize<IDictionary<string, JsonElement>> (json);
        var d = new Dictionary<string, object> ();
        foreach (var kv in a) {
          if (TryDeserializeItem (kv, extensions, out var v)) {
            d[kv.Key] = v;
          }
        }
        return d;
      }
      catch (Exception ex) {
        log.Error ($"Deserialize: exception", ex);
        throw;
      }
    }

    static bool TryDeserializeItem (KeyValuePair<string, JsonElement> kv, IEnumerable<IReasonDataExtension> extensions, out object v)
    {
      try {
        var extension = extensions.SingleOrDefault (x => x.Name.Equals (kv.Key, StringComparison.InvariantCultureIgnoreCase));
        if (extension is null) {
          log.Error ($"DeserializeItem: no item with name {kv.Key}");
          v = null;
          return false;
        }
        else {
          var t = extension.ReasonDataType;
          v = kv.Value.Deserialize (t);
          return true;
        }
      }
      catch (Exception ex) {
        log.Error ($"DeserializeItem: exception for name={kv.Key}", ex);
        v = null;
        return false;
      }
    }

    /// <summary>
    /// Test if the two data in Json format are equal
    /// </summary>
    /// <param name="json1"></param>
    /// <param name="json2"></param>
    /// <returns></returns>
    public static bool AreJsonEqual (string json1, string json2)
    {
      if (string.IsNullOrEmpty (json1)) {
        return string.IsNullOrEmpty (json2);
      }
      else { // json1 not null or empty 
        if (string.IsNullOrEmpty (json2)) {
          return false;
        }
        else { // both are not null or empty
          if (string.Equals (json1, json2)) {
            return true;
          }
          else {
            var node1 = System.Text.Json.Nodes.JsonNode.Parse (json1);
            var node2 = System.Text.Json.Nodes.JsonNode.Parse (json2);
            return System.Text.Json.Nodes.JsonNode.DeepEquals (node1, node2);
          }
        }
      }
    }
  }
}
