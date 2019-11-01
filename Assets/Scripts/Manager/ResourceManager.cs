﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ResourceManager : Singleton<ResourceManager>
{
    public bool IsLoadFromAssetBundle = false;

    protected CMapList<AssetItem> unRefAseetItems = new CMapList<AssetItem>();

    public Dictionary<uint, AssetItem> AssetDic { get; set; } = new Dictionary<uint, AssetItem>();

    public T LoadResource<T>(string path) where T : Object
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        uint crc = CRC32.GetCRC32(path);
        AssetItem item = GetCacheAssetItem(crc);
        if (item != null)
        {
            return item.AssetObject as T;
        }

        T obj = null;
#if UNITY_EDITOR
        if (IsLoadFromAssetBundle)
        {
            item = AssetBundleManager.Instance.FindAssetItem(crc);
            if (item.AssetObject != null)
                obj = item.AssetObject as T;
            else
                obj = LoadAssetByEditor<T>(path);

        }
#endif
        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadAssetItemBundle(crc);
            if (item != null && item.assetBundle != null)
            {
                if (item.AssetObject != null)
                    obj = item.AssetObject as T;
                else
                    obj = item.assetBundle.LoadAsset<T>(item.assetName);
            }
        }

        CacheResource(path, ref item, crc, obj);
        return obj;
    }

    public bool ReleaseResource(Object obj, bool destroyObj = false)
    {
        if (obj == null) return false;
        AssetItem item = null;
        foreach (var res in AssetDic.Values)
        {
            if (res.guid == obj.GetInstanceID())
            {
                item = res;
            }
        }

        if (item == null)
        {
            Debug.LogError("AssetDic not exits " + obj.name + "，可能进行了多次释放");
            return false;
        }

        item.RefCount--;
        DestroyAssetItem(item, destroyObj);
        return true;
    }

    private void CacheResource(string path, ref AssetItem item, uint crc, Object obj, int addRefCount = 1)
    {
        WashOut();
        if (item == null)
        {
            Debug.LogError("Resource item is null,path:" + path);
        }

        if (obj == null)
        {
            Debug.LogError("Resource Load Fail,path:" + path);
        }

        item.AssetObject = obj;
        item.guid = obj.GetInstanceID();
        item.lastUseTime = Time.realtimeSinceStartup;
        item.RefCount += addRefCount;
        AssetItem oldItem;
        if (AssetDic.TryGetValue(item.crc, out oldItem))
        {
            AssetDic[crc] = item;
        }
        else
        {
            AssetDic.Add(crc, item);
        }
    }

    private void WashOut()
    {
//        {
//            if (unRefAseetItems.Size() <= 0)
//                break;
//            AssetItem item = unRefAseetItems.Back();
//            DestroyAssetItem(item);
//            unRefAseetItems.Pop();
//        }
    }

    private void DestroyAssetItem(AssetItem item, bool destroyCache = false)
    {
        if (item == null || item.RefCount > 0)
            return;
        if (!AssetDic.Remove(item.crc))
            return;
        if (!destroyCache)
        {
            unRefAseetItems.InsertToHead(item);
            return;
        }
        AssetBundleManager.Instance.ReleaseAsset(item);
        item.AssetObject = null;
    }
#if UNITY_EDITOR
    protected T LoadAssetByEditor<T>(string path) where T : Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }
#endif


    private AssetItem GetCacheAssetItem(uint crc, int refCount = 1)
    {
        if (AssetDic.TryGetValue(crc, out var item))
        {
            item.RefCount++;
            item.lastUseTime = Time.realtimeSinceStartup;
//            if (item.RefCount <= 1)
//            {
//                unRefAseetItems.Remove(item);
//            }
        }

        return item;
    }
}

public class DoubleLinkedListNode<T> where T : class
{
    public DoubleLinkedListNode<T> prev;

    public DoubleLinkedListNode<T> next;

    public T t = null;
}

public class DoubleLinkedList<T> where T : class
{
    public DoubleLinkedListNode<T> Head;
    public DoubleLinkedListNode<T> Tail;


    protected ClassObjectPool<DoubleLinkedListNode<T>> DoubleLinkNodePool =
        ObjectPoolManager.Instance.GetOrCreateClassPool<DoubleLinkedListNode<T>>(Capacity.DoubleLinkedListNode);

    protected int count = 0;
    public int Count => count;

