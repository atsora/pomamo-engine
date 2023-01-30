// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data.Common;
using Lemoine.Core.Log;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.Type;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Abstract truncated string type
  /// 
  /// Truncate the string if its length is greater than Length
  /// </summary>
  [Serializable]
  public abstract class AbstractTruncatedStringType : AbstractStringType
  {
    internal AbstractTruncatedStringType()
      : base(new StringSqlType())
    {
    }
    
    internal AbstractTruncatedStringType(StringSqlType sqlType)
      : base(sqlType)
    {
    }
    
    /// <summary>
    /// Maximum length of the string
    /// </summary>
    public abstract int Length { get; }
    
    /// <summary>
    /// Set the value in database after truncating it if needed
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="value"></param>
    /// <param name="index"></param>
    /// <param name="session"></param>
    public override void Set(DbCommand cmd, object value, int index, ISessionImplementor session)
    {
      string str = (string)value;
      if (str.Length > Length) {
        str = str.Substring(0, Length);
      }

      base.Set(cmd, str, index, session);
    }
  }
}
