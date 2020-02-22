#version 460

layout(set=0,binding=0) readonly uniform CAMERA_MVP
{
    mat4 view;
    mat4 projection;
};
layout(set=1,binding=0) readonly uniform MESH_MVP
{
    mat4 model;
};

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec2 inTexture;
layout(location = 2) in vec3 inNormal;
layout(location = 3) in vec3 inTangent;
layout(location = 4) in vec3 inBiTangent;

layout(location = 0) out vec2 outUV;

void main() {
    gl_Position = vec4(inPosition, 1.0);
    outUV = inTexture;
}