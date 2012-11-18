//-----------------------------------------------------------------------------
// Copyright (c) 2008 dhpoware. All Rights Reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Globals.
//-----------------------------------------------------------------------------

float4x4 world;
float4x4 worldInvTrans;
float4x4 worldViewProjection;

float3 sunlightDir;
float4 sunlightColor;
float4 terrainAmbient;
float4 terrainDiffuse;
float terrainTilingFactor;

float3 groundCursorPosition;
texture groundCursorTex;
int groundCursorSize;
bool bShowCursor;

float2 terrainRegion1;
float2 terrainRegion2;
float2 terrainRegion3;
float2 terrainRegion4;
float2 terrainRegion5;
float2 terrainRegion6;

//-----------------------------------------------------------------------------
// Textures.
//-----------------------------------------------------------------------------

texture region1ColorMapTexture;
sampler2D region1ColorMap = sampler_state
{
	Texture = <region1ColorMapTexture>;
	MagFilter = Linear;
	MinFilter = Anisotropic;
	MipFilter = Linear;
	MaxAnisotropy = 16;
	AddressU = WRAP;
	AddressV = WRAP;
};

texture region2ColorMapTexture;
sampler2D region2ColorMap = sampler_state
{
	Texture = <region2ColorMapTexture>;
	MagFilter = Linear;
	MinFilter = Anisotropic;
	MipFilter = Linear;
	MaxAnisotropy = 16;
	AddressU = WRAP;
	AddressV = WRAP;
};

texture region3ColorMapTexture;
sampler2D region3ColorMap = sampler_state
{
	Texture = <region3ColorMapTexture>;
	MagFilter = Linear;
	MinFilter = Anisotropic;
	MipFilter = Linear;
	MaxAnisotropy = 16;
	AddressU = WRAP;
	AddressV = WRAP;
};

texture region4ColorMapTexture;
sampler2D region4ColorMap = sampler_state
{
	Texture = <region4ColorMapTexture>;
	MagFilter = Linear;
	MinFilter = Anisotropic;
	MipFilter = Linear;
	MaxAnisotropy = 16;
	AddressU = WRAP;
	AddressV = WRAP;
};

texture region5ColorMapTexture;
sampler2D region5ColorMap = sampler_state
{
	Texture = <region5ColorMapTexture>;
	MagFilter = Linear;
	MinFilter = Anisotropic;
	MipFilter = Linear;
	MaxAnisotropy = 16;
	AddressU = WRAP;
	AddressV = WRAP;
};

texture region6ColorMapTexture;
sampler2D region6ColorMap = sampler_state
{
	Texture = <region6ColorMapTexture>;
	MagFilter = Linear;
	MinFilter = Anisotropic;
	MipFilter = Linear;
	MaxAnisotropy = 16;
	AddressU = WRAP;
	AddressV = WRAP;
};

