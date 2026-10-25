using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Content;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;
using Microsoft.Xna.Framework.Media;
using MonoGameGum;
using MonoGame_Super_Pang.UI;
using MonoGame_Super_Pang.Backgrounds;
using MonoGame_Super_Pang.Config;
using System.Collections.Generic;
using MonoGame_Super_Pang.GameObjects;

namespace MonoGame_Super_Pang.Scenes;

public class TitleScene : Scene
{
    // The font to use to render normal text.
    private SpriteFont _font;

    private Background _levelBackground;

    private Random _backgroundRand;

    private static bool _volumeInitialized = false;

    // The 3d material  
    private Material _3dMaterial;

    private const int TITLE_LEVEL_CONFIG = 0;

    private const int SPIN_AMOUNT_Y = 150;

    private List<Ball> _balls;

    private Rectangle _roomBounds;

    private AnimatedSprite _orangeSpiralAnimation;
    private Vector2 _orangeSpiralAnimationPos;

    private Sprite _darkGreenBallSprite;
    private Vector2 _darkGreenBallSpritePos;
    private float _darkGreenBallRot = 0.0f;
    private float DARK_GREEN_ROT_FACTOR = 0.05f;

    private float _timerExplosion = 0.0f;
    private float TIME_EXPLOSION_MS = 500.0f;

    public override void Initialize()
    {
        // LoadContent is called during base.Initialize().
        base.Initialize();

        // While on the title screen, we can enable exit on escape so the player
        // can close the game by pressing the escape key.
        Core.ExitOnEscape = true;

        _roomBounds = Core.GraphicsDevice.PresentationParameters.Bounds;

        if(_volumeInitialized == false)
        {
            Core.Audio.SongVolume = 0.5f;
            Core.Audio.SoundEffectVolume = 0.5f;
            _volumeInitialized = true;
        }

        InitializeUI();
    }

    public override void LoadContent()
    {
        // Load the font for the standard text.
        _font = Core.Content.Load<SpriteFont>("fonts/04B_30");

        try
        {
            // Load the background theme music
            Song theme = Content.Load<Song>("audio/Music/14. Traveling the Sky");
            Core.Audio.PlaySong(theme);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load theme music: {ex.Message}");
        }

        Ball.LoadContent();
        _balls = [];

        _backgroundRand = new Random();
        int backgroundIndex = _backgroundRand.Next(0, LevelRegistry.AllLevels.Count);

        List<string> backgroundList = LevelRegistry.AllLevels[backgroundIndex].backgroundStr;
        List<Texture2D> clouds = new List<Texture2D>();

        foreach(string backgroundStr in backgroundList)
        {
            clouds.Add(Content.Load<Texture2D>(backgroundStr));
        }

        _levelBackground = new Background(clouds);

        PlayerStatsManager.LoadContent();

        PowerUpSpritesHandler.LoadContent();

        // Load the 3d effect 
        _3dMaterial = Core.SharedContent.WatchMaterial("effects/3dEffect");
        _3dMaterial.IsDebugVisible = false;

        var camera = new SpriteCamera3d();
        _3dMaterial.SetParameter("MatrixTransform", camera.CalculateMatrixTransform());
        _3dMaterial.SetParameter("ScreenSize", new Vector2(Core.GraphicsDevice.Viewport.Width, Core.GraphicsDevice.Viewport.Height));

        _3dMaterial.SetParameter("SpinAmountY", SPIN_AMOUNT_Y);

        //LoadLevel(LevelRegistry.AllLevels[TITLE_LEVEL_CONFIG]);

        _orangeSpiralAnimationPos = new Vector2(Core.GraphicsDevice.PresentationParameters.Bounds.Width*0.5f, Core.GraphicsDevice.PresentationParameters.Bounds.Height*0.4f);
    
        _darkGreenBallSprite = new Sprite(Ball.getGreenBallSquareTexture());
        _darkGreenBallSprite.Scale = new Vector2(0.8f);
        _darkGreenBallSprite.CenterOrigin();
        _darkGreenBallSpritePos = new Vector2(Core.GraphicsDevice.PresentationParameters.Bounds.Width*0.44f, Core.GraphicsDevice.PresentationParameters.Bounds.Height*0.3f);
    }

