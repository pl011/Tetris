/* 
 * This class represents one cell of the board
 */

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    class Cell
    {
        protected bool isOccupied;
        protected bool isActive; // does cell contain a part of an active piece?
        protected Texture2D texture;
        protected Vector2 position;

        public bool IsOccupied
        {
            get { return isOccupied; }
            set { isOccupied = value; }
        }

        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }

        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        // Create a cell in coordinates (i,j)
        // Parameters:  i       -   y coordinate of Cell (row)
        //              j       -   x coordinate of Cell (column)
        //              width   -   width of a square on the board
        //              origin  -   origin of the playable area of the board
        public Cell(int i, int j, int width, Vector2 origin)
        {
            // Calculate position
            position.X = origin.X + j * width;
            position.Y = origin.Y + (i - 2) * width; // Top 2 rows are above playing area
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (isOccupied)
                spriteBatch.Draw(texture, position, Color.White);
        }
    }
}
