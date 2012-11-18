float4x4 wvp : WorldViewProjection;
float4x4 world : World;

float tiles = 3.1;

float3 LightPosition : POSITION
<
	string Object = "DirectionalLight";
    string Space = "World";
> = {1.0f, -1.0f, 1.0f};
float3 EyePosition : CAMERAPOSITION;

struct VS_OUTPUT
{
	float4 Pos : POSITION;
	float2 Tex : TEXCOORD0;
	float4 Light : TEXCOORD1;
	float3 lView : TEXCOORD2;
};

VS_OUTPUT VS(float4 Pos : POSITION,float2 Tex : TEXCOORD,float3 Normal : NORMAL,float3 Tangent : TANGENT)
{
	VS_OUTPUT Out = (VS_OUTPUT)0;
		
	Out.Pos = mul(Pos,wvp);
	
	float3x3 worldToTangentSpace;
	worldToTangentSpace[1] = mul(Tangent,world);
	worldToTangentSpace[0] = mul(cross(Tangent,Normal),world);
	worldToTangentSpace[2] = mul(Normal,world);
	
	Out.Tex = Tex * tiles; // Add this so the rock texture would look better.
	
	float4 PosWorld = mul(Pos,world);
	
	Out.Light.xyz = mul(worldToTangentSpace,LightPosition);	
	Out.Light.w = 1;
	Out.lView = mul(worldToTangentSpace,-EyePosition - PosWorld);	
	
	return Out;
}

texture ColorMap : RCMaterialParameter
<
	string Object = "ColorMap";
	string UIName = "ColorMap";
>;
sampler ColorMapSampler = sampler_state
{
	Texture = <ColorMap>;
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};

texture BumpMap : RCMaterialParameter
<
	string Object = "BumpMap";
	string UIName = "BumpMap";
>;
sampler BumpMapSampler = sampler_state
{
	Texture = <BumpMap>;
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};
struct PixelToFrame
{
    float4 Color : COLOR0;
};

PixelToFrame PS(float2 Tex : TEXCOORD0,float4 Light : TEXCOORD1,float3 lView : TEXCOORD2) : COLOR
{
	PixelToFrame Out = (PixelToFrame)0;
	
	float4 Color = tex2D(ColorMapSampler,Tex);
	float3 Normal = (2 * (tex2D(BumpMapSampler,Tex))) - 1.0;
	Normal = (2 * tex2D(BumpMapSampler,Tex)) -1;
	
	float4 LightDir = normalize(Light);
	float3 ViewDir = normalize(lView);
	
	float Diffuse = saturate(dot(LightDir,Normal));	
	
	Out.Color = 0.2 * Color + Color * Diffuse;
		
	return Out;
}

technique BumpMapShader
{
    pass P0
    {    
        VertexShader = compile vs_2_0 VS();
        PixelShader  = compile ps_2_0 PS();
    }
}