#version 460

struct LightInfo
{
    vec4 position;
    vec4 forward;
    vec4 color;
    int type;
    float intensity;
    float range;
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

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec2 inTexture;
layout(location = 2) in vec3 inNormal;
layout(location = 3) in vec3 inTangent;
layout(location = 4) in vec3 inBiTangent;

layout(location = 0) out vec3 outNormal;
layout(location = 1) out vec3 outLightVector[10];

void main() {
    vec4 worldPosition = model * vec4(inPosition, 1.0);
    gl_Position = projection * view * worldPosition;

    outNormal = (model * vec4(inNormal, 0.)).xyz;
    for (int i = 0; i < lightsCount; i++)
        outLightVector[i] = lights[i].position.xyz - worldPosition.xyz;
}