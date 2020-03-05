#version 460
#extension GL_ARB_separate_shader_objects : enable

layout(set=0, binding=0) readonly uniform CAMERA_MVP
{
    mat4 view;
    mat4 projection;
    int cameraX;
    int cameraY;
    int cameraWidth;
    int cameraHeight;
};
layout(set=1, binding=0) readonly uniform UI_DATA
{
    vec2 position;
    vec2 scale;
    int isStatic;
    int indexZ;
    float borderRadius;
    int shadowType;
    vec2 shadowOffset;
    float shadowBlur;
    float shadowSpread;
    vec4 shadowColor;
};
layout(set=2, binding=0) uniform sampler2D albedo;

layout(location = 0) in vec2 inUV;

layout(location = 0) out vec4 outColor;

void main() {
    outColor = texture(albedo, inUV);
}