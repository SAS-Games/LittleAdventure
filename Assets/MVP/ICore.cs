using SAS.StateMachineGraph;
using SAS.Utilities.TagSystem;
using System;
using System.Collections.Generic;

public interface ICore
{
    void Init();
    bool TryGet<T>(out T instance, Tag tag = Tag.None);
    public IEnumerable<T> GetAll<T>(Tag tag = Tag.None);
    void Add<T>(object instance, Tag tag = Tag.None);
    void Add(Type type, object instance, Tag tag = Tag.None);

}
