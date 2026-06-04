using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using System.Linq;
using lda.Core;
using lda.Entities;
using lda.Levels;
using lda.Controllers;
using lda.Views;
using lda.Views.UI;
using lda.Models;

namespace lda;

public class Game1 : Game
{
    private GameState _currentState = GameState.Title;
    private SpriteFont _font;

    private Dictionary<string, LevelConfig> _levelConfigs;
    private string _currentLevelId;
    private List<ExitZone> _activeExits;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    private InputHandler _inputHandler;
    private List<GameObject> _gameObjects;

    private PlayerModel _playerModel;
    private PlayerController _playerController;
    private PlayerView _playerView;

    private Camera _camera;
    private TileMap _tileMap;
    private Texture2D _checkpointInactiveTex;
    private Texture2D _checkpointActiveTex;
    private List<Checkpoint> _checkpoints;
    
    private Texture2D _playerTexture;
    private Texture2D _tileTexture;

    private List<EnemyModel> _enemyModels;
    private List<EnemyController> _enemyControllers;
    private List<EnemyView> _enemyViews;

    private HealthBarView _healthBar;
    private Texture2D _heartTexture;

    public Game1()
    {
        System.Console.Error.WriteLine("🔧 [Game1] Constructor called");
        
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = 800;
        _graphics.PreferredBackBufferHeight = 600;
        _graphics.IsFullScreen = false;
    }

    protected override void Initialize()
    {
        _graphics.ApplyChanges();
        _inputHandler = new InputHandler();
        _gameObjects = new List<GameObject>();

        SetupLevels();
        base.Initialize();
    }

    private void SetupLevels()
    {
        _levelConfigs = new Dictionary<string, LevelConfig>();
        _activeExits = new List<ExitZone>();

        //lvl1
        int[,] lvl1Data = new int[25, 33];
        for (int x = 0; x < 33; x++) lvl1Data[18, x] = 1; // Пол
        lvl1Data[14, 5] = 1; lvl1Data[14, 6] = 1; lvl1Data[14, 7] = 1;
        lvl1Data[10, 20] = 1; lvl1Data[10, 21] = 1; lvl1Data[10, 22] = 1; lvl1Data[10, 23] = 1;
        lvl1Data[16, 25] = 1; lvl1Data[16, 26] = 1; lvl1Data[16, 27] = 1;

        _levelConfigs["level_1"] = new LevelConfig
        {
            Id = "level_1",
            TileData = lvl1Data,
            PlayerSpawn = new Vector2(100, 400),
            EnemySpawns = { new Vector2(400, 500), new Vector2(600, 500) },
            Exits = {
                new ExitZone { Bounds = new Rectangle(1000, 480, 32, 96), TargetLevelId = "level_2", TargetSpawn = new Vector2(100, 500) }
            }
        };

        //lvl2
        int[,] lvl2Data = new int[25, 33];
        for (int x = 0; x < 33; x++) lvl2Data[20, x] = 1; // Пол ниже
        lvl2Data[16, 8] = 1; lvl2Data[16, 9] = 1; lvl2Data[16, 10] = 1;
        lvl2Data[12, 20] = 1; lvl2Data[12, 21] = 1; lvl2Data[12, 22] = 1;

        _levelConfigs["level_2"] = new LevelConfig
        {
            Id = "level_2",
            TileData = lvl2Data,
            PlayerSpawn = new Vector2(100, 500),
            EnemySpawns = { new Vector2(300, 550), new Vector2(700, 400) },
            Exits = {
                new ExitZone { Bounds = new Rectangle(50, 480, 32, 96), TargetLevelId = "level_1", TargetSpawn = new Vector2(900, 500) }
            }
        };
    }

