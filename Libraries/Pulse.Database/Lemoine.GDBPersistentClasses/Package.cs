// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.Core.Log;
using System.Xml.Serialization;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Package
  /// </summary>
  public class Package: IPackage, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    int m_numVersion = 0;
    string m_name = "";
    string m_identifyingName = "";
    string m_description = "";
    bool m_activated = true;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Package).FullName);

    #region Getters / Setters
    /// <summary>
    /// Package Id
    /// </summary>
    public virtual int Id {
      get { return m_id; }
    }
    
    /// <summary>
    /// Package Version (for NHibernate)
    /// </summary>
    public virtual int Version {
      get { return m_version; }
    }
    
    /// <summary>
    /// Package Version
    /// </summary>
    public virtual int NumVersion {
      get { return m_numVersion; }
      set { m_numVersion = value; }
    }

    /// <summary>
    /// Package name, displayed to the user
    /// </summary>
    public virtual string Name {
      get { return m_name; }
      set { m_name = value; }
    }

    /// <summary>
    /// Package identifying name (unique per package)
    /// </summary>
    public virtual string IdentifyingName {
      get { return m_identifyingName; }
      protected set { m_identifyingName = value; }
    }

    /// <summary>
    /// Package description
    /// </summary>
    public virtual string Description {
      get { return m_description; }
      set { m_description = value; }
    }

    /// <summary>
    /// Activation of the package
    /// </summary>
    public virtual bool Activated {
      get { return m_activated; }
      set { m_activated = value; }
    }

    /// <summary>
    /// Sequence detail (nullable)
    /// </summary>
    public virtual PackageDetail Detail { get; set; } = null;
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected internal Package() {}
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="identifyingName"></param>
    public Package(string identifyingName)
    {
      IdentifyingName = identifyingName;
    }
    #endregion // Constructors
  }
}
