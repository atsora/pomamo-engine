// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.DataRepository.UnitTests
{
  /// <summary>
  /// Unit tests for the class ListFactory.
  /// </summary>
  [TestFixture]
  public class ListFactory_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ListFactory_UnitTest).FullName);

    [Serializable]
    public class ClassA
    {
      int i;
      List<int> l;
      string s;
      
      [XmlAttribute("I")]
      public int I {
        get { return i; }
        set { i = value; }
      }
      
      [XmlElement]
      public List<int> L {
        get { return l; }
        set { l = value; }
      }
      
      [XmlAttribute("S")]
      public string S {
        get { return s; }
        set { s = value; }
      }
      
      public ClassA()
      { }
      
      public ClassA(int a, List <int> l, string s)
      {
        this.i = a;
        this.l = l;
        this.s = s;
      }
    }
    
    IList <ClassA> GenerateList ()
    {
      List <ClassA> list = new List <ClassA> ();
      list.Add (new ClassA (1, null, "s1"));
      list.Add (new ClassA (2, null, "s2"));
      return list;
    }
    
    /// <summary>
    /// Test the GetData method
    /// </summary>
    [Test]
    public void TestGetData ()
    {
      ListFactory<ClassA> f;
      XmlDocument d;
      
      // Given a list
      f = new ListFactory<ClassA> (new ListMaker<ClassA> (GenerateList));
      d = f.GetData (cancellationToken: System.Threading.CancellationToken.None);
      Assert.AreNotEqual (d, null);
      XmlElement root = d.DocumentElement;
      Assert.AreNotEqual (root, null);
      Assert.AreEqual (root.Name, "root");
      Assert.AreEqual (root.GetElementsByTagName ("ClassA").Count, 2);
      XmlElement element = root.GetElementsByTagName ("ClassA") [0] as XmlElement;
      XmlSerializer xmlSerializer = new XmlSerializer (typeof (ClassA));
      ClassA first;
      using (TextReader reader = new StringReader (element.OuterXml))
      {
        first = (ClassA) xmlSerializer.Deserialize (reader);
      }
      Assert.AreEqual (1, first.I);
      Assert.IsNotNull (first.L);
      Assert.AreEqual (0, first.L.Count); // Note: here the null list is converted to an empty list
      Assert.AreEqual ("s1", first.S);
      element = root.GetElementsByTagName ("ClassA") [1] as XmlElement;
      ClassA second;
      using (TextReader reader = new StringReader (element.OuterXml))
      {
        second = (ClassA) xmlSerializer.Deserialize (reader);
      }
      Assert.AreEqual (2, second.I);
      Assert.IsNotNull (second.L);
      Assert.AreEqual (0, second.L.Count);
      Assert.AreEqual ("s2", second.S);
      
      // List null
      List<ClassA> list = null;
      f = new ListFactory<ClassA> (list);
      d = f.GetData (cancellationToken: System.Threading.CancellationToken.None);
      Assert.AreNotEqual (d, null);
      root = d.DocumentElement;
      Assert.AreNotEqual (root, null);
      Assert.AreEqual (root.Name, "root");
      Assert.AreEqual (root.HasChildNodes, false);
    }
  }
}
