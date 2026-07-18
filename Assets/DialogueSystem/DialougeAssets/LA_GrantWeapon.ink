INCLUDE LA_common.ink

VAR weapon_name = "Blade Of Dawn"
EXTERNAL grant_weapon(player_name, weaponName)

{isCoop:
-> NPC_UNLOCK_WEAPON
- else:
   -> SINGLEPLAYER_NPC_UNLOCK_WEAPON
}

=== NPC_UNLOCK_WEAPON ===
Greetings, adventurers! I bestow upon you the legendary {weapon_name}.#speaker:npc #speaker_name:KAIROS #animation:Talk
~ grant_weapon(Player1_name, weapon_name)
~ grant_weapon(Player2_name, weapon_name)
(Both players now wield the {weapon_name}!) #speaker:npc #speaker_name:KAIROS #animation:Talk
To attack, press {attackButton}. #speaker:npc #speaker_name:KAIROS #animation:Talk
-> END

=== SINGLEPLAYER_NPC_UNLOCK_WEAPON ===
Greetings, adventurer! I bestow upon you the legendary {weapon_name}.#speaker:npc #speaker_name:KAIROS #animation:Talk
~ grant_weapon(Player1_name, weapon_name)
(You now wield the {weapon_name}!) #speaker:npc #speaker_name:KAIROS #animation:Talk
To attack, press {attackButton}. #speaker:npc #speaker_name:KAIROS #animation:Talk
-> END
