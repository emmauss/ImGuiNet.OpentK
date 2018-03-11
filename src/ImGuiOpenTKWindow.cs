using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using ImGuiNET;
using System.IO;
using System.Runtime.InteropServices;

namespace ImGuiOpenTK
{
    public class ImGuiOpenTKWindow : OpenTKWindow {

        protected readonly bool _IsSuperClass;

        protected double g_Time = 0.0f;
        protected readonly bool[] g_MousePressed = { false, false, false };
        protected float g_MouseWheel = 0.0f;
        protected int g_FontTexture = 0;

        public System.Numerics.Vector2 Position {
            get {
                return new System.Numerics.Vector2(X, Y);
            }
            set {
                X = (int)Math.Round(value.X);
                Y = (int) Math.Round(value.Y);
            }
        }

        public new Point Size {
            get
            {
                return new Point(Width, Height);
            }
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        public ImGuiOpenTKWindow(
            string title = "ImGui.NET-OpenTK-CS Window",
            int width = 800, int height = 600) : base(title, width, height) {
            _IsSuperClass = GetType() == typeof(ImGuiOpenTKWindow);
            ImGuiOpenTKHelper.Init();
            OnEvent = ImGuiOnEvent;
            OnLoop = ImGuiOnLoop;

        }

        public bool ImGuiOnEvent(OpenTKWindow window, TKEvent e)
            => ImGuiOpenTKHelper.HandleEvent(e);

        public void ImGuiOnLoop(OpenTKWindow window)
        {
            GL.ClearColor(0.1f, 0.125f, 0.15f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            ImGuiRender();
        }

        public override void Start() {
            if (!File.Exists("imgui.ini"))
                File.WriteAllText("imgui.ini", "");

            Create();

            base.Start();
        }

        private unsafe void UpdateImGuiInput(IO io)
        {
            MouseState cursorState = Mouse.GetCursorState();
            MouseState mouseState = Mouse.GetState();

            if (Focused)
            {
                Point windowPoint = PointToClient(new Point(cursorState.X, cursorState.Y));
                io.MousePosition = new System.Numerics.Vector2(windowPoint.X / io.DisplayFramebufferScale.X, windowPoint.Y / io.DisplayFramebufferScale.Y);
            }
            else
            {
                io.MousePosition = new System.Numerics.Vector2(-1f, -1f);
            }

            io.MouseDown[0] = mouseState.LeftButton == ButtonState.Pressed;
            io.MouseDown[1] = mouseState.RightButton == ButtonState.Pressed;
            io.MouseDown[2] = mouseState.MiddleButton == ButtonState.Pressed;

            float newWheelPos = mouseState.WheelPrecise;
            float delta = newWheelPos - g_MouseWheel;
            g_MouseWheel = newWheelPos;
            io.MouseWheel += delta;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            OpenTK.Graphics.OpenGL.GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.ColorBufferBit);
            base.OnUpdateFrame(e);
            UpdateImGuiInput(ImGui.GetIO());
            ImGuiRender();            
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {            
            Swap();
        }

        public virtual void ImGuiRender() {
            //if(Mouse?.GetState().LeftButton == OpenTK.Input.ButtonState.Pressed)
            ImGuiOpenTKHelper.NewFrame(Size, System.Numerics.Vector2.One, ref g_Time);

            ImGuiLayout();

            ImGuiOpenTKHelper.Render(Size);
        }

        public virtual void ImGuiLayout() {
            if (_IsSuperClass)
                ImGui.Text($"Create a new class inheriting {GetType().FullName}, overriding {nameof(ImGuiLayout)}!");
            else
                ImGui.Text($"Override {nameof(ImGuiLayout)} in {GetType().FullName}!");
        }

        protected  unsafe virtual void Create() {
            IO io = ImGui.GetIO();

            // Build texture atlas
            FontTextureData texData = io.FontAtlas.GetTexDataAsAlpha8();

            // Create OpenGL texture
            g_FontTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, g_FontTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Alpha,
                texData.Width,
                texData.Height,
                0,
                PixelFormat.Alpha,
                PixelType.UnsignedByte,
                new IntPtr(texData.Pixels));

            // Store the texture identifier in the ImFontAtlas substructure.
            io.FontAtlas.SetTexID(g_FontTexture);

            // Cleanup (don't clear the input data if you want to append new fonts later)
            //io.Fonts->ClearInputData();
            io.FontAtlas.ClearTexData();
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        protected override void Dispose(bool disposing) {
            /*ImGuiIO io = ImGui.GetIO();

            if (disposing) {
                // Dispose managed state (managed objects).
            }

            // Free unmanaged resources (unmanaged objects) and override a finalizer below.
            // Set large fields to null.
            if (g_FontTexture != 0) {
                // Texture gets deleted with the context.
                // GL.DeleteTexture(g_FontTexture);
                if ((int) io.FontAtlas.TexID == g_FontTexture)
                    io.FontAtlas.TexID = IntPtr.Zero;
                g_FontTexture = 0;
            }
            */
            base.Dispose(disposing);
        }

        ~ImGuiOpenTKWindow() {
            Dispose(false);
        }

    }
}
