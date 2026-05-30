using Microsoft.Xna.Framework.Input;

namespace lda.Core
{
    public class InputHandler
    {
        private KeyboardState _currentKeyState;
        private KeyboardState _previousKeyState;

        public void Update()
        {
            _previousKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();
        }

        public bool IsKeyDown(Keys key) => _currentKeyState.IsKeyDown(key);
        
        public bool IsKeyPressed(Keys key) => 
            _currentKeyState.IsKeyDown(key) && _previousKeyState.IsKeyUp(key);

        public bool IsKeyReleased(Keys key) => 
            _currentKeyState.IsKeyUp(key) && _previousKeyState.IsKeyDown(key);
    }
}