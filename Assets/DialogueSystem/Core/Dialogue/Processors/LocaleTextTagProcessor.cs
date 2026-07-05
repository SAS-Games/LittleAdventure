public class LocaleTextTagProcessor : BaseTagProcessor
{
    public string Locale { get; private set; } = string.Empty;

    public override bool CanHandle(string tagKey)
    {
        return KeyEquals(tagKey, "local") || KeyEquals(tagKey, "locale") || base.CanHandle(tagKey);
    }

    public override void Process(string tagValue, TagProcessContext context)
    {
        Locale = tagValue;
        context.CurrentLine.SetLocale(tagValue);
    }

    public override void Reset()
    {
        base.Reset();
        Locale = string.Empty;
    }
}
