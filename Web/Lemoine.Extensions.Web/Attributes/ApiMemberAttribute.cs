// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Extensions.Web.Attributes
{
  /// <summary>
  /// ApiMember attribute in a web service definition:
  /// [ApiMember(Name="Domain", Description="Domain name", ParameterType="path", DataType="string", IsRequired=true)]
  /// 
  /// TODO: limit to some values
  /// </summary>
  //TODO define attribute scope
  [AttributeUsage (AttributeTargets.Property, AllowMultiple = false)]
  public class ApiMemberAttribute : Attribute
  {
    /// <summary>
    /// </summary>
    public ApiMemberAttribute ()
    {
    }

    /// <summary>
    /// Name of the parameter
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Parameter type:
    /// <item>path</item>
    /// </summary>
    public string ParameterType { get; set; }

    /// <summary>
    /// Data type:
    /// <item>int</item>
    /// <item>double</item>
    /// <item>boolean</item>
    /// <item>string</item>
    /// </summary>
    public string DataType { get; set; }

    /// <summary>
    /// Is the parameter required ?
    /// </summary>
    public bool IsRequired { get; set; }
  }
}
