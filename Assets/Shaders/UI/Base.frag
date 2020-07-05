#version 460

layout(set = 1, binding = 0) readonly uniform DATA
{
    vec2 position;
    vec2 scale;
    vec4 borderRadius;
} model;
layout(set = 2, binding = 0) uniform sampler2D baseColor;

layout(location=0) in vec2 inUV;
layout(location=1) in vec2 inPosition;

layout(location = 0) out vec4 outColor;

float BorderRadiusTest(vec4 borderRadius, vec2 position, vec2 scale)
{
    vec2 fragPosition = inPosition - vec2(position.x, position.y);

    //top left
    if (fragPosition.x < borderRadius.x && fragPosition.y < borderRadius.x)
    {
        vec2 center = vec2(borderRadius.x);
        float amount = length(center - fragPosition) - borderRadius.x;
        if (amount > 0) {
            return 1. - amount;
        }
    }
    //top right
    else if (fragPosition.x + borderRadius.y > scale.x && fragPosition.y < borderRadius.y)
    {
        vec2 center = vec2(scale.x - borderRadius.y, borderRadius.y);
        float amount = length(center - fragPosition) - borderRadius.y;
        if (amount > 0) {
            return 1. - amount;
        }
    }
    //bottom left
    else if (fragPosition.x < borderRadius.z && fragPosition.y + borderRadius.z > scale.y)
    {
        vec2 center = vec2(borderRadius.z, scale.y - borderRadius.z);
        float amount = length(center - fragPosition) - borderRadius.z;
        if (amount > 0) {
            return 1. - amount;
        }
    }
    //bottom right
    else if (fragPosition.x + borderRadius.w > scale.x && fragPosition.y + borderRadius.w > scale.y)
    {
        vec2 center = vec2(scale.x - borderRadius.w, scale.y - borderRadius.w);
        float amount = length(center - fragPosition) - borderRadius.w;
        if (amount > 0) {
            return 1. - amount;
        }
    }

    return 1.;
}

void main() {
    outColor = texture(baseColor, inUV);
    if (model.borderRadius != vec4(0.))
    {
        float borderTestAlpha = BorderRadiusTest(model.borderRadius, model.position, model.scale);
        if (borderTestAlpha < 0.01)
            discard;
        
        outColor.a *= borderTestAlpha;
    }
}