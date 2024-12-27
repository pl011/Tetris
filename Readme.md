# Tetris Clone
Originally written in XNA, this has been ported to Monogame, and additional keyboard support added

# Controls
Modify TetrisGame.Update(). Either ProcessUserInputKeyboard() or ProcessUserInput() depending on whether keyboard or controller supoort is required respectively.
This was originally written to be used with a controller, so the GUI only displays the controller controls.

On a keyboard, use the following:
* Enter to start the game, or escape to quit
* Arrow keys to move
* Z and X to rotate the piece
* On game over, enter to restart, or escape to quit
