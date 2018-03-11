using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using ImGuiNET;
using TKEventType = ImGuiOpenTK.TKEvent.Type;
using System.IO;
using System.Runtime.InteropServices;

namespace ImGuiOpenTK{
    public static class ImGuiOpenTKHelper {

        private static bool _Initialized = false;
        public static bool Initialized => _Initialized;

        public static void Init() {
            if (_Initialized)
                return;
            _Initialized = true;
            
            IO io = ImGui.GetIO();
            io.KeyMap[GuiKey.Tab] = (int)Key.Tab;
            io.KeyMap[GuiKey.LeftArrow] = (int)Key.Left;
            io.KeyMap[GuiKey.RightArrow] = (int)Key.Right;
            io.KeyMap[GuiKey.UpArrow] = (int)Key.Up;
            io.KeyMap[GuiKey.DownArrow] = (int)Key.Down;
            io.KeyMap[GuiKey.PageUp] = (int)Key.PageUp;
            io.KeyMap[GuiKey.PageDown] = (int)Key.Down;
            io.KeyMap[GuiKey.Home] = (int)Key.Home;
            io.KeyMap[GuiKey.End] = (int)Key.End;
            io.KeyMap[GuiKey.Delete] = (int)Key.Delete;
            io.KeyMap[GuiKey.Backspace] = (int)Key.BackSpace;
            io.KeyMap[GuiKey.Enter] = (int)Key.Enter;
            io.KeyMap[GuiKey.Escape] = (int)Key.Escape;
            io.KeyMap[GuiKey.A] = (int)Key.A;
            io.KeyMap[GuiKey.C] = (int)Key.C;
            io.KeyMap[GuiKey.V] = (int)Key.V;
            io.KeyMap[GuiKey.X] = (int)Key.X;
            io.KeyMap[GuiKey.Y] = (int)Key.Y;
            io.KeyMap[GuiKey.Z] = (int)Key.Z;

        }

        public unsafe static void NewFrame(Point size, System.Numerics.Vector2 scale, ref double g_Time) {
            IO io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(size.X,size.Y);
            io.DisplayFramebufferScale = scale;

            double currentTime = Environment.TickCount / 1000D;
            io.DeltaTime = g_Time > 0D ? (float) (currentTime - g_Time) : (1f/60f);
            g_Time = currentTime;

            //SDL.SDL_ShowCursor(io.MouseDrawCursor ? 0 : 1);

            ImGui.NewFrame();
        }

        public unsafe static void Render(Point size) {
            ImGui.Render();
            if (ImGuiNative.igGetIO()->RenderDrawListsFn == IntPtr.Zero)
                RenderDrawData(ImGuiNative.igGetDrawData(), size.X, size.Y);
        }
        
        public unsafe static void RenderDrawData(DrawData* drawData, int displayW, int displayH) {
            // We are using the OpenGL fixed pipeline to make the example code simpler to read!
            // Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled, vertex/texcoord/color pointers.
            System.Numerics.Vector4 clear_color = new System.Numerics.Vector4(114f / 255f, 144f / 255f, 154f / 255f, 1.0f);
            GL.Viewport(0, 0, displayW, displayH);
            GL.ClearColor(clear_color.X, clear_color.Y, clear_color.Z, clear_color.W);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            int last_texture;
            GL.GetInteger(GetPName.TextureBinding2D, out last_texture);
            GL.PushAttrib(AttribMask.EnableBit | AttribMask.ColorBufferBit | AttribMask.TransformBit);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.ScissorTest);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.Enable(EnableCap.Texture2D);

            GL.UseProgram(0);

            // Handle cases of screen coordinates != from framebuffer coordinates (e.g. retina displays)
            IO io = ImGui.GetIO();
            ImGui.ScaleClipRects(drawData, io.DisplayFramebufferScale);

            // Setup orthographic projection matrix
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(
                0.0f,
                io.DisplaySize.X / io.DisplayFramebufferScale.X,
                io.DisplaySize.Y / io.DisplayFramebufferScale.Y,
                0.0f,
                -1.0f,
                1.0f);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            // Render command lists

            for (int n = 0; n < drawData->CmdListsCount; n++)
            {
                NativeDrawList* cmd_list = drawData->CmdLists[n];
                byte* vtx_buffer = (byte*)cmd_list->VtxBuffer.Data;
                ushort* idx_buffer = (ushort*)cmd_list->IdxBuffer.Data;

                DrawVert vert0 = *((DrawVert*)vtx_buffer);
                DrawVert vert1 = *(((DrawVert*)vtx_buffer) + 1);
                DrawVert vert2 = *(((DrawVert*)vtx_buffer) + 2);

                GL.VertexPointer(2, VertexPointerType.Float, sizeof(DrawVert), new IntPtr(vtx_buffer + DrawVert.PosOffset));
                GL.TexCoordPointer(2, TexCoordPointerType.Float, sizeof(DrawVert), new IntPtr(vtx_buffer + DrawVert.UVOffset));
                GL.ColorPointer(4, ColorPointerType.UnsignedByte, sizeof(DrawVert), new IntPtr(vtx_buffer + DrawVert.ColOffset));

                for (int cmd_i = 0; cmd_i < cmd_list->CmdBuffer.Size; cmd_i++)
                {
                    DrawCmd* pcmd = &(((DrawCmd*)cmd_list->CmdBuffer.Data)[cmd_i]);
                    if (pcmd->UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.BindTexture(TextureTarget.Texture2D, pcmd->TextureId.ToInt32());
                        GL.Scissor(
                            (int)pcmd->ClipRect.X,
                            (int)(io.DisplaySize.Y - pcmd->ClipRect.W),
                            (int)(pcmd->ClipRect.Z - pcmd->ClipRect.X),
                            (int)(pcmd->ClipRect.W - pcmd->ClipRect.Y));
                        ushort[] indices = new ushort[pcmd->ElemCount];
                        for (int i = 0; i < indices.Length; i++) { indices[i] = idx_buffer[i]; }
                        GL.DrawElements(PrimitiveType.Triangles, (int)pcmd->ElemCount, DrawElementsType.UnsignedShort, new IntPtr(idx_buffer));
                    }
                    idx_buffer += pcmd->ElemCount;
                }
            }

