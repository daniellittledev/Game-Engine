using System;
using System.Collections.Generic;
using System.Text;

using SharpDX;
using SharpDX.Direct3D9;

using System.Windows.Forms;
using System.Drawing;


namespace EngineX.Effects
{
    public class Particles
    {

        public abstract class emitter
        {

            protected Vector3 sprayDirection;
            public Vector3 SprayDirection
            {
                get { return sprayDirection; }
                set { sprayDirection = value;  }
            }

            protected Vector3 sprayFocus;
            public Vector3 SprayFocus
            {
                get { return sprayFocus; }
                set { sprayFocus = value; }
            }

            protected float sprayRate;
            public float SprayRate
            {
                get { return sprayRate; }
                set 
                { 
                    sprayRate = value;
                    createElapsedTime = value;
                }
            }

            protected float createElapsedTime;
            public float CreateElapsedTime
            {

                get { return createElapsedTime; }
                set { 
                    createElapsedTime = value;
                }
            
            }

            protected float absoluteElapsedTime;
            public float AbsoluteElapsedTime
            {

                get { return absoluteElapsedTime; }
                set
                {
                    absoluteElapsedTime = value;

                    if (baseEmitter != null)
                    {
                        baseEmitter.AbsoluteElapsedTime = value;
                    }
                }

            }

            protected Vector3 position;
            public Vector3 Position
            {
                get { return position; }
                set { position = value; }
            }

            protected emitter baseEmitter;
            public emitter BaseEmitter
            {
                get { return baseEmitter; }
                set { baseEmitter = value; }
            }

            protected float colourRandomess;
            public float ColourRandomess
            {
                get { return colourRandomess; }
                set { colourRandomess = value; }
            }

            protected float particleVerlocity;
            public float ParticleVerlocity
            {
                get { return particleVerlocity; }
                set { particleVerlocity = value; }
            }

            protected float verlocityRandomess;
            public float VerlocityRandomess
            {
                get { return verlocityRandomess; }
                set { verlocityRandomess = value; }
            }

            protected Color4 beginColour;
            public Color4 BeginColour
            {
                get { return beginColour; }
                set { beginColour = value; }
            }

            protected Color4 endColour;
            public Color4 EndColour
            {
                get { return endColour; }
                set { endColour = value; }
            }

            protected float particleLife;
            public float ParticleLife
            {
                get { return particleLife; }
                set { particleLife = value; }
            }

            protected Random rnd;

            public emitter()
            {
                rnd = new Random();
            }

            public bool CanGenerate()
            {
                if (createElapsedTime >= sprayRate)
                {
                    createElapsedTime = 0;
                    return true;
                }
                else
                {
                    return false;
                }
            
            }

            public abstract Particle GenerateParticle();

            public abstract Vector3 GetParticlePosition();

            public abstract void GenerateNext(ref Particle particle);
                // Generate next particle from old particle
        
        }

        public class point : emitter
        {
            public override Particle GenerateParticle()
            {
                    Particle result = new Particle();
                    GenerateNext(ref result);
                    return result;
            }

            public override Vector3 GetParticlePosition()
            {
                if (baseEmitter != null)
                {
                    return position + baseEmitter.GetParticlePosition();
                }
                else
                {
                    return position;
                }
            }

            public override void GenerateNext(ref Particle particle)
            {
                float number = (float)rnd.NextDouble();
                number = number * 2.0f - 1.0f;

                float a, b;
                a = (float)rnd.NextDouble() * 2.0f - 1.0f;
                b = (float)rnd.NextDouble() * 2.0f - 1.0f;

                //float newRadius = (float)Math.Cos(a);
                //normal.Y = (float)Math.Sin(a);
                //normal.X = (float)(Math.Cos(b) * newRadius);
                //normal.Z = (float)(Math.Sin(b) * newRadius);

                Vector3 normal;
                normal = Vector3.Add(sprayDirection, new Vector3(number * sprayFocus.X, a * sprayFocus.Y, b * sprayFocus.Z));
   
                particle.Verlocity = Add(normal * particleVerlocity, verlocityRandomess * number);
                particle.LifeTime = particleLife;
                particle.ElapsedTime = 0;
                
                //Colour
                float factor = colourRandomess * number;

                particle.BeginColour = new Vector3(
                    beginColour.Red + factor,
                    beginColour.Green + factor,
                    beginColour.Blue + factor);

                particle.EndColour = new Vector3(
                    endColour.Red + factor,
                    endColour.Green + factor,
                    endColour.Blue + factor);

               particle.Position = GetParticlePosition();

            }

            private Vector3 Add(Vector3 vector, float value)
            {
                vector.X += value;
                vector.Y += value;
                vector.Z += value;
                return vector;
            }
        }

        public class SphereFollow : emitter
        {

            private float radius;

            public float Radius
            {
                get { return radius; }
                set { radius = value; }
            }

            private float rotateSpeed;

            public float RotateSpeed
            {
                get { return rotateSpeed; }
                set { rotateSpeed = value; }
            }


            public override Particle GenerateParticle()
            {
                Particle result = new Particle();
                GenerateNext(ref result);
                return result;
            }

