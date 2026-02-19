Shader"Custom/Outline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1, 1, 0, 1)
        _OutlineWidth ("Outline Width", Float) = 3.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        Pass
        {
Name"Outline"
            Cull
Front
            ZWrite
Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "UnityCG.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float3 smoothNormal : TEXCOORD1;
};

struct v2f
{
    float4 vertex : SV_POSITION;
};

float _OutlineWidth;
fixed4 _OutlineColor;

v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);

    float4 clipNormal = mul(UNITY_MATRIX_VP, mul(UNITY_MATRIX_M, float4(v.smoothNormal, 0)));
    float2 screenNormal = normalize(clipNormal.xy);

    // Correct for aspect ratio
    float aspect = _ScreenParams.x / _ScreenParams.y;
    screenNormal.x /= aspect;

    // Divide by w instead of multiply — this keeps pixel size fixed at any distance
    o.vertex.xy += screenNormal * (_OutlineWidth * 0.01);

    return o;
}

fixed4 frag(v2f i) : SV_Target
{
    return _OutlineColor;
}
            ENDCG
        }
    }

Fallback"Standard"
}