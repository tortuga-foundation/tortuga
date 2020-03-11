#version 460

layout(set = 0, binding = 1) uniform sampler2D fontTexture;

layout(location = 0) in vec4 color;
layout(location = 1) in vec2 texCoord;

layout(location = 0) out vec4 outputColor;

void main() {
    outputColor = color * texture(fontTexture, texCoord);
}