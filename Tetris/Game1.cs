using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Tetris
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        public int gameState = 0;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        TetrisGame tetrisGame;

        public int finalScore = 0;

        // Start screen
        Texture2D startScreen;
        string startScreenAssetName = @"Images\StartScreen";

        // Game over screen
        Texture2D gameOverScreen;
        string gameOverScreenAssetName = @"Images\GameOverScreen";

        SpriteFont scoreFont;
        string scoreFontAssetName = @"Fonts\NumberFont";
        Vector2 scorePosition = new Vector2(200, 242);

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferHeight = 500;
            _graphics.PreferredBackBufferWidth = 550;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Set up game component
            tetrisGame = new TetrisGame(this);
            Components.Add(tetrisGame);
            tetrisGame.Enabled = false;
            tetrisGame.Visible = false;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load textures
            startScreen = Content.Load<Texture2D>(startScreenAssetName);
            gameOverScreen = Content.Load<Texture2D>(gameOverScreenAssetName);

            // Fonts
            scoreFont = Content.Load<SpriteFont>(scoreFontAssetName);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (gameState)
            {
                case 0:
                    tetrisGame.Enabled = false;
                    tetrisGame.Visible = false;

                    if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Enter))
                        gameState = 1;

                    break;

                case 1:
                    tetrisGame.Enabled = true;
                    tetrisGame.Visible = true;
                    break;

                case 2:
                    tetrisGame.Enabled = false;
                    tetrisGame.Visible = false;

                    if (GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Enter))
                    {
                        tetrisGame.Initialize();
                        gameState = 1;
                    }

                    if (GamePad.GetState(PlayerIndex.One).Buttons.B == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                        this.Exit();

                    break;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            switch (gameState)
            {
                case 0:
                    _spriteBatch.Draw(startScreen, new Vector2(0, 0), Color.White);
                    break;

                case 1:
                    break;

                case 2:
                    _spriteBatch.Draw(gameOverScreen, new Vector2(0, 0), Color.White);
                    _spriteBatch.DrawString(scoreFont, finalScore.ToString(), scorePosition, Color.Black);
                    break;
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
