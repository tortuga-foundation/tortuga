#version 460

layout(set = 0, binding = 0) uniform ProjectionMatrixBuffer
{
    mat4 projection;
};

layout(location=0) in vec2 inPosition;
layout(location=1) in vec2 inUV;

layout(location=0) out vec2 outUV;

void main()
{
    gl_Position = projection * vec4(inPosition, 0, 1);
	outUV = inUV;
}