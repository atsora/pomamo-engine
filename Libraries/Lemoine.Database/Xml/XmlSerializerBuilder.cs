// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Lemoine.Extensions;
using Lemoine.Extensions.Database;
using Lemoine.Core.Log;

namespace Lemoine.Database.Xml
{
  /// <summary>
  /// Class to create a XmlSerializer that includes all the extra types
  /// </summary>
  public class XmlSerializerBuilder: XmlSerializer
  {
    #region Members
    IEnumerable<Type> m_extraTypes = new List<Type> ();
    #endregion // Members

    // disable once StaticFieldInGenericType
    static readonly ILog log = LogManager.GetLogger(typeof (XmlSerializerBuilder).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public XmlSerializerBuilder ()
    {
      IEnumerable<IXmlSerializationExtension> extensions = Lemoine.Business.ServiceProvider
        .Get<IEnumerable<IXmlSerializationExtension>> (new Lemoine.Business.Extension.GlobalExtensions<IXmlSerializationExtension> ());
      foreach (var extension in extensions) {
        m_extraTypes = m_extraTypes.Union (extension.GetExtraTypes ());
      }
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get the built XmlSerializer
    /// </summary>
    /// <returns></returns>
    public XmlSerializer GetSerializer<T> ()
    {
      return GetSerializer (typeof(T));
    }
    
    /// <summary>
    /// Get the built XmlSerializer
    /// </summary>
    /// <returns></returns>
    public XmlSerializer GetSerializer (Type type)
    {
      var extraTypesArray = m_extraTypes.ToArray ();
      return new XmlSerializer (type, extraTypesArray);
    }
    #endregion // Methods
  }
}
