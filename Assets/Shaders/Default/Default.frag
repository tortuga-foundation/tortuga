#version 460
#extension GL_ARB_separate_shader_objects : enable

layout(set=3, binding=0) uniform sampler2D colorTexture;
layout(set=3, binding=1) uniform sampler2D normalTexture;
layout(set=3, binding=2) uniform sampler2D detailTexture;

layout(location = 0) in vec3 inNormal;
layout(location = 1) in vec2 inUV;
layout(location = 2) in vec3 inCameraPosition;
layout(location = 3) in vec3 inWorldPosition;

layout(location = 0) out vec4 outColor;

void main() {
    outColor = vec4(1.);
}