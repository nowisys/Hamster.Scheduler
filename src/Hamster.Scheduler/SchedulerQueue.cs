using System;
using System.Collections.Generic;

namespace Hamster.Scheduler
{
  public class SchedulerQueue : IList<ISchedulerItem>
  {
    private List<ISchedulerItem> items = new List<ISchedulerItem>();
    private List<ISchedulerItem> itemHeap = new List<ISchedulerItem>();
    private Dictionary<string, int> itemHeapIndex = new Dictionary<string, int>();

    private void SinkItem(int heapIndex)
    {
      int childIndex = heapIndex * 2 + 1;
      while (childIndex < itemHeap.Count)
      {
        if (childIndex + 1 < itemHeap.Count
                                          && ItemFromHeapIndex(childIndex).NextStart > ItemFromHeapIndex(childIndex + 1).NextStart)
        {
          childIndex++;
        }

        if (ItemFromHeapIndex(heapIndex).NextStart <= ItemFromHeapIndex(childIndex).NextStart)
          break;

        SwapHeap(heapIndex, childIndex);

        heapIndex = childIndex;
        childIndex = heapIndex * 2 + 1;
      }
    }

    private void RaiseItem(int heapIndex)
    {
      int parentIndex;
      while (heapIndex > 0)
      {
        parentIndex = (heapIndex - 1) / 2;
        if (ItemFromHeapIndex(parentIndex).NextStart <= ItemFromHeapIndex(heapIndex).NextStart)
          break;

        SwapHeap(heapIndex, parentIndex);

        heapIndex = parentIndex;
      }
    }

    private ISchedulerItem ItemFromHeapIndex(int heapIndex)
    {
      return itemHeap[heapIndex];
    }

    private void SwapHeap(int a, int b)
    {
      ISchedulerItem buff = itemHeap[a];
      itemHeap[a] = itemHeap[b];
      itemHeap[b] = buff;

      itemHeapIndex[ItemFromHeapIndex(a).Name] = a;
      itemHeapIndex[ItemFromHeapIndex(b).Name] = b;
    }

    protected virtual void ItemIncreased(object sender, EventArgs args)
    {
      ISchedulerItem item = (ISchedulerItem)sender;
      SinkItem(itemHeapIndex[item.Name]);

      OnItemsChanged(EventArgs.Empty);
    }

    public ISchedulerItem Top()
    {
      if (items.Count == 0)
        return null;
      else
        return ItemFromHeapIndex(0);
    }

    public event EventHandler ItemsChanged;

    protected virtual void OnItemsChanged(EventArgs args)
    {
      EventHandler handler = ItemsChanged;
      if (handler != null)
      {
        handler(this, args);
      }
    }

    public bool TryGetValue(string name, out ISchedulerItem value)
    {
      int heapIndex;

      if (!itemHeapIndex.TryGetValue(name, out heapIndex))
      {
        value = null;
        return false;
      }

      value = ItemFromHeapIndex(heapIndex);
      return true;
    }

    public ISchedulerItem this[string name]
    {
      get { return ItemFromHeapIndex(itemHeapIndex[name]); }
    }

    public int IndexOf(ISchedulerItem item)
    {
      return items.IndexOf(item);
    }

    public virtual void Insert(int index, ISchedulerItem item)
    {
      items.Insert(index, item);

      int heapIndex = itemHeap.Count;
      itemHeap.Add(item);

      itemHeapIndex[item.Name] = heapIndex;

      RaiseItem(heapIndex);

      item.Increased += ItemIncreased;

      OnItemsChanged(EventArgs.Empty);
    }

    public void RemoveAt(int index)
    {
      Remove(items[index]);
    }

    public ISchedulerItem this[int index]
    {
      get
      {
        return items[index];
      }
      set
      {
        items[index].Increased -= ItemIncreased;
        value.Increased += ItemIncreased;

        items[index] = value;
        SinkItem(index);
        RaiseItem(index);

        OnItemsChanged(EventArgs.Empty);
      }
    }

    public void Add(ISchedulerItem item)
    {
      Insert(items.Count, item);
    }

    public void Clear()
    {
      foreach (ISchedulerItem item in items)
        item.Increased -= ItemIncreased;

      items.Clear();
      itemHeap.Clear();
      itemHeapIndex.Clear();

      OnItemsChanged(EventArgs.Empty);
    }

    public bool Contains(ISchedulerItem item)
    {
      int heapIndex;
      if (!itemHeapIndex.TryGetValue(item.Name, out heapIndex))
        return false;
      return ItemFromHeapIndex(heapIndex) == item;
    }

    public void CopyTo(ISchedulerItem[] array, int arrayIndex)
    {
      items.CopyTo(array, arrayIndex);
    }

    public int Count
    {
      get { return items.Count; }
    }

    bool ICollection<ISchedulerItem>.IsReadOnly
    {
      get { return false; }
    }

    public virtual bool Remove(ISchedulerItem item)
    {
      int heapIndex;
      if (!itemHeapIndex.TryGetValue(item.Name, out heapIndex))
        return false;

      if (ItemFromHeapIndex(heapIndex) != item)
        return false;

      item.Increased -= ItemIncreased;

      SwapHeap(heapIndex, itemHeap.Count - 1);

      items.Remove(item);
      itemHeap.RemoveAt(itemHeap.Count - 1);
      itemHeapIndex.Remove(item.Name);

      if (heapIndex < Count)
      {
        SinkItem(heapIndex);
        RaiseItem(heapIndex);
      }

      OnItemsChanged(EventArgs.Empty);

      return true;
    }

    public IEnumerator<ISchedulerItem> GetEnumerator()
    {
      return items.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}
