using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;
using System.Numerics;

namespace DataStructure
{
    public class BinaryTree<T>
    {
        private struct Node
        {
            internal T Value;
            internal ByteString LeftChildID;
            internal ByteString RightChildID;
        }

        private readonly byte[] RootIDPrefix;
        private readonly byte[] NodeMapPrefix;
        private readonly byte[] CountPrefix;

        private Func<T, T, int> Compare;

        internal BinaryTree(byte treePrefix, Func<T, T, int> compare)
        {
            this.RootIDPrefix = new byte[] { treePrefix, 0x00 };
            this.NodeMapPrefix = new byte[] { treePrefix, 0x01 };
            this.CountPrefix = new byte[] { treePrefix, 0x02 };
            this.Compare = compare;
        }

        internal ByteString RootID()
        {
            return Storage.Get(Storage.CurrentReadOnlyContext, RootIDPrefix);
        }

        internal T RootValue()
        {
            return Get(RootID()).Value;
        }

        internal bool Insert(ByteString id, T value)
        {
            var node = new Node() { Value = value };
            var rootID = RootID();

            if (rootID is null)
            {
                Storage.Put(Storage.CurrentContext, RootIDPrefix, id);
                Set(id, node);
                IncreaseCount();
                return true;
            }

            // Find position
            var currentID = rootID;
            var current = Get(currentID);
            while ((Compare(value, current.Value) >= 0 && current.RightChildID is not null) || (Compare(value, current.Value) < 0 && current.LeftChildID is not null))
            {
                if (Compare(value, current.Value) > 0) currentID = current.RightChildID;
                else if (Compare(value, current.Value) < 0) currentID = current.LeftChildID;
                else return false;
                current = Get(currentID);
            }
            if (Compare(value, current.Value) > 0) current.RightChildID = id;
            else current.LeftChildID = id;

            // Do insert
            Set(currentID, current);
            Set(id, node);
            IncreaseCount();
            return true;
        }

        internal void RemoveByID(ByteString id)
        {
            var value = Get(id).Value;
            RemoveByValue(value);
        }

