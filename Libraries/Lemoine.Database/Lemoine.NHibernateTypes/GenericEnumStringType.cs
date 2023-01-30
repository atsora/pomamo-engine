// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using NHibernate.Type;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Convert an enum to a string in database
  /// 
  /// To use it in the mapping file:
  /// type="Lemoine.NHibernateTypes.GenericEnumStringType`1[[Lemoine.GDBPersistentClasses.MyEnum, Pulse.Database]], Lemoine.NHibernateTypes"
  /// 
  /// I made a try but it did not work. Was it a typo error?
  /// </summary>
  [Serializable]
  public class GenericEnumStringType<TEnum>: EnumStringType
  {
    private readonly string m_typeName;

    /// <summary>
    /// Constructor
    /// </summary>
    public GenericEnumStringType (): base (typeof (TEnum))
    {
      System.Type type = GetType ();
      m_typeName = type.FullName + ", " + type.Assembly.GetName ().Name;
    }

    /// <summary>
    /// 
    /// </summary>
    public override string Name
    {
      get { return m_typeName; }
    }
  }
}
