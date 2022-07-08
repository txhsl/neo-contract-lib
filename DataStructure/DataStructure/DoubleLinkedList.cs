using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System.Numerics;

namespace DataStructure
{
    public class DoubleLinkedList<T>
    {
        private struct Node
        {
            internal T Value;
            internal ByteString NextID;
            internal ByteString PrevID;
        }

        internal readonly byte[] FirstIDPrefix = { 0x01, 0x00 };
        internal readonly byte[] LastIDPrefix = { 0x01, 0x01 };
        internal readonly byte[] NodePrefix = { 0x01, 0x02 };
        internal readonly byte[] CountPrefix = { 0x01, 0x03 };

        public DoubleLinkedList(byte listPrefix)
        {
            this.FirstIDPrefix = new byte[] { listPrefix, 0x00 };
            this.LastIDPrefix = new byte[] { listPrefix, 0x01 };
            this.NodePrefix = new byte[] { listPrefix, 0x02 };
            this.CountPrefix = new byte[] { listPrefix, 0x03 };
        }

        internal ByteString FirstID()
        {
            return Storage.Get(Storage.CurrentReadOnlyContext, FirstIDPrefix);
        }

        internal ByteString LastID()
        {
            return Storage.Get(Storage.CurrentReadOnlyContext, LastIDPrefix);
        }

        internal ByteString NextID(ByteString id)
        {
            return Get(id).NextID;
        }

        internal ByteString PrevID(ByteString id)
        {
            return Get(id).PrevID;
        }

        private Node Get(ByteString id)
        {
            StorageMap nodeMap = new(Storage.CurrentReadOnlyContext, NodePrefix);
            return (Node)StdLib.Deserialize(nodeMap.Get(id));
        }

        private void Set(ByteString id, Node node)
        {
            StorageMap nodeMap = new(Storage.CurrentContext, NodePrefix);
            nodeMap.Put(id, StdLib.Serialize(node));
        }

        private void Delete(ByteString id)
        {
            StorageMap nodeMap = new(Storage.CurrentContext, NodePrefix);
            nodeMap.Delete(id);
        }

        internal T First()
        {
            var firstID = FirstID();
            return Get(firstID).Value;
        }

        internal T Last()
        {
            var lastID = LastID();
            return Get(lastID).Value;
        }

        internal T Next(ByteString id)
        {
            return Get(NextID(id)).Value;
        }

        internal T Prev(ByteString id)
        {
            return Get(PrevID(id)).Value;
        }

        internal BigInteger Count()
        {
            return (BigInteger)Storage.Get(Storage.CurrentReadOnlyContext, CountPrefix);
        }

        private void IncreaseCount()
        {
            var count = (BigInteger)Storage.Get(Storage.CurrentReadOnlyContext, CountPrefix);
            Storage.Put(Storage.CurrentContext, CountPrefix, count + 1);
        }

        private void DecreaseCount()
        {
            var count = (BigInteger)Storage.Get(Storage.CurrentReadOnlyContext, CountPrefix);
            Storage.Put(Storage.CurrentContext, CountPrefix, count - 1);
        }

        internal T GetValue(ByteString id)
        {
            return Get(id).Value;
        }

        internal void AddFirst(ByteString id, T value)
        {
            var node = new Node() { Value = value };
            var firstID = FirstID();
            Storage.Put(Storage.CurrentContext, FirstIDPrefix, id);
            if (LastID() is null) Storage.Put(Storage.CurrentContext, LastIDPrefix, id);
            node.NextID = firstID;
            Set(id, node);
            if (firstID is not null)
            {
                var first = Get(firstID);
                first.PrevID = id;
                Set(firstID, first);
            }
            IncreaseCount();
        }

        internal void AddLast(ByteString id, T value)
        {
            var node = new Node() { Value = value };
            var lastID = LastID();
            Storage.Put(Storage.CurrentContext, LastIDPrefix, id);
            if (FirstID() is null) Storage.Put(Storage.CurrentContext, FirstIDPrefix, id);
            node.PrevID = lastID;
            Set(id, node);
            if (LastID() is not null)
            {
                var last = Get(lastID);
                last.NextID = id;
                Set(lastID, last);
            }
            IncreaseCount();
        }

        internal void AddAfter(ByteString parentID, ByteString id, T value)
        {
            if (parentID is null) AddFirst(id, value);

            var node = new Node() { Value = value };
            var parent = Get(parentID);
            var childID = parent.NextID;
            var child = Get(childID);
            node.NextID = childID;
            node.PrevID = parentID;
            parent.NextID = id;
            child.PrevID = id;
            Set(parentID, parent);
            Set(childID, child);
            Set(id, node);
            IncreaseCount();
        }

