﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace VirusX
{
    /// <summary>
    /// highly optimized particle rendering
    /// </summary>
    class ParticleRenderer
    {
        struct vertex2dPosition
        {
            public Vector2 Position;
            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
                        (new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0));
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

        // bindings
        private VertexBufferBinding particleVertexBufferBinding;

        private Texture2D[] virusTextures = new Texture2D[(int)VirusSwarm.VirusType.NUM_VIRUSES];

        private Effect particleEffect;

        private static readonly Vector2 RENDERING_SIZE_CONSTANT = new Vector2(0.009f / Level.RELATIVECOR_ASPECT_RATIO, 0.009f) / 15.0f;
        private const float minimumHealth = 5.0f;  // added to the health in rendering shader

        public ParticleRenderer(GraphicsDevice device, ContentManager content)
        {
            particleEffect = content.Load<Effect>("shader/particleRendering");
            particleEffect.Parameters["TextureSize"].SetValue((float)VirusSwarm.MAX_PARTICLES_SQRT);
            particleEffect.Parameters["HealthToSizeScale"].SetValue(RENDERING_SIZE_CONSTANT);
            particleEffect.Parameters["MinHealth"].SetValue(minimumHealth);
            particleEffect.Parameters["RelativeMax"].SetValue(Level.RELATIVE_MAX);

            particleVertexBuffer = new VertexBuffer(device, vertex2dPosition.VertexDeclaration, 4, BufferUsage.None);
            var vertices = new vertex2dPosition[4];
            vertices[0].Position = new Vector2(-0.5f, -0.5f);
            vertices[1].Position = new Vector2(-0.5f, 0.5f);
            vertices[2].Position = new Vector2(0.5f, -0.5f);
            vertices[3].Position = new Vector2(0.5f, 0.5f);
            particleVertexBuffer.SetData(vertices);

            particleIndexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
            particleIndexBuffer.SetData(new ushort[] { 0, 1, 2, 2, 1, 3 });

            particleVertexBufferBinding = new VertexBufferBinding(particleVertexBuffer, 0, 0);

            // load virus textures
            for (int i = 0; i < virusTextures.Length; ++i)
                virusTextures[i] = content.Load<Texture2D>(GetVirusTextureName((VirusSwarm.VirusType)i));
        }

        public static string GetVirusTextureName(VirusSwarm.VirusType type)
        {
            switch (type)
            {
                case VirusSwarm.VirusType.H5N1:
                    return "viruses//h5n1";
                case VirusSwarm.VirusType.HEPATITISB:
                    return "viruses//hepatitisb";
                case VirusSwarm.VirusType.HIV:
                    return "viruses//hiv";
                case VirusSwarm.VirusType.EPSTEINBARR:
                    return "viruses//epsteinbarr";
                case VirusSwarm.VirusType.EBOLA:
                    return "viruses//ebola";
                case VirusSwarm.VirusType.MARV:
                    return "viruses//marv";
                case VirusSwarm.VirusType.WNV:
                    return "viruses//wnv";
                default:
                    return "pix";
            }
        }

        private bool renderingOrderFlag = true;

        public void Draw(GraphicsDevice device, Player[] players, bool damage /*= false*/)
        {
            // constant settings
            device.Indices = particleIndexBuffer;

            // reversing rendering order every frame - this seems to affect the player damaging!
            if (damage)
                renderingOrderFlag = !renderingOrderFlag;
            if (renderingOrderFlag)
            {
                for (int i = players.Length - 1; i > -1; --i)
                    DrawIntern(device, damage, players[i]);
            }
            else
            {
                for (int i = 0; i < players.Length; ++i)
                    DrawIntern(device, damage, players[i]);
            }


            //particleEffect.Parameters["PositionTexture"].SetValue((Texture2D)null);
            device.VertexTextures[0] = null;
            device.VertexTextures[1] = null;
        }

        private void DrawIntern(GraphicsDevice device, bool damage, Player player)
        {
            if (!player.Alive)
                return;

            if (damage)
                particleEffect.CurrentTechnique = particleEffect.Techniques["DamageMap"];
            else
            {
                particleEffect.CurrentTechnique = particleEffect.Techniques["Virus"];
                particleEffect.Parameters["VirusTexture"].SetValue(virusTextures[(int)player.Virus]);
            }

            particleEffect.Parameters["PositionTexture"].SetValue(player.PositionTexture);
            particleEffect.Parameters["InfoTexture"].SetValue(player.HealthTexture);

            if (damage)
                particleEffect.Parameters["Color"].SetValue(player.DamageMapDrawColor.ToVector4());
            else
                particleEffect.Parameters["Color"].SetValue(player.ParticleColor.ToVector4());

            particleEffect.CurrentTechnique.Passes[0].Apply();

            device.SamplerStates[0] = SamplerState.PointClamp;
            device.SamplerStates[1] = SamplerState.LinearClamp;

            device.SetVertexBuffers(particleVertexBufferBinding);
            device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2, player.HighestUsedParticleIndex + 1);
        }
    }
}
