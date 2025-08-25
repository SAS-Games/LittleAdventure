INCLUDE LA_common.ink

-> ROPE_TUTORIAL

=== ROPE_TUTORIAL ===
{isCoop:
    -> COOP_ROPE_TUTORIAL
- else:
    -> SINGLEPLAYER_ROPE_TUTORIAL
}

=== COOP_ROPE_TUTORIAL ===
Hear me, adventurers! #speaker:id::npc, name::KAIROS, anim::Talk
Before you lies a great rope challenge. #speaker:id::npc, name::KAIROS, anim::Talk
Use the [{ropeKey}] key to pull the rope. #speaker:id::npc, name::KAIROS, anim::Talk
If both of you pull together, the rope will move faster and victory will come swiftly! #speaker:id::npc, name::KAIROS, anim::Talk
-> END

=== SINGLEPLAYER_ROPE_TUTORIAL ===
Brave adventurer! #speaker:id::npc, name::KAIROS, anim::Talk
Use the [{ropeKey}] key to pull the rope and overcome obstacles. #speaker:id::npc, name::KAIROS, anim::Talk
-> END
