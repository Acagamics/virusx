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
            public Vector2 Texcoord;
            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
                        (new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                         new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));
        };
        struct ParticleInstance
        {
            public Vector2 InstancePosition;
            public Vector2 InstanceDirection;
            public float Size;
            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
                                    (new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
                                     new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 2),
                                     new VertexElement(16, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 3));
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

        public BackgroundParticles(GraphicsDevice device, ContentManager content, int numParticles)
        {
            particleEffect = content.Load<Effect>("shader/backgroundParticles");

            // particle
            particleVertexBuffer = new VertexBuffer(device, ParticleVertex.VertexDeclaration, 4, BufferUsage.None);
            var vertices = new ParticleVertex[4];
            vertices[0].Position = new Vector2(-0.5f, -0.5f); vertices[0].Texcoord = new Vector2(0.0f, 0.0f);
            vertices[1].Position = new Vector2(-0.5f, 0.5f); vertices[1].Texcoord = new Vector2(0.0f, 1.0f);
            vertices[2].Position = new Vector2(0.5f, -0.5f); vertices[2].Texcoord = new Vector2(1.0f, 0.0f);
            vertices[3].Position = new Vector2(0.5f, 0.5f); vertices[3].Texcoord = new Vector2(1.0f, 1.0f);
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

               // particles[i].InstancePosition = 
            }
        }
    }
}
