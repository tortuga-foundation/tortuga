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
    vec4 borderRadius;
} model;

layout(location=0) in vec2 inPosition;
layout(location=1) in vec2 inUv;

layout(location=0) out vec2 outUV;

void main()
{
    gl_Position = projection * vec4(inPosition, 1, 1);

    //outputs
	outUV = inUv;
}