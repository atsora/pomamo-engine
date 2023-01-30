// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

using Lemoine.Core.Log;

namespace Lem_Translator
{
  /// <summary>
  /// Description of TranslationFile.
  /// </summary>
  public abstract class TranslationFile
  {
    #region Members
    string a_path;
    bool ansi; // Use ANSI characters in the translation file instead of UTF-8
    CultureInfo cultureInfo;
    IComparer keyComparer;
    #endregion
    
    private static readonly ILog log = LogManager.GetLogger(typeof (TranslationFile).FullName);
    
    #region Constructors
    public TranslationFile(string path)
    {
      a_path = path;
      ansi = false;
      cultureInfo = null;
      keyComparer = null;
    }
    
    public TranslationFile ()
    {
      a_path = null;
      ansi = false;
      cultureInfo = null;
      keyComparer = null;
    }
    
    public TranslationFile(string path, CultureInfo usedCultureInfo, bool useAnsi)
    {
      a_path = path;
      ansi = useAnsi;
      cultureInfo = usedCultureInfo;
      keyComparer = null;
    }
    #endregion
    
    #region Getters / Setters / Properties
    public string FilePath { get { return a_path; } }
    
    public bool Ansi {
      get { return ansi; }
      set { ansi = value; }
    }
    
    public CultureInfo CultureInfo {
      get { return cultureInfo; }
      set { cultureInfo = value; }
    }
    
    public IComparer KeyComparer {
      get { return keyComparer; }
      set { keyComparer = value; }
    }
    #endregion
    
    #region Methods
    protected abstract char getSeparator ();
    protected abstract object decodeKey (string key);
    protected abstract object decodeValue (string value);
    protected abstract string encodeValue (string value);
    protected abstract bool isComment (string line);
    protected abstract bool isSection (string line, out string section);
    protected abstract string sectionLine (string section);
    
    /// <summary>
    /// Read the file in the given path.
    /// 
    /// Warning: the path must be given in the constructor
    /// to use this method, else null is returned.
    /// </summary>
    /// <returns></returns>
    public virtual Hashtable Read ()
    {
      Debug.Assert (a_path != null);
      if (null == a_path) {
        log.Fatal ("Trying to read a translation file " +
                   "although the path is not defined");
        return null;
      }
      
      Hashtable result = new Hashtable ();
      StreamReader reader = GetReader ();
      string line;
      while ((line = reader.ReadLine ()) != null) {
        if (isComment (line)) {
          continue;
        }
        int separatorPosition = line.IndexOf (getSeparator ());
        if (separatorPosition > 0) {
          object key = decodeKey (line.Substring (0, separatorPosition));
          object value = decodeValue (line.Substring (separatorPosition+1));
          result [key] = value;
          log.Debug ("Add key " + key + " value " + value);
        }
      }
      reader.Close ();
      return result;
    }
    
    public virtual Hashtable ReadSection (string section)
    {
      Debug.Assert (a_path != null);
      if (null == a_path) {
        log.Fatal ("Trying to read a secion in a translation file " +
                   "although the path is not defined");
        return null;
      }

      Hashtable result = new Hashtable ();
      StreamReader reader = GetReader ();
      string line;
      bool rightSection = false;
      while ((line = reader.ReadLine ()) != null) {
        if (isComment (line)) {
          continue;
        }
        string readSection;
        if (isSection (line, out readSection)) {
          if (true == section.Equals (readSection)) {
            rightSection = true;
          }
          else {
            rightSection = false;
          }
        }
        if (true == rightSection) {
          int separatorPosition = line.IndexOf (getSeparator ());
          if (separatorPosition > 0) {
            object key = decodeKey (line.Substring (0, separatorPosition));
            object value = decodeValue (line.Substring (separatorPosition+1));
            result [key] = value;
            log.Debug ("Add key " + key + " value " + value);
          }
        }
      }
      reader.Close ();
      return result;
    }
    
    public virtual void Save (Hashtable hashtable)
    {
      Debug.Assert (a_path != null);
      if (null == a_path) {
        log.Fatal ("Trying to save a translation file " +
                   "although the path is not defined");
        return;
      }

      StreamWriter writer = GetWriter ();
      SaveSection (writer, "", hashtable);
      writer.Close ();
      return;
    }
    
    public StreamReader GetReader ()
    {
      Debug.Assert (a_path != null);
      if (null == a_path) {
        log.Fatal ("Trying to get the writer of a translation file " +
                   "although the path is not defined");
        return null;
      }

      if (true == ansi) {
        if (cultureInfo == null) {
          log.Error ("Try to use ANSI encoding, but the cultureInfo is unknown " +
                     "=> use default encoding (UTF-8) instead");
          return new StreamReader (a_path);
        }
        else {
          int codePage = CultureInfo.TextInfo.ANSICodePage;
          Encoding encoding;
          try {
            encoding = codePage.Equals (0) ?
              Encoding.UTF8 :
              Encoding.GetEncoding (codePage);
          }
          catch (Exception) {
            encoding = Encoding.UTF8;
          }
          log.InfoFormat ("Use the following encoding {0} for {1}, " +
                          "culture is {2} and code page is {3}",
                          encoding, a_path, cultureInfo, codePage);
          return new StreamReader (a_path, encoding);
        }
      }
      else {
        // Use default encoding
        return new StreamReader (a_path);
      }
    }

    public StreamWriter GetWriter ()
    {
      Debug.Assert (a_path != null);
      if (null == a_path) {
        log.Fatal ("Trying to get the writer of a translation file " +
                   "although the path is not defined");
        return null;
      }

      if (true == ansi) {
        if (cultureInfo == null) {
          log.Error ("Try to use ANSI encoding, but the cultureInfo is unknown " +
                     "=> use default encoding (UTF-8) instead");
          return new StreamWriter (a_path);
        }
        else {
          int codePage = CultureInfo.TextInfo.ANSICodePage;
          Encoding encoding = codePage.Equals(0)?
            Encoding.UTF8:
            Encoding.GetEncoding(codePage);
          log.InfoFormat ("Use the following encoding {0} for {1}, " +
                          "culture is {2} and code page is {3}",
                          encoding, a_path, cultureInfo, codePage);
          return new StreamWriter (a_path, false, encoding);
        }
      }
      else {
        // Use default encoding (UTF-8)
        return new StreamWriter (a_path);
      }
    }
    
    public virtual void SaveSection (StreamWriter writer,
                                     string section,
                                     Hashtable hashtable)
    {
      Debug.Assert (a_path != null);
      if (null == a_path) {
        log.Fatal ("Trying to save a section in a translation file " +
                   "although the path is not defined");
        return;
      }

      if (! section.Equals ("")) {
        writer.WriteLine (sectionLine (section));
      }
      object [] keys = new object [hashtable.Count];
      hashtable.Keys.CopyTo (keys, 0);
      Array.Sort (keys, keyComparer);
      foreach (object key in keys) {
        Debug.Assert (hashtable [key] != null);
        writer.WriteLine (key.ToString () + getSeparator () + hashtable [key].ToString ());
      }
      writer.WriteLine ("");
      return;
    }
    #endregion
  }
}