            // Restore modified state
            GL.DisableClientState(ArrayCap.ColorArray);
            GL.DisableClientState(ArrayCap.TextureCoordArray);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.BindTexture(TextureTarget.Texture2D, last_texture);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.PopAttrib();
        }

       

                public static bool HandleEvent(TKEvent tKEvent) {
            IO io = ImGui.GetIO();
            switch (tKEvent.EventType)
            {
                case TKEventType.Keyboard:
                    var KeyboardEvent = tKEvent as KeyBoardEvent;
                    switch (KeyboardEvent.Key) {
                        case Key.ControlLeft:
                        case Key.ControlRight:
                            io.CtrlPressed = KeyboardEvent.IsKeyDown;
                            break;
                        case Key.ShiftLeft:
                        case Key.ShiftRight:
                            io.ShiftPressed = KeyboardEvent.IsKeyDown;
                            break;
                        case Key.AltLeft:
                        case Key.AltRight:
                            io.AltPressed = KeyboardEvent.IsKeyDown;
                            break;
                        default:
                            io.KeysDown[(int)KeyboardEvent.Key] = KeyboardEvent.IsKeyDown;
                            break;
                    }
                    break;
                case TKEventType.MouseButton:
                    var MouseButton = tKEvent as MouseButtonEvent;
                    switch (MouseButton.Button)
                    {
                        case OpenTK.Input.MouseButton.Left:
                            io.MouseDown[0] = MouseButton.IsButtonDown;
                            break;
                        case OpenTK.Input.MouseButton.Right:
                            io.MouseDown[1] = MouseButton.IsButtonDown;
                            break;
                        case OpenTK.Input.MouseButton.Middle:
                            io.MouseDown[2] = MouseButton.IsButtonDown;
                            break;
                    }
                    break;
                case TKEventType.MouseWheel:
                    var MouseWheel = tKEvent as MouseWheelEvent;
                    io.MouseWheel = MouseWheel.Value;
                    break;
                case TKEventType.MouseMotion:
                    var MouseMotion = tKEvent as MouseMotionEvent;
                    io.MousePosition = new System.Numerics.Vector2(MouseMotion.Position.X,MouseMotion.Position.Y);
                    break;
                case TKEventType.TextInput:
                    var TextEvent = tKEvent as TextInputEvent;
                    unsafe
                    {
                        ImGuiNative.ImGuiIO_AddInputCharactersUTF8(TextEvent.Text);
                    }
                    break;

            }
            

           /*switch (mouse.GetState) {
                case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    if (e.wheel.y > 0)
                        mouseWheel = 1;
                    if (e.wheel.y < 0)
                        mouseWheel = -1;
                    return true;
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    if (mousePressed == null)
                        return true;
                    if (e.button.button == SDL.SDL_BUTTON_LEFT && mousePressed.Length > 0)
                        mousePressed[0] = true;
                    if (e.button.button == SDL.SDL_BUTTON_RIGHT && mousePressed.Length > 1)
                        mousePressed[1] = true;
                    if (e.button.button == SDL.SDL_BUTTON_MIDDLE && mousePressed.Length > 2)
                        mousePressed[2] = true;
                    return true;
                case SDL.SDL_EventType.SDL_TEXTINPUT:
                    unsafe
                    {
                        // THIS IS THE ONLY UNSAFE THING LEFT!
                        ImGui.AddInputCharactersUTF8(e.text.text);
                    }
                    return true;
                case SDL.SDL_EventType.SDL_KEYDOWN:
                case SDL.SDL_EventType.SDL_KEYUP:
                    int key = (int) e.key.keysym.sym & ~SDL.SDLK_SCANCODE_MASK;
                    io.KeysDown[key] = e.type == SDL.SDL_EventType.SDL_KEYDOWN;
                    SDL.SDL_Keymod keyModState = SDL.SDL_GetModState();
                    io.ShiftPressed = (keyModState & SDL.SDL_Keymod.KMOD_SHIFT) != 0;
                    io.CtrlPressed = (keyModState & SDL.SDL_Keymod.KMOD_CTRL) != 0;
                    io.AltPressed = (keyModState & SDL.SDL_Keymod.KMOD_ALT) != 0;
                    io.SuperPressed = (keyModState & SDL.SDL_Keymod.KMOD_GUI) != 0;
                    return true;
            }
            */
            return true;
        }

    }
}
