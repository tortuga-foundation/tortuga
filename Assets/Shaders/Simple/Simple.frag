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

layout(location = 0) in vec3 inNormal;
layout(location = 1) in vec3 inLightVector[10];

layout(location = 0) out vec4 outColor;

void main() {
    vec3 unitNormal = normalize(inNormal);
    vec3 unitLightVector = normalize(inLightVector[0]);
    
    float nDot1 = dot(unitNormal, unitLightVector);
    float brightness = max(nDot1, 0.);

    outColor = vec4(brightness, brightness, brightness, 1.0);
}