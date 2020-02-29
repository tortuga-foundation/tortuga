#version 460
#extension GL_ARB_separate_shader_objects : enable

struct LightInfo
{
    vec4 position;
    vec4 forward;
    vec4 color;
    int type;
    float intensity;
    float range;
    int reserved;
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
    LightInfo info[10];
    int lightsCount;
} lightData;
layout(set=3) readonly uniform MATERIAL_INFO
{
    int enableSmoothShading;
};

layout(set=4, binding=0) uniform sampler2D albedoTexture;
layout(set=5, binding=0) uniform sampler2D normalTexture;
layout(set=6, binding=0) uniform sampler2D metalTexture;
layout(set=7, binding=0) uniform sampler2D roughnessTexture;
layout(set=8, binding=0) uniform sampler2D aoTexture;

layout(location = 0) in vec3 inNormal;
layout(location = 1) in vec2 inUV;
layout(location = 2) in vec3 inCameraPosition;
layout(location = 3) in vec3 inWorldPosition;

layout(location = 0) out vec4 outColor;

vec3 getNormalFromMap();

void main() {
    vec3 albedo = texture(albedoTexture, inUV).rgb;
    float metallic = texture(metalTexture, inUV).r;
    float roughness = texture(roughnessTexture, inUV).r;
    float ao = texture(aoTexture, inUV).r;

    vec3 N = getNormalFromMap();
    vec3 V = normalize(inCameraPosition - inWorldPosition);


    outColor = vec4(1.);
}

// ----------------------------------------------------------------------------
vec3 getNormalFromMap()
{
    vec3 tangentNormal = texture(normalTexture, inUV).rbg;

    vec3 Q1  = dFdx(inWorldPosition);
    vec3 Q2  = dFdy(inWorldPosition);
    vec2 st1 = dFdx(inUV);
    vec2 st2 = dFdy(inUV);

    vec3 N   = normalize(inNormal);
    vec3 T  = normalize(Q1*st2.t - Q2*st1.t);
    vec3 B  = -normalize(cross(N, T));
    mat3 TBN = mat3(T, B, N);

    return normalize(TBN * tangentNormal);
}