    private void LoadLevel(string levelId)
    {
        if (!_levelConfigs.TryGetValue(levelId, out var config)) return;

        _currentLevelId = levelId;
        _activeExits = new List<ExitZone>(config.Exits);

        _tileMap = new TileMap(config.TileData, 32);
        _playerModel.SetWorldColliders(_tileMap.SolidTiles);

        _playerModel.RespawnPoint = config.PlayerSpawn;
        _playerModel.Reset();

        _enemyModels.Clear();
        _enemyControllers.Clear();
        _enemyViews.Clear();
        _checkpoints.Clear();

        foreach (var pos in config.EnemySpawns)
        {
            AddEnemy(pos);
        }

        var cp = new Checkpoint(config.PlayerSpawn + new Vector2(150, 0), _checkpointInactiveTex, _checkpointActiveTex);
        _checkpoints.Add(cp);

        _camera = new Camera(33 * 32, 25 * 32, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _playerTexture = new Texture2D(GraphicsDevice, 32, 32);
        _playerTexture.SetData(Enumerable.Repeat(Color.White, 32 * 32).ToArray());
        
        _tileTexture = new Texture2D(GraphicsDevice, 32, 32);
        _tileTexture.SetData(Enumerable.Repeat(Color.Green, 32 * 32).ToArray());
        
        _checkpointInactiveTex = new Texture2D(GraphicsDevice, 32, 32);
        _checkpointInactiveTex.SetData(Enumerable.Repeat(Color.White, 32 * 32).ToArray());
        
        _checkpointActiveTex = new Texture2D(GraphicsDevice, 32, 32);
        _checkpointActiveTex.SetData(Enumerable.Repeat(Color.White, 32 * 32).ToArray());
        
        _heartTexture = new Texture2D(GraphicsDevice, 1, 1);
        _heartTexture.SetData(new Color[] { Color.White });

        _playerModel = new PlayerModel(new Vector2(100, 400), new List<Rectangle>()); // Временные коллайдеры
        _playerController = new PlayerController(_playerModel, _inputHandler);
        _playerView = new PlayerView(_playerModel, _playerTexture);

        _enemyModels = new List<EnemyModel>();
        _enemyControllers = new List<EnemyController>();
        _enemyViews = new List<EnemyView>();
        _checkpoints = new List<Checkpoint>();

        _healthBar = new HealthBarView(_playerModel, _heartTexture);

        // try { _font = Content.Load<SpriteFont>("Fonts/Default"); } catch { }

        LoadLevel("level_1");
    }

    private void AddEnemy(Vector2 position)
    {
        var model = new EnemyModel(position, _tileMap.SolidTiles);
        var controller = new EnemyController(model, _playerModel, _tileMap.SolidTiles); 
        
        var view = new EnemyView(model, _playerTexture);

        _enemyModels.Add(model);
        _enemyControllers.Add(controller);
        _enemyViews.Add(view);
    }

    protected override void Update(GameTime gameTime)
    {
        _inputHandler.Update();

        if (_currentState == GameState.Title)
        {
            if (_inputHandler.IsKeyPressed(Keys.Enter) || _inputHandler.IsKeyPressed(Keys.Space))
            {
                _currentState = GameState.Playing;
                LoadLevel("level_1");
            }
            base.Update(gameTime);
            return;
        }

        if (_inputHandler.IsKeyPressed(Keys.Escape))
        {
            if (_currentState == GameState.Playing)
                _currentState = GameState.Paused;
            else if (_currentState == GameState.Paused)
                _currentState = GameState.Playing;
        }

        if (_currentState == GameState.Paused)
        {
            base.Update(gameTime);
            return;
        }

        _playerController.Update();
        _playerModel.Update(gameTime);

        if (_playerModel.Position.Y > 800 || !_playerModel.IsAlive)
        {
            _playerModel.Reset();
        }

        foreach (var obj in _gameObjects)
            obj.Update(gameTime);

        if (_playerModel.IsAlive)
        {
            _camera.Update(_playerModel.Position, GetCameraOffset());
        }

        for (int i = 0; i < _enemyModels.Count; i++)
        {
            if (!_enemyModels[i].IsAlive) continue;

            _enemyControllers[i].Update();
            _enemyModels[i].Update();

            if (_playerModel.AttackHitbox.HasValue && _playerModel.AttackHitbox.Value.Intersects(_enemyModels[i].Bounds))
            {
                _enemyModels[i].TakeDamage();
            }

            if (_enemyModels[i].Bounds.Intersects(_playerModel.Bounds))
            {
                _playerModel.TakeDamage();
                float dir = _playerModel.Position.X < _enemyModels[i].Position.X ? -1 : 1;
                _playerModel.SetVelocity(new Vector2(dir * 5, -4));
            }
        }

        foreach (var cp in _checkpoints)
        {
            cp.Update(_playerModel);
        }

        foreach (var exit in _activeExits)
        {
            if (_playerModel.Bounds.Intersects(exit.Bounds))
            {
                LoadLevel(exit.TargetLevelId);
                break;
            }
        }
        
        base.Update(gameTime);
    }

    private Vector2 GetCameraOffset()
    {
        Vector2 offset = Vector2.Zero;
        if (_inputHandler.IsKeyDown(Keys.W) || _inputHandler.IsKeyDown(Keys.Up))
            offset.Y -= 140f;
        else if (_inputHandler.IsKeyDown(Keys.S) || _inputHandler.IsKeyDown(Keys.Down))
            offset.Y += 140f;
        return offset;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        if (_currentState == GameState.Title)
        {
            _spriteBatch.Begin();
            DrawTitleScreen(_spriteBatch, gameTime);
            _spriteBatch.End();
            return;
        }

        _spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix());
        
        _tileMap.Draw(_spriteBatch, _tileTexture);
        _playerView.Draw(_spriteBatch, gameTime);

        foreach (var view in _enemyViews)
            view.Draw(_spriteBatch);

        foreach (var obj in _gameObjects)
            obj.Draw(_spriteBatch);

        foreach (var cp in _checkpoints)
            cp.Draw(_spriteBatch);

        foreach (var exit in _activeExits)
        {
            _spriteBatch.Draw(_tileTexture, exit.Bounds, Color.Cyan * 0.3f);
        }
        
        _spriteBatch.End();

        _spriteBatch.Begin();
        
        _healthBar.Draw(_spriteBatch);

        if (_currentState == GameState.Paused)
        {
            DrawPauseOverlay(_spriteBatch, gameTime);
        }
        
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }

