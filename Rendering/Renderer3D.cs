using System;
using System.Drawing;
using Lifeblood.Engine;
using Lifeblood.Network;

namespace Lifeblood.Rendering
{
    public class Renderer3D
    {
        private Texture defaultTexture;
        private Texture wallTexture;
        private Texture floorTexture;
        private Texture boxTexture;
        
        public Mesh cubeMesh;
        public Mesh enemyMesh;
        private Mesh floorMesh;
        private Mesh sphereMesh;

        public Renderer3D()
        {
            // Initialize Textures
            try { defaultTexture = new Texture("content/textures/default.png"); } catch { }
            try { wallTexture = new Texture("content/textures/wall.png"); } catch { }
            try { floorTexture = new Texture("content/textures/floor.png"); } catch { }
            try { boxTexture = new Texture("content/textures/box.png"); } catch { }
            
            if (defaultTexture == null) defaultTexture = new Texture("content/textures/default.png"); // Retry or fail gracefully

            // Initialize Meshes
            floorMesh = Mesh.CreateCube(1.0f); // Temp floor as flattened cube
            sphereMesh = Mesh.CreateSphere(1.0f, 16);
            
            // Load Models
            if (System.IO.File.Exists("content/models/player.obj"))
                cubeMesh = ModelLoader.LoadObj("content/models/player.obj");
            else
                cubeMesh = Mesh.CreateCube(1.0f);

            if (System.IO.File.Exists("content/models/enemy.obj"))
                enemyMesh = ModelLoader.LoadObj("content/models/enemy.obj");
            else
                enemyMesh = sphereMesh; // Fallback
        }

        public void RenderSphere(Vector3 pos, float radius, float r, float g, float b)
        {
            RenderMesh(sphereMesh, pos, radius * 2, r, g, b); // Diameter scale
        }

        public void DrawLine(Vector3 start, Vector3 end, float r, float g, float b)
        {
             GL.glDisable(GL.GL_TEXTURE_2D);
             GL.glBegin(GL.GL_LINES);
             GL.glColor3f(r, g, b);
             GL.glVertex3f(start.X, start.Y, start.Z);
             GL.glVertex3f(end.X, end.Y, end.Z);
             GL.glEnd();
             GL.glEnable(GL.GL_TEXTURE_2D);
             GL.glColor3f(1,1,1);
        }

        public void RenderViewModel(Game.WeaponType weapon, float fireAnimTime)
        {
            GL.glPushMatrix();
            GL.glLoadIdentity();
            
            // Positioning (Right hand side, down a bit, forward)
            GL.glTranslatef(0.5f, -0.4f, -0.8f);
            
            // Recoil Animation
            if (fireAnimTime > 0)
            {
                GL.glTranslatef(0, 0, fireAnimTime * 0.2f); // Kick back
                GL.glRotatef(fireAnimTime * 10, 1, 0, 0);   // Kick up
            }

            // Check if ModLoaded model exists
            string weaponName = Game.WeaponDef.Get(weapon).Name.ToLower().Replace(" ", "");
            Mesh m =  Lifeblood.Modding.ModLoader.GetModel(weaponName);
            
            if (m != null)
            {
                 // Render Mod Model
                 GL.glRotatef(-90, 0, 1, 0); // Rotate to face forward usually
                 RenderMesh(m, Vector3.Zero, 0.1f, 1, 1, 1);
            }
            else
            {
                // Fallback Box (stick)
                GL.glScalef(0.1f, 0.1f, 0.8f);
                GL.glDisable(GL.GL_TEXTURE_2D);
                GL.glBegin(GL.GL_QUADS);
                
                // Color based on weapon
                if (weapon == Game.WeaponType.Scout) GL.glColor3f(0f, 1f, 0.5f);
                else if (weapon == Game.WeaponType.Deagle) GL.glColor3f(1f, 1f, 0f); 
                else if (weapon == Game.WeaponType.RocketLauncher) GL.glColor3f(0.5f, 0f, 0f);
                else GL.glColor3f(0.5f, 0.5f, 0.5f);

                // Simple Box
                // Front
                GL.glVertex3f(-0.5f, -0.5f,  0.5f); GL.glVertex3f( 0.5f, -0.5f,  0.5f);
                GL.glVertex3f( 0.5f,  0.5f,  0.5f); GL.glVertex3f(-0.5f,  0.5f,  0.5f);
                // Back
                GL.glVertex3f(-0.5f, -0.5f, -0.5f); GL.glVertex3f(-0.5f,  0.5f, -0.5f);
                GL.glVertex3f( 0.5f,  0.5f, -0.5f); GL.glVertex3f( 0.5f, -0.5f, -0.5f);
                // Top
                GL.glVertex3f(-0.5f,  0.5f, -0.5f); GL.glVertex3f(-0.5f,  0.5f,  0.5f);
                GL.glVertex3f( 0.5f,  0.5f,  0.5f); GL.glVertex3f( 0.5f,  0.5f, -0.5f);
                // Bottom
                GL.glVertex3f(-0.5f, -0.5f, -0.5f); GL.glVertex3f( 0.5f, -0.5f, -0.5f);
                GL.glVertex3f( 0.5f, -0.5f,  0.5f); GL.glVertex3f(-0.5f, -0.5f,  0.5f);
                // Right
                GL.glVertex3f( 0.5f, -0.5f, -0.5f); GL.glVertex3f( 0.5f,  0.5f, -0.5f);
                GL.glVertex3f( 0.5f,  0.5f,  0.5f); GL.glVertex3f( 0.5f, -0.5f,  0.5f);
                // Left
                GL.glVertex3f(-0.5f, -0.5f, -0.5f); GL.glVertex3f(-0.5f, -0.5f,  0.5f);
                GL.glVertex3f(-0.5f,  0.5f,  0.5f); GL.glVertex3f(-0.5f,  0.5f, -0.5f);
                
                GL.glEnd();
                GL.glEnable(GL.GL_TEXTURE_2D);
            }
            
            GL.glPopMatrix();
        }

