// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Lemoine.Core.Log;

using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.Type;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// NHibernate type for XML serializable objects
  /// that are reprenseted in postgresql with a varchar or xml type
  /// </summary>
  [Serializable]
  public class XmlSerializableType : AdvancedMutableType
  {
    static readonly string NULL_VALUE = "null";

    static readonly ILog log = LogManager.GetLogger (typeof (XmlSerializableType).FullName);

    private readonly System.Type m_serializableClass;

    /// <summary>
    /// Default constructor
    /// </summary>
    public XmlSerializableType ()
      : this (typeof (Object))
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="serializableClass"></param>
    public XmlSerializableType (System.Type serializableClass)
      : base (new StringSqlType (), false)
    {
      this.m_serializableClass = serializableClass;
    }

    /// <summary>
    /// Implements MutableType
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="value"></param>
    /// <param name="index"></param>
    /// <param name="session"></param>
    public override void Set (DbCommand cmd, object value, int index, ISessionImplementor session)
    {
      string xml = ToXml (value);
      ((IDataParameter)cmd.Parameters[index]).Value = xml;
    }

    /// <summary>
    /// Implements MutableType
    /// </summary>
    /// <param name="rs"></param>
    /// <param name="name"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    public override object NullSafeGet (DbDataReader rs, string name, ISessionImplementor session)
    {
      return Get (rs, rs.GetOrdinal (name), session);
    }

    /// <summary>
    /// Implements MutableType
    /// </summary>
    /// <param name="rs"></param>
    /// <param name="index"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    public override object Get (DbDataReader rs, int index, ISessionImplementor session)
    {
      if (rs.IsDBNull (index)) {
        return null;
      }

      string xml = Convert.ToString (rs[index]);
      return FromXml (xml);
    }

    /// <summary>
    /// Implements MutableType
    /// </summary>
    public override System.Type ReturnedClass => m_serializableClass;

    /// <summary>
    /// Implements MutableType
    /// </summary>
    public override string Name => m_serializableClass == typeof (IXmlSerializable) ? "serializable" : m_serializableClass.FullName;

    /// <summary>
    /// Implements MutableType
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override object DeepCopyNotNull (object value)
    {
      var xml = ToXml (value);
      return FromXml (xml);
    }

    private string ToXml (object obj)
    {
      if (obj is null) {
        return NULL_VALUE;
      }

      try {
        if (obj is TimeSpan) { // Work around for TimeSpan
          TimeSpan timeSpan = (TimeSpan)obj;
          return $"<TimeSpan>{timeSpan}</TimeSpan>";
        }
        else {
          var xmlSerializer = new XmlSerializer (obj.GetType ());
          using (TextWriter stream = new StringWriter ()) {
            XmlWriterSettings settings = new XmlWriterSettings ();
            settings.OmitXmlDeclaration = true;
            settings.NewLineChars = "\n";
            XmlWriter writer = XmlWriter.Create (stream, settings);
            try {
              xmlSerializer.Serialize (writer, obj);
            }
            catch (Exception serializeException) {
              if (TrySanitizeAndSerialize (serializeException, obj, xmlSerializer, writer)) {
                log.Error ($"ToXml: obj {obj} had to be sanitized before being serialized", serializeException);
              }
              else {
                throw;
              }
            }
            return stream.ToString ();
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"ToXml: could not serialize a serializable property", ex);
        throw new SerializationException ("Could not serialize a serializable property: ", ex);
      }
    }

    bool TrySanitizeAndSerialize (Exception serializeException, object obj, XmlSerializer xmlSerializer, XmlWriter writer)
    {
      try {
        if (null != serializeException.InnerException) {
          var innerExceptionMessage = serializeException.InnerException.Message;
          if (!string.IsNullOrEmpty (innerExceptionMessage) && innerExceptionMessage.Contains ("0x00, is an invalid character")) {
            // Check if it is an invalid string and if it can sanitized
            if (obj is string) {
              string s = (string)obj;
              log.Debug ($"TrySanitizeAndSerialize: try to sanitize string {s}");
              var sanitized = Sanitize (s);
              xmlSerializer.Serialize (writer, sanitized);
              log.WarnFormat ("TrySanitizeAndSerialize: string {0} was successfully sanitized in {1}",
                s, sanitized);
              return true;
            }
          }
        }
      }
      catch (Exception ex) {
        log.Error ("TrySanitizeAndSerialize: 2nd attempt failed", ex);
      }

      return false;
    }

    string Sanitize (string s)
    {
      var nullIndex = s.IndexOf ('\0');
      string result = s;
      if (-1 != nullIndex) {
        result = s.Substring (nullIndex);
      }
      return result.Replace ("&#x0;", "[0x00]");
    }

    private object FromXml (string xml)
    {
      if (string.IsNullOrEmpty (xml)) {
        return null;
      }
      if (xml.Equals (NULL_VALUE, StringComparison.InvariantCultureIgnoreCase)) {
        return null;
      }

      try {
        XmlDocument document = new XmlDocument ();
        document.LoadXml (xml);

        string elementName = document.DocumentElement.Name;
        if (elementName.Equals ("TimeSpan")) { // Work around for TimeSpan
          return TimeSpan.Parse (document.DocumentElement.InnerText);
        }
        else { // Not a TimeSpan
          Type type;
          if (elementName.Equals ("int")) {
            type = typeof (int);
          }
          else if (elementName.Equals ("long")) {
            type = typeof (long);
          }
          else if (elementName.Equals ("JsonElement")) {
            var text = document.DocumentElement.InnerText;
            if (log.IsDebugEnabled) {
              log.Debug ($"FromXml: JsonElement {text}");
            }
            if (long.TryParse (text, out var l)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"FromXml: JsonElement is long");
              }
              return l;
            }
            if (double.TryParse (text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"FromXml: JsonElement is double");
              }
              return d;
            }
            log.Debug ($"FromXml: JsonElement is text");
            return text;
          }
          else {
            type = Type.GetType (elementName,
                                 false, true);
            if (type is null) {
              // this won't work for int / System.Int32 but works for boolean/System.Boolean
              type = Type.GetType ("System." + elementName,
                                   false, true);
              // TODO: this should be moved into Pulse.Database
              if (type is null) {
                type = Type.GetType ($"Lemoine.GDBPersistentClasses.{elementName}, Pulse.Database",
                                     false, true);
              }
              if (type is null) {
                type = Type.GetType ($"Lemoine.Model.{elementName}, Lemoine.ModelDAO",
                                     false, true);
              }
              if (type is null) {
                type = Type.GetType ($"Lemoine.ModelDAO.{elementName}, Lemoine.ModelDAO",
                                     false, true);
              }
            }
          }

          if (null == type) {
            log.Error ($"FromXml: unsupported type {elementName} inner={document.DocumentElement.InnerText} at {System.Environment.StackTrace}");
            return null;
          }

          XmlSerializer xmlSerializer = new XmlSerializer (type);
          StringReader reader = new StringReader (xml);
          return xmlSerializer.Deserialize (reader);
        }
      }
      catch (Exception ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Could not deserialize a serializable property", ex);
        }
        throw new SerializationException ("Could not deserialize a serializable property: ", ex);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public override bool IsEqual (object x, object y)
    {
      if (x == null && y == null) {
        return false; // TODO: for a test
      }

      if (x == null || y == null) {
        return false;
      }

      return object.Equals (ToXml (x), ToXml (y));
    }
  }
}
