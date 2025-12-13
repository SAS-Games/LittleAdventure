public class LocaleTextTagProcessor : BaseTagProcessor
{
    public string Locale { get; private set; } = string.Empty;
    public override void Process(string tagValue, TagProcessContext context)
    {
        Locale = tagValue;
    }

    public override void Reset()
    {
        base.Reset();
        Locale = string.Empty;
    }
}