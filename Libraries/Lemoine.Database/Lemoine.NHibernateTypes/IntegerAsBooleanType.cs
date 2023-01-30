// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Get a boolean from an integer column in database
  /// </summary>
  [Serializable]
  public class IntegerAsBooleanType: SimpleGenericType<Boolean>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (IntegerAsBooleanType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public IntegerAsBooleanType ()
      : base (NHibernateUtil.Int64.SqlType, NpgsqlTypes.NpgsqlDbType.Integer, false)
    {
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override Boolean Get (object v)
    {
      try {
        return (Boolean) Convert.ToBoolean (v);
      }
      catch (Exception ex) {
        throw new FormatException (string.Format ("Input '{0}' was not in the correct format",
                                                  v), ex);
      }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override object Set (Boolean v)
    {
      return v ? 1 : 0;
    }

    #region AbstractType implementation
    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public override string Name
    {
      get { return "IntegerAsBoolean"; }
    }
    #endregion // AbstractType implementation
  }
}
