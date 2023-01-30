// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Description of XmlReaderDialog.
  /// </summary>
  public partial class XmlReaderDialog : Form
  {
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="fileTitle">Title of the file being read</param>
    /// <param name="content">Xml content of the dialog</param>
    public XmlReaderDialog(string fileTitle, string content)
    {
      InitializeComponent();
      if (Form.ActiveForm != null) {
        this.Icon = Form.ActiveForm.Icon;
      }

      this.Text = fileTitle;
      this.StartPosition = FormStartPosition.CenterParent;
      
      // Convenient refactoring of the text
      try {
        richTextBox.Text = System.Xml.Linq.XDocument.Parse(content).ToString()
          .Replace("\r\n", "\n")
          .Replace("\r", "\n")
          .Replace("&lt;=", "≤")
          .Replace("&gt;=", "≥")
          .Replace("&lt;", "<")
          .Replace("&gt;", ">");
      } catch (Exception e) {
        richTextBox.Text = "Couldn't read file " + fileTitle + ":\n\n" + e;
      }
      
      // Default coloration for xml text
      ColorXmlText();
    }
    #endregion // Constructors

    #region Methods
    void ColorXmlText()
    {
      // Attributes in red
      ColorTextBetween("<", ">", Color.Red, false);
      
      // Token in dark blue
      ColorTextUntilChar("<", new [] {">", " "}, Color.DarkBlue);
      ColorString(">", Color.DarkBlue);
      
      // Text in green
      ColorTextBetween("\"", "\"", Color.Green, true);
      ColorTextBetween("<!--", "-->", Color.Green, true);
      
      // Text in orange
      ColorTextBetween("<![CDATA[", "]]>", Color.Orange, true);
      
      // Metadata highlighted
      HighlightTextBetween("<description>", "</description>", Color.BlanchedAlmond, true);
      HighlightTextBetween("<title>", "</title>", Color.BlanchedAlmond, true);
      HighlightTextBetween("<eventtype>", "</eventtype>", Color.BlanchedAlmond, true);
      HighlightTextBetween("<actiontype>", "</actiontype>", Color.BlanchedAlmond, true);
      HighlightTextBetween("<advanced>", "</advanced>", Color.BlanchedAlmond, true);
    }
    
    /// <summary>
    /// Color the text between two strings, including them or not
    /// </summary>
    /// <param name="startStr">beginning of the text to color</param>
    /// <param name="endStr">end of the text to color</param>
    /// <param name="color">color</param>
    /// <param name="includeText">true if startStr and endStr must be colored</param>
    public void ColorTextBetween(string startStr, string endStr, Color color, bool includeText)
    {
      int pos = 0;
      int startPos = 0;
      int endPos = 0;
      while ((startPos = richTextBox.Text.IndexOf(startStr, pos)) != -1) {
        pos = startPos + 1;
        endPos = richTextBox.Text.IndexOf(endStr, pos);
        
        if (endPos != -1) {
          pos = endPos + 1;
          richTextBox.Select(
            startPos + (includeText ? 0 : startStr.Length),
            endPos - startPos + (includeText ? endStr.Length : 0));
          richTextBox.SelectionColor = color;
        }
      }
      
      // Remove selection
      richTextBox.Select(0, 0);
    }
    
    /// <summary>
    /// Highlight the text between two strings, including them or not
    /// </summary>
    /// <param name="startStr">beginning of the text to color</param>
    /// <param name="endStr">end of the text to color</param>
    /// <param name="color">color</param>
    /// <param name="includeText">true if startStr and endStr must be colored</param>
    public void HighlightTextBetween(string startStr, string endStr, Color color, bool includeText)
    {
      int pos = 0;
      int startPos = 0;
      int endPos = 0;
      var font = new Font(richTextBox.Font, FontStyle.Bold);
      while ((startPos = richTextBox.Text.IndexOf(startStr, pos)) != -1) {
        pos = startPos + 1;
        endPos = richTextBox.Text.IndexOf(endStr, pos);
        
        if (endPos != -1) {
          pos = endPos + 1;
          richTextBox.Select(
            startPos + (includeText ? 0 : startStr.Length),
            endPos - startPos + (includeText ? endStr.Length : 0));
          richTextBox.SelectionBackColor = color;
          richTextBox.SelectionFont = font;
        }
      }
      
      // Remove selection
      richTextBox.Select(0, 0);
    }
    
    /// <summary>
    /// Color the text beginning at a specified string and
    /// ending at one of several specified strings
    /// </summary>
    /// <param name="startStrIncluded"></param>
    /// <param name="endStrExcluded"></param>
    /// <param name="color"></param>
    public void ColorTextUntilChar(string startStrIncluded, string [] endStrExcluded, Color color)
    {
      int pos = 0;
      int startPos = 0;
      int endPos = 0;
      while ((startPos = richTextBox.Text.IndexOf(startStrIncluded, pos)) != -1) {
        pos = startPos + 1;
        endPos = -1;
        foreach (var endStr in endStrExcluded) {
          int tmp = richTextBox.Text.IndexOf(endStr, pos);
          if (tmp != -1 && (endPos == -1 || tmp < endPos)) {
            endPos = tmp;
          }
        }
        
        if (endPos != -1) {
          pos = endPos + 1;
          richTextBox.Select(startPos, endPos - startPos + 1);
          richTextBox.SelectionColor = color;
        }
      }
      
      // Remove selection
      richTextBox.Select(0, 0);
    }
    
    /// <summary>
    /// Color a specified string
    /// </summary>
    /// <param name="coloredString"></param>
    /// <param name="color"></param>
    public void ColorString(string coloredString, Color color)
    {
      int pos = 0;
      int startPos = 0;
      while ((startPos = richTextBox.Text.IndexOf(coloredString, pos)) != -1) {
        pos = startPos + 1;
        
        richTextBox.Select(startPos, 1);
        richTextBox.SelectionColor = color;
      }
      
      // Remove selection
      richTextBox.Select(0, 0);
    }
    #endregion // Methods
  }
}
