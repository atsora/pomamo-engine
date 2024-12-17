// Copyright (C) 2024 Atsora Solutions

using Lemoine.Business.Extension;
using Lemoine.Core.Log;
using Pulse.Extensions.Business.Reason;
using System;
using System.Collections.Generic;
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
    /// <param name="jsonElement"></param>
    /// <returns></returns>
    public static async Task<IDictionary<string, object>> DeserializeAsync (string json)
    {
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

    /// <summary>
    /// Deserialize a value if the extensions are known
    /// </summary>
    /// <param name="json"></param>
    /// <param name="extensions"></param>
    /// <returns></returns>
    public static IDictionary<string,object> Deserialize (string json, IEnumerable<IReasonDataExtension> extensions)
    {
      var a = JsonSerializer.Deserialize<IDictionary<string, JsonElement>> (json);
      var d = new Dictionary<string, object> ();
      foreach (var kv in a) {
        if (TryDeserializeItem (kv, extensions, out var v)) {
          d[kv.Key] = v;
        }
      }
      return d;
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

  }
}
