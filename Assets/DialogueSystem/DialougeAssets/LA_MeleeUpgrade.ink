INCLUDE LA_common.ink

VAR cost = 3
VAR damage_bonus = 5
EXTERNAL grant_damage(player_name, cost, damageBonus)

{isCoop:
->NPC_EXTRA_DAMAGE
- else:
   ->SINGLEPLAYER_NPC_EXTRA_DAMAGE
}

=== NPC_EXTRA_DAMAGE ===
Greetings, warriors! I can offer you a powerful combat enhancement that will permanently increase your melee damage by +{damage_bonus}.  
But it will cost {cost} coins per adventurer. #speaker:id::npc, name::KAIROS, anim::Talk
 -> CHECK_DAMAGE_ELIGIBILITY

=== SINGLEPLAYER_NPC_EXTRA_DAMAGE ===
Greetings, warrior! I can offer you a powerful combat enhancement that will permanently increase your melee damage by +{damage_bonus}.  
But it will cost {cost} coins. #speaker:id::npc, name::KAIROS, anim::Talk
 -> SINGLEPLAYER_DAMAGE_ELIGIBILITY

=== SINGLEPLAYER_DAMAGE_ELIGIBILITY ===
~ temp p1_has_enough = player1_coins >= cost

{p1_has_enough:
    You can afford this enhancement. #speaker:id::npc, name::KAIROS, anim::Talk
    -> OFFER_SINGLE_DAMAGE
- else:
    It seems you don’t have enough coins for this. Come back when you do! #speaker:id::npc, name::KAIROS, anim::Talk
    -> END
}

=== CHECK_DAMAGE_ELIGIBILITY ===
~ temp p1_has_enough = player1_coins >= cost
~ temp p2_has_enough = player2_coins >= cost

{p1_has_enough && p2_has_enough:
    Both of you can afford this enhancement. #speaker:id::npc, name::KAIROS, anim::Talk
    -> OFFER_BOTH_DAMAGE
- else: 
    {p1_has_enough && not p2_has_enough:
        {Player1_name}, you have enough coins, but {Player2_name} doesn’t.
        -> OFFER_P1_DAMAGE_ONLY
    - else:
        {not p1_has_enough && p2_has_enough:
            {Player2_name}, you have enough coins, but {Player1_name} doesn’t.
            -> OFFER_P2_DAMAGE_ONLY
        - else:
            Neither of you can afford this enhancement. Return when you have more coins. #speaker:id::npc, name::KAIROS, anim::Talk
            -> END
        }
    }
}

=== OFFER_SINGLE_DAMAGE ===
Would you like to spend {cost} coins to gain +{damage_bonus} melee damage? #speaker:id::npc, name::KAIROS, anim::Talk
+ [Yes, give me the enhancement]
    ~ grant_damage(Player1_name, cost, damage_bonus)
    (You feel your strikes grow sharper!) #speaker:id::npc, name::KAIROS, anim::Talk
    -> END
+ [No, I’ll pass]
    Very well, maybe another time. #speaker:id::npc, name::KAIROS, anim::Talk
    -> END

=== OFFER_BOTH_DAMAGE ===
Would you both like to spend {cost} coins each to gain +{damage_bonus} melee damage? 
+ [Yes, enhance us both]
    ~ grant_damage(Player1_name, cost, damage_bonus)
    ~ grant_damage(Player2_name, cost, damage_bonus)
    (Both warriors feel their strikes grow sharper!) #speaker:id::npc, name::KAIROS, anim::Talk
    -> END
+ [No, maybe later]
    Very well, maybe another time. #speaker:id::npc, name::KAIROS, anim::Talk
    -> END

=== OFFER_P1_DAMAGE_ONLY ===
{Player1_name}, would you like to spend {cost} coins to gain +{damage_bonus} melee damage?
+ [Yes, give me the enhancement]
    ~ grant_damage(Player1_name, cost, damage_bonus)
    ({Player1_name} feels their strikes grow sharper!)
    -> END
+ [No, I’ll pass]
    Very well, maybe another time.
    -> END

=== OFFER_P2_DAMAGE_ONLY ===
{Player2_name}, would you like to spend {cost} coins to gain +{damage_bonus} melee damage?
+ [Yes, give me the enhancement]
    ~ grant_damage(Player2_name, cost, damage_bonus)
    ({Player2_name} feels their strikes grow sharper!)
    -> END
+ [No, I’ll pass]
    Very well, maybe another time.
    -> END
