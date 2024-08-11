using Terraria.UI;

namespace NoitariaPublicizerPart.UI;

// 包含:
//   是否可见 Visible
//     不可见时忽略鼠标操作, 不会认为鼠标在其中
//   是否活跃
//     不活跃时不会 Update
//   封住 Update 和 Draw 重写, 改用 UpdateSelf 代替, 可以重写 BaseUpdate 和 BaseDraw 以改变其行为
//   更多事件
//     OnHovering, OnNotHovering: 鼠标在 / 不在上面时每次 Update 调用
//     PostUpdate, OnDraw
//   拖拽 Draggable 和 RightDraggable
//   BorderX, BorderY, Border 用以快速设置 Padding
//   ClampWidth, ClampHeight 用以设置是否将长宽限制为父元素的大小
public class UIElementPro : UIElement
{
    static UIElementPro() {
        On_UIElement.GetDimensionsBasedOnParentDimensions += (orig, self, parent) => self is UIElementPro pro ? pro.GetDimensionsBasedOnParentDimensions(parent) : orig(self, parent);
    }

    public UIElementPro()
    {
        Recalculate();
        SettleUserInterface = true;
        OnUpdate += OnUpdate_MoveEvents;
    }
    /// <summary>
    /// 是否可见, 若不可见则跳过 Draw
    /// </summary>
    public virtual bool Visible { get; set; } = true;
    /// <summary>
    /// 是否活动, 若不活动则跳过 Update
    /// </summary>
    public virtual bool Active { get; set; } = true;
    public virtual bool AutoSetIgnoresMouseInteraction => true;

    #region 更多事件
    public event Action? OnHovering;
    public event Action? OnNotHovering;
    private void OnUpdate_MoveEvents(UIElement affectedElement)
    {
        if (IsMouseHovering)
        {
            OnHovering?.Invoke();
        }
        else
        {
            OnNotHovering?.Invoke();
        }
    }
    public event Action? PostUpdate;
    #endregion

    #region Update
    private bool _settleUserInterface;
    public bool SettleUserInterface
    {
        get => _settleUserInterface;
        set
        {
            if (_settleUserInterface == value)
                return;
            _settleUserInterface = value;
            if (value)
                OnHovering += SettleUserInterfaceOnHovering;
            else
                OnHovering -= SettleUserInterfaceOnHovering;
        }
    }

    private void SettleUserInterfaceOnHovering()
    {
        Main.LocalPlayer.mouseInterface = true;
    }

    public override void Update(GameTime gameTime)
    {
        if (!Active)
        {
            return;
        }
        UpdateSelf(gameTime);
        BaseUpdate(gameTime);
        PostUpdate?.Invoke();
    }
    protected virtual void UpdateSelf(GameTime gameTime) { }
    protected virtual void BaseUpdate(GameTime gameTime) => base.Update(gameTime);
    #endregion

