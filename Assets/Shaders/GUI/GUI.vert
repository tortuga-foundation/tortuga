#version 460

layout(set = 0, binding = 0) readonly uniform ProjectionMatrixBuffer
{
    mat4 projection;
};
layout(set = 1, binding = 0) readonly uniform ModelMatrix
{
    vec2 position;
    vec2 scale;
    float rotation;
} model;

layout(location=0) out vec2 outUV;

void main()
{
    vec2 scalePlusPosition = model.scale + model.position;

    vec2 vertexPositions[6] = vec2[](
        model.position,
        vec2(scalePlusPosition.x, model.position.y),
        vec2(model.position.x, scalePlusPosition.y),
        vec2(model.position.x, scalePlusPosition.y),
        vec2(scalePlusPosition.x, model.position.y),
        vec2(scalePlusPosition.x, scalePlusPosition.y)
    );
    vec2 uv[] = vec2[](
        vec2(1, 0),
        vec2(1, 1),
        vec2(0, 0),
        vec2(0, 0),
        vec2(1, 1),
        vec2(0, 1)
    );

    gl_Position = projection * vec4(vertexPositions[gl_VertexIndex], 0, 1);
	outUV = uv[gl_VertexIndex];
}