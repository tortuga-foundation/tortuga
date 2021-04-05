#pragma warning disable CS1591
using System;
using System.Numerics;

namespace Tortuga.Graphics
{
    [Serializable]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector2 TextureCoordinates;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 BiTangent;

        public static PipelineInputBuilder PipelineInput
        => new PipelineInputBuilder(
            new PipelineInputBuilder.BindingElement[]
            {
                new PipelineInputBuilder.BindingElement
                {
                    Type = PipelineInputBuilder.BindingElement.BindingType.Vertex,
                    Elements = new PipelineInputBuilder.AttributeElement[]
                    {
                        //position
                        new PipelineInputBuilder.AttributeElement(
                            PipelineInputBuilder.AttributeElement.FormatType.Float3
                        ),
                        //texture coordinates
                        new PipelineInputBuilder.AttributeElement(
                            PipelineInputBuilder.AttributeElement.FormatType.Float2
                        ),
                        //normal
                        new PipelineInputBuilder.AttributeElement(
                            PipelineInputBuilder.AttributeElement.FormatType.Float3
                        ),
                        //tangent
                        new PipelineInputBuilder.AttributeElement(
                            PipelineInputBuilder.AttributeElement.FormatType.Float3
                        ),
                        //bi-tangent
                        new PipelineInputBuilder.AttributeElement(
                            PipelineInputBuilder.AttributeElement.FormatType.Float3
                        )
                    }
                }
            }
        );
    }
}