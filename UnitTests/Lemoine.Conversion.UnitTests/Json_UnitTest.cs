using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lemoine.Conversion.UnitTests
{
  public class Json_UnitTest
  {
    [Test]
    public void TestDictionarySerialization () {
      var d = new Dictionary<string, object> ();
      d["123"] = 123.0;
      var ts = d.GetType ().ToString ();
      var json = JsonSerializer.Serialize (d, JsonSerializerOptions.Default);
      Assert.That (json, Is.EqualTo ("""
        {"123":123}
        """));
      var t = Type.GetType (ts);
      var d1 = (IDictionary<string, object>)JsonSerializer.Deserialize (json, t);
      var v = d1["123"];
    }
  }
}
