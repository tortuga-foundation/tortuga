#version 460
#extension GL_ARB_separate_shader_objects : enable

layout(set=0,binding=0) uniform sampler2D colorTexture;
layout(set=0,binding=1) uniform sampler2D normalTexture;
layout(set=0,binding=2) uniform sampler2D positionTexture;
layout(set=0,binding=3) uniform sampler2D detailTexture;

layout(location=0) in vec2 inUV;

layout(location=0) out vec4 outColor;

void main() {
    outColor = texture(colorTexture, inUV);
}