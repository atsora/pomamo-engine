// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Lemoine.Conversion
{
  /// <summary>
  /// Description of EncryptString.
  /// </summary>
  public static class EncryptString
  {
    // This constant string is used as a "salt" value for the PasswordDeriveBytes function calls.
    // This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
    // 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
    private static readonly byte[] initVectorBytes = Encoding.ASCII.GetBytes("tu89geji340t89u2");

    // This constant is used to determine the keysize of the encryption algorithm.
    private const int keysize = 256;

    /// <summary>
    /// Encrypt a string with a password
    /// </summary>
    /// <param name="plainText"></param>
    /// <param name="passPhrase"></param>
    /// <returns></returns>
    public static string Encrypt(string plainText, string passPhrase)
    {
      byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
      var password = new PasswordDeriveBytes(passPhrase, null);
      byte[] keyBytes = password.GetBytes(keysize / 8);
      using (var symmetricKey = new RijndaelManaged())
      {
        symmetricKey.Mode = CipherMode.CBC;
        using (ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes))
        {
          using (var memoryStream = new MemoryStream())
          {
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
              cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
              cryptoStream.FlushFinalBlock();
              byte[] cipherTextBytes = memoryStream.ToArray();
              return Convert.ToBase64String(cipherTextBytes);
            }
          }
        }
      }
    }

    /// <summary>
    /// Decrypt a string with a password
    /// </summary>
    /// <param name="cipherText"></param>
    /// <param name="passPhrase"></param>
    /// <returns></returns>
    public static string Decrypt(string cipherText, string passPhrase)
    {
      byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
      var password = new PasswordDeriveBytes(passPhrase, null);
      byte[] keyBytes = password.GetBytes(keysize / 8);
      using(var symmetricKey = new RijndaelManaged())
      {
        symmetricKey.Mode = CipherMode.CBC;
        using(ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes))
        {
          using(var memoryStream = new MemoryStream(cipherTextBytes))
          {
            using(var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            {
              byte[] plainTextBytes = new byte[cipherTextBytes.Length];
              int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
              return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
            }
          }
        }
      }
    }
  }
}
