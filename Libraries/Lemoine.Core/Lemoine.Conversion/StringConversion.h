// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#pragma once

#include <string>
#include <vcclr.h>

namespace Lemoine
{
  namespace Conversion
  {
    /// <summary>
    /// Convert a managed string to a std::string
    /// </summary>
    /// <param name="managedString"></param>
    /// <returns></returns>
    inline std::string ConvertToStdString (String ^managedString)
    {
      if (managedString == nullptr) {
        return std::string ("");
      }
      // Convert a Unicode string to an ASCII string
      pin_ptr<const wchar_t> wstr1 = PtrToStringChars (managedString);
      std::wstring wstr (wstr1);
      std::string strTo;
      char *szTo = new char[wstr.length() + 1];
      szTo[wstr.size()] = '\0';
      WideCharToMultiByte(CP_ACP, 0, wstr.c_str(), -1, szTo, (int)wstr.length(), NULL, NULL);
      strTo = szTo;
      delete[] szTo;
      return strTo;
    }

    /// <summary>
    /// Convert a const char* string into a managed string String^
    /// </summary>
    /// <param name="origStr"></param>
    /// <returns></returns>
    inline String^ ConvertToManagedString (const char* origStr)
    {
      // Convert an ASCII string to a Unicode String
      std::string str (origStr);
      wchar_t *wszTo = new wchar_t[str.length() + 1];
      wszTo[str.size()] = L'\0';
      MultiByteToWideChar(CP_ACP, 0, str.c_str(), -1, wszTo, (int)str.length());
      String^ wstrTo = gcnew String (wszTo);
      delete[] wszTo;
      return wstrTo;
    }
  }
}