    private void DrawTitleScreen(SpriteBatch sb, GameTime gameTime)
    {
        sb.Draw(_playerTexture, new Rectangle(0, 0, 800, 600), Color.Black);
        
        if (_font != null)
        {
            sb.DrawString(_font, "MY METROIDVANIA", new Vector2(180, 150), Color.White, 0, Vector2.Zero, 2f, SpriteEffects.None, 0);
            sb.DrawString(_font, "2D Platformer with MVC Architecture", new Vector2(120, 250), Color.LightGray, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
            
            string hint = "Press [ENTER] or [SPACE] to Start";

            Color hintColor = (int)(gameTime.TotalGameTime.TotalMilliseconds / 500) % 2 == 0 ? Color.Yellow : Color.White;
            sb.DrawString(_font, hint, new Vector2(200, 400), hintColor, 0, Vector2.Zero, 1.2f, SpriteEffects.None, 0);
            
            sb.DrawString(_font, "Controls:", new Vector2(300, 480), Color.Gray, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
            sb.DrawString(_font, "WASD - Move | SPACE - Jump | J - Attack | ESC - Pause", new Vector2(100, 510), Color.Gray, 0, Vector2.Zero, 0.9f, SpriteEffects.None, 0);
        }
    }

    private void DrawPauseOverlay(SpriteBatch sb, GameTime gameTime)
    {
        sb.Draw(_playerTexture, new Rectangle(0, 0, 800, 600), Color.Black * 0.7f);
        
        if (_font != null)
        {
            sb.DrawString(_font, "PAUSED", new Vector2(320, 250), Color.White, 0, Vector2.Zero, 2f, SpriteEffects.None, 0);
            sb.DrawString(_font, "Press [ESC] to Resume", new Vector2(260, 320), Color.Yellow, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
        }
    }
}