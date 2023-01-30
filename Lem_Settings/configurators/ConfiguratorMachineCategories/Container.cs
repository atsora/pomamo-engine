// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace ConfiguratorMachineCategories
{
  /// <summary>
  /// Description of Container.
  /// </summary>
  public class Container
  {
    public class Element : IComparable
    {
      public object Item { get; set; }
      public string Name { get; set; }
      public string Code { get; set; }
      public int DisplayPriority { get; set; }
      
      public Element(object item, string name, string code, int displayPriority)
      {
        Item = item;
        Name = name;
        Code = code;
        DisplayPriority = displayPriority;
      }
      
      public int CompareTo(object obj)
      {
        if (obj == null) {
          return -1;
        }

        Element other = obj as Element;
        if (other == null) {
          return -1;
        }

        if (DisplayPriority == other.DisplayPriority) {
          return Name.CompareTo(other.Name);
        }
        else {
          return DisplayPriority.CompareTo(other.DisplayPriority);
        }
      }
    }
    
    #region Getters / Setters
    /// <summary>
    /// Elements comprised in the container
    /// </summary>
    public IList<Element> Elements { get; private set; }
    
    /// <summary>
    /// Elements deleted
    /// </summary>
    public IList<Element> DeletedElements { get; private set; }
    
    /// <summary>
    /// Return true if all new codes are unique
    /// </summary>
    public bool AreCodesUnique {
      get {
        IList<string> codes = new List<string>();
        foreach (Element element in Elements) {
          if (!String.IsNullOrEmpty(element.Code)) {
            if (codes.Contains(element.Code)) {
              return false;
            }
            else {
              codes.Add(element.Code);
            }
          }
        }
        return true;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Container(IList<Element> elements)
    {
      List<Element> list = new List<Container.Element>(elements);
      list.Sort();
      Elements = list;
      DeletedElements = new List<Container.Element>();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a new element to the list
    /// </summary>
    /// <param name="position"></param>
    /// <param name="name"></param>
    /// <param name="code"></param>
    public void AddElement(int position, string name, string code)
    {
      Elements.Insert(position, new Element(null, name, code, 0));
    }
    
    /// <summary>
    /// Move an element upward
    /// </summary>
    /// <param name="index"></param>
    public void Up(int index)
    {
      Element elTmp = Elements[index - 1];
      Elements[index - 1] = Elements[index];
      Elements[index] = elTmp;
    }
    
    /// <summary>
    /// Move an element downward
    /// </summary>
    /// <param name="index"></param>
    public void Down(int index)
    {
      Element elTmp = Elements[index + 1];
      Elements[index + 1] = Elements[index];
      Elements[index] = elTmp;
    }
    
    /// <summary>
    /// Move an element to the top
    /// </summary>
    /// <param name="index"></param>
    public void Top(int index)
    {
      Element elTmp = Elements[index];
      Elements.RemoveAt(index);
      Elements.Insert(0, elTmp);
    }
    
    /// <summary>
    /// Move an element to the bottom
    /// </summary>
    /// <param name="index"></param>
    public void Bottom(int index)
    {
      Element elTmp = Elements[index];
      Elements.RemoveAt(index);
      Elements.Add(elTmp);
    }
    
    /// <summary>
    /// Delete an element
    /// </summary>
    /// <param name="index"></param>
    public void Delete(int index)
    {
      Element elTmp = Elements[index];
      Elements.RemoveAt(index);
      if (elTmp.Item != null) {
        DeletedElements.Add(elTmp);
      }
    }
    
    /// <summary>
    /// Edit an element
    /// </summary>
    /// <param name="index"></param>
    /// <param name="name"></param>
    /// <param name="code"></param>
    public void Set(int index, string name, string code)
    {
      Element elTmp = Elements[index];
      elTmp.Name = name;
      elTmp.Code = code;
    }
    #endregion // Methods
  }
}
