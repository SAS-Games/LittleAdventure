using SAS.StateMachineGraph;
using SAS.Utilities.TagSystem;
using System;

public interface ICore
{
    void Init();
    bool TryGet<T>(out T instance, Tag tag = Tag.None);

    void Add<T>(object instance, Tag tag = Tag.None);
    void Add(Type type, object instance, Tag tag = Tag.None);

}
