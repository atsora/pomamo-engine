// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table DayTemplate
  /// </summary>
  [Serializable]
  public class DayTemplate: BaseData, IDayTemplate, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name;
    ISet<IDayTemplateItem> m_items = new HashSet<IDayTemplateItem> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (DayTemplate).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    internal protected DayTemplate ()
    { }
    
    /// <summary>
    /// Constructor for default values
    /// </summary>
    /// <param name="name">not null and not empty</param>
    internal protected DayTemplate (string name)
    {
      this.Name = name;
    }
    #endregion // Constructors
    
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
    /// MachineStateTemplate ID
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
    /// Name
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    [XmlAttribute("Name")]
    public virtual string Name {
      get { return m_name; }
      set { m_name = value; }
    }
    
    /// <summary>
    /// Display name
    /// </summary>
    [XmlIgnore]
    public virtual string Display {
      get
      {
        return this.Name;
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
    
    /// <summary>
    /// List of items that are part of the day template
    /// </summary>
    [XmlIgnore] // For the moment
    public virtual ISet<IDayTemplateItem> Items {
      get { return m_items; }
    }
    #endregion // Getters / Setters
    
    /// <summary>
    /// IDisplay implementation
    /// </summary>
    /// <param name="variant"></param>
    /// <returns></returns>
    public virtual string GetDisplay (string variant)
    {
      log.WarnFormat ("GetDisplay: " +
                      "No variant is supported for the DayTemplate");
      return this.Display;
    }
    
    /// <summary>
    /// Add an item
    /// </summary>
    /// <param name="cutOff">cut-off time</param>
    /// <param name="weekDays">Applicable week days</param>
    /// <returns></returns>
    public virtual IDayTemplateItem AddItem (TimeSpan cutOff, WeekDay weekDays)
    {
      IDayTemplateItem newTemplateItem = new DayTemplateItem (cutOff, weekDays);
      m_items.Add (newTemplateItem);
      return newTemplateItem;
    }
    
    /// <summary>
    /// Adjust a date/time to the next day begin if it is not already a day begin
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    internal protected virtual DateTime AdjustToDayBegin (DateTime dateTime)
    {
      DateTime localDateTime = dateTime.ToLocalTime ();
      TimeSpan offset = localDateTime.TimeOfDay;
      DateTime result = dateTime.AddDays (2);
      foreach (var item in this.Items) {
        TimeSpan positiveCutOff;
        DateTime weekDay;
        if (0 <= item.CutOff.Ticks) {
          positiveCutOff = item.CutOff;
          weekDay = localDateTime.Date;
        }
        else { // item.CutOff.Ticks < 0
          positiveCutOff = TimeSpan.FromHours (24).Add (item.CutOff);
          weekDay = localDateTime.Date.AddDays (1);
        }
        if (positiveCutOff.Equals (offset)) { // Same cut-off
          DateTime day = localDateTime.Date;
          if (item.WeekDays.HasFlagDayOfWeek (weekDay.DayOfWeek)) {
            log.DebugFormat ("AdjustToDayBegin: " +
                             "dateTime {0} is a day begin",
                             dateTime);
            return dateTime;
          }
          if (item.WeekDays.HasFlagDayOfWeek (weekDay.AddDays (1).DayOfWeek)) {
            DateTime possibleNextDayBegin =
              day.AddDays (1).Add (positiveCutOff).ToUniversalTime ();
            Debug.Assert (dateTime < possibleNextDayBegin);
            if (possibleNextDayBegin < result) {
              result = possibleNextDayBegin;
            }
          }
        }
        else if (positiveCutOff < offset) {
          DateTime nextDay = localDateTime.Date.AddDays (1);
          if (item.WeekDays.HasFlagDayOfWeek (weekDay.AddDays (1).DayOfWeek)) {
            DateTime possibleNextDayBegin =
              nextDay.Add (positiveCutOff).ToUniversalTime ();
            Debug.Assert (dateTime < possibleNextDayBegin);
            if (possibleNextDayBegin < result) {
              result = possibleNextDayBegin;
            }
          }
          else if (item.WeekDays.HasFlagDayOfWeek (weekDay.AddDays (2).DayOfWeek)) {
            DateTime possibleNextDayBegin =
              nextDay.AddDays (1).Add (positiveCutOff).ToUniversalTime ();
            Debug.Assert (dateTime < possibleNextDayBegin);
            if (possibleNextDayBegin < result) {
              result = possibleNextDayBegin;
            }
          }
        }
        else { // offset < positiveCutOff
          DateTime day = localDateTime.Date;
          if (item.WeekDays.HasFlagDayOfWeek (weekDay.DayOfWeek)) {
            DateTime possibleNextDayBegin =
              day.Add (positiveCutOff).ToUniversalTime ();
            Debug.Assert (dateTime < possibleNextDayBegin);
            if (possibleNextDayBegin < result) {
              result = possibleNextDayBegin;
            }
          }
          else if (item.WeekDays.HasFlagDayOfWeek (weekDay.AddDays (1).DayOfWeek)) {
            DateTime possibleNextDayBegin =
              day.AddDays (1).Add (positiveCutOff).ToUniversalTime ();
            Debug.Assert (dateTime < possibleNextDayBegin);
            if (possibleNextDayBegin < result) {
              result = possibleNextDayBegin;
            }
          }
        }
      }
      return result;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      // Nothing to do here for the moment
    }

    /// <summary>
    /// Determines whether the specified Object
    /// is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals(this, obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      DayTemplate other = obj as DayTemplate;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id);
      }
      return false;
    }
    
    /// <summary>
    /// Serves as a hash function for a particular type
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

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[DayTemplate {this.Id} Name={this.Name}]";
      }
      else {
        return $"[DayTemplate {this.Id}]";
      }
    }
  }
}
