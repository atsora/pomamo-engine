// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Service
  /// </summary>
  [Serializable]
  public class Service: Updater, IService
  {
    #region Members
    string m_name;
    string m_program;
    bool m_lemoine = true;
    IComputer m_computer;
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (Service).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Id", "Name", "Program", "Computer"}; }
    }
    
    /// <summary>
    /// Service name
    /// </summary>
    [XmlAttribute("Name")]
    public virtual string Name
    {
      get { return this.m_name; }
      set { this.m_name = value; }
    }

    /// <summary>
    /// Service program name (case insensitive)
    /// </summary>
    [XmlAttribute("Program")]
    public virtual string Program {
      get { return m_program; }
      set { m_program = value; }
    }
    
    /// <summary>
    /// Is the service a Lemoine service ?
    /// </summary>
    [XmlAttribute("Lemoine")]
    public virtual bool Lemoine {
      get { return m_lemoine; }
      set { m_lemoine = value; }
    }
    
    /// <summary>
    /// Computer on which the service is installed
    /// 
    /// Not null
    /// </summary>
    [XmlIgnore]
    public virtual IComputer Computer {
      get { return m_computer; }
      set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.ErrorFormat ("Computer.set: " +
                           "null value");
          throw new ArgumentNullException ("Service.Computer");
        }
        m_computer = value;
      }
    }
    
    /// <summary>
    /// Computer on which the service is installed for Xml Serialization
    /// </summary>
    [XmlElement("Computer")]
    public virtual Computer XmlSerializationComputer {
      get { return this.Computer as Computer; }
      set { this.Computer = value; }
    }
    #endregion // Getters / Setters
        
    /// <summary>
    /// Unproxy the entity for XML serialization
    /// </summary>
    public override void Unproxy ()
    {
      NHibernateHelper.Unproxy<IComputer> (ref m_computer);
    }
  }
}
