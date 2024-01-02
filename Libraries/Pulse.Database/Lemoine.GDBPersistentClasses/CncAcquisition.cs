// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Lemoine.Collections;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table CncAcquisition
  /// </summary>
  [Serializable]
  public class CncAcquisition: BaseData, ICncAcquisition, IVersionable
  {
    static readonly TimeSpan DEFAULT_EVERY = TimeSpan.FromSeconds (2);
    static readonly TimeSpan DEFAULT_NOT_RESPONDING_TIMEOUT = TimeSpan.FromMinutes (2);
    static readonly TimeSpan DEFAULT_SLEEP_BEFORE_RESTART = TimeSpan.FromSeconds (10);
    
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name = null;
    string m_configFile;
    string m_configPrefix = "";
    string m_configParameters;
    bool m_useProcess = false;
    bool m_useCoreService = false;
    TimeSpan m_every = DEFAULT_EVERY;
    TimeSpan m_notRespondingTimeout = DEFAULT_NOT_RESPONDING_TIMEOUT;
    TimeSpan m_sleepBeforeRestart = DEFAULT_SLEEP_BEFORE_RESTART;
    ICollection <IMachineModule> m_machineModules = new InitialNullIdSet<IMachineModule, int> ();
    IComputer m_computer;
    bool m_staThread = false;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (CncAcquisition).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Id", "Name"}; }
    }

    /// <summary>
    /// CncAcquisition Id
    /// </summary>
    [XmlIgnore]
    public virtual int Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// CncAcquisition Id for XML serialization
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int XmlSerializationId
    {
      get { return this.Id; }
      set { m_id = value; }
    }
    
    /// <summary>
    /// CncAcquisition Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// CncAcquisition name
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    [XmlAttribute("Name")]
    public virtual string Name
    {
      get { return this.m_name; }
      set { this.m_name = value; }
    }
    
    /// <summary>
    /// XML configuration file
    /// </summary>
    [XmlAttribute("ConfigFile")]
    public virtual string ConfigFile {
      get { return m_configFile; }
      set { m_configFile = value; }
    }
    
    /// <summary>
    /// Prefix used in the XML configuration file to reference some parameters
    /// 
    /// The default value is the empty string. It can't be null.
    /// </summary>
    [XmlAttribute("ConfigPrefix")]
    public virtual string ConfigPrefix {
      get { return m_configPrefix; }
      set
      {
        if (null == value) {
          log.ErrorFormat ("ConfigPrefix.set: " +
                           "null argument");
          throw new ArgumentNullException ();
        }
        m_configPrefix = value;
      }
    }
    
    /// <summary>
    /// Parameters used in XML configuration file.
    /// 
    /// The first character in this string is the separator to use to separate the different parameters
    /// </summary>
    [XmlAttribute("ConfigParameters")]
    public virtual string ConfigParameters {
      get { return m_configParameters; }
      set { m_configParameters = value; }
    }

    /// <summary>
    /// <see cref="ICncAcquisition"/>
    /// </summary>
    [XmlIgnore]
    public virtual IDictionary<string, string> ConfigKeyParams { get; set; } = new Dictionary<string, string> ();

    /// <summary>
    /// ConfigKeyParams for Xml Serialization
    /// </summary>
    [XmlAttribute("ConfigKeyParams")]
    public virtual string ConfigKeyParamsJson {
      get => System.Text.Json.JsonSerializer.Serialize (this.ConfigKeyParams);
      set {
        this.ConfigKeyParams = System.Text.Json.JsonSerializer.Deserialize<IDictionary<string, string>> (value);
      }
    }

    /// <summary>
    /// Use a process instead of a thread
    /// 
    /// This may useful when the acquisition modules uses some unmanaged code
    /// </summary>
    [XmlAttribute("UseProcess")]
    public virtual bool UseProcess {
      get { return m_useProcess; }
      set { m_useProcess = value; }
    }

    /// <summary>
    /// True if the thread must be run in a single thread apartment
    /// 
    /// Default si false
    /// </summary>
    [XmlAttribute("StaThread")]
    public virtual bool StaThread {
      get { return m_staThread; }
      set { m_staThread = value; }
    }

    /// <summary>
    /// Use the service in .Net Core Lem_CncCoreService
    /// </summary>
    [XmlAttribute ("UseCoreService")]
    public virtual bool UseCoreService
    {
      get { return m_useCoreService; }
      set { m_useCoreService = value; }
    }

    /// <summary>
    /// Frequency when the data is acquired
    /// 
    /// Default is every 2 seconds
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan Every {
      get { return m_every; }
      set { m_every = value; }
    }
    
    /// <summary>
    /// Frequency when the data is acquired for XML serialization
    /// </summary>
    [XmlAttribute("Every")]
    public virtual string XmlSerializationEvery {
      get { return this.Every.ToString (); }
      set { this.Every = TimeSpan.Parse (value); }
    }

    /// <summary>
    /// Time after which the acquisition module is considered as not responding any more
    /// 
    /// Default is 2 minutes.
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan NotRespondingTimeout {
      get { return m_notRespondingTimeout; }
      set { m_notRespondingTimeout = value; }
    }
    
    /// <summary>
    /// Time after which the acquisition module is considered as not responding any more
    /// for XML serialization
    /// </summary>
    [XmlAttribute("NotRespondingTimeout")]
    public virtual string XmlSerializationNotRespondingTimeout {
      get { return this.NotRespondingTimeout.ToString (); }
      set { this.NotRespondingTimeout = TimeSpan.Parse (value); }
    }
    
    /// <summary>
    /// In case the machine module acquisition was stopped (because it was considered as not responding any more),
    /// time after which the acquisition is restarted
    /// 
    /// Default is 10 seconds
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan SleepBeforeRestart {
      get { return m_sleepBeforeRestart; }
      set { m_sleepBeforeRestart = value; }
    }
    
    /// <summary>
    /// In case the machine module acquisition was stopped (because it was considered as not responding any more),
    /// time after which the acquisition is restarted
    /// for XML serialization
    /// 
    /// Default is 10 seconds
    /// </summary>
    [XmlAttribute("SleepBeforeRestart")]
    public virtual string XmlSerializationSleepBeforeRestart {
      get { return this.SleepBeforeRestart.ToString (); }
      set { this.SleepBeforeRestart = TimeSpan.Parse (value); }
    }
    
    /// <summary>
    /// List of machine modules this Cnc Acquisition drives
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IMachineModule> MachineModules {
      get { return m_machineModules; }
    }
    
    /// <summary>
    /// Computer on which the acquisition is made
    /// </summary>
    [XmlIgnore]
    public virtual IComputer Computer {
      get { return m_computer; }
      set { m_computer = value; }
    }
    
    /// <summary>
    /// Computer on which the acquisition is made
    /// for XML serialization
    /// </summary>
    [XmlElement("Computer")]
    public virtual Computer XmlSerializationComputer {
      get { return this.Computer as Computer; }
      set { this.Computer = value; }
    }

    /// <summary>
    /// <see cref="ICncAcquisition"/>
    /// </summary>
    [XmlIgnore]
    public virtual CncModuleLicense License { get; set; }

    /// <summary>
    /// License for XML serialization
    /// </summary>
    [XmlAttribute("License")]
    public virtual int XmlLicense {
      get => (int)this.License;
      set {
        this.License = (CncModuleLicense)value;
      }
    }

    /// <summary>
    /// Text to use in a selection dialog
    /// </summary>
    [XmlIgnore]
    public virtual string SelectionText {
      get { return string.Format ("{0}: {1}",
                                  this.Id, this.Name); }
    }
    #endregion // Getters / Setters
    
    #region Add methods
    /// <summary>
    /// Add a machine module
    /// </summary>
    /// <param name="machineModule"></param>
    public virtual void AddMachineModule (IMachineModule machineModule)
    {
      machineModule.CncAcquisition = this; // Everything is done in the setter
    }
    #endregion // Add methods
    
    #region Methods
    /// <summary>
    /// Add a machine module in the member directly
    /// 
    /// To be used by the MachineModule class only
    /// </summary>
    /// <param name="machineModule"></param>
    protected internal virtual void AddMachineModuleForInternalUse (IMachineModule machineModule)
    {
      AddToProxyCollection<IMachineModule> (m_machineModules, machineModule);
    }
    
    /// <summary>
    /// Remove a machine module in the member directly
    /// 
    /// To be used by the MachineModule class only
    /// </summary>
    /// <param name="machineModule"></param>
    protected internal virtual void RemoveMachineModuleForInternalUse (IMachineModule machineModule)
    {
      RemoveFromProxyCollection<IMachineModule> (m_machineModules, machineModule);
    }
    #endregion // Methods    
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IComputer> (ref m_computer);
    }

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(ICncAcquisition other)
    {
      return this.Equals ((object) other);
    }
    
    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals(this,obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      CncAcquisition other = obj as CncAcquisition;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id);
      }
      return false;
    }

    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode()
    {
      if (0 != Id) {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * Id.GetHashCode();
        }
        return hashCode;
      }
      else {
        return base.GetHashCode ();
      }
    }
  }
}
