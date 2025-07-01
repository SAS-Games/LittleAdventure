using System.Collections.Generic;
using UnityEngine;

public class SpeakerState
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public Sprite Portrait { get; set; }
    public string Emotion { get; set; }
    public Vector2 Position { get; set; }

    public void UpdateFromTags(Dictionary<string, string> tags)
    {
        if (tags.TryGetValue("emotion", out var emotion))
            Emotion = emotion;
        // ... other tag updates
    }
}