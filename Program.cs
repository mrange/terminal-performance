#define USE_SENDER
#define USE_BACKBUFFER
#define LIMIT_FPS

Console.OutputEncoding = Encoding.UTF8;

Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

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
var shader    = new StarFieldShader();
var capacity  = viewPort.Width*viewPort.Height*64;
var current   = new Utf8Buffer(capacity);
var send      = new Utf8Buffer(capacity);
var frameNo   = 0;
var fpsFrameNo= 0;
var isRunning = true;
var fpsStart  = sw.Elapsed.TotalSeconds;

#if USE_SENDER
using var sender = new Sender();
#else
using var stdOut = Console.OpenStandardOutput();
#endif

#if USE_BACKBUFFER
var bufferState=false;
#endif

while (isRunning)
{
  var frameStart = sw.Elapsed.TotalSeconds;
  ++frameNo;
  ++fpsFrameNo;
  if(fpsFrameNo>120)
  { 
    fpsFrameNo= 0;
    fpsStart  = sw.Elapsed.TotalSeconds;
  }

  shader.Render(renderContext, sw.Elapsed.TotalSeconds);

  current.Clear();

#if USE_BACKBUFFER
  current.WriteDisableDECPCCM();
  current.WriteMoveToBuffer(bufferState);
#endif

  current.WritePrelude();
  
  for(var i=0;i<cells.Length;++i)
  {
    current.WriteCell(cells[i]);
  }

  current.WriteString($"\r\u001b[49m\x1b[39m#{frameNo}, FPS:{fpsFrameNo/(sw.Elapsed.TotalSeconds-fpsStart):0}, RES:{viewPort.Width}x{viewPort.Height}");

#if USE_BACKBUFFER
  current.WriteEnableDECPCCM();
  bufferState=!bufferState;
#endif

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

  var frameEnd = sw.Elapsed.TotalSeconds;
  var waitFor= Math.Floor((1/120.0 + frameStart-frameStart)*1000);
  if(waitFor>0)
  {
#if LIMIT_FPS
    Thread.Sleep((int)waitFor);
#endif
  }

}

// Resets terminal
Console.WriteLine("\x1b[!p");

#if USE_SENDER
sealed class Sender : IDisposable
{
  sealed record Data(Utf8Buffer? Buffer);
  readonly Thread         _thread;
  readonly Stream         _stdOut;
  readonly object         _lock  = new();
  Data?                   _data  ;

  public Sender()
  {
    _stdOut = Console.OpenStandardOutput();
    _thread = new (OnRun);
    _thread.Start();
  }

  public void Send(Utf8Buffer buffer)
  {
    lock(_lock)
    {
      Debug.Assert(_data is null);
      _data = new(buffer);
      Monitor.Pulse(_lock);
    }
  }

  public void Dispose()
  {
    lock(_lock)
    {
      Debug.Assert(_data is null);
      // null indicates end
      _data = new(null);
      Monitor.Pulse(_lock);
    }
    _thread.Join();
    _stdOut.Dispose();
  }

  void OnRun(object? obj)
  {
    var cont=true;
    lock(_lock)
    {
      while(cont)
      {
        Utf8Buffer? buffer=null;

        while(_data is null)
        {
          Monitor.Wait(_lock);
        }

        buffer=_data.Buffer;
        _data = null;

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

sealed class Utf8Buffer(int capacity)
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
  readonly byte[]   utf8_disable_DECPCCM  = ToUTF8("\x1B[?64l");
  readonly byte[]   utf8_enable_DECPCCM   = ToUTF8("\x1B[?64h");
  readonly byte[]   utf8_move_to_page_1   = ToUTF8("\x1B[1 P");
  readonly byte[]   utf8_move_to_page_2   = ToUTF8("\x1B[2 P");


  public void Clear()
  { 
    Position = 0;
  }

  public void WritePrelude()
  {
    WriteBytes(utf8_Prelude);
  }

  public void WriteDisableDECPCCM()
  {
    WriteBytes(utf8_disable_DECPCCM);
  }

  public void WriteEnableDECPCCM()
  {
    WriteBytes(utf8_enable_DECPCCM);
  }

  public void WriteMoveToBuffer(bool b)
  {
    WriteBytes(b?utf8_move_to_page_1:utf8_move_to_page_2);
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void WriteBytes(byte[] s)
  {
    Buffer.BlockCopy(s,0,Bytes,Position,s.Length);
    Position+=s.Length;
  }
}

class Cell()
{
  public Color  Background = Color.Black;
  public Color  Foreground = Color.Black;
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

sealed record Viewport(int Width, int Height);

sealed record RenderContext(Viewport Viewport, Cell[] Cells)
{
  public Cell? GetCell(int x, int y)
  {
    if(x<0||x>=Viewport.Width||y<0||y>=Viewport.Height) return null;
    return Cells[Viewport.Width*y+x];
  }
}

record struct Color(byte R, byte G, byte B)
{
  public static readonly Color Black = new(0,0,0);
  public static readonly Color White = new(255,255,255);
}
