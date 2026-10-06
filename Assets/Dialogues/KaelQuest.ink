// The Bloomblade Delivery
// Gameplay state, external methods, and variables are supplied by KaelQuestInkBinding.

EXTERNAL kael_quest_state()
EXTERNAL kael_is_first_conversation()
EXTERNAL kael_has_moonbloom_oil()
EXTERNAL kael_mark_conversation_started()
EXTERNAL kael_try_deliver()
EXTERNAL kael_last_reward()

VAR kael_requested_item = "Moonbloom Oil"

// Used only by Ink's editor Player window. In the game, KaelQuestInkBinding
// overrides the external functions below with the real quest and inventory state.
VAR preview_kael_quest_state = 1
VAR preview_kael_first_conversation = true
VAR preview_kael_has_moonbloom_oil = false
VAR preview_kael_delivery_result = 1
VAR preview_kael_reward = 150

~ temp quest_state = kael_quest_state()

{ quest_state:
- 1:
    { kael_is_first_conversation():
        -> first_conversation
    - else:
        -> return_conversation
    }
- 2:
    -> after_completion
- 3:
    -> after_failure
- else:
    -> END
}

=== first_conversation ===
# id:kael.first.guild_sent
# speaker:kael
# listener:player
You're the one the guild sent?

# id:kael.first.player_confirm
# speaker:player
# listener:kael
That's me. What do you need?

# id:kael.first.thornhide
# speaker:kael
# listener:player
The Thornhide attacks at dusk. Normal steel barely cuts through its hide.

# id:kael.first.need_oil
# speaker:kael
# listener:player
I need {kael_requested_item} for my weapon before it gets here.

# id:kael.first.instructions
# speaker:kael
# listener:player
Find a Moonbloom Flower, prepare the oil, and bring it back to me.

~ kael_mark_conversation_started()

# id:kael.first.have_oil_question
# speaker:kael
# listener:player
Do you already have the oil?

{ kael_has_moonbloom_oil():
    -> first_has_oil
- else:
    -> first_no_oil
}

