#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(binding = 1) uniform sampler2D texSampler;

layout(location = 0) in vec3 fragColor;
layout(location = 1) in vec2 fragTexCoord;

layout(location = 0) out vec4 outColor;

void main() {
    vex2 texCoords = vec2(1.0 - fragTexCoord.x, fragTexCoord.y);
    outColor = texture(texSampler, texCoords);
}