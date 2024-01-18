// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Cnc;
using NUnit.Framework;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Lemoine.Conversion.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class CncObjects_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (CncObjects_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CncObjects_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestJPosition ()
    {
      var position = new JPosition (0, 3, 4);
      var options = new JsonSerializerOptions {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
      };
      options.Converters.Add (new JsonStringEnumConverter ());
      var serialized = System.Text.Json.JsonSerializer.Serialize (position, options);
      Assert.That (serialized, Does.StartWith ("{\"X\":0,\"Y\":3,\"Z\":4"));

      var newtonsoftSettings = new Newtonsoft.Json.JsonSerializerSettings ();
      newtonsoftSettings.NullValueHandling = NullValueHandling.Ignore;
      var newtonsoftSerialized = JsonConvert.SerializeObject (position, newtonsoftSettings);
      var newtonsoftDeserialized = JsonConvert.DeserializeObject<JPosition> (newtonsoftSerialized);
      Assert.That (newtonsoftDeserialized, Is.EqualTo (position));
      var newtonsoftObjectDeserialized = JsonConvert.DeserializeObject (newtonsoftSerialized);
      Assert.Multiple (() => {
        Assert.That (newtonsoftObjectDeserialized is Newtonsoft.Json.Linq.JObject, Is.True);
        Assert.That (((Newtonsoft.Json.Linq.JObject)newtonsoftObjectDeserialized).ToObject<JPosition> (), Is.EqualTo (position));
      });

      var objectDeserialized = JsonConvert.DeserializeObject (serialized);
      Assert.Multiple (() => {
        Assert.That (objectDeserialized is Newtonsoft.Json.Linq.JObject, Is.True);
        Assert.That (((Newtonsoft.Json.Linq.JObject)objectDeserialized).ToObject<JPosition> (), Is.EqualTo (position));
      });

      var autoConverter = new DefaultAutoConverter ();
      var jposition = autoConverter.ConvertAuto<JPosition> (objectDeserialized);
      Assert.That (jposition, Is.EqualTo (position));
      var p = autoConverter.ConvertAuto<Position> (jposition);
      Assert.That (p, Is.EqualTo (new Position (position)));

      var ip = autoConverter.ConvertAuto<IPosition> (jposition);
      Assert.That (ip.GetAxisValue ("X"), Is.EqualTo (0));
    }
  }
}
