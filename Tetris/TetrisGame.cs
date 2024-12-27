/*
 * Controls the game
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    /// <summary>
    /// This is a game component that implements IUpdateable
    /// </summary>
    public class TetrisGame : DrawableGameComponent
    {
        SpriteBatch spriteBatch;

        bool gameOver;

        //private AudioEngine audioEngine;
        //private WaveBank waveBank;
        //private SoundBank soundBank;

        int score; // Game score
        // Points awarded for line clears
        const int singleScore = 100;
        const int doubleScore = 300;
        const int tripleScore = 600;
        const int tetrisScore = 1000;

        int startGameDelay = 1000;
        int gameDelay; // Milliseconds between drops
        int gameDelayTimer;
        int fastGameDelay = 50; // Time between drops when user holds down
        bool downHeld; // Flags whether down is held on the d-pad

        int leftRightDelay = 50; // Milliseconds between movements when left/right held down
        int leftRightDelayTimer = 0;
        int initialLeftRightDelay = 250; // Time until 'key held' state is registered
        int initialLeftRightDelayTimer = 0;

        Cell[,] board = new Cell[20, 10];
        string boardAssetName = @"Images\Board";
        Texture2D boardTexture;
        Vector2 boardOrigin = new Vector2(20, 20); // Origin of board sprite
        Vector2 boardOffset = new Vector2(5, 5); // Offset into playable area
        int cellSize = 25; // Size of a cell on board

        bool gameStarted;

        // Number box textures
        string numberBoxAssetName = @"Images\NumberBox";
        Texture2D levelNumberBoxTexture;
        Texture2D scoreNumberBoxTexture;
        Texture2D linesNumberBoxTexture;

        // Positions of number displays
        Vector2 levelOrigin = new Vector2(320, 215);
        Vector2 scoreOrigin = new Vector2(320, 315);
        Vector2 linesOrigin = new Vector2(320, 415);

        Vector2 levelNumberOrigin;
        Vector2 scoreNumberOrigin;
        Vector2 linesNumberOrigin;

        Vector2 levelHeadingOrigin;
        Vector2 scoreHeadingOrigin;
        Vector2 linesHeadingOrigin;

        // Single cell textures
        string blueAssetName = @"Images\Blue";
        Texture2D blueTexture;
        string cyanAssetName = @"Images\Cyan";
        Texture2D cyanTexture;
        string greenAssetName = @"Images\Green";
        Texture2D greenTexture;
        string orangeAssetName = @"Images\Orange";
        Texture2D orangeTexture;
        string purpleAssetName = @"Images\Purple";
        Texture2D purpleTexture;
        string redAssetName = @"Images\Red";
        Texture2D redTexture;
        string yellowAssetName = @"Images\Yellow";
        Texture2D yellowTexture;

        // Next piece textures
        string iNextAssetName = @"Images\INext";
        Texture2D iNextTexture;
        string jNextAssetName = @"Images\JNext";
        Texture2D jNextTexture;
        string lNextAssetName = @"Images\LNext";
        Texture2D lNextTexture;
        string oNextAssetName = @"Images\ONext";
        Texture2D oNextTexture;
        string sNextAssetName = @"Images\SNext";
        Texture2D sNextTexture;
        string tNextAssetName = @"Images\TNext";
        Texture2D tNextTexture;
        string zNextAssetName = @"Images\ZNext";
        Texture2D zNextTexture;

        // Game fonts
        string headingFontAssetName = @"Fonts\HeadingFont";
        SpriteFont headingFontSprite;
        string numberFontAssetName = @"Fonts\NumberFont";
        SpriteFont numberFontSprite;

        PieceType activePieceType; // Type of the active piece
        Texture2D activeTexture; // Texture of the active piece
        int activeRotateState; // Rotation state of the active piece

        bool nextPieceRequired; // Indicates if a new piece needs to be spawned

        Point[] activeCoordinates = new Point[4]; // Coordinates of active piece

        PieceType nextPieceType;
        Texture2D nextPieceTexture; // Texture for displaying next piece to user
        Vector2 nextPieceDisplayOrigin = new Vector2(350, 50); // Position of next piece display
        Vector2 nextTextPosition = new Vector2(350, 25); // Position of "Next" text

        GamePadState oldGamePadState;
        KeyboardState oldKeyboardState;

        int level; // Current level
        int totalLinesCleared = 0; // Total number of lines cleared
        int levelLinesCleared = 0; // Count of number of completed lines for current level
        const int linesForNextLevel = 10; // Number of lines required to advance one level
        const int nextLevelDelayDecrease = 100; // Miilisecond reduction in drop time between consecutive levels

        Random random = new Random();

        enum PieceType { I, J, L, O, S, T, Z };

        public TetrisGame(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // Initialise variables
            gameOver = false;
            score = 0;
            gameDelay = startGameDelay;
            downHeld = false;
            gameStarted = false;
            nextPieceRequired = true;
            level = 1;
            totalLinesCleared = 0;
            levelLinesCleared = 0;
            gameDelayTimer = gameDelay; // Ensure first update is run immediately

            // Initialise cells
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    board[i, j] = new Cell(i, j, cellSize, boardOrigin + boardOffset);
                }
            }

            // Initialise active coordinates
            for (int i = 0; i < activeCoordinates.Length; i++)
            {
                activeCoordinates[i] = new Point(0, 0);
            }

            // Initialise coordinates of text
            levelNumberOrigin = new Vector2(levelOrigin.X + 30, levelOrigin.Y + 10);
            scoreNumberOrigin = new Vector2(scoreOrigin.X + 30, scoreOrigin.Y + 10);
            linesNumberOrigin = new Vector2(linesOrigin.X + 30, linesOrigin.Y + 10);

            levelHeadingOrigin = new Vector2(levelOrigin.X, levelOrigin.Y - 25);
            scoreHeadingOrigin = new Vector2(scoreOrigin.X, scoreOrigin.Y - 25);
            linesHeadingOrigin = new Vector2(linesOrigin.X, linesOrigin.Y - 25);

            base.Initialize();

            // Select first piece type. Requires the next piece textures to have been loaded
            SelectNextPiece();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            // Load audio
            //audioEngine = new AudioEngine(@"Content\Sound\GameAudio.xgs");
            //waveBank = new WaveBank(audioEngine, @"Content\Sound\Wave Bank.xwb");
            //soundBank = new SoundBank(audioEngine, @"Content\Sound\Sound Bank.xsb");

            // Load textures
            boardTexture = Game.Content.Load<Texture2D>(boardAssetName);

            levelNumberBoxTexture = Game.Content.Load<Texture2D>(numberBoxAssetName);
            scoreNumberBoxTexture = Game.Content.Load<Texture2D>(numberBoxAssetName);
            linesNumberBoxTexture = Game.Content.Load<Texture2D>(numberBoxAssetName);

            blueTexture = Game.Content.Load<Texture2D>(blueAssetName);
            cyanTexture = Game.Content.Load<Texture2D>(cyanAssetName);
            greenTexture = Game.Content.Load<Texture2D>(greenAssetName);
            orangeTexture = Game.Content.Load<Texture2D>(orangeAssetName);
            purpleTexture = Game.Content.Load<Texture2D>(purpleAssetName);
            redTexture = Game.Content.Load<Texture2D>(redAssetName);
            yellowTexture = Game.Content.Load<Texture2D>(yellowAssetName);

            iNextTexture = Game.Content.Load<Texture2D>(iNextAssetName);
            jNextTexture = Game.Content.Load<Texture2D>(jNextAssetName);
            lNextTexture = Game.Content.Load<Texture2D>(lNextAssetName);
            oNextTexture = Game.Content.Load<Texture2D>(oNextAssetName);
            sNextTexture = Game.Content.Load<Texture2D>(sNextAssetName);
            tNextTexture = Game.Content.Load<Texture2D>(tNextAssetName);
            zNextTexture = Game.Content.Load<Texture2D>(zNextAssetName);

            headingFontSprite = Game.Content.Load<SpriteFont>(headingFontAssetName);
            numberFontSprite = Game.Content.Load<SpriteFont>(numberFontAssetName);

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            ProcessUserInput(gameTime);
            //ProcessUserInputKeyboard(gameTime);

            gameDelayTimer += gameTime.ElapsedGameTime.Milliseconds;

            if ((!downHeld && gameDelayTimer >= gameDelay) || (downHeld && gameDelayTimer >= fastGameDelay)) // Check if user is holding down
            {
                if (GameOver())
                {
                    //soundBank.PlayCue("endSound");
                    ((Game1)Game).finalScore = score; // Send final score
                    ((Game1)Game).gameState = 2;
                }

                // Spawn a new piece if required
                if (nextPieceRequired)
                {
                    if (gameStarted)
                    {
                        DeactivatePiece(); // No need to deactivate before any pieces have spawned
                    }
                    else
                    {
                        gameStarted = true;
                        //soundBank.PlayCue("startSound"); // Play start sound
                    }
                    SpawnPiece();
                    nextPieceRequired = false;
                    gameDelayTimer = 0;
                }
                else if (CanMoveDown())
                {
                    DropPiece();
                    gameDelayTimer = 0;
                }
                else
                {
                    nextPieceRequired = true;
                    // Remove any complete lines
                    RemoveLines();

                    // Increase level if required
                    while (levelLinesCleared >= linesForNextLevel)
                    {
                        levelLinesCleared -= linesForNextLevel;
                        level++;

                        if (gameDelay - nextLevelDelayDecrease >= fastGameDelay) // Don't allow drop below minimum delay
                        {
                            gameDelay -= nextLevelDelayDecrease;
                        }
                        else
                        {
                            gameDelay = fastGameDelay;
                        }
                    }
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            // Draw board
            spriteBatch.Draw(boardTexture, boardOrigin, Color.White);

            // Draw text
            spriteBatch.DrawString(headingFontSprite, "Next", nextTextPosition, Color.Black);

            // Draw next piece display
            spriteBatch.Draw(nextPieceTexture, nextPieceDisplayOrigin, Color.White);

            // Draw score, level and lines
            spriteBatch.Draw(levelNumberBoxTexture, levelOrigin, Color.White);
            spriteBatch.Draw(scoreNumberBoxTexture, scoreOrigin, Color.White);
            spriteBatch.Draw(linesNumberBoxTexture, linesOrigin, Color.White);

            spriteBatch.DrawString(numberFontSprite, level.ToString(), levelNumberOrigin, Color.Black);
            spriteBatch.DrawString(numberFontSprite, score.ToString(), scoreNumberOrigin, Color.Black);
            spriteBatch.DrawString(numberFontSprite, totalLinesCleared.ToString(), linesNumberOrigin, Color.Black);

            spriteBatch.DrawString(headingFontSprite, "Level", levelHeadingOrigin, Color.Black);
            spriteBatch.DrawString(headingFontSprite, "Score", scoreHeadingOrigin, Color.Black);
            spriteBatch.DrawString(headingFontSprite, "Lines Cleared", linesHeadingOrigin, Color.Black);

            // Draw pieces
            for (int i = 2; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    board[i, j].Draw(gameTime, spriteBatch);
                }
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected void SpawnPiece()
        {
            // Coordinate definitions for starting positions
            int[] Ix = { 2, 2, 2, 2 }; int[] Iy = { 3, 4, 5, 6 };
            int[] Jx = { 2, 1, 1, 1 }; int[] Jy = { 5, 3, 4, 5 };
            int[] Lx = { 2, 1, 1, 1 }; int[] Ly = { 3, 3, 4, 5 };
            int[] Ox = { 2, 2, 1, 1 }; int[] Oy = { 4, 5, 4, 5 };
            int[] Sx = { 2, 2, 1, 1 }; int[] Sy = { 3, 4, 4, 5 };
            int[] Tx = { 2, 1, 1, 1 }; int[] Ty = { 4, 3, 4, 5 };
            int[] Zx = { 2, 2, 1, 1 }; int[] Zy = { 4, 5, 3, 4 };

            // Replace current piece
            activePieceType = nextPieceType;

            activeRotateState = 0;

            switch (activePieceType)
            {
                case PieceType.I:
                    activeTexture = cyanTexture;

                    gameOver = !CheckAndActivateCells(Iy, Ix); // If piece cannot be spawned, game is over

                    //ActivateCell(2, 3, 0);
                    //ActivateCell(2, 4, 1);
                    //ActivateCell(2, 5, 2);
                    //ActivateCell(2, 6, 3);
                    break;

                case PieceType.J:
                    activeTexture = blueTexture;

                    gameOver = !CheckAndActivateCells(Jy, Jx);

                    //ActivateCell(2, 5, 0);
                    //ActivateCell(1, 3, 1);
                    //ActivateCell(1, 4, 2);
                    //ActivateCell(1, 5, 3);
                    break;

                case PieceType.L:
                    activeTexture = orangeTexture;

                    gameOver = !CheckAndActivateCells(Ly, Lx);

                    //ActivateCell(2, 3, 0);
                    //ActivateCell(1, 3, 1);
                    //ActivateCell(1, 4, 2);
                    //ActivateCell(1, 5, 3);
                    break;

                case PieceType.O:
                    activeTexture = yellowTexture;

                    gameOver = !CheckAndActivateCells(Oy, Ox);

                    //ActivateCell(2, 4, 0);
                    //ActivateCell(2, 5, 1);
                    //ActivateCell(1, 4, 2);
                    //ActivateCell(1, 5, 3);
                    break;

                case PieceType.S:
                    activeTexture = greenTexture;

                    gameOver = !CheckAndActivateCells(Sy, Sx);

                    //ActivateCell(2, 3, 0);
                    //ActivateCell(2, 4, 1);
                    //ActivateCell(1, 4, 2);
                    //ActivateCell(1, 5, 3);
                    break;

                case PieceType.T:
                    activeTexture = purpleTexture;

                    gameOver = !CheckAndActivateCells(Ty, Tx);

                    //ActivateCell(2, 4, 0);
                    //ActivateCell(1, 3, 1);
                    //ActivateCell(1, 4, 2);
                    //ActivateCell(1, 5, 3);
                    break;

                case PieceType.Z:
                    activeTexture = redTexture;

                    gameOver = !CheckAndActivateCells(Zy, Zx);

                    //ActivateCell(2, 4, 0);
                    //ActivateCell(2, 5, 1);
                    //ActivateCell(1, 3, 2);
                    //ActivateCell(1, 4, 3);
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }

            // Choose a random next piece
            SelectNextPiece();
        }

        // Check if active piece can move down
        protected bool CanMoveDown()
        {
            Cell tempCell;

            for (int i = 0; i < 4; i++)
            {
                // Check if piece has reached bottom of board
                if (activeCoordinates[i].Y == (board.GetLength(0) - 1))
                    return false;

                // Check if piece is blocked by an inactive piece
                tempCell = board[activeCoordinates[i].Y + 1, activeCoordinates[i].X];

                if (tempCell.IsOccupied && !tempCell.IsActive)
                    return false;
            }

            return true;
        }

        // Drop the active piece one square
        protected void DropPiece()
        {
            Cell currentActive;
            Cell newActive;

            for (int i = 0; i < 4; i++)
            {
                currentActive = board[activeCoordinates[i].Y, activeCoordinates[i].X];

                currentActive.IsActive = false;
                currentActive.IsOccupied = false;
            }

            for (int i = 0; i < 4; i++)
            {
                activeCoordinates[i].Y = activeCoordinates[i].Y + 1;

                newActive = board[activeCoordinates[i].Y, activeCoordinates[i].X];

                newActive.IsActive = true;
                newActive.IsOccupied = true;
                newActive.Texture = activeTexture;
            }


        }

        // Deactivate the active piece
        protected void DeactivatePiece()
        {
            for (int i = 0; i < 4; i++)
            {
                board[activeCoordinates[i].Y, activeCoordinates[i].X].IsActive = false;
            }

            // Play sound
            //soundBank.PlayCue("fixSound");
        }

        // Check if game is over
        protected bool GameOver()
        {
            return gameOver;

            /*
            // Check if a piece has settled above the game board
            for (int i = 0; i <= 2; i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (!board[i, j].IsActive && board[i, j].IsOccupied)
                    {
                        return true;
                    }
                }
            }

            return false;*/
        }

        // User movement of active piece
        protected void UserMoveLeft()
        {
            Cell currentActive;
            Cell newActive;

            for (int i = 0; i < 4; i++)
            {
                currentActive = board[activeCoordinates[i].Y, activeCoordinates[i].X];

                currentActive.IsActive = false;
                currentActive.IsOccupied = false;
            }

            for (int i = 0; i < 4; i++)
            {
                activeCoordinates[i].X = activeCoordinates[i].X - 1;

                newActive = board[activeCoordinates[i].Y, activeCoordinates[i].X];

                newActive.IsActive = true;
                newActive.IsOccupied = true;
                newActive.Texture = activeTexture;
            }
        }

        // User movement of active piece
        protected void UserMoveRight()
        {
            Cell currentActive;
            Cell newActive;

            for (int i = 0; i < 4; i++)
            {
                currentActive = board[activeCoordinates[i].Y, activeCoordinates[i].X];

                currentActive.IsActive = false;
                currentActive.IsOccupied = false;
            }

            for (int i = 0; i < 4; i++)
            {
                activeCoordinates[i].X = activeCoordinates[i].X + 1;

                newActive = board[activeCoordinates[i].Y, activeCoordinates[i].X];

                newActive.IsActive = true;
                newActive.IsOccupied = true;
                newActive.Texture = activeTexture;
            }
        }

        // Check if active piece can move left
        protected bool CanMoveLeft()
        {
            Cell tempCell;

            for (int i = 0; i < 4; i++)
            {
                // Check if piece has reached left wall
                if (activeCoordinates[i].X == 0)
                    return false;

                // Check if piece is blocked by an inactive piece
                tempCell = board[activeCoordinates[i].Y, activeCoordinates[i].X - 1];

                if (tempCell.IsOccupied && !tempCell.IsActive)
                    return false;
            }

            return true;
        }

        // Check if active piece can move left
        protected bool CanMoveRight()
        {
            Cell tempCell;

            for (int i = 0; i < 4; i++)
            {
                // Check if piece has reached right wall
                if (activeCoordinates[i].X == board.GetLength(1) - 1)
                    return false;

                // Check if piece is blocked by an inactive piece
                tempCell = board[activeCoordinates[i].Y, activeCoordinates[i].X + 1];

                if (tempCell.IsOccupied && !tempCell.IsActive)
                    return false;
            }

            return true;
        }

        // Processes user input
        void ProcessUserInput(GameTime gameTime)
        {
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            if (!nextPieceRequired) // Stop user movement code when between piece appearances
            {
                if (gamePadState.DPad.Left == ButtonState.Pressed && CanMoveLeft())
                {
                    if (oldGamePadState.DPad.Left == ButtonState.Released)
                    {
                        UserMoveLeft();
                        initialLeftRightDelayTimer = 0;
                    }
                    else
                    {
                        initialLeftRightDelayTimer += gameTime.ElapsedGameTime.Milliseconds;

                        if (initialLeftRightDelayTimer >= initialLeftRightDelay) // Check if initial delay is expired
                        {
                            leftRightDelayTimer += gameTime.ElapsedGameTime.Milliseconds;

                            // Holding left movement
                            if (leftRightDelayTimer >= leftRightDelay)
                            {
                                leftRightDelayTimer = 0;
                                UserMoveLeft();
                            }
                        }
                    }
                }
                if (gamePadState.DPad.Right == ButtonState.Pressed && CanMoveRight())
                {
                    if (oldGamePadState.DPad.Right == ButtonState.Released)
                    {
                        UserMoveRight();
                        initialLeftRightDelayTimer = 0;
                    }
                    else
                    {
                        initialLeftRightDelayTimer += gameTime.ElapsedGameTime.Milliseconds;

                        if (initialLeftRightDelayTimer >= initialLeftRightDelay) // Check if initial delay is expired
                        {
                            leftRightDelayTimer += gameTime.ElapsedGameTime.Milliseconds;

                            // Holding right movement
                            if (leftRightDelayTimer >= leftRightDelay)
                            {
                                leftRightDelayTimer = 0;
                                UserMoveRight();
                            }
                        }
                    }
                }
                // Set flag for user holding down
                if (gamePadState.DPad.Down == ButtonState.Pressed)
                {
                    downHeld = true;
                }
                else
                {
                    downHeld = false;
                }

                // Rotations
                if (gamePadState.Buttons.B == ButtonState.Pressed)
                {
                    if (oldGamePadState.Buttons.B == ButtonState.Released)
                    {
                        RotatePiece(activePieceType);

                        // Play sound
                        //soundBank.PlayCue("rotateSound");
                    }
                }

                if (gamePadState.Buttons.X == ButtonState.Pressed)
                {
                    if (oldGamePadState.Buttons.X == ButtonState.Released)
                    {
                        // Perform 3 right rotations to simulate a left rotation
                        RotatePiece(activePieceType);
                        RotatePiece(activePieceType);
                        RotatePiece(activePieceType);

                        // Play sound
                        //soundBank.PlayCue("rotateSound");
                    }
                }

                oldGamePadState = gamePadState;
            }
        }

        // Processes user input (Keyboard version)
        void ProcessUserInputKeyboard(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            if (!nextPieceRequired) // Stop user movement code when between piece appearances
            {
                if (keyboardState.IsKeyDown(Keys.Left) && CanMoveLeft())
                {
                    if (!oldKeyboardState.IsKeyDown(Keys.Left))
                    {
                        UserMoveLeft();
                        initialLeftRightDelayTimer = 0;
                    }
                    else
                    {
                        initialLeftRightDelayTimer += gameTime.ElapsedGameTime.Milliseconds;

                        if (initialLeftRightDelayTimer >= initialLeftRightDelay) // Check if initial delay is expired
                        {
                            leftRightDelayTimer += gameTime.ElapsedGameTime.Milliseconds;

                            // Holding left movement
                            if (leftRightDelayTimer >= leftRightDelay)
                            {
                                leftRightDelayTimer = 0;
                                UserMoveLeft();
                            }
                        }
                    }
                }
                if (keyboardState.IsKeyDown(Keys.Right) && CanMoveRight())
                {
                    if (!oldKeyboardState.IsKeyDown(Keys.Right))
                    {
                        UserMoveRight();
                        initialLeftRightDelayTimer = 0;
                    }
                    else
                    {
                        initialLeftRightDelayTimer += gameTime.ElapsedGameTime.Milliseconds;

                        if (initialLeftRightDelayTimer >= initialLeftRightDelay) // Check if initial delay is expired
                        {
                            leftRightDelayTimer += gameTime.ElapsedGameTime.Milliseconds;

                            // Holding right movement
                            if (leftRightDelayTimer >= leftRightDelay)
                            {
                                leftRightDelayTimer = 0;
                                UserMoveRight();
                            }
                        }
                    }
                }
                // Set flag for user holding down
                if (keyboardState.IsKeyDown(Keys.Down))
                {
                    downHeld = true;
                }
                else
                {
                    downHeld = false;
                }

                // Rotations
                if (keyboardState.IsKeyDown(Keys.X))
                {
                    if (!oldKeyboardState.IsKeyDown(Keys.X))
                    {
                        RotatePiece(activePieceType);
                    }
                }

                if (keyboardState.IsKeyDown(Keys.Z))
                {
                    if (!oldKeyboardState.IsKeyDown(Keys.Z))
                    {
                        // Perform 3 right rotations to simulate a left rotation
                        RotatePiece(activePieceType);
                        RotatePiece(activePieceType);
                        RotatePiece(activePieceType);
                    }
                }

                oldKeyboardState = keyboardState;
            }
        }

        void RemoveLines()
        {
            int[] completeRowIndices = new int[4];
            int completeRowsCounter = 0;
            bool rowComplete;

            // Identify complete rows
            for (int i = board.GetLength(0) - 1; i >= 1; i--)
            {
                rowComplete = true;
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (!board[i, j].IsOccupied)
                    {
                        rowComplete = false;
                        break;
                    }
                }

                if (rowComplete)
                {
                    completeRowIndices[completeRowsCounter] = i;
                    completeRowsCounter++;
                }
            }

            // Remove complete rows and drop higher pieces
            for (int i = 0; i < completeRowsCounter; i++)
            {
                // Remove row
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    board[completeRowIndices[i] + i, j].IsOccupied = false;
                }

                // Drop pieces down
                for (int k = completeRowIndices[i] + i; k >= 1; k--)
                {
                    for (int j = 0; j < board.GetLength(1); j++)
                    {
                        board[k, j].IsOccupied = board[k - 1, j].IsOccupied;
                        board[k, j].Texture = board[k - 1, j].Texture;
                    }
                }

                // Play sound
                //soundBank.PlayCue("clearSound");
            }

            // Update lines cleared counters and score
            totalLinesCleared += completeRowsCounter;
            levelLinesCleared += completeRowsCounter;

            switch (completeRowsCounter)
            {
                case 1:
                    score += singleScore;
                    break;

                case 2:
                    score += doubleScore;
                    break;

                case 3:
                    score += tripleScore;
                    break;

                case 4:
                    score += tetrisScore;
                    break;
            }
        }

        // Activates a single cell and stores coordinates in the activeCoordinates array in index activeIndex.
        void ActivateCell(int y, int x, int activeIndex)
        {
            board[y, x].IsActive = true;
            board[y, x].IsOccupied = true;
            board[y, x].Texture = activeTexture;

            activeCoordinates[activeIndex].Y = y;
            activeCoordinates[activeIndex].X = x;
        }

        // Activates cells after checking the spaces are free. Returns true if successful, false otherwise.
        bool CheckAndActivateCells(int[] x, int[] y)
        {
            for (int i = 0; i < 4; i++)
            {
                if (board[y[i], x[i]].IsOccupied)
                    return false;
            }

            for (int i = 0; i < 4; i++)
            {
                ActivateCell(y[i], x[i], i);
            }

            return true;
        }

        // Removes a single cell
        void RemoveCell(int y, int x)
        {
            board[y, x].IsActive = false;
            board[y, x].IsOccupied = false;
        }

        // Moves an active cell (CURRENTLY UNUSED)
        /*
        void MoveCell(int y, int x, int activeIndex)
        {
            RemoveCell(activeCoordinates[activeIndex].Y, activeCoordinates[activeIndex].X);
            ActivateCell(y, x, activeIndex);
        }
        */
        // Checks for valid coordinates
        bool ValidCoordinates(Point[] point)
        {
            for (int i = 0; i < point.Length; i++)
            {
                // Check coordinates are within bounds
                if (point[i].Y < 0 || point[i].Y >= board.GetLength(0))
                    return false;

                if (point[i].X < 0 || point[i].X >= board.GetLength(1))
                    return false;

                // Check for collisions with occupied, inactive cells
                if (!board[point[i].Y, point[i].X].IsActive && board[point[i].Y, point[i].X].IsOccupied)
                    return false;
            }

            return true;
        }

        // Rotate piece helper
        void RotatePieceCase(int caseNum, Point[] temp)
        {
            if (ValidCoordinates(temp))
            {
                for (int i = 0; i < temp.Length; i++)
                {
                    RemoveCell(activeCoordinates[i].Y, activeCoordinates[i].X);
                }
                for (int i = 0; i < temp.Length; i++)
                {
                    ActivateCell(temp[i].Y, temp[i].X, i);
                }
                activeRotateState = (caseNum + 1) % 4;
            }
        }

        // Rotate a piece to the right
        void RotatePiece(PieceType pieceType)
        {
            Point[] temp = new Point[4];

            switch (pieceType)
            {
                case PieceType.I:
                    switch (activeRotateState)
                    {
                        case 0:
                        case 2:
                            temp[0] = new Point(activeCoordinates[0].X + 1, activeCoordinates[0].Y - 1);
                            temp[1] = new Point(activeCoordinates[1].X, activeCoordinates[1].Y);
                            temp[2] = new Point(activeCoordinates[2].X - 1, activeCoordinates[2].Y + 1);
                            temp[3] = new Point(activeCoordinates[3].X - 2, activeCoordinates[3].Y + 2);

                            RotatePieceCase(activeRotateState, temp);
                            break;

                        case 1:
                        case 3:
                            temp[0] = new Point(activeCoordinates[0].X - 1, activeCoordinates[0].Y + 1);
                            temp[1] = new Point(activeCoordinates[1].X, activeCoordinates[1].Y);
                            temp[2] = new Point(activeCoordinates[2].X + 1, activeCoordinates[2].Y - 1);
                            temp[3] = new Point(activeCoordinates[3].X + 2, activeCoordinates[3].Y - 2);

                            RotatePieceCase(activeRotateState, temp);
                            break;
                    }
                    break;

                case PieceType.J:
                    switch (activeRotateState)
                    {
                        case 0:
                            temp[0] = new Point(activeCoordinates[0].X - 2, activeCoordinates[0].Y);
                            temp[1] = new Point(activeCoordinates[1].X + 1, activeCoordinates[1].Y - 1);
                            temp[2] = new Point(activeCoordinates[2].X, activeCoordinates[2].Y);
                            temp[3] = new Point(activeCoordinates[3].X - 1, activeCoordinates[3].Y + 1);

                            RotatePieceCase(activeRotateState, temp);
                            break;

                        case 1:
                            temp[0] = new Point(activeCoordinates[0].X, activeCoordinates[0].Y - 2);
                            temp[1] = new Point(activeCoordinates[1].X + 1, activeCoordinates[1].Y + 1);
                            temp[2] = new Point(activeCoordinates[2].X, activeCoordinates[2].Y);
                            temp[3] = new Point(activeCoordinates[3].X - 1, activeCoordinates[3].Y - 1);

                            RotatePieceCase(activeRotateState, temp);
                            break;

                        case 2:
                            temp[0] = new Point(activeCoordinates[0].X + 2, activeCoordinates[0].Y);
                            temp[1] = new Point(activeCoordinates[1].X - 1, activeCoordinates[1].Y + 1);
                            temp[2] = new Point(activeCoordinates[2].X, activeCoordinates[2].Y);
                            temp[3] = new Point(activeCoordinates[3].X + 1, activeCoordinates[3].Y - 1);

                            RotatePieceCase(activeRotateState, temp);
                            break;

                        case 3:
                            temp[0] = new Point(activeCoordinates[0].X, activeCoordinates[0].Y + 2);
                            temp[1] = new Point(activeCoordinates[1].X - 1, activeCoordinates[1].Y - 1);
                            temp[2] = new Point(activeCoordinates[2].X, activeCoordinates[2].Y);
                            temp[3] = new Point(activeCoordinates[3].X + 1, activeCoordinates[3].Y + 1);

                            RotatePieceCase(activeRotateState, temp);
                            break;
                    }
                    break;

                case PieceType.L:
                    switch (activeRotateState)
                    {
                        case 0:
                            temp[0] = new Point(activeCoordinates[0].X, activeCoordinates[0].Y - 2);
                            temp[1] = new Point(activeCoordinates[1].X + 1, activeCoordinates[1].Y - 1);
                            temp[2] = new Point(activeCoordinates[2].X, activeCoordinates[2].Y);
                            temp[3] = new Point(activeCoordinates[3].X - 1, activeCoordinates[3].Y + 1);

                            RotatePieceCase(activeRotateState, temp);
                            break;

                        case 1:
                            temp[0] = new Point(activeCoordinates[0].X + 2, activeCoordinates[0].Y);
                            temp[1] = new Point(activeCoordinates[1].X + 1, activeCoordinates[1].Y + 1);
                            temp[2] = new Point(activeCoordinates[2].X, activeCoordinates[2].Y);
                            temp[3] = new Point(activeCoordinates[3].X - 1, activeCoordinates[3].Y - 1);

                            RotatePieceCase(activeRotateState, temp);
                            break;

                        case 2:
                            temp[0] = new Point(activeCoordinates[0].X, activeCoordinates[0].Y + 2);
                            temp[1] = new Point(activeCoordinates[1].X - 1, activeCoordinates[1].Y + 1);
                            temp[2] = new Point(activeCoordinates[2].X, activeCoordinates[2].Y);
                            temp[3] = new Point(activeCoordinates[3].X + 1, activeCoordinates[3].Y - 1);

                            RotatePieceCase(activeRotateState, temp);
                            break;

                        case 3:
                            temp[0] = new Point(activeCoordinates[0].X - 2, activeCoordinates[0].Y);
                            temp[1] = new Point(activeCoordinates[1].X - 1, activeCoordinates[1].Y - 1);
                            temp[2] = new Point(activeCoordinates[2].X, activeCoordinates[2].Y);
                            temp[3] = new Point(activeCoordinates[3].X + 1, activeCoordinates[3].Y + 1);

                            RotatePieceCase(activeRotateState, temp);
                            break;
                    }
                    break;

                case PieceType.O:
                    switch (activeRotateState)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                            temp[0] = new Point(activeCoordinates[0].X, activeCoordinates[0].Y);
                            temp[1] = new Point(activeCoordinates[1].X, activeCoordinates[1].Y);
                            temp[2] = new Point(activeCoordinates[2].X, activeCoordinates[2].Y);
                            temp[3] = new Point(activeCoordinates[3].X, activeCoordinates[3].Y);

                            RotatePieceCase(activeRotateState, temp);
                            break;
                    }
                    break;

                case PieceType.S:
                    switch (activeRotateState)
                    {
                        case 0:
                        case 2:
                            temp[0] = new Point(activeCoordinates[0].X + 1, activeCoordinates[0].Y - 2);
                            temp[1] = new Point(activeCoordinates[1].X, activeCoordinates[1].Y - 1);
                            temp[2] = new Point(activeCoordinates[2].X + 1, activeCoordinates[2].Y);
                            temp[3] = new Point(activeCoordinates[3].X, activeCoordinates[3].Y + 1);

                            RotatePieceCase(activeRotateState, temp);
                            break;

                        case 1:
                        case 3:
                            temp[0] = new Point(activeCoordinates[0].X - 1, activeCoordinates[0].Y + 2);
                            temp[1] = new Point(activeCoordinates[1].X, activeCoordinates[1].Y + 1);
                            temp[2] = new Point(activeCoordinates[2].X - 1, activeCoordinates[2].Y);
                            temp[3] = new Point(activeCoordinates[3].X, activeCoordinates[3].Y - 1);

                            RotatePieceCase(activeRotateState, temp);
                            break;
                    }
                    break;

                case PieceType.T:
                    switch (activeRotateState)
                    {
                        case 0:
                            temp[0] = new Point(activeCoordinates[0].X - 1, activeCoordinates[0].Y - 1);
                            temp[1] = new Point(activeCoordinates[1].X + 1, activeCoordinates[1].Y - 1);
                            temp[2] = new Point(activeCoordinates[2].X, activeCoordinates[2].Y);
                            temp[3] = new Point(activeCoordinates[3].X - 1, activeCoordinates[3].Y + 1);

                            RotatePieceCase(activeRotateState, temp);
                            break;

                        case 1:
                            temp[0] = new Point(activeCoordinates[0].X + 1, activeCoordinates[0].Y - 1);
                            temp[1] = new Point(activeCoordinates[1].X + 1, activeCoordinates[1].Y + 1);
                            temp[2] = new Point(activeCoordinates[2].X, activeCoordinates[2].Y);
                            temp[3] = new Point(activeCoordinates[3].X - 1, activeCoordinates[3].Y - 1);

                            RotatePieceCase(activeRotateState, temp);
                            break;

                        case 2:
                            temp[0] = new Point(activeCoordinates[0].X + 1, activeCoordinates[0].Y + 1);
                            temp[1] = new Point(activeCoordinates[1].X - 1, activeCoordinates[1].Y + 1);
                            temp[2] = new Point(activeCoordinates[2].X, activeCoordinates[2].Y);
                            temp[3] = new Point(activeCoordinates[3].X + 1, activeCoordinates[3].Y - 1);

                            RotatePieceCase(activeRotateState, temp);
                            break;

                        case 3:
                            temp[0] = new Point(activeCoordinates[0].X - 1, activeCoordinates[0].Y + 1);
                            temp[1] = new Point(activeCoordinates[1].X - 1, activeCoordinates[1].Y - 1);
                            temp[2] = new Point(activeCoordinates[2].X, activeCoordinates[2].Y);
                            temp[3] = new Point(activeCoordinates[3].X + 1, activeCoordinates[3].Y + 1);

                            RotatePieceCase(activeRotateState, temp);
                            break;
                    }
                    break;

                case PieceType.Z:
                    switch (activeRotateState)
                    {
                        case 0:
                        case 2:
                            temp[0] = new Point(activeCoordinates[0].X, activeCoordinates[0].Y - 1);
                            temp[1] = new Point(activeCoordinates[1].X - 1, activeCoordinates[1].Y);
                            temp[2] = new Point(activeCoordinates[2].X + 2, activeCoordinates[2].Y - 1);
                            temp[3] = new Point(activeCoordinates[3].X + 1, activeCoordinates[3].Y);

                            RotatePieceCase(activeRotateState, temp);
                            break;

                        case 1:
                        case 3:
                            temp[0] = new Point(activeCoordinates[0].X, activeCoordinates[0].Y + 1);
                            temp[1] = new Point(activeCoordinates[1].X + 1, activeCoordinates[1].Y);
                            temp[2] = new Point(activeCoordinates[2].X - 2, activeCoordinates[2].Y + 1);
                            temp[3] = new Point(activeCoordinates[3].X - 1, activeCoordinates[3].Y);

                            RotatePieceCase(activeRotateState, temp);
                            break;
                    }
                    break;
            }
        }

        void SelectNextPiece()
        {
            nextPieceType = (PieceType)random.Next(Enum.GetNames(typeof(PieceType)).Length);

            // Set the next piece texture
            switch (nextPieceType)
            {
                case PieceType.I:
                    nextPieceTexture = iNextTexture;
                    break;

                case PieceType.J:
                    nextPieceTexture = jNextTexture;
                    break;

                case PieceType.L:
                    nextPieceTexture = lNextTexture;
                    break;

                case PieceType.O:
                    nextPieceTexture = oNextTexture;
                    break;

                case PieceType.S:
                    nextPieceTexture = sNextTexture;
                    break;

                case PieceType.T:
                    nextPieceTexture = tNextTexture;
                    break;

                case PieceType.Z:
                    nextPieceTexture = zNextTexture;
                    break;
            }
        }
    }
}
