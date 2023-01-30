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
      Assert.IsTrue (serialized.StartsWith ("{\"X\":0,\"Y\":3,\"Z\":4"));

      var newtonsoftSettings = new Newtonsoft.Json.JsonSerializerSettings ();
      newtonsoftSettings.NullValueHandling = NullValueHandling.Ignore;
      var newtonsoftSerialized = JsonConvert.SerializeObject (position, newtonsoftSettings);
      var newtonsoftDeserialized = JsonConvert.DeserializeObject<JPosition> (newtonsoftSerialized);
      Assert.AreEqual (position, newtonsoftDeserialized);
      var newtonsoftObjectDeserialized = JsonConvert.DeserializeObject (newtonsoftSerialized);
      Assert.IsTrue (newtonsoftObjectDeserialized is Newtonsoft.Json.Linq.JObject);
      Assert.AreEqual (position, ((Newtonsoft.Json.Linq.JObject)newtonsoftObjectDeserialized).ToObject<JPosition> ());

      var objectDeserialized = JsonConvert.DeserializeObject (serialized);
      Assert.IsTrue (objectDeserialized is Newtonsoft.Json.Linq.JObject);
      Assert.AreEqual (position, ((Newtonsoft.Json.Linq.JObject)objectDeserialized).ToObject<JPosition> ());

      var autoConverter = new DefaultAutoConverter ();
      var jposition = autoConverter.ConvertAuto<JPosition> (objectDeserialized);
      Assert.AreEqual (position, jposition);
      var p = autoConverter.ConvertAuto<Position> (jposition);
      Assert.AreEqual (new Position (position), p);

      var ip = autoConverter.ConvertAuto<IPosition> (jposition);
      Assert.AreEqual (0, ip.GetAxisValue ("X"));
    }
  }
}
