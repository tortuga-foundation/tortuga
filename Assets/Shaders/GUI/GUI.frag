#version 460

layout(set = 0, binding = 1) uniform sampler2D fontTexture;
layout(set = 1, binding = 1) uniform sampler2D backgroundTexture;

layout(location = 0) in vec2 inUV;

layout(location = 0) out vec4 outColor;

void main() {
    vec4 bg = texture(backgroundTexture, inUV);

    outColor = vec4(bg.rgb, 1.);
}