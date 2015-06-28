﻿//BSD  2015,2014 ,WinterDev

using System;
using System.Text;
using System.Collections.Generic;
using LayoutFarm;
using LayoutFarm.UI;

namespace LayoutFarm.WebDom
{

    partial class DomElement : IEventListener
    {

        void IEventListener.ListenKeyPress(UIKeyEventArgs e)
        {
            OnKeyPress(e);
        }
        void IEventListener.ListenKeyDown(UIKeyEventArgs e)
        {
            OnKeyDown(e);
        }
        void IEventListener.ListenKeyUp(UIKeyEventArgs e)
        {
            OnKeyUp(e);
        }
        bool IEventListener.ListenProcessDialogKey(UIKeyEventArgs e)
        {
            return OnProcessDialogKey(e);
        }
        void IEventListener.ListenMouseDown(UIMouseEventArgs e)
        {
            OnMouseDown(e);
        }
        void IEventListener.ListenLostMouseFocus(UIMouseEventArgs e)
        {
            OnLostMouseFocus(e);

        }
        void IEventListener.ListenMouseMove(UIMouseEventArgs e)
        {
            OnMouseMove(e);
        }
        void IEventListener.ListenMouseUp(UIMouseEventArgs e)
        {
            //1. mouse up
            OnMouseUp(e);

        }
        void IEventListener.ListenMouseClick(UIMouseEventArgs e)
        {
        }
        void IEventListener.ListenMouseDoubleClick(UIMouseEventArgs e)
        {
            OnDoubleClick(e);
        }
        void IEventListener.ListenMouseWheel(UIMouseEventArgs e)
        {
            OnMouseWheel(e);
        }
        void IEventListener.ListenMouseLeave(UIMouseEventArgs e)
        {
            OnMouseLeave(e);
        }
        void IEventListener.ListenGotKeyboardFocus(UIFocusEventArgs e)
        {
            OnGotFocus(e);
        }
        void IEventListener.ListenLostKeyboardFocus(UIFocusEventArgs e)
        {
            OnLostFocus(e);
        }

        void IEventListener.HandleContentLayout()
        {
            OnContentLayout();
        }
        void IEventListener.HandleContentUpdate()
        {
            OnContentUpdate();
        }
        void IEventListener.HandleElementUpdate()
        {
            OnElementChanged();
        }
        bool IEventListener.BypassAllMouseEvents
        {
            get { return false; }
        }
        bool IEventListener.AutoStopMouseEventPropagation
        {
            get { return false; }
        }
        void IEventListener.ListenInterComponentMsg(object sender, int msgcode, string msg)
        {
            this.OnInterComponentMsg(sender, msgcode, msg);
        }

        void IEventListener.ListenGuestTalk(UIGuestTalkEventArgs e)
        {
        }
        void IEventListener.GetGlobalLocation(out int x, out int y)
        {
            this.GetGlobalLocation(out x, out y);
        }

        public abstract void GetGlobalLocation(out int x, out int y);

    }
}