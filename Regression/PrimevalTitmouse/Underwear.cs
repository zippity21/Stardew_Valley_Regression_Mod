using PyTK.CustomElementHandler;
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
            Vector2 offset = new(0, 0);
            Rectangle source = Animations.UnderwearRectangle(container, null, Animations.LARGE_SPRITE_DIM);
            spriteBatch.Draw(Animations.sprites, location + offset, new Rectangle?(source), Color.White * transparency, 0.0f, new Vector2(0,0), Game1.pixelZoom * scaleSize/ratio, SpriteEffects.None, layerDepth);
            if (drawStackNumber.Equals(StackDrawType.Hide) || maximumStackSize() <= 1 || (scaleSize <= 0.3 || Stack == int.MaxValue) || Stack <= 1)
                return;
            Utility.drawTinyDigits(Stack, spriteBatch, location + new Vector2(Game1.tileSize - Utility.getWidthOfTinyDigitString(Stack, 3f * scaleSize) + 3f * scaleSize, (float)(Game1.tileSize - 18.0 * scaleSize + 2.0)), 3f * scaleSize, 1f, Color.White);
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            Rectangle rectangle = Animations.UnderwearRectangle(this.container, (string)null, Animations.LARGE_SPRITE_DIM);
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
            this.container = new Container(type, wetness, messiness);
            if (count > 1)
                Stack = count;
            id = type;
            name = container.name;
            Price = 2;
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
    }
}