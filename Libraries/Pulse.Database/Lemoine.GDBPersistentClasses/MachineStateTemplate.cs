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
  /// Persistent class of table MachineStateTemplate
  /// </summary>
  [Serializable]
  public class MachineStateTemplate : DataWithTranslation, IMachineStateTemplate, IVersionable
  {
    int m_id = 0;
    int m_version = 0;
    MachineStateTemplateCategory? m_category = null;
    bool m_userRequired;
    bool m_shiftRequired;
    bool? m_onSite;
    IMachineStateTemplate m_siteAttendanceChange;
    LinkDirection m_linkOperationDirection = LinkDirection.None;
    IList<IMachineStateTemplateItem> m_items = new List<IMachineStateTemplateItem> ();
    ISet<IMachineStateTemplateStop> m_stops = new HashSet<IMachineStateTemplateStop> ();

    static readonly ILog log = LogManager.GetLogger (typeof (MachineStateTemplate).FullName);

    /// <summary>
    /// Default constructor
    /// </summary>
    internal protected MachineStateTemplate ()
    { }

    /// <summary>
    /// Constructor for default values
    /// </summary>
    /// <param name="name">not null and not empty</param>
    internal protected MachineStateTemplate (string name)
    {
      this.Name = name;
    }

    /// <summary>
    /// Constructor for default values
    /// </summary>
    /// <param name="id"></param>
    /// <param name="translationKey"></param>
    /// <param name="userRequired"></param>
    internal protected MachineStateTemplate (int id, string translationKey, bool userRequired)
    {
      m_id = id;
      this.TranslationKey = translationKey;
      m_userRequired = userRequired;
    }

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] { "Id", "Name", "TranslationKey" }; }
    }

    /// <summary>
    /// MachineStateTemplate ID
    /// </summary>
    [XmlAttribute ("Id")]
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
    /// Category
    /// </summary>
    [XmlIgnore]
    public virtual MachineStateTemplateCategory? Category
    {
      get { return m_category; }
      set { m_category = value; }
    }

    /// <summary>
    /// Is a user required for this observation state ?
    /// </summary>
    [XmlAttribute ("UserRequired")]
    public virtual bool UserRequired
    {
      get { return m_userRequired; }
      set { m_userRequired = value; }
    }

    /// <summary>
    /// Is a shift required for this observation state ?
    /// </summary>
    [XmlAttribute ("ShiftRequired")]
    public virtual bool ShiftRequired
    {
      get { return m_shiftRequired; }
      set { m_shiftRequired = value; }
    }

    /// <summary>
    /// Does this Machine Observation State mean the associated user
    /// is on site ?
    /// 
    /// null in case of not applicable
    /// </summary>
    [XmlIgnore]
    public virtual bool? OnSite
    {
      get { return m_onSite; }
      set { m_onSite = value; }
    }

    /// <summary>
    /// use for Xml serialization of Onsite
    /// </summary>
    [XmlAttribute ("OnSite")]
    public virtual bool XmlSerializationOnSite
    {
      get { return m_onSite.Value; }
      set { m_onSite = value; }
    }

    /// <summary>
    /// used to serialize OnSite only when not null
    /// </summary>
    public virtual bool XmlSerializationOnSiteSpecified { get { return m_onSite.HasValue; } }

    /// <summary>
    /// In which new MachineStateTemplate should this MachineStateTemplate
    /// be changed in case the site attendance change of the associated user
    /// changes ?
    /// </summary>
    [XmlIgnore]
    public virtual IMachineStateTemplate SiteAttendanceChange
    {
      get { return m_siteAttendanceChange; }
      set { m_siteAttendanceChange = value; }
    }

    /// <summary>
    /// Does this machine state template imply an operation should be automatically set before or after it
    /// </summary>
    [XmlIgnore]
    public virtual LinkDirection LinkOperationDirection
    {
      get { return m_linkOperationDirection; }
      set { m_linkOperationDirection = value; }
    }

    /// <summary>
    /// Optional color
    /// 
    /// <see cref="IMachineStateTemplate"/>
    /// </summary>
    [XmlAttribute ("Color")]
    public virtual string Color
    {
      get; set;
    }

    /// <summary>
    /// used to serialize Color only when not null or empty
    /// </summary>
    public virtual bool ColorSpecified => !string.IsNullOrEmpty (this.Color);

    /// <summary>
    /// Text to use in a selection dialog
    /// </summary>
    [XmlIgnore]
    public virtual string SelectionText
    {
      get {
        return string.Format ("{0}: {1}{2}",
                                  this.Id, this.Name, this.TranslationKey);
      }
    }

    /// <summary>
    /// List of items that are part of the machine state template
    /// </summary>
    [XmlIgnore] // For the moment
    public virtual IList<IMachineStateTemplateItem> Items
    {
      get { return m_items; }
    }

    /// <summary>
    /// Set of stop conditions
    /// </summary>
    [XmlIgnore] // For the moment
    public virtual ISet<IMachineStateTemplateStop> Stops
    {
      get { return m_stops; }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// Append an item with the specified machine observation state
    /// </summary>
    /// <param name="machineObservationState"></param>
    /// <returns></returns>
    public virtual IMachineStateTemplateItem AddItem (IMachineObservationState machineObservationState)
    {
      IMachineStateTemplateItem newTemplateItem = new MachineStateTemplateItem (machineObservationState);
      m_items.Add (newTemplateItem);
      return newTemplateItem;
    }

    /// <summary>
    /// Insert an item at the specified position
    /// </summary>
    /// <param name="position"></param>
    /// <param name="machineObservationState"></param>
    /// <returns></returns>
    public virtual IMachineStateTemplateItem InsertItem (int position, IMachineObservationState machineObservationState)
    {
      IMachineStateTemplateItem newTemplateItem = new MachineStateTemplateItem (machineObservationState);
      m_items.Insert (position, newTemplateItem);
      return newTemplateItem;
    }

    /// <summary>
    /// Add a stop condition
    /// </summary>
    /// <returns></returns>
    public virtual IMachineStateTemplateStop AddStop ()
    {
      IMachineStateTemplateStop newStop = new MachineStateTemplateStop ();
      m_stops.Add (newStop);
      return newStop;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      // Nothing to do here for the moment
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      MachineStateTemplate other = obj as MachineStateTemplate;
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
    public override int GetHashCode ()
    {
      if (0 != Id) {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * Id.GetHashCode ();
        }
        return hashCode;
      }
      else {
        return base.GetHashCode ();
      }
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[MachineStateTemplate {this.Id} {this.TranslationKey ?? this.Name}]";
      }
      else {
        return $"[MachineStateTemplate {this.Id}]";
      }
    }
  }
}
