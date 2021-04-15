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
layout(location = 5) in vec3 inModelPos;
layout(location = 6) in vec3 inModelRot;
layout(location = 7) in vec3 inModelSca;

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

mat4 constructMatrix(vec3 pos, vec3 rot, vec3 sca) {
    mat4 mat;

    // rotation
    float s, c;
    mat3 mx, my, mz;
    // rotate around x
    s = sin(rot.x);
    c = cos(rot.x);
    mx[0] = vec3(c, s, 0.);
    mx[1] = vec3(-s, c, 0.);
    mx[2] = vec3(0., 0., 1.);
    // rotate around y
    s = sin(rot.y);
    c = cos(rot.y);
    my[0] = vec3(c, 0., s);
    my[1] = vec3(0., 1., 0.);
    my[2] = vec3(-s, 0., c);
    // rotate around z
    s = sin(rot.z);
    c = cos(rot.z);
    my[0] = vec3(1., 0., 0.);
    my[1] = vec3(0., c, s);
    my[2] = vec3(0., -s, c);
    mat3 rotMat = mz * my * mx;

    mat[0] = vec4(rotMat[0], 0.);
    mat[1] = vec4(rotMat[1], 0.);
    mat[2] = vec4(rotMat[2], 0.);
    mat[3] = vec4(0., 0., 0., 1.);

    // scale
    mat[0][0] *= sca.x;
    mat[1][1] *= sca.y;
    mat[2][2] *= sca.z;

    // position
    mat[0][3] = pos.x;
    mat[1][3] = pos.y;
    mat[2][3] = pos.z;
    
    return mat;
}

void main() {
    mat4 model = constructMatrix(
        inModelPos,
        inModelRot,
        inModelSca
    );

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