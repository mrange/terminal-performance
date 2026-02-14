
static partial class ShaderMath
{
  readonly static Vector3 _27     = new(27);
  readonly static Vector3 _255    = new(255);
  readonly static Vector3 _linear = new(.2126F, .7152F, .0722F);
  readonly static Vector3 _srgb   = new(.2990F, .5870F, .1140F);
  readonly static Vector3 _rota   = new(0,11,33);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float LumLinear(Vector3 x)
  {
    return Vector3.Dot(x,_linear);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float LumSrgb(Vector3 x)
  {
    return Vector3.Dot(x,_srgb);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Smoothstep(float edge0, float edge1, float x)
  {
    float
      t=(float)Math.Clamp((x-edge0)/(edge1-edge0),0,1)
      ;
    return t*t*(3-2*t);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 TanhApprox(Vector3 x)
  {
    Vector3
      x2=x*x
    ;
    return Vector3.Clamp(x*(_27+x2)/(_27+9*x2),-Vector3.One,Vector3.One);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 RotApprox(float a)
  {
    return Vector3.Cos(new Vector3(a)+_rota).AsVector4();
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
}
