using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace BlowToTheBall
{
	class MenuSelect
	{
		Game1 game1;

		//Текстуры кнопок
		Texture2D Texture_Tile_BG;
		List<Texture2D> Texture_Tile;
		Texture2D Texture_Tile_Stars_0;
		Texture2D Texture_Tile_Stars_1;
		Texture2D Texture_Tile_Stars_2;
		Texture2D Texture_Tile_Stars_3;

		Texture2D Texture_Text_Select;

		Button Button_Back;

		//Системные переменные
		TouchLocationState Touch_State;
		Vector2 Touch_Position;
		Vector2 Position_tmp;
		int i;
		int j;
		int tmpi;

		public MenuSelect(Game1 game)
		{
			game1 = game;

			//Загрузка текстуры кнопок выбора уровня
			Texture_Tile_BG = game1.Content.Load<Texture2D>("images/buttons/select_bg");

			//Загрузка текстур номеров
			Texture_Tile = new List<Texture2D>();
			for (i = 0; i < 10; i++)
				Texture_Tile.Add(game1.Content.Load<Texture2D>("images/buttons/select_" + i.ToString()));

			//Загрузка текстур звёзд
			Texture_Tile_Stars_0 = game1.Content.Load<Texture2D>("images/buttons/select_stars_0");
			Texture_Tile_Stars_1 = game1.Content.Load<Texture2D>("images/buttons/select_stars_1");
			Texture_Tile_Stars_2 = game1.Content.Load<Texture2D>("images/buttons/select_stars_2");
			Texture_Tile_Stars_3 = game1.Content.Load<Texture2D>("images/buttons/select_stars_3");

			//Инициализация кнопок выбора уровня
			Position_tmp.X = game1.Center.X - (100 + Texture_Tile_BG.Width + (100 - Texture_Tile_BG.Width) / 2);
			Position_tmp.Y = game1.Center.Y - (200 + Texture_Tile_BG.Height / 2);

			for (i = 0; i < (game1.Level_Count / 4); i++)
			{
				for (j = 0; j < 4; j++)
				{
					if (j == 0)
						game1.Level_Data[i * 4 + j].Position.X = Position_tmp.X;
					else
						game1.Level_Data[i * 4 + j].Position.X = game1.Level_Data[j - 1].Position.X + 100;

					game1.Level_Data[i * 4 + j].Position.Y = Position_tmp.Y + 100 * i;
				}
			}

			Button_Back.texture = game1.Content.Load<Texture2D>("images/buttons/back");
			Button_Back.position.X = 0;
			Button_Back.position.Y = game1.graphics.PreferredBackBufferHeight - Button_Back.texture.Height;

			Texture_Text_Select = game1.Content.Load<Texture2D>("images/text/selectlevel");
		}

		//Рисование кнопки выбора уровня
		protected void DrawTile(int num)
		{
			if (game1.Level_Data[num].Enable)
			{
				game1.spriteBatch.Draw(Texture_Tile_BG, game1.Level_Data[num].Position, Color.White);

				if (game1.Level_Data[num].Used)
				{
					Position_tmp = game1.Level_Data[num].Position;
					Position_tmp.X += 8;
					Position_tmp.Y += 40;

					if (game1.Level_Data[num].Stars < 1)
						game1.spriteBatch.Draw(Texture_Tile_Stars_0, Position_tmp, Color.White);
					else if (game1.Level_Data[num].Stars == 1)
						game1.spriteBatch.Draw(Texture_Tile_Stars_1, Position_tmp, Color.White);
					else if (game1.Level_Data[num].Stars == 2)
						game1.spriteBatch.Draw(Texture_Tile_Stars_2, Position_tmp, Color.White);
					else if (game1.Level_Data[num].Stars > 2)
						game1.spriteBatch.Draw(Texture_Tile_Stars_3, Position_tmp, Color.White);
				}
			}
			else
			{
				game1.spriteBatch.Draw(Texture_Tile_BG, game1.Level_Data[num].Position, new Color(0.7f, 0.7f, 0.7f, 1f));
			}
			Position_tmp = game1.Level_Data[num].Position;
			Position_tmp.X += 17;
			Position_tmp.Y += 10;
			num++;
			tmpi = (int)System.Decimal.Divide(num, 10);
			game1.spriteBatch.Draw(Texture_Tile[tmpi], Position_tmp, Color.White);
			Position_tmp.X += 20;
			game1.spriteBatch.Draw(Texture_Tile[num - (tmpi * 10)], Position_tmp, Color.White);
		}

		public void Update(GameTime gameTime)
		{
			if (game1.gamePadState.Buttons.Back == ButtonState.Pressed)
				game1.Stat_Next = GameStat.MenuMain;

			foreach (TouchLocation tl in game1.touchState)
			{
				Touch_State = tl.State;
				Touch_Position = tl.Position;

				if (Touch_State == TouchLocationState.Pressed)
				{
					for (i = 0; i < game1.Level_Count; i++)
					{
						if (game1.Level_Data[i].Enable)
						{
							if (game1.Level_Data[i].touched(Touch_Position, Texture_Tile_BG.Width, Texture_Tile_BG.Height))
							{
								game1.Level = i;
								game1.Stat_Next = GameStat.Start;
								break;
							}
						}
					}
					if (Button_Back.touched(Touch_Position, game1))
						game1.Stat_Next = GameStat.MenuMain;
				}
				break;
			}
		}

		public void Draw()
		{
			game1.GraphicsDevice.Clear(Color.White);
			game1.spriteBatch.Begin();
			game1.spriteBatch.Draw(game1.Texture_BG, Vector2.Zero, Color.White);
			game1.spriteBatch.Draw(game1.Texture_Plate, game1.Pos_Plate, Color.White);
			game1.spriteBatch.Draw(Texture_Text_Select, game1.Pos_Plate, Color.White);

			for (i = 0; i < game1.Level_Count; i++)
				DrawTile(i);

			Button_Back.draw(game1.spriteBatch);

			game1.spriteBatch.End();
		}
	}
}