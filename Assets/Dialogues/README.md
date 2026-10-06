# Dialogue Story Demo

Open `DialogueTest.unity` and enter Play Mode. Use the small top-left launcher to start either assigned story.

## Stories

- `KaelQuest.ink` implements **The Bloomblade Delivery** with first/return conversations, highest-health item selection, timer failure, item-health outcomes, rewards, completion, and post-completion dialogue.
- `ShopkeeperPurchase.ink` implements a configurable single-item purchase loop. `shop_try_purchase` validates both Coins and inventory capacity before committing either side of the transaction.

The matching `.metadata.json` files provide tag suggestions in the customized Inky editor. The compiled `.json` files are the `TextAsset` inputs used by `DialogueHandler`.

## Runtime architecture

The package `DialogueTrigger` owns one assigned compiled Ink story. Call `ShowDialogue()` from an interaction, UnityEvent, proximity component, or test launcher.

Bindings on the same GameObject are discovered automatically. Additional binding components can be assigned through **Binding Sources**. Game-specific bindings extend the package `InkStoryBinding` base class:

- Add `[InkExternal("ink_function_name")]` to a C# method to implement an Ink `EXTERNAL`.
- Add `[InkVariable("ink_variable_name")]` to a field or readable property to inject its current value before the first line is evaluated.

The demo scene contains:

- `Kael Quest Dialogue` with `DialogueTrigger` and `KaelQuestInkBinding`.
- `Shopkeeper Purchase Dialogue` with `DialogueTrigger` and `ShopkeeperPurchaseInkBinding`.
- `Dialogue Story Demo Launcher`, an optional story-agnostic test menu.

The Kael binding registers:

- `kael_quest_state`
- `kael_is_first_conversation`
- `kael_has_moonbloom_oil`
- `kael_mark_conversation_started`
- `kael_try_deliver`
- `kael_last_reward`

It also injects `kael_requested_item`.

The shop binding registers:

- `shop_try_purchase`
- `shop_coin_count`
- `shop_item_quantity`

It also injects `shop_item_name` and `shop_item_price`.

For production, retain these Ink contracts and replace the binding method bodies with calls to the real quest, inventory, item-health, timer, and currency services.

## Package boundary

The package owns story launching and the reusable binding mechanism. The consuming game owns quest, inventory, timer, item-health, and currency behavior. The test prefab exposes two active `SpeakerView` instances and uses `DialogueCharacterCatalog.asset`; the test scene also includes an `EventSystem` for clickable choices.
