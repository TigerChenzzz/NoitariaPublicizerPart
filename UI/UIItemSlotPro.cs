using Terraria.Audio;
using Terraria.GameInput;
using Terraria.UI;

namespace NoitariaPublicizerPart.UI;

public class UIItemSlotPro : UIElementPro {
    protected readonly int itemSlotContext;
    public virtual Item? Item { get; set; }
    public UIItemSlotPro(Item? item, int itemSlotContext, float size = 48) : base() {
        Item = item;
        this.itemSlotContext = itemSlotContext;
        this.SetSize(size, size);
    }
    public UIItemSlotPro(int itemSlotContext, float size = 48) : base() {
        this.itemSlotContext = itemSlotContext;
        this.SetSize(size, size);
    }

    /// <summary>
    /// 默认在 DrawSelf 时被使用
    /// </summary>
	protected virtual void HandleItemSlotLogic(ref Item? item) {
        if (IsMouseHovering) {
            Main.LocalPlayer.mouseInterface = true;
            Item?[] inv = [item];
            ItemSlot.Handle(inv, itemSlotContext);
        }
    }

    public override void DrawSelf(SpriteBatch spriteBatch) {
        Item? item = Item ?? SampleItem(0);
        HandleItemSlotLogic(ref item);
        item ??= SampleItem(0);
        var dimensions = Dimensions;
        float oldInventoryScale = Main.inventoryScale;
        Main.inventoryScale = Math.Min(dimensions.Width, dimensions.Height) / 52f;
        Vector2 position = dimensions.Center() + new Vector2(52f, 52f) * -0.5f * Main.inventoryScale;
        ItemSlot.Draw(spriteBatch, ref item, itemSlotContext, position);
        Main.inventoryScale = oldInventoryScale;
        if (item.IsAirS) {
            item = null;
        }
        Item = item;
    }

    #region For Extends
    protected static bool LeftClick_SellOrTrash(Item?[] inv, int context, int slot) => LeftClick_SellOrTrash(ref inv[slot], context);
    protected static bool LeftClick_SellOrTrash(ref Item? item, int context) {
        bool canSellOrTrash = false;
        bool result = false;
        if (!PlayerInput.UsingGamepad && ItemSlot.Options.DisableLeftShiftTrashCan) {
            if (!ItemSlot.Options.DisableQuickTrash) {
                if ((uint)context <= 4u && context >= 0 || context == 7 || context == 32)
                    canSellOrTrash = true;

                if (ItemSlot.ControlInUse && canSellOrTrash) {
                    SellOrTrash(ref item, context);
                    result = true;
                }
            }
        }
        else {
            if ((uint)context <= 4u && context >= 0 || context == 32)
                canSellOrTrash = Main.LocalPlayer.chest == -1;

            if (ItemSlot.ShiftInUse && canSellOrTrash && (PlayerInput.UsingGamepad || !ItemSlot.Options.DisableQuickTrash)) {
                SellOrTrash(ref item, context);
                result = true;
            }
        }

        return result;
    }

    protected static void SellOrTrash(ref Item? item, int context) {

        Player player = Main.LocalPlayer;
        if (item.IsAirS)
            return;

        if (Main.npcShop > 0 && !item.favorited) {
            Chest chest = Main.instance.shop[Main.npcShop];
            if (item.type < ItemID.CopperCoin || item.type > ItemID.PlatinumCoin) {
                if (!PlayerLoader.CanSellItem(player, player.TalkNPC, chest.item, item)) { }
                else
                    if (player.SellItem(item)) {
                        // Moved below AnnounceTransfer
                        /*
                        chest.AddItemToShop(item);
                        */
                        ItemSlot.AnnounceTransfer(new ItemSlot.ItemTransferInfo(item, context, 15));
                        int soldItemIndex = chest.AddItemToShop(item);
                        item.TurnToAir();
                        SoundEngine.PlaySound(18);

                        PlayerLoader.PostSellItem(player, player.TalkNPC, chest.item, chest.item[soldItemIndex]);
                    }
                    else if (item.value == 0) {
                        // Moved below AnnounceTransfer
                        /*
                        chest.AddItemToShop(item);
                        */
                        ItemSlot.AnnounceTransfer(new ItemSlot.ItemTransferInfo(item, context, 15));
                        int soldItemIndex = chest.AddItemToShop(item);
                        item.TurnToAir();
                        SoundEngine.PlaySound(7);

                        PlayerLoader.PostSellItem(player, player.TalkNPC, chest.item, chest.item[soldItemIndex]);
                    }
            }
        }
        else if (!item.favorited) {
            SoundEngine.PlaySound(7);
            player.trashItem = item.Clone();
            ItemSlot.AnnounceTransfer(new ItemSlot.ItemTransferInfo(player.trashItem, context, 6));
            item.TurnToAir();
            /*
            if (context == ItemSlot.Context.ChestItem && Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, player.chest, slot);
            */
        }
    }
    #endregion
}
