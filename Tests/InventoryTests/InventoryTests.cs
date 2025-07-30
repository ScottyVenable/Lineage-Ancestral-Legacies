using NUnit.Framework;
using Lineage.Systems.Inventory;

namespace InventoryTests;

public class InventoryTests
{
    [Test]
    public void AddItem_IncreasesCount()
    {
        var inv = new Inventory(5);
        Assert.IsTrue(inv.AddItem("stone"));
        Assert.AreEqual(1, inv.GetItemCount("stone"));
    }

    [Test]
    public void AddItem_RespectsCapacity()
    {
        var inv = new Inventory(1);
        Assert.IsTrue(inv.AddItem("stone"));
        Assert.IsFalse(inv.AddItem("wood"));
    }

    [Test]
    public void RemoveItem_DecreasesCount()
    {
        var inv = new Inventory(3);
        inv.AddItem("wood", 2);
        Assert.IsTrue(inv.RemoveItem("wood"));
        Assert.AreEqual(1, inv.GetItemCount("wood"));
    }

    [Test]
    public void RemoveItem_FailsWhenNotEnough()
    {
        var inv = new Inventory(3);
        Assert.IsFalse(inv.RemoveItem("wood"));
    }
}