        internal void AddBefore(ByteString childID, ByteString id, T value)
        {
            if (childID is null) AddFirst(id, value);

            var node = new Node() { Value = value };
            var child = Get(childID);
            var parentID = child.PrevID;
            var parent = Get(parentID);

            node.NextID = childID;
            node.PrevID = parentID;
            parent.NextID = id;
            child.PrevID = id;
            Set(parentID, parent);
            Set(childID, child);
            Set(id, node);
            IncreaseCount();
        }

        internal void RemoveFirst()
        {
            var firstID = FirstID();
            var first = Get(firstID);
            Delete(firstID);
            if (first.NextID is null)
            {
                Storage.Delete(Storage.CurrentContext, FirstIDPrefix);
                Storage.Delete(Storage.CurrentContext, LastIDPrefix);
                Storage.Delete(Storage.CurrentContext, CountPrefix);
            }
            else
            {
                Storage.Put(Storage.CurrentContext, FirstIDPrefix, first.NextID);
                var child = Get(first.NextID);
                child.PrevID = null;
                Set(first.NextID, child);
                DecreaseCount();
            }
        }

        internal void RemoveLast()
        {
            var lastID = LastID();
            var last = Get(lastID);
            Delete(lastID);
            if (last.PrevID is null)
            {
                Storage.Delete(Storage.CurrentContext, FirstIDPrefix);
                Storage.Delete(Storage.CurrentContext, LastIDPrefix);
                Storage.Delete(Storage.CurrentContext, CountPrefix);
            }
            else
            {
                Storage.Put(Storage.CurrentContext, LastIDPrefix, last.PrevID);
                var parent = Get(last.PrevID);
                parent.NextID = null;
                Set(last.PrevID, parent);
                DecreaseCount();
            }
        }

        internal void RemoveByID(ByteString id)
        {
            if (id == FirstID())
            {
                RemoveFirst();
                return;
            }
            if (id == LastID())
            {
                RemoveLast();
                return;
            }

            var current = Get(id);
            var parent = Get(current.PrevID);
            var child = Get(current.NextID);

            parent.NextID = current.NextID;
            child.PrevID = current.PrevID;
            Delete(id);
            Set(current.PrevID, parent);
            Set(current.NextID, child);
        }

        // From here
        internal void RemoveByValue(T value)
        {
            var firstID = FirstID();
            var first = Get(firstID);
            if (value.Equals(first.Value))
            {
                RemoveFirst();
                return;
            }

            var parentID = firstID;
            var parent = Get(firstID);
            while (parent.NextID is not null)
            {
                var currentID = parent.NextID;
                var current = Get(currentID);
                if (current.Value.Equals(value))
                {
                    var child = Get(current.NextID);
                    parent.NextID = current.NextID;
                    child.PrevID = current.PrevID;
                    Delete(parent.NextID);
                    Set(parentID, parent);
                    Set(current.NextID, child);
                    DecreaseCount();
                    return;
                }
                parentID = currentID;
                parent = current;
            }
            ExecutionEngine.Abort();
        }

        internal ByteString FindFirst(T value)
        {
            StorageMap nodeMap = new(Storage.CurrentReadOnlyContext, NodePrefix);
            var currentID = Storage.Get(Storage.CurrentReadOnlyContext, FirstIDPrefix);

            while (currentID is not null)
            {
                var current = (Node)StdLib.Deserialize(nodeMap.Get(currentID));
                if (current.Value.Equals(value)) return currentID;
                currentID = current.NextID;
            }
            return null;
        }

        internal ByteString FindLast(T value)
        {
            StorageMap nodeMap = new(Storage.CurrentReadOnlyContext, NodePrefix);
            var currentID = Storage.Get(Storage.CurrentReadOnlyContext, LastIDPrefix);

            while (currentID is not null)
            {
                var current = (Node)StdLib.Deserialize(nodeMap.Get(currentID));
                if (current.Value.Equals(value)) return currentID;
                currentID = current.PrevID;
            }
            return null;
        }

        internal void Clear()
        {
            var firstID = FirstID();
            Storage.Delete(Storage.CurrentContext, FirstIDPrefix);
            Storage.Delete(Storage.CurrentContext, LastIDPrefix);
            var currentID = firstID;
            while (currentID is not null)
            {
                var nextID = Get(currentID).NextID;
                Delete(currentID);
                currentID = nextID;
            }
            Storage.Delete(Storage.CurrentContext, CountPrefix);
        }
    }
}
