// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using NHibernate.UserTypes;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Type to truncate the string if its length is more than 255 characters.
  /// </summary>
  [Serializable]
  public class TruncatedString255Type : AbstractTruncatedStringType
  {
    /// <summary>
    /// Maximum length of the string
    /// </summary>
    public override int Length
    {
      get { return 255; }
    }
    
    /// <summary>
    /// Name of the type
    /// </summary>
    public override string Name
    {
      get { return "TruncatedString255"; }
    }
  }
  
  /// <summary>
  /// Type to truncate the string if its length is more than 511 characters.
  /// </summary>
  [Serializable]
  public class TruncatedString511Type : AbstractTruncatedStringType
  {
    /// <summary>
    /// Maximum length of the string
    /// </summary>
    public override int Length
    {
      get { return 511; }
    }
    
    /// <summary>
    /// Name of the type
    /// </summary>
    public override string Name
    {
      get { return "TruncatedString511"; }
    }
  }

  /// <summary>
  /// Type to truncate the string if its length is more than 1023 characters.
  /// </summary>
  [Serializable]
  public class TruncatedString1023Type : AbstractTruncatedStringType
  {
    /// <summary>
    /// Maximum length of the string
    /// </summary>
    public override int Length
    {
      get { return 1023; }
    }
    
    /// <summary>
    /// Name of the type
    /// </summary>
    public override string Name
    {
      get { return "TruncatedString1023"; }
    }
  }

  /// <summary>
  /// Type to truncate the string if its length is more than 2047 characters.
  /// </summary>
  [Serializable]
  public class TruncatedString2047Type : AbstractTruncatedStringType
  {
    /// <summary>
    /// Maximum length of the string
    /// </summary>
    public override int Length
    {
      get { return 2047; }
    }
    
    /// <summary>
    /// Name of the type
    /// </summary>
    public override string Name
    {
      get { return "TruncatedString2047"; }
    }
  }
  
  /// <summary>
  /// Parametetrized truncated string type
  /// 
  /// The parameter to input the maximum size is length
  /// </summary>
  [Serializable]
  public class TruncatedStringType : AbstractTruncatedStringType, IParameterizedType
  {
    static readonly int DEFAULT_LIMIT = 255;
    static readonly string LENGTH_PARAMETER = "length";
    
    int m_length = DEFAULT_LIMIT;
    
    /// <summary>
    /// Maximum length of the string
    /// </summary>
    public override int Length
    {
      get { return m_length; }
    }
    
    /// <summary>
    /// Name of the type
    /// </summary>
    public override string Name
    {
      get { return "TruncatedString"; }
    }
    
    /// <summary>
    /// Parse the parameters to get the maximum length
    /// </summary>
    /// <param name="parameters"></param>
    public new void SetParameterValues(System.Collections.Generic.IDictionary<string, string> parameters)
    {
      if (parameters == null) {
        return;
      }

      base.SetParameterValues (parameters);

      if (false == int.TryParse(parameters[LENGTH_PARAMETER], out m_length)) {
        m_length = DEFAULT_LIMIT;
      }
    }
  }
}
