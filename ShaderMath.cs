
static partial class ShaderMath
{
  readonly static Vector3 _27     = new(27);
  readonly static Vector3 _255    = new(255);
  readonly static Vector3 _linear = new(.2126F, .7152F, .0722F);
  readonly static Vector3 _srgb   = new(.2990F, .5870F, .1140F);
  readonly static Vector3 _rota   = new(0,11,33);
  readonly static Vector3 _i255   = new(1F/255F);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Fract(float x)
  {
    return x-MathF.Floor(x);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Clamp(float x, float min, float max)
  {
    return Min(Max(x, min), max);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Clamp(int x, int min, int max)
  {
    return x<min?min:(x>max?max:x);
  }

  // License: WTFPL, author: sam hocevar, found: https://stackoverflow.com/a/17897228/418488
  readonly static Vector3 _K      = new Vector3(3,2,1)/new Vector3(3);
  public static Vector3 HSV2RGB(Vector3 c) 
  {
    var p = Abs(Fract(Shuffle(c,0,0,0)+_K)*6F-new Vector3(3));
    return c.Z*Lerp(Vector3.One, Vector3.Clamp(p-Vector3.One,Vector3.Zero,Vector3.One),c.Y);
  }

  public static Vector3 RGB(byte r, byte g, byte b) 
  {
    return new Vector3(r,g,b)*_i255;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float LumLinear(Vector3 x)
  {
    return Dot(x,_linear);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float LumSRGB(Vector3 x)
  {
    return Dot(x,_srgb);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Smoothstep(float edge0, float edge1, float x)
  {
    float t=Clamp((x-edge0)/(edge1-edge0),0,1);
    return t*t*(3-2*t);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 TanhApprox(Vector3 x)
  {
    var x2=x*x;
    return Vector3.Clamp(x*(_27+x2)/(_27+9*x2),-Vector3.One,Vector3.One);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 RotApprox(float a)
  {
    return Cos(new Vector3(a)+_rota).AsVector4();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Rot(float a)
  {
    var (s,c) = SinCos(a);
    return new (c,s,-s,0);
  }

  public static Color ToColor(Vector3 c)
  {
    var C=Vector3.Clamp(c,Vector3.Zero,Vector3.One)*_255;
    return new((byte)C.X,(byte)C.Y,(byte)C.Z);
  }

  public static Vector3 FromColor(Color c)
  {
    return new Vector3(c.R,c.G,c.B)*_i255;
  }

}
