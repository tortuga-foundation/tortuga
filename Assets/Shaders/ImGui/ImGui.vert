#version 460

layout(set = 0, binding = 0) uniform ProjectionMatrixBuffer
{
    mat4 projection_matrix;
};

layout(location=0) in vec2 in_position;
layout(location=1) in vec2 in_texCoord;
layout(location=2) in vec4 in_color;

layout(location=0) out vec4 color;
layout(location=1) out vec2 texCoord;

vec3 SrgbToLinear(vec3 srgb)
{
    return srgb * (srgb * (srgb * 0.305306011 + 0.682171111) + 0.012522878);
}

void main()
{
    gl_Position = projection_matrix * vec4(in_position, 0, 1);
    color = vec4(SrgbToLinear(in_color.rgb), in_color.a);
	texCoord = in_texCoord;
}