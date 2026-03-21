float4x4 WorldViewProj;
float3 light;
float ambient;

Texture low;
Texture med;
Texture hig;
Texture TextureDetail;

sampler sampLow = sampler_state { texture = <low>; 
    minfilter = LINEAR; mipfilter = LINEAR; magfilter = LINEAR;};
    
sampler sampMed = sampler_state { texture = <med>; 
    minfilter = LINEAR; mipfilter = LINEAR; magfilter = LINEAR;};
    
sampler sampHig = sampler_state { texture = <hig>; 
    minfilter = LINEAR; mipfilter = LINEAR; magfilter = LINEAR;};
    
sampler sampDet = sampler_state { texture = <TextureDetail>; 
    minfilter = LINEAR; mipfilter = LINEAR; magfilter = LINEAR;};
    
void Transform(
    in float4 inPos			: POSITION0,
    in float2 inCoordA		: TEXCOORD0,
    in float2 inCoordB		: TEXCOORD1,    
    in float2 blendA		: TEXCOORD2,
    in float2 blendB		: TEXCOORD3,
    in float3 normal		: NORMAL,
        
    out float4 outPos		: POSITION0,
    
    out float2 outCoordA	: TEXCOORD0,
    out float2 outCoordB	: TEXCOORD1,
    out float2 BlendA		: TEXCOORD2,
    out float2 BlendB		: TEXCOORD3,
    out float3 Normal		: TEXCOORD4,
    out float3 lightDir		: TEXCOORD5  )
{
    outPos =        mul(inPos, WorldViewProj);				    //transform position
    outCoordA =     inCoordA;
    outCoordB =     inCoordB;			
    BlendA =        blendA;
    BlendB =        blendB;
    Normal =        normalize( mul(normal, WorldViewProj) );	// transform normal
    lightDir =      inPos.xyz - light;					        // calculate the direction of the light
}

float4 TextureColor(
 in float2 texCoordA		: TEXCOORD0,
 in float2 texCoordB		: TEXCOORD1,
 in float2 BlendA			: TEXCOORD2,
 in float2 BlendB			: TEXCOORD3,
 in float3 normal			: TEXCOORD4,
 in float3 lightDir			: TEXCOORD5) : COLOR0
{
    // Textures
    float4 texCol1 = tex2D(sampLow, texCoordA) * BlendA[0];
    float4 texCol2 = tex2D(sampMed, texCoordA) * BlendA[1];
    float4 texCol3 = tex2D(sampHig, texCoordA) * BlendB[0];
    
    // Detail
    float4 texCol4 = tex2D(sampDet, texCoordB) * BlendB[1];
    
    // Textures
    return (texCol1 + texCol2 + texCol3 + texCol4) 
    
    // Lighing
    * (saturate(dot(normalize(normal), normalize(light)))* (1-ambient) + ambient);
}

technique TransformTexture
{
    pass P0
    {
		//CullMode = CCW;
		CullMode = None;
		
		//FillMode = Wireframe;
		FillMode = Solid;
    
        VertexShader = compile vs_2_0 Transform();
        
        PixelShader  = compile ps_2_0 TextureColor();
    }
    
        pass P1
    {
		//CullMode = CCW;
		FillMode = Wireframe;
    
        VertexShader = compile vs_2_0 Transform();
        
        PixelShader  = compile ps_2_0 TextureColor();
    }
}

