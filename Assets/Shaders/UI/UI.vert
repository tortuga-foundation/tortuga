#version 460

layout(set = 0, binding = 0) readonly uniform ProjectionMatrixBuffer
{
    mat4 projection;
};
layout(set = 0, binding = 1) readonly uniform DATA
{
    vec4 color;
    vec4 rect;
    vec4 borderRadius;
} model;

layout(location=0) out vec2 outUV;
layout(location=1) out vec2 outPosition;

void main()
{
    vec2 scalePlusPosition = vec2(
        model.rect.x + model.rect.z,
        model.rect.y + model.rect.w
    );

    vec2 vertexPositions[6] = vec2[](
        vec2(model.rect.x, model.rect.y),
        vec2(model.rect.x, scalePlusPosition.y),
        vec2(scalePlusPosition.x, model.rect.y),
        vec2(model.rect.x, scalePlusPosition.y),
        vec2(scalePlusPosition.x, scalePlusPosition.y),
        vec2(scalePlusPosition.x, model.rect.y)
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