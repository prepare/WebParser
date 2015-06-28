﻿// 2015,2014 ,Apache2, WinterDev

namespace LayoutFarm.UI
{
    public interface ITopWindowBox
    {
        IPlatformWindowBox PlatformWinBox { get; set; }
    }
    public interface IPlatformWindowBox
    {
        bool Visible { get; set; }
        void Close();
        void SetLocation(int x, int y);
        void SetSize(int w, int h);
        bool UseRelativeLocationToParent { get; set; }
    }

}