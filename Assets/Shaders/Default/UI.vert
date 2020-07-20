#version 460

layout(set = 0, binding = 0) readonly uniform ProjectionMatrixBuffer
{
    mat4 projection;
};
layout(set = 1, binding = 0) readonly uniform DATA
{
    vec2 position;
    vec2 scale;
    vec4 borderRadius;
    vec4 color;
} model;

layout(location=0) in vec2 inPosition;
layout(location=1) in vec2 inUV;

layout(location=0) out vec2 outUV;
layout(location=1) out vec2 outPosition;

void main()
{
    gl_Position = projection * vec4(inPosition, 1, 1);

    //outputs
	outUV = inUV;
    outPosition = inPosition;
}