using SAS.Core.TagSystem;

public interface IEnergyModel : IStatModel,IBindable
{
}

public class EnergyModel : StatBase, IEnergyModel
{
    public EnergyModel(IContextBinder _)
    {
    }
}