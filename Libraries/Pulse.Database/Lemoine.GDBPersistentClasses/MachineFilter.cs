// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table MachineFilter
  /// </summary>
  public class MachineFilter: IMachineFilter, IVersionable, IDisplayable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name;
    MachineFilterInitialSet m_initialSet = MachineFilterInitialSet.All;
    IList<IMachineFilterItem> m_items = new List<IMachineFilterItem> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineFilter).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">Name to use for this machine filter (not null)</param>
    /// <param name="initialSet"></param>
    public MachineFilter (string name, MachineFilterInitialSet initialSet)
    {
      this.Name = name;
      m_initialSet = initialSet;
    }
    
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected MachineFilter ()
    {
    }
    
    #region Getters / Setters
    /// <summary>
    /// Machine Filter ID
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// <see cref="IDisplayable"/>
    /// </summary>
    public virtual string Display
    {
      get { return this.Name; }
    }

    /// <summary>
    /// Name of the machine filter.
    /// </summary>
    public virtual string Name
    {
      get { return this.m_name; }
      set
      {
        Debug.Assert (null != value);
        
        if (string.IsNullOrEmpty (value)) {
          log.ErrorFormat ("Name.set: " +
                           "null or empty argument");
          throw new ArgumentNullException("Name");
        }
        
        this.m_name = value;
      }
    }
    
    /// <summary>
    /// Text to use in a selection dialog
    /// </summary>
    public virtual string SelectionText {
      get { return string.Format ("{0}: {1}",
                                  this.Id, this.Name); }
    }
    
    /// <summary>
    /// Initial set of machines
    /// </summary>
    public virtual MachineFilterInitialSet InitialSet
    {
      get { return m_initialSet; }
      set { m_initialSet = value; }
    }
    
    /// <summary>
    /// List of items
    /// </summary>
    public virtual IList<IMachineFilterItem> Items
    {
      get { return m_items; }
    }
    #endregion // Getters / Setters
    
    /// <summary>
    /// Check if a machine matches this machine filter
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public virtual bool IsMatch (IMachine machine)
    {
      Debug.Assert (null != machine);

      var initializedMachine = machine;
      bool result;

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        if (!ModelDAOHelper.DAOFactory.IsInitialized (initializedMachine)) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("IsMatch: machine id={0} is not initialized. StackTrace={1}", machine.Id, System.Environment.StackTrace);
          }
          initializedMachine = ModelDAOHelper.DAOFactory.MachineDAO
            .FindById (machine.Id);
          if (null == initializedMachine) {
            log.FatalFormat ("IsMatch: no machine with id {0}, unexpected", machine.Id);
            Debug.Assert (false);
            initializedMachine = machine;
          }
        }

        switch (this.InitialSet) {
          case MachineFilterInitialSet.None:
            result = false;
            break;
          case MachineFilterInitialSet.All:
            result = true;
            break;
          default:
            log.FatalFormat ("IsMatch: " +
                             "invalid MachineFilterInitialSet {0}",
                             this.InitialSet);
            throw new Exception ("Invalid value for MachineFilterInitialSet");
        }

        foreach (IMachineFilterItem item in Items) {
          // A null item can be among the items if order numbers are skipped in the database
          if (item == null) {
            log.Warn ("A null MachineFilterItem is among us... problably because of wrong order numbers in the database.");
            continue;
          }

          switch (item.Rule) {
            case MachineFilterRule.Add:
              if (!result) {
                result = item.IsMatch (initializedMachine);
              }
              break;
            case MachineFilterRule.Remove:
              if (result) {
                result = !item.IsMatch (initializedMachine);
              }
              break;
            default:
              log.FatalFormat ("IsMatch: " +
                               "invalid MachineFilterRule {0}",
                               item.Rule);
              throw new Exception ("Invalid value for MachineFilterRule");
          }
        }

        log.DebugFormat ("IsMatch: " +
                         "MachineFilter {0} Machine {1} Result {2}",
                         this, initializedMachine.Id, result);
      }
      return result;
    }

    #region Methods
    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[MachineFilter {this.Id} Name={this.Name}]";
      }
      else {
        return $"[MachineFilter {this.Id}]";
      }
    }
    
    /// <summary>
    /// <see cref="Object.GetHashCode" />
    /// </summary>
    /// <returns></returns>
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
    /// <see cref="Object.Equals(object)" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
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
      MachineFilter other = obj as MachineFilter;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id);
      }
      return false;
    }
    
    /// <summary>
    /// Unproxy all the properties
    /// </summary>
    public virtual void Unproxy ()
    {
      // Nothing here for the moment
    }
    #endregion // Methods
  }
}
