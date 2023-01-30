// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;

using Lemoine.Model;

namespace Lem_MachineStateTemplateGUI
{
  /// <summary>
  /// RichTextBoxAppender.
  /// </summary>
  internal class RichTextBoxAppender {
      int m_currentPosition = 0;
      RichTextBox m_richTextBox;
      
      /// <summary>
      /// Return current cursor position
      /// </summary>
      public int Position {
        get { return m_currentPosition; }
      }
      
      public RichTextBoxAppender(RichTextBox richTextBox) {
        m_richTextBox = richTextBox;
      }
      
      /// <summary>
      /// Replace str from given position
      /// </summary>
      /// <param name="str"></param>
      /// <param name="posStart"></param>
      public void ReplaceAtEnd(string str, int posStart) {
        m_richTextBox.Select(posStart, m_richTextBox.Text.Length - posStart + 1);
        m_richTextBox.Cut();
        m_richTextBox.AppendText(str);
      }
      
      /// <summary>
      /// Append text inot TextBox.
      /// </summary>
      /// <param name="str"></param>
      /// <param name="fnt"></param>
      public void AddText(string str, Font fnt) {
        // str must not contain escaping literals
        // (only System.Environment.NewLine alone in str is tolerated)
        int nbChars;
        
        if (str == System.Environment.NewLine) nbChars = 1;
        else nbChars = str.Length;
        
        m_richTextBox.AppendText(str);
        m_richTextBox.Select(m_currentPosition, nbChars);
        m_richTextBox.SelectionFont = fnt;
        m_richTextBox.DeselectAll();
        m_currentPosition += nbChars;
      }
      
      /// <summary>
      /// Used to do a report about MachineStateTemplateAssociation
      /// </summary>
      /// <param name="machineStateTemplateAssociation"></param>
      public void AddMachineMachineStateTemplateAssociation(IMachineStateTemplateAssociation machineStateTemplateAssociation)
      {
        Font currentFont = m_richTextBox.Font;
        
        Font boldFont = new System.Drawing.Font(currentFont.FontFamily,
                                                currentFont.Size,
                                                FontStyle.Bold);
        
        if (machineStateTemplateAssociation.MachineStateTemplate != null) {
          this.AddText("MachineStateTemplate", currentFont);
          
          this.AddText(machineStateTemplateAssociation.MachineStateTemplate.Display, boldFont);
        } else {
          this.AddText("No MachineStateTemplate", currentFont);
        }
        
        this.AddText(" on ", currentFont);
        this.AddText("machine " + machineStateTemplateAssociation.Machine.Name,
                         boldFont);
        this.AddText(": begin ", currentFont);
        this.AddText(String.Format("{0}", machineStateTemplateAssociation.Begin.HasValue
                                   ? machineStateTemplateAssociation.Begin.Value.ToLocalTime().ToString ()
                                   : "-oo"), boldFont);
        this.AddText(" end ", currentFont);
        
        if (machineStateTemplateAssociation.End.HasValue)
          this.AddText(String.Format("{0}", machineStateTemplateAssociation.End.Value.ToLocalTime()), boldFont);
        else
          this.AddText("oo", boldFont);
        
        if(machineStateTemplateAssociation.Shift != null){
          this.AddText(" during ", currentFont);
          this.AddText(String.Format("{0}",machineStateTemplateAssociation.Shift.Display),boldFont);
        }
        
        this.AddText(System.Environment.NewLine, currentFont);
      }
      
    }
}
