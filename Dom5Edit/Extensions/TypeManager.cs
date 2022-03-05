using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TypeManager
{
    public static Dictionary<Type, Dictionary<string, Type>> CodexOfAllLife = new Dictionary<Type, Dictionary<string, Type>>();

    private static TypeManager _instance;
    public static TypeManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new TypeManager();
            if (_instance.Initialized)
                return _instance;
            _instance.Initialized = true;
            _instance.Initialize();
            return _instance;
        }
    }

    public bool Initialized { get; private set; }

    public void Initialize()
    {
    }

    protected Dictionary<string, Type> Harvest<T>()
    {
        var type = typeof(T);
        Dictionary<string, Type> dict;
        if (CodexOfAllLife.TryGetValue(type, out dict)) return dict;

        dict = new Dictionary<string, Type>();
        CodexOfAllLife[type] = dict;
        var types = type.GetSubclassesOf();
        foreach (var t in types)
        {
            dict.Add(t.ToString().ToUpper(), t);
        }
        return dict;
    }

    public List<Type> GetSubclasses<T>()
    {
        return Harvest<T>().Values.ToList();
    }

    public List<T> GetInstantiations<T>()
    {
        var ret = new List<T>();
        foreach (var t in GetSubclasses<T>())
        {
            ret.Add((T)Activator.CreateInstance(t));
        }
        return ret;
    }

    public bool TryGetValue<T>(string key, out Type type)
    {
        var dict = Harvest<T>();
        return dict.TryGetValue(key.ToUpper(), out type);
    }

    public Type Get<T>(string key)
    {
        Type ret;
        TryGetValue<T>(key, out ret);
        return ret;
    }

    public T Instantiate<T>(string key)
    {
        T item;
        TryInstantiate(key, out item);
        return item;
    }

    public bool TryInstantiate<T>(string key, out T item)
    {
        Type t;
        if (TryGetValue<T>(key, out t))
        {
            item = (T)Activator.CreateInstance(Get<T>(key));
            return true;
        }
        item = default(T);
        return false;
    }
}