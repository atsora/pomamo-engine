// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.Plugin.MachineStateTemplateChangeEvent
{
  /// <summary>
  /// Description of XmlSerializationExtension.
  /// </summary>
  public class XmlSerializationExtension: Lemoine.Extensions.Database.IXmlSerializationExtension
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (XmlSerializationExtension).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public XmlSerializationExtension ()
    {
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IXmlSerializationExtension implementation

    public Type[] GetExtraTypes()
    {
      var extraTypes = new Type[] { typeof (EventMachineStateTemplateChange) };
      return extraTypes;
    }

    #endregion

    #region IExtension implementation

    public bool UniqueInstance {
      get {
        return true;
      }
    }

    #endregion
  }
}
