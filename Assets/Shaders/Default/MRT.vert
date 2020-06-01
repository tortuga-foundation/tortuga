#version 460

layout(set=0, binding=0) readonly uniform PROJECTION
{
    mat4 projection;
};
layout(set=1, binding=0) readonly uniform VIEW
{
    mat4 view;
};
layout(set=2, binding=0) readonly uniform MODEL
{
    mat4 model;
};

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec2 inUV;
layout(location = 2) in vec3 inNormal;

layout(location = 0) out vec3 outNormal;
layout(location = 1) out vec2 outUV;
layout(location = 2) out vec3 outCameraPosition;
layout(location = 3) out vec3 outWorldPosition;

vec2 positions[3] = vec2[](
    vec2(0.0, -0.5),
    vec2(0.5, 0.5),
    vec2(-0.5, 0.5)
);

void main() {
    vec4 worldPosition = model * vec4(inPosition, 1.0);
    gl_Position = projection * view * worldPosition;
    outWorldPosition = worldPosition.xyz;
    outUV = inUV;
    outCameraPosition = inverse(view)[3].xyz;
    outNormal = normalize(model * vec4(inNormal, 0.)).xyz;
}