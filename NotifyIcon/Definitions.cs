// Definition Library Copyright © 2019-2020 by Ronald M. Martin
// This software is licensed under the Code Project Open License.  See the Licenses.txt file.

// These definitions parallel Microsoft Win32 definitions, but they take the form of public constant integers
// defined in a series of classes intended to group them by purpose and to avoid running afoul of ReSharper.
// The Win32 equivalent values are given in the comments.
// ReSharper disable CommentTypo
// ReSharper disable UnusedMember.Global
namespace DefinitionLibrary
{
    public static class NotifyIconVersions
    {
        public const int Version4 = 4; // NOTIFYICON_VERSION_4
    }

    public static class NotifyIconMethods
    {
        public const int Add        = 0x00000000, // NIM_ADD
                         Modify     = 0x00000001, // NIM_MODIFY
                         Delete     = 0x00000002, // NIM_DELETE
                         SetFocus   = 0x00000003, // NIM_SETFOCUS
                         SetVersion = 0x00000004; // NIM_SETVERSION
    }

    public static class NotifyIconNotifications
    {
        public const int Select           = WindowMessages.User + 0, // NIN_SELECT
                         KeySelect        = WindowMessages.User + 1, // NIN_KEYSELECT
                         BalloonShow      = WindowMessages.User + 2, // NIN_BALLOONSHOW
                         BalloonHide      = WindowMessages.User + 3, // NIN_BALLOOONHIDE
                         BalloonTimeout   = WindowMessages.User + 4, // NIN_BALLOONTIMEOUT
                         BalloonUserClick = WindowMessages.User + 5, // NIN_BALLOONUSERCLICK
                         PopupOpen        = WindowMessages.User + 6, // NIN_POPUPOPEN
                         PopupClose       = WindowMessages.User + 7; // NIN_POPUPCLOSE
    }

    public static class NotifyIconFlags
    {
        public const int 
            Message  = 0x00000001, // NIF_MESSAGE
            Icon     = 0x00000002, // NIF_ICON
            Tip      = 0x00000004, // NIF_TIP
            State    = 0x00000008, // NIF_STATE
            Info     = 0x00000010, // NIF_INFO
            Guid     = 0x00000020, // NIF_GUID
            RealTime = 0x00000040, // NIF_REALTIME
            ShowTip  = 0x00000080; // NIF_SHOWTIP
    }

    public static class NotifyIconType
    {
        public const int None    = 0x00000000, // NIIF_NONE
                         Info    = 0x00000001, // NIIF_INFO
                         Warning = 0x00000002, // NIIF_WARNING
                         Error   = 0x00000003, // NIIF_ERROR
                         User    = 0x00000004; // NIIF_USER
    }

    public static class NotifyIconInfoFlags
    {
        public const int IconMask         = 0x0000000F, // NIIF_ICON_MASK
                         NoSound          = 0x00000010, // NIIF_NOSOUND
                         LargeIcon        = 0x00000020, // NIIF_LARGEICON
                         RespectQuietTime = 0x00000080; // NIIF_RESPECT_QUIET_TIME
    }

    public static class NotifyIconStates
    {
        public const int Hidden     = 0x00000001, // NIS_HIDDEN
                         SharedIcon = 0x00000002; // NIS_SHAREDICON
    }

    public static class SystemMetrics
    {
        public const int XIcon = 11,   // SM_CXICON
                         YIcon = 12,   // SM_CYICON
                         XSmIcon = 49, // SM_CXSMICON
                         YSmIcon = 50; // SM_CYSMICON
    }

