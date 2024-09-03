// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.SharedData;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of the life of a tool
  /// ToolLifeDataItem can contain several LifeDescription
  /// </summary>
  public class ToolLifeDescription
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    public ToolLifeDescription () { }

    /// <summary>
    /// Build a LifeDescription object based on a textual description
    /// </summary>
    /// <param name="objectStr"></param>
    public ToolLifeDescription (string objectStr)
    {
      ParseString (objectStr);
    }

    /// <summary>
    /// Life type
    /// Defines the kind of metrics we use
    /// </summary>
    public ToolUnit LifeType { get; set; }

    /// <summary>
    /// Life direction
    /// Up if the life increases normally from 0 to LifeLimit
    /// Down if the life decreases normally from LifeLimit to 0
    /// </summary>
    public ToolLifeDirection LifeDirection { get; set; }

    /// <summary>
    /// Life value
    /// Current life value that increases or decreases
    /// </summary>
    public double LifeValue { get; set; }

    /// <summary>
    /// Life limit
    /// Defining the maximum to be reached before expiration,
    /// or the initial value of LifeValue that will be decreased
    /// </summary>
    public double? LifeLimit
    {
      get { return m_lifeLimit; }
      set {
        if (value.HasValue && value.Value == 0) {
          m_lifeLimit = null;
        }
        else {
          m_lifeLimit = value;
        }
      }
    }
    double? m_lifeLimit = null;

    /// <summary>
    /// Life warning offset
    /// Defines the offset before the limit is reached
    /// </summary>
    public double? LifeWarningOffset
    {
      get { return m_lifeWarningOffset; }
      set {
        if (value.HasValue && value.Value == 0) {
          m_lifeWarningOffset = null;
        }
        else {
          m_lifeWarningOffset = value;
        }
      }
    }
    double? m_lifeWarningOffset = null;

    /// <summary>
    /// Override of the ToString() method
    /// (properties are not in the result)
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      return string.Format ("Type:{0}, Direction:{1}, Value:{2}, Limit:{3}, WarningOffset:{4}",
        (int)LifeType, (int)LifeDirection, LifeValue, LifeLimit ?? -1, LifeWarningOffset ?? -1);
    }

    void ParseString (string objectStr)
    {
      var mainSplit = objectStr.Split (',');
      foreach (var elt in mainSplit) {
        var split = elt.Trim ().Split (':');
        if (split.Length == 2) {
          switch (split[0]) {
            case "Type":
              LifeType = (ToolUnit)Int32.Parse (split[1]);
              break;
            case "Direction":
              LifeDirection = (ToolLifeDirection)Int32.Parse (split[1]);
              break;
            case "Value":
              LifeValue = Double.Parse (split[1]);
              break;
            case "Limit":
              LifeLimit = Double.Parse (split[1]);
              if (LifeLimit < 0) {
                LifeLimit = null;
              }

              break;
            case "WarningOffset":
              LifeWarningOffset = Double.Parse (split[1]);
              if (LifeWarningOffset < 0) {
                LifeWarningOffset = null;
              }

              break;
            default:
              // Ignored
              break;
          }
        }
      }
    }
  }

  /// <summary>
  /// Description of a specific tool
  /// ToolLifeData can contain several ToolLifeDataItem
  /// </summary>
  public class ToolLifeDataItem
  {
    readonly IList<ToolLifeDescription> m_lifeDescriptions = new List<ToolLifeDescription> ();
    readonly IDictionary<string, object> m_properties = new Dictionary<string, object> ();

    /// <summary>
    /// Default constructor
    /// </summary>
    public ToolLifeDataItem () { }

    /// <summary>
    /// Load a string
    /// </summary>
    /// <param name="stringObject"></param>
    public ToolLifeDataItem (string stringObject)
    {
      ParseString (stringObject);
    }

    /// <summary>
    /// Number of life descriptions described
    /// </summary>
    public int LifeDescriptionNumber { get { return m_lifeDescriptions.Count; } }

    /// <summary>
    /// Get all the tool life descriptions
    /// </summary>
    public IList<ToolLifeDescription> LifeDescriptions => m_lifeDescriptions;

    /// <summary>
    /// Override of the [] operator (acts as a getter)
    /// </summary>
    public ToolLifeDescription this[int index] { get { return m_lifeDescriptions[index]; } }

    /// <summary>
    /// Magazine number
    /// </summary>
    public int? MagazineNumber { get; set; }

    /// <summary>
    /// Pot number
    /// </summary>
    public int? PotNumber { get; set; }

    string m_toolNumber = "0";

    /// <summary>
    /// Tool number
    /// </summary>
    public string ToolNumber
    {
      get { return m_toolNumber; }
      set {
        m_toolNumber = value;
        if (string.IsNullOrEmpty (ToolId)) {
          ToolId = value;
        }
      }
    }

    /// <summary>
    /// Tool id
    /// </summary>
    public string ToolId { get; set; }

    /// <summary>
    /// Tool state
    /// </summary>
    public ToolState ToolState { get; set; }

    /// <summary>
    /// Properties associated to the pot
    /// </summary>
    public IDictionary<string, object> Properties
    {
      get { return m_properties; }
    }

    /// <summary>
    /// Add a new life description
    /// </summary>
    /// <returns>The index of the added life description</returns>
    public int AddLifeDescription ()
    {
      m_lifeDescriptions.Add (new ToolLifeDescription ());
      return m_lifeDescriptions.Count - 1;
    }

    /// <summary>
    /// Add a new tool life description and return it
    /// </summary>
    /// <returns></returns>
    public ToolLifeDescription AddNewLifeDescription ()
    {
      var toolLifeDescription = new ToolLifeDescription ();
      m_lifeDescriptions.Add (toolLifeDescription);
      return toolLifeDescription;
    }

    /// <summary>
    /// Add an existing life description (make a copy)
    /// </summary>
    /// <param name="lifeDescription"></param>
    public void AddLifeDescription (ToolLifeDescription lifeDescription)
    {
      int index = m_lifeDescriptions.Count;

      // Copy the tool life
      this.AddLifeDescription ();
      this[index].LifeDirection = lifeDescription.LifeDirection;
      this[index].LifeLimit = lifeDescription.LifeLimit;
      this[index].LifeType = lifeDescription.LifeType;
      this[index].LifeValue = lifeDescription.LifeValue;
      this[index].LifeWarningOffset = lifeDescription.LifeWarningOffset;
    }

    /// <summary>
    /// Remove a life description
    /// </summary>
    /// <param name="index"></param>
    public void RemoveLifeDescription (int index)
    {
      m_lifeDescriptions.RemoveAt (index);
    }

    /// <summary>
    /// Clear all life descriptions
    /// </summary>
    public void ClearLifeDescriptions ()
    {
      m_lifeDescriptions.Clear ();
    }

    /// <summary>
    /// Override of the ToString() method
    /// (properties are not in the result)
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      // Process life descriptions
      string str = "";
      bool first = true;
      foreach (var item in m_lifeDescriptions) {
        if (first) {
          first = false;
        }
        else {
          str += "#";
        }

        str += item.ToString ();
      }

      return string.Format ("ToolId={0}; ToolNumber={1}; Pot={2}; Magazine={3}; ToolState={4}; LifeDescription={5}",
        ToolId, ToolNumber, PotNumber ?? -1, MagazineNumber ?? -1, (int)ToolState, str);
    }

    void ParseString (string stringObject)
    {
      var mainSplit = stringObject.Split (';');
      foreach (var elt in mainSplit) {
        var split = elt.Trim ().Split ('=');
        if (split.Length == 2) {
          switch (split[0]) {
            case "ToolId":
              ToolId = split[1];
              break;
            case "ToolNumber":
              ToolNumber = split[1];
              break;
            case "Pot":
              PotNumber = Int32.Parse (split[1]);
              if (PotNumber < 0) {
                PotNumber = null;
              }

              break;
            case "Magazine":
              MagazineNumber = Int32.Parse (split[1]);
              if (MagazineNumber < 0) {
                MagazineNumber = null;
              }

              break;
            case "ToolState":
              ToolState = (ToolState)Int32.Parse (split[1]);
              break;
            case "LifeDescription":
              var subSplit = split[1].Split ('#');
              foreach (var lifeElt in subSplit) {
                try {
                  m_lifeDescriptions.Add (new ToolLifeDescription (lifeElt));
                }
                catch (Exception) {

                }
              }
              break;
            default:
              // ignored
              break;
          }
        }
      }
    }

    /// <summary>
    /// Get a property
    /// </summary>
    /// <param name="property">can be null</param>
    /// <returns></returns>
    public object GetProperty (string property)
    {
      return m_properties.ContainsKey (property) ? m_properties[property] : null;
    }

    /// <summary>
    /// Set a property
    /// List of known properties
    /// * LengthCompensation (double)
    /// * CutterCompensation (double)
    /// * GeometryUnit (ToolUnit)
    /// * ATCSpeed (int: 0=normal, 1=slow, 2=middle)
    /// </summary>
    /// <param name="property"></param>
    /// <param name="value"></param>
    public void SetProperty (string property, object value)
    {
      m_properties[property] = value;
    }

    /// <summary>
    /// Clear all properties
    /// </summary>
    public void ClearProperties ()
    {
      m_properties.Clear ();
    }
  }

  /// <summary>
  /// This class stores all data describing the state of the tools, for a specific machine.
  /// It is filled by input modules (Fanuc, MTConnect, ...) and processed by the output module
  /// "Lemoine.Cnc.ToolLife" which stores the data in the database, associated with events.
  /// </summary>
  public class ToolLifeData
  {
    #region Members
    readonly IList<ToolLifeDataItem> m_items = new List<ToolLifeDataItem> ();
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    public ToolLifeData () { }

    /// <summary>
    /// Load an object ToolLifeData based on a textual description coming from ToString()
    /// </summary>
    /// <param name="strObject"></param>
    public ToolLifeData (string strObject)
    {
      ParseString (strObject);
    }
    #endregion Constructors

    #region Getters / Setters
    /// <summary>
    /// Number of tools described
    /// </summary>
    public int ToolNumber { get { return m_items.Count; } }

    /// <summary>
    /// Tool life items
    /// </summary>
    public IList<ToolLifeDataItem> Items => m_items;

    /// <summary>
    /// Override of the [] operator (acts as a getter)
    /// </summary>
    public ToolLifeDataItem this[int index] { get { return m_items[index]; } }
    #endregion // Getters / Setters

    #region Methods
    /// <summary>
    /// Add a new tool
    /// </summary>
    /// <returns>The index of the added tool in this container</returns>
    public int AddTool ()
    {
      m_items.Add (new ToolLifeDataItem ());
      return m_items.Count - 1;
    }

    /// <summary>
    /// Add a new tool data and return it
    /// </summary>
    /// <returns></returns>
    public ToolLifeDataItem AddNewTool ()
    {
      var toolDataItem = new ToolLifeDataItem ();
      m_items.Add (toolDataItem);
      return toolDataItem;
    }

    /// <summary>
    /// Add an existing tool (make a copy of it)
    /// </summary>
    /// <param name="item"></param>
    public void AddTool (ToolLifeDataItem item)
    {
      int index = m_items.Count;

      // Copy the tool
      this.AddTool ();
      m_items[index].MagazineNumber = item.MagazineNumber;
      m_items[index].PotNumber = item.PotNumber;
      m_items[index].ToolNumber = item.ToolNumber;
      m_items[index].ToolId = item.ToolId;
      m_items[index].ToolState = item.ToolState;
      m_items[index].ClearProperties ();
      foreach (var key in item.Properties.Keys) {
        m_items[index].SetProperty (key, item.Properties[key]);
      }

      // Copy the tool life
      for (int i = 0; i < item.LifeDescriptionNumber; i++) {
        m_items[index].AddLifeDescription (item[i]);
      }
    }

    /// <summary>
    /// Remove a tool
    /// </summary>
    /// <param name="index"></param>
    public void RemoveTool (int index)
    {
      m_items.RemoveAt (index);
    }

    /// <summary>
    /// Clear all tools
    /// </summary>
    public void ClearTools ()
    {
      m_items.Clear ();
    }

    /// <summary>
    /// Function to call before processing the content and store it in the database
    /// Remove tools with no id
    /// Remove duplicated ids
    /// </summary>
    public void Filter ()
    {
      IList<string> ids = new List<string> ();
      int size = m_items.Count;
      for (int i = size - 1; i >= 0; i--) {
        string id = m_items[i].ToolId;
        if (string.IsNullOrEmpty (id) || ids.Contains (id)) {
          m_items.RemoveAt (i);
        }
        else {
          ids.Add (id);
        }
      }
    }

    /// <summary>
    /// Override of the ToString() method
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      string str = "";
      bool first = true;
      foreach (var item in m_items) {
        if (first) {
          first = false;
        }
        else {
          str += "|";
        }

        str += item.ToString ();
      }
      return str;
    }

    void ParseString (string stringObject)
    {
      var split = stringObject.Split ('|');
      foreach (var elt in split) {
        try {
          m_items.Add (new ToolLifeDataItem (elt));
        }
        catch (Exception) {
          // Cannot parse the element
        }
      }
    }

    /// <summary>
    /// Clone the current object
    /// </summary>
    /// <returns></returns>
    public ToolLifeData Clone ()
    {
      var tld = new ToolLifeData ();
      for (int i = 0; i < this.ToolNumber; i++) {
        tld.AddTool (this[i]);
      }

      return tld;
    }
    #endregion // Methods
  }
}
