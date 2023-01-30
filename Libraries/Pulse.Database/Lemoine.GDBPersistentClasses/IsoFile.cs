// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table IsoFile
  /// </summary>
  [Serializable]
  public class IsoFile: DataWithDisplayFunction, IIsoFile
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name;
    IComputer m_computer;
    string m_sourceDirectory;
    string m_stampingDirectory;
    int? m_size;
    DateTime m_stampingDateTime;
    // TODO: add m_postProcessor and m_camSystem
    ICollection <IStamp> m_stamps = new List<IStamp> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (IsoFile).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Id", "Name", "SourceDirectory", "StampingDirectory"}; }
    }

    /// <summary>
    /// IsoFile ID
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
    /// File name of the program without the path, with the extension
    /// </summary>
    [XmlAttribute("Name")]
    public virtual string Name
    {
      get { return this.m_name; }
      set { this.m_name = value; }
    }
    
    /// <summary>
    /// Reference to the Computer where the stamping took place
    /// </summary>
    [XmlIgnore]
    public virtual IComputer Computer {
      get { return m_computer; }
      set { m_computer = value; }
    }
    
    /// <summary>
    /// Directory of the source ISO file (before stamping)
    /// </summary>
    [XmlAttribute("SourceDirectory")]
    public virtual string SourceDirectory {
      get { return m_sourceDirectory; }
      set { m_sourceDirectory = value; }
    }
    
    /// <summary>
    /// Directory where the ISO file was stamped
    /// </summary>
    [XmlAttribute("StampingDirectory")]
    public virtual string StampingDirectory {
      get { return m_stampingDirectory; }
      set { m_stampingDirectory = value; }
    }
    
    /// <summary>
    /// Size of the ISO file, in block number,
    /// line number or bytes according to the post-processor setting
    /// </summary>
    [XmlIgnore]
    public virtual int? Size {
      get { return m_size; }
      set { m_size = value; }
    }
    
    /// <summary>
    /// UTC date/time of the stamping
    /// </summary>
    [XmlIgnore]
    public virtual DateTime StampingDateTime {
      get { return m_stampingDateTime; }
      set { m_stampingDateTime = value; }
    }
    
    /// <summary>
    /// List of stamps that are associated to this ISO file
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IStamp> Stamps {
      get
      {
        if (null == m_stamps) {
          m_stamps = new List<IStamp> ();
        }
        return m_stamps;
      }
    }    
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    protected IsoFile ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    internal protected IsoFile (string name)
    {
      this.Name = name;
    }
    #endregion // Constructors
  }
}
