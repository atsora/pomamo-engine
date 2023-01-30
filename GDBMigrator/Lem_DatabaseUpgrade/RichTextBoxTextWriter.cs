// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Text;

namespace Lem_DatabaseUpgrade
{
  class RichTextBoxTextWriter : TextWriter
  {
    RichTextBox m_richTextBox;
    Color m_defaultColor;

    public bool AutoScroll { get; set; }

    public RichTextBoxTextWriter (RichTextBox richTextBox, Color defaultColor)
    {
      m_richTextBox = richTextBox;
      m_defaultColor = defaultColor;
    }

    public RichTextBoxTextWriter (RichTextBox richTextBox)
      : this (richTextBox, Color.Black)
    {
    }

    public override void Write (char value)
    {
      m_richTextBox.Invoke ((Action)(() => AppendText (value.ToString (), m_defaultColor)));
    }

    public override void Write (char[] buffer, int index, int count)
    {
      string text = new string (buffer, index, count);
      text = text.Replace ("\r\n\r\n", "\r\n");
      m_richTextBox.Invoke ((Action)(() => AppendText (text, m_defaultColor)));
    }

    void AppendText (string text, Color color)
    {
      if (string.IsNullOrEmpty (text)) {
        return;
      }

      if (text.Contains ("(E) ")) {
        int position = text.IndexOf ("(E) ");
        if (1 <= position) {
          AppendText (text.Substring (0, position - 1), color);
        }
        AppendTextNoFilter ("(E) ", Color.Red);
        AppendText (text.Substring (position + "(E) ".Length), color);
        return;
      }
      if (text.Contains ("(W) ")) {
        int position = text.IndexOf ("(W) ");
        if (1 <= position) {
          AppendText (text.Substring (0, position - 1), color);
        }
        AppendTextNoFilter ("(W) ", Color.Orange);
        AppendText (text.Substring (position + "(W) ".Length), color);
        return;
      }

      AppendTextNoFilter (text, color);
    }

    void AppendTextNoFilter (string text, Color color)
    {
      m_richTextBox.SelectionStart = m_richTextBox.TextLength;
      m_richTextBox.SelectionLength = 0;

      m_richTextBox.SelectionColor = color;
      m_richTextBox.AppendText (text);
      m_richTextBox.SelectionColor = m_richTextBox.ForeColor;

      if (AutoScroll) {
        m_richTextBox.ScrollToCaret ();
      }
    }

    public override Encoding Encoding
    {
      get
      {
        return System.Text.Encoding.UTF8;
      }
    }
  }
}
