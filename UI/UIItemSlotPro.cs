using Terraria.UI;

namespace NoitariaPublicizerPart.UI;

public class UIItemSlotPro : UIElementPro
{
    protected readonly int itemSlotContext;
    public Item? Item { get; set; }
    public UIItemSlotPro(Item? item, int itemSlotContext, float size = 48) : base()
    {
        Item = item;
        this.itemSlotContext = itemSlotContext;
        Width = new(size, 0f);
        Height = new(size, 0f);
    }

    /// <summary>
    /// 默认在 DrawSelf 时被使用
    /// </summary>
	protected virtual void HandleItemSlotLogic(ref Item? item)
	{
		if (IsMouseHovering) {
			ItemSlot.OverrideHover(ref item, itemSlotContext);
			ItemSlot.LeftClick(ref item, itemSlotContext);
			ItemSlot.RightClick(ref item, itemSlotContext);
			ItemSlot.MouseHover(ref item, itemSlotContext);
		}
	}

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        Item? item = Item;
        HandleItemSlotLogic(ref item);
        var dimensions = Dimensions;
        float oldInventoryScale = Main.inventoryScale;
        Main.inventoryScale = Math.Min(dimensions.Width, dimensions.Height) / 52f;
        Vector2 position = dimensions.Center() + new Vector2(52f, 52f) * -0.5f * Main.inventoryScale;
        ItemSlot.Draw(spriteBatch, ref item, itemSlotContext, position);
        Main.inventoryScale = oldInventoryScale;
        Item = item;
    }
}
