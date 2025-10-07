Shader "Custom/FlagWave"
{
    Properties
    {
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Speed("Speed", Range(0, 100.0)) = 1
        _Frequency("Frequency", Range(0, 1.3)) = 1
        _Amplitude("Amplitude", Range(0, 5.0)) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }
        Cull off

        Pass
        {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _Speed;
            float _Frequency;
            float _Amplitude;

            v2f vert(appdata_base v)
            {
                float amplitude = _Amplitude * (sin(_Time.y * 2) * 0.2 + 1);
                float2 offset = float2(sin(_Time.y * _Speed + v.vertex.x * _Frequency) * amplitude, 0);
                if (v.vertex.x < 6) v.vertex.x = 1.5;   // snap the edge to the pole
                else {
                    v.vertex.x += offset;
                    v.vertex.z += offset*1.5;
                }
                
                v2f o; 
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG

        }
    }
    FallBack "Diffuse"
}