// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Lemoine.Conversion;

namespace Lemoine.Info
{
  /// <summary>
  /// Description of IniFile.
  /// </summary>
  public class IniFile
  {
    /// <summary>
    /// Path of the ini file
    /// </summary>
    protected string FilePath { get; set; }

    [DllImport("kernel32")]
    static extern long WritePrivateProfileString(string section, string key,
                                                 string val, string filePath);
    
    [DllImport("kernel32.dll")]
    static extern int GetPrivateProfileString(string section, string key, string def,
                                              StringBuilder retVal, int size, string filePath);
    
    [DllImport("kernel32",
               EntryPoint = "GetPrivateProfileStringW",
               SetLastError=true,
               CharSet=CharSet.Unicode, ExactSpelling=true,
               CallingConvention=CallingConvention.StdCall)]
    static extern int GetPrivateProfileString2(string section, string key, string def,
                                               string retVal, int size, string filePath);
    
    [DllImport("kernel32.dll")]
    static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer,
                                               int nSize, string lpFileName);

    /// <summary>
    /// INIFile Constructor
    /// </summary>
    /// <param name="INIFileName">name of the ini file, or fullpath</param>
    /// <param name="fullPath">true if the full path of the ini file is specified (may not have a write access)
    /// or false for a generated ini file in a directory with rw access</param>
    public IniFile(string INIFileName, bool fullPath)
    {
      if (fullPath) {
        FilePath = INIFileName;
      }
      else {
        // Create the "ini_files" directory if it doesn't already exist
        string path = Path.Combine(PulseInfo.LocalConfigurationDirectory, "ini_files");
        if (!Directory.Exists(path)) {
          Directory.CreateDirectory(path);
        }

        FilePath = Path.Combine(path, INIFileName + ".ini");
      }
    }
    
    /// <summary>
    /// Get all sections
    /// </summary>
    /// <returns></returns>
    public ICollection<string> GetSections()
    {
      string returnString = new string(' ', 32768);
      GetPrivateProfileString2(null, null, null, returnString, 32768, FilePath);
      return returnString.Trim('\0').Split(new char[] {'\0'}, StringSplitOptions.RemoveEmptyEntries);
    }
    
    /// <summary>
    /// Get all keys from a section
    /// </summary>
    /// <param name="section"></param>
    /// <returns></returns>
    public ICollection<string> GetKeys(string section)
    {
      var buffer = new byte[32768];

      GetPrivateProfileSection(section, buffer, 32768, FilePath);
      String[] tmp = Encoding.ASCII.GetString(buffer).Trim('\0')
        .Split(new char[] {'\0'}, StringSplitOptions.RemoveEmptyEntries);

      IList<string> result = new List<string>();
      foreach (String entry in tmp) {
        result.Add(entry.Substring(0, entry.IndexOf("=")));
      }

      return result;
    }
    
    /// <summary>
    /// Read data from the INI file
    /// </summary>
    /// <param name="section">section name</param>
    /// <param name="key">key name</param>
    /// <returns>value to retrieve</returns>
    public string GetValue(string section, string key)
    {
      var returnString = new StringBuilder(255);
      int i = GetPrivateProfileString(section, key, "", returnString, 32768, FilePath);
      return returnString.ToString();
    }
    
    /// <summary>
    /// Write data to the INI file
    /// </summary>
    /// <param name="section">section name</param>
    /// <param name="key">key name</param>
    /// <param name="value">value to store</param>
    public void SetValue(string section, string key, string value)
    {
      WritePrivateProfileString(section, key, value, FilePath);
    }
    
    /// <summary>
    /// Read data from the INI file
    /// </summary>
    /// <param name="section">section name</param>
    /// <param name="key">key name</param>
    /// <param name="code">code for the decryption</param>
    /// <returns></returns>
    public string GetAndDecryptValue(string section, string key, string code)
    {
      string value = GetValue(section, key);
      if (value != "") {
        value = EncryptString.Decrypt(value, code);
      }

      return value;
    }
    
    /// <summary>
    /// Write data to the INI file
    /// </summary>
    /// <param name="section">section name</param>
    /// <param name="key">key name</param>
    /// <param name="code">code for the encryption</param>
    /// <param name="value">value to store</param>
    public void EncryptAndSetValue(string section, string key, string code, string value)
    {
      if (value != "") {
        value = EncryptString.Encrypt(value, code);
      }

      SetValue (section, key, value);
    }
    
    /// <summary>
    /// Return true if a section / key is defined
    /// </summary>
    /// <param name="section"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool IsSet(string section, string key)
    {
      return GetSections().Contains(section) &&
        GetKeys(section).Contains(key);
    }
  }
}
