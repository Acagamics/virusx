using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ParticleStormControl
{
    class BackgroundParticles
    {
        struct ParticleVertex
        {
            public Vector2 Position;
            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
                        (new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0));
        };
        struct ParticleInstance
        {
            public Vector2 InstancePosition;
            public Vector2 InstanceDirection;
            public float Size;
            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
                                    (new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                                     new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
                                     new VertexElement(16, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2));
        };

        /// <summary>
        /// static vertexbuffer containing a single quad, used for all particles
        /// </summary>
        private VertexBuffer particleVertexBuffer;
        /// <summary>
        /// instanced primitives need an indexbuffer
        /// containing only 0,1,2,3 - <b>silly</b>, but needed for instancing
        /// </summary>
        private IndexBuffer particleIndexBuffer;
        /// <summary>
        /// vertexbuffer with instance data
        /// </summary>
        private VertexBuffer instanceVertexBuffer;

        // bindings
        private VertexBufferBinding particleVertexBufferBinding;
        private VertexBufferBinding instanceVertexBufferBinding;

        /// <summary>
        /// effect
        /// </summary>
        private Effect particleEffect;

        private int numParticles;

        private const float PARTICLE_SPEED = 0.00001f;

        public BackgroundParticles(GraphicsDevice device, ContentManager content, int numParticles)
        {
            this.numParticles = numParticles;
            particleEffect = content.Load<Effect>("shader/backgroundParticles");

            // particle
            particleVertexBuffer = new VertexBuffer(device, ParticleVertex.VertexDeclaration, 4, BufferUsage.None);
            var vertices = new ParticleVertex[4];
            vertices[0].Position = new Vector2(-0.5f, -0.5f);
            vertices[1].Position = new Vector2(-0.5f, 0.5f);
            vertices[2].Position = new Vector2(0.5f, -0.5f);
            vertices[3].Position = new Vector2(0.5f, 0.5f);
            particleVertexBuffer.SetData(vertices);
            particleVertexBufferBinding = new VertexBufferBinding(particleVertexBuffer, 0, 0);

            // indexbuffer
            particleIndexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, 4, BufferUsage.WriteOnly);
            particleIndexBuffer.SetData(new ushort[] { 0, 1, 2, 3 });

            // instance
            instanceVertexBuffer = new VertexBuffer(device, ParticleInstance.VertexDeclaration, numParticles, BufferUsage.WriteOnly);
            instanceVertexBufferBinding = new VertexBufferBinding(instanceVertexBuffer, 0, 1);
            ParticleInstance[] particles = new ParticleInstance[numParticles];
            for (int i = 0; i < numParticles; ++i)
            {
                particles[i].InstancePosition = new Vector2((float)Random.NextDouble(), (float)Random.NextDouble()) * Level.RELATIVE_MAX;
                particles[i].InstanceDirection = Random.NextDirection();
                particles[i].Size = (float)(Random.NextDouble(0.005f, 0.008f));
            }
            instanceVertexBuffer.SetData(particles);
        }

        /// <summary>
        /// updates size settings - please call once before using!
        /// </summary>
        public void Resize(int screenWidth, int screenHeight, Point fieldPixelSize, Point fieldPixelOffset)
        {
            particleEffect.Parameters["PosScale"].SetValue(new Vector2(fieldPixelSize.X, -fieldPixelSize.Y) /
                                                             new Vector2(screenWidth, screenHeight) * 2);
            particleEffect.Parameters["PosOffset"].SetValue(new Vector2(fieldPixelOffset.X, -fieldPixelOffset.Y) /
                                                             new Vector2(screenWidth, screenHeight) * 2 - new Vector2(1, -1));
            particleEffect.Parameters["RelativeMax"].SetValue(Level.RELATIVE_MAX);
        }


        public void Draw(GraphicsDevice device, float passedTime)
        {
            particleEffect.Parameters["ParticleMoving"].SetValue(passedTime* PARTICLE_SPEED);
            particleEffect.CurrentTechnique.Passes[0].Apply();
            device.SetVertexBuffers(particleVertexBufferBinding, instanceVertexBufferBinding);
            device.DrawInstancedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 4, 0, 2, numParticles);
        }
    }
}
