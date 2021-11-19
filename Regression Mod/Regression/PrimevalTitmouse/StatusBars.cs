using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace PrimevalTitmouse
{
  internal class StatusBars
  {
    public static Color barBackgroundColor = Color.Black;
    public static Color barBackgroundTick = new Color(120, 120, 120);
    public static Color barBorderColor = Color.DarkGoldenrod;
    public static int barBorderWidth = 2;
    public static Color barForegroundColor = new Color(150, 150, 150);
    public static Color barForegroundTick = new Color(50, 50, 50);
    public static int barHeight = 204;
    public static int barWidth = 24;
    public static Texture2D barBackground;
    public static Texture2D barForeground;

    private static void CreateTextures()
    {
      StatusBars.barBackground = new Texture2D(((GraphicsDeviceManager) Game1.graphics).GraphicsDevice, StatusBars.barWidth, StatusBars.barHeight);
      StatusBars.barForeground = new Texture2D(((GraphicsDeviceManager) Game1.graphics).GraphicsDevice, StatusBars.barWidth, StatusBars.barHeight);
      Color[] data1 = new Color[StatusBars.barHeight * StatusBars.barWidth];
      Color[] data2 = new Color[StatusBars.barHeight * StatusBars.barWidth];
      for (int index1 = 0; index1 < StatusBars.barWidth; ++index1)
      {
        for (int index2 = 0; index2 < StatusBars.barHeight; ++index2)
        {
          Color color1 = StatusBars.barBackgroundColor;
          Color color2 = StatusBars.barForegroundColor;
          bool flag1 = index1 + 1 <= StatusBars.barBorderWidth || index1 >= StatusBars.barWidth - StatusBars.barBorderWidth;
          bool flag2 = index2 + 1 <= StatusBars.barBorderWidth || index2 >= StatusBars.barHeight - StatusBars.barBorderWidth;
          if (flag1 | flag2)
          {
            color1 = StatusBars.barBorderColor;
            color2 = Color.Transparent;
            if (flag1 & flag2)
              color1 = Color.Transparent;
          }
          if (!flag1)
          {
            float scale = new float[10]
            {
              1f,
              1.3f,
              1.7f,
              2f,
              1.9f,
              1.5f,
              1.3f,
              1f,
              0.8f,
              0.4f
            }[(int) ((double) index1 * 10.0 / (double) StatusBars.barWidth)];
            color1 = Color.Multiply(color1, scale);
            color2 = Color.Multiply(color2, scale);
          }
          data1[index1 + index2 * StatusBars.barWidth] = color1;
          data2[index1 + index2 * StatusBars.barWidth] = color2;
        }
      }
      StatusBars.barBackground.SetData<Color>(data1);
      StatusBars.barForeground.SetData<Color>(data2);
    }

    public static void DrawStatusBar(int x, int y, float percentage, Color color)
    {
      SpriteBatch spriteBatch = (SpriteBatch) Game1.spriteBatch;
      if (StatusBars.barBackground == null || StatusBars.barForeground == null)
        StatusBars.CreateTextures();
      percentage = Math.Min(percentage, 1f);
      Rectangle destinationRectangle = new Rectangle(x, y, StatusBars.barWidth, StatusBars.barHeight);
      spriteBatch.Draw(StatusBars.barBackground, destinationRectangle, new Rectangle?(new Rectangle(0, 0, StatusBars.barWidth, StatusBars.barHeight)), Color.White);
      int height = (int) ((double) (destinationRectangle.Height - StatusBars.barBorderWidth * 2) * (double) percentage);
      destinationRectangle.Y = destinationRectangle.Y + destinationRectangle.Height - height - StatusBars.barBorderWidth;
      destinationRectangle.Height = height;
      spriteBatch.Draw(StatusBars.barForeground, destinationRectangle, new Rectangle?(new Rectangle(0, 0, StatusBars.barWidth, height)), color);
    }
  }
}
