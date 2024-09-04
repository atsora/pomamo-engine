// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Lemoine.Core.Log;

namespace Lemoine.Settings
{
  /// <summary>
  /// Data that navigates through pages of items
  /// ToString() has to be implemented for every item that can be contained
  /// a system of log will use it
  /// </summary>
  public class ItemData
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ItemData).FullName);

    private class ObjectLogged
    {
      object m_object = null;

      public object Obj
      {
        get { return m_object; }
        set {
          if (value != null && !Type.IsInstanceOfType (value)) {
            string message = String.Format (@"The types mismatch:
  - expected: {0},
  - received: {1} ({2}).",
                                           Type, value.GetType (), value);
            log.FatalFormat (message);
            throw new ArgumentException (message);
          }
          m_object = value;
        }
      }
      public bool Logged { get; private set; }
      public ObjectLogged (Type type, bool logged) { Type = type; Logged = logged; }
      public override string ToString () { return (m_object ?? "-").ToString (); }

      Type Type { get; set; }
    }

    #region Members
    readonly IDictionary<string, IDictionary<string, ObjectLogged>> m_values = new Dictionary<string, IDictionary<string, ObjectLogged>> ();
    string m_currentPageName = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Name of the current page, that has to be set prior to every method
    /// An empty string means common data
    /// </summary>
    public string CurrentPageName
    {
      get {
        if (m_currentPageName == null) {
          const string message = "CurrentPageIndex has never been set";
          log.FatalFormat (message);
          throw new Exception (message);
        }
        return m_currentPageName;
      }
      set {
        if (value == null) {
          const string message = "The name of the page cannot be null";
          log.FatalFormat (message);
          throw new ArgumentNullException (message);
        }
        m_currentPageName = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default the constructor
    /// </summary>
    public ItemData () { }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Initialize a value having a specific type, associated to a key
    /// Take care CurrentPageName has been previously set
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="initValue"></param>
    /// <param name="showInLog"></param>
    public void InitValue<T> (string key, T initValue, bool showInLog = true)
    {
      InitValue (key, typeof (T), initValue, showInLog);
    }

    /// <summary>
    /// Initialize a value having a specific type, associated to a key
    /// Take care CurrentPageName has been previously set
    /// </summary>
    /// <param name="key">key to use</param>
    /// <param name="type">type of the value to store</param>
    /// <param name="initialValue">initial value to store</param>
    /// <param name="showInLog">visibility of the value in the logs (false in the case of
    /// data related to the GUI settings for instance)</param>
    public void InitValue (string key, Type type, object initialValue, bool showInLog)
    {
      if (!m_values.ContainsKey (CurrentPageName)) {
        m_values[CurrentPageName] = new Dictionary<string, ObjectLogged> ();
      }

      if (m_values[CurrentPageName].ContainsKey (key)) {
        log.Fatal ($"InitValue: {key} is already defined");
        throw new InvalidOperationException ($"{key} is already defined");
      }

      m_values[CurrentPageName][key] = new ObjectLogged (type, showInLog);
      Store (key, initialValue);
    }

    /// <summary>
    /// Store a value for a specific page, associated to a key.
    /// Take care CurrentPageName has been previously set.
    /// An exception will be raised of the key is not found or if the type of
    /// the object is different from the type defined in the InitValue method.
    /// </summary>
    /// <param name="key">key to use</param>
    /// <param name="value">value to store</param>
    public void Store (string key, object value)
    {
      try {

        // Try to store for a specific page
        if (m_values.ContainsKey (CurrentPageName)) {
          if (m_values[CurrentPageName].ContainsKey (key)) {
            m_values[CurrentPageName][key].Obj = value;
            return;
          }
        }

        // Try to store in the common data
        if (!string.IsNullOrEmpty (this.CurrentPageName) && m_values.ContainsKey ("")) {
          if (m_values[""].ContainsKey (key)) {
            m_values[""][key].Obj = value;
            return;
          }
        }

        log.Fatal ($"Store: invalid internal data for key={key} page={this.CurrentPageName}");
        throw new InvalidOperationException ("Invalid internal data");
      }
      catch (Exception ex) {
        log.Error ($"Store: exception {ex.Message} when trying to store key={key} in page {CurrentPageName}", ex);
        throw;
      }
    }

    /// <summary>
    /// Return true if the key exists in the general part
    /// and if the corresponding value is in the good type
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool IsStored<T> (string key)
    {
      if (!m_values.ContainsKey ("") || !m_values[""].ContainsKey (key)) {
        return false;
      }

      object obj = m_values[""][key].Obj;
      return obj == null || obj is T;
    }

    /// <summary>
    /// Get a casted value from the data
    /// First try to find the value for a specific page and then in the common data
    /// </summary>
    /// <param name="key">key of the data to load</param>
    /// <returns></returns>
    public T Get<T> (string key)
    {
      object obj = null;
      bool found = false;
      if (m_values.ContainsKey (CurrentPageName)) {
        if (m_values[CurrentPageName].ContainsKey (key)) {
          obj = m_values[CurrentPageName][key].Obj;
          found = true;
        }
      }

      if (!found && CurrentPageName != "" && m_values.ContainsKey ("")) {
        if (m_values[""].ContainsKey (key)) {
          obj = m_values[""][key].Obj;
          found = true;
        }
      }

      if (!found) {
        string message = "ItemData doesn't contain " + key + " for page " + CurrentPageName;
        log.FatalFormat (message);
        throw new KeyNotFoundException (message);
      }

      if (obj != null) {
        if (!(obj is T)) {
          string message = "Tried to convert " + obj.GetType () + " into " + typeof (T);
          log.FatalFormat (message);
          throw new InvalidCastException (message);
        }
      }

      return (T)obj;
    }

    /// <summary>
    /// Textual description of the content for a specific page including common data
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      IList<string> listText = new List<string> ();

      if (m_values.ContainsKey ("")) {
        foreach (string key in m_values[""].Keys) {
          if (m_values[""][key].Logged) {
            listText.Add ("  - " + key + " = " + m_values[""][key]);
          }
        }
      }

      if (CurrentPageName != "" && m_values.ContainsKey (CurrentPageName)) {
        foreach (string key in m_values[CurrentPageName].Keys) {
          if (m_values[CurrentPageName][key].Logged) {
            listText.Add ("  - " + key + " = " + m_values[CurrentPageName][key]);
          }
        }
      }

      return string.Join ("\n", listText.ToArray ());
    }
    #endregion // Methods
  }
}
