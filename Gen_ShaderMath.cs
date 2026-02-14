
static partial class ShaderMath
{
  static readonly Vector4 _maskXY = Vector128.Create(0,0,~0,~0).AsSingle().AsVector4();
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 RotXY(Vector4 r, Vector4 p)
  {
    return Vector4.FusedMultiplyAdd(
      Vector4.Shuffle(r,2,1,3,3)
    , Vector4.Shuffle(p,1,0,2,3)
    , Vector4.FusedMultiplyAdd(
        Vector4.Shuffle(r,0,0,3,3)
      , p
      , Vector4.BitwiseAnd(p,_maskXY)
      )
    );
  }

  static readonly Vector4 _maskXZ = Vector128.Create(0,~0,0,~0).AsSingle().AsVector4();
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 RotXZ(Vector4 r, Vector4 p)
  {
    return Vector4.FusedMultiplyAdd(
      Vector4.Shuffle(r,2,3,1,3)
    , Vector4.Shuffle(p,2,1,0,3)
    , Vector4.FusedMultiplyAdd(
        Vector4.Shuffle(r,0,3,0,3)
      , p
      , Vector4.BitwiseAnd(p,_maskXZ)
      )
    );
  }

  static readonly Vector4 _maskXW = Vector128.Create(0,~0,~0,0).AsSingle().AsVector4();
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 RotXW(Vector4 r, Vector4 p)
  {
    return Vector4.FusedMultiplyAdd(
      Vector4.Shuffle(r,2,3,3,1)
    , Vector4.Shuffle(p,3,1,2,0)
    , Vector4.FusedMultiplyAdd(
        Vector4.Shuffle(r,0,3,3,0)
      , p
      , Vector4.BitwiseAnd(p,_maskXW)
      )
    );
  }

  static readonly Vector4 _maskYX = Vector128.Create(0,0,~0,~0).AsSingle().AsVector4();
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 RotYX(Vector4 r, Vector4 p)
  {
    return Vector4.FusedMultiplyAdd(
      Vector4.Shuffle(r,1,2,3,3)
    , Vector4.Shuffle(p,1,0,2,3)
    , Vector4.FusedMultiplyAdd(
        Vector4.Shuffle(r,0,0,3,3)
      , p
      , Vector4.BitwiseAnd(p,_maskYX)
      )
    );
  }

  static readonly Vector4 _maskYZ = Vector128.Create(~0,0,0,~0).AsSingle().AsVector4();
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 RotYZ(Vector4 r, Vector4 p)
  {
    return Vector4.FusedMultiplyAdd(
      Vector4.Shuffle(r,3,2,1,3)
    , Vector4.Shuffle(p,0,2,1,3)
    , Vector4.FusedMultiplyAdd(
        Vector4.Shuffle(r,3,0,0,3)
      , p
      , Vector4.BitwiseAnd(p,_maskYZ)
      )
    );
  }

  static readonly Vector4 _maskYW = Vector128.Create(~0,0,~0,0).AsSingle().AsVector4();
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 RotYW(Vector4 r, Vector4 p)
  {
    return Vector4.FusedMultiplyAdd(
      Vector4.Shuffle(r,3,2,3,1)
    , Vector4.Shuffle(p,0,3,2,1)
    , Vector4.FusedMultiplyAdd(
        Vector4.Shuffle(r,3,0,3,0)
      , p
      , Vector4.BitwiseAnd(p,_maskYW)
      )
    );
  }

  static readonly Vector4 _maskZX = Vector128.Create(0,~0,0,~0).AsSingle().AsVector4();
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 RotZX(Vector4 r, Vector4 p)
  {
    return Vector4.FusedMultiplyAdd(
      Vector4.Shuffle(r,1,3,2,3)
    , Vector4.Shuffle(p,2,1,0,3)
    , Vector4.FusedMultiplyAdd(
        Vector4.Shuffle(r,0,3,0,3)
      , p
      , Vector4.BitwiseAnd(p,_maskZX)
      )
    );
  }

