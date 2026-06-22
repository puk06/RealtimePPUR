using System;
using System.Collections.Generic;

namespace RealtimePPUR.Services;

public static class ProcessIntPtrManager
{
    private static readonly Dictionary<Type, IntPtr> _processIntPtrs = new();

    public static void Register(Type type, IntPtr ptr)
    {
        _processIntPtrs[type] = ptr;
    }
    
    public static IntPtr? Get(Type type)
    {
        if (!_processIntPtrs.TryGetValue(type, out IntPtr ptr)) return null;
        return ptr;
    }

    public static bool HasValue(IntPtr ptr)
    {
        return _processIntPtrs.ContainsValue(ptr);
    }

    public static bool HasKey(Type type)
    {
        return _processIntPtrs.ContainsKey(type);
    }
}
