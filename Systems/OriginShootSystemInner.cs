using Terraria.Graphics.Capture;
using Terraria.Localization;

namespace NoitariaPublicizerPart.Systems;

public abstract class OriginShootSystemInner : ModSystem {
    protected static void OriginShootInner(Player player, Item item, Vector2 position, float rotation, Vector2 targetPosition, int damage)
    {
        SupressNetMessage = true;

        using var playerInfoStorage = PlayerInfoStorage.Create(player);

        player.MountedCenter = position;
        player.oldPosition = player.position;
        player.itemRotation = rotation;
        Main_SetMouseWorld(targetPosition);

        player.ItemCheck_Shoot(player.whoAmI, item, damage);

        SupressNetMessage = false;
    }
    #region 抑制发包
    public static bool SupressNetMessage { get; set; }
    // private static HashSet<int> SupressNetMessageIds { get; } = [ MessageID.PlayerControls, MessageID.ShotAnimationAndSound ];
    private static bool NeedSupress(int msgType) {
        return msgType == MessageID.PlayerControls  // 13
            || msgType == MessageID.ShotAnimationAndSound; // 41
    }
	private static void On_NetMessage_SendData_SupressNetMessage(On_NetMessage.orig_SendData orig, int msgType, int remoteClient = -1, int ignoreClient = -1, NetworkText? text = null, int number = 0, float number2 = 0f, float number3 = 0f, float number4 = 0f, int number5 = 0, int number6 = 0, int number7 = 0) {
        if (SupressNetMessage && NeedSupress(msgType)) {
            return;
        }
        orig(msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);
    }
    private static void Load_SpressNetMessage() {
        On_NetMessage.SendData += On_NetMessage_SendData_SupressNetMessage;
    }
    #endregion
    #region Player 信息暂存
    protected record struct PlayerInfoStorage(
        Player Player,
        int MouseX,
        int MouseY,
        Vector2 Position,
        Vector2 OldPosition,
        Vector2 Velocity,
        int ToolTime,
        int ItemTime,
        int ItemTimeMax,
        int ItemAnimation,
        int ItemAnimationMax,
        int ReuseDelay,
        float ItemRotation,
        Vector2 ItemLocation,
        int ItemWidth,
        int ItemHeight,
        int Direction,
        bool ReleaseUseItem,
        bool ControlUseItem,
        bool ControlUseTile,
        int AttackCD,
        int ItemUsesThisAnimation
    ) : IDisposable {
        /// <summary>
        /// <br/>更多: 取自 (SOTS.FakePlayer.SaveRealPlayerValues)
        /// <br/><see cref="Player.lastVisualizedSelectedItem"/> (使用 <see cref="Item.Clone"/>)
        /// <br/><see cref="Player.channel"/>
        /// <br/><see cref="Player.frozen"/>
        /// <br/><see cref="Player.webbed"/>
        /// <br/><see cref="Player.stoned"/>
        /// <br/><see cref="Player.mount"/> (<see cref="Mount.Active"/>)
        /// <br/><see cref="Player.altFunctionUse"/>
        /// <br/><see cref="Player.pulley"/>
        /// <br/><see cref="Player.isPettingAnimal"/>
        /// <br/><see cref="Player.heldProj"/>
        /// <br/><see cref="Player.stealth"/>
        /// <br/><see cref="Player.gravDir"/>
        /// <br/><see cref="Player.invis"/>
        /// <br/><see cref="Player.gfxOffY"/>
        /// <br/><see cref="Player.selectedItem"/>
        /// <br/><see cref="Player.selectItemOnNextUse"/>
        /// <para/>右键相关:
        /// <br/>    <see cref="Player.mouseInterface"/>
        /// <br/>    <see cref="Main.HoveringOverAnNPC"/>
        /// <br/>    <see cref="CaptureManager.Active"/> (使用 (<see cref="CaptureManager.Ins"/>))
        /// <br/>    <see cref="Main.SmartInteractShowingGenuine"/>
        /// </summary>
        public static PlayerInfoStorage Create(Player player) {
            return new(
                player,
                Main.mouseX,
                Main.mouseY,
                player.position,
                player.oldPosition,
                player.velocity,
                player.toolTime,
                player.itemTime,
                player.itemTimeMax,
                player.itemAnimation,
                player.itemAnimationMax,
                player.reuseDelay,
                player.itemRotation,
                player.itemLocation,
                player.itemWidth,
                player.itemHeight,
                player.direction,
                player.releaseUseItem,
                player.controlUseItem,
                player.controlUseTile,
                player.attackCD,
                player.ItemUsesThisAnimation
            );
        }
        public readonly void Dispose() {
            Main.mouseX = MouseX;
            Main.mouseY = MouseY;
            Player.position = Position;
            Player.oldPosition = OldPosition;
            Player.velocity = Velocity;
            Player.toolTime = ToolTime;
            Player.itemTime = ItemTime;
            Player.itemTimeMax = ItemTimeMax;
            Player.itemAnimation = ItemAnimation;
            Player.itemAnimationMax = ItemAnimationMax;
            Player.reuseDelay = ReuseDelay;
            Player.itemRotation = ItemRotation;
            Player.itemLocation = ItemLocation;
            Player.itemWidth = ItemWidth;
            Player.itemHeight = ItemHeight;
            Player.direction = Direction;
            Player.releaseUseItem = ReleaseUseItem;
            Player.controlUseItem = ControlUseItem;
            Player.controlUseTile = ControlUseTile;
            Player.attackCD = AttackCD;
            Player.ItemUsesThisAnimation = ItemUsesThisAnimation;
        }
    }
    #endregion
    public override void Load() {
        Load_SpressNetMessage();
    }
}
