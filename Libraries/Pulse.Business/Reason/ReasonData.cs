// Copyright (C) 2024 Atsora Solutions

using Lemoine.Business.Extension;
using Lemoine.Core.Log;
using Lemoine.Model;
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
      Debug.Assert (null != json);
      var extensionRequest = new GlobalExtensions<IReasonDataExtension> (x => x.Initialize ());
      var extensions = Lemoine.Business.ServiceProvider.Get (extensionRequest);
      return Deserialize (json, extensions);
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
        log.Error ($"Deserialize: exception for json={json}", ex);
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
    /// Merge the data from reasonSlot with the data from reasonMachineAssociation
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <param name="possibleReason"></param>
    /// <returns></returns>
    public static string Merge (IReasonSlot reasonSlot, IPossibleReason possibleReason)
    {
      if (!IsJsonNullOrEmpty (possibleReason.JsonData)) {
        if (IsJsonNullOrEmpty (reasonSlot.JsonData)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Merge: no existing reason data => keep the data from ReasonMachineAssociation");
          }
          return possibleReason.JsonData;
        }
        else { // reasonSlot.JsonData not null
          if (log.IsDebugEnabled) {
            log.Debug ($"Merge: old={reasonSlot.JsonData}, new={possibleReason.JsonData}");
          }
          var extensionRequest = new GlobalExtensions<IReasonDataExtension> (x => x.Initialize ());
          var extensions = Lemoine.Business.ServiceProvider.Get (extensionRequest).ToList ();
          if (extensions.Any (x => x.DoMerge (reasonSlot, possibleReason) || !x.Keep (reasonSlot, possibleReason))) {
            var data = Pulse.Business.Reason.ReasonData.Deserialize (reasonSlot.JsonData, extensions);
            data = data.ToDictionary (x => x.Key, x => x.Value); // Clone it
            var newData = Pulse.Business.Reason.ReasonData.Deserialize (possibleReason.JsonData, extensions);
            bool change = false;
            foreach (var extension in extensions.Where (ext => ext.DoMerge (reasonSlot, possibleReason) || !ext.Keep (reasonSlot, possibleReason))) {
              var name = extension.Name;
              if (newData.TryGetValue (name, out var o)) { // name is in both structures
                extension.Merge (data, o, possibleReason);
                change = true;
              }
              else if (!extension.Keep (reasonSlot, possibleReason)) { // Do not keep the old value
                data.Remove (name);
                change = true;
              }
            }
            if (change) {
              if (data.Any ()) {
                var newJsonData = JsonSerializer.Serialize (data);
                if (log.IsDebugEnabled) {
                  log.Debug ($"Merge: merged data is {newJsonData}");
                }
                return newJsonData;
              }
              else {
                if (log.IsDebugEnabled) {
                  log.Debug ($"Merge: no data any more => return null");
                }
                return null;
              }
            }
            else {
              return reasonSlot.JsonData;
            }
          }
          else { // No data to merge
            if (log.IsDebugEnabled) {
              log.Debug ($"Merge: no IReasonDataExtension with a Merge => keep the data from ReasonMachineAssociation");
            }
            return possibleReason.JsonData;
          }
        }
      }
      else if (IsJsonNullOrEmpty (reasonSlot.JsonData)) { // Both JsonData are null
        return null;
      }
      else { // reasonMachineAssociation.JsonData is null or empty => remove it?
        var extensionRequest = new GlobalExtensions<IReasonDataExtension> (x => x.Initialize ());
        var extensions = Lemoine.Business.ServiceProvider.Get (extensionRequest).ToList ();
        if (extensions.Any (x => !x.Keep (reasonSlot, possibleReason))) {
          var data = Pulse.Business.Reason.ReasonData.Deserialize (reasonSlot.JsonData, extensions);
          data = data.ToDictionary (x => x.Key, x => x.Value); // Clone it
          bool change = false;
          foreach (var extension in extensions.Where (ext => !ext.Keep (reasonSlot, possibleReason))) {
            var name = extension.Name;
            if (data.ContainsKey (name)) {
              data.Remove (name);
              change = true;
            }
          }
          if (change) {
            if (data.Any ()) {
              var newJsonData = JsonSerializer.Serialize (data);
              if (log.IsDebugEnabled) {
                log.Debug ($"Merge: data is {newJsonData} after purging the data not to keep");
              }
              return newJsonData;
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"Merge: there is no data any more");
              }
              return null;
            }
          }
          else {
            return reasonSlot.JsonData;
          }
        }
        else { // Keep all the data
          return reasonSlot.JsonData;
        }
      }
    }

    /// <summary>
    /// Get the new json data in the case the slot is reset (or switched to processing)
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <param name="newJsonData"></param>
    /// <returns></returns>
    public static bool ResetData (IReasonSlot reasonSlot, out string newJsonData)
    {
      if (!string.IsNullOrEmpty (reasonSlot.JsonData)) {
        var extensionRequest = new GlobalExtensions<IReasonDataExtension> (x => x.Initialize ());
        var extensions = Lemoine.Business.ServiceProvider.Get (extensionRequest).ToList ();
        if (extensions.Any (x => x.DoReset (reasonSlot))) {
          var data = Pulse.Business.Reason.ReasonData.Deserialize (reasonSlot.JsonData, extensions);
          data = data.ToDictionary (x => x.Key, x => x.Value); // Clone it
          foreach (var extension in extensions.Where (ext => ext.DoReset (reasonSlot))) {
            extension.Reset (data);
          }
          if (data.Any ()) {
            newJsonData = JsonSerializer.Serialize (data);
          }
          else {
            newJsonData = null;
          }
          return true;
        }
      }
      newJsonData = null;
      return false;
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

    public static bool IsJsonNullOrEmpty (string json) => string.IsNullOrEmpty (json) || string.Equals (json.Trim (), "{}") || AreJsonEqual (json, "{}");
  }
}
