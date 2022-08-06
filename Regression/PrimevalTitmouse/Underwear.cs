using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PrimevalTitmouse
{
    public class Underwear : StardewValley.Object, PyTK.CustomElementHandler.ISaveElement
    {
        public static Color color;
        public Container container;
        public string id;
        
        public Underwear()
        {
            //base.Actor();
        }

        public Underwear(string type, float wetness = 0.0f, float messiness = 0.0f, int count = 1)
        {
            //base.Actor();
            this.Initialize(type, wetness, messiness, count);
        }

        public override bool canBeDropped()
        {
            return false;
        }

        public override bool canBeGivenAsGift()
        {
            return false;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            int ratio = Animations.LARGE_SPRITE_DIM / Animations.SMALL_SPRITE_DIM;
            Vector2 offset = new(Game1.tileSize/2, Game1.tileSize/2); //Center of tile
            Vector2 origin = new(Animations.LARGE_SPRITE_DIM/2, Animations.LARGE_SPRITE_DIM/2); //Center of Sprrite
            Rectangle source = Animations.UnderwearRectangle(container, FullnessType.None, Animations.LARGE_SPRITE_DIM);
            spriteBatch.Draw(Animations.sprites, location + offset, new Rectangle?(source), Color.White * transparency, 0.0f, origin, Game1.pixelZoom * scaleSize/ratio, SpriteEffects.None, layerDepth);
            if (drawStackNumber.Equals(StackDrawType.Hide) || maximumStackSize() <= 1 || (scaleSize <= 0.3 || Stack == int.MaxValue) || Stack <= 1)
                return;
            Utility.drawTinyDigits(Stack, spriteBatch, location + new Vector2(Game1.tileSize - Utility.getWidthOfTinyDigitString(Stack, 3f * scaleSize) + 3f * scaleSize, (float)(Game1.tileSize - 18.0 * scaleSize + 2.0)), 3f * scaleSize, 1f, Color.White);
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            Rectangle rectangle = Animations.UnderwearRectangle(this.container, FullnessType.None, Animations.LARGE_SPRITE_DIM);
            spriteBatch.Draw(Animations.sprites, objectPosition, new Rectangle?(rectangle), Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom/(Animations.LARGE_SPRITE_DIM/Animations.SMALL_SPRITE_DIM), SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + 2) / 10000f));
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            return new Dictionary<string, string>()
            {
                {
                  "type",
                  container.name
                },
                {
                  "wetness",
                  string.Format("{0}",  container.wetness)
                },
                {
                  "messiness",
                  string.Format("{0}",  container.messiness)
                },
                {
                  "stack",
                  string.Format("{0}",  Stack)
                }
            };
        }

        public override string getDescription()
        {
            string source = Strings.DescribeUnderwear(this.container, (string)null);
            return Game1.parseText(source.First().ToString().ToUpper() + source.Substring(1), Game1.smallFont, Game1.tileSize * 6 + Game1.tileSize / 6);
        }

        public override Item getOne()
        {
            return new Underwear(this.name, this.container.wetness, this.container.messiness, 1);
        }

        public object getReplacement()
        {
            return new StardewValley.Object(685, 1, false, -1, 0);
        }

        public void Initialize(string type, float wetness, float messiness, int count = 1)
        {
            this.container = new Container(type);
            this.container.wetness = wetness;
            this.container.messiness = messiness;
            if (count > 1)
                Stack = count;
            id = type;
            name = container.name;
            Price = this.container.price;
        }

        public override int maximumStackSize()
        {
            if (container.messiness > 0.0 || container.wetness > 0.0 || container.IsDrying())
                return 1;
            return base.maximumStackSize();
        }

        public void rebuild(Dictionary<string, string> data, object replacement)
        {
            Initialize(data["type"], float.Parse(data["wetness"]), float.Parse(data["messiness"]), int.Parse(data["stack"]));
        }

        public override string DisplayName
        {
            get
            {
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Status + id);
            }
            set
            {
                displayName = value;
            }
        }

        public override string Name
        {
            get
            {
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Status + id);
            }
        }

        public string Status
        {
            get
            {
                if (container.messiness > 0.0 && container.wetness > 0.0)
                    return "wet and messy ";
                if (container.messiness > 0.0)
                    return "messy ";
                if (container.wetness > 0.0)
                    return "wet ";
                return container.IsDrying() ? "drying " : "";
            }
        }

        public static bool getPantsPlural(int itemNum)
        {
            //This was built based on the game's ClothingInformation.json file
            switch(itemNum)
            {
                case  -1: { return true; }
                case   0: { return true; }
                case   2: { return false; }
                case   3: { return false; }
                case   4: { return false; }
                case   5: { return true; }
                case   6: { return false; }
                case   7: { return false; }
                case   8: { return true; }
                case   9: { return true; }
                case  10: { return true; }
                case  11: { return false; }
                case  12: { return true; }
                case  13: { return true; }
                case  14: { return true; }
                case  15: { return true; }
                case 998: { return true; }
                case 999: { return true; }
            }
            return false;
        }
    }
}