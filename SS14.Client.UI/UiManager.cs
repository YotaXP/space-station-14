using Awesomium.Core;
using SFML.Window;
using SS14.Client.Graphics;
using SS14.Client.Interfaces.State;
using SS14.Shared.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SS14.Client.UI
{
    public class UiManager : IUiManager
    {
        public UiManager()
        {
            var cfg = new WebConfig()
            {
                RemoteDebuggingPort = 8000 // TODO: This needs to be a config thing, obviously.  Must be enabled in both client and server configs.
            };

            var size = SS14.Client.Graphics.CluwneLib.Screen.Size;

            UiThread = new Thread(() =>
            {
                var prefs = new WebPreferences()
                {
                    CanScriptsAccessClipboard = true,
                    SmoothScrolling = false,
                    CustomCSS = "body { background: transparent; }"
                };

                WebCore.Initialize(cfg);

                session = WebCore.CreateWebSession(@".\session", prefs);
                session.ClearCache();
                webview = WebCore.CreateWebView((int)size.X, (int)size.Y, session);

                session.AddDataSource("ui", new Awesomium.Core.Data.DirectoryDataSource(@".\html", 2 << 20));

                webview.IsTransparent = true;
                webview.Surface = surface = new SfmlSurface();
                webview.Source = new Uri("asset://ui/test.html");
                webview.Presenter = new Presenter(this);

                //WebCore.Started += (s, e) =>
                //{
                //    UiContext = SynchronizationContext.Current;
                //    onReady.Set();
                //};

                webview.DocumentReady += webview_DocumentReady;

                webview.FocusView();
                WebCore.Run();
            });
            UiThread.Name = "Awesomium Thread";
            UiThread.Start();
        }


        WebSession session;
        WebView webview;
        public bool HasKeyboardFocus { get; private set; }
        public bool HasMouseCapture { get; private set; }

        SfmlSurface surface;

        public Thread UiThread { get; private set; }

        // TODO: May want to throttle this to prevent the resource allocation nightmare from occurring for every pixel that the mouse moves while resizing.
        public void Resize(int width, int height)
        {
            if (webview != null)
                webview.BeginInvoke((Action)delegate()
                {
                    webview.Resize(width, height);
                }, null);
        }

        public void Shutdown()
        {
            if (WebCore.IsRunning)
                WebCore.Shutdown();
        }

        public void Navigate(Uri uri)
        {
            if (webview != null)
                webview.BeginInvoke((Action)delegate()
                {
                    webview.Source = uri;
                }, null);
        }

        public void Draw()
        {
            if (surface != null)
                surface.Draw();
        }

        #region Input

        private void KillFocus()
        {
            webview.ExecuteJavascript("document.activeElement.blur();");
        }

        void webview_DocumentReady(object sender, UrlEventArgs e)
        {
            JSObject callbacks = webview.ExecuteJavascriptWithResult(@"
                if (!window.ssCallbacks) {
                    window.ssCallbacks = {}

                    document.addEventListener('focus', function(e) {
                        window.ssCallbacks.focused();
                    }, true);

                    document.addEventListener('blur', function(e) {
                        window.ssCallbacks.blurred();
                    }, true);
                }
                window.ssCallbacks;
            ");
            if (callbacks["ready"]) return;

            callbacks.Bind("focused", false, (s, ev) =>
            {
                HasKeyboardFocus = true;
            });
            callbacks.Bind("blurred", false, (s, ev) =>
            {
                HasKeyboardFocus = false;
            });

            callbacks["ready"] = true;
        }

        private WebKeyboardEvent ConvertKeyEventArgs(KeyEventArgs kea, WebKeyboardEventType type)
        {
            var wke = new WebKeyboardEvent();
            if (kea.Alt) wke.Modifiers |= Modifiers.AltKey;
            if (kea.Control) wke.Modifiers |= Modifiers.ControlKey;
            if (kea.Shift) wke.Modifiers |= Modifiers.ShiftKey;
            if (kea.System) wke.Modifiers |= Modifiers.MetaKey;
            wke.IsSystemKey = kea.System;
            wke.NativeKeyCode = (int)kea.Code;
            wke.Type = type;
            return wke;

        }

        private class Presenter : IWebViewPresenter
        {
            private UiManager manager;
            public IWebView View { get; set; }

            internal Presenter(UiManager manager)
            {
                this.manager = manager;
            }

            public void ShowCursor(CursorType cursor)
            {
                manager.HasMouseCapture = cursor != CursorType.None;
                CluwneLib.Screen.SetTitle(cursor.ToString());
            }


            public void ShowLoginDialog(LoginRequestEventArgs e)
            {
                e.Cancel = true;
            }
            public void ShowChooseFilesDialog(FileDialogEventArgs e)
            {
                e.Cancel = true;
            }
            public void ShowDownloadDialog(DownloadEventArgs e)
            {
                e.Cancel = true;
            }
            public void ShowJSDialog(JavascriptDialogEventArgs e)
            {
                e.Cancel = true;
            }
            public void ShowCertificateErrorDialog(CertificateErrorEventArgs e)
            {
                throw new NotImplementedException(); // Let's do our best to make sure this never happens.
            }

            public void ShowWebPageInfo(WebPageInfo info)
            {
            }
            public void ShowWebContextMenu(WebContextMenuInfo info)
            {
            }
            public void ShowPopupMenu(WebPopupMenuInfo info)
            {
            }
            public void HidePopupMenu(bool cancel)
            {
            }
            public void Print()
            {
            }


            public void ShowToolTip(string toolTipText)
            {
                // TODO: Should implement tooltips.
            }
            public void HideToolTip()
            {
                // TODO: Should implement tooltips.
            }

            public void Dispose()
            {
            }
        }

        public void SetupInput()
        {
            CluwneLib.Screen.KeyReleased += OnKeyUp;
            CluwneLib.Screen.KeyPressed += OnKeyDown;
            CluwneLib.Screen.TextEntered += OnTextEntered;

            CluwneLib.Screen.MouseButtonPressed += OnMouseDown;
            CluwneLib.Screen.MouseButtonReleased += OnMouseUp;
            CluwneLib.Screen.MouseMoved += OnMouseMove;
            CluwneLib.Screen.MouseWheelMoved += OnMouseWheel;
            CluwneLib.Screen.MouseEntered += OnMouseEnter;
            CluwneLib.Screen.MouseLeft += OnMouseExit;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (HasKeyboardFocus && webview != null)
                webview.BeginInvoke((Action)delegate()
                {
                    webview.InjectKeyboardEvent(ConvertKeyEventArgs(e, WebKeyboardEventType.KeyUp));
                }, null);
            //else
            //    IoCManager.Resolve<IStateManager>().KeyDown(e);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (HasKeyboardFocus && webview != null)
                webview.BeginInvoke((Action)delegate()
                {
                    webview.InjectKeyboardEvent(ConvertKeyEventArgs(e, WebKeyboardEventType.KeyDown));
                }, null);
            //else
            //    IoCManager.Resolve<IStateManager>().KeyDown(e);
        }

        private void OnTextEntered(object sender, TextEventArgs e)
        {
            if (HasKeyboardFocus && webview != null)
                webview.BeginInvoke((Action)delegate()
                {
                    webview.InjectKeyboardEvent(new WebKeyboardEvent()
                    {
                        Type = WebKeyboardEventType.Char,
                        Text = e.Unicode
                    });
                }, null);
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (webview != null)
                webview.BeginInvoke((Action)delegate()
                {
                    switch (e.Button)
                    {
                        case Mouse.Button.Left:
                            webview.InjectMouseDown(MouseButton.Left);
                            break;
                        case Mouse.Button.Middle:
                            webview.InjectMouseDown(MouseButton.Middle);
                            break;
                        case Mouse.Button.Right:
                            webview.InjectMouseDown(MouseButton.Right);
                            break;
                    }
                }, null);
            //IoCManager.Resolve<IStateManager>().MouseDown(e);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (webview != null)
                webview.BeginInvoke((Action)delegate()
                {
                    switch (e.Button)
                    {
                        case Mouse.Button.Left:
                            webview.InjectMouseUp(MouseButton.Left);
                            break;
                        case Mouse.Button.Middle:
                            webview.InjectMouseUp(MouseButton.Middle);
                            break;
                        case Mouse.Button.Right:
                            webview.InjectMouseUp(MouseButton.Right);
                            break;
                    }
                }, null);
            //IoCManager.Resolve<IStateManager>().MouseUp(e);
        }

        private void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            if (webview != null)
                webview.BeginInvoke((Action)delegate()
                {
                    webview.InjectMouseMove(e.X, e.Y);
                }, null);
            //IoCManager.Resolve<IStateManager>().MouseMove(e);
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
        }

        private void OnMouseEnter(object sender, EventArgs e)
        {
        }

        private void OnMouseExit(object sender, EventArgs e)
        {
        }

        #endregion
    }
}
