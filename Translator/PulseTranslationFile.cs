// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using Lemoine.Core.Log;

namespace Lem_Translator
{
  /// <summary>
  /// Description of PulseTranslationFile.
  /// </summary>
  public class PulseTranslationFile: TranslationFile
  {
    private static readonly ILog log = LogManager.GetLogger(typeof (TranslationFile).FullName);
    
    private class IntegerOrStringComparer: IComparer
    {
      int IComparer.Compare (object x, object y) {
        if (x is Int32) {
          if (y is Int32) {
            return ((Int32) x).CompareTo (y);
          }
          else {
            return 1;
          }
        }
        else if (x is string) {
          if (y is string) {
            return ((string) x).CompareTo (y);
          }
          else {
            return -1;
          }
        }
        else {
          throw new ArgumentException("object is not an IntegerOrString");
        }
      }
    }
    
    #region Constructors
    public PulseTranslationFile(string path, CultureInfo cultureInfo): base (path, cultureInfo, true)
    {
      KeyComparer = new IntegerOrStringComparer ();
    }
    
    public PulseTranslationFile(string path): base (path, new CultureInfo ("en"), true)
    {
      KeyComparer = new IntegerOrStringComparer ();
    }
    #endregion
    
    #region Methods
    protected override char getSeparator ()
    {
      return ':';
    }
    
    protected override object decodeKey (string key)
    {
      try {
        return Int32.Parse (key);
      } catch (Exception) {
        return key;
      }
    }
    
    protected override object decodeValue (string value)
    {
      // TODO
      return value;
    }
    
    protected override string encodeValue (string value)
    {
      // TODO
      return value;
    }
    
    protected override bool isComment (string line)
    {
      return line.StartsWith ("#");
    }
    
    protected override bool isSection (string line, out string section)
    {
      if (line.StartsWith ("*")) {
        section = line.Substring (1);
        section = section.Trim ();
        return true;
      }
      
      section = null;
      return false;
    }
    
    protected override string sectionLine (string section)
    {
      return "*" + section;
    }
    
    public override void SaveSection (StreamWriter writer,
                                      string section,
                                      Hashtable hashtable)
    {
      Debug.Assert (FilePath != null);
      if (null == FilePath) {
        log.Fatal ("Trying to save a section in a translation file " +
                   "although the path is not defined");
        return;
      }

      if (! section.Equals ("")) {
        writer.WriteLine (sectionLine (section));
      }
      object [] keys = new object [hashtable.Count];
      hashtable.Keys.CopyTo (keys, 0);
      Array.Sort (keys, KeyComparer);
      foreach (object key in keys) {
        Debug.Assert (hashtable [key] != null);
        string keyString = key.ToString ();
        try    {
          Convert.ToInt32(keyString);
          // Integer: fill with 0 on the left
          keyString = keyString.PadLeft (4, '0');
        }
        catch (FormatException)    {
        }
        writer.WriteLine (keyString + getSeparator () + hashtable [key].ToString ());
      }
      writer.WriteLine ("");
      return;
    }
    
    #endregion
  }
}
