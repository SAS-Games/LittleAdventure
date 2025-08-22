INCLUDE LA_common.ink

-> intro_section

=== intro_section ===
{
    -isCoop:
    -> coop_intro
  - else:
    ->solo_intro
}

=== coop_intro ===
You both awaken on cold stone — back to back, blades in hand.#speaker:id::npc, name::KAIROS, anim::Talk
The world remembers you, even if you do not.#speaker:id::npc, name::KAIROS, anim::Talk
 ->who_are_you
 
 === solo_intro ===
You awaken alone, a sword by your side and silence all around. So one returns. #speaker:id::npc, name::KAIROS, anim::Talk
Strange… the Shard remembers two.#speaker:id::npc, name::KAIROS, anim::Talk
-> who_are_you

=== who_are_you === 
    Who are you?
    ->kairos_intro
    
=== kairos_intro
    KAIROS!  A fragment. A keeper of what was lost. I will guide you… if you let me.#speaker:id::npc, name::KAIROS, anim::Talk
    {isCoop:->what_happen_to_us|->what_happen_to_me}
    
=== what_happen_to_us ===
    What happened to us?
    ->chosen_one_coop
===chosen_one_coop===    
    You were chosen — forged in twin flame. The Shard broke, and with it, your past.#speaker:id::npc, name::KAIROS, anim::Talk
    ->what_is_shard
    
=== what_happen_to_me ===
    What happened to me
    ->chosen_one_solo
    === chosen_one_solo===
    You were chosen — a soul once whole. When the Shard broke, so did your past.#speaker:id::npc, name::KAIROS, anim::Talk
    ->what_is_shard
    ===what_is_shard===
    Shard? What is that?
    ->about_shard
    
=== about_shard===
    A relic of unity — now fractured. You carry part of it in your blood.#speaker:id::npc, name::KAIROS, anim::Talk
    {isCoop:->where_are_we|->where_am_i}
 
 === where_are_we===
    Where are we?
    ->fring_place
    
===where_am_i===
    Where am I?
    ->fring_place
    
=== fring_place===
    This place is the Fringe. It exists between memory and purpose. You must learn to move again... #speaker:id::npc, name::KAIROS, anim::Talk
    To dash, press {dashButton}.  #speaker:id::npc, name::KAIROS, anim::Talk
-> END
