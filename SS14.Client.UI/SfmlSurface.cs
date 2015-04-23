using Awesomium.Core;
using SFML.Graphics;
using SS14.Client.Graphics;
using SS14.Client.Graphics.Render;
using SS14.Client.Graphics.Shader;
using SS14.Client.Graphics.Sprite;
using System;
using System.Collections.Concurrent;
using Marshal = System.Runtime.InteropServices.Marshal;
using SystemColor = System.Drawing.Color;

namespace SS14.Client.UI
{
    public class SfmlSurface : Surface
    {
        // Belonging to main thread.  Hungarian notation to make it more obvious.
        private static Shader mtShader;
        private RenderImage mtImage;
        private Sprite mtSurfaceSprite;

        // Shared
        private ConcurrentQueue<Action> changeQueue = new ConcurrentQueue<Action>();

        protected override void OnInitialized(object sender, SurfaceInitializedEventArgs e)
        {
            if (this.IsDisposed)
                return;

            var width = e.View.Width;
            var height = e.View.Height;
            changeQueue.Enqueue(() =>
            {
                mtImage = new RenderImage((uint)width, (uint)height);
                mtSurfaceSprite = new Sprite(mtImage.Texture);
            });

            base.OnInitialized(sender, e);
        }

        protected override void OnResized(object sender, SurfaceResizedEventArgs e)
        {
            if (this.IsDisposed)
                return;

            changeQueue.Enqueue(() =>
            {
                mtImage.Dispose();
                mtImage = new RenderImage((uint)e.NewWidth, (uint)e.NewHeight);
                mtSurfaceSprite.Texture = mtImage.Texture;
            });

            base.OnResized(sender, e);
        }


        protected override void Paint(IntPtr srcBuffer, int srcRowSpan, AweRect srcRect, AweRect destRect)
        {
            if (this.IsDisposed)
                return;

            var cropped = new byte[srcRect.Width * srcRect.Height * 4];

            if (srcRowSpan == srcRect.Width * 4)
            {
                Marshal.Copy(srcBuffer + srcRowSpan * srcRect.Y, cropped, 0, cropped.Length);
            }
            else
            {
                for (int destY = 0, srcY = srcRect.Y; destY < srcRect.Height; ++destY, ++srcY)
                    Marshal.Copy(srcBuffer + srcRowSpan * srcY + srcRect.X * 4, cropped, destY * srcRect.Width * 4, srcRect.Width * 4);
            }

            changeQueue.Enqueue(() =>
            {
                if (mtShader == null)
                {
                    var asm = System.Reflection.Assembly.GetExecutingAssembly();
                    string fragShader;
                    using (var fragStream = asm.GetManifestResourceStream(string.Format("{0}.ColorFix.frag", asm.GetName().Name)))
                        fragShader = new System.IO.StreamReader(fragStream).ReadToEnd();
                    mtShader = Shader.FromString(null, fragShader);
                    mtShader.SetParameter("texture", SFML.Graphics.Shader.CurrentTexture);
                }

                using (var img = new Image((uint)srcRect.Width, (uint)srcRect.Height, cropped))
                using (var tex = new Texture(img))
                using (var sprite = new CluwneSprite(tex))
                {
                    sprite.SetPosition(destRect.X, destRect.Y);
                    mtImage.Draw(sprite, new RenderStates(BlendMode.None, Transform.Identity, null, mtShader));
                }

            });

            base.Paint(srcBuffer, srcRowSpan, srcRect, destRect);
        }

        protected override void Scroll(int dx, int dy, AweRect clipRect)
        {
            if (this.IsDisposed)
                return;

            changeQueue.Enqueue(() =>
            {
                mtImage.BeginDrawing();

                var src = new IntRect(clipRect.X, clipRect.Y, clipRect.Width, clipRect.Height);
                var dest = new IntRect(clipRect.X + dx, clipRect.Y + dy, clipRect.Width, clipRect.Height);

                mtImage.Blit(dest, src, SystemColor.White);

                mtImage.EndDrawing();
            });

            base.Scroll(dx, dy, clipRect);
        }

        // Callable from main thread.
        public void Draw()
        {
            if (this.IsDisposed)
                return;

            Action step;
            while (changeQueue.TryDequeue(out step))
                step();

            if (mtImage != null && mtSurfaceSprite != null)
            {
                mtImage.Display();
                mtSurfaceSprite.Draw(CluwneLib.CurrentRenderTarget, RenderStates.Default);
            }
        }

        public override void Dispose()
        {
            if (mtShader != null)
                mtShader.Dispose();
            if (mtSurfaceSprite != null)
                mtSurfaceSprite.Dispose();
            if (mtImage != null)
                mtImage.Dispose();

            base.Dispose();
        }
    }
}
