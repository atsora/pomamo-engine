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
  /// Description of Page2.
  /// </summary>
  public partial class Page2 : GenericViewPage, IViewPage
  {
    #region Members
    ILine m_lineDoubleClicked;
    DateTime m_currentDate;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Planning of a day"; } }
    
    /// <summary>
    /// Some help to fill the page
    /// </summary>
    public string Help { get { return "The list of all lines planned for a particular day is displayed to the left part.\n\n" +
          "By clicking on a line, the different quantities to produce per shift - within a day - is detailed in a tree.\n\n" +
          "Moving to the previous or the next day is possible with the left and right arrows."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page2()
    {
      InitializeComponent();
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
      m_currentDate = data.Get<DateTime>(Item.DATE);
      DistinctBrushes brushes = data.Get<DistinctBrushes>(Item.BRUSHES);
      
      treeQuantities.Nodes.Clear();
      using (new SuspendDrawing(verticalScrollLines))
      {
        verticalScrollLines.Clear();
        
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            
            // Definition of the period of time corresponding to the day
            DateTime startDate = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetDayBeginUtcDateTime(m_currentDate).ToLocalTime();
            DateTime endDate = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetDayEndUtcDateTime(m_currentDate).ToLocalTime();
            
            // Update labels
            labelDayStart.Text = startDate.ToLongDateString() + ", " + startDate.ToShortTimeString();
            labelDayEnd.Text = endDate.ToLongDateString() + ", " + endDate.ToShortTimeString();
            
            // WorkOrderLine list associated with their planned days / shifts
            IDictionary<IWorkOrderLine, IList<IIntermediateWorkPieceTarget>> wolsExtended =
              Item.GetExtendedWols(startDate, endDate);
            
            // Fill the list
            foreach (IWorkOrderLine wol in wolsExtended.Keys) {
              DateTime startWol = wol.BeginDateTime.Value.ToLocalTime();
              DateTime endWol = wol.Deadline.ToLocalTime();
              var cell = new ClickableCell(this);
              cell.DisplayMode = ClickableCell.Mode.SingleTextRight;
              cell.AutoLineBreak = false;
              cell.ImageBorderStyle = BorderStyle.Fixed3D;
              cell.Height = 60;
              cell.Text = "Line \"" + wol.Line.Display + "\"\n" +
                "Production \"" + wol.WorkOrder.Display + "\"\n" +
                "Start " + startWol.ToShortDateString() + ", " + startWol.ToShortTimeString() + "\n" +
                "Deadline " + endWol.ToShortDateString() + ", " + endWol.ToShortTimeString();
              cell.ImageMargin = new Padding(2, 2, 2, 2);
              cell.ImageBackgroundBrush = brushes.GetBrush(wol.Line.Id);
              cell.ImageSize = new Size(26, 26);
              cell.Dock = DockStyle.Fill;
              cell.Margin = new Padding(3, 3, 3, 0);
              cell.HoverColor = LemSettingsGlobal.COLOR_ITEM_HOVER;
              cell.Tag = wol;
              cell.MouseClick += CellClicked;
              cell.MouseDoubleClick += CellDoubleClicked;
              
              verticalScrollLines.AddControl(cell);
            }
          }
        }
      }
    }
    
    /// <summary>
    /// Save the parameters
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.DATE, m_currentDate);
      data.Store(Item.LINE, m_lineDoubleClicked);
    }
    #endregion // Page methods
    
    #region Event reactions
    void CellClicked(object sender, EventArgs e)
    {
      treeQuantities.Nodes.Clear();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          // WorkOrderLine
          IWorkOrderLine wol = (IWorkOrderLine)((sender as Control).Tag);
          ModelDAOHelper.DAOFactory.WorkOrderLineDAO.Lock(wol);
          
          // Intermediate work piece summaries
          IList<IIntermediateWorkPieceTarget> iwpts = ModelDAOHelper.DAOFactory
            .IntermediateWorkPieceTargetDAO.FindByWorkOrderLine(wol.WorkOrder, wol.Line);
          
          IDictionary<int, IList<string>> qttByShift = new Dictionary<int, IList<string>>();
          int nbElement = 0;
          foreach (IIntermediateWorkPieceTarget iwpt in iwpts) {
            if (iwpt.Day.HasValue && iwpt.Number > 0) {
              DateTime iwpsDay = iwpt.Day.Value;
              if (iwpsDay.Date == m_currentDate.Date) {
                int shiftId = -1;
                if (iwpt.Shift != null) {
                  shiftId = iwpt.Shift.Id;
                }

                if (!qttByShift.ContainsKey(shiftId)) {
                  qttByShift[shiftId] = new List<string>();
                }

                string plural = iwpt.Number > 1 ? "s" : "";
                qttByShift[shiftId].Add(iwpt.Number + " part" + plural +
                                        " for " + iwpt.IntermediateWorkPiece.Display);
                nbElement++;
              }
            }
          }
          
          if (nbElement == 0) {
            treeQuantities.Nodes.Add("No productions");
          } else {
            if (qttByShift.Keys.Count == 1 && qttByShift.Keys.First() == -1)
            {
              // No shifts
              IList<string> listTmp = qttByShift[-1];
              foreach (string str in listTmp) {
                treeQuantities.Nodes.Add(str);
              }
            } else {
              foreach (int shiftId in qttByShift.Keys) {
                TreeNode node = treeQuantities.Nodes.Add(
                  shiftId == -1 ? "Undefined" :
                  ModelDAOHelper.DAOFactory.ShiftDAO.FindById(shiftId).Display);
                foreach (string str in qttByShift[shiftId]) {
                  node.Nodes.Add(str);
                }
              }
            }
          }
          
          treeQuantities.ExpandAll();
        }
      }
    }
    
    void CellDoubleClicked(object sender, EventArgs e)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        m_lineDoubleClicked = ((IWorkOrderLine)((sender as Control).Tag)).Line;
      }

      EmitDisplayPageEvent ("Page3", null);
    }
    
    void ButtonPreviousClick(object sender, EventArgs e)
    {
      m_currentDate = m_currentDate.AddDays(-1);
      EmitDisplayPageEvent("Page2", null);
    }
    
    void ButtonNextClick(object sender, EventArgs e)
    {
      m_currentDate = m_currentDate.AddDays(1);
      EmitDisplayPageEvent("Page2", null);
    }
    #endregion // Event reactions
  }
}
