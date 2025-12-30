// Gives me a few extra FPS
#define USE_SENDER

using System.Diagnostics;
using System.Numerics;
using System.Text;
using static System.Math;
using static System.Numerics.Vector3;

Console.OutputEncoding = Encoding.UTF8;

var viewPort = new Viewport(
    Width : Console.BufferWidth
  , Height: Console.BufferHeight
  );

var cells = Enumerable
  .Range(0,viewPort.Width*viewPort.Height)
  .Select(_ => new Cell())
  .ToArray()
  ;

var renderContext = new RenderContext(
    viewPort
  , cells
  );

var sw        = Stopwatch.StartNew();
var shader    = new ApolloShader();
var capacity  = viewPort.Width*viewPort.Height*64;
var current   = new Buffer(capacity);
var send      = new Buffer(capacity);
var frameNo   = 0;
var fpsFrameNo= 0;
var isRunning = true;
var fpsStart  = sw.Elapsed.TotalSeconds;

#if USE_SENDER
using var sender = new Sender();
#else
using var stdOut = Console.OpenStandardOutput();
#endif


while (isRunning)
{
  current.Clear();
  ++frameNo;
  ++fpsFrameNo;
  if(fpsFrameNo>120)
  { 
    fpsFrameNo= 0;
    fpsStart  = sw.Elapsed.TotalSeconds;
  }

  shader.Render(renderContext, sw.Elapsed.TotalSeconds);

  current.WritePrelude();
  
  for(var i=0;i<cells.Length-viewPort.Width;++i)
  {
    current.WriteCell(cells[i]);
  }

  current.WriteString($"\u001b[49m\x1b[39m#{frameNo}, FPS:{fpsFrameNo/(sw.Elapsed.TotalSeconds-fpsStart):0}       ");

  (current, send) = (send, current);

#if USE_SENDER
  sender.Send(send);
#else
  stdOut.Write(send.Bytes, 0, send.Position);
#endif

  if (Console.KeyAvailable)
  {
    if (Console.ReadKey(true).Key == ConsoleKey.Escape)
    {
      isRunning = false;
    }
  }
}

// Resets terminal
Console.WriteLine("\x1b[!p");

#if USE_SENDER
class Sender : IDisposable
{
  readonly Thread         _thread;
  readonly Queue<Buffer?> _queue = new();
  readonly Stream         _stdOut;

  public Sender()
  {
    _stdOut = Console.OpenStandardOutput();
    _thread = new (OnRun);
    _thread.Start();
  }

  public void Send(Buffer buffer)
  {
    lock(_queue)
    {
      Debug.Assert(_queue.Count<1);
      _queue.Enqueue(buffer);
      Monitor.Pulse(_queue);
    }
  }

  public void Dispose()
  {
    lock(_queue)
    {
      Debug.Assert(_queue.Count<1);
      _queue.Clear();
      // null indicates end
      _queue.Enqueue(null);
      Monitor.Pulse(_queue);
    }
    _thread.Join();
    _stdOut.Dispose();
  }

  void OnRun(object? obj)
  {
    var cont=true;
    lock(_queue)
    {
      while(cont)
      {
        Buffer? buffer=null;

        while(_queue.Count == 0)
        {
          Monitor.Wait(_queue);
        }

        buffer=_queue.Dequeue();

        if (buffer is null)
        {
          cont = false;
          continue;
        }

        _stdOut.Write(buffer.Bytes, 0, buffer.Position);
      }
    }
  }
}
#endif

class Buffer(int capacity)
{
  public byte[] Bytes     = new byte[capacity];
  public int    Position  = 0;

  public static byte[] ToUTF8(string s)
  {
    return Encoding.UTF8.GetBytes(s);
  }

  // Disable cursor, move to top left corner
  readonly byte[]   utf8_Prelude  = ToUTF8("\x1b[?25l\x1b[H");
  readonly byte[]   utf8_Pixel_0  = ToUTF8("\u001b[48;2");
  readonly byte[]   utf8_Pixel_1  = ToUTF8("m\u001b[38;2");
  readonly byte[]   utf8_Pixel_2  = ToUTF8("m\x2580");
  readonly byte[][] utf8_Numbers  = Enumerable
    .Range(0,256)
    .Select(i => ToUTF8($";{i}"))
    .ToArray()
    ;

  public void Clear()
  { 
    Position = 0;
  }

  public void WritePrelude()
  {
    WriteBytes(utf8_Prelude);
  }

  public void WriteCell(Cell c)
  {
    var b = c.Background;
    var f = c.Foreground;
    WriteBytes(utf8_Pixel_0);
    WriteNumber(b.R);
    WriteNumber(b.G);
    WriteNumber(b.B);
    WriteBytes(utf8_Pixel_1);
    WriteNumber(f.R);
    WriteNumber(f.G);
    WriteNumber(f.B);
    WriteBytes(utf8_Pixel_2);
  }