    public override void Update(GameTime gameTime)
    {
        GumService.Default.Update(gameTime);

        _levelBackground.Update(gameTime);

        _3dMaterial.Update();

        double totalSeconds = gameTime.TotalGameTime.TotalSeconds;
        float spinAmountX = (float)Math.Sin(totalSeconds * 0.8) * 0.1f;
        _3dMaterial.SetParameter("SpinAmountX", spinAmountX);

        TitlePanelManager.Update(gameTime);

        if(_orangeSpiralAnimation != null && _orangeSpiralAnimation.IsComplete)
        {
            _orangeSpiralAnimation = null;
        }

        if(_timerExplosion < TIME_EXPLOSION_MS)
        {
            float delta = gameTime.ElapsedGameTime.Milliseconds;
            _timerExplosion+=delta;
            if(_timerExplosion >= TIME_EXPLOSION_MS)
            {
                TextureAtlas spiralAtlas = TextureAtlas.FromFile(Core.Content, "images/titleEffects/spiral_effect.xml");
                Animation spiralAnimation = spiralAtlas.GetAnimation("orange-spiral-animation");
                _orangeSpiralAnimation = new AnimatedSprite(spiralAnimation)
                {
                    Scale = new Vector2(5.0f, 5.0f)
                };
                _orangeSpiralAnimation.CenterOrigin();
            }
        }

        if(TitlePanelManager.IsTitlePanelVisible())
        {
            foreach(Ball ball in _balls)
            {
                ball.Update(gameTime);
                Circle ballBounds = ball.GetBounds();
                Vector2 pos = ball.Position;

                if (ballBounds.Top < _roomBounds.Top)
                {
                    ball.Bounce(Vector2.UnitY);
                }
                else if (ballBounds.Bottom > _roomBounds.Bottom)
                {
                    ball.Bounce(-Vector2.UnitY);
                }

                if (ballBounds.Left < _roomBounds.Left)
                {
                    ball.Bounce(Vector2.UnitX);
                }
                else if (ballBounds.Right > _roomBounds.Right)
                {
                    ball.Bounce(-Vector2.UnitX);
                }
            }

            _orangeSpiralAnimation?.Update(gameTime);
            _orangeSpiralAnimation?.CenterOrigin();
            _darkGreenBallRot += DARK_GREEN_ROT_FACTOR;
            if(_darkGreenBallRot >= 2* Math.PI)
            {
                _darkGreenBallRot = 0.0f;
            }
            _darkGreenBallSprite.Rotation = _darkGreenBallRot;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        // Draw the background
        _levelBackground.Draw();

        if(TitlePanelManager.IsTitlePanelVisible())
        {
            Core.SpriteBatch.Begin();

            foreach(Ball ball in _balls)
            {
                ball.Draw();
            }

            Core.SpriteBatch.End();
        }

        // Begin the sprite batch to prepare for rendering.
        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp,
                                rasterizerState: RasterizerState.CullNone,
                                effect: _3dMaterial.Effect);

        if(TitlePanelManager.IsTitlePanelVisible())
        {
            _orangeSpiralAnimation?.Draw(Core.SpriteBatch, _orangeSpiralAnimationPos);

            if(_orangeSpiralAnimation == null && _timerExplosion >= TIME_EXPLOSION_MS)
            {
                TitlePanelManager.Draw();
                _darkGreenBallSprite.Draw(Core.SpriteBatch, _darkGreenBallSpritePos);
            }
        }

        // Always end the sprite batch when finished.
        Core.SpriteBatch.End();

        GumService.Default.Draw();
    }

    private void InitializeUI()
    {
        // Clear out any previous UI in case we came here from
        // a different screen:
        GumService.Default.Root.Children.Clear();

        // Create the directory if it doesn't exist yet
        Directory.CreateDirectory(PlayerStatsManager.saveDirectory);

        TitlePanelManager.LoadContent();
    }

    private void LoadLevel(LevelConfig config)
    {
        foreach(var spawnConfig in config.Balls)
        {
            BallType ballType = spawnConfig.BallType;
            switch(spawnConfig.BallType)
            {
                case BallType.GREEN_ROUND:
                case BallType.RED_ROUND:
                case BallType.BLUE_ROUND:
                    _balls.Add(new BouncingBall(spawnConfig.Size, spawnConfig.DirectionX, ballType, spawnConfig.Position));
                break;
                case BallType.LBLUE_SQUARED:
                case BallType.DBLUE_SQUARED:
                case BallType.GREEN_SQUARED:
                    _balls.Add(new ReflectiveBall(spawnConfig.Size, spawnConfig.DirectionX, ballType, spawnConfig.Position));
                break;
            }
        }
    }

}