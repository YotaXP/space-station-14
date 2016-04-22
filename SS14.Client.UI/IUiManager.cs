using SFML.Graphics;

namespace SS14.Client.UI
{
    public interface IUiManager
    {
        void Resize(int width, int height);
        void Draw(RenderTarget target);

        void SetupInput();
        void Shutdown();
    }
}
