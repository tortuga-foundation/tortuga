#version 460
#extension GL_ARB_separate_shader_objects : enable

layout(set=0,binding=0) readonly uniform CAMERA_MVP
{
    mat4 view;
    mat4 projection;
};
layout(set=1,binding=0) readonly uniform MESH_MVP
{
    mat4 model;
};

layout(location = 0) in vec3 inNormal;

layout(location = 0) out vec4 outColor;

void main() {
    outColor = vec4(1.0);
}