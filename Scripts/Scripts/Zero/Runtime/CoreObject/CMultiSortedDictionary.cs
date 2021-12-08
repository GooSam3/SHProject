using System.Collections.Generic;

public class CMultiSortedDictionary<Key, Value>
{
    private SortedDictionary<Key, List<Value>> dic_ = null;

    public CMultiSortedDictionary()
    {
        dic_ = new SortedDictionary<Key, List<Value>>();
    }

    public CMultiSortedDictionary(IComparer<Key> comparer)
    {
        dic_ = new SortedDictionary<Key, List<Value>>(comparer);
    }

    public void Add(Key key, Value value)
    {
        List<Value> list = null;

        if (dic_.TryGetValue(key, out list))
        {
            list.Add(value);
        }
        else
        {
            list = new List<Value>();
            list.Add(value);
            dic_.Add(key, list);
        }
    }

    public bool ContainsKey(Key key)
    {
        return dic_.ContainsKey(key);
    }

	public bool Remove(Key key)
	{
		return dic_.Remove(key);
	}

    public void Clear()
    {
        dic_.Clear();
    }

    public List<Value> this[Key key]
    {
        get
        {
            List<Value> list = null;
            if (!dic_.TryGetValue(key, out list))
            {
                list = new List<Value>();
                dic_.Add(key, list);
            }

            return list;
        }
    }

    public IEnumerable<Key> keys
    {
        get
        {
            return dic_.Keys;
        }
    }

    public IEnumerable<List<Value>> value
    {
        get
        {
            return dic_.Values;
        }        
    }
   
}
