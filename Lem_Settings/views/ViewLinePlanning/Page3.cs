// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ViewLinePlanning
{
  /// <summary>
  /// Description of Page3.
  /// </summary>
  public partial class Page3 : GenericViewPage, IViewPage
  {
    #region Members
    Brush m_brush = null;
    LineBarData m_barData = new LineBarData();
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Planning of a line"; } }
    
    /// <summary>
    /// Some help to fill the page
    /// </summary>
    public string Help { get { return "All productions planned for a line are displayed in the timeline. " +
          "The period of production are represented by colored segments. Each target is an ellipse.\n" +
          "Buttons just below the timeline allow you to change the period.\n\n" +
          "The productions are also listed in the left list. Details can be displayed by clicking on it. " +
          "A tree will show the different quantities to produce per shift and per day."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page3()
    {
      InitializeComponent();
      pictureBox.Paint += PaintPictureBox;
      timelinesWidget.AddTimeline("", m_barData);
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize(ItemContext context) {}
    
    /// <summary>
    /// Load the page
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      DistinctBrushes brushes = data.Get<DistinctBrushes>(Item.BRUSHES);
      ILine line = data.Get<ILine>(Item.LINE);
      m_brush = brushes.GetBrush(line.Id);
      
      treeQuantities.Nodes.Clear();
      using (new SuspendDrawing(verticalScroll))
      {
        verticalScroll.Clear();
        
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            // Name of the line
            ModelDAOHelper.DAOFactory.LineDAO.Lock(line);
            labelLine.Text = line.Display;
            
            // Parts produced
            IList<string> partNames = new List<string>();
            foreach (Lemoine.Model.IComponent component in line.Components) {
              partNames.Add(component.Display);
            }

            labelPart.Text = "Part" + (line.Components.Count > 1 ? "s" : "") + " produced: \"" +
              String.Join("\", \"", partNames.ToArray()) + "\"";
            
            // List of plannings
            IList<IWorkOrderLine> wols = ModelDAOHelper.DAOFactory.WorkOrderLineDAO.FindAllByLine(line);
            foreach (IWorkOrderLine wol in wols) {
              DateTime startWol = wol.BeginDateTime.Value.ToLocalTime();
              DateTime endWol = wol.Deadline.ToLocalTime();
              var cell = new ClickableCell(this);
              cell.DisplayMode = ClickableCell.Mode.SingleTextRight;
              cell.AutoLineBreak = false;
              cell.ImageSize = new Size(0, 0);
              cell.Height = 60;
              cell.Text =
                "Production \"" + wol.WorkOrder.Display + "\"\n" +
                "Start " + startWol.ToShortDateString() + ", " + startWol.ToShortTimeString() + "\n" +
                "Deadline " + endWol.ToShortDateString() + ", " + endWol.ToShortTimeString() + "\n" +
                "Target " + wol.Quantity + " parts";
              cell.Dock = DockStyle.Fill;
              cell.Margin = new Padding(3, 3, 3, 0);
              cell.HoverColor = LemSettingsGlobal.COLOR_ITEM_HOVER;
              cell.Tag = wol;
              cell.MouseClick += CellClicked;
              verticalScroll.AddControl(cell);
            }
          }
        }
      }
      
      m_barData.Line = line;
      timelinesWidget.SetPeriod(data.Get<DateTime>(Item.TIMELINE_START), data.Get<DateTime>(Item.TIMELINE_END));
      timelinesWidget.Draw();
    }
    
    /// <summary>
    /// Save the parameters
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.TIMELINE_START, timelinesWidget.StartDateTime);
      data.Store(Item.TIMELINE_END, timelinesWidget.EndDateTime);
    }
    #endregion // Page methods
    
    #region Event reactions
    void PaintPictureBox(object sender, PaintEventArgs e)
    {
      if (m_brush != null) {
        e.Graphics.FillRectangle(m_brush, base.ClientRectangle);
      }
    }
    
    void CellClicked(object sender, EventArgs e)
    {
      treeQuantities.Nodes.Clear();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        // WorkOrderLine
        IWorkOrderLine wol = (IWorkOrderLine)((sender as Control).Tag);
        
        // Intermediate work piece summaries
        IList<IIntermediateWorkPieceTarget> iwpts = ModelDAOHelper.DAOFactory
          .IntermediateWorkPieceTargetDAO.FindByWorkOrderLine(wol.WorkOrder, wol.Line);
        
        IDictionary<DateTime, IDictionary<int, IList<string>>> qttByDayShift =
          new Dictionary<DateTime, IDictionary<int, IList<string>>>();
        int nbElement = 0;
        foreach (IIntermediateWorkPieceTarget iwpt in iwpts)
        {
          if (iwpt.Day.HasValue && iwpt.Number > 0)
          {
            // Date of shift
            DateTime iwpsDay = iwpt.Day.Value;
            if (!qttByDayShift.ContainsKey(iwpsDay)) {
              qttByDayShift[iwpsDay] = new Dictionary<int, IList<string>>();
            }

            // Shift index
            int shiftId = -1;
            if (iwpt.Shift != null) {
              shiftId = iwpt.Shift.Id;
            }

            if (!qttByDayShift[iwpsDay].ContainsKey(shiftId)) {
              qttByDayShift[iwpsDay][shiftId] = new List<string>();
            }

            string plural = iwpt.Number > 1 ? "s" : "";
            qttByDayShift[iwpsDay][shiftId].Add(iwpt.Number + " part" + plural + " for " +
                                                iwpt.IntermediateWorkPiece.Display);
            nbElement++;
          }
        }
        
        if (nbElement == 0) {
          treeQuantities.Nodes.Add("No productions");
        } else {
          foreach (DateTime shiftDate in qttByDayShift.Keys) {
            TreeNode rootNode = treeQuantities.Nodes.Add(shiftDate.ToShortDateString());
            if (qttByDayShift[shiftDate].Keys.Count == 1 && qttByDayShift[shiftDate].Keys.First() == -1)
            {
              // No shifts
              IList<string> listTmp = qttByDayShift[shiftDate][-1];
              foreach (string str in listTmp) {
                rootNode.Nodes.Add(str);
              }
            } else {
              foreach (int shiftId in qttByDayShift[shiftDate].Keys) {
                TreeNode node = rootNode.Nodes.Add(
                  shiftId == -1 ? "Undefined" :
                  ModelDAOHelper.DAOFactory.ShiftDAO.FindById(shiftId).Display);
                foreach (string str in qttByDayShift[shiftDate][shiftId]) {
                  node.Nodes.Add(str);
                }
              }
            }
            rootNode.Expand();
          }
        }
      }
    }
    #endregion // Event reactions
  }
}
