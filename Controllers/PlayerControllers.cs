using lda.Core;
using lda.Models;
using Microsoft.Xna.Framework.Input;

namespace lda.Controllers
{
    public class PlayerController
    {
        private readonly PlayerModel _model;
        private readonly InputHandler _input;

        public PlayerController(PlayerModel model, InputHandler input)
        {
            _model = model;
            _input = input;
        }

        public void Update()
        {
            if (_input.IsKeyDown(Keys.A) || _input.IsKeyDown(Keys.Left))
                _model.SetMoveDirection(-1);
            else if (_input.IsKeyDown(Keys.D) || _input.IsKeyDown(Keys.Right))
                _model.SetMoveDirection(1);
            else
                _model.Idle();

            if (_input.IsKeyPressed(Keys.Space))
                _model.RequestJump();

            _model.ExecuteJump();

            if (_input.IsKeyReleased(Keys.Space))
                _model.CutJump();

            if (_input.IsKeyPressed(Keys.J))
            {
                AttackDirection dir = AttackDirection.Right;
                if (_input.IsKeyDown(Keys.W) || _input.IsKeyDown(Keys.Up)) dir = AttackDirection.Up;
                else if (_input.IsKeyDown(Keys.S) || _input.IsKeyDown(Keys.Down)) dir = AttackDirection.Down;
                else dir = _input.IsKeyDown(Keys.A) || _input.IsKeyDown(Keys.Left) ? AttackDirection.Left : AttackDirection.Right;

                _model.Attack(dir);
            }
        }
    }
}