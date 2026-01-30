using System;
using System.Runtime.InteropServices;

namespace Lifeblood.Engine
{
    /// <summary>
    /// Native OpenGL bindings for pure 3D without OpenTK.
    /// </summary>
    public static class GL
    {
        private const string DLL_NAME = "opengl32.dll";

        // Constants
        public const uint GL_COLOR_BUFFER_BIT = 0x00004000;
        public const uint GL_DEPTH_BUFFER_BIT = 0x00000100;
        public const uint GL_TRIANGLES = 0x0004;
        public const uint GL_QUADS = 0x0007;
        public const uint GL_LINES = 0x0001;
        public const uint GL_TEXTURE_2D = 0x0DE1;
        public const uint GL_MODELVIEW = 0x1700;
        public const uint GL_PROJECTION = 0x1701;
        public const uint GL_DEPTH_TEST = 0x0B71;
        public const uint GL_CULL_FACE = 0x0B44;
        public const uint GL_BLEND = 0x0BE2;
        public const uint GL_SRC_ALPHA = 0x0302;
        public const uint GL_ONE_MINUS_SRC_ALPHA = 0x0303;
        public const uint GL_UNSIGNED_BYTE = 0x1401;
        public const uint GL_RGBA = 0x1908;
        public const uint GL_NEAREST = 0x2600;
        public const uint GL_LINEAR = 0x2601;
        public const uint GL_TEXTURE_MAG_FILTER = 0x2800;
        public const uint GL_TEXTURE_MIN_FILTER = 0x2801;

        [DllImport(DLL_NAME)]
        public static extern void glClear(uint mask);

        [DllImport(DLL_NAME)]
        public static extern void glClearColor(float red, float green, float blue, float alpha);

        [DllImport(DLL_NAME)]
        public static extern void glEnable(uint cap);

        [DllImport(DLL_NAME)]
        public static extern void glDisable(uint cap);

        [DllImport(DLL_NAME)]
        public static extern void glMatrixMode(uint mode);

        [DllImport(DLL_NAME)]
        public static extern void glLoadIdentity();
        
        [DllImport("glu32.dll")]
        public static extern void gluPerspective(double fovy, double aspect, double zNear, double zFar);
        
        [DllImport("glu32.dll")]
        public static extern void gluLookAt(double eyeX, double eyeY, double eyeZ, 
                                          double centerX, double centerY, double centerZ, 
                                          double upX, double upY, double upZ);

        [DllImport(DLL_NAME)]
        public static extern void glBegin(uint mode);
        
        [DllImport("glu32.dll")]
        public static extern void gluOrtho2D(double left, double right, double bottom, double top);
        
        [DllImport(DLL_NAME)]
        public static extern void glOrtho(double left, double right, double bottom, double top, double zNear, double zFar);

        [DllImport(DLL_NAME)]
        public static extern void glEnd();

        [DllImport(DLL_NAME)]
        public static extern void glVertex3f(float x, float y, float z);

        [DllImport(DLL_NAME)]
        public static extern void glColor3f(float red, float green, float blue);
        
        [DllImport(DLL_NAME)]
        public static extern void glTexCoord2f(float s, float t);

        [DllImport(DLL_NAME)]
        public static extern void glViewport(int x, int y, int width, int height);

        [DllImport(DLL_NAME)]
        public static extern void glGenTextures(int n, uint[] textures);

        [DllImport(DLL_NAME)]
        public static extern void glBindTexture(uint target, uint texture);

        [DllImport(DLL_NAME)]
        public static extern void glTexParameteri(uint target, uint pname, int param);

        [DllImport(DLL_NAME)]
        public static extern void glTexImage2D(uint target, int level, int internalformat, 
                                             int width, int height, int border, uint format, 
                                             uint type, IntPtr pixels);
        
        [DllImport(DLL_NAME)]
        public static extern void glRotatef(float angle, float x, float y, float z);
        
        [DllImport(DLL_NAME)]
        public static extern void glTranslatef(float x, float y, float z);
        
        [DllImport(DLL_NAME)]
        public static extern void glScalef(float x, float y, float z);

        [DllImport(DLL_NAME)]
        public static extern void glPushMatrix();

        [DllImport(DLL_NAME)]
        public static extern void glPopMatrix();

        [DllImport(DLL_NAME)]
        public static extern void glBlendFunc(uint sfactor, uint dfactor);
    }

    public static class WGL
    {
        [DllImport("gdi32.dll")]
        public static extern int ChoosePixelFormat(IntPtr hdc, [In] ref PIXELFORMATDESCRIPTOR ppfd);

        [DllImport("gdi32.dll")]
        public static extern bool SetPixelFormat(IntPtr hdc, int iPixelFormat, [In] ref PIXELFORMATDESCRIPTOR ppfd);

        [DllImport("gdi32.dll")]
        public static extern bool SwapBuffers(IntPtr hdc);

        [DllImport("opengl32.dll")]
        public static extern IntPtr wglCreateContext(IntPtr hdc);

        [DllImport("opengl32.dll")]
        public static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hglrc);

        [DllImport("opengl32.dll")]
        public static extern bool wglDeleteContext(IntPtr hglrc);

        [StructLayout(LayoutKind.Sequential)]
        public struct PIXELFORMATDESCRIPTOR
        {
            public ushort nSize;
            public ushort nVersion;
            public uint dwFlags;
            public byte iPixelType;
            public byte cColorBits;
            public byte cRedBits;
            public byte cRedShift;
            public byte cGreenBits;
            public byte cGreenShift;
            public byte cBlueBits;
            public byte cBlueShift;
            public byte cAlphaBits;
            public byte cAlphaShift;
            public byte cAccumBits;
            public byte cAccumRedBits;
            public byte cAccumGreenBits;
            public byte cAccumBlueBits;
            public byte cAccumAlphaBits;
            public byte cDepthBits;
            public byte cStencilBits;
            public byte cAuxBuffers;
            public byte iLayerType;
            public byte bReserved;
            public uint dwLayerMask;
            public uint dwVisibleMask;
            public uint dwDamageMask;
        }

        public const uint PFD_DRAW_TO_WINDOW = 4;
        public const uint PFD_SUPPORT_OPENGL = 32;
        public const uint PFD_DOUBLEBUFFER = 1;
        public const byte PFD_TYPE_RGBA = 0;
        public const byte PFD_MAIN_PLANE = 0;
    }
}
