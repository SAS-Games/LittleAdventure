namespace SAS.Collectables
{
    public interface ICollectable
    {
        void Collect(Collector collector);
    }

    public interface ICollectable<in T> : ICollectable where T : ICollectable<T>
    {
    }
}
