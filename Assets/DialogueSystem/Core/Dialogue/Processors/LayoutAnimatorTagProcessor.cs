namespace SAS.DialogueSystem
{
    public class LayoutAnimatorTagProcessor : BaseTagProcessor
    {
        public string LayoutAnim { get; private set; } = string.Empty;

        public override bool CanHandle(string tagKey)
        {
            return KeyEquals(tagKey, "layout") || base.CanHandle(tagKey);
        }
       
        public override void Process(string tagValue, TagProcessContext context)
        {
            LayoutAnim = tagValue;
            context.CurrentLine.SetLayoutAnim(tagValue);
        }

        public override void Reset()
        {
            base.Reset();
            LayoutAnim = string.Empty;
        }
    }
}
