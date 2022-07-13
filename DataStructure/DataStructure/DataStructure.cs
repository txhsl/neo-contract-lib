using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using System;
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
            Assert(list.FirstValue() == 1, "Wrong First Value");
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
            Assert(list.FirstID() == null, "Wrong First ID");
            Assert(list.Count() == 0, "Wrong Count");

            list.AddFirst("E", 4);
            list.AddLast("F", 5);
            list.AddLast("G", 6);
            list.Clear();
            Assert(list.FirstID() == null, "Wrong First ID");
            Assert(list.Count() == 0, "Wrong Count");
            return true;
        }

        public static bool TestDoubleLinkedList()
        {
            // DoubleLinkedList
            DoubleLinkedList<BigInteger> list = new DoubleLinkedList<BigInteger>(0x01);
            list.AddFirst("A", 0);
            list.AddFirst("B", 1);
            list.AddAfter("B", "C", 2);
            list.AddLast("D", 3);
            Assert(list.Count() == 4, "Wrong Count");
            Assert(list.FirstID() == "B", "Wrong First ID");
            Assert(list.FirstValue() == 1, "Wrong First Value");
            Assert(list.LastID() == "D", "Wrong Last ID");
            Assert(list.LastValue() == 3, "Wrong Last Value");
            Assert(list.FindFirst(2) == "C", "Wrong Find");
            Assert(list.FindLast(1) == "B", "Wrong Find");
            Assert(list.NextID("B") == "C" && list.NextID("C") == "A" && list.NextID("A") == "D", "Wrong Order");
            Assert(list.PrevID("D") == "A" && list.PrevID("A") == "C" && list.PrevID("C") == "B", "Wrong Order");

            list.RemoveFirst();
            list.RemoveLast();
            Assert(list.Count() == 2, "Wrong Count");
            Assert(list.FirstID() == "C", "Wrong First ID");
            Assert(list.LastID() == "A", "Wrong Last ID");
            Assert(list.GetValue("A") == 0, "Wrong Get");
            Assert(list.NextID("C") == "A" && list.NextID("A") == null, "Wrong Order");
            Assert(list.PrevID("A") == "C" && list.PrevID("C") == null, "Wrong Order");

            list.RemoveByID("A");
            list.RemoveByValue(2);
            Assert(list.FirstID() == null, "Wrong First ID");
            Assert(list.LastID() == null, "Wrong Last ID");
            Assert(list.Count() == 0, "Wrong Count");

            list.AddFirst("E", 4);
            list.AddLast("F", 5);
            list.AddLast("G", 6);
            list.Clear();
            Assert(list.FirstID() == null, "Wrong First ID");
            Assert(list.LastID() == null, "Wrong Last ID");
            Assert(list.Count() == 0, "Wrong Count");
            return true;
        }

        public static bool TestBinaryTree()
        {
            Func<BigInteger, BigInteger, int> compare = Compare;
            BinaryTree<BigInteger> tree = new BinaryTree<BigInteger>(0x02, compare);

            tree.Insert("A", 5);
            tree.Insert("B", 3);
            tree.Insert("C", 7);
            tree.Insert("D", 2);
            tree.Insert("E", 4);
            tree.Insert("F", 1);
            tree.Insert("G", 8);
            tree.Insert("H", 6);
            tree.Insert("I", 9);
            Assert(tree.Count() == 9, "Wrong Size");

            var preOrder = tree.ToPreOrderArray();
            Assert(preOrder[0] == 5 && preOrder[1] == 3 && preOrder[2] == 2 && preOrder[3] == 1 && preOrder[4] == 4
                && preOrder[5] == 7 && preOrder[6] == 6 && preOrder[7] == 8 && preOrder[8] == 9, "Wrong PreOrder");
            var postOrder = tree.ToPostOrderArray();
            Assert(postOrder[0] == 1 && postOrder[1] == 2 && postOrder[2] == 4 && postOrder[3] == 3 && postOrder[4] == 6
                && postOrder[5] == 9 && postOrder[6] == 8 && postOrder[7] == 7 && postOrder[8] == 5, "Wrong PostOrder");
            var inOrder = tree.ToInOrderArray();
            Assert(inOrder[0] == 1 && inOrder[1] == 2 && inOrder[2] == 3 && inOrder[3] == 4 && inOrder[4] == 5
                && inOrder[5] == 6 && inOrder[6] == 7 && inOrder[7] == 8 && inOrder[8] == 9, "Wrong InOrder");

            tree.RemoveByValue(2);
            tree.RemoveByValue(3);
            tree.RemoveByValue(8);
            tree.RemoveByValue(7);
            tree.RemoveByValue(4);
            tree.RemoveByValue(6);
            tree.RemoveByValue(5);  // bug
            onFault("8",tree.ToInOrderArray());
            tree.RemoveByValue(1);
            onFault("9",tree.ToInOrderArray());
            tree.RemoveByValue(9);
            onFault("10");
            return true;
        }

        private static int Compare(BigInteger x, BigInteger y)
        {
            if (x == y) return 0;
            else if (x < y) return -1;
            else return 1;
        }
    }
}
