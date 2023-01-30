// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Settings
{
  /// <summary>
  /// Description of KeywordSearch.
  /// </summary>
  public static class KeywordSearch
  {
    // All separators to split a string into words
    static char[] SEPARATORS = new char[] {',', ' ', '\n', '\t', ';', '-', '.', '?', '!', '/',
      '\"', '\\', '(', ')', '[', ']', '{', '}', '\'', ':' };
    
    #region Methods
    /// <summary>
    /// Split a text into words
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static ICollection<string> SplitIntoWords(string text)
    {
      return text.Split(SEPARATORS, StringSplitOptions.RemoveEmptyEntries);
    }
    
    /// <summary>
    /// Format all keywords in lower cases
    /// Exclude all keywords whose length is 1
    /// Exclude common english words
    /// Remove duplicates
    /// </summary>
    /// <param name="keywords"></param>
    /// <returns></returns>
    public static ICollection<string> FormatKeywords(ICollection<string> keywords)
    {
      ICollection<string> formattedKeywords = new List<string>();
      
      foreach (string baseKeyword in keywords) {
        int length = baseKeyword.Length;
        string keyword = baseKeyword.ToLower();
        if (!formattedKeywords.Contains(keyword)) {
          if (
            (
              length > 4
             ) || (
              length == 4 &&
              keyword != "does" && keyword != "have" && keyword != "that" && keyword != "what" &&
              keyword != "this" && keyword != "used" && keyword != "more" && keyword != "less" &&
              keyword != "here" && keyword != "will" && keyword != "each" && keyword != "must" &&
              keyword != "into" && keyword != "same" && keyword != "them" && keyword != "with"
             ) || (
              length == 3 &&
              keyword != "and" && keyword != "any" && keyword != "but" && keyword != "for" &&
              keyword != "has" && keyword != "how" && keyword != "not" && keyword != "one" &&
              keyword != "the" && keyword != "get" && keyword != "set" && keyword != "who" &&
              keyword != "why" && keyword != "you" && keyword != "its" && keyword != "our" &&
              keyword != "can" && keyword != "add" && keyword != "may" && keyword != "use" &&
              keyword != "yet" && keyword != "all"
             ) || (
              length == 2 &&
              keyword != "an" && keyword != "as" && keyword != "at" && keyword != "do" &&
              keyword != "if" && keyword != "in" && keyword != "is" && keyword != "it" &&
              keyword != "no" && keyword != "of" && keyword != "on" && keyword != "or" &&
              keyword != "so" && keyword != "to" && keyword != "us" && keyword != "we" &&
              keyword != "by" && keyword != "be" && keyword != "go" && keyword != "me" &&
              keyword != "up"
             )) {
            formattedKeywords.Add(keyword);
          }
        }
      }
      
      return formattedKeywords;
    }
    
    /// <summary>
    /// Get the score representing how much a keyword list matches with a list of words
    /// 0 means no matching
    /// </summary>
    /// <param name="keywords">keywords in which the words are searched</param>
    /// <param name="words">at least one element, no empty elements, only lower case</param>
    /// <returns></returns>
    public static double GetScore(IDictionary<string, int> keywords, ICollection<string> words)
    {
      double score = 0;
      foreach (string word in words) {
        score += GetScore(keywords, word);
      }

      return score;
    }
    
    static double GetScore(IDictionary<string, int> keywords, string word)
    {
      double score = 0;
      foreach (var element in keywords) {
        double scoreTmp = (double)element.Value * Match(element.Key, word);
        if (scoreTmp > score) {
          score = scoreTmp;
        }
      }
      return score;
    }
    
    static double Match(string keyword, string word)
    {
      int keywordLength = keyword.Length;
      int wordLength = word.Length;

      // Location of the word within the keyword
      int index = keyword.IndexOf(word, StringComparison.CurrentCulture);
      if (index == -1) {
        return 0;
      }

      // Compute the score, based on the number of letter compared to the total number of letters
      // 50% or above gives us the value of 1.0
      // Below, this is proportional
      double score = Math.Min(2.0 * wordLength / keywordLength, 1.0);
      
      // The score is multiplied by 3 if the word is the beginning of the keyword      
      return ((index == 0) ? 3.0 : 1.0) * score;
    }
    #endregion // Methods
  }
}
