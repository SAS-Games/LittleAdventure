using System.Collections.Generic;

namespace SAS.WorldStreaming.Editor
{
    public interface IWorldRegionProvider
    {
        IReadOnlyList<StreamingRegionDefinition> Generate(WorldStreamingProfile profile);
    }
}
