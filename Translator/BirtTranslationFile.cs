// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Globalization;

namespace Lem_Translator
{
  /// <summary>
  /// Description of BirtTranslationFile.
  /// </summary>
  public class BirtTranslationFile: TranslationFile
  {
    #region Constructors
    public BirtTranslationFile(string path, CultureInfo cultureInfo): base (path, cultureInfo, true)
    {
    }
    
    public BirtTranslationFile(string path): base (path, new CultureInfo ("en"), true)
    {
    }
    #endregion
    
    #region Methods
    protected override char getSeparator ()
    {
      return '=';
    }
    
    protected override object decodeKey (string key)
    {
      return key;
    }
    
    protected override object decodeValue (string value)
    {
      return value.Replace ("\\:", ":");
    }
    
    protected override string encodeValue (string value)
    {
      return value.Replace (":", "\\:");
    }
    
    protected override bool isComment (string line)
    {
      return line.StartsWith ("#");
    }
    
    protected override bool isSection (string line, out string section)
    {
      section = "";
      return false;
    }
    
    protected override string sectionLine (string section)
    {
      return "#" + section;
    }
    #endregion
  }
}

