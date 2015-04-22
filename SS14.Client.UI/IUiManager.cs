using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS14.Client.UI
{
    public interface IUiManager
    {
        void Resize(int width, int height);
        void Draw();
        void Shutdown();
    }
}
