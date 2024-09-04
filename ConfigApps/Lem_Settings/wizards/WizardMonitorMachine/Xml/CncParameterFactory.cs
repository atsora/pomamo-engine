// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml;
using Lemoine.Core.Log;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of CncParameterFactory.
  /// </summary>
  public class CncParameterFactory
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncParameterFactory).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CncParameterFactory() {}
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Create a ICncParameter based on an .xml description
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    static public AbstractParameter GetCncParameter(XmlNode node)
    {
      // Check if the type and the name is specified
      XmlAttributeCollection attributes = node.Attributes;
      XmlAttribute typeAttribute = attributes["type"];
      XmlAttribute nameAttribute = attributes["name"];
      if (typeAttribute == null || nameAttribute == null ||
          typeAttribute.Value == "" || nameAttribute.Value == "") {
        log.Error("the type and / or the name of the cnc parameter is unknown");
        return null;
      }
      
      // Create of CncParameter of the right type
      AbstractParameter iCncParameter = null;
      switch (typeAttribute.Value.ToLower()) {
        case "ip":
          iCncParameter = new CncParameterIP(node);
          break;
        case "path":
          iCncParameter = new CncParameterPath(node);
          break;
        case "url":
          iCncParameter = new CncParameterUrl(node);
          break;
        case "host":
          iCncParameter = new CncParameterHost(node);
          break;
        case "string":
          iCncParameter = new CncParameterString(node);
          break;
        case "int": case "integer":
          iCncParameter = new CncParameterInt(node);
          break;
        case "double":
          iCncParameter = new CncParameterDouble(node);
          break;
        case "list":
          iCncParameter = new CncParameterList(node);
          break;
        case "null":
          iCncParameter = new CncParameterNull(node);
          break;
        case "bool": case "boolean":
          iCncParameter = new CncParameterBool(node);
          break;
        case "file":
          iCncParameter = new CncParameterFile(node);
          break;
        default:
          log.Error("the type of the cnc parameter \"" + nameAttribute.Value + "\" is unknown");
          return null;
      }
      
      // Check validity
      if (!iCncParameter.IsValid) {
        log.Error("couldn't create the cnc parameter \"" + nameAttribute.Value + "\"");
        return null;
      }
      
      return iCncParameter;
    }
    #endregion // Methods
  }
}
