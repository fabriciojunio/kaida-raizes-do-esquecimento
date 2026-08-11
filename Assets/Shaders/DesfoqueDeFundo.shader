// Desfoque para o fundo do menu principal.
//
// Faz uma média de nove amostras ao redor de cada pixel — um box blur simples.
// É barato o bastante para rodar sobre a imagem da cena inteira sem precisar
// de pós-processamento nem de pacote extra instalado.
Shader "Kaida/DesfoqueDeFundo"
{
    Properties
    {
        _MainTex ("Textura", 2D) = "white" {}
        _Raio ("Raio do desfoque", Range(0, 12)) = 4
        _Escurecer ("Escurecer", Range(0, 1)) = 0.35
        _Cor ("Tingir", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct entrada
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct saida
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Raio;
            float _Escurecer;
            fixed4 _Cor;

            saida vert (entrada v)
            {
                saida o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (saida i) : SV_Target
            {
                float2 passo = _MainTex_TexelSize.xy * _Raio;

                fixed4 soma = fixed4(0, 0, 0, 0);
                soma += tex2D(_MainTex, i.uv + float2(-passo.x, -passo.y));
                soma += tex2D(_MainTex, i.uv + float2( 0.0,     -passo.y));
                soma += tex2D(_MainTex, i.uv + float2( passo.x, -passo.y));
                soma += tex2D(_MainTex, i.uv + float2(-passo.x,  0.0));
                soma += tex2D(_MainTex, i.uv);
                soma += tex2D(_MainTex, i.uv + float2( passo.x,  0.0));
                soma += tex2D(_MainTex, i.uv + float2(-passo.x,  passo.y));
                soma += tex2D(_MainTex, i.uv + float2( 0.0,      passo.y));
                soma += tex2D(_MainTex, i.uv + float2( passo.x,  passo.y));

                fixed4 cor = soma / 9.0;
                cor.rgb *= (1.0 - _Escurecer);
                cor *= _Cor;
                cor.a = 1.0;
                return cor;
            }
            ENDCG
        }
    }

    Fallback "Unlit/Texture"
}
