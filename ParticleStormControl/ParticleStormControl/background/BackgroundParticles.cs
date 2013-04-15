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
        private VertexBuffer instanceVertexBuffer_Circles;
        private VertexBuffer instanceVertexBuffer_FilledCircles;

        // bindings
        private VertexBufferBinding particleVertexBufferBinding;
        private VertexBufferBinding instanceVertexBufferBinding_Circles;
        private VertexBufferBinding instanceVertexBufferBinding_FilledCircles;

        /// <summary>
        /// effect
        /// </summary>
        private Effect particleEffect;

        // last settings
        Vector2 posScale;
        Vector2 posOffset;
        Vector2 relativeMax;

        // particle numbers
        private const int NUM_CIRCLES = 1024;
        private const int NUM_FILLED_CIRCLES = 6144;

        // sizes
        private const float MIN_SIZE_CIRCLES = 0.008f;
        private const float MAX_SIZE_CIRCLES = 0.012f;
        private const float MIN_SIZE_FILLED_CIRCLES = 0.006f;
        private const float MAX_SIZE_FILLED_CIRCLES = 0.010f;

        /// <summary>
        /// particle speed
        /// </summary>
        private const float PARTICLE_SPEED = 0.00005f;

        public BackgroundParticles(GraphicsDevice device, ContentManager content)
        {
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

            // instances - circles
            instanceVertexBuffer_Circles = new VertexBuffer(device, ParticleInstance.VertexDeclaration, NUM_CIRCLES, BufferUsage.WriteOnly);
            instanceVertexBufferBinding_Circles = new VertexBufferBinding(instanceVertexBuffer_Circles, 0, 1);
            ParticleInstance[] particles = new ParticleInstance[NUM_CIRCLES];
            for (int i = 0; i < NUM_CIRCLES; ++i)
            {
                particles[i].InstancePosition = new Vector2((float)Random.NextDouble(), (float)Random.NextDouble()) * Level.RELATIVE_MAX;
                particles[i].InstanceDirection = Random.NextDirection();
                particles[i].Size = (float)(Random.NextDouble(MIN_SIZE_CIRCLES, MAX_SIZE_CIRCLES));
            }
            instanceVertexBuffer_Circles.SetData(particles);

            // instances filled circles
            instanceVertexBuffer_FilledCircles = new VertexBuffer(device, ParticleInstance.VertexDeclaration, NUM_FILLED_CIRCLES, BufferUsage.WriteOnly);
            instanceVertexBufferBinding_FilledCircles = new VertexBufferBinding(instanceVertexBuffer_FilledCircles, 0, 1);
            particles = new ParticleInstance[NUM_FILLED_CIRCLES];
            for (int i = 0; i < NUM_FILLED_CIRCLES; ++i)
            {
                particles[i].InstancePosition = new Vector2((float)Random.NextDouble(), (float)Random.NextDouble()) * Level.RELATIVE_MAX;
                particles[i].InstanceDirection = Random.NextDirection();
                particles[i].Size = (float)(Random.NextDouble(MIN_SIZE_FILLED_CIRCLES, MAX_SIZE_FILLED_CIRCLES));
            }
            instanceVertexBuffer_FilledCircles.SetData(particles);
        }

        /// <summary>
        /// updates size settings - please call once before using!
        /// </summary>
        public void Resize(int screenWidth, int screenHeight, Point fieldPixelSize, Point fieldPixelOffset, Vector2 relativeMax)
        {
            posScale = new Vector2(fieldPixelSize.X, -fieldPixelSize.Y) /
                            new Vector2(screenWidth, screenHeight) * 2;
            posOffset = new Vector2(fieldPixelOffset.X, -fieldPixelOffset.Y) /
                            new Vector2(screenWidth, screenHeight) * 2 - new Vector2(1, -1);
            this.relativeMax = relativeMax;
        }

        public void Draw(GraphicsDevice device, float passedTime)
        {
            particleEffect.Parameters["PosScale"].SetValue(posScale);
            particleEffect.Parameters["PosOffset"].SetValue(posOffset);
            particleEffect.Parameters["RelativeMax"].SetValue(relativeMax);
            particleEffect.Parameters["ParticleMoving"].SetValue(passedTime* PARTICLE_SPEED);
            device.Indices = particleIndexBuffer;

            particleEffect.CurrentTechnique.Passes[0].Apply();
            device.SetVertexBuffers(particleVertexBufferBinding, instanceVertexBufferBinding_Circles);
            device.DrawInstancedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 4, 0, 2, NUM_CIRCLES);

            particleEffect.CurrentTechnique.Passes[1].Apply();
            device.SetVertexBuffers(particleVertexBufferBinding, instanceVertexBufferBinding_FilledCircles);
            device.DrawInstancedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 4, 0, 2, NUM_FILLED_CIRCLES);
        }
    }
}
