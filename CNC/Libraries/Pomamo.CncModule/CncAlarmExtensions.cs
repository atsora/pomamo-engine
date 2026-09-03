// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Pomamo.CncModule
{
  /// <summary>
  /// Behaviour that is common to every implementation of <see cref="ICncAlarm"/>
  ///
  /// Equals, GetHashCode and ToString can not be extension methods, since they would not be
  /// reached by the virtual dispatch of object. Each implementation keeps them, but it delegates
  /// to the methods here, so the behaviour is defined once and two different implementations
  /// remain interchangeable.
  /// </summary>
  public static class CncAlarmExtensions
  {
    /// <summary>
    /// Clean the message of an alarm: the control characters are removed, since they are welcome
    /// neither in the data queue nor in the database
    ///
    /// It is used by the Message setter of the implementations, which can not be an extension method
    /// </summary>
    /// <param name="message">nullable</param>
    /// <returns>not null</returns>
    public static string CleanMessage (string message)
    {
      if (string.IsNullOrEmpty (message)) {
        return "";
      }
      return Regex.Replace (message, @"[\x00-\x1f]+", "");
    }

    /// <summary>
    /// Do two alarms identify the same alarm, with the same message and the same properties ?
    ///
    /// The moment they were reported is not considered
    /// </summary>
    /// <param name="alarm">not null</param>
    /// <param name="other">nullable</param>
    public static bool HasSameValues (this ICncAlarm alarm, ICncAlarm other)
    {
      if (alarm == null) {
        throw new ArgumentNullException ("alarm");
      }
      if (other == null) {
        return false;
      }
      if (ReferenceEquals (alarm, other)) {
        return true;
      }

      return string.Equals (alarm.CncInfo, other.CncInfo)
        && string.Equals (alarm.CncSubInfo, other.CncSubInfo)
        && string.Equals (alarm.Type, other.Type)
        && string.Equals (alarm.Number, other.Number)
        && string.Equals (alarm.Message, other.Message)
        && HasSameProperties (alarm.Properties, other.Properties);
    }

    static bool HasSameProperties (IDictionary<string, string> properties, IDictionary<string, string> otherProperties)
    {
      var count = (properties == null) ? 0 : properties.Count;
      var otherCount = (otherProperties == null) ? 0 : otherProperties.Count;
      if (count != otherCount) {
        return false;
      }
      if (0 == count) {
        return true;
      }

      foreach (var property in properties) {
        string otherValue;
        if (!otherProperties.TryGetValue (property.Key, out otherValue)
          || !string.Equals (property.Value, otherValue)) {
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Hash code that matches <see cref="HasSameValues" />: two alarms that carry the same values
    /// return the same hash code, whatever their implementation
    ///
    /// The properties only contribute by their number, so that the hash code stays cheap
    /// </summary>
    /// <param name="alarm">not null</param>
    public static int GetValueHashCode (this ICncAlarm alarm)
    {
      if (alarm == null) {
        throw new ArgumentNullException ("alarm");
      }

      unchecked {
        var hashCode = 17;
        hashCode = hashCode * 23 + ((alarm.CncInfo == null) ? 0 : alarm.CncInfo.GetHashCode ());
        hashCode = hashCode * 23 + ((alarm.CncSubInfo == null) ? 0 : alarm.CncSubInfo.GetHashCode ());
        hashCode = hashCode * 23 + ((alarm.Type == null) ? 0 : alarm.Type.GetHashCode ());
        hashCode = hashCode * 23 + ((alarm.Number == null) ? 0 : alarm.Number.GetHashCode ());
        hashCode = hashCode * 23 + ((alarm.Message == null) ? 0 : alarm.Message.GetHashCode ());
        hashCode = hashCode * 23 + ((alarm.Properties == null) ? 0 : alarm.Properties.Count);
        return hashCode;
      }
    }

    /// <summary>
    /// Textual description of an alarm
    ///
    /// <see cref="ParseDescription" /> reads it back, and the automatic converter uses it to build
    /// an alarm from a string, so the format must not change
    /// </summary>
    /// <param name="alarm">not null</param>
    public static string ToDescription (this ICncAlarm alarm)
    {
      if (alarm is null) {
        throw new ArgumentNullException ("alarm");
      }

      return $"CncInfo={alarm.CncInfo}; Type={alarm.Type}; Number={alarm.Number}; Message={alarm.Message}";
    }

    /// <summary>
    /// Fill an alarm from the description <see cref="ToDescription" /> returns
    ///
    /// The unknown keys are ignored
    /// </summary>
    /// <param name="alarm">not null</param>
    /// <param name="description">nullable</param>
    public static void ParseDescription (this ICncAlarm alarm, string description)
    {
      if (alarm is null) {
        throw new ArgumentNullException ("alarm");
      }
      if (string.IsNullOrEmpty (description)) {
        return;
      }

      foreach (var item in description.Split (';')) {
        var keyValue = item.Trim ().Split (new char[] { '=' }, 2);
        if (2 != keyValue.Length) {
          continue;
        }
        switch (keyValue[0]) {
        case "CncInfo":
          alarm.CncInfo = keyValue[1];
          break;
        case "CncSubInfo":
          alarm.CncSubInfo = keyValue[1];
          break;
        case "Type":
          alarm.Type = keyValue[1];
          break;
        case "Number":
          alarm.Number = keyValue[1];
          break;
        case "Message":
          alarm.Message = keyValue[1];
          break;
        default:
          // Ignore
          break;
        }
      }
    }

    /// <summary>
    /// Copy every value of an alarm into another one, whatever their implementations
    /// </summary>
    /// <param name="alarm">not null</param>
    /// <param name="target">not null</param>
    public static void CopyTo (this ICncAlarm alarm, ICncAlarm target)
    {
      if (alarm == null) {
        throw new ArgumentNullException ("alarm");
      }
      if (target == null) {
        throw new ArgumentNullException ("target");
      }

      target.CncInfo = alarm.CncInfo;
      target.CncSubInfo = alarm.CncSubInfo;
      target.Type = alarm.Type;
      target.Number = alarm.Number;
      target.Message = alarm.Message;
      if (target.Properties == null) {
        target.Properties = new Dictionary<string, string> ();
      }
      else {
        target.Properties.Clear ();
      }
      if (alarm.Properties != null) {
        foreach (var property in alarm.Properties) {
          target.Properties[property.Key] = property.Value;
        }
      }
    }
  }
}
