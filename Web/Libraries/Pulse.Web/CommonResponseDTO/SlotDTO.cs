// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// Response DTO for a generic slot
  /// </summary>
  [Api("Generic slot DTO")]
  public class SlotDTO
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    public SlotDTO ()
    { }
    
    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="range"></param>
    public SlotDTO (int id, string range)
    {
      this.Id = id;
      this.Range = range;
    }
    
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Display
    /// </summary>
    public string Display { get; set; }
    
    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }
    
    /// <summary>
    /// Foreground color
    /// </summary>
    public string FgColor { get; set; }
    
    /// <summary>
    /// Background color
    /// </summary>
    public string BgColor { get; set; }
    
    /// <summary>
    /// Name of the pattern
    /// 
    /// One of the following:
    /// <item>circles-n with n=1..9</item>
    /// <item>diagonal-stripe-n with n=1..6</item>
    /// <item>dots-n with n=1..9</item>
    /// <item>vertical-stripe-n with n=1..9</item>
    /// <item>crosshatch</item>
    /// <item>houndstooth</item>
    /// <item>lightstripe</item>
    /// <item>smalldot</item>
    /// <item>verticalstripe</item>
    /// <item>whitecarbon</item>
    /// 
    /// See: http://iros.github.io/patternfills/sample_svg.html
    /// </summary>
    public string PatternName { get; set; }
    
    /// <summary>
    /// Color of the pattern
    /// </summary>
    public string PatternColor { get; set; }
  }
}
