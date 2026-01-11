using System;
using System.Collections.Generic;
using SAS.Core.TagSystem;

public interface ICore
{
    void Init();
    bool TryGet<T>(out T instance, Tag tag = default);
    public IEnumerable<T> GetAll<T>(Tag tag = default);
    void Add<T>(object instance, Tag tag = default);
    void Add(Type type, object instance, Tag tag = default);

}
