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
      Assert.That (d, Is.Not.EqualTo (null));
      XmlElement root = d.DocumentElement;
      Assert.That (root, Is.Not.EqualTo (null));
      Assert.Multiple (() => {
        Assert.That (root.Name, Is.EqualTo ("root"));
        Assert.That (root.GetElementsByTagName ("ClassA"), Has.Count.EqualTo (2));
      });
      XmlElement element = root.GetElementsByTagName ("ClassA") [0] as XmlElement;
      XmlSerializer xmlSerializer = new XmlSerializer (typeof (ClassA));
      ClassA first;
      using (TextReader reader = new StringReader (element.OuterXml))
      {
        first = (ClassA) xmlSerializer.Deserialize (reader);
      }

      Assert.Multiple (() => {
        Assert.That (first.I, Is.EqualTo (1));
        Assert.That (first.L, Is.Not.Null);
      });
      Assert.That (first.L, Is.Empty); // Note: here the null list is converted to an empty list
      Assert.That (first.S, Is.EqualTo ("s1"));
      element = root.GetElementsByTagName ("ClassA") [1] as XmlElement;
      ClassA second;
      using (TextReader reader = new StringReader (element.OuterXml))
      {
        second = (ClassA) xmlSerializer.Deserialize (reader);
      }

      Assert.Multiple (() => {
        Assert.That (second.I, Is.EqualTo (2));
        Assert.That (second.L, Is.Not.Null);
      });
      Assert.That (second.L, Is.Empty);
      Assert.That (second.S, Is.EqualTo ("s2"));
      
      // List null
      List<ClassA> list = null;
      f = new ListFactory<ClassA> (list);
      d = f.GetData (cancellationToken: System.Threading.CancellationToken.None);
      Assert.That (d, Is.Not.EqualTo (null));
      root = d.DocumentElement;
      Assert.That (root, Is.Not.EqualTo (null));
      Assert.Multiple (() => {
        Assert.That (root.Name, Is.EqualTo ("root"));
        Assert.That (root.HasChildNodes, Is.EqualTo (false));
      });
    }
  }
}
