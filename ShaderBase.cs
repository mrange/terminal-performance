abstract class ShaderBase
{
  void SeqFor(int fromInclusive, int toExclusive, Action<int> body)
  {
    for(var i=fromInclusive;i<toExclusive;++i) 
    {
      body(i);
    }
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
