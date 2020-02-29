#version 460

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

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec2 inTexture;
layout(location = 2) in vec3 inNormal;

layout(location = 0) out vec3 outNormal;
layout(location = 1) out vec2 outUV;
layout(location = 2) out vec3 outCameraPosition;
layout(location = 3) out vec3 outWorldPosition;

void main() {
    vec4 worldPosition = model * vec4(inPosition, 1.0);
    gl_Position = projection * view * worldPosition;

    //vertex position
    outWorldPosition = worldPosition.xyz;
    //texture uv
    outUV = inTexture;

    //camera position
    outCameraPosition = inverse(view)[3].xyz;

    //normals
    if (enableSmoothShading == 1)
        outNormal = (model * normalize(vec4(inPosition, 1.0))).xyz; //todo
    else
        outNormal = normalize(model * vec4(inNormal, 0.)).xyz;
}