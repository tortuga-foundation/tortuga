#version 460

layout(set=0,binding=0) readonly uniform CAMERA_MVP
{
    mat4 view;
    mat4 projection;
    int cameraX;
    int cameraY;
    int cameraWidth;
    int cameraHeight;
};
layout(set=1, binding=0) readonly uniform UI_DATA
{
    vec4 shadowColor;
    vec2 position;
    vec2 scale;
    vec2 shadowOffset;
    float borderRadiusTopLeft;
    float borderRadiusTopRight;
    float borderRadiusBottomLeft;
    float borderRadiusBottomRight;
    int indexZ;
    int shadowType;
    float shadowBlur;
    float shadowSpread;
};

layout(location = 0) out vec2 outUV;

void main() {
    
    vec2 pos = vec2(position.x / cameraWidth, position.y / cameraHeight);
    vec2 sca = vec2(scale.x / cameraWidth,  scale.y / cameraHeight);

    //setup vertex position and uvs
    vec2 vertexPositions[6] = vec2[](
        pos,
        sca,
        vec2(sca.x, pos.y),
        vec2(sca.x,  sca.y),
        pos,
        vec2(pos.x,  sca.y)
    );
    vec2 uv[] = vec2[](
        vec2(0, 0),
        vec2(1, 1),
        vec2(1, 0),
        vec2(1, 1),
        vec2(0, 0),
        vec2(0, 1)
    );

    //output uv texture coords
    outUV = uv[gl_VertexIndex];
    //convert vertices position and output
    gl_Position = vec4((vertexPositions[gl_VertexIndex]  - .5) * 2., 0., 1.);
}