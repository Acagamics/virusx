using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace VirusX
{
    class ScreenTriangleRenderer
    {
        /// <summary>
        /// singleton instance for all rendering processes
        /// </summary>
        public static readonly ScreenTriangleRenderer instance = new ScreenTriangleRenderer();

        private bool initalised = false;
        private ScreenTriangleRenderer() { }

        /// <summary>
        /// Vertexbuffer for a screenfilling vertextriangle
        /// uses pretransformed coordinates
        /// texcoords are not included! use tex = pos*0.5 + 0.5
        /// </summary>
        private VertexBuffer screenTriangleVertexBuffer;

        /// <summary>
        /// even simpler - only 2d vector
        /// shader can set z and w arbitrary
        /// </summary>
        public struct ScreenAlignedTriangleVertex : IVertexType
        {
            public Vector2 PretransformedPosition;

            private static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration
                        (new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0));

            static public VertexDeclaration VertexDeclaration
            { get { return vertexDeclaration; } }
            VertexDeclaration IVertexType.VertexDeclaration
            { get { return vertexDeclaration; } }
        }

        private void Init(GraphicsDevice graphicsDevice)
        {
            screenTriangleVertexBuffer = new VertexBuffer(graphicsDevice, ScreenAlignedTriangleVertex.VertexDeclaration, 3, BufferUsage.WriteOnly);
            ScreenAlignedTriangleVertex[] screenTriangleVertices = new ScreenAlignedTriangleVertex[3];
            screenTriangleVertices[0].PretransformedPosition = new Vector2(-1.0f, -1.0f);
            screenTriangleVertices[1].PretransformedPosition = new Vector2( 3.0f, -1.0f);
            screenTriangleVertices[2].PretransformedPosition = new Vector2(-1.0f,  3.0f);
            screenTriangleVertexBuffer.SetData<ScreenAlignedTriangleVertex>(screenTriangleVertices);

            initalised = true;
        }

        /// <summary>
        /// draws a screen aligned triangle
        /// </summary>
        public void DrawScreenAlignedTriangle(GraphicsDevice device)
        {
            if (!initalised)
                Init(device);

            device.SetVertexBuffer(screenTriangleVertexBuffer);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
        }
    }
}