        internal void RemoveByValue(T value)
        {
            var currentID = RootID();
            var current = Get(currentID);
            var parentID = currentID;
            var parent = current;

            while (currentID is not null)
            {
                current = Get(currentID);
                if (value.Equals(current.Value))
                {
                    // Do remove
                    if (parentID == currentID)
                    {
                        // Remove root
                        if (current.LeftChildID is not null)
                        {
                            // L
                            parentID = current.LeftChildID;
                            parent = Get(parentID);
                            if (parent.RightChildID is not null)
                            {
                                // R, ..., R
                                var newRootID = parent.RightChildID;
                                var newRoot = Get(newRootID);
                                while (newRoot.RightChildID is not null)
                                {
                                    parentID = parent.RightChildID;
                                    parent = newRoot;
                                    newRootID = parent.RightChildID;
                                    newRoot = Get(newRootID);
                                }
                                Storage.Put(Storage.CurrentContext, RootIDPrefix, parent.RightChildID);
                                parent.RightChildID = newRoot.LeftChildID;
                                newRoot.LeftChildID = current.LeftChildID;
                                newRoot.RightChildID = current.RightChildID;
                                Set(parentID, parent);
                                Set(newRootID, newRoot);
                            }
                            else
                            {
                                // Move left child to root
                                parent.RightChildID = current.RightChildID;
                                Storage.Put(Storage.CurrentContext, RootIDPrefix, parentID);
                                Set(parentID, parent);
                            }
                        }
                        else if (current.RightChildID is not null)
                        {
                            // R
                            parentID = current.RightChildID;
                            parent = Get(parentID);
                            if (parent.LeftChildID is not null)
                            {
                                // L, ..., L
                                var newRootID = parent.LeftChildID;
                                var newRoot = Get(newRootID);
                                while (newRoot.LeftChildID is not null)
                                {
                                    parentID = parent.LeftChildID;
                                    parent = newRoot;
                                    newRootID = parent.LeftChildID;
                                    newRoot = Get(newRootID);
                                }
                                Storage.Put(Storage.CurrentContext, RootIDPrefix, parent.LeftChildID);
                                parent.LeftChildID = newRoot.RightChildID;
                                newRoot.LeftChildID = current.LeftChildID;
                                newRoot.RightChildID = current.RightChildID;
                                Set(parentID, parent);
                                Set(newRootID, newRoot);
                            }
                            else
                            {
                                // Move right child to root
                                parent.LeftChildID = current.LeftChildID;
                                Storage.Put(Storage.CurrentContext, RootIDPrefix, parentID);
                                Set(parentID, parent);
                            }
                        }
                        else
                        {
                            // Clear root
                            Storage.Delete(Storage.CurrentContext, RootIDPrefix);
                            Storage.Delete(Storage.CurrentContext, CountPrefix);
                            Delete(currentID);
                            return;
                        }
                    }

                    // Not root
                    if (current.LeftChildID is null)
                    {
                        if (currentID == parent.LeftChildID) parent.LeftChildID = current.RightChildID;
                        else parent.RightChildID = current.RightChildID;
                        Set(parentID, parent);
                    }
                    else if (current.RightChildID is null)
                    {
                        if (currentID == parent.LeftChildID) parent.LeftChildID = current.LeftChildID;
                        else parent.RightChildID = current.LeftChildID;
                        Set(parentID, parent);
                    }
                    else
                    {
                        // R, L, L, ..., L
                        var newCurrentID = current.RightChildID;
                        var newCurrent = Get(newCurrentID);
                        var newParentID = currentID;
                        var newParent = current;
                        while (newCurrent.LeftChildID is not null)
                        {
                            parentID = newCurrentID;
                            parent = newCurrent;
                            newCurrentID = newCurrent.LeftChildID;
                            newCurrent = Get(newCurrentID);
                        }

                        // Directly move
                        if (newParentID == currentID)
                        {
                            newCurrent.LeftChildID = current.LeftChildID;
                            if (currentID == parent.LeftChildID) parent.LeftChildID = newCurrentID;
                            else parent.RightChildID = newCurrentID;
                            Set(parentID, parent);
                            Set(newCurrentID, newCurrent);
                        }
                        // Deal with child nodes
                        else
                        {
                            newParent.LeftChildID = newCurrent.RightChildID;
                            newCurrent.LeftChildID = current.LeftChildID;
                            newCurrent.RightChildID = current.RightChildID;
                            if (currentID == parent.LeftChildID) parent.LeftChildID = newCurrentID;
                            else parent.RightChildID = newCurrentID;
                            Set(parentID, parent);
                            Set(newParentID, newParent);
                            Set(newCurrentID, newCurrent);
                        }
                    }
                    Delete(currentID);
                    DecreaseCount();
                    return;
                }
                else
                {
                    // Next
                    parentID = currentID;
                    parent = current;
                    if (Compare(value, current.Value) > 0) currentID = current.RightChildID;
                    else currentID = current.LeftChildID;
                }
            }
            ExecutionEngine.Abort();
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

        internal T FirstValue()
        {
            var rootID = RootID();
            var current = Get(rootID);

            while (current.LeftChildID is not null) current = Get(current.LeftChildID);
            return current.Value;
        }

        internal T LastValue()
        {
            var rootID = RootID();
            var current = Get(rootID);

            while (current.RightChildID is not null) current = Get(current.RightChildID);
            return current.Value;
        }

        internal T[] ToPreOrderArray()
        {
            var result = new List<T>();
            return PreOrderAdd(result, RootID());
        }

        private List<T> PreOrderAdd(List<T> result, ByteString id)
        {
            if (id is null) return result;
            var node = Get(id);
            result.Add(node.Value);
            result = PreOrderAdd(result, node.LeftChildID);
            result = PreOrderAdd(result, node.RightChildID);
            return result;
        }

        internal T[] ToPostOrderArray()
        {
            var result = new List<T>();
            return PostOrderAdd(result, RootID());
        }

        private List<T> PostOrderAdd(List<T> result, ByteString id)
        {
            if (id is null) return result;
            var node = Get(id);
            result = PostOrderAdd(result, node.LeftChildID);
            result = PostOrderAdd(result, node.RightChildID);
            result.Add(node.Value);
            return result;
        }

        internal T[] ToInOrderArray()
        {
            var result = new List<T>();
            return InOrderAdd(result, RootID());
        }

        private List<T> InOrderAdd(List<T> result, ByteString id)
        {
            if (id is null) return result;
            var node = Get(id);
            result = InOrderAdd(result, node.LeftChildID);
            result.Add(node.Value);
            result = InOrderAdd(result, node.RightChildID);
            return result;
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
    }
}