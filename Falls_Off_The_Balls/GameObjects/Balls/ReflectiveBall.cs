using System;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;

namespace Falls_Off_The_Balls.GameObjects;

public class ReflectiveBall : Ball
{

    public ReflectiveBall(BallSize ballSize, float dirX, BallType ballType, Vector2 ballInitialPosition = default)
                        :base(ballSize, dirX, ballType, ballInitialPosition)
    {
        Vector2 direction = new Vector2(dirX, -1);

        // Multiply the direction vector by the movement speed to get the
        // final velocity
        _velocity = direction * MOVEMENT_SPEED;
    }

    public override void Bounce(Vector2 normal)
    {
        Vector2 newPosition = Position;

        // Adjust the position based on the normal to prevent sticking to walls.
        if (normal.X != 0)
        {
            // We are bouncing off a vertical wall (left/right).
            // Move slightly away from the wall in the direction of the normal.
            newPosition.X += normal.X * (_ballSprite.Width * 0.1f);
        }

        if (normal.Y != 0)
        {
            // We are bouncing off a horizontal wall (top/bottom).
            // Move slightly way from the wall in the direction of the normal.
            newPosition.Y += normal.Y * (_ballSprite.Height * 0.1f);
        }

        // Apply the new position
        Position = newPosition;

        // Normalize before reflecting
        normal.Normalize();

        // Apply reflection based on the normal.
        _velocity = Vector2.Reflect(_velocity, normal);

        base.Bounce(normal);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if(FreezeHandler.freezeTimer <= 0)
        {
            // Update the position of the ball based on the velocity.
            Position += _velocity;
        }
    }

    protected override void LoadSprite()
    {
        _ballSprite = _ballType switch
        {
            BallType.GREEN_SQUARED => new Sprite(_greenBallSquaredTexture),
            BallType.DBLUE_SQUARED => new Sprite(_dBlueBallSquaredTexture),
            BallType.LBLUE_SQUARED => new Sprite(_lBlueBallSquaredTexture),
            _ => new Sprite(_greenBallSquaredTexture)
        };
    }

}
