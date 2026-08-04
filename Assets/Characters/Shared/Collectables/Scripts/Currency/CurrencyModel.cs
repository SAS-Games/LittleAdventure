using SAS.Core.TagSystem;
using UniRx;

public interface ICurrencyModel: IBindable
{
    public ReactiveDictionary<string, int> Value { get; }

    public IReadOnlyReactiveProperty<int> GetValue(string _currencyType);

}

public class CurrencyModel : ICurrencyModel
{
    public ReactiveDictionary<string, int> Value { get; } = new ReactiveDictionary<string, int>();
    public IReadOnlyReactiveProperty<int> GetValue(string _currencyType)
    {
        IReadOnlyReactiveProperty<int> value = Value.ObserveReplace()
            .Where(evt => evt.Key == _currencyType).Select(evt => evt.NewValue)
            .ToReactiveProperty(Value[_currencyType]);

        return value;
    }
    
    public CurrencyModel(IContextBinder _){}
}