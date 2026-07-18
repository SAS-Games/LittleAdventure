INCLUDE LA_common.ink

-> ROPE_TUTORIAL

=== ROPE_TUTORIAL ===
{isCoop:
    -> COOP_ROPE_TUTORIAL
- else:
    -> SINGLEPLAYER_ROPE_TUTORIAL
}

=== COOP_ROPE_TUTORIAL ===
Hear me, adventurers! #speaker:npc #speaker_name:KAIROS #animation:Talk
Before you lies a great rope challenge. #speaker:npc #speaker_name:KAIROS #animation:Talk
Use the [{ropeKey}] key to pull the rope. #speaker:npc #speaker_name:KAIROS #animation:Talk
If both of you pull together, the rope will move faster and victory will come swiftly! #speaker:npc #speaker_name:KAIROS #animation:Talk
-> END

=== SINGLEPLAYER_ROPE_TUTORIAL ===
Brave adventurer! #speaker:npc #speaker_name:KAIROS #animation:Talk
Use the [{ropeKey}] key to pull the rope and overcome obstacles. #speaker:npc #speaker_name:KAIROS #animation:Talk
-> END