            public override Vector3 GetParticlePosition()
            {

                Vector3 result;

                float angle = rotateSpeed * absoluteElapsedTime;

                float newRadius = (float)Math.Cos(angle) * radius;
                result.Y = (float)Math.Sin(angle) * radius;
                result.X = (float)(Math.Cos(angle) * newRadius);
                result.Z = (float)(Math.Sin(angle) * newRadius);

                result += Position;

                if (baseEmitter != null)
                {
                    return result + baseEmitter.GetParticlePosition(); ;
                }
                else
                {

                    return result;

                }
            }

            public override void GenerateNext(ref Particle particle)
            {
                float number = (float)rnd.NextDouble();
                number = number * 2.0f - 1.0f;

                float a, b;
                a = (float)rnd.NextDouble() * 2.0f - 1.0f;
                b = (float)rnd.NextDouble() * 2.0f - 1.0f;

                Vector3 normal;
                normal = Vector3.Add(sprayDirection, new Vector3(number * sprayFocus.X, a * sprayFocus.Y, b * sprayFocus.Z));

                particle.Verlocity = Add(normal * particleVerlocity, verlocityRandomess * number);
                particle.LifeTime = particleLife;
                particle.ElapsedTime = 0;

                //Colour
                float factor = colourRandomess * number;

                particle.BeginColour = new Vector3(
                    beginColour.Red + factor,
                    beginColour.Green + factor,
                    beginColour.Blue + factor);

                particle.EndColour = new Vector3(
                    endColour.Red + factor,
                    endColour.Green + factor,
                    endColour.Blue + factor);

                particle.Position = GetParticlePosition();

            }

            private Vector3 Add(Vector3 vector, float value)
            {
                vector.X += value;
                vector.Y += value;
                vector.Z += value;
                return vector;
            }
        }

        public class CircleFollow : emitter
        {

            private float radius;

            public float Radius
            {
                get { return radius; }
                set { radius = value; }
            }

            private float rotateSpeed;

            public float RotateSpeed
            {
                get { return rotateSpeed; }
                set { rotateSpeed = value; }
            }


            public override Particle GenerateParticle()
            {
                Particle result = new Particle();
                GenerateNext(ref result);
                return result;
            }

            public override Vector3 GetParticlePosition()
            {

                Vector3 result;

                float angle = rotateSpeed * absoluteElapsedTime;
                result.X = (float)(Math.Cos(angle) * radius);
                result.Z = (float)(Math.Sin(angle) * radius);
                result.Y = 0;

                result += Position;

                if (baseEmitter != null)
                {
                    return result + baseEmitter.GetParticlePosition(); ;
                }
                else
                {
                    return result;
                }
            }

            public override void GenerateNext(ref Particle particle)
            {
                float number = (float)rnd.NextDouble();
                number = number * 2.0f - 1.0f;

                float a, b;
                a = (float)rnd.NextDouble() * 2.0f - 1.0f;
                b = (float)rnd.NextDouble() * 2.0f - 1.0f;

                Vector3 normal;
                normal = Vector3.Add(sprayDirection, new Vector3(number * sprayFocus.X, a * sprayFocus.Y, b * sprayFocus.Z));

                particle.Verlocity = Add(normal * particleVerlocity, verlocityRandomess * number);
                particle.LifeTime = particleLife;
                particle.ElapsedTime = 0;

                //Colour
                float factor = colourRandomess * number;

                particle.BeginColour = new Vector3(
                    beginColour.Red + factor,
                    beginColour.Green + factor,
                    beginColour.Blue + factor);

                particle.EndColour = new Vector3(
                    endColour.Red + factor,
                    endColour.Green + factor,
                    endColour.Blue + factor);

                if (baseEmitter != null)
                {
                    particle.Position = Vector3.Add(GetParticlePosition(), baseEmitter.GetParticlePosition());
                }
                else
                {
                    particle.Position = GetParticlePosition(); ;
                }

            }

            private Vector3 Add(Vector3 vector, float value)
            {
                vector.X += value;
                vector.Y += value;
                vector.Z += value;
                return vector;
            }
        }

        public class Particle
        {
            private Vector3 position;
	        public Vector3 Position
	        {
		        get { return position;}
		        set { position = value;}
	        }

            private Vector3 verlocity;
            public Vector3 Verlocity
            {
                get { return verlocity; }
                set { verlocity = value; }
            }

            private Vector3 acceleration;
            public Vector3 Acceleration
            {
                get { return acceleration; }
                set { acceleration = value; }
            }

            private float absoluteTime;
            public float ElapsedTime
            {
                get { return absoluteTime; }
                set { absoluteTime = value; }
            }

            private float lifeTimeInv;
            private float lifeTime;
            public float LifeTime
            {
                get { return lifeTime; }
                set 
                { lifeTime = value;
                lifeTimeInv = 1 / value;
                }   
            }

            private Vector3 beginColour;
            public Vector3 BeginColour
            {
                get { return beginColour; }
                set { beginColour = value; }
            }

            private Vector3 endColour;
            public Vector3 EndColour
            {
                get { return endColour; }
                set { endColour = value; }
            }
            
