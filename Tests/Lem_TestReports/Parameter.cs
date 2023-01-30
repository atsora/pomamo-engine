// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Lemoine.Core.Log;
using System.Text.RegularExpressions;

namespace Lem_TestReports
{
  
  public enum ParameterType {
    SIMPLE = 0,
    MULTI_VALUE = 1
  }
  
  /// <summary>
  /// Description of Parameter.
  /// </summary>
  public class Parameter
  {
    
    static readonly Regex REGEX_MULTIVALUED = new Regex(@"\[(.)*](\[(.)*\])*");
    #region Members
    String m_name;
    ParameterType m_type;
    IList<String> m_values = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Parameter).FullName);

    #region Getters / Setters
    public String Name {
      get{ return m_name; }
      set{ this.m_name = value; }
    }

    public ParameterType Type {
      get{ return m_type; }
      set{ this.m_type = value; }
    }

    public IList<String> Values {
      get{ return m_values; }
      set{ this.m_values = value; }
    }

    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Parameter (String name)
    {
      this.m_name = name;
      this.m_type = ParameterType.SIMPLE;
      this.m_values = new List<String>();
    }

    public Parameter (String name, ParameterType type)
    {
      this.m_name = name;
      this.m_type = type;
      this.m_values = new List<String>();
    }

    #endregion // Constructors

    #region Methods
    public static String buildViewerUrl(IList<Parameter> parameterList, ViewerType viewerType) {
      StringBuilder parameterString = new StringBuilder();
      switch (viewerType) {
        case ViewerType.PULSEREPORTING:
          foreach (Parameter parameter in parameterList) {
            if (null == parameter.Values) {
              parameterString.Append("&__isnull=");
              parameterString.Append(parameter.Name);
            }
            else {
              switch (parameter.Type) {
                case ParameterType.SIMPLE :
                  parameterString.Append("&");
                  parameterString.Append(parameter.Name);
                  parameterString.Append("=");
                  parameterString.Append(parameter.Values[0]);
                  break;
                case ParameterType.MULTI_VALUE :
                  for (int i = 0; i < parameter.Values.Count; i++) {
                    parameterString.Append("&");
                    parameterString.Append(parameter.Name);
                    parameterString.Append("=");
                    parameterString.Append(parameter.Values[i]);
                  }

                  break;
                default:
                  throw new Exception("Invalid value for ParameterType");
              }
            }
          }

          break;
        default:
          throw new Exception("Invalid value for ViewerType");
      }
      return (parameterString.Length == 0)? "": parameterString.ToString(1, parameterString.Length-1);
    }
    
    
    public static IList<Parameter> retrieveParameterList(String viewerUrl, ViewerType viewerType) {
      List<Parameter> parameterList = new List<Parameter>();
      string[] entries = viewerUrl.Split(new char[] {'&'},StringSplitOptions.RemoveEmptyEntries);
      foreach (string entry in entries) {
        string[] words = entry.Split(new char[] {'='},2,StringSplitOptions.RemoveEmptyEntries);
        if(words.Length != 2) {
          continue;
        }
        switch (viewerType) {
          case ViewerType.PULSEREPORTING:
            if(String.Equals(words[0],"__session",StringComparison.Ordinal)) {
              break;
            }
            else if (String.Equals(words[0],"__isnull",StringComparison.Ordinal)) {
              Parameter parameter1 = new Parameter(words[1]);
              parameter1.Values = null;
              parameterList.Add(parameter1);
              
            } else {
              // this variable is used to know if this parameter name was already put in parameter list
              // if true, insert current value value's list and set his type to MULTI_VALUE
              // otherwise create new parameter and insert it in parameter list
              bool alreadyExist = false;
              foreach (Parameter parameter1 in parameterList) {
                if (parameter1.Name.Equals(words[0],StringComparison.Ordinal)) {
                  parameter1.Type = ParameterType.MULTI_VALUE;
                  parameter1.Values.Add(words[1]);
                  alreadyExist = true;
                  break;
                }
              }
              if(!alreadyExist) {
                Parameter parameter1 = new Parameter(words[0]);
                parameter1.Values.Add(words[1]);
                parameterList.Add(parameter1);
              }
            }
            
            break;
          default:
            throw new Exception("Invalid value for ViewerType");
        }
      }
      return parameterList;
    }
    
    #endregion // Methods
  }
}
