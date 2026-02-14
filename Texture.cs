using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System.Runtime.CompilerServices;
using static System.Numerics.Vector2;

class Texture
{
  readonly int        _width  ;
  readonly int        _height ;
  readonly Vector4[]  _pixels ;
  readonly Vector2    _dim    ;
  readonly Vector2    _idim   ;

  public Texture(int width, int height, Vector4[] pixels)
  {
    Debug.Assert(pixels.Length==width*height);
    _width  = width ;
    _height = height;
    _pixels = pixels;
    _dim    = new(width, height);
    _idim   = One/_dim;
  }

  public static Texture LoadFromFile(string fileName)
  {
    using var image = Image.Load<Rgba32>(fileName);
    var width       = image.Width;
    var height      = image.Height;
    var pixels      = new Vector4[width*height];
    var div         = new Vector4(1/255F);

    image.ProcessPixelRows(accessor =>
    {
      for (var y=0; y<height; ++y)
      {
        var row = accessor.GetRowSpan(y);
        for (var x=0; x<width; ++x)
        {
          var p = row[x];
          pixels[y*width+x] = div*new Vector4(
            p.R
          , p.G
          , p.B
          , p.A
          );
        }
      }
    });

    return new (width, height, pixels);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  Vector4 Internal(Vector2 ip)
  {
    // Assumes ip has no fractional part
    var p=Clamp(ip,Zero,_dim-One);
    return _pixels[(int)(p.Y*_dim.X+p.X)];
  }

  public Vector4 Nearest(Vector2 p)
  {
    return Internal(Round(p*_dim));
  }

  public Vector4 Linear(Vector2 p)
  {
    p*=_dim;
    // No Vector2.Floor? Why?
    var n0 = new Vector2(Floor(p.X),Floor(p.Y));
    var f0 = p-n0;
    var c0 = Internal(n0+new Vector2(0,0));
    var c1 = Internal(n0+new Vector2(1,0));
    var c2 = Internal(n0+new Vector2(0,1));
    var c3 = Internal(n0+new Vector2(1,1));
    var c4 = Vector4.Lerp(c0,c1,f0.X);
    var c5 = Vector4.Lerp(c2,c3,f0.X);
    var c6 = Vector4.Lerp(c4,c5,f0.Y);

    return c6;
  }
}