    public static class WindowStyles
    {
        public const int Overlapped       = 0x00000000, // WS_OVERLAPPED
                         Popup            = unchecked((int) 0x80000000), // WS_POPUP
                         Child            = 0x40000000, // WS_CHILD
                         Minimize         = 0x20000000, // WS_MINIMIZE
                         Visible          = 0x10000000, // WS_VISIBLE
                         Disabled         = 0x08000000, // WS_DISABLED
                         ClipSiblings     = 0x04000000, // WS_CLIPSIBLINGS
                         ClipChildren     = 0x02000000, // WS_CLIPCHILDREN
                         Maximize         = 0x01000000, // WS_MAXIMIZE
                         Caption          = 0x00C00000, // WS_CAPTION
                         Border           = 0x00800000, // WS_BORDER
                         DlgFrame         = 0x00400000, // WS_DLGFRAME
                         VScroll          = 0x00200000, // WS_VSCROLL
                         HScroll          = 0x00100000, // WS_HSCROLL
                         SysMenu          = 0x00080000, // WS_SYSMENU
                         ThickFrame       = 0x00040000, // WS_THICKFRAME
                         MinimizeBox      = 0x00020000, // WS_MINIMIZEBOX
                         MaximizeBox      = 0x00010000, // WS_MAXIMIZEBOX
                         TabStop          = 0x00010000, // WS_TABSTOP
                         OverlappedWindow = Overlapped  // WS_OVERLAPPEDWINDOW
                                          | Caption
                                          | SysMenu
                                          | ThickFrame
                                          | MinimizeBox
                                          | MaximizeBox,
                         PopupWindow =      Popup       // WS_POPUPWINDOW - Combine with Caption to make the popup visible
                                          | Border
                                          | SysMenu;
    }

    public static class ExtendedWindowStyles
    {
        public const int DlgModalFrame   = 0x00000001, // WS_EX_DLGMODALFRAME
                         TopMost         = 0x00000008, // WS_EX_TOPMOST
                         Transparent     = 0x00000020, // WS_EX_TRANSPARENT
                         MdiChild        = 0x00000040, // WS_EX_MDICHILD
                         ToolWindow      = 0x00000080, // WS_EX_TOOLWINDOW
                         WindowEdge      = 0x00000100, // WS_EX_WINDOWEDGE
                         ClientEdge      = 0x00000200, // WS_EX_CLIENTEDGE
                         ContextHelp     = 0x00000400, // WS_EX_CONTEXTHELP
                         Left            = 0x00000000, // WS_EX_LEFT
                         Right           = 0x00001000, // WS_EX_RIGHT
                         RtlReading      = 0x00002000, // WS_EX_RTLREADING
                         LeftScrollBar   = 0x00004000, // WS_EX_LEFTSCROLLBAR
                         ControlParent   = 0x00010000, // WS_EX_CONREOLPARENT
                         StaticEdge      = 0x00020000, // WS_EX_STATICEDGE
                         AppWindow       = 0x00040000, // WS_EX_APPWINDOW
                         Layered         = 0x00080000, // WS_EX_LAYERED
                         NoInheritLayout = 0x00100000, // WS_EX_NOINHERITLAYOUT
                         LayoutRtl       = 0x00400000, // WS_EX_LAYOUTRTL
                         Composited      = 0x02000000; // WS_EX_COMPOSITED
    }

    public static class WindowMessages
    {
        public const int Destroy       = 0x0002, // WM_DESTROY,
                         Close         = 0x0010, // WM_CLOSE,
                         ContextMenu   = 0x007B, // WM_CONTEXTMENU
                         KeyDown       = 0x0100, // WM_KEYDOWN
                         KeyUp         = 0x0101, // WM_KEYUP
                         Char          = 0x0102, // WM_CHAR
                         Command       = 0x0111, // WM_COMMAND
                         MouseMove     = 0x0200, // WM_MOUSEMOVE
                         LButtonDown   = 0x0201, // WM_LBUTTONDOWN
                         LButtonUp     = 0x0202, // WM_LBUTTONUP
                         LButtonDblClk = 0x0203, // WM_LBUTTONDBLCLK
                         RButtonDown   = 0x0204, // WM_RBUTTONDOWN
                         RButtonUp     = 0x0205, // WM_RBUTTONUP
                         RButtonDblClk = 0x0206, // WM_RBUTTONDBLCLK
                         MButtonDown   = 0x0207, // WM_MBUTTONDOWN
                         MButtonUp     = 0x0208, // WM_MBUTTONUP
                         MButtonDblClk = 0x0209, // WM_MBUTTONDBLCLK
                         User          = 0x0400, // WM_USER
                         App           = 0x8000; // WM_APP
    }
}
