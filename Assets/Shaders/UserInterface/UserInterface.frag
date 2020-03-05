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
    vec4 shadowColor;
    vec2 position;
    vec2 scale;
    vec2 shadowOffset;
    float borderRadiusTopLeft;
    float borderRadiusTopRight;
    float borderRadiusBottomLeft;
    float borderRadiusBottomRight;
    int indexZ;
    int shadowType;
    float shadowBlur;
    float shadowSpread;
};
layout(set=2, binding=0) uniform sampler2D albedo;

layout(location = 0) in vec2 inUV;

layout(location = 0) out vec4 outColor;

bool BorderRadiusCheck()
{
    if (borderRadiusTopLeft == 0 || borderRadiusTopRight == 0 && borderRadiusBottomLeft == 0 && borderRadiusBottomRight == 0)
        return true;

    vec2 pos = (inUV * 2.) - 1.;

    //bottom right
    if (borderRadiusBottomRight > 0)
    {
        vec2 center = vec2(borderRadiusBottomRight / scale.x, borderRadiusBottomRight / scale.y);
        if (pos.x > 1. - center.x && pos.y > 1. - center.y && length(pos - vec2(1.) + vec2(center.x)) > center.x)
            return false;
    }

    //top right
    if (borderRadiusTopRight > 0)
    {
        vec2 center = vec2(borderRadiusTopRight / scale.x, borderRadiusTopRight / scale.y);
        if (pos.x > 1. - center.x && pos.y < center.y - 1. && length(pos - vec2(1., -1.) + vec2(center.x, -center.x)) > center.x)
            return false;
    }

    //bottom left
    if (borderRadiusBottomLeft > 0)
    {
        vec2 center = vec2(borderRadiusBottomLeft / scale.x, borderRadiusBottomLeft / scale.y);
        if (pos.x < center.x - 1. && pos.y > 1. - center.y && length(pos - vec2(-1., 1.) + vec2(-center.x, center.x)) > center.x)
            return false;
    }

    //top left
    if (borderRadiusTopLeft > 0)
    {
        vec2 center = vec2(borderRadiusTopLeft / scale.x, borderRadiusTopLeft / scale.y);
        if (pos.x < center.x - 1. && pos.y < center.y - 1. && length(pos - vec2(-1., -1.) + vec2(-center.x, -center.x)) > center.x)
            return false;
    }

    return true;
}

void main() {   
    if (BorderRadiusCheck() == false)
    {
        outColor = vec4(0.);
        return;
    }

    outColor = vec4(texture(albedo, inUV).rgb, 1.);
}