=== first_no_oil ===
+ [Not yet. I'll get it. # id:kael.choice.first.not_yet]
    # id:kael.first.not_yet.player
    # speaker:player
    # listener:kael
    Not yet. I'll bring it back.
    # id:kael.first.not_yet.reply
    # speaker:kael
    # listener:player
    Then hurry. We don't have much time.
    -> END
+ [Remind me what you need. # id:kael.choice.first.remind] -> first_reminder
+ [Leave. # id:kael.choice.first.leave] -> END

=== first_reminder ===
# id:kael.first.reminder.player
# speaker:player
# listener:kael
What exactly do you need again?

# id:kael.first.reminder.reply
# speaker:kael
# listener:player
{kael_requested_item}. You'll need a Moonbloom Flower to make it.

    ++ [I'll get it. # id:kael.choice.first.reminder_accept]
    # id:kael.first.reminder.accept_player
    # speaker:player
    # listener:kael
    Alright. I'll bring it back.
    # id:kael.first.reminder.accept_reply
    # speaker:kael
    # listener:player
    Good. Don't take too long.
    -> END
    ++ [Leave. # id:kael.choice.first.reminder_leave] -> END

=== first_has_oil ===
+ [\[Give {kael_requested_item}\] I already have it. # id:kael.choice.first.give_oil]
    # id:kael.first.give.player
    # speaker:player
    # listener:kael
    I already have it. Here.
    -> attempt_delivery
+ [Not yet. I'll bring it later. # id:kael.choice.first.keep_oil]
    # id:kael.first.keep.player
    # speaker:player
    # listener:kael
    I have it, but I'm not handing it over yet.
    # id:kael.first.keep.reply
    # speaker:kael
    # listener:player
    Fine. Just don't wait too long.
    -> END
+ [Leave. # id:kael.choice.first.has_oil_leave] -> END

=== return_conversation ===
# id:kael.return.greeting
# speaker:kael
# listener:player
You're back. Do you have the {kael_requested_item}?

{ kael_has_moonbloom_oil():
    -> return_has_oil
- else:
    -> return_no_oil
}

=== return_no_oil ===
+ [Not yet. I'm still looking. # id:kael.choice.return.not_yet]
    # id:kael.return.not_yet.player
    # speaker:player
    # listener:kael
    Not yet. I'm still looking.
    # id:kael.return.not_yet.reply
    # speaker:kael
    # listener:player
    Then don't waste time. Bring it to me as soon as you have it.
    -> END
+ [Remind me what you need. # id:kael.choice.return.remind]
    # id:kael.return.reminder.player
    # speaker:player
    # listener:kael
    Remind me what you need.
    # id:kael.return.reminder.reply
    # speaker:kael
    # listener:player
    {kael_requested_item}. Find a Moonbloom Flower and prepare it as oil.
    ++ [Got it. # id:kael.choice.return.reminder_got_it]
        # id:kael.return.reminder.got_it
        # speaker:player
        # listener:kael
        Got it.
        -> END
    ++ [Leave. # id:kael.choice.return.reminder_leave] -> END
+ [Leave. # id:kael.choice.return.leave] -> END

=== return_has_oil ===
+ [\[Give {kael_requested_item}\] I have it here. # id:kael.choice.return.give_oil]
    # id:kael.return.give.player
    # speaker:player
    # listener:kael
    I have it here.
    -> attempt_delivery
+ [Not yet. # id:kael.choice.return.keep_oil]
    # id:kael.return.keep.player
    # speaker:player
    # listener:kael
    Not yet. I'll bring it when I'm ready.
    # id:kael.return.keep.reply
    # speaker:kael
    # listener:player
    Alright. Come back when you're ready.
    -> END
+ [Leave. # id:kael.choice.return.has_oil_leave] -> END

=== attempt_delivery ===
~ temp delivery_result = kael_try_deliver()
{ delivery_result:
- 1:
    -> clean_success
- 2:
    -> damaged_success
- 3:
    -> expired_failure
- 4:
    -> unusable_failure
- else:
    -> delivery_item_missing
}

=== delivery_item_missing ===
# id:kael.delivery.item_missing
# speaker:kael
# listener:player
You don't have the {kael_requested_item} anymore. Come back when you do.
-> END

=== clean_success ===
~ temp reward = kael_last_reward()
# id:kael.delivery.clean.player
# speaker:player
# listener:kael
Here's the {kael_requested_item} you asked for!

# id:kael.delivery.clean.condition
# speaker:kael
# listener:player
Good. It's still in excellent condition.

# id:kael.delivery.clean.weapon
# speaker:kael
# listener:player
This should be enough to get through the Thornhide's armor.

# id:kael.delivery.clean.reward
# speaker:kael
# listener:player
You made good time. Here's your payment: {reward} Coins.

# id:kael.delivery.clean.end
# speaker:kael
# listener:player
Leave the rest to me.
-> END

=== damaged_success ===
~ temp reward = kael_last_reward()
# id:kael.delivery.damaged.player
# speaker:player
# listener:kael
I brought the oil.

# id:kael.delivery.damaged.condition
# speaker:kael
# listener:player
I see this took some damage on the way here.

# id:kael.delivery.damaged.accepted
# speaker:kael
# listener:player
It's not ideal, but I can still use it.

# id:kael.delivery.damaged.reward
# speaker:kael
# listener:player
I'll pay you {reward} Coins, but not the full amount.

# id:kael.delivery.damaged.end
# speaker:kael
# listener:player
I'll make this work.
-> END

=== expired_failure ===
# id:kael.delivery.expired.player
# speaker:player
# listener:kael
I brought the {kael_requested_item}.

# id:kael.delivery.expired.now
# speaker:kael
# listener:player
Now?

# id:kael.delivery.expired.attack
# speaker:kael
# listener:player
The attack already started. I needed this before dusk.

# id:kael.delivery.expired.end
# speaker:kael
# listener:player
Keep it. The job is over.
-> END

=== unusable_failure ===
# id:kael.delivery.unusable.player
# speaker:player
# listener:kael
I brought the oil.

# id:kael.delivery.unusable.what_happened
# speaker:kael
# listener:player
What happened to this?

# id:kael.delivery.unusable.ruined
# speaker:kael
# listener:player
It's practically ruined.

# id:kael.delivery.unusable.cannot_use
# speaker:kael
# listener:player
I can't use this against the Thornhide.

# id:kael.delivery.unusable.end
# speaker:kael
# listener:player
The guild can explain why their delivery arrived like this.
-> END

=== after_completion ===
# id:kael.after_completion
# speaker:kael
# listener:player
The oil did its job. Thanks for getting it here.
-> END

=== after_failure ===
# id:kael.after_failure
# speaker:kael
# listener:player
The job is over. There's nothing more to discuss.
-> END

// Standalone Ink Player fallbacks -------------------------------------------

=== function kael_quest_state() ===
~ return preview_kael_quest_state

=== function kael_is_first_conversation() ===
~ return preview_kael_first_conversation

=== function kael_has_moonbloom_oil() ===
~ return preview_kael_has_moonbloom_oil

=== function kael_mark_conversation_started() ===
~ preview_kael_first_conversation = false

=== function kael_try_deliver() ===
{
- not preview_kael_has_moonbloom_oil:
    ~ return 0
- else:
    ~ preview_kael_has_moonbloom_oil = false
    {
    - preview_kael_delivery_result == 1 || preview_kael_delivery_result == 2:
        ~ preview_kael_quest_state = 2
    - else:
        ~ preview_kael_quest_state = 3
    }
    ~ return preview_kael_delivery_result
}

=== function kael_last_reward() ===
~ return preview_kael_reward
