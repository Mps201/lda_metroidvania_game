using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace lda.Core
{
    public class Camera
    {
        public Vector2 Position { get; private set; }
        private Vector2 _targetPosition;
        private readonly float _smoothSpeed = 0.08f; 

        private readonly int _levelWidth;
        private readonly int _levelHeight;
        private readonly int _viewportWidth;
        private readonly int _viewportHeight;

        private Vector2 _focusOffset = Vector2.Zero;

        public Camera(int levelWidth, int levelHeight, int viewportWidth, int viewportHeight)
        {
            _levelWidth = levelWidth;
            _levelHeight = levelHeight;
            _viewportWidth = viewportWidth;
            _viewportHeight = viewportHeight;
            Position = Vector2.Zero;
        }

        public void Update(Vector2 playerPosition, Vector2 focusOffset)
        {
            _focusOffset = focusOffset;

            _targetPosition = playerPosition - new Vector2(_viewportWidth / 2f, _viewportHeight / 2f) + _focusOffset;
            Position = Vector2.Lerp(Position, _targetPosition, _smoothSpeed);

            float minX = 0;
            float minY = 0;
            float maxX = _levelWidth - _viewportWidth;
            float maxY = _levelHeight - _viewportHeight;

            if (maxX < minX) maxX = minX;
            if (maxY < minY) maxY = minY;

            float clampedX = MathHelper.Clamp(Position.X, minX, maxX);
            float clampedY = MathHelper.Clamp(Position.Y, minY, maxY);
            Position = new Vector2(clampedX, clampedY);
        }

        public Matrix GetViewMatrix()
        {
            return Matrix.CreateTranslation(new Vector3(-Position, 0.0f));
        }
    }
}