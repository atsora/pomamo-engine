// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

using Lemoine.Core.Log;
using Lemoine.Model;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for <see cref="ShiftTemplateItemLayers"/>: flattening of the nested shift
  /// templates, grouping in layers and reset of the impacted week or day
  ///
  /// Note: no database is required here, the model is replaced by the local stubs below
  /// </summary>
  [TestFixture]
  public class ShiftTemplateItemLayers_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ShiftTemplateItemLayers_UnitTest).FullName);

    #region Stubs
    /// <summary>
    /// Minimal implementation of <see cref="IShiftTemplateItem"/>
    /// </summary>
    class Item : IShiftTemplateItem
    {
      public int Id { get; set; }
      public int Version => 0;
      public IShift Shift { get; set; }
      public IShiftTemplate SubShiftTemplate { get; set; }
      public WeekDay WeekDays { get; set; } = WeekDay.AllDays;
      public TimePeriodOfDay TimePeriod { get; set; }
      public DateTime? Day { get; set; }
      public int? WeekYear { get; set; }
      public int? WeekNumber { get; set; }
      public int? WeekFrequency { get; set; }

      public void Unproxy () { }

      public override string ToString () => $"[Item {this.Id}]";
    }

    /// <summary>
    /// Minimal implementation of <see cref="IShiftTemplate"/>
    /// </summary>
    class Template : IShiftTemplate
    {
      readonly ISet<IShiftTemplateItem> m_items = new HashSet<IShiftTemplateItem> ();

      public Template (int id)
      {
        this.Id = id;
      }

      public int Id { get; }
      public int Version => 0;
      public string Name { get; set; } = "";
      public string Display => $"Template {this.Id}";
      public string SelectionText => this.Display;
      public string[] Identifiers => new string[] { "Id" };
      public ISet<IShiftTemplateItem> Items => m_items;
      public ISet<IShiftTemplateBreak> Breaks => throw new NotImplementedException ();

      /// <summary>
      /// Append an item, the id of the items being unique in the tested templates
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      public Item Add (int id)
      {
        var item = new Item { Id = id };
        m_items.Add (item);
        return item;
      }

      public IShiftTemplateItem AddItem (IShift shift) => throw new NotImplementedException ();
      public IShiftTemplateItem AddItem (IShiftTemplate subShiftTemplate) => throw new NotImplementedException ();
      public IShiftTemplateBreak AddBreak () => throw new NotImplementedException ();

      public void Unproxy () { }

      public override string ToString () => this.Display;
    }

    /// <summary>
    /// Minimal implementation of <see cref="IShift"/>, only used to make an item valid
    /// </summary>
    class TestShift : IShift
    {
      public int Id => 0;
      public int Version => 0;
      public string Name { get; set; }
      public string Code { get; set; }
      public string ExternalCode { get; set; }
      public string Color { get; set; } = "#FFFFFF";
      public int? DisplayPriority { get; set; }
      public string Display => "shift";
      public string SelectionText => "shift";

      public int CompareTo (object obj) => 0;

      public void Unproxy () { }
    }

    static readonly IShift SHIFT = new TestShift ();

    static DateTime D (int year, int month, int day) => new DateTime (year, month, day, 0, 0, 0, DateTimeKind.Local);

    /// <summary>
    /// Ids of the leaf items of the layers, one list per layer
    /// </summary>
    static IList<IList<int>> LayerLeafIds (IEnumerable<ShiftTemplateItemLayer> layers)
    {
      return layers
        .Select (l => (IList<int>)l.Paths.Select (p => p[p.Count - 1].Id).OrderBy (i => i).ToList ())
        .ToList ();
    }
    #endregion // Stubs

    /// <summary>
    /// A template without any nested template gives one path per item
    /// </summary>
    [Test]
    public void TestFlatTemplate ()
    {
      var template = new Template (1);
      template.Add (11).Shift = SHIFT;
      template.Add (12).Shift = SHIFT;

      var layers = ShiftTemplateItemLayers.GetLayers (template);

      Assert.Multiple (() => {
        Assert.That (layers, Has.Count.EqualTo (1), "One single layer without any criteria");
        Assert.That (layers[0].Priority, Is.EqualTo (0));
        Assert.That (LayerLeafIds (layers)[0], Is.EqualTo (new[] { 11, 12 }));
        Assert.That (layers[0].Paths.All (p => 1 == p.Count), Is.True, "No nested template");
      });
    }

    /// <summary>
    /// An item that references another shift template is replaced by the paths
    /// that lead to the items of the referenced template
    /// </summary>
    [Test]
    public void TestNestedTemplate ()
    {
      var sub = new Template (2);
      sub.Add (21).Shift = SHIFT;
      sub.Add (22).Shift = SHIFT;

      var template = new Template (1);
      var subItem = template.Add (10);
      subItem.SubShiftTemplate = sub;
      template.Add (11).Shift = SHIFT;

      var layers = ShiftTemplateItemLayers.GetLayers (template);

      Assert.Multiple (() => {
        Assert.That (layers, Has.Count.EqualTo (1));
        Assert.That (LayerLeafIds (layers)[0], Is.EqualTo (new[] { 11, 21, 22 }),
                     "The item 10 is replaced by the items of the sub template");
        var nestedPaths = layers[0].Paths.Where (p => 1 < p.Count).ToList ();
        Assert.That (nestedPaths, Has.Count.EqualTo (2));
        Assert.That (nestedPaths.All (p => 10 == p[0].Id), Is.True,
                     "Each nested path starts with the item that references the sub template");
      });
    }

    /// <summary>
    /// Three levels of nesting
    /// </summary>
    [Test]
    public void TestTwoLevelsOfNesting ()
    {
      var subSub = new Template (3);
      subSub.Add (31).Shift = SHIFT;

      var sub = new Template (2);
      sub.Add (20).SubShiftTemplate = subSub;

      var template = new Template (1);
      template.Add (10).SubShiftTemplate = sub;

      var layers = ShiftTemplateItemLayers.GetLayers (template);

      Assert.Multiple (() => {
        Assert.That (layers, Has.Count.EqualTo (1));
        Assert.That (layers[0].Paths, Has.Count.EqualTo (1));
        Assert.That (layers[0].Paths[0].Select (i => i.Id), Is.EqualTo (new[] { 10, 20, 31 }));
      });
    }

    /// <summary>
    /// A cycle in the shift templates is detected and the guilty item is skipped
    /// </summary>
    [Test]
    public void TestCycle ()
    {
      var template = new Template (1);
      var sub = new Template (2);

      // 1 -> 2 -> 1
      template.Add (10).SubShiftTemplate = sub;
      sub.Add (20).SubShiftTemplate = template;
      sub.Add (21).Shift = SHIFT;

      var layers = ShiftTemplateItemLayers.GetLayers (template);

      Assert.Multiple (() => {
        Assert.That (layers, Has.Count.EqualTo (1));
        Assert.That (LayerLeafIds (layers)[0], Is.EqualTo (new[] { 21 }),
                     "Only the item that does not close the cycle is kept");
      });
    }

    /// <summary>
    /// A template that references itself is skipped as well
    /// </summary>
    [Test]
    public void TestDirectCycle ()
    {
      var template = new Template (1);
      template.Add (10).SubShiftTemplate = template;
      template.Add (11).Shift = SHIFT;

      var layers = ShiftTemplateItemLayers.GetLayers (template);

      Assert.Multiple (() => {
        Assert.That (layers, Has.Count.EqualTo (1));
        Assert.That (LayerLeafIds (layers)[0], Is.EqualTo (new[] { 11 }));
      });
    }

    /// <summary>
    /// The recursion stops when the maximum depth is reached
    /// </summary>
    [Test]
    public void TestMaxRecursionDepth ()
    {
      // 1 -> 2 -> 3, each template also defining a shift
      var templates = new Template[] { new Template (1), new Template (2), new Template (3) };
      for (int i = 0; i < templates.Length; ++i) {
        templates[i].Add (10 * (i + 1)).Shift = SHIFT;
        if (i + 1 < templates.Length) {
          templates[i].Add (10 * (i + 1) + 1).SubShiftTemplate = templates[i + 1];
        }
      }

      Assert.Multiple (() => {
        Assert.That (LayerLeafIds (ShiftTemplateItemLayers.GetLayers (templates[0], 3))[0],
                     Is.EqualTo (new[] { 10, 20, 30 }), "The three levels are applied");
        Assert.That (LayerLeafIds (ShiftTemplateItemLayers.GetLayers (templates[0], 2))[0],
                     Is.EqualTo (new[] { 10, 20 }), "The third level is skipped");
        Assert.That (LayerLeafIds (ShiftTemplateItemLayers.GetLayers (templates[0], 1))[0],
                     Is.EqualTo (new[] { 10 }), "Only the root template is applied");
      });
    }

    /// <summary>
    /// An item that references neither a shift nor a shift template is skipped
    /// </summary>
    [Test]
    public void TestItemWithoutShift ()
    {
      var template = new Template (1);
      template.Add (10);
      template.Add (11).Shift = SHIFT;

      var layers = ShiftTemplateItemLayers.GetLayers (template);

      Assert.That (LayerLeafIds (layers)[0], Is.EqualTo (new[] { 11 }));
    }

    /// <summary>
    /// The paths are grouped in layers by restriction criteria, the layers being sorted
    /// from the least specific one to the most specific one
    /// </summary>
    [Test]
    public void TestLayerOrdering ()
    {
      var template = new Template (1);
      template.Add (10).Shift = SHIFT;                                    // no criteria
      var day = template.Add (11);                                        // a specific day
      day.Shift = SHIFT;
      day.Day = D (2026, 12, 25);
      var week2027 = template.Add (12);                                   // the week 2 of 2027
      week2027.Shift = SHIFT;
      week2027.WeekYear = 2027;
      week2027.WeekNumber = 2;
      var week2026 = template.Add (13);                                   // the week 34 of 2026
      week2026.Shift = SHIFT;
      week2026.WeekYear = 2026;
      week2026.WeekNumber = 34;
      var anyYear = template.Add (14);                                    // the week 10 of any year
      anyYear.Shift = SHIFT;
      anyYear.WeekNumber = 10;

      var layers = ShiftTemplateItemLayers.GetLayers (template);

      Assert.Multiple (() => {
        Assert.That (layers.Select (l => l.Priority), Is.EqualTo (new[] { 0, 1, 1, 1, 2 }));
        Assert.That (LayerLeafIds (layers),
                     Is.EqualTo (new[] {
                       new[] { 10 }, new[] { 14 }, new[] { 13 }, new[] { 12 }, new[] { 11 }
                     }));
      });
    }

    /// <summary>
    /// Two items that share the same criteria belong to the same layer, so that
    /// the second one does not reset what the first one applied
    /// </summary>
    [Test]
    public void TestSameCriteriaInOneLayer ()
    {
      var sub = new Template (2);
      sub.Add (20).Shift = SHIFT; // morning
      sub.Add (21).Shift = SHIFT; // afternoon

      var template = new Template (1);
      var subItem = template.Add (10);
      subItem.SubShiftTemplate = sub;
      subItem.WeekYear = 2026;
      subItem.WeekNumber = 35;
      template.Add (11).Shift = SHIFT;

      var layers = ShiftTemplateItemLayers.GetLayers (template);

      Assert.Multiple (() => {
        Assert.That (layers, Has.Count.EqualTo (2));
        Assert.That (LayerLeafIds (layers)[0], Is.EqualTo (new[] { 11 }));
        Assert.That (LayerLeafIds (layers)[1], Is.EqualTo (new[] { 20, 21 }),
                     "The two items of the sub template are in the same layer");
        Assert.That (layers[1].Priority, Is.EqualTo (1));
      });
    }

    /// <summary>
    /// When both an item that references a shift template and one of its items are restricted,
    /// the most specific criterion is considered
    /// </summary>
    [Test]
    public void TestMostSpecificCriterionOfAPath ()
    {
      var sub = new Template (2);
      var subDay = sub.Add (20);
      subDay.Shift = SHIFT;
      subDay.Day = D (2026, 12, 25);
      sub.Add (21).Shift = SHIFT;

      var template = new Template (1);
      var subItem = template.Add (10);
      subItem.SubShiftTemplate = sub;
      subItem.WeekYear = 2026;
      subItem.WeekNumber = 35;

      var layers = ShiftTemplateItemLayers.GetLayers (template);

      Assert.Multiple (() => {
        Assert.That (layers, Has.Count.EqualTo (2));
        Assert.That (LayerLeafIds (layers)[0], Is.EqualTo (new[] { 21 }));
        Assert.That (layers[0].Priority, Is.EqualTo (1), "The week of the parent item");
        Assert.That (LayerLeafIds (layers)[1], Is.EqualTo (new[] { 20 }));
        Assert.That (layers[1].Priority, Is.EqualTo (2), "The day of the sub item is more specific");
      });
    }

    /// <summary>
    /// A path that is restricted to a specific week resets the whole week,
    /// even the days on which it defines no shift
    /// </summary>
    [Test]
    public void TestIsResetApplicableOnAWeek ()
    {
      var item = new Item {
        Id = 10,
        WeekYear = 2026,
        WeekNumber = 35,
        WeekDays = DayOfWeek.Monday.ConvertToWeekDay ()
      };
      var path = new List<IShiftTemplateItem> { item };

      Assert.Multiple (() => {
        Assert.That (ShiftTemplateItemLayers.IsResetApplicable (path, D (2026, 08, 24), 2026, 35), Is.True,
                     "Monday of the week 35");
        Assert.That (ShiftTemplateItemLayers.IsResetApplicable (path, D (2026, 08, 29), 2026, 35), Is.True,
                     "Saturday of the week 35: the week days are not considered");
        Assert.That (ShiftTemplateItemLayers.IsResetApplicable (path, D (2026, 08, 17), 2026, 34), Is.False,
                     "The week 34 is not impacted");
      });
    }

    /// <summary>
    /// A path that is restricted to a specific day only resets that day
    /// </summary>
    [Test]
    public void TestIsResetApplicableOnADay ()
    {
      var item = new Item { Id = 10, Day = D (2026, 12, 25) };
      var path = new List<IShiftTemplateItem> { item };

      Assert.Multiple (() => {
        Assert.That (ShiftTemplateItemLayers.IsResetApplicable (path, D (2026, 12, 25), 2026, 52), Is.True);
        Assert.That (ShiftTemplateItemLayers.IsResetApplicable (path, D (2026, 12, 26), 2026, 52), Is.False);
      });
    }

    /// <summary>
    /// The week criteria of all the items of a path must be satisfied for the reset to apply
    /// </summary>
    [Test]
    public void TestIsResetApplicableOnAPath ()
    {
      var parent = new Item { Id = 10, WeekYear = 2026, WeekNumber = 35 };
      var child = new Item { Id = 20, Day = D (2026, 08, 26) };
      var path = new List<IShiftTemplateItem> { parent, child };

      Assert.Multiple (() => {
        Assert.That (ShiftTemplateItemLayers.IsResetApplicable (path, D (2026, 08, 26), 2026, 35), Is.True);
        Assert.That (ShiftTemplateItemLayers.IsResetApplicable (path, D (2026, 08, 25), 2026, 35), Is.False,
                     "Another day of the week 35");
        Assert.That (ShiftTemplateItemLayers.IsResetApplicable (path, D (2026, 08, 26), 2026, 34), Is.False,
                     "The week of the parent item does not match");
      });
    }

    /// <summary>
    /// The example of the specification: a global template references a template A
    /// that defines the morning and a template B that defines the afternoon
    /// from the week 35 of 2026.
    /// From that week, the morning must be stopped
    /// </summary>
    [Test]
    public void TestGlobalTemplateWithTwoSubTemplates ()
    {
      var templateA = new Template (2);
      var morning = templateA.Add (20);
      morning.Shift = SHIFT;
      morning.TimePeriod = new TimePeriodOfDay ("08:00-13:00");

      var templateB = new Template (3);
      var afternoon = templateB.Add (30);
      afternoon.Shift = SHIFT;
      afternoon.TimePeriod = new TimePeriodOfDay ("13:00-21:00");

      var global = new Template (1);
      global.Add (10).SubShiftTemplate = templateA;
      var itemB = global.Add (11);
      itemB.SubShiftTemplate = templateB;
      itemB.WeekYear = 2026;
      itemB.WeekNumber = 35;
      itemB.WeekFrequency = 1;

      var layers = ShiftTemplateItemLayers.GetLayers (global);

      Assert.Multiple (() => {
        Assert.That (layers, Has.Count.EqualTo (2));
        Assert.That (LayerLeafIds (layers)[0], Is.EqualTo (new[] { 20 }), "The morning first");
        Assert.That (LayerLeafIds (layers)[1], Is.EqualTo (new[] { 30 }), "Then the afternoon");

        var morningPath = layers[0].Paths[0];
        var afternoonPath = layers[1].Paths[0];

        // Before the week 35, only the morning applies
        Assert.That (ShiftTemplateItemLayers.IsResetApplicable (afternoonPath, D (2026, 08, 18), 2026, 34), Is.False);
        Assert.That (morningPath.All (i => i.IsDayApplicable (D (2026, 08, 18), 2026, 34)), Is.True);
        Assert.That (afternoonPath.All (i => i.IsDayApplicable (D (2026, 08, 18), 2026, 34)), Is.False);

        // From the week 35, the day is reset before the afternoon is applied,
        // so that the morning is stopped
        Assert.That (ShiftTemplateItemLayers.IsResetApplicable (afternoonPath, D (2026, 08, 25), 2026, 35), Is.True);
        Assert.That (afternoonPath.All (i => i.IsDayApplicable (D (2026, 08, 25), 2026, 35)), Is.True);

        // And still on the following weeks, because the frequency is 1 week
        Assert.That (ShiftTemplateItemLayers.IsResetApplicable (afternoonPath, D (2026, 09, 01), 2026, 36), Is.True);
      });
    }

    /// <summary>
    /// An item is considered as restricted as soon as it has a day, a week
    /// or a time period of day
    /// </summary>
    [Test]
    public void TestIsDateRestricted ()
    {
      Assert.Multiple (() => {
        Assert.That (ShiftTemplateItemLayers.IsDateRestricted (new Item ()), Is.False);
        Assert.That (ShiftTemplateItemLayers.IsDateRestricted (new Item { Day = D (2026, 12, 25) }), Is.True);
        Assert.That (ShiftTemplateItemLayers.IsDateRestricted (new Item { WeekNumber = 35 }), Is.True);
        Assert.That (ShiftTemplateItemLayers.IsDateRestricted (
          new Item { TimePeriod = new TimePeriodOfDay ("08:00-13:00") }), Is.True);
      });
    }
  }
}
