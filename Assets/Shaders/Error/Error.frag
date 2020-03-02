#version 460
#extension GL_ARB_separate_shader_objects : enable

struct LightInfo
{
    vec4 Position;
    vec4 Forward;
    vec4 Color;
    int Type;
    float Intensity;
    float Range;
};

layout(set=0,binding=0) readonly uniform CAMERA_MVP
{
    mat4 view;
    mat4 projection;
};
layout(set=1,binding=0) readonly uniform MESH_MVP
{
    mat4 model;
};
layout(set=2,binding=0) readonly uniform LIGHT_SHADER_INFO
{
    int lightsCount;
    int lightReserved1;
    int lightReserved2;
    int lightReserved3;
    LightInfo lights[10];
};

layout(location = 0) out vec4 outColor;

void main() {
    outColor = vec4(vec3(1., 0., 0.), 1.);
}