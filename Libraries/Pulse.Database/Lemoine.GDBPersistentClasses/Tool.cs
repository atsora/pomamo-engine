// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Tool
  /// 
  /// This table lists the tools used in the workshop.
  /// </summary>
  [Serializable]
  public class Tool: DataWithDisplayFunction, ITool
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name;
    string m_code;
    double? m_diameter;
    double? m_radius;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Tool).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public Tool () { }

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Id", "Code", "Name", "Diameter", "Radius"}; }
    }
    
    /// <summary>
    /// Tool ID
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Full tool name / description. This is optional
    /// </summary>
    [XmlAttribute("Name")]
    public virtual string Name
    {
      get { return this.m_name; }
      set { this.m_name = value; }
    }
    
    /// <summary>
    /// Unique number that allows to identify in some companies a tool. This tool code might be found in the ISO file
    /// </summary>
    [XmlAttribute("Code")]
    public virtual string Code {
      get { return m_code; }
      set { m_code = value; }
    }
    
    /// <summary>
    /// Diameter of the tool
    /// </summary>
    [XmlIgnore]
    public virtual double? Diameter {
      get { return m_diameter; }
      set { m_diameter = value; }
    }
    
    /// <summary>
    /// Diameter as string
    /// </summary>
    [XmlAttribute("Diameter")]
    public virtual string DiameterAsString {
      get
      {
        return (this.Diameter.HasValue)
          ? this.Diameter.Value.ToString (CultureInfo.InvariantCulture)
          : null;
      }
      set
      {
        try {
          this.Diameter =
            string.IsNullOrEmpty (value)
            ? default (double?)
            : double.Parse (value, CultureInfo.InvariantCulture);
        }
        catch (Exception ex) {
          log.ErrorFormat ("DiameterAsString.set: " +
                           "the specified value {0} could not be converted to a diameter, " +
                           "{1}",
                          value, ex);
          throw new InvalidCastException ("The diameter could not be determined from a string", ex);
        }
      }
    }
    
    /// <summary>
    /// Radius of the tool
    /// </summary>
    [XmlIgnore]
    public virtual double? Radius {
      get { return m_radius; }
      set { m_radius = value; }
    }

    /// <summary>
    /// Radius as string
    /// </summary>
    [XmlAttribute("Radius")]
    public virtual string RadiusAsString {
      get
      {
        return (this.Radius.HasValue)
          ? this.Radius.Value.ToString (CultureInfo.InvariantCulture)
          : null;
      }
      set
      {
        this.Radius =
          string.IsNullOrEmpty (value)
          ? default (double?)
          : double.Parse (value, CultureInfo.InvariantCulture);
      }
    }
    
    /// <summary>
    /// Size of the tool
    /// 
    /// This concatenates the diameter and the radius in case they exist,
    /// else an empty string is returned
    /// </summary>
    [XmlIgnore]
    public virtual string Size {
      get
      {
        if (m_diameter.HasValue && m_radius.HasValue) {
          return string.Format ("({0:G4} {1:G4})", // Keep only 4 significant digits 
                                m_diameter, m_radius);
        }
        else {
          return "";
        }
      }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      // Do nothing for the moment
    }
  }
}
