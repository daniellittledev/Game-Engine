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

};

float4 ps( in v2p IN) : COLOR0//,  out p2f OUT)
{	
    //OUT.color = 
    return tex2D(TextureSampler, IN.tex0); 
};

/////////////////////////////////////////////////
//                 Techniques                  //
/////////////////////////////////////////////////

technique ShaderInstancing
{
    pass P0
    {      
        VertexShader = compile vs_2_0 vs();           
        PixelShader  = compile ps_2_0 ps();          
    }
}