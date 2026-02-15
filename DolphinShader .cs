sealed class DolphinShader : ShaderBase
{
  float   _inv;
  float   _t  ;
  Vector2 _res;
  Vector4 _ROT;
  Vector3 _FontColor;
  Texture _mandus=Assets.Tex_Mandus;
  int     _fontX;
  int     _fontY;
  protected override void Setup(int width, int height, double time)
  {
    var t=(float)time;
    _t=t;
    _res=new(width, height);
    _inv=1/_res.Y;
    _ROT=Rot(t);
    _FontColor=Assets.Palette_SRGB_Tic80[(int)(t*8)&0xF];
    _fontX=(int)(40F*Sin(t))+40;
    t=Fract(t)-.5F;    
    _fontY=(int)(160*(.25+t*t))-20;
  }

  protected override Color Run(int x, int y)
  {
    Vector2
      c=new (x,y)
    , p=(c+c-_res)*_inv
    ;

    Vector3
      C=Vector3.Zero
    ;

    Vector4
      T
    ;
    C=Assets.Palette_SRGB_Tic80[1];
    p*=.5F;
    p+=Cos(_t*new Vector2(1,.707F));
    p*=Sin(_t);
    p=RotYX(_ROT,p);
    p*=new Vector2(80F/128F,1);
    p+=new Vector2(.5F);
    var n=Floor(p);
    var f=p-n;
    p=Lerp(f,Vector2.One-f,n-2F*Floor(.5F*p));

    T=_mandus.Linear(p);
    C=Lerp(C,T.AsVector3(),T.W);

    T=Print(x,y,"Impulse!!!",_fontX+1,_fontY+1,Vector3.Zero);
    C=Lerp(C,T.AsVector3(),T.W);

    T=Print(x,y,"Impulse!!!",_fontX,_fontY,_FontColor);
    C=Lerp(C,T.AsVector3(),T.W);

    return ToColor(C);
  }
}
