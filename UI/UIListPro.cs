﻿using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace NoitariaPublicizerPart.UI;

/// <summary>
/// 使滚轮更加顺滑, 取消自动排序
/// </summary>
public class UIListPro : UIList {
    public UIListPro() {
        ManualSortMethod = _ => { };
    }
    #region 使滚轮更加顺滑
    public void StopMoving() {
        scrollbarAim = 0;
    }
    // TODO: 滚动太上或太下时的缓动
    public override void ScrollWheel(UIScrollWheelEvent evt) {
        var scrollbar = _scrollbar;
        _scrollbar = null;
        base.ScrollWheel(evt);
        _scrollbar = scrollbar;
        if (_scrollbar != null) {
            scrollbarAim -= evt.ScrollWheelValue;
            // _scrollbar.ViewPosition -= evt.ScrollWheelValue;
        }
    }
    private float scrollbarAim;
    private void Update_Scrollbar() {
        if (_scrollbar == null || _scrollbar._isDragging) {
            scrollbarAim = 0;
            return;
        }
        if (scrollbarAim == 0) {
            return;
        }
        var absAim = Math.Abs(scrollbarAim);
        var signAim = Math.Sign(scrollbarAim);
        float delta = absAim > 40 ? scrollbarAim * 0.15f : Math.Min(absAim / 8 + 1, absAim) * signAim;
        _scrollbar.ViewPosition += delta;
        if (signAim > 0 && _scrollbar.ViewPosition >= _scrollbar.MaxViewSize - _scrollbar.ViewSize) {
            scrollbarAim = 0;
        }
        else if (signAim < 0 && _scrollbar.ViewPosition <= 0) {
            scrollbarAim = 0;
        }
        else {
            scrollbarAim -= delta;
        }
    }
    #endregion
    public override void Update(GameTime gameTime) {
        Update_Scrollbar();
    }
    public void RemoveAt(int i) {
        _innerList.Elements[i].Parent = null;
        _innerList.Elements.RemoveAt(i);
        _items.RemoveAt(i);
        UpdateOrder();
    }
    public List<UIElement> GetListElements() => _innerList.Elements;
}
