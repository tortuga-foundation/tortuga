#version 460

layout(set=0, binding=0) readonly uniform PROJECTION
{
    mat4 projection;
};
layout(set=1, binding=0) readonly uniform VIEW
{
    mat4 view;
};
layout(set=3, binding=0) readonly uniform MATERIAL
{
    int shadingModel;
};

// vertex attributes
layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec2 inUV;
layout(location = 2) in vec3 inNormal;
layout(location = 3) in vec3 inTangent;
layout(location = 4) in vec3 inBiTangent;

// instanced attributes
layout(location = 5) in vec4 inModel1;
layout(location = 6) in vec4 inModel2;
layout(location = 7) in vec4 inModel3;
layout(location = 8) in vec4 inModel4;

// output attributes
layout(location = 0) out vec3 outNormal;
layout(location = 1) out vec2 outUV;
layout(location = 2) out vec3 outCameraPosition;
layout(location = 3) out vec3 outWorldPosition;
layout(location = 4) out vec3 outTangent;
layout(location = 5) out vec3 outBiTangent;

vec2 positions[3] = vec2[](
    vec2(0.0, -0.5),
    vec2(0.5, 0.5),
    vec2(-0.5, 0.5)
);

mat4 getModel() {
    mat4 model;
    model[0] = inModel1;
    model[1] = inModel2;
    model[2] = inModel3;
    model[3] = inModel4;
    return model;
}

void main() {
    mat4 model = getModel();

    vec4 worldPosition = model * vec4(inPosition, 1.0);
    gl_Position = projection * view * worldPosition;
    outWorldPosition = worldPosition.xyz;
    outUV = inUV;
    outCameraPosition = inverse(view)[3].xyz;
    
    //normal
    mat3 mNormal = transpose(inverse(mat3(model)));
    if (shadingModel == 0)
        outNormal = mNormal * normalize(inNormal);
    else if (shadingModel == 1)
        outNormal = mNormal * normalize(inPosition);
    outTangent = mNormal * normalize(inTangent);
    outBiTangent = mNormal * normalize(inBiTangent);
}