    #region Draw
    public event Action<SpriteBatch>? OnDraw;
    public sealed override void Draw(SpriteBatch spriteBatch)
    {
        if (AutoSetIgnoresMouseInteraction) {
            IgnoresMouseInteraction = !Visible;
        }
        if (!Visible)
        {
            return;
        }
        OnDraw?.Invoke(spriteBatch);
        BaseDraw(spriteBatch);
    }
    protected virtual void BaseDraw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
    }
    #endregion

    #region 拖拽
    protected bool draggable;
    public bool Draggable
    {
        get => draggable;
        set
        {
            if (value == draggable)
            {
                return;
            }
            if (value)
            {
                OnLeftMouseDown += OnMouseDown_Drag;
                OnLeftMouseUp += OnMouseUp_Drag;
                if (!updateDragAdded)
                {
                    OnUpdate += Update_Drag;
                    updateDragAdded = true;
                }
                
            }
            else
            {
                OnLeftMouseDown -= OnMouseDown_Drag;
                OnLeftMouseUp -= OnMouseUp_Drag;
                if (updateDragAdded)
                {
                    OnUpdate -= Update_Drag;
                    updateDragAdded = false;
                }
                Dragging = false;
            }
        }
    }
    protected bool rightDraggable;
    public bool RightDraggable
    {
        get => rightDraggable;
        set
        {
            if (value == rightDraggable)
            {
                return;
            }
            if (value)
            {
                OnRightMouseDown += OnMouseDown_Drag;
                OnRightMouseUp += OnMouseUp_Drag;
                if (!updateDragAdded)
                {
                    OnUpdate += Update_Drag;
                    updateDragAdded = true;
                }
            }
            else
            {
                OnRightMouseDown -= OnMouseDown_Drag;
                OnRightMouseUp -= OnMouseUp_Drag;
                if (updateDragAdded)
                {
                    OnUpdate -= Update_Drag;
                    updateDragAdded = false;
                }
                Dragging = false;
            }
        }
    }
    public bool Dragging { get; protected set; }
    public event Action? OnDragStart;
    public event Action? OnDragging;
    protected Vector2 mouseDeltaWhenDragging;
    protected void OnMouseDown_Drag(UIMouseEvent evt, UIElement listeningElement)
    {
        if (!Visible)
        {
            return;
        }
        mouseDeltaWhenDragging = new Vector2(Left.Pixels, Top.Pixels) - Main.MouseScreen;
        Dragging = true;
        OnDragStart?.Invoke();
    }
    protected void OnMouseUp_Drag(UIMouseEvent evt, UIElement listeningElement)
    {
        Dragging = false;
    }
    protected bool updateDragAdded;
    protected void Update_Drag(UIElement affectedElement)
    {
        if (!Dragging)
        {
            return;
        }
        Left.Pixels = mouseDeltaWhenDragging.X + Main.MouseScreen.X;
        Top.Pixels = mouseDeltaWhenDragging.Y + Main.MouseScreen.Y;
        OnDragging?.Invoke();
    }
    #endregion

    #region 获取参数
    public CalculatedStyle Dimensions => _dimensions;
    public CalculatedStyle InnerDimensions => _innerDimensions;
    public float BorderX
    {
        get => PaddingLeft;
        set => PaddingLeft = PaddingRight = value;
    }
    public float BorderY
    {
        get => PaddingTop;
        set => PaddingTop = PaddingBottom = value;
    }
    public float Border
    {
        get => PaddingLeft;
        set => SetPadding(value);
    }
    #endregion

    #region 是否将长宽限制于 Parent 的大小
    public bool ClampWidth { get; set; } = true;
    public bool ClampHeight { get; set; } = true;

	protected new CalculatedStyle GetDimensionsBasedOnParentDimensions(CalculatedStyle parentDimensions)
	{
		CalculatedStyle result = default;
		result.X = Left.GetValue(parentDimensions.Width) + parentDimensions.X;
		result.Y = Top.GetValue(parentDimensions.Height) + parentDimensions.Y;
		float value = MinWidth.GetValue(parentDimensions.Width);
		float value2 = MaxWidth.GetValue(parentDimensions.Width);
		float value3 = MinHeight.GetValue(parentDimensions.Height);
		float value4 = MaxHeight.GetValue(parentDimensions.Height);
        float width = Width.GetValue(parentDimensions.Width);
        float height = Height.GetValue(parentDimensions.Height);
		result.Width = ClampWidth ? MathHelper.Clamp(width, value, value2) : width;
		result.Height = ClampHeight ? MathHelper.Clamp(height, value3, value4) : height;
		result.Width += MarginLeft + MarginRight;
		result.Height += MarginTop + MarginBottom;
		result.X += parentDimensions.Width * HAlign - result.Width * HAlign;
		result.Y += parentDimensions.Height * VAlign - result.Height * VAlign;
		return result;
	}
    #endregion

    #region overrides
    public override bool ContainsPoint(Vector2 point) => Visible && GetDimensions().ContainsPoint(point);
    #endregion
}
