// Notify Icon Copyright © 2019-2020 by Ronald M. Martin
// This software is licensed under the Code Project Open License.  See the Licenses.txt file.

using DefinitionLibrary;
using NotifyIconLibrary.Events;
using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using SystemInformationLibrary;

namespace NotifyIconLibrary
{
    /// <summary>
    /// This class provides a single point of contact between a client and the classes:
    /// <c>System.Windows.Interop.HwndSource</c>, and <c>System.Windows.Interop.HwndTarget</c>.
    /// It also provides the interface to <c>Shell_NotifyIcon.</c>
    /// </summary>
    public sealed class NotifyIconWrapper : IDisposable
    {
        #region Events

        public event EventHandler BalloonTipClicked;
        public event EventHandler BalloonTipClosed;
        public event EventHandler BalloonTipShown;
        public event EventHandler<MouseLocationEventArgs> MouseMove;
        public event EventHandler<MouseLocationEventArgs> LeftMouseButtonDown;
        public event EventHandler<MouseLocationEventArgs> LeftMouseButtonClick;
        public event EventHandler<MouseLocationEventArgs> LeftMouseButtonDoubleClick;
        public event EventHandler<MouseLocationEventArgs> LeftMouseButtonUp;
        public event EventHandler<MouseLocationEventArgs> MiddleMouseButtonDown;
        public event EventHandler<MouseLocationEventArgs> MiddleMouseButtonClick;
        public event EventHandler<MouseLocationEventArgs> MiddleMouseButtonDoubleClick;
        public event EventHandler<MouseLocationEventArgs> MiddleMouseButtonUp;
        public event EventHandler<MouseLocationEventArgs> RightMouseButtonDown;
        public event EventHandler<MouseLocationEventArgs> RightMouseButtonClick;
        public event EventHandler<MouseLocationEventArgs> RightMouseButtonDoubleClick;
        public event EventHandler<MouseLocationEventArgs> RightMouseButtonUp;
        public event EventHandler<MouseLocationEventArgs> ShowContextMenu;
        public event EventHandler NotifyIconSelectedViaKeyboard;

        #endregion

        // Application defined messages
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once StringLiteralTypo
        // ReSharper disable once IdentifierTypo
        private static readonly int TaskbarCreatedMessage = NativeMethods.RegisterWindowMessage("TaskbarCreated");

        private HwndSource _hWndSource;
        private HwndTarget _hWndTarget;
        private bool _notifyIconActive;
        private bool _leftMouseButtonDown;
        private bool _middleMouseButtonDown;
        private bool _rightMouseButtonDown;
        private int _iconType = NotifyIconType.None;

        private Icon _icon;
        private static int _nextId;
        private readonly NativeMethods.NotifyIconData _data;
        private int _validItems;
        private int _seenItems;
        private int _state;
        private int _stateMask;
        private int _cumulativeState;
        private int _cumulativeStateMask;
        private int _infoFlags;
        private Icon _balloonIcon;
        private Icon _singleSizedBalloonIcon;
        private bool _iconShownInTray;

        /// <summary>
        /// This is the constructor for the <c>NotifyIconWrapper</c> class.
        /// </summary>
        /// <param name="callbackMessage">
        /// This optional parameter can be used to override the default message number
        /// used for callback messages from <c>Shell_NotifyIcon</c> if there is a conflict
        /// in the application.
        /// </param>
        public NotifyIconWrapper(int callbackMessage = WindowMessages.User)
        {
            _data = new NativeMethods.NotifyIconData();
            _data.cbSize = Marshal.SizeOf(_data);
            _data.uFlags = 0x0;
            _data.hIcon = IntPtr.Zero;
            _data.szTip = string.Empty;
            _data.dwState = 0x0;
            _data.dwStateMask = 0x0;
            _data.szInfo = string.Empty;
            _data.szInfoTitle = string.Empty;
            _data.guidItem = Guid.Empty;
            _data.hBalloonIcon = IntPtr.Zero;
            _data.hWnd = IntPtr.Zero;
            _data.uID = ++_nextId;
            _data.uVersion = NotifyIconVersions.Version4;

            if (callbackMessage < WindowMessages.User || 0x10000 <= callbackMessage)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(callbackMessage),
                    $"0x{callbackMessage:x8}",
                    "");
            }
            _data.uCallbackMessage = callbackMessage;
            _validItems |= NotifyIconFlags.Message; // This item is valid,
            _seenItems &= ~NotifyIconFlags.Message; // but it hasn't been seen by the current notify icon

