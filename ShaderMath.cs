using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

static class ShaderMath
{
  readonly static Vector3 _27     = new(27);
  readonly static Vector3 _255    = new(255);
  readonly static Vector3 _linear = new(.2126F, .7152F, .0722F);
  readonly static Vector3 _srgb   = new(.2990F, .5870F, .1140F);
  readonly static Vector3 _rota   = new(0,11,33);
  readonly static Vector4 _unit   = new(0,1,2,3);
  readonly static Vector4 _maskzw = Vector128.Create(0,0,~0,~0).AsSingle().AsVector4();

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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 RotXY(Vector4 r, Vector4 p)
  {
    return Vector4.FusedMultiplyAdd(
      Vector4.Shuffle(r,2,1,3,3)
    , Vector4.Shuffle(p,1,0,2,3)
    , Vector4.FusedMultiplyAdd(
        Vector4.Shuffle(r,0,0,3,3)
      , p
      , Vector4.BitwiseAnd(p,_maskzw)
      )
    );
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 RotXY(Vector4 r, Vector3 p)
  {
    return RotXY(r,p.AsVector4()).AsVector3();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 RotXY(Vector4 r, Vector2 p)
  {
    return RotXY(r,p.AsVector4()).AsVector2();
  }

  public static Color ToColor(Vector3 c)
  {
    var C=Vector3.Clamp(c,Vector3.Zero,Vector3.One)*_255;
    return new((byte)C.X,(byte)C.Y,(byte)C.Z);
  }
}