  static readonly Vector4 _maskZY = Vector128.Create(~0,0,0,~0).AsSingle().AsVector4();
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 RotZY(Vector4 r, Vector4 p)
  {
    return Vector4.FusedMultiplyAdd(
      Vector4.Shuffle(r,3,1,2,3)
    , Vector4.Shuffle(p,0,2,1,3)
    , Vector4.FusedMultiplyAdd(
        Vector4.Shuffle(r,3,0,0,3)
      , p
      , Vector4.BitwiseAnd(p,_maskZY)
      )
    );
  }

  static readonly Vector4 _maskZW = Vector128.Create(~0,~0,0,0).AsSingle().AsVector4();
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 RotZW(Vector4 r, Vector4 p)
  {
    return Vector4.FusedMultiplyAdd(
      Vector4.Shuffle(r,3,3,2,1)
    , Vector4.Shuffle(p,0,1,3,2)
    , Vector4.FusedMultiplyAdd(
        Vector4.Shuffle(r,3,3,0,0)
      , p
      , Vector4.BitwiseAnd(p,_maskZW)
      )
    );
  }

  static readonly Vector4 _maskWX = Vector128.Create(0,~0,~0,0).AsSingle().AsVector4();
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 RotWX(Vector4 r, Vector4 p)
  {
    return Vector4.FusedMultiplyAdd(
      Vector4.Shuffle(r,1,3,3,2)
    , Vector4.Shuffle(p,3,1,2,0)
    , Vector4.FusedMultiplyAdd(
        Vector4.Shuffle(r,0,3,3,0)
      , p
      , Vector4.BitwiseAnd(p,_maskWX)
      )
    );
  }

  static readonly Vector4 _maskWY = Vector128.Create(~0,0,~0,0).AsSingle().AsVector4();
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 RotWY(Vector4 r, Vector4 p)
  {
    return Vector4.FusedMultiplyAdd(
      Vector4.Shuffle(r,3,1,3,2)
    , Vector4.Shuffle(p,0,3,2,1)
    , Vector4.FusedMultiplyAdd(
        Vector4.Shuffle(r,3,0,3,0)
      , p
      , Vector4.BitwiseAnd(p,_maskWY)
      )
    );
  }

  static readonly Vector4 _maskWZ = Vector128.Create(~0,~0,0,0).AsSingle().AsVector4();
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 RotWZ(Vector4 r, Vector4 p)
  {
    return Vector4.FusedMultiplyAdd(
      Vector4.Shuffle(r,3,3,1,2)
    , Vector4.Shuffle(p,0,1,3,2)
    , Vector4.FusedMultiplyAdd(
        Vector4.Shuffle(r,3,3,0,0)
      , p
      , Vector4.BitwiseAnd(p,_maskWZ)
      )
    );
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 RotXY(Vector4 r, Vector3 p)
  {
    return RotXY(r,p.AsVector4()).AsVector3();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 RotXZ(Vector4 r, Vector3 p)
  {
    return RotXZ(r,p.AsVector4()).AsVector3();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 RotYX(Vector4 r, Vector3 p)
  {
    return RotYX(r,p.AsVector4()).AsVector3();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 RotYZ(Vector4 r, Vector3 p)
  {
    return RotYZ(r,p.AsVector4()).AsVector3();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 RotZX(Vector4 r, Vector3 p)
  {
    return RotZX(r,p.AsVector4()).AsVector3();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 RotZY(Vector4 r, Vector3 p)
  {
    return RotZY(r,p.AsVector4()).AsVector3();
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 RotXY(Vector4 r, Vector2 p)
  {
    return RotXY(r,p.AsVector4()).AsVector2();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 RotYX(Vector4 r, Vector2 p)
  {
    return RotYX(r,p.AsVector4()).AsVector2();
  }

}