        public void RenderScene()
        {
            GL.glEnable(GL.GL_TEXTURE_2D);
            if (defaultTexture != null) defaultTexture.Bind();
            GL.glColor3f(1, 1, 1);

            // Draw Floor
            if (floorTexture != null) floorTexture.Bind();
            GL.glPushMatrix();
            GL.glTranslatef(0, -5, 0);
            GL.glScalef(1000, 10, 1000); // Create large floor
            cubeMesh.Draw();
            GL.glPopMatrix();

            // Draw Test Cubes (as Walls/Boxes)
            if (boxTexture != null) boxTexture.Bind();
            else if (wallTexture != null) wallTexture.Bind();
            
            RenderMesh(cubeMesh, new Vector3(0, 25, 100), 50, 1, 1, 1); // Red Tint -> White with Tex
            RenderMesh(cubeMesh, new Vector3(100, 25, 0), 50, 1, 1, 1); // Green Tint -> White with Tex
            RenderMesh(cubeMesh, new Vector3(-100, 50, -100), 50, 1, 1, 1); // Blue Tint -> White with Tex
            
            if (defaultTexture != null) defaultTexture.Unbind();
            GL.glDisable(GL.GL_TEXTURE_2D);
        }
        
        public void RenderMesh(Mesh mesh, Vector3 pos, float scale, float r, float g, float b)
        {
            GL.glPushMatrix();
            GL.glTranslatef(pos.X, pos.Y, pos.Z);
            GL.glScalef(scale, scale, scale);
            GL.glColor3f(r, g, b);
            mesh.Draw();
            GL.glPopMatrix();
        }

        public void RenderPlayer(PlayerStatePacket p)
        {
            GL.glEnable(GL.GL_TEXTURE_2D);
            defaultTexture.Bind(); // Use same texture for player for now
            
            GL.glPushMatrix();
            GL.glTranslatef(p.PosX, p.PosY, p.PosZ);
            GL.glRotatef(-p.Yaw * 57.2958f, 0, 1, 0); 

            GL.glColor3f(1.0f, 0.0f, 0.0f); 
            
            // Draw Player as a scaled cube for now
            // Height 72, Width 32
            GL.glPushMatrix();
            GL.glScalef(32, 72, 32);
            GL.glTranslatef(0, 0.5f, 0); // Pivot at feet
            cubeMesh.Draw();
            GL.glPopMatrix();

            GL.glPopMatrix();
            
            defaultTexture.Unbind();
            GL.glDisable(GL.GL_TEXTURE_2D);
        }
    }
}
