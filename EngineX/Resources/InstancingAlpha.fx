// Instancing Shader
//---------------------------------------------------------------------------------------

float4x4 ViewProjection;	// View * Projection matrix
texture Texture;

sampler TextureSampler = sampler_state
{
    Texture = <Texture>;
    
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;    
};

float4x4 InstanceData[10] : INSTANCEARRAYDATA : register(c16);
float InstanceAlpha[10] : INSTANCEARRAYDATA;// : register(c15);

/////////////////////////////////////////////////
//                  Structs                    //
/////////////////////////////////////////////////

//application to vertex structure
struct a2v
{ 
    float4 position      : POSITION0;
    float3 normal	     : NORMAL;
    float2 tex0          : TEXCOORD0;   
    
    float instance       : TEXCOORD1;   
};
 
//vertex to pixel shader structure
struct v2p
{   	

	float4 position      : POSITION0;
    float2 tex0          : TEXCOORD0;
    float alpha          : TEXCOORD1;
};

//pixel shader to screen
struct p2f
{
    float4 color    : COLOR0;
};

/////////////////////////////////////////////////
//                  Methods                    //
/////////////////////////////////////////////////

void vs( in a2v IN, out v2p OUT )
{       
    //TODO: Lighting

    OUT.position = mul(IN.position, mul(InstanceData[IN.instance], ViewProjection) ); 
	OUT.tex0 = IN.tex0;
	OUT.alpha = InstanceAlpha[IN.instance];

};

void ps( in v2p IN,  out p2f OUT) //
{	
    OUT.color = tex2D(TextureSampler, IN.tex0);
    OUT.color.a = IN.alpha;
};

/////////////////////////////////////////////////
//                 Techniques                  //
/////////////////////////////////////////////////

technique ShaderInstancing
{
    pass P0
    {      
        AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = InvSrcAlpha;
    
        VertexShader = compile vs_2_0 vs();           
        PixelShader  = compile ps_2_0 ps();          
    }
}