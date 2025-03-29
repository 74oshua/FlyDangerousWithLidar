Shader "Lidar" {
    SubShader {
        Tags { "RenderType"="Transparent" }
        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata_base v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            half4 frag(v2f i) : SV_Target {
                return half4(1, 1, 1, 1) / (length(i.pos) / 1000);
                // UNITY_OUTPUT_DEPTH(i.depth);
            }
            ENDCG
        }
    }
}