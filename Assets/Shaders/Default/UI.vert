#version 460

layout(set=0,binding=0) readonly uniform PROJECTION {
    mat4 projection_matrix;
};

layout(location=0) in vec2 inPosition;
layout(location=1) in vec2 inUV;
layout(location=2) in vec4 inColor;

layout(location=0) out vec4 outColor;
layout(location=1) out vec2 outUV;

vec3 SrgbToLinear(vec3 srgb)
{
    return srgb * (srgb * (srgb * 0.305306011 + 0.682171111) + 0.012522878);
}

void main()
{
    gl_Position = projection_matrix * vec4(inPosition, 0, 1);
	outUV = inUV;
    outColor = vec4(SrgbToLinear(inColor.rgb), inColor.a);
}
