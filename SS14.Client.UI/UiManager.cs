using Awesomium.Core;
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

                WebCore.Run();
                //WebCore.Run((s, e) =>
                //{
                //    uiContext = SynchronizationContext.Current;
                //});
            });
            UiThread.Name = "Awesomium Thread";
            UiThread.Start();
        }

        WebSession session;
        WebView webview;

        SfmlSurface surface;

        public Thread UiThread;
        private SynchronizationContext uiContext;

        public void Resize(int width, int height) // TODO: May want to throttle this to prevent the resource allocation nightmare from occurring for every pixel that the mouse moves while resizing.
        {
            if (uiContext != null)
                uiContext.Post(s => webview.Resize(width, height), null);
        }

        public void Shutdown()
        {
            if (WebCore.IsRunning)
                WebCore.Shutdown();
        }

        public void Navigate(Uri uri)
        {
            if (uiContext != null)
                uiContext.Post(s => webview.Source = uri, null);
        }

        public void Draw()
        {
            if (surface != null)
                surface.Draw();
        }
    }
}
