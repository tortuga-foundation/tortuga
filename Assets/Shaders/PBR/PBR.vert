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

layout(set=0) readonly uniform CAMERA_MVP
{
    mat4 view;
    mat4 projection;
};
layout(set=1) readonly uniform MESH_MVP
{
    mat4 model;
};
layout(set=2) readonly uniform LIGHT_SHADER_INFO
{
    int lightsCount;
    int lightReserved1;
    int lightReserved2;
    int lightReserved3;
    LightInfo info[10];
} lightData;
layout(set=3) readonly uniform MATERIAL_INFO
{
    float metallic;
    float roughness;
    int enableSmoothShading;
};

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec2 inTexture;
layout(location = 2) in vec3 inNormal;
layout(location = 3) in vec3 inTangent;
layout(location = 4) in vec3 inBiTangent;

layout(location = 0) out vec3 outNormal;
layout(location = 1) out vec2 outUV;
layout(location = 2) out vec3 outCameraDirection;
layout(location = 3) out vec3 outWorldPosition;
layout(location = 4) out mat3 TBN;

void main() {
    vec4 worldPosition = model * vec4(inPosition, 1.0);
    gl_Position = projection * view * worldPosition;

    vec4 camreaPos = inverse(view)[3];
    outCameraDirection = camreaPos.xyz - worldPosition.xyz;
    outUV = inTexture;
    if (enableSmoothShading == 1)
        outNormal = (model * normalize(vec4(inPosition, 1.0))).xyz;
    else
        outNormal = mat3(model) * inNormal;
    outWorldPosition = worldPosition.xyz;
    //TBN
    vec3 surfaceTangent = normalize(model * vec4(inTangent, 0.)).xyz;
    vec3 SurfaceBiTangent = normalize(model * vec4(inBiTangent, 0.)).xyz;
    TBN = mat3(surfaceTangent, SurfaceBiTangent, outNormal);
}