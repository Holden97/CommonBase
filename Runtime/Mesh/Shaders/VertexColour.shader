Shader "Custom/VertexColour"
{
	SubShader
	{
		CGPROGRAM
		#pragma surface surf Lambert

		struct Input
		{
			float4 vertColour :COLOR;
		};

		void surf(Input IN,inout SurfaceOutput o)
		{
			o.Albedo=IN.vertColour;
		}

		ENDCG
	}

	FallBack "Diffuse"
}
