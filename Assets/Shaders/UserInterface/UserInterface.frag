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
    float rotation;
};
layout(set=2, binding=0) uniform sampler2D albedo;

layout(location = 0) in vec2 inUV;

layout(location = 0) out vec4 outColor;

bool BorderRadiusCheck(vec2 pos)
{
    if (borderRadiusTopLeft == 0 || borderRadiusTopRight == 0 && borderRadiusBottomLeft == 0 && borderRadiusBottomRight == 0)
        return true;

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

vec4 GetShadowColor(vec2 pos, vec2 spreadArea, vec2 blurArea)
{
    vec2 spreadPos = pos * (blurArea + vec2(1.));
    vec2 spreadPosAbs = vec2(abs(spreadPos.x), abs(spreadPos.y));
    if (spreadPosAbs.x < 1 && spreadPosAbs.y < 1)
    {
        if (BorderRadiusCheck(spreadPos))
            return shadowColor;
        else
            return vec4(0.);
    }
    return vec4(0.);
}

void main() {
    vec2 pos = (inUV * 2.) - 1.;
    vec4 color;

    vec2 shadowSpreadArea = vec2(
        shadowSpread / scale.x,
        shadowSpread / scale.y
    );
    vec2 shadowBlurArea = vec2(
        shadowBlur / scale.x,
        shadowBlur / scale.y
    );

    if (shadowType == 0)
    {
        if (BorderRadiusCheck(pos) == false)
            color = vec4(0.);
        else
            color = texture(albedo, inUV);
    }
    else if (shadowType == 1)
    {
        vec2 totalShadowArea = shadowSpreadArea + shadowBlurArea + vec2(1.);
        vec2 posWithShadow = vec2(pos.x * totalShadowArea.x, pos.y * totalShadowArea.y);
        if (posWithShadow.x > -1 && 
            posWithShadow.y > -1 && 
            posWithShadow.x < 1 && 
            posWithShadow.y < 1 && 
            BorderRadiusCheck(posWithShadow)
        )
            color = texture(albedo, (posWithShadow + 1.) / 2.);
        else
            color = GetShadowColor(pos, shadowSpreadArea, shadowBlurArea);
    }

    outColor = color;
}