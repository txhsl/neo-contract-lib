using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using System.ComponentModel;
using System.Numerics;

namespace DataStructure
{
    [DisplayName("DataStructure")]
    [ManifestExtra("Author", "NEO")]
    [ManifestExtra("Email", "developer@neo.org")]
    [ManifestExtra("Description", "This is a DataStructure Library")]
    public partial class DataStructure : SmartContract
    {
        public static bool TestSingleLinkedList()
        {
            // SingleLinkedList
            SingleLinkedList<BigInteger> list = new SingleLinkedList<BigInteger>(0x00);

            list.AddFirst("A", 0);
            list.AddFirst("B", 1);
            list.AddAfter("B", "C", 2);
            list.AddLast("D", 3);
            Assert(list.Count() == 4, "Wrong Count");
            Assert(list.FirstID() == "B", "Wrong First ID");
            Assert(list.First() == 1, "Wrong First Value");
            Assert(list.Find(2) == "C", "Wrong Find");
            Assert(list.NextID("B") == "C" && list.NextID("C") == "A" && list.NextID("A") == "D", "Wrong Order");

            list.RemoveFirst();
            list.RemoveLast();
            Assert(list.Count() == 2, "Wrong Count");
            Assert(list.FirstID() == "C", "Wrong First ID");
            Assert(list.GetValue("A") == 0, "Wrong Get");
            Assert(list.NextID("C") == "A" && list.NextID("A") == null, "Wrong Order");

            list.RemoveByID("A");
            list.RemoveByValue(2);
            Assert(list.Count() == 0, "Wrong Count");
            Assert(list.FirstID() == null, "Wrong First ID");

            list.AddFirst("E", 4);
            list.AddLast("F", 5);
            list.AddLast("G", 6);
            list.Clear();
            Assert(list.FirstID() == null, "Wrong First");
            Assert(list.Count() == 0, "Wrong Count");
            return true;
        }
    }
}
