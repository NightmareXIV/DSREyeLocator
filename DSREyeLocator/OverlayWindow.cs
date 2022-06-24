using ImGuiScene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSREyeLocator
{
    internal class OverlayWindow : Window, IDisposable
    {
        Vector2 WinSize;
        TextureWrap imgYes, imgNo1, imgNo2;
        
        public OverlayWindow() : base("DSREye Overlay",
            ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysUseWindowPadding | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.AlwaysAutoResize |
            ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoFocusOnAppearing,
            true)
        {
            this.RespectCloseHotkey = false;
            imgYes = Svc.PluginInterface.UiBuilder.LoadImage(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "yes.png"));
            imgNo1 = Svc.PluginInterface.UiBuilder.LoadImage(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "no1.png"));
            imgNo2 = Svc.PluginInterface.UiBuilder.LoadImage(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "no2.png"));
        }

        public override void PreDraw()
        {
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new(ImGuiHelpers.MainViewport.Size.X / 2 - WinSize.X / 2, P.config.VerticalOffset));
        }

        public override void Draw()
        {
            WinSize = ImGui.GetWindowSize();
        }

        public void Dispose()
        {
            imgYes.Dispose();
            imgNo1.Dispose();
            imgNo2.Dispose();
        }
    }
}
