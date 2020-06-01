#version 460

vec2 positions[6] = vec2[](
    vec2(-1., -1.),
    vec2( 1., -1.),
    vec2( 1.,  1.),
    vec2(-1., -1.),
    vec2( 1.,  1.),
    vec2(-1.,  1.)
);

vec2 uvs[6] = vec2[](
    vec2(0, 0),
    vec2(1, 0),
    vec2(1, 1),
    vec2(0, 0),
    vec2(1, 1),
    vec2(0, 1)
);

layout(location=0) out vec2 outUV;

void main() {
    gl_Position = vec4(positions[gl_VertexIndex], 0.0, 1.0);
    outUV = uvs[gl_VertexIndex];
}