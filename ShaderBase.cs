abstract class ShaderBase
{
  Texture _font8x16=Assets.Tex_Font8x16;

  void SeqFor(int fromInclusive, int toExclusive, Action<int> body)
  {
    for(var i=fromInclusive;i<toExclusive;++i) 
    {
      body(i);
    }
  }

  public Vector4 Print(
    int     x
  , int     y
  , string  text
  , int     offx
  , int     offy
  , Vector3 color
  , bool    serif=true
  , bool    half =false
  )
  {
    var ex=x-offx;
    var ey=y-offy+(half?y:0);
    if (ey<0||ey>15) return Vector4.Zero;
    var nx=ex>>3;
    if ((uint)nx>=(uint)text.Length) return Vector4.Zero;
    var cx=ex&0x7;
    var c=text[nx];
    if(c<33||c>127) return Vector4.Zero;
    cx+=((c-32)&0x1F)<<3;
    ey+=((c-32)>>1)&~0xF;
    return new Vector4(color, _font8x16.Texel(new (cx,ey)).W);
  }

  public void Render(RenderContext context, double time)
  {
    int width = context.Viewport.Width;
    int height = context.Viewport.Height;

    Setup(width, height+height, time);

#if DEBUG
    SeqFor(0, height, y =>
#else
    Parallel.For(0, height, y =>
#endif
    {
        for (var x = 0; x < width; ++x)
        {
          var c = context.GetCell(x,y);
          if (c is not null)
          {
            // Resolution doubler: U+2580
            c.SetSymbol('\x2580');
            c.SetForeground(Run(x, y+y+0));
            c.SetBackground(Run(x, y+y+1));
          }
        }
    });
  }

  protected abstract void Setup(int width, int height, double time);
  protected abstract Color Run(int x, int y);
}
