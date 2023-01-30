// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lemoine.Core.Log;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Lemoine.Collections
{
  /// <summary>
  /// A ListString is string that describes a IEnumerable of strings.
  /// The first character is the separator to separate the different elements of the list
  /// </summary>
  public static class EnumerableString
  {
    static readonly char[] SEPARATORS = { ';', ',', '/', '#', '%', '-', '_', '$', '|', '@' };
    static readonly char[] DICTIONARY_ITEM_SEPARATORS = { ';', ',', '#', '_', '$', '|', '@' };
    static readonly char[] KEY_VALUE_SEPARATORS = { ':', '=', '/', '-' };
    static readonly Regex WITH_TYPE_REGEX = new Regex ("^<>(?<NoTyped1>.*)$|^(?<NoTyped2>[^<].*)$|^<(?<ListType>[^,]+)>(?<ListString>.*)$|^<(?<DictionaryKeyType>[^,]+),(?<DictionaryValueType>[^,]+)>(?<DictionaryString>.*)$", RegexOptions.Compiled);

    static ILog log = LogManager.GetLogger (typeof (EnumerableString).FullName);

    /// <summary>
    /// Convert an IEnumerable collection to a string, where the first character is the separator character between the elements
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <returns></returns>
    public static string ToListString<T> (this IEnumerable<T> t)
    {
      return t.ToListString<T> (false);
    }

    /// <summary>
    /// Convert an IEnumerable collection to a string, where the first character is the separator character between the elements
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <param name="withType"></param>
    /// <returns></returns>
    public static string ToListString<T> (this IEnumerable<T> t, bool withType)
    {
      if (!t.Any ()) {
        log.DebugFormat ("ToListString: empty IEnumerable => return an empty string");
        return "";
      }

      string prefix = "";
      if (withType) {
        prefix = string.Format ("<{0}>", GetTypeString (typeof (T)));
      }

      foreach (var separator in SEPARATORS) {
        if (!t.Any (i => i.ToString ().Contains (separator))) { // Separator is ok
          log.DebugFormat ("ToListString: separator {0} is used", separator);
          return prefix + separator.ToString () + string.Join (separator.ToString (), t.Select (i => i.ToString ()).ToArray ());
        }
      }

      log.ErrorFormat ("ToListString: none of the separator is ok, they are all used => throw an exception");
      throw new Exception ("ToListString: no valid separator");
    }

    /// <summary>
    /// Convert an IEnumerable collection to a string, where the first character is the separator character between the elements
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static string ConvertNotGenericToListString (System.Collections.IEnumerable t)
    {
      var genericList = new List<string> ();
      foreach (var i in t) {
        if (null != i) {
          genericList.Add (i.ToString ());
        }
      }
      return genericList.ToListString<string> ();
    }

    /// <summary>
    /// Parse a ListString, where the first character is the separator character between the elements
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string[] ParseListString (string s)
    {
      if (string.IsNullOrEmpty (s)) {
        return new string[] { };
      }
      else {
        var separator = s[0];
        var subString = s.Substring (1);
        if (string.IsNullOrEmpty (subString)) {
          return new string[] { };
        }
        else {
          return subString.Split (new char[] { separator });
        }
      }
    }

    /// <summary>
    /// Parse a ListString, where the first character is the separator character between the elements
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static IEnumerable<T> ParseListString<T> (string s)
    {
      return ParseListString (s)
        .Select (i => ConvertString<T> (i));
    }

    /// <summary>
    /// Parse a ListString, where the first character is the separator character between the elements
    /// </summary>
    /// <param name="s"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static System.Collections.IEnumerable ParseListString (string s, Type type)
    {
      return ParseListString (s)
        .Select (i => ConvertString (i, type));
    }

    /// <summary>
    /// Convert an IDictionary collection to a string, where:
    /// <item>the first character is the separator between a key and a value</item>
    /// <item>the second character is the separator character between the elements</item>
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="t"></param>
    /// <returns></returns>
    public static string ToDictionaryString<TKey, TValue> (this IDictionary<TKey, TValue> t)
    {
      return t.ToDictionaryString (false);
    }

    /// <summary>
    /// Convert an IDictionary collection to a string, where:
    /// <item>the first character is the separator between a key and a value</item>
    /// <item>the second character is the separator character between the elements</item>
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="t"></param>
    /// <param name="withType">output the type too at the start of the string</param>
    /// <returns></returns>
    public static string ToDictionaryString<TKey, TValue> (this IDictionary<TKey, TValue> t, bool withType)
    {
      if (!t.Any ()) {
        log.DebugFormat ("ToDictionaryString: empty IEnumerable => return an empty string");
        return "";
      }

      string prefix = "";
      if (withType) {
        prefix = string.Format ("<{0},{1}>", GetTypeString (typeof (TKey)), GetTypeString (typeof (TValue)));
      }

      foreach (var itemSeparator in DICTIONARY_ITEM_SEPARATORS) {
        if (!t.Keys.Any (i => i.ToString ().Contains (itemSeparator))
          && !t.Values.Any (i => i.ToString ().Contains (itemSeparator))) { // Separator is ok
          log.DebugFormat ("ToDictionaryString: item separator {0} is used", itemSeparator);
          foreach (var keyValueSeparator in KEY_VALUE_SEPARATORS) {
            if (!t.Keys.Any (i => i.ToString ().Contains (keyValueSeparator))
              && !t.Values.Any (i => i.ToString ().Contains (keyValueSeparator))) { // Separator is ok
              log.DebugFormat ("ToDictionaryString: key value separator {0} is used", keyValueSeparator);
              return prefix + keyValueSeparator.ToString () + itemSeparator.ToString ()
                + string.Join (itemSeparator.ToString (), t.Select (i => i.Key.ToString () + keyValueSeparator + i.Value.ToString ()).ToArray ());
            }
          }
        }
      }

      log.ErrorFormat ("ToDictionaryString: none of the separator is ok, they are all used => throw an exception");
      throw new Exception ("ToDictionaryString: no valid separator");
    }

    /// <summary>
    /// Parse a DictionaryString, where:
    /// <item>the first character is the separator between a key and a value</item>
    /// <item>the second character is the separator character between the elements</item>
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static IDictionary<string, string> ParseDictionaryString (string s)
    {
      if (string.IsNullOrEmpty (s) || (2 == s.Length)) {
        return new Dictionary<string, string> ();
      }
      else if (1 == s.Length) {
        log.ErrorFormat ("ParseDictionaryString: invalid string {0}, there is only a single separator", s);
        throw new ArgumentException ("missing separator", "s");
      }
      else {
        Debug.Assert (2 <= s.Length);
        var keyValueSeparator = s[0];
        var itemSeparator = s[1];
        return s.Substring (2).Split (new char[] { itemSeparator })
          .Select (i => i.Split (new char[] { keyValueSeparator }, 2))
          .ToDictionary (i => i[0], i => i[1]);
      }
    }

    /// <summary>
    /// Parse a DictionaryString
    /// </summary>
    /// <param name="s"></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static IDictionary<TKey, TValue> ParseDictionaryString<TKey, TValue> (string s)
    {
      return ParseDictionaryString<TKey, TValue> (s, ConvertString<TKey>, ConvertString<TValue>);
    }

    /// <summary>
    /// Parse a DictionaryString
    /// </summary>
    /// <param name="s"></param>
    /// <param name="convertKey"></param>
    /// <param name="convertValue"></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static IDictionary<TKey, TValue> ParseDictionaryString<TKey, TValue> (string s, Func<string, TKey> convertKey, Func<string, TValue> convertValue)
    {
      return ParseDictionaryString (s)
        .ToDictionary (i => convertKey (i.Key),
         i => convertValue (i.Value));
    }

    /// <summary>
    /// Parse a DictionaryString
    /// </summary>
    /// <param name="s"></param>
    /// <param name="keyType"></param>
    /// <param name="valueType"></param>
    /// <returns></returns>
    public static System.Collections.IDictionary ParseDictionaryString (string s, Type keyType, Type valueType)
    {
      return ParseDictionaryString (s)
        .ToDictionary (i => ConvertString (i.Key, keyType),
         i => ConvertString (i.Value, valueType));
    }

    static T ConvertString<T> (string s)
    {
      return (T)Convert.ChangeType (s, typeof (T));
    }

    static object ConvertString (string s, Type type)
    {
      return Convert.ChangeType (s, type);
    }

    static string GetTypeString (Type type)
    {
      var s = type.ToString ();
      if (s.StartsWith ("System.", StringComparison.InvariantCulture)) {
        s = s.Substring ("System.".Length);
      }
      return s;
    }

    static Type ParseType (string s)
    {
      var typeString = s;
      if (!s.Contains ('.')) {
        typeString = "System." + s;
      }
      return Type.GetType (typeString);
    }

    /// <summary>
    /// Parse a ListString, where the first character is the separator character between the elements
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static object ParseAuto (string s)
    {
      var match = WITH_TYPE_REGEX.Match (s);
      if (!match.Success) {
        log.ErrorFormat ("ParseListString: no match with regex, invalid string {0}", s);
        throw new ArgumentException ("Invalid ListString");
      }
      else if (match.Groups["ListType"].Success) {
        Debug.Assert (match.Groups["ListString"].Success);
        var listType = ParseType (match.Groups["ListType"].Value);
        var listString = match.Groups["ListString"].Value;
        return ParseListString (listString, listType);
      }
      else if (match.Groups["DictionaryKeyType"].Success) {
        Debug.Assert (match.Groups["DictionaryValueType"].Success);
        Debug.Assert (match.Groups["DictionaryString"].Success);
        var keyType = ParseType (match.Groups["DictionaryKeyType"].Value);
        var valueType = ParseType (match.Groups["DictionaryValueType"].Value);
        var dictionaryString = match.Groups["DictionaryString"].Value;
        return ParseDictionaryString (dictionaryString, keyType, valueType);
      }
      else if (match.Groups["NoTyped1"].Success) {
        log.InfoFormat ("ParseAuto: no type with <> in {0}", s);
        return ParseListString (match.Groups["NoTyped1"].Value);
      }
      else {
        log.InfoFormat ("ParseAuto: no type in {0}", s);
        return ParseListString (s);
      }
    }
  }
}
