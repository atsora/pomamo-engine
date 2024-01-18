// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Pulse.Web.UnitTests.User
{
  /// <summary>
  /// 
  /// </summary>
  public class CheckJwtToken_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (CheckJwtToken_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CheckJwtToken_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestFordClaim ()
    {
      var tokenHandler = new JwtSecurityTokenHandler ();
      var token = tokenHandler.ReadJwtToken ("eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6InI1VFl6SHFoX3BPZjVWYXRCdUI2eFF5cGFWUSJ9.eyJhdWQiOiJ1cm46bHBtbTpyZXNvdXJjZTp3ZWJfbHBtbXFhc2l0ZTpxYSIsImlzcyI6Imh0dHBzOi8vY29ycHFhLnN0cy5mb3JkLmNvbS9hZGZzL3NlcnZpY2VzL3RydXN0IiwiaWF0IjoxNjE3MTA5ODAyLCJleHAiOjE2MTcxMTM0MDIsIkNvbW1vbk5hbWUiOiJQTkcxIiwic3ViIjoiUE5HMSIsInVzZXJpZCI6InBuZzEiLCJkZXB0IjoiNTAwMUoyMTAzIiwiZW1wY29kZSI6IkYiLCJtcnJvbGUiOiJOIiwib3JnIjoiRk5BTVIiLCJjb21wYW55IjoiRm9yZCBNb3RvciBDb21wYW55IFVTQSIsImRpdmFiYnIiOiJJVFMiLCJzaXRlY29kZSI6IjUyOTgiLCJjaXR5IjoiRGVhcmJvcm4iLCJzdGF0ZSI6Ik1JIiwiY291bnRyeSI6IlVTQSIsIkFDSUdST1VQIjoiRW1wbG95ZWUiLCJzdWJqZWN0aWQiOiJQTkcxQGZvcmQuY29tIiwiYXBwdHlwZSI6IlB1YmxpYyIsImFwcGlkIjoidXJuOmxwbW06Y2xpZW50aWQ6d2ViX2xwbW1xYXNpdGU6cWEiLCJhdXRobWV0aG9kIjoiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2F1dGhlbnRpY2F0aW9ubWV0aG9kL3dpbmRvd3MiLCJhdXRoX3RpbWUiOiIyMDIxLTAzLTMwVDEzOjE1OjAyLjUwOFoiLCJ2ZXIiOiIxLjAifQ.Wfop3Hb_JLG9KfcJ6QpZHki0kWpv4CsjGLcsHWuES5KWvCqFGo48LTrMpbMaNDdhI5f1orDuMrKsVRuqGvS7AJ5ztDF1tu9Hd_PP2nU2WXmJjY9_I5l3IuJqPU4lfTGRS1GFWBjFKjB0qObXRJMTqpwAdXpvYJkXuts6Ju9UYYNF9GUBysifxS9sAHXoOGFcH96_bSr9s51jQ2XXTvIrQAwtlKldpCPF7Ty-hv5UsarwBrgyW6w_asakuEeaU7k01v79yDWiM_9SGLzfw-7Uh3uwmt5d5R8SANAnPBvSQ997BSqbA2FrcR8aszeSwaKsfa7y5P6Qz2ZWQ02_8zdU1A");
      var userIdClaim = token.Claims.First (x => x.Type.Equals ("userid"));
      Assert.That (userIdClaim.Value, Is.EqualTo ("png1"));
    }
  }
}