            HwndSourceParameters hWsp = new HwndSourceParameters
            {
                WindowClassStyle = 0,
                WindowStyle = 0,
                ExtendedWindowStyle = ExtendedWindowStyles.ToolWindow,
                PositionX = 500,
                PositionY = 500,
                Width = 0,
                Height = 0,
                WindowName = "Notify Icon Window",
                HwndSourceHook = WndProc,
                AdjustSizingForNonClientArea = true,
                TreatAncestorsAsNonClientArea = false,
                UsesPerPixelOpacity = true,
                UsesPerPixelTransparency = false,
                RestoreFocusMode = RestoreFocusMode.Auto,
                AcquireHwndFocusInMenuMode = false,
                TreatAsInputRoot = true
            };
            _hWndSource = new HwndSource(hWsp);
            _hWndTarget = _hWndSource.CompositionTarget;
            _hWndSource.RootVisual = null;
            _data.hWnd = _hWndSource.Handle;
        }

        /// <summary>
        /// Get or set the Icon to be displayed as the notify icon.
        /// </summary>
        /// <remarks>
        /// Changes to this value take effect the next time <c>Update</c> is called.
        /// The value returned by get does not necessarily reflect the value in effect.
        /// </remarks>
        public Icon Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                IntPtr handle = value?.Handle ?? IntPtr.Zero;

                if (_data.hIcon != handle)
                {
                    _data.hIcon = handle;
                    _validItems |= NotifyIconFlags.Icon; // This item is valid,
                    _seenItems &= ~NotifyIconFlags.Icon; // but it hasn't been seen by the current notify icon
                }
            }
        }

        /// <summary>
        /// Get or set the style to be used in displaying the Tool Tip text.  Set this to true to
        /// show the original tool tip style.  Set it to false to show the balloon tool tip style.
        /// </summary>
        /// <remarks>
        /// Changes to this value take effect the next time <c>Update</c> is called.
        /// The value returned by get does not necessarily reflect the value in effect.
        /// </remarks>
        public bool ShowTip
        {
            get => (_validItems & NotifyIconFlags.ShowTip) != 0x0;
            set
            {
                if (ShowTip != value)
                {
                    // Unlike most of the flags, this one is simply sampled by Shell_NotifyIcon
                    // when it is dealing with a tooltip.  It is neither set nor cleared when
                    // a NIM_ADD or a NIM_MODIFY operation is performed.
                    _validItems = value ? _validItems | NotifyIconFlags.ShowTip
                        : _validItems & ~NotifyIconFlags.ShowTip;
                    _seenItems &= ~NotifyIconFlags.ShowTip; // Never mark this item as having been seen by the current notify icon!
                }
            }
        }

        /// <summary>
        /// Get or set the Tool Tip text to be displayed when hovering over the notify icon.
        /// The string is limited to 63 characters.
        /// </summary>
        /// <remarks>
        /// Changes to this value take effect the next time <c>Update</c> is called.
        /// The value returned by get does not necessarily reflect the value in effect.
        /// </remarks>
        public String Tip
        {
            get => _data.szTip;
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                if (!_data.szTip.Equals(value, StringComparison.InvariantCulture))
                {
                    if (value.Length > 63)
                    {
                        throw new ArgumentOutOfRangeException(nameof(Tip), value, "");
                    }

                    _data.szTip = value;
                    _validItems |= NotifyIconFlags.Tip; // This item is valid,
                    _seenItems &= ~NotifyIconFlags.Tip; // but it hasn't been seen by the current notify icon
                }
            }
        }

        /// <summary>
        /// Get or set a value that indicates whether the notify icon is hidden.
        /// </summary>
        /// <remarks>
        /// Changes to this value take effect the next time <c>Update</c> is called.
        /// The value returned by get does not necessarily reflect the value in effect.
        /// </remarks>
        public bool Hidden
        {
            get => (_cumulativeState & NotifyIconStates.Hidden) != 0;
            set
            {
                _state = (_state & ~NotifyIconStates.Hidden) | (value ? NotifyIconStates.Hidden : 0);
                _stateMask |= NotifyIconStates.Hidden;
                _validItems |= NotifyIconFlags.State; // This item is valid,
                _seenItems &= ~NotifyIconFlags.State; // but it hasn't been seen by the current notify icon
            }
        }

        /// <summary>
        /// Get or set a value that indicates whether the notify icon is shared.
        /// </summary>
        /// <remarks>
        /// Changes to this value take effect the next time <c>Update</c> is called.
        /// The value returned by get does not necessarily reflect the value in effect.
        /// </remarks>
        // ReSharper disable once UnusedMember.Global
        public bool SharedIcon
        {
            get => (_cumulativeState & NotifyIconStates.SharedIcon) != 0;
            set
            {
                _state = (_state & ~NotifyIconStates.SharedIcon) | (value ? NotifyIconStates.SharedIcon : 0);
                _stateMask |= NotifyIconStates.SharedIcon;
                _validItems |= NotifyIconFlags.State; // This item is valid,
                _seenItems &= ~NotifyIconFlags.State; // but it hasn't been seen by the current notify icon
            }
        }

        /// <summary>
        /// Get or set the info text for the balloon notification.  The string is limited to
        /// 255 characters.
        /// </summary>
        /// <remarks>
        /// Changes to this value take effect the next time <c>Update</c> is called.
        /// The value returned by get does not necessarily reflect the value in effect.
        /// </remarks>
        public string Info
        {
            get => _data.szInfo;
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                if (!_data.szInfo.Equals(value, StringComparison.InvariantCulture))
                {
                    if (value.Length > 255)
                    {
                        throw new ArgumentOutOfRangeException(nameof(Info), value, "");
                    }

                    _data.szInfo = value;
                    _validItems |= NotifyIconFlags.Info; // This item is valid,
                    _seenItems &= ~NotifyIconFlags.Info; // but it hasn't been seen by the current notify icon
                }
            }
        }

        /// <summary>
        /// Get or set the title text for the balloon notification.  The string is limited to 63 characters.
        /// </summary>
        /// <remarks>
        /// Changes to this value take effect the next time <c>Update</c> is called.
        /// The value returned by get does not necessarily reflect the value in effect.
        /// </remarks>
        public string InfoTitle
        {
            get => _data.szInfoTitle;
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                if (!_data.szInfoTitle.Equals(value, StringComparison.InvariantCulture))
                {
                    if (value.Length > 63)
                    {
                        throw new ArgumentOutOfRangeException(nameof(InfoTitle), value, "");
                    }

                    _data.szInfoTitle = value;
                    _validItems |= NotifyIconFlags.Info; // This item is valid,
                    _seenItems &= ~NotifyIconFlags.Info; // but it hasn't been seen by the current notify icon
                }
            }
        }

        /// <summary>
        /// Gets or sets the balloon icon type.
        /// </summary>
        /// <remarks>
        /// The size of the icon used is determined by the <c>LargeIcon</c> property.
        /// <para>
        /// Changes to this value take effect the next time <c>Update</c> is called.
        /// The value returned by get does not necessarily reflect the value in effect.
        /// </para>
        /// </remarks>
        public int IconType
        {
            get => _data.dwInfoFlags & NotifyIconInfoFlags.IconMask;
            set
            {
                if (NotifyIconType.None <= value && value <= NotifyIconType.User)
                {
                    _iconType = value;
                    _data.dwInfoFlags = _infoFlags | _iconType;
                }
            }
        }

        /// <summary>
        /// Get or set a value that indicates whether the balloon notification should be muted or audible.
        /// </summary>
        /// <remarks>
        /// Changes to this value take effect the next time <c>Update</c> is called.
        /// The value returned by get does not necessarily reflect the value in effect.
        /// </remarks>
        public bool NoSound
        {
            get => (_data.dwInfoFlags & NotifyIconInfoFlags.NoSound) != 0;
            set
            {
                _infoFlags = (_infoFlags & ~NotifyIconInfoFlags.NoSound) | (value ? NotifyIconInfoFlags.NoSound : 0);
                _data.dwInfoFlags = _infoFlags | _iconType;
                _validItems |= NotifyIconFlags.Info; // This item is valid,
                _seenItems &= ~NotifyIconFlags.Info; // but it hasn't been seen by the current notify icon
            }
        }

        /// <summary>
        /// Get or set a value that indicates whether large or small icons should be displayed in
        /// the balloon notification.
        /// </summary>
        /// <remarks>
        /// Changes to this value take effect the next time <c>Update</c> is called.
        /// The value returned by get does not necessarily reflect the value in effect.
        /// </remarks>
        public bool LargeIcon
        {
            get => (_data.dwInfoFlags & NotifyIconInfoFlags.LargeIcon) != 0;
            set
            {
                _infoFlags = (_infoFlags & ~NotifyIconInfoFlags.LargeIcon) | (value ? NotifyIconInfoFlags.LargeIcon : 0);
                _data.dwInfoFlags = _infoFlags | _iconType;
                BalloonIcon = BalloonIcon; // The balloon icon size might need to be changed.
                _validItems |= NotifyIconFlags.Info; // This item is valid,
                _seenItems &= ~NotifyIconFlags.Info; // but it hasn't been seen by the current notify icon
            }
        }

        /// <summary>
        /// Get or set a value that indicates whether quiet time should be respected or ignored when displaying
        /// the balloon notification.
        /// </summary>
        /// <remarks>
        /// Changes to this value take effect the next time <c>Update</c> is called.
        /// The value returned by get does not necessarily reflect the value in effect.
        /// </remarks>
        public bool RespectQuietTime
        {
            get => (_data.dwInfoFlags & NotifyIconInfoFlags.RespectQuietTime) != 0;
            set
            {
                _infoFlags = (_infoFlags & ~NotifyIconInfoFlags.RespectQuietTime) | (value ? NotifyIconInfoFlags.RespectQuietTime : 0);
                _data.dwInfoFlags = _infoFlags | _iconType;
                _validItems |= NotifyIconFlags.Info; // This item is valid,
                _seenItems &= ~NotifyIconFlags.Info; // but it hasn't been seen by the current notify icon
            }
        }

        /// <summary>
        /// Get or set a Guid to be used to uniquely identify this notify icon.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public Guid GuidItem
        {
            get => _data.guidItem;
            set
            {
                if (!_data.guidItem.Equals(value))
                {
                    _data.guidItem = value;
                    _validItems |= NotifyIconFlags.Guid; // This item is valid,
                    _seenItems &= ~NotifyIconFlags.Guid; // but it hasn't been seen by the current notify icon
                }
            }
        }

        /// <summary>
        /// Get or set the icon to be be used in a balloon notification when the <c>IconType</c> property is equal to
        /// <c>NotifyIconType.User</c>.  The size of this icon is determined by the <c>LargeIcon</c> property.
        /// </summary>
        public Icon BalloonIcon
        {
            get => _balloonIcon;
            set
            {
                _balloonIcon = value;
                _singleSizedBalloonIcon = SingleFromMulti(_balloonIcon);
                // ReSharper disable once MergeConditionalExpression
                IntPtr handle = _singleSizedBalloonIcon == null ? IntPtr.Zero : _singleSizedBalloonIcon.Handle;

                if (_data.hBalloonIcon != handle)
                {
                    _data.hBalloonIcon = handle;

                    _validItems |= NotifyIconFlags.Info; // This item is valid,
                    _seenItems &= ~NotifyIconFlags.Info; // but it hasn't been seen by the current notify icon
                }
            }
        }

        /// <summary>
        /// If multi is null, return null, else extract and return the single-sized icon selected
        /// by InfoFlags from multi.
        /// </summary>
        /// <param name="multi">A (potentially) multi-sized icon</param>
        /// <returns>A single-sized icon</returns>
        private Icon SingleFromMulti(Icon multi) =>
            multi == null
                ? null
                : new Icon(
                    multi,
                    LargeIcon
                        ? SystemInformation.IconSize
                        : SystemInformation.SmallIconSize);

        /// <summary>
        /// This is used when the context menu is closed by pressing the escape key.  It returns focus to the
        /// notify icon to allow the use of keyboard commands, such as pressing the enter key or the escape key.
        /// </summary>
        public void SetFocusOnNotifyIcon()
        {
            if (_iconShownInTray)
            {
                _data.uFlags = 0x0;
                PerformOperation(NotifyIconMethods.SetFocus);
            }
        }

        /// <summary>
        /// Prepare the notify icon data structure for the <c>Delete</c> method and perform it.
        /// </summary>
        public void Delete()
        {
            if (_iconShownInTray)
            {
                _data.uFlags = 0x0;
                PerformOperation(NotifyIconMethods.Delete);
                _iconShownInTray = false;
                _seenItems = 0x0;
                _data.dwState = 0x0;
                _data.dwStateMask = 0x0;
            }
            _notifyIconActive = false;
        }

        /// <summary>
        /// Recover from a restart of the command shell (explorer.exe).
        /// </summary>
        public void Recover()
        {
            _iconShownInTray = false;
            _seenItems = 0x0;
            _state = _cumulativeState;
            _stateMask = _cumulativeStateMask;
            Update();
        }

        /// <summary>
        /// If the icon is already being shown in the system tray, modify it with any changes
        /// that have accumulated.  Otherwise, show the icon in the system tray with all of
        /// the specified options.
        /// </summary>
        public void Update()
        {
            _data.uFlags = _validItems & ~_seenItems;
            if ((_data.uFlags & NotifyIconFlags.State) != 0)
            {
                _data.dwStateMask = _stateMask;
                _cumulativeStateMask |= _stateMask;
                _stateMask = 0;
                _data.dwState = _state & _stateMask;
                _cumulativeState = (_cumulativeState & ~_stateMask) | (_state & _stateMask);
            }
            if (_iconShownInTray)
            {
                PerformOperation(NotifyIconMethods.Modify);
                _data.uFlags = 0x0;
            }
            else
            {
                PerformOperation(NotifyIconMethods.Add);
                _data.uFlags = 0x0;
                PerformOperation(NotifyIconMethods.SetVersion);
                _iconShownInTray = true;
            }
            _seenItems = _validItems & ~NotifyIconFlags.ShowTip; // Never mark ShowTip as seen!
            _data.dwStateMask = 0x0;
            _notifyIconActive = true;
        }

        /// <summary>
        /// Use the notify icon data structure and perform the indicated method.
        /// </summary>
        /// <param name="nim">This is the ID of the method to be performed.</param>
        private void PerformOperation(int nim)
        {
            if (!NativeMethods.Shell_NotifyIcon(nim, _data))
            {
                throw new ApplicationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "", // Shell_NotifyIcon function[{0}] failed and returned extended status code {1} = 0x{1:x8}
                        nim,
                        Marshal.GetLastWin32Error()));
            }
        }

        /// <summary>
        /// This event is raised when the <c>NotifyIconWindow</c> has been closed.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Notify subscribers that the <c>NotifyIconWindow</c> has been closed.
        /// </summary>
        public void OnClosed()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// The <c>NotifyIconWindow</c> is the parent of the notify icon and it handles <c>TrayMessage</c>'s sent from the
        /// notify window to encode the messages it receives.
        /// <para>This window is shown, not hidden, but it is invisible.</para>
        /// </summary>
        /// <param name="hWnd">This is the handle of the <c>NotifyIconWindow</c>.</param>
        /// <param name="msg">This is the numerical window message type.</param>
        /// <param name="wParam">This is the <c>wParam</c> received with the message.</param>
        /// <param name="lParam">This is the <c>lParam</c> received with the message.</param>
        /// <param name="handled">Mark this true to discourage other code from handling this message.</param>
        /// <returns>InPtr.Zero on success.</returns>
        // ReSharper disable once RedundantAssignment
        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Set handled to true if this message is processed here and
            // we don't want it processed by anyone else.

            // Assume that we are going to handle the message.
            handled = true;

            // Handle the message
            if (msg == _data.uCallbackMessage)
            {
                int trayMsg = lParam.ToInt32() & 0xffff;
                switch (trayMsg)
                {
                    case WindowMessages.MouseMove:
                        OnMouseMove();
                        break;
                    case WindowMessages.LButtonDblClk:
                        OnLeftMouseButtonDoubleClick();
                        _leftMouseButtonDown = false; // Prevent sending click and up on up after double click
                        break;
                    case WindowMessages.LButtonDown:
                        OnLeftMouseButtonDown();
                        _leftMouseButtonDown = true; // Allow sending click and up on up
                        break;
                    case WindowMessages.LButtonUp:
                        if (_leftMouseButtonDown)
                        {
                            OnLeftMouseButtonClick();
                            OnLeftMouseButtonUp();
                            _leftMouseButtonDown = false;
                        }
                        break;
                    case WindowMessages.MButtonDblClk:
                        OnMiddleMouseButtonDoubleClick();
                        _middleMouseButtonDown = false; // Prevent sending click and up on up after double click
                        break;
                    case WindowMessages.MButtonDown:
                        OnMiddleMouseButtonDown();
                        _middleMouseButtonDown = true; // Allow sending click and up on up
                        break;
                    case WindowMessages.MButtonUp:
                        if (_middleMouseButtonDown)
                        {
                            OnMiddleMouseButtonClick();
                            OnMiddleMouseButtonUp();
                            _middleMouseButtonDown = false;
                        }
                        break;
                    case WindowMessages.RButtonDblClk:
                        OnRightMouseButtonDoubleClick();
                        _rightMouseButtonDown = false; // Prevent sending click and up on up after double click
                        break;
                    case WindowMessages.RButtonDown:
                        OnRightMouseButtonDown();
                        _rightMouseButtonDown = true; // Allow sending click and up on up
                        break;
                    case WindowMessages.RButtonUp:
                        if (_rightMouseButtonDown)
                        {
                            OnRightMouseButtonClick();
                            OnRightMouseButtonUp();
                            _rightMouseButtonDown = false;
                        }
                        break;
                    case WindowMessages.ContextMenu: // Also triggered internally by WindowMessages.RButtonUp
                        OnShowContextMenu();
                        break;
                    case NotifyIconNotifications.BalloonShow:
                        OnBalloonTipShown();
                        break;
                    case NotifyIconNotifications.BalloonHide:
                        OnBalloonTipClosed();
                        break;
                    case NotifyIconNotifications.BalloonTimeout:
                        OnBalloonTipClosed();
                        break;
                    case NotifyIconNotifications.BalloonUserClick:
                        OnBalloonTipClicked();
                        break;
                    case NotifyIconNotifications.KeySelect:
                        OnNotifyIconSelectedViaKeyboard();
                        break;
                    default:
                        handled = false;
                        break;
                }
            }
            else
            {
                switch (msg)
                {
                    case WindowMessages.Close:
                        Icon = null;
                        Tip = string.Empty;

                        if (_notifyIconActive)
                        {
                            _notifyIconActive = false;
                            Delete();
                        }
                        OnClosed();
                        break;
                    case WindowMessages.Destroy:
                        Dispose();
                        break;
                    default:
                        handled = false;

                        if (msg == TaskbarCreatedMessage)
                        {
                            if (_notifyIconActive)
                            {
                                Recover();
                            }
                        }
                        break;
                }
            }
            return IntPtr.Zero;

            void OnBalloonTipClicked() => BalloonTipClicked?.Invoke(this, EventArgs.Empty);
            void OnBalloonTipClosed() => BalloonTipClosed?.Invoke(this, EventArgs.Empty);
            void OnBalloonTipShown() => BalloonTipShown?.Invoke(this, EventArgs.Empty);
            void OnMouseMove() => MouseMove?.Invoke(this, Args());
            void OnLeftMouseButtonDown() => LeftMouseButtonDown?.Invoke(this, Args());
            void OnLeftMouseButtonClick() => LeftMouseButtonClick?.Invoke(this, Args());
            void OnLeftMouseButtonDoubleClick() => LeftMouseButtonDoubleClick?.Invoke(this, Args());
            void OnLeftMouseButtonUp() => LeftMouseButtonUp?.Invoke(this, Args());
            void OnMiddleMouseButtonDown() => MiddleMouseButtonDown?.Invoke(this, Args());
            void OnMiddleMouseButtonClick() => MiddleMouseButtonClick?.Invoke(this, Args());
            void OnMiddleMouseButtonDoubleClick() => MiddleMouseButtonDoubleClick?.Invoke(this, Args());
            void OnMiddleMouseButtonUp() => MiddleMouseButtonUp?.Invoke(this, Args());
            void OnRightMouseButtonDown() => RightMouseButtonDown?.Invoke(this, Args());
            void OnRightMouseButtonClick() => RightMouseButtonClick?.Invoke(this, Args());
            void OnRightMouseButtonDoubleClick() => RightMouseButtonDoubleClick?.Invoke(this, Args());
            void OnRightMouseButtonUp() => RightMouseButtonUp?.Invoke(this, Args());
            void OnShowContextMenu() => ShowContextMenu?.Invoke(this, Args());
            void OnNotifyIconSelectedViaKeyboard() => NotifyIconSelectedViaKeyboard?.Invoke(this, EventArgs.Empty);

            // Transform the mouse location in wParam from device coordinates to WPF screen coordinates
            // and create a new MouseLocationEventArgs using these coordinates.
            MouseLocationEventArgs Args()
            {
                double x = (int)wParam & 0xffff;
                double y = ((int)wParam >> 16) & 0xffff;
                Matrix t = _hWndTarget.TransformFromDevice;
                t.Translate(8, 8);
                return new MouseLocationEventArgs(t.Transform(new System.Windows.Point(x, y)));
            }
        }

        /// <summary>
        /// Close the <c>NotifyIconWindow</c>.
        /// </summary>
        public void Close()
        {
            if (_hWndSource != null)
            {
                NativeMethods.PostMessage(_hWndSource.Handle, WindowMessages.Close, 0, 0);
            }
            else
            {
                Dispose();
            }
        }

        #region IDisposable Support
        /// <summary>
        /// The usual Disposing(bool) implementation.  Note that all the managed resources
        /// held by the <c>NotifyIconWindow</c> were likely created on a thread other than the
        /// current one.  Therefore, they are disposed via Dispatcher.Invoke().
        /// </summary>
        /// <param name="disposing">This is true if we were called from Dispose().</param>
        // ReSharper disable once UnusedParameter.Local
        private void Dispose(bool disposing)
        {
            if (_hWndSource != null)
            {
                if (_notifyIconActive)
                {
                    _notifyIconActive = false;
                    Delete();
                }
                if (Icon != null)
                {
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        Icon.Dispose();
                        Icon = null;
                    });
                }
                _hWndTarget = null;
                _hWndSource.RemoveHook(WndProc);
                _hWndSource = null;
            }
        }

        /// <summary>
        /// This is the destructor (finalizer) for the <c>NotifyIconWindow</c> class.
        /// </summary>
        ~NotifyIconWrapper()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        /// <summary>
        /// This code added to correctly implement the disposable pattern.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