            public CustomVertex.PositionColored Update(float elapsedTime, out bool particleDead)
            {

                // Update Times
                absoluteTime += elapsedTime;

                // Update Position
                verlocity += acceleration * elapsedTime;
                position += verlocity * elapsedTime;

                // Update Color
                float factor  = absoluteTime * lifeTimeInv;
                float alpha = 1.0f - factor;

                // Alive Status
                particleDead = absoluteTime > lifeTime;

                if (factor > 1.0f)
                {
                    alpha = 0.0f;
                    factor = 1.0f;
                }

                // Create Vertex
                return new CustomVertex.PositionColored(
                    // Position
                    position,
                    // Colour (Color4 constructor: red, green, blue, alpha)
                    new Color4(
                    lerp(beginColour.X, endColour.X, factor),
                    lerp(beginColour.Y, endColour.Y, factor),
                    lerp(beginColour.Z, endColour.Z, factor),
                    alpha).ToArgb());
            }

            float lerp(float a, float b, float f)
            {
                return a + f * (b - a);
            }

        }
        
        private Texture texture;

        List<Particle> particles;
        Device device;
        CustomVertex.PositionColored[] vertices;

        emitter particleEmitter;
        public emitter ParticleEmitter
        {
            get { return particleEmitter; }
            set { particleEmitter = value; }
        }

        int RenderCount;
        int MaxCount;        
        float size;

        public Particles(int maxParticles, Device direct3dDeivce, float particleSize, string Texture)
        {

            device = direct3dDeivce;

            texture = SharpDX.Direct3D9.Texture.FromFile(device, Texture);

            MaxCount = maxParticles;

            particles = new List<Particle>(maxParticles);
            
            vertices = new CustomVertex.PositionColored[maxParticles];

            size = particleSize;

            SetEffect();
           
        }

        public void Update(float elapsedTime)
        {

            RenderCount = particles.Count;

            particleEmitter.CreateElapsedTime += elapsedTime;
            particleEmitter.AbsoluteElapsedTime += elapsedTime;

            if (RenderCount < MaxCount && particleEmitter.CanGenerate())
            {
                particles.Add(particleEmitter.GenerateParticle());

            }

            bool dead; 
            Particle particle;
            for (int index = 0; index < RenderCount; index++)
            {
                particle = particles[index];
                vertices[index] = particle.Update(elapsedTime, out dead);

                if (dead && particleEmitter.CanGenerate())
                {
                    particleEmitter.GenerateNext(ref particle);
                    
                    // Is our original particle updated?? YES!!
                    //particles[index] = particle;
                }
                
            }

        }

        public void SetEffect()
        {

            device.SetRenderState(RenderState.PointScaleA, size * 0.5f);
            device.SetRenderState(RenderState.PointScaleB, size * 0.5f);
            device.SetRenderState(RenderState.PointScaleC, size * 0.5f);

            device.SetRenderState(RenderState.PointSize, size);

            device.SetRenderState(RenderState.PointScaleEnable, true);

            //device.RenderState.AlphaBlendOperation = BlendOperation.Add;

            //device.TextureState[0].ColorArgument0 = TextureArgument.TextureColor;
            //device.TextureState[0].ColorArgument1 = TextureArgument.Current;

            //device.TextureState[0].AlphaArgument0 = TextureArgument.Current;
            //device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
            //device.TextureState[0].AlphaOperation = TextureOperation.Lerp;

            //de.TextureState[0].ColorOperation = TextureOperation.Modulate;
            //de.TextureState[0].ColorArgument0 = TextureArgument.Diffuse;
            //de.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;

            //de.TextureState[0].AlphaOperation = TextureOperation.Modulate;
            //de.TextureState[0].AlphaArgument0 = TextureArgument.Diffuse;
            //de.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;


            //device.TextureState[0].AlphaOperation = TextureOperation.SelectArg1;
            //device.TextureState[0].AlphaArgument1 = TextureArgument.Diffuse;
            //d3dDevice.TextureState[0].AlphaArgument2 = TextureArgument.TextureColor; //ignored
        }

        public void Render()
        {
            device.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.SelectArg1);
            device.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Diffuse);

            device.VertexFormat = CustomVertex.PositionColored.Format;
            device.SetTexture(0, texture);

            device.SetRenderState(RenderState.Lighting, false);
            device.SetRenderState(RenderState.ZWriteEnable, false);

            device.SetRenderState(RenderState.PointSpriteEnable, true);

            device.SetRenderState(RenderState.AlphaBlendEnable, true);
            device.SetRenderState(RenderState.AlphaTestEnable, true);

            device.SetRenderState(RenderState.DestinationBlend, Blend.One);
            device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);

            //device.RenderState.AlphaDestinationBlend = Blend.One;
            //device.RenderState.AlphaSourceBlend = Blend.SourceColor;

            device.DrawUserPrimitives(PrimitiveType.PointList, RenderCount, vertices);

            device.SetRenderState(RenderState.AlphaBlendEnable, false);
            device.SetRenderState(RenderState.AlphaTestEnable, false);

            device.SetRenderState(RenderState.ZWriteEnable, true);

        }

    }
}
