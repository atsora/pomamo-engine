// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Lem_Translator
{
  public class DotNetTranslationFile : TranslationFile
  {
    public DotNetTranslationFile (string path)
      : base (path)
    { }

    public DotNetTranslationFile (string path, CultureInfo cultureInfo)
      : base (path, cultureInfo, true)
    {
    }

    protected override object decodeKey (string key)
    {
      return key;
    }

    protected override object decodeValue (string value)
    {
      return value;
    }

    protected override string encodeValue (string value)
    {
      return value;
    }

    protected override char getSeparator ()
    {
      return '=';
    }

    protected override bool isComment (string line)
    {
      return line.StartsWith ("#");
    }

    protected override bool isSection (string line, out string section)
    {
      section = null;
      return false;
    }

    protected override string sectionLine (string section)
    {
      return $"No section {section}";
    }
  }
}
