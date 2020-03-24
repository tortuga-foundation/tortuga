#version 460

layout(set = 1, binding = 0) readonly uniform DATA
{
    vec2 position;
    vec2 scale;
    vec4 color;
    vec4 borderRadius;
} model;

layout(set = 2, binding = 0) uniform sampler2D fontAtlas;

layout(location=0) in vec2 inUV;

layout(location = 0) out vec4 outColor;

void main() {
    outColor = model.color;
    outColor.a = texture(fontAtlas, inUV).a;
}