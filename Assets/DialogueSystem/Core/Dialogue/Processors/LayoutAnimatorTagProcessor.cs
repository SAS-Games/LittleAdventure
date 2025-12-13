namespace SAS.DialogueSystem
{
    public class LayoutAnimatorTagProcessor : BaseTagProcessor
    {
        public string LayoutAnim { get; private set; } = string.Empty;
       
        public override void Process(string tagValue, TagProcessContext context)
        {
            LayoutAnim = tagValue;
        }

        public override void Reset()
        {
            base.Reset();
            LayoutAnim = string.Empty;
        }
    }
}