    public DoubleLinkedListNode<T> AddToHeader(T t)
    {
        DoubleLinkedListNode<T> pList = DoubleLinkNodePool.Spawn();
        pList.next = null;
        pList.prev = null;
        pList.t = t;
        return AddToHeader(pList);
    }

    public DoubleLinkedListNode<T> AddToHeader(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null)
        {
            return null;
        }

        pNode.prev = null;
        if (Head == null)
        {
            Head = Tail = pNode;
        }
        else
        {
            pNode.next = Head;
            Head.prev = pNode;
            Head = pNode;
        }

        count++;
        return pNode;
    }

    public DoubleLinkedListNode<T> AddToTail(T t)
    {
        DoubleLinkedListNode<T> pList = DoubleLinkNodePool.Spawn();
        pList.next = null;
        pList.prev = null;
        pList.t = t;
        return AddToTail(pList);
    }

    public DoubleLinkedListNode<T> AddToTail(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null)
        {
            return null;
        }

        pNode.next = null;
        if (Tail == null)
        {
            Head = Tail = pNode;
        }
        else
        {
            pNode.prev = Tail;
            Tail.next = pNode;
            Tail = pNode;
        }

        count++;
        return pNode;
    }

    public void Remove(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null) return;

//        if (pNode.prev != null)
//        {
//            if (pNode == Tail)
//            {
//                Tail = pNode.prev;
//                Tail.next = null;
//            }
//            else if (pNode.next != null)
//            {
//                pNode.prev.next = pNode.next;
//                pNode.next.prev = pNode.prev;
//            }
//        }
//        else
//        {
//            Head = pNode.next;
//            pNode.next.prev = null;
//        }

        if (pNode == Head)
        {
            Head = pNode.next;
        }

        if (pNode == Tail)
        {
            Tail = pNode.prev;
        }

        if (pNode.prev != null)
        {
            pNode.prev.next = pNode.next;
        }

        if (pNode.next != null)
        {
            pNode.next.prev = pNode.prev;
        }

        pNode.next = pNode.prev = null;
        pNode.t = null;
        DoubleLinkNodePool.Recycle(pNode);
        count--;
    }

    public void MoveToHead(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null || pNode == Head)
        {
            return;
        }

        if (pNode.prev == null && pNode.next == null)
        {
            return;
        }

        if (pNode == Tail)
        {
            Tail = pNode.prev;
        }

        if (pNode.prev != null)
        {
            pNode.prev.next = pNode.next;
        }

        if (pNode.next != null)
        {
            pNode.next.prev = pNode.prev;
        }

        pNode.prev = null;
        pNode.next = Head;
        Head.prev = pNode;
        Head = pNode;
        if (Tail == null)
        {
            Tail = Head;
        }
    }
}

public class CMapList<T> where T : class, new()
{
    private DoubleLinkedList<T> doubleLinkedList = new DoubleLinkedList<T>();
    private Dictionary<T, DoubleLinkedListNode<T>> findMap = new Dictionary<T, DoubleLinkedListNode<T>>();

    ~CMapList()
    {
        Clear();
    }

    public void Clear()
    {
        while (doubleLinkedList.Tail != null)
        {
            Remove(doubleLinkedList.Tail.t);
        }
    }

    public void InsertToHead(T t)
    {
        if (findMap.TryGetValue(t, out var node))
        {
            doubleLinkedList.AddToHeader(node);
            return;
        }

        doubleLinkedList.AddToHeader(t);
        findMap.Add(t, doubleLinkedList.Head);
    }

    public void Pop()
    {
        if (doubleLinkedList.Tail != null)
        {
            Remove(doubleLinkedList.Tail.t);
        }
    }

    public void Remove(T t)
    {
        if (!findMap.TryGetValue(t, out var node))
        {
            return;
        }

        doubleLinkedList.Remove(node);
        findMap.Remove(t);
    }

    public T Back()
    {
        return doubleLinkedList.Tail.t;
    }

    public int Size()
    {
        return findMap.Count;
    }

    public bool Find(T t)
    {
        if (!findMap.TryGetValue(t, out var node))
            return false;
        return true;
    }

    public bool Refresh(T t)
    {
        if (!findMap.TryGetValue(t, out var node))
            return false;
        doubleLinkedList.MoveToHead(node);
        return true;
    }
}