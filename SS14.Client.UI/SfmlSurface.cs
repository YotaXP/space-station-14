﻿using Awesomium.Core;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Concurrent;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace SS14.Client.UI
{
    public class SfmlSurface : Surface
    {
        // Belonging to main thread.  Hungarian notation to make it more obvious.
        private static Shader mtShader;
        private RenderTexture mtImage;
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
                mtImage = new RenderTexture((uint)width, (uint)height);
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
                mtImage = new RenderTexture((uint)e.NewWidth, (uint)e.NewHeight);
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
                    mtShader.SetParameter("texture", Shader.CurrentTexture);
                }

                using (var img = new Image((uint)srcRect.Width, (uint)srcRect.Height, cropped))
                using (var tex = new Texture(img))
                using (var sprite = new Sprite(tex))
                {
                    sprite.Position = new Vector2f(destRect.X, destRect.Y);
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
                var tf = new Transform();
                tf.Translate(dx, dy);
                mtImage.Display();
                mtSurfaceSprite.Draw(mtImage, new RenderStates(BlendMode.None, tf, null, null));
            });

            base.Scroll(dx, dy, clipRect);
        }

        // Callable from main thread.
        public void Draw(RenderTarget target)
        {
            if (this.IsDisposed)
                return;

            Action step;
            while (changeQueue.TryDequeue(out step))
                step();

            if (mtImage != null && mtSurfaceSprite != null)
            {
                mtImage.Display();
                mtSurfaceSprite.Draw(target, RenderStates.Default);
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
