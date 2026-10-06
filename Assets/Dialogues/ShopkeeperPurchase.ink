// Configurable single-item shop example
// The transaction is atomic: Coins are removed only if the item can be added.

EXTERNAL shop_try_purchase()
EXTERNAL shop_coin_count()
EXTERNAL shop_item_quantity()

VAR shop_item_name = "Tempering Powder"
VAR shop_item_price = 120

// Used only by Ink's editor Player window. In the game,
// ShopkeeperPurchaseInkBinding overrides these external functions.
VAR preview_shop_coins = 200
VAR preview_shop_item_quantity = 0
VAR preview_shop_inventory_capacity = 10

-> start

=== start ===
# id:shop.greeting
# speaker:shopkeeper
# listener:player
Looking for something?

+ [Show me what you have. # id:shop.choice.show_items] -> item_list
+ [I'm looking for {shop_item_name}. # id:shop.choice.ask_tempering_powder]
    # id:shop.ask.player
    # speaker:player
    # listener:shopkeeper
    I'm looking for {shop_item_name}.
    # id:shop.ask.reply
    # speaker:shopkeeper
    # listener:player
    I've got some. It'll cost you {shop_item_price} Coins.
    -> purchase_options
+ [Leave. # id:shop.choice.leave]
    # id:shop.leave.player
    # speaker:player
    # listener:shopkeeper
    Maybe another time.
    # id:shop.leave.reply
    # speaker:shopkeeper
    # listener:player
    Suit yourself.
    -> END

=== item_list ===
~ temp coins = shop_coin_count()
~ temp powder_count = shop_item_quantity()
# id:shop.item_list
# speaker:shopkeeper
# listener:player
Available today: {shop_item_name} - {shop_item_price} Coins. You have {coins} Coins and {powder_count} {shop_item_name}.

+ [{shop_item_name} - {shop_item_price} Coins # id:shop.choice.select_tempering_powder]
    # id:shop.item.selected
    # speaker:shopkeeper
    # listener:player
    {shop_item_name}. {shop_item_price} Coins.
    -> purchase_options
+ [Leave the shop. # id:shop.choice.item_list_leave]
    # id:shop.item_list.leave.player
    # speaker:player
    # listener:shopkeeper
    That's all.
    # id:shop.item_list.leave.reply
    # speaker:shopkeeper
    # listener:player
    Come back if you need anything.
    -> END

=== purchase_options ===
+ [\[{shop_item_price} Coins\] I'll take it. # id:shop.choice.buy]
    # id:shop.buy.player
    # speaker:player
    # listener:shopkeeper
    I'll take it.
    -> purchase_check
+ [That's too expensive. # id:shop.choice.too_expensive]
    # id:shop.expensive.player
    # speaker:player
    # listener:shopkeeper
    That's too expensive.
    # id:shop.expensive.reply
    # speaker:shopkeeper
    # listener:player
    Then have a look at something else.
    ++ [Show me something else. # id:shop.choice.expensive.show_items] -> item_list
    ++ [Leave. # id:shop.choice.expensive.leave]
        # id:shop.expensive.leave.player
        # speaker:player
        # listener:shopkeeper
        I'll pass.
        # id:shop.expensive.leave.reply
        # speaker:shopkeeper
        # listener:player
        Come back if you change your mind.
        -> END
+ [Leave. # id:shop.choice.purchase_leave]
    # id:shop.purchase.leave.player
    # speaker:player
    # listener:shopkeeper
    That's all.
    # id:shop.purchase.leave.reply
    # speaker:shopkeeper
    # listener:player
    Come back if you need anything.
    -> END

=== purchase_check ===
~ temp purchase_result = shop_try_purchase()
{ purchase_result:
- 2:
    -> purchase_success
- 1:
    -> inventory_full
- else:
    -> not_enough_coins
}

=== not_enough_coins ===
# id:shop.not_enough_coins
# speaker:shopkeeper
# listener:player
You don't have enough Coins.

+ [I'll come back later. # id:shop.choice.no_coins.later]
    # id:shop.no_coins.later.player
    # speaker:player
    # listener:shopkeeper
    I'll come back later.
    # id:shop.no_coins.later.reply
    # speaker:shopkeeper
    # listener:player
    I'll be here.
    -> END
+ [Show me something else. # id:shop.choice.no_coins.show_items] -> item_list
+ [Leave. # id:shop.choice.no_coins.leave] -> END

=== inventory_full ===
# id:shop.inventory_full
# speaker:shopkeeper
# listener:player
Looks like you're carrying too much already.

# id:shop.inventory_full.advice
# speaker:shopkeeper
# listener:player
Make some room and come back.

+ [I'll make some room. # id:shop.choice.inventory_full.make_room]
    # id:shop.inventory_full.player
    # speaker:player
    # listener:shopkeeper
    I'll be back.
    -> END
+ [Show me something else. # id:shop.choice.inventory_full.show_items] -> item_list
+ [Leave. # id:shop.choice.inventory_full.leave] -> END

=== purchase_success ===
~ temp coins = shop_coin_count()
~ temp powder_count = shop_item_quantity()
# id:shop.purchase_success
# speaker:shopkeeper
# listener:player
Pleasure doing business. You now have {coins} Coins and {powder_count} {shop_item_name}.

+ [Anything else? # id:shop.choice.success.anything_else]
    # id:shop.success.more.player
    # speaker:player
    # listener:shopkeeper
    What else have you got?
    # id:shop.success.more.reply
    # speaker:shopkeeper
    # listener:player
    Take a look.
    -> item_list
+ [Leave. # id:shop.choice.success.leave]
    # id:shop.success.leave.player
    # speaker:player
    # listener:shopkeeper
    That's all.
    # id:shop.success.leave.reply
    # speaker:shopkeeper
    # listener:player
    Come back if you need more.
    -> END

// Standalone Ink Player fallbacks -------------------------------------------

=== function shop_try_purchase() ===
{
- preview_shop_coins < shop_item_price:
    ~ return 0
- preview_shop_item_quantity >= preview_shop_inventory_capacity:
    ~ return 1
- else:
    ~ preview_shop_coins -= shop_item_price
    ~ preview_shop_item_quantity += 1
    ~ return 2
}

=== function shop_coin_count() ===
~ return preview_shop_coins

=== function shop_item_quantity() ===
~ return preview_shop_item_quantity