  public void WriteNumber(byte b)
  {
    WriteBytes(utf8_Numbers[b]);
  }

  public void WriteString(string s)
  {
    WriteBytes(ToUTF8(s));
  }

  public void WriteBytes(byte[] s)
  {
    Array.Copy(s,0,Bytes,Position,s.Length);
    Position+=s.Length;
  }

}

class Cell()
{
  public Color  Background = Color.Black;
  public Color  Foreground = Color.White;
  public char   Symbol     = ' ';

  public void SetSymbol(char v)
  {
    Symbol=v;
  }

  public void SetForeground(Color color)
  {
    Foreground=color;
  }

  public void SetBackground(Color color)
  {
    Background=color;
  }
}
record Viewport(int Width, int Height);

record RenderContext(Viewport Viewport, Cell[] Cells)
{
  public Cell? GetCell(int x, int y)
  {
    var i=x+Viewport.Width*y;
    return 
        i>-1&&i<Cells.Length
      ? Cells[i]
      : null
      ;
  }
}

record struct Color(byte R, byte G, byte B)
{
  public static readonly Color Black = new(0,0,0);
  public static readonly Color White = new(255,255,255);
}

abstract class ShaderBase
{
    readonly static Vector3 _27   = new(27);
    readonly static Vector3 _255  = new(255);

    public static float Smoothstep(float edge0, float edge1, float x)
    {
      float
        t=(float)Clamp((x-edge0)/(edge1-edge0),0,1)
       ;
      return t*t*(3-2*t);
    }

    public static Color ToColor(Vector3 c)
    {
      var C=Clamp(c,Zero,One)*_255;
      return new((byte)C.X,(byte)C.Y,(byte)C.Z);
    }

    public static Vector3 TanhApprox(Vector3 x)
    {
      Vector3
        x2=x*x
      ;
      return Clamp(x*(_27+x2)/(_27+9*x2),-One,One);
    }

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

        // Not sure if Spectre handles parallel assignments?
        //  At least we won't modify the same cell from multiple threads
        Parallel.For(0, width, x =>
        {
            for (var y = 0; y < height; y++)
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

sealed class ApolloShader : ShaderBase
{
  readonly static Vector3 _Base = new(2,1,0);

  float   _inv;
  float   _fad;
  float   _sin;
  Vector2 _res;
  Vector3 _rot;

  protected override void Setup(int width, int height, double time)
  {
    float
      t=(float)time
    ;
    _res=new(width, height);
    _inv=1/_res.Y;
    _rot=Normalize(Sin(new Vector3(.2F*t+123)+new Vector3(0,1,2)));
    _sin=(float)Sin(.123*time);
#if DEBUG_WEIRD_FPS
    _fad=(float)(.25+.25*Cos(time));
#else
    _fad=.5F;
#endif
  }

  protected override Color Run(int x, int y)
  {
    Vector2
      c=new (x,y)
    , p=(c+c-_res)*_inv
    ;

    Vector3
      P=new(p.X,p.Y,.5F*_sin)
    ;

    float
      s=1
    , k
    ;

    P=Dot(P,_rot)*_rot+Cross(P,_rot);

    for(int i=0; i<3;++i)
    {
      P-=2F*Round(.5F*P);
      k=1.41F/Dot(P,P);
      P*=k;
      s*=k;
    }

    P=Abs(P)/s;
    k=Min(P.Z, new Vector2(P.X,P.Y).Length());

    return ToColor(
        k<5E-3F
      ? One
      : _fad/(1+k*k*5)*(One+Sin(_Base+new Vector3((float)(2+Log2(k)))))
      );
  }
}

sealed class BoxShader : ShaderBase
{
  readonly static Vector3 _Base = new(-0.7F,-0.2F,0.3F);

  float   _inv;
  float   _fad;
  Vector2 _res;
  Vector3 _rot;

  protected override void Setup(int width, int height, double time)
  {
    float
      t=(float)time
    ;
    _res=new(width, height);
    _inv=1/_res.Y;
    _rot=Normalize(Sin(new Vector3(t)+new Vector3(0,1,2)));
    _fad=.5F;
  }

  protected override Color Run(int x, int y)
  {
    Vector2
      c=new (x,y)
    , p=(c+c-_res)*_inv
    ;

    Vector3
      P
    , R=Normalize(new(p.X,p.Y,2))
    , r=_rot
    ;

    float
      z=0
    , d=1
    ;

    int
      i
    ;

    // No comments necessary...
    for(i=0;i<49&&z<4&&d>1e-3;++i)
    {
      P=z*R;
      P.Z-=3;
      P=Dot(P,r)*r+Cross(P,r);
      P*=P;
      d=(float)Sqrt(Sqrt(Dot(P,P)))-1;
      z+=d;
    }

    return ToColor(
        z<4
      ? _fad*(One+Sin(_Base-new Vector3(i/33F+2*(p.X+p.Y))))
      : Zero
      );
  }
}