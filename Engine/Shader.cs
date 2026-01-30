using System;

namespace Lifeblood.Engine
{
    /// <summary>
    /// Заглушка для Shader (не используется в GDI+ версии)
    /// </summary>
    public class Shader
    {
        public int ProgramID { get; private set; }

        public Shader(string vertexSource, string fragmentSource)
        {
            // Заглушка - шейдеры не используются в GDI+ версии
            ProgramID = 0;
        }

        public void Use() { }
        public void SetVector3(string name, Vector3 vector) { }
        public void SetFloat(string name, float value) { }
        public void SetInt(string name, int value) { }
        public void Dispose() { }

        public static Shader CreateDefaultShader()
        {
            return new Shader("", "");
        }
    }
}
