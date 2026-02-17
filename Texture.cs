using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

class Texture
{
  readonly int        _width  ;
  readonly int        _height ;
  readonly Vector4[]  _pixels ;
  readonly Vector2    _dim    ;
  readonly Vector2    _cscale ;
  readonly Vector2    _coff   ;

  public Texture(int width, int height, Vector4[] pixels)
  {
    Debug.Assert(pixels.Length==width*height);
    _width  = width ;
    _height = height;
    _pixels = pixels;
    _dim    = new(width, height);
    _cscale = .5F*new Vector2(((float)height)/width,1F);
    _coff   = new Vector2(.5F);
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
  public Vector4 Texel(int x, int y)
  {
    var xx=Clamp(x,0,_width-1);
    var yy=Clamp(y,0,_height-1);
    return _pixels[yy*_width+xx];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector4 Texel(Vector2 ip)
  {
    // Assumes ip has no fractional part
    var p=Clamp(ip,Vector2.Zero,_dim-Vector2.One);
    return _pixels[(int)FusedMultiplyAdd(p.Y,_dim.X,p.X)];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector4 Nearest(Vector2 p)
  {
    return Texel(Round(p*_dim));
  }

  public Vector4 Linear(Vector2 p)
  {
    p*=_dim;
    var n0 = Floor(p);
    var f0 = p-n0;
    var c0 = Texel(n0);
    var c1 = Texel(n0+new Vector2(1,0));
    var c2 = Texel(n0+new Vector2(0,1));
    var c3 = Texel(n0+new Vector2(1,1));
    var c4 = Lerp(c0,c1,f0.X);
    var c5 = Lerp(c2,c3,f0.X);
    var c6 = Lerp(c4,c5,f0.Y);

    return c6;
  }

  public Vector4 CenteredLinear(Vector2 p)
  {
    return Linear(FusedMultiplyAdd(_cscale,p,_coff));
  }

}
