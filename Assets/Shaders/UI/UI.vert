#version 460

layout(set = 0, binding = 0) readonly uniform ProjectionMatrixBuffer
{
    mat4 projection;
};
layout(set = 1, binding = 0) readonly uniform DATA
{
    vec2 position;
    vec2 scale;
    vec4 color;
} model;

layout(location=0) out vec2 outUV;
layout(location=1) out vec2 outPosition;

void main()
{
    vec2 scalePlusPosition = vec2(
        model.position.x + model.scale.x,
        model.position.y + model.scale.y
    );

    vec2 vertexPositions[6] = vec2[](
        vec2(model.position.x, model.position.y),
        vec2(model.position.x, scalePlusPosition.y),
        vec2(scalePlusPosition.x, model.position.y),
        vec2(model.position.x, scalePlusPosition.y),
        vec2(scalePlusPosition.x, scalePlusPosition.y),
        vec2(scalePlusPosition.x, model.position.y)
    );
    vec2 uv[] = vec2[](
        vec2(1, 0),
        vec2(1, 1),
        vec2(0, 0),
        vec2(0, 0),
        vec2(1, 1),
        vec2(0, 1)
    );

    vec2 position = vertexPositions[gl_VertexIndex];
    gl_Position = projection * vec4(position, 1, 1);

    //outputs
	outUV = uv[gl_VertexIndex];
    outPosition = position;
}