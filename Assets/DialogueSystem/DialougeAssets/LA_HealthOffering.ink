INCLUDE LA_common.ink

VAR cost = 2
VAR health_bonus = 25
EXTERNAL grant_health(player_name, cost, healthBonus)

{isCoop:
->NPC_EXTRA_HEALTH
- else:
   ->SINGLEPLAYER_NPC_EXTRA_HEALTH
}

=== NPC_EXTRA_HEALTH ===
Hello, brave ones! I have a special herbal tonic that will grant you +{health_bonus} max health. But it will cost {cost} coins per adventurer.#speaker:id::npc, name::KAIROS, anim::Talk
 -> CHECK_ELIGIBILITY

=== SINGLEPLAYER_NPC_EXTRA_HEALTH ===
Hello, brave ones! I have a special herbal tonic that will grant you +{health_bonus} max health. But it will cost {cost} coins for you.#speaker:id::npc, name::KAIROS, anim::Talk
 -> SINGLEPLAYER_ELIGIBILITY

=== SINGLEPLAYER_ELIGIBILITY ===
~ temp p1_has_enough = player1_coins >= cost

{p1_has_enough:
    You can afford it.#speaker:id::npc, name::KAIROS, anim::Talk
    -> OFFER_SINGLE
- else:
    Looks like you can't afford the tonic. Come back when you have more coins!#speaker:id::npc, name::KAIROS, anim::Talk
    -> END
}

=== CHECK_ELIGIBILITY ===
~ temp p1_has_enough = player1_coins >= cost
~ temp p2_has_enough = player2_coins >= cost

{p1_has_enough && p2_has_enough:
    Both of you can afford it.#speaker:id::npc, name::KAIROS, anim::Talk
    -> OFFER_BOTH
- else: 
    {p1_has_enough && not p2_has_enough:
        {Player1_name}, you have enough coins, but {Player2_name} doesn't.
        -> OFFER_P1_ONLY
    - else:
        {not p1_has_enough && p2_has_enough:
            {Player2_name}, you have enough coins, but {Player1_name} doesn't.
            -> OFFER_P2_ONLY
        - else:
            Looks like neither of you can afford the tonic.#speaker:id::npc, name::KAIROS, anim::Talk
            Come back when you have more coins!#speaker:id::npc, name::KAIROS, anim::Talk
            -> END
        }
    }
}

=== OFFER_SINGLE ===
Would you like to buy the tonic for {cost} coins?#speaker:id::npc, name::KAIROS, anim::Talk
+ [Yes, give me the tonic]
    ~ grant_health(Player1_name, cost, health_bonus)
    (You feel healthier!) #speaker:id::npc, name::KAIROS, anim::Talk
    -> END
+ [No, I’ll pass]
    Very well, maybe another time. #speaker:id::npc, name::KAIROS, anim::Talk
    -> END

=== OFFER_BOTH ===
Would you both like to buy the tonic for {cost} coins each?
+ [Yes, both take it]
    ~ grant_health(Player1_name, cost, health_bonus)
    ~ grant_health(Player2_name, cost, health_bonus)
    (Both players feel healthier!) #speaker:id::npc, name::KAIROS, anim::Talk
    -> END
+ [No, maybe later]
    Very well, maybe another time. #speaker:id::npc, name::KAIROS, anim::Talk
    -> END

=== OFFER_P1_ONLY ===
{Player1_name}, would you like to buy the tonic for {cost} coins?
+ [Yes, give me the tonic]
    ~ grant_health(Player1_name, cost, health_bonus)
    ({Player1_name} feels healthier!)
    -> END
+ [No, I’ll pass]
    Very well, maybe another time.
    -> END

=== OFFER_P2_ONLY ===
{Player2_name}, would you like to buy the tonic for {cost} coins?
+ [Yes, give me the tonic]
    ~ grant_health(Player2_name, cost, health_bonus)
    ({Player2_name} feels healthier!)
    -> END
+ [No, I’ll pass]
    Very well, maybe another time.
    -> END
