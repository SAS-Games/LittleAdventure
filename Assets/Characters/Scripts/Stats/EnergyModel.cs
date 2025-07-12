using SAS.Utilities.TagSystem;

public interface IEnergyModel : IStatModel
{
}

public class EnergyModel : StatBase, IEnergyModel
{
    public EnergyModel(IContextBinder _) : base()
    {
    }
}