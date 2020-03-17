#version 460

layout(set = 0, binding = 1) readonly uniform DATA
{
    vec4 color;
    vec4 rect;
    vec4 borderRadius;
} model;

layout(location=0) in vec2 inUV;
layout(location=1) in vec2 inPosition;

layout(location = 0) out vec4 outColor;

bool BorderRadiusTest()
{
    vec2 fragPosition = inPosition - vec2(model.rect.x, model.rect.y);

    //top left
    if (fragPosition.x < model.borderRadius.x && fragPosition.y < model.borderRadius.x)
    {
        vec2 center = vec2(model.borderRadius.x);
        if (length(center - fragPosition) > model.borderRadius.x) {
            return true;
        }
    }
    //top right
    else if (fragPosition.x + model.borderRadius.y > model.rect.z && fragPosition.y < model.borderRadius.y)
    {
        vec2 center = vec2(model.rect.z - model.borderRadius.y, model.borderRadius.y);
        if (length(center - fragPosition) > model.borderRadius.y) {
            return true;
        }
    }
    //bottom left
    else if (fragPosition.x < model.borderRadius.z && fragPosition.y + model.borderRadius.z > model.rect.w)
    {
        vec2 center = vec2(model.borderRadius.z, model.rect.w - model.borderRadius.z);
        if (length(center - fragPosition) > model.borderRadius.z) {
            return true;
        }
    }
    //bottom right
    else if (fragPosition.x + model.borderRadius.w > model.rect.z && fragPosition.y + model.borderRadius.w > model.rect.w)
    {
        vec2 center = vec2(model.rect.z - model.borderRadius.w, model.rect.w - model.borderRadius.w);
        if (length(center - fragPosition) > model.borderRadius.w) {
            return true;
        }
    }

    return false;
}

void main() {
    if (BorderRadiusTest())
        discard;

    outColor = vec4(model.color);
}