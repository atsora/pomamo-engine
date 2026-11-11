// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Base class for some persistent classes
  /// 
  /// This allows a set of persistent classes to have a same base set of methods
  /// </summary>
  [Serializable]
  public abstract class DataWithPatternName: BaseData, IDisplayable
  {
    static readonly Regex keywordRegex = new Regex ("<%([^%]+)%>");
    
    static readonly ILog log = LogManager.GetLogger(typeof (DataWithPatternName).FullName);

    /// <summary>
    /// Display name that is deduced from the display table
    /// </summary>
    [XmlIgnore]
    public virtual string Display
    {
      get
      {
        return GetDisplay (null);
      }
      set { }
    }
    
    #region IDisplay implementation
    /// <summary>
    /// IDisplay implementation
    /// </summary>
    /// <param name="variant"></param>
    /// <returns></returns>
    public virtual string GetDisplay (string variant)
    {
      Type t = this.GetType();
      string pattern = GetPattern (t, variant);
      if (string.IsNullOrEmpty (pattern)) {
        log.Warn ($"Display.get: no pattern is defined for {this.GetType ().Name}");
        return "";
      }
      else {
        return GetDisplayFromPattern (pattern);
      }
    }
    
    string GetDisplayFromPattern (string pattern)
    {
      string result = pattern;
      foreach (Match match in keywordRegex.Matches (pattern)) {
        try {
          Debug.Assert (match.Groups [1].Success);
          result = result.Replace (match.Value, GetKeywordValue (this, match.Groups [1].Value));
        }
        catch (Exception ex) {
          log.Error ($"GetDisplayFromPattern: got error {ex} in match {match}");
        }
      }
      return result;
    }
    
    string GetKeywordValue (object o, string keyword)
    {
      if (keyword.Contains (".")) {
        string [] keywordSplit = keyword.Split (new char [] {'.'}, 2);
        Debug.Assert (2 == keywordSplit.Length);
        string subObject = keywordSplit [0];
        Type type = o.GetType ();
        var property = type.GetProperty (subObject);
        object subObjectValue =
          property.GetValue (o, null);
        if (null == subObjectValue) {
          log.Info ($"GetDisplayFromPattern: sub-object {subObject} is not valid for {o} return an empty string");
          return "";
        }
        else { // null != subObjectValue
          return GetKeywordValue (subObjectValue, keywordSplit [1]);
        }
      }
      else { // No '.'
        object propertyValue =
          o.GetType ().GetProperty (keyword).GetValue (o, null);
        if (null == propertyValue) {
          log.Info ($"Display.get: null value for property {keyword}, replace it by an emptpy string");
          return "";
        }
        else { // null != propertyValue
          string propertyString;
          IDisplayable displayablePropertyValue =
            propertyValue as IDisplayable;
          if (null != displayablePropertyValue) {
            propertyString =
              displayablePropertyValue.Display.Trim ();
          }
          else {
            propertyString =
              propertyValue.ToString ().Trim ();
          }
          if (log.IsDebugEnabled) {
            log.Debug ($"Display.get: replace property {keyword} by {propertyString}");
          }
          return propertyString;
        }
      }
    }

    string GetPattern (Type type, string variant)
    {
      string pattern;
      
      // - Try from the ConfigSet with the variant
      pattern = GetPatternFromConfigSet (type, variant);
      if (!string.IsNullOrEmpty (pattern)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetPattern: consider pattern {pattern} from config set for type {type} and variant {variant}");
        }
        return pattern;
      }
      
      // - Try to get it from the database with the variant
      pattern = GetPatternFromDatabase (type, variant);
      if (!string.IsNullOrEmpty (pattern)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetPattern: consider pattern {pattern} from database for type {type} and variant {variant}");
        }
        return pattern;
      }
      
      // - Try from the ConfigSet without the variant
      pattern = GetPatternFromConfigSet (type, null);
      if (!string.IsNullOrEmpty (pattern)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetPattern: consider pattern {pattern} from config set for type {type}");
        }
        return pattern;
      }
      
      // - Try to get it from the database without the variant
      pattern = GetPatternFromDatabase (type, null);
      if (!string.IsNullOrEmpty (pattern)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetPattern: consider pattern {pattern} from database for type {type}");
        }
        return pattern;
      }
      
      log.Warn ($"GetPattern: no pattern was found for {type}");
      return "";
    }

    string GetPatternFromDatabase (Type type, string variant)
    {
      string pattern;

      // - Try with type.Name
      if (DisplayHelper.TryGetPattern (type.Name, variant, out pattern)) {
        return pattern;
      }
      
      // - Try with type.BaseType.Name
      if (DisplayHelper.TryGetPattern (type.BaseType.Name, variant, out pattern)) {
        return pattern;
      }
      
      return "";
    }

    string GetPatternFromConfigSet (Type type, string variant)
    {
      string pattern;
      
      // - Try with type.Name
      pattern = GetPatternFromConfigSet (type.Name, variant);
      if (!string.IsNullOrEmpty (pattern)) {
        return pattern;
      }
      // - Try with type.BaseType.Name
      pattern = GetPatternFromConfigSet (type.BaseType.Name, variant);
      return pattern;
    }

    string GetPatternFromConfigSet (string name, string variant)
    {
      string key = "Display." + name;
      if (!string.IsNullOrEmpty (variant)) {
        key += "." + variant;
      }
      string pattern = Lemoine.Info.ConfigSet.LoadAndGet<string> (key, "");
      return pattern;
    }
    #endregion // IDisplay implementation
  }
}
