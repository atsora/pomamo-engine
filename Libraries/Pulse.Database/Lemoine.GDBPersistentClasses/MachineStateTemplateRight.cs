// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;

using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table MachineStateTemplateRight
  /// </summary>
  [Serializable]
  public class MachineStateTemplateRight: Right, IMachineStateTemplateRight, IDataGridViewMachineStateTemplateRight, IVersionable
  {
    #region Members
    IMachineStateTemplate m_machineStateTemplate;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateRight).FullName);

    #region Getters / Setters
    /// <summary>
    /// Associated machine state template
    /// 
    /// null means it is applicable to all the machine state templates
    /// </summary>
    [XmlElement("MachineStateTemplate")]
    public virtual IMachineStateTemplate MachineStateTemplate
    {
      get { return this.m_machineStateTemplate; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    internal protected MachineStateTemplateRight ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineStateTemplate"></param>
    /// <param name="role"></param>
    /// <param name="accessPrivilege"></param>
    public MachineStateTemplateRight (IMachineStateTemplate machineStateTemplate, IRole role, RightAccessPrivilege accessPrivilege)
      : base (role, accessPrivilege)
    {
      m_machineStateTemplate = machineStateTemplate;
    }
    #endregion // Constructors
    
    /// <summary>
    /// Implementation of Lemoine.Model.ISerializable
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
      NHibernateHelper.Unproxy<IMachineStateTemplate> (ref m_machineStateTemplate);
    }
    
    #region Equals and GetHashCode implementation
    /// <summary>
    /// <see cref="Object.Equals(object)" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      MachineStateTemplateRight other = obj as MachineStateTemplateRight;
      if (other == null) {
        return false;
      }

      return (this.AccessPrivilege.Equals(other.AccessPrivilege) && this.Id.Equals(other.Id) && this.Version.Equals(other.Version)
        && this.Role.Equals(other.Role) && object.Equals(this.MachineStateTemplate, other.MachineStateTemplate));
    }
    
    /// <summary>
    /// <see cref="Object.GetHashCode" />
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        if (m_machineStateTemplate != null) {
          hashCode += 1000000007 * m_machineStateTemplate.GetHashCode();
        }
      }
      return hashCode;
    }
    #endregion

  }
}
