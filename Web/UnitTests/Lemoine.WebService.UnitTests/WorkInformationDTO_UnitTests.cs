// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.DTO;
using NUnit.Framework;

namespace Lemoine.WebService.UnitTests
{
  /// <summary>
  /// Unit tests of <see cref="WorkInformationDTO"/> and <see cref="WorkInformationDTOBuilder"/>
  /// that do not require a database
  /// </summary>
  [TestFixture]
  public class WorkInformationDTO_UnitTests
  {
    /// <summary>
    /// A work information with no item has no id
    /// </summary>
    [Test]
    public void TestNoItemHasNoId ()
    {
      Assert.Multiple (() => {
        Assert.That (WorkInformationDTOBuilder.BuildNoOperation ().Id, Is.Null);
        Assert.That (WorkInformationDTOBuilder.BuildNoComponent ().Id, Is.Null);
        Assert.That (WorkInformationDTOBuilder.BuildNoPart ().Id, Is.Null);
        Assert.That (WorkInformationDTOBuilder.BuildNoWorkOrder ().Id, Is.Null);
        Assert.That (WorkInformationDTOBuilder.BuildNoJob ().Id, Is.Null);
        Assert.That (WorkInformationDTOBuilder.BuildNoProject ().Id, Is.Null);
      });
    }

    /// <summary>
    /// Constructors
    /// </summary>
    [Test]
    public void TestConstructors ()
    {
      var withoutId = new WorkInformationDTO (WorkInformationKind.Operation, "OP1");
      var withId = new WorkInformationDTO (WorkInformationKind.Operation, "OP1", 12);
      Assert.Multiple (() => {
        Assert.That (withoutId.Id, Is.Null);
        Assert.That (withId.Kind, Is.EqualTo (WorkInformationKind.Operation));
        Assert.That (withId.Value, Is.EqualTo ("OP1"));
        Assert.That (withId.Id, Is.EqualTo (12));
      });
    }
  }
}
