using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System.Numerics;

namespace DataStructure
{
    public class SingleLinkedList<T>
    {
        private struct Node
        {
            internal T Value;
            internal ByteString NextID;
        }

        private readonly byte[] FirstIDPrefix;
        private readonly byte[] NodeMapPrefix;
        private readonly byte[] CountPrefix;

        internal SingleLinkedList(byte listPrefix)
        {
            this.FirstIDPrefix = new byte[] { listPrefix, 0x00 };
            this.NodeMapPrefix = new byte[] { listPrefix, 0x01 };
            this.CountPrefix = new byte[] { listPrefix, 0x02 };
        }

        internal ByteString FirstID()
        {
            return Storage.Get(Storage.CurrentReadOnlyContext, FirstIDPrefix);
        }

        internal ByteString NextID(ByteString id)
        {
            return Get(id).NextID;
        }

        private Node Get(ByteString id)
        {
            StorageMap nodeMap = new(Storage.CurrentReadOnlyContext, NodeMapPrefix);
            return (Node)StdLib.Deserialize(nodeMap.Get(id));
        }

        private void Set(ByteString id, Node node)
        {
            StorageMap nodeMap = new(Storage.CurrentContext, NodeMapPrefix);
            nodeMap.Put(id, StdLib.Serialize(node));
        }

        private void Delete(ByteString id)
        {
            StorageMap nodeMap = new(Storage.CurrentContext, NodeMapPrefix);
            nodeMap.Delete(id);
        }

        internal T FirstValue()
        {
            var firstID = FirstID();
            return Get(firstID).Value;
        }

        internal T NextValue(ByteString id)
        {
            return Get(NextID(id)).Value;
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
            node.NextID = FirstID();
            Storage.Put(Storage.CurrentContext, FirstIDPrefix, id);
            Set(id, node);
            IncreaseCount();
        }

        internal void AddLast(ByteString id, T value)
        {
            var firstID = FirstID();
            if (firstID is null)
            {
                AddFirst(id, value);
            }
            else
            {
                var node = new Node() { Value = value };
                var currentID = firstID;
                var current = Get(currentID);
                while (current.NextID is not null)
                {
                    currentID = current.NextID;
                    current = Get(current.NextID);
                }
                current.NextID = id;
                Set(currentID, current);
                Set(id, node);
                IncreaseCount();
            }
        }

        internal void AddAfter(ByteString parentID, ByteString id, T value)
        {
            if (parentID is null) AddFirst(id, value);

            var node = new Node() { Value = value };
            var parent = Get(parentID);
            node.NextID = parent.NextID;
            parent.NextID = id;
            Set(parentID, parent);
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
                Storage.Delete(Storage.CurrentContext, CountPrefix);
            }
            else
            {
                Storage.Put(Storage.CurrentContext, FirstIDPrefix, first.NextID);
                DecreaseCount();
            }
        }

        internal void RemoveLast()
        {
            var parentID = FirstID();
            var parent = Get(parentID);

            if (parent.NextID is null)
            {
                RemoveFirst();
                return;
            }

            while (parent.NextID is not null)
            {
                var current = Get(parent.NextID);
                if (current.NextID is null)
                {
                    Delete(parent.NextID);
                    parent.NextID = null;
                    Set(parentID, parent);
                    break;
                }
                parentID = parent.NextID;
                parent = current;
            }
            DecreaseCount();
        }

        internal void RemoveByID(ByteString id)
        {
            var firstID = FirstID();
            if (id == firstID)
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
                if (currentID == id)
                {
                    parent.NextID = current.NextID;
                    Delete(currentID);
                    Set(parentID, parent);
                    DecreaseCount();
                    return;
                }
                parentID = currentID;
                parent = current;
            }
            ExecutionEngine.Abort();
        }

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
                    parent.NextID = current.NextID;
                    Delete(parent.NextID);
                    Set(parentID, parent);
                    DecreaseCount();
                    return;
                }
                parentID = currentID;
                parent = current;
            }
            ExecutionEngine.Abort();
        }

        internal ByteString Find(T value)
        {
            StorageMap nodeMap = new(Storage.CurrentReadOnlyContext, NodeMapPrefix);
            var currentID = Storage.Get(Storage.CurrentReadOnlyContext, FirstIDPrefix);

            while (currentID is not null)
            {
                var current = (Node)StdLib.Deserialize(nodeMap.Get(currentID));
                if (current.Value.Equals(value)) return currentID;
                currentID = current.NextID;
            }
            return null;
        }

        internal void Clear()
        {
            var firstID = FirstID();
            Storage.Delete(Storage.CurrentContext, FirstIDPrefix);
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
