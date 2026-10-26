using MonoGameGum.GueDeriving;

namespace Falls_Off_The_Balls.UI;

public class GameText : TextRuntime
{
    public GameText(string text)
    {
        Text = text;
        UseCustomFont = true;
        CustomFontFile = "fonts/04b_30.fnt";
        FontScale = 0.25f;
    }
}