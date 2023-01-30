// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Get a Integer object from a column in string format in database
  /// </summary>
  [Serializable]
  public class StringAsInt32Type: SimpleGenericType<Int32>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (StringAsInt32Type).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public StringAsInt32Type ()
      : base (NHibernateUtil.String.SqlType, NpgsqlTypes.NpgsqlDbType.Text, false)
    {
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override Int32 Get (object v)
    {
      try {
        return (Int32) Convert.ToInt32 (v);
      }
      catch (Exception ex) {
        throw new FormatException (string.Format ("Input string '{0}' was not in the correct format",
                                                  v), ex);
      }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected override object Set (Int32 v)
    {
      return v.ToString ();
    }

    #region AbstractType implementation
    /// <summary>
    /// AbstractType implementation
    /// </summary>
    public override string Name
		{
			get { return "StringAsInt32"; }
		}
		#endregion // AbstractType implementation
  }
}
