// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.Business.Extension
{
  /// <summary>
  /// Request class to get extensions (from cache) that depend on a name and on a machine
  /// </summary>
  public class NameMachineExtensions<T>
    : Parameter2Extensions<T, string, IMachine>
    , IRequest<IEnumerable<T>>
    where T : Lemoine.Extensions.IExtension, Lemoine.Extensions.Extension.Categorized.INamed
  {
    readonly string m_name;
    readonly IMachine m_machine;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="machine">not null</param>
    public NameMachineExtensions (string name, IMachine machine)
      : base (name, machine, GetKey)
    {
      m_name = name;
      m_machine = machine;
    }

    /// <summary>
    /// Constructor with a filter lambda function
    /// 
    /// To be used when the Initialize method is available in the extension and returns true
    /// </summary>
    /// <param name="name"></param>
    /// <param name="machine">not null</param>
    /// <param name="filter"></param>
    public NameMachineExtensions (string name, IMachine machine, Func<T, IMachine, bool> filter)
      : base (name, machine, GetKey, ConvertFilter (filter))
    {
      m_name = name;
      m_machine = machine;
    }

    static string GetKey (string name, IMachine machine)
    {
      Debug.Assert (null != machine);
      return name + machine.Id;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>Group or null if not found</returns>
    public override IEnumerable<T> Get ()
    {
      return Lemoine.Extensions.ExtensionManager
        .GetExtensions<T> ()
        .Where (ext => this.Filter (ext, m_name, m_machine))
        .ToList (); // ToList is mandatory else the result of the Linq command is not cached
    }

    static Func<T, string, IMachine, bool> ConvertFilter (Func<T, IMachine, bool> f)
    {
      return (ext, name, machine) => f (ext, machine) && NameFilter (ext, name);
    }

    static bool NameFilter (T ext, string name)
    {
      return string.Equals (ext.Name, name,
        StringComparison.InvariantCultureIgnoreCase);
    }
    #endregion // Constructors
  }
}
