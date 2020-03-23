#version 460

layout(set = 1, binding = 0) readonly uniform DATA
{
    vec2 position;
    vec2 scale;
    vec4 color;
} model;

layout(location=0) in vec2 inUV;
layout(location=1) in vec2 inPosition;

layout(location = 0) out vec4 outColor;

bool BorderRadiusTest(vec4 borderRadius, vec2 position, vec2 scale)
{
    vec2 fragPosition = inPosition - vec2(position.x, position.y);

    //top left
    if (fragPosition.x < borderRadius.x && fragPosition.y < borderRadius.x)
    {
        vec2 center = vec2(borderRadius.x);
        if (length(center - fragPosition) > borderRadius.x) {
            return true;
        }
    }
    //top right
    else if (fragPosition.x + borderRadius.y > scale.x && fragPosition.y < borderRadius.y)
    {
        vec2 center = vec2(scale.x - borderRadius.y, borderRadius.y);
        if (length(center - fragPosition) > borderRadius.y) {
            return true;
        }
    }
    //bottom left
    else if (fragPosition.x < borderRadius.z && fragPosition.y + borderRadius.z > scale.y)
    {
        vec2 center = vec2(borderRadius.z, scale.y - borderRadius.z);
        if (length(center - fragPosition) > borderRadius.z) {
            return true;
        }
    }
    //bottom right
    else if (fragPosition.x + borderRadius.w > scale.x && fragPosition.y + borderRadius.w > scale.y)
    {
        vec2 center = vec2(scale.x - borderRadius.w, scale.y - borderRadius.w);
        if (length(center - fragPosition) > borderRadius.w) {
            return true;
        }
    }

    return false;
}

void main() {
    outColor = vec4(1.);
}