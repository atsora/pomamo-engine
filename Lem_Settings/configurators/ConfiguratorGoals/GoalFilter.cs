// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace ConfiguratorGoals
{
  /// <summary>
  /// Description of GoalFilter.
  /// </summary>
  public class GoalFilter
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GoalFilter).FullName);

    #region Getters / Setters
    /// <summary>
    /// Return true if the filter relates to a full company
    /// </summary>
    public bool IsCompany {
      get {
        return Company != null && Department == null && Cell == null &&
          Category == null && SubCategory == null && Machine == null;
      }
    }
    
    /// <summary>
    /// True if the Goal filter is valid
    /// Invalid: when "unknown" elements are selected
    /// They are not electable to define a rule
    /// </summary>
    public bool Valid { get; set; }
    
    ICompany Company { get; set; }
    IDepartment Department { get; set; }
    ICell Cell { get; set; }
    IMachineCategory Category { get; set; }
    IMachineSubCategory SubCategory { get; set; }
    IMachine Machine { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Create a default filter
    /// </summary>
    public GoalFilter()
    {
      Company = null;
      Department = null;
      Cell = null;
      Category = null;
      SubCategory = null;
      Machine = null;
      Valid = true;
    }
    
    /// <summary>
    /// Create an filter based on a Goal, or an invalid Goal filter
    /// </summary>
    public GoalFilter(IGoal goal)
    {
      Company = null;
      Department = null;
      Cell = null;
      Category = null;
      SubCategory = null;
      Machine = null;
      
      if (goal == null) {
        Valid = false;
      } else {
        Valid = true;
        if (goal.Machine != null) {
          Machine = goal.Machine;
        } else if (goal.Category == null && goal.SubCategory == null) {
          Company = goal.Company;
          Department = goal.Department;
          Cell = goal.Cell;
        } else if (goal.Department == null && goal.Cell == null) {
          Company = goal.Company;
          Category = goal.Category;
          SubCategory = goal.SubCategory;
        }
      }
    }
    
    /// <summary>
    /// Create a filter based on the department
    /// </summary>
    /// <param name="company"></param>
    /// <param name="department"></param>
    /// <param name="cell"></param>
    public GoalFilter(ICompany company, IDepartment department, ICell cell)
    {
      Company = company;
      Department = department;
      Cell = cell;
      Category = null;
      SubCategory = null;
      Machine = null;
      Valid = true;
    }
    
    /// <summary>
    /// Create a filter based on the category
    /// </summary>
    /// <param name="company"></param>
    /// <param name="category"></param>
    /// <param name="subCategory"></param>
    public GoalFilter(ICompany company, IMachineCategory category, IMachineSubCategory subCategory)
    {
      Company = company;
      Department = null;
      Cell = null;
      Category = category;
      SubCategory = subCategory;
      Machine = null;
      Valid = true;
    }
    
    /// <summary>
    /// Create a goal filter focusing on a machine
    /// </summary>
    /// <param name="machine"></param>
    public GoalFilter(IMachine machine)
    {
      Company = null;
      Department = null;
      Cell = null;
      Category = null;
      SubCategory = null;
      Machine = machine;
      Valid = true;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get the description of the filter so that we know on what machine(s) it can be applied
    /// </summary>
    /// <returns></returns>
    public string GetFilterDescription()
    {
      string text = "";
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          if (Machine != null) {
            ModelDAOHelper.DAOFactory.MachineDAO.Lock(Machine);
            text = "machine \"" + Machine.Display + "\"";
          } else if (Category == null && SubCategory == null) {
            // Locks
            if (Company != null) {
              ModelDAOHelper.DAOFactory.CompanyDAO.Lock(Company);
            }

            if (Department != null) {
              ModelDAOHelper.DAOFactory.DepartmentDAO.Lock(Department);
            }

            if (Cell != null) {
              ModelDAOHelper.DAOFactory.CellDAO.Lock(Cell);
            }

            if (Cell == null) {
              if (Department == null) {
                if (Company == null) {
                  text = "all machines";
                }
                else {
                  text = "company \"" + Company.Display + "\"";
                }
              } else {
                text = (Company == null ? "unknown company" : "company \"" + Company.Display + "\"") +
                  ", department \"" + Department.Display + "\"";
              }
            } else {
              text = (Company == null ? "unknown company" : "company \"" + Company.Display + "\"") +
                ", " + (Department == null ? "unknown department" : "department \"" + Department.Display + "\"") +
                ", cell \"" + Cell.Display + "\"";
            }
          } else if (Department == null && Cell == null) {
            // Locks
            if (Company != null) {
              ModelDAOHelper.DAOFactory.CompanyDAO.Lock(Company);
            }

            if (Category != null) {
              ModelDAOHelper.DAOFactory.MachineCategoryDAO.Lock(Category);
            }

            if (SubCategory != null) {
              ModelDAOHelper.DAOFactory.MachineSubCategoryDAO.Lock(SubCategory);
            }

            if (SubCategory == null) {
              if (Category == null) {
                if (Company == null) {
                  text = "all machines";
                }
                else {
                  text = "company \"" + Company.Display + "\"";
                }
              } else {
                text = (Company == null ? "unknown company" : "company \"" + Company.Display + "\"") +
                  ", category \"" + Category.Display + "\"";
              }
            } else {
              text = (Company == null ? "unknown company" : "company \"" + Company.Display + "\"") +
                ", " + (Category == null ? "unknown category" : "category \"" + Category.Display + "\"") +
                ", subcategory \"" + SubCategory.Display + "\"";
            }
          } else {
            text = "...";
          }
        }
      }
      
      return text;
    }
    
    /// <summary>
    /// Hashcode
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        if (Company != null) {
          hashCode += 1000000007 * Company.Id;
        }

        if (Department != null) {
          hashCode += 1000000009 * Department.Id;
        }

        if (Cell != null) {
          hashCode += 1000000021 * Cell.Id;
        }

        if (Category != null) {
          hashCode += 1000000033 * Category.Id;
        }

        if (SubCategory != null) {
          hashCode += 1000000087 * SubCategory.Id;
        }

        if (Machine != null) {
          hashCode += 1000000093 * Machine.Id;
        }
      }
      return hashCode;
    }
    
    /// <summary>
    /// Equality
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      var other = obj as GoalFilter;
      if (other == null) {
        return false;
      }

      return this.GetHashCode() == obj.GetHashCode();
    }
    
    /// <summary>
    /// Equality
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool operator ==(GoalFilter lhs, GoalFilter rhs)
    {
      if (ReferenceEquals(lhs, rhs)) {
        return true;
      }

      if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null)) {
        return false;
      }

      return lhs.Equals(rhs);
    }
    
    /// <summary>
    /// Inequality
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool operator !=(GoalFilter lhs, GoalFilter rhs)
    {
      return !(lhs == rhs);
    }
    
    /// <summary>
    /// Prepare a goal: specify the filter
    /// </summary>
    /// <param name="goal"></param>
    public void PrepareGoal(IGoal goal)
    {
      goal.Machine = Machine;
      goal.Company = Company;
      goal.Department = Department;
      goal.Cell = Cell;
      goal.Category = Category;
      goal.SubCategory = SubCategory;
    }
    #endregion // Methods
  }
}
