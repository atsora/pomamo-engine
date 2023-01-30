// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ShiftTemplate
  /// </summary>
  [Serializable]
  public class ShiftTemplate: BaseData, IShiftTemplate, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name;
    ISet<IShiftTemplateItem> m_items = new HashSet<IShiftTemplateItem> ();
    ISet<IShiftTemplateBreak> m_breaks = new HashSet<IShiftTemplateBreak> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ShiftTemplate).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    internal protected ShiftTemplate ()
    { }
    
    /// <summary>
    /// Constructor for default values
    /// </summary>
    /// <param name="name">not null and not empty</param>
    internal protected ShiftTemplate (string name)
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
    /// ShiftTemplate ID
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
    /// IDisplayable implementation
    /// </summary>
    [XmlIgnore]
    public virtual string Display {
      get { return this.Name; }
    }
    
    /// <summary>
    /// IDisplayable implementation
    /// </summary>
    /// <param name="variant"></param>
    /// <returns></returns>
    public virtual string GetDisplay (string variant)
    {
      return this.Display;
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
    /// List of items that are part of the shift template
    /// </summary>
    [XmlIgnore] // For the moment
    public virtual ISet<IShiftTemplateItem> Items {
      get { return m_items; }
    }
    
    /// <summary>
    /// Set of breaks
    /// </summary>
    [XmlIgnore] // For the moment
    public virtual ISet<IShiftTemplateBreak> Breaks {
      get { return m_breaks; }
    }
    #endregion // Getters / Setters
        
    /// <summary>
    /// Append an item with the specified shift
    /// </summary>
    /// <param name="shift"></param>
    /// <returns></returns>
    public virtual IShiftTemplateItem AddItem (IShift shift)
    {
      IShiftTemplateItem newTemplateItem = new ShiftTemplateItem (shift);
      m_items.Add (newTemplateItem);
      return newTemplateItem;
    }
    
    /// <summary>
    /// Add a break
    /// </summary>
    /// <returns></returns>
    public virtual IShiftTemplateBreak AddBreak ()
    {
      IShiftTemplateBreak newBreak = new ShiftTemplateBreak ();
      m_breaks.Add (newBreak);
      return newBreak;
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
      ShiftTemplate other = obj as ShiftTemplate;
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

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[ShiftTemplate {this.Id} {this.Name}]";
      }
      else {
        return $"[ShiftTemplate {this.Id}]";
      }
    }
  }
}