sampler2D CursorSampler = sampler_state
{
	Texture = <groundCursorTex>;
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//-----------------------------------------------------------------------------
// Vertex Shaders.
//-----------------------------------------------------------------------------

void VS_Main(in  float3 inPos       : POSITION,
			 in  float3 inNormal    : NORMAL,
			 in  float2 inTexCoord  : TEXCOORD,
			 
			 out float4 outPos      : POSITION,
			 out float2 outTexCoord : TEXCOORD0,
			 out float4 outNormal   : TEXCOORD1)
{
	outPos = mul(float4(inPos, 1.0f), worldViewProjection);
	outTexCoord = inTexCoord * terrainTilingFactor;
	outNormal.xyz = mul(inNormal, (float3x3)worldInvTrans);
	
	// Store the terrain height at this vertex position.
	// The terrain height will be used to generate the terrain texture.
	outNormal.w   = inPos.y;
}

struct PS_INPUT
{
	float3 Position		: POSITION0;
	float2 Texcoord		: TEXCOORD0;
	float4 Normal		: TEXCOORD1;
	float3 Position3D	: TEXCOORD2;
};

//-----------------------------------------------------------------------------
// Pixel Shaders.
//-----------------------------------------------------------------------------
void PS_Main(in PS_INPUT Input,
		   //in  float2 texCoord : TEXCOORD0,
		   //in  float4 normal   : TEXCOORD1,
			 
			 out float4 color    : COLOR)
{	
	float4 terrainColor = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float height = Input.Normal.w;
	float regionMin;
	float regionMax;
	float regionRange;
	float regionWeight;
		
	// Terrain region 1.
	
	regionMin = terrainRegion1.x;
	regionMax = terrainRegion1.y;
	regionRange = regionMax - regionMin;
	regionWeight = (regionRange - abs(height - regionMax)) / regionRange;
	regionWeight = saturate(regionWeight);
	terrainColor += regionWeight * tex2D(region1ColorMap, Input.Texcoord);
			
	// Terrain region 2.
	
	regionMin = terrainRegion2.x;
	regionMax = terrainRegion2.y;
	regionRange = regionMax - regionMin;
	regionWeight = (regionRange - abs(height - regionMax)) / regionRange;
	regionWeight = saturate(regionWeight);
	terrainColor += regionWeight * tex2D(region2ColorMap, Input.Texcoord);
			
	// Terrain region 3.
	
	regionMin = terrainRegion3.x;
	regionMax = terrainRegion3.y;
	regionRange = regionMax - regionMin;
	regionWeight = (regionRange - abs(height - regionMax)) / regionRange;
	regionWeight = saturate(regionWeight);
	terrainColor += regionWeight * tex2D(region3ColorMap, Input.Texcoord);
	
	// Terrain region 4.
	
	regionMin = terrainRegion4.x;
	regionMax = terrainRegion4.y;
	regionRange = regionMax - regionMin;
	regionWeight = (regionRange - abs(height - regionMax)) / regionRange;
	regionWeight = saturate(regionWeight);
	terrainColor += regionWeight * tex2D(region4ColorMap, Input.Texcoord);
	
	//// Terrain region 5.
	
	//regionMin = terrainRegion5.x;
	//regionMax = terrainRegion5.y;
	//regionRange = regionMax - regionMin;
	//regionWeight = (regionRange - abs(height - regionMax)) / regionRange;
	//regionWeight = saturate(regionWeight);
	//terrainColor += regionWeight * tex2D(region5ColorMap, Input.Texcoord);
	
	//// Terrain region 6.
	
	//regionMin = terrainRegion6.x;
	//regionMax = terrainRegion6.y;
	//regionRange = regionMax - regionMin;
	//regionWeight = (regionRange - abs(height - regionMax)) / regionRange;
	//regionWeight = saturate(regionWeight);
	//terrainColor += regionWeight * tex2D(region6ColorMap, Input.Texcoord);
	
	// Light and texture the terrain.

	float3 n = normalize(Input.Normal.xyz);
	float3 l = normalize(-sunlightDir);
	
	color = terrainColor * (sunlightColor * terrainAmbient +
							sunlightColor * terrainDiffuse * saturate(dot(n, l)));
	if(bShowCursor)
	{
		float CursorScale = 40.0f;
		float4 CursorColor = tex2D(CursorSampler, (Input.Texcoord * (CursorScale / groundCursorSize)) - (groundCursorPosition.xz * (CursorScale / groundCursorSize)) + 0.5f);	
		color.rgb += CursorColor;    
	}	
}

Technique Texture
{
	Pass P0
	{
		FillMode = Solid;
		VertexShader = compile vs_2_0 VS_Main();
		PixelShader = compile ps_2_0 PS_Main();
	}
}

Technique WireframeTechnique
{
	Pass P0
	{
		FillMode = Wireframe;
		VertexShader = compile vs_2_0 VS_Main();
		PixelShader = compile ps_2_0 PS_Main();
	}
}
Technique TextureAndWireframe
{
	Pass P0
	{
		FillMode = Solid;
		VertexShader = compile vs_2_0 VS_Main();
		PixelShader = compile ps_2_0 PS_Main();
	}
	Pass P1
	{
		FillMode = Wireframe;
		VertexShader = compile vs_2_0 VS_Main();
		PixelShader = Null;
	}
}