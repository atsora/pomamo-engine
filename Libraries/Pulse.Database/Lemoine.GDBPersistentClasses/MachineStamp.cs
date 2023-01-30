// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// abstract class  for all the machine stamp modification tables
  /// </summary>
  [Serializable,
   XmlInclude(typeof(WorkOrderMachineStamp)),
   XmlInclude(typeof(SerialNumberMachineStamp))]
  public abstract class MachineStamp: MachineModification
  {
    #region Members
    #endregion // Members
    
    #region Getters / Setters
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    protected MachineStamp ()
    {
    }
    
    /// <summary>
    /// Constructor (DateTime = UtcNow)
    /// </summary>
    /// <returns></returns>
    protected MachineStamp (IMachine machine)
      : base (machine)
    {
    }

    /// <summary>
    /// Constructor (DateTime passed as argument)
    /// </summary>
    /// <returns></returns>
    protected MachineStamp (IMachine machine, DateTime dateTime)
      : base (machine)
    {
      this.DateTime = dateTime;
    }

    #endregion // Constructors

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
    }
  }
}


