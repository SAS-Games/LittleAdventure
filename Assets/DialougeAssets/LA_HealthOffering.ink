INCLUDE LA_globals.ink

VAR player1_coins = 25
VAR player2_coins = 10
VAR cost = 20
VAR health_bonus = 25

{isCoop:
->NPC_EXTRA_HEALTH
- else:
   ->SINGLEPLAYER_NPC_EXTRA_HEALTH
}

=== NPC_EXTRA_HEALTH ===
Hello, brave ones! I have a special herbal tonic that will grant you +{health_bonus} max health. But it will cost {cost} coins per adventurer.
 -> CHECK_ELIGIBILITY

=== SINGLEPLAYER_NPC_EXTRA_HEALTH ===
Hello, brave ones! I have a special herbal tonic that will grant you +{health_bonus} max health. But it will cost {cost} coins for you.
 -> SINGLEPLAYER_ELIGIBILITY

=== SINGLEPLAYER_ELIGIBILITY ===
~ temp p1_has_enough = player1_coins >= cost

{p1_has_enough:
    You can afford it.
    -> OFFER_SINGLE
- else:
    Looks like you can't afford the tonic. Come back when you have more coins!
    -> END
}

=== CHECK_ELIGIBILITY ===
~ temp p1_has_enough = player1_coins >= cost
~ temp p2_has_enough = player2_coins >= cost

{p1_has_enough && p2_has_enough:
    Both of you can afford it.
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
            Looks like neither of you can afford the tonic.
            Come back when you have more coins!
            -> END
        }
    }
}

=== OFFER_SINGLE ===
Would you like to buy the tonic for {cost} coins?
+ [Yes, give me the tonic]
    ~ player1_coins = player1_coins - cost
    (You feel healthier!)
    -> END
+ [No, I’ll pass]
    Very well, maybe another time.
    -> END

=== OFFER_BOTH ===
Would you both like to buy the tonic for {cost} coins each?
+ [Yes, both take it]
    ~ player1_coins = player1_coins - cost
    ~ player2_coins = player2_coins - cost
    (Both players feel healthier!)
    -> END
+ [No, maybe later]
    Very well, maybe another time.
    -> END

=== OFFER_P1_ONLY ===
{Player1_name}, would you like to buy the tonic for {cost} coins?
+ [Yes, give me the tonic]
    ~ player1_coins = player1_coins - cost
    ({Player1_name} feels healthier!)
    -> END
+ [No, I’ll pass]
    Very well, maybe another time.
    -> END

=== OFFER_P2_ONLY ===
{Player2_name}, would you like to buy the tonic for {cost} coins?
+ [Yes, give me the tonic]
    ~ player2_coins = player2_coins - cost
    ({Player2_name} feels healthier!)
    -> END
+ [No, I’ll pass]
    Very well, maybe another time.
    -> END
