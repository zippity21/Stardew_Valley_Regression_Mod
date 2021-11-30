using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace PrimevalTitmouse
{ 

    internal static class Animations
    {
        private static readonly List<string> NPC_LIST = new List<string> { "Linus", "Krobus", "Dwarf" };
        public static readonly int poopAnimationTime = 2000; //ms
        public static readonly int peeAnimationTime = 2000; //ms
        //Magic Constants
        public const string SPRITES = "Assets/sprites.png";
        public const int PAUSE_TIME = 20000;
        public const float DRINK_ANIMATION_INTERVAL = 80f;
        public const int DRINK_ANIMATION_FRAMES = 8;
        public const int LARGE_SPRITE_DIM = 64;
        public const int SMALL_SPRITE_DIM = 16;
        public const int DIAPER_HUD_DIM   = 64;
        enum FaceDirection : int
        {
            Down  = 2,
            Left  = 1,
            Right = 3,
            Up    = 0
        };

        public static Texture2D sprites;
        private static Data t;
        private static Farmer who;

        //Static Accessor Methods. Ensure that variables are initialized.
        public static Data GetData()
        {
            t ??= Regression.t;
            return t;
        }
        public static Texture2D GetSprites()
        {
            sprites ??= Regression.help.Content.Load<Texture2D>(SPRITES, StardewModdingAPI.ContentSource.ModFolder);
            return sprites;
        }

        public static Farmer GetWho()
        {
            Animations.who ??= Game1.player;
            return who;
        }

        public static float ZoomScale()
        {
            return Game1.options.zoomLevel / Game1.options.uiScale;
        }

        public static void AnimateDrinking(bool waterSource = false)
        {
            //If we aren't facing downward, turn
            if (Animations.GetWho().getFacingDirection() != (int)FaceDirection.Down)
                Animations.GetWho().faceDirection((int)FaceDirection.Down);

            //Stop doing anything that would prevent us from moving
            //Essentially take control of the variable
            Animations.GetWho().forceCanMove();

            //Stop any form of animation
            Animations.GetWho().completelyStopAnimatingOrDoingAction();

            // ISSUE: method pointer
            //Start Drinking animation. While drinking pause time and don't allow movement.
            Animations.GetWho().FarmerSprite.animateOnce(StardewValley.FarmerSprite.drink, DRINK_ANIMATION_INTERVAL, DRINK_ANIMATION_FRAMES, new AnimatedSprite.endOfAnimationBehavior(EndDrinking));
            Animations.GetWho().freezePause = PAUSE_TIME;
            Animations.GetWho().canMove = false;

            //If we drink from the watering can, don't say anything
            if (!waterSource)
                return;

            //Otherwise say something about it
            Say(Animations.GetData().Drink_Water_Source, null);
        }

        //Not really an animation. Just say the bedding's current state.
        public static void AnimateDryingBedding(Body b)
        {
            Write(Animations.GetData().Bedding_Still_Wet, b);
        }


        public static void AnimateMessingStart(Body b, bool voluntary, bool inUnderwear)
        {

            if (b.IsFishing()) return;

            if (b.underwear.removable || inUnderwear)
                Game1.playSound("slosh");

            if (b.isSleeping || !voluntary && !Regression.config.AlwaysNoticeAccidents && (double)b.bowelContinence + 0.449999988079071 <= Regression.rnd.NextDouble())
                return;

            if (!(b.underwear.removable || inUnderwear))
            {
                Animations.Say(Animations.GetData().Cant_Remove, b);
                return;
            }

            if (!inUnderwear)
            {
                if (b.InToilet(inUnderwear))
                    Say(Animations.GetData().Poop_Toilet, b);
                else
                    Say(Animations.GetData().Poop_Voluntary, b);
            }
            else if (voluntary)
                Say(Animations.GetData().Mess_Voluntary, b);
            else
                Say(Animations.GetData().Mess_Accident, b);

            //Animations.GetWho().forceCanMove();
            //Animations.GetWho().completelyStopAnimatingOrDoingAction();
            Animations.GetWho().jitterStrength = 1.0f;
            Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(192, 1152, Game1.tileSize, Game1.tileSize), 50f, 4, 0, Animations.GetWho().position - new Vector2(((Character)Animations.GetWho()).facingDirection == 1 ? 0.0f : (float)-Game1.tileSize, (float)(Game1.tileSize * 2)), false, ((Character)Animations.GetWho()).facingDirection == 1, (float)((Character)Animations.GetWho()).getStandingY() / 10000f, 0.01f, Microsoft.Xna.Framework.Color.White, 1f, 0.0f, 0.0f, 0.0f, false));
         
            Animations.GetWho().freezePause = poopAnimationTime;
            Animations.GetWho().canMove = false;
            Animations.GetWho().doEmote(12, false);
        }
        public static void AnimateMessingEnd(Body b)
        {

            if (b.IsFishing()) return;
            Game1.playSound("coin");
            Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(192, 1152, Game1.tileSize, Game1.tileSize), 50f, 4, 0, Animations.GetWho().position - new Vector2(Animations.GetWho().facingDirection == 1 ? 0.0f : -Game1.tileSize, Game1.tileSize * 2), false, Animations.GetWho().facingDirection == 1, Animations.GetWho().getStandingY() / 10000f, 0.01f, Microsoft.Xna.Framework.Color.White, 1f, 0.0f, 0.0f, 0.0f, false));
        }

        public static void AnimateWettingStart(Body b, bool voluntary, bool inUnderwear)
        {
            if (b.IsFishing()) return;

            if(b.underwear.removable || inUnderwear)
              Game1.playSound("wateringCan");

            if (b.isSleeping || !voluntary && !Regression.config.AlwaysNoticeAccidents && (double)b.bladderContinence + 0.200000002980232 <= Regression.rnd.NextDouble())
                return;

            if (!(b.underwear.removable || inUnderwear))
            {
                Animations.Say(Animations.GetData().Cant_Remove, b);
                return;
            }

                if (!inUnderwear)
            {
                if (b.InToilet(inUnderwear))
                    Animations.Say(Animations.GetData().Pee_Toilet, b);
                else
                    Animations.Say(Animations.GetData().Pee_Voluntary, b);

                ((List<TemporaryAnimatedSprite>)((GameLocation)Animations.GetWho().currentLocation).temporarySprites).Add(new TemporaryAnimatedSprite(13, (Vector2)((Character)Game1.player).position, Microsoft.Xna.Framework.Color.White, 10, ((Random)Game1.random).NextDouble() < 0.5, 70f, 0, (int)Game1.tileSize, 0.05f, -1, 0));
                HoeDirt terrainFeature;
                if (Animations.GetWho().currentLocation.terrainFeatures.ContainsKey(((Character)Animations.GetWho()).getTileLocation()) && (terrainFeature = Animations.GetWho().currentLocation.terrainFeatures[((Character)Animations.GetWho()).getTileLocation()] as HoeDirt) != null)
                    terrainFeature.state.Value = 1;
            }
            else if (voluntary)
                Animations.Say(Animations.GetData().Wet_Voluntary, b);
            else
                Animations.Say(Animations.GetData().Wet_Accident, b);

            //Animations.GetWho().forceCanMove();
            //Animations.GetWho().completelyStopAnimatingOrDoingAction();
            Animations.GetWho().jitterStrength = 0.5f;
            Animations.GetWho().freezePause = peeAnimationTime; //milliseconds
            Animations.GetWho().canMove = false;
            ((Character)Animations.GetWho()).doEmote(28, false);
        }

        public static void AnimateWettingEnd(Body b)
        {
            if (b.IsFishing()) return;
            if ((double)b.pants.wetness > (double)b.pants.absorbency)
            {
                ((List<TemporaryAnimatedSprite>)((GameLocation)Animations.GetWho().currentLocation).temporarySprites).Add(new TemporaryAnimatedSprite(13, (Vector2)((Character)Game1.player).position, Microsoft.Xna.Framework.Color.White, 10, ((Random)Game1.random).NextDouble() < 0.5, 70f, 0, (int)Game1.tileSize, 0.05f, -1, 0));
                HoeDirt terrainFeature;
                if (Animations.GetWho().currentLocation.terrainFeatures.ContainsKey(((Character)Animations.GetWho()).getTileLocation()) && (terrainFeature = Animations.GetWho().currentLocation.terrainFeatures[((Character)Animations.GetWho()).getTileLocation()] as HoeDirt) != null)
                    terrainFeature.state.Value = 1;
            }
        }

        public static void AnimateMorning(Body b)
        {
            bool flag = (double)b.pants.wetness > 0.0;
            bool second = (double)b.pants.messiness > 0.0;
            string msg = "" + Strings.RandString(Animations.GetData().Wake_Up_Underwear_State);
            if (second)
            {
                msg = msg + " " + Strings.ReplaceOptional(Strings.RandString(Animations.GetData().Messed_Bed), flag);
                if (!Regression.config.Easymode)
                    msg = msg + " " + Strings.ReplaceAndOr(Strings.RandString(Animations.GetData().Washing_Bedding), flag, second, "&");
            }
            else if (flag)
            {
                msg = msg + " " + Strings.RandString(Animations.GetData().Wet_Bed);
                if (!Regression.config.Easymode)
                    msg = msg + " " + Strings.ReplaceAndOr(Strings.RandString(Animations.GetData().Spot_Washing_Bedding), flag, second, "&");
            }
            Animations.Write(msg, b);
        }

        public static void AnimateNight(Body b)
        {
            bool first = b.numPottyPeeAtNight > 0;
            bool second = b.numPottyPooAtNight > 0;
            if (!(first | second) || !Regression.config.Wetting && !Regression.config.Messing)
              return;
            string toiletMsg = Strings.ReplaceAndOr(Strings.RandString(Animations.GetData().Toilet_Night), first, second, "&");

            if (b.numAccidentPooAtNight == 0 && b.numAccidentPeeAtNight == 0)
                toiletMsg = Strings.InsertVariable(toiletMsg, "$HOW_MANY_TIMES", ".");
            else
            {
                if (!b.underwear.removable)
                {
                    toiletMsg = Strings.InsertVariable(toiletMsg, "$HOW_MANY_TIMES", ", but couldn't get your $UNDERWEAR_NAME$ off!$HOW_MANY_TIMES");
                    toiletMsg = Strings.InsertVariable(toiletMsg, "$HOW_MANY_TIMES", " So you still woke up$HOW_MANY_TIMES");
                } else
                {
                    toiletMsg = Strings.InsertVariable(toiletMsg, "$HOW_MANY_TIMES", ", but you still woke up$HOW_MANY_TIMES");
                }
                if (b.numAccidentPeeAtNight > 0)
                    toiletMsg = Strings.InsertVariable(toiletMsg, "$HOW_MANY_TIMES", " wet$HOW_MANY_TIMES");
                if (b.numAccidentPooAtNight > 0)
                    toiletMsg = Strings.InsertVariable(toiletMsg, "$HOW_MANY_TIMES", " and messy$HOW_MANY_TIMES");
                if (b.numAccidentPooAtNight > 0 || b.numAccidentPeeAtNight > 0)
                    toiletMsg = Strings.InsertVariable(toiletMsg, "$HOW_MANY_TIMES", "! Looks like someone really does need to be in their diapers at night$HOW_MANY_TIMES");
            }
            toiletMsg = Strings.InsertVariable(toiletMsg, "$HOW_MANY_TIMES", ".");
            Write(toiletMsg, b);
        }

        public static void AnimatePeeAttempt(Body b, bool inUnderwear)
        {

            if (b.IsFishing()) return;
            if (inUnderwear)
                Say(Animations.GetData().Wet_Attempt, b);
            else if (b.InToilet(inUnderwear))
                Say(Animations.GetData().Pee_Toilet_Attempt, b);
            else
                Say(Animations.GetData().Pee_Attempt, b);
        }

        public static void AnimatePoopAttempt(Body b, bool inUnderwear)
        {

            if (b.IsFishing()) return;
            if (inUnderwear)
                Animations.Say(Animations.GetData().Mess_Attempt, b);
            else if (b.InToilet(inUnderwear))
                Animations.Say(Animations.GetData().Poop_Toilet_Attempt, b);
            else
                Animations.Say(Animations.GetData().Poop_Attempt, b);
        }

        public static void AnimateStillSoiled(Body b)
        {
            Animations.Say(Strings.ReplaceAndOr(Strings.RandString(Animations.GetData().Still_Soiled), (double)b.underwear.wetness > 0.0, (double)b.underwear.messiness > 0.0, "&"), b);
        }

        public static void AnimateWashingUnderwear(Container c)
        {
            if (c.MarkedForDestroy())
            {
                Animations.Write(Strings.InsertVariables(Animations.GetData().Overwashed_Underwear[0], (Body)null, c), (Body)null);
                Game1.player.reduceActiveItemByOne();
            }
            else
            {
                Animations.Write(Strings.InsertVariables(Strings.RandString(Animations.GetData().Washing_Underwear), (Body)null, c), (Body)null);
            }
        }


        public static Texture2D Bitmap2Texture(Bitmap bmp)
        {
            MemoryStream memoryStream = new MemoryStream();
            bmp.Save((Stream)memoryStream, ImageFormat.Png);
            memoryStream.Seek(0L, SeekOrigin.Begin);
            return Texture2D.FromStream(((GraphicsDeviceManager)Game1.graphics).GraphicsDevice, (Stream)memoryStream);
        }

        public static void CheckPants(Body b)
        {
            StardewValley.Objects.Clothing pants = (StardewValley.Objects.Clothing)Animations.GetWho().pantsItem.Value;
            b.pants.name = pants.displayName;
            b.pants.description = pants.displayName;
            b.pants.plural = true;
            Animations.Say(Animations.GetData().LookPants[0] + " " + Strings.DescribeUnderwear(b.pants, null) + ".", b);
        }

        public static void CheckUnderwear(Body b)
        {
            Say(Animations.GetData().PeekWaistband[0] + " " + Strings.DescribeUnderwear(b.underwear, (string)null) + ".", b);
        }

        public static void DrawUnderwearIcon(Container c, int x, int y)
        {
            if (c.IsDrying())
            {
                ((SpriteBatch)Game1.spriteBatch).Draw(Animations.GetSprites(), new Microsoft.Xna.Framework.Rectangle(x, y, DIAPER_HUD_DIM, DIAPER_HUD_DIM), new Microsoft.Xna.Framework.Rectangle?(Animations.UnderwearRectangle(c, "drying", LARGE_SPRITE_DIM)), Microsoft.Xna.Framework.Color.White);
            }
            else
            {
                Microsoft.Xna.Framework.Color defaultColor = Microsoft.Xna.Framework.Color.White;
                double wetPercent = (double)c.wetness / (double)c.absorbency;
                double messPercent =(double)c.messiness / (double)c.containment;

                Texture2D underwearSprites = Animations.GetSprites();
                Microsoft.Xna.Framework.Rectangle srcBoxClean = Animations.UnderwearRectangle(c, "clean", LARGE_SPRITE_DIM);
                Microsoft.Xna.Framework.Rectangle srcBoxWet   = Animations.UnderwearRectangle(c, "wet"  , (int)(wetPercent*LARGE_SPRITE_DIM));
                Microsoft.Xna.Framework.Rectangle srcBoxMessy = Animations.UnderwearRectangle(c, "messy", (int)(messPercent*LARGE_SPRITE_DIM));

                int messHUD = (int)(messPercent * DIAPER_HUD_DIM);
                int wetHUD = (int)(wetPercent * DIAPER_HUD_DIM);
                int yClean = 0;
                int yMessy = DIAPER_HUD_DIM - messHUD;
                int yWet   = DIAPER_HUD_DIM - wetHUD;
                Microsoft.Xna.Framework.Rectangle destBoxClean = new Microsoft.Xna.Framework.Rectangle(x, y+yClean, DIAPER_HUD_DIM, DIAPER_HUD_DIM);
                Microsoft.Xna.Framework.Rectangle destBoxMessy = new Microsoft.Xna.Framework.Rectangle(x, y+yMessy, DIAPER_HUD_DIM, messHUD);
                Microsoft.Xna.Framework.Rectangle destBoxWet   = new Microsoft.Xna.Framework.Rectangle(x, y+yWet  , DIAPER_HUD_DIM, wetHUD);

                //Texture, Destination, Source, Color
                //Draw(Texture2D texture, Vector2 position, Rectangle ? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth);
                //((SpriteBatch)Game1.spriteBatch).Draw(underwearSprites, new Vector2(x, y+yClean), srcBoxClean, defaultColor, 0f, new Vector2(0,0), new Vector2(1,1), (SpriteEffects)0, 0f);

                ((SpriteBatch)Game1.spriteBatch).Draw(underwearSprites, destBoxClean, srcBoxClean, defaultColor);
                if(wetPercent >= messPercent)
                {
                    ((SpriteBatch)Game1.spriteBatch).Draw(underwearSprites, destBoxWet, srcBoxWet, defaultColor);
                    ((SpriteBatch)Game1.spriteBatch).Draw(underwearSprites, destBoxMessy, srcBoxMessy, defaultColor);
                } else
                {
                    ((SpriteBatch)Game1.spriteBatch).Draw(underwearSprites, destBoxMessy, srcBoxMessy, defaultColor);
                    ((SpriteBatch)Game1.spriteBatch).Draw(underwearSprites, destBoxWet, srcBoxWet, defaultColor);
                }

                if (Game1.getMouseX() >= x && Game1.getMouseX() <= x + DIAPER_HUD_DIM && Game1.getMouseY() >= y && Game1.getMouseY() <= y + DIAPER_HUD_DIM)
                {
                    string source = Strings.DescribeUnderwear(c, (string)null);
                    string str = source.First<char>().ToString().ToUpper() + source.Substring(1);
                    int num = Game1.tileSize * 6 + Game1.tileSize / 6;
                    IClickableMenu.drawHoverText((SpriteBatch)Game1.spriteBatch, Game1.parseText(str, (SpriteFont)Game1.tinyFont, num), (SpriteFont)Game1.smallFont, 0, 0, -1, (string)null, -1, (string[])null, (Item)null, 0, -1, -1, -1, -1, 1f, (CraftingRecipe)null);
                }
            }
        }

        private static void EndDrinking(Farmer who)
        {
            Animations.GetWho().completelyStopAnimatingOrDoingAction();
            Animations.GetWho().forceCanMove();
        }

        public static void HandlePasserby()
        {
            if (Utility.isThereAFarmerOrCharacterWithinDistance(Animations.GetWho().getTileLocation(), 3, (GameLocation)Game1.currentLocation) is not NPC npc || NPC_LIST.Contains(npc.Name))
                return;
            npc.CurrentDialogue.Push(new Dialogue("Oh wow, your diaper is all wet!", npc));
        }

        public static bool HandleVillager(Body b, bool mess, bool inUnderwear, bool overflow, bool attempt = false, int baseFriendshipLoss = 20, int radius = 3)
        {
            bool someoneNoticed = true;
            int actualLoss = -(baseFriendshipLoss / 20);

            //If we are messing, increase the radius of noticability (stinky)
            //Double how much friendship we lose (mess is gross)
            if (mess)
            {
                radius *= 2;
                actualLoss *= 2;
            }

            //If we pulled down our pants, quadrupal the radius (not contained and visible!)
            //Double loss since you're just going infront of people (how uncouth)
            if (!inUnderwear)
            {
                radius *= 4;
                actualLoss *= 2;
            }

            //Did we try, but not actually succeed? (not full enough)
            if (attempt)
                actualLoss /= 2;

            //Double noticability is we had a blow-out/leak (people can see)
            if (overflow)
                radius *= 2;

            //Get NPC in radius
            //<TODO> This needs to be reworked to get a list of NPCs
            if (Utility.isThereAFarmerOrCharacterWithinDistance(((Character)Animations.GetWho()).getTileLocation(), radius, (GameLocation)Game1.currentLocation) is not NPC npc || NPC_LIST.Contains(npc.Name))
                return false;

            //Reduce the loss if the person likes you (more forgiving)
            int heartLevelForNpc = Animations.GetWho().getFriendshipHeartLevelForNPC(npc.getName());

            //Does this leave the possiblity of friendship gain if we have enough hearts already? Maybe because they find the vulnerability endearing?
            int friendshipLoss = actualLoss + (heartLevelForNpc - 2) / 2 * baseFriendshipLoss;

            //Make a list based on who saw us.
            List<string> npcType = new List<string>();
            string npcName = "";
            if (npc is Horse || npc is Cat || npc is Dog)
            {
                npcType.Add("animal");
                npcName += string.Format("{0}: ", npc.Name);
            }
            else
            {
                switch (npc.Age)
                {
                    case 0:
                        npcType.Add("adult");
                        break;
                    case 1:
                        npcType.Add("teen");
                        break;
                    case 2:
                        npcType.Add("kid");
                        break;
                }
                npcType.Add(npc.getName().ToLower());
            }

            //What did we do? Use to figure out the response.
            string responseKey = "";
            if (!inUnderwear)
            {
                //If we weren't wearing underwear, we tried on the ground.
                //But did we succeed or just try to go?
                responseKey = attempt ? "ground_attempt" : "ground";
            }
            else
            {
                //Otherwise, we are soiling ourselves
                responseKey += "soiled";

                //Animals only have a "nice" reponse
                if (npcType.Contains("animal"))
                {
                    responseKey += "_nice";
                    friendshipLoss = 0;
                }
                //If we have a really high relationship with the NPC, they're very nice about our accident
                else if (heartLevelForNpc >= 8)
                {
                    responseKey += "_verynice";
                    friendshipLoss = 0;
                }
                else
                    //Otherwise they'll be mean or nice depending on how much friendship we're losing
                    responseKey = friendshipLoss < 0 ? responseKey + "_mean" : responseKey + "_nice";

                //Why are Abigail and Jodi special?
                if (npc.getName() == "Abigail" || npc.getName() == "Jodi")
                    friendshipLoss = 0;
            }

            //If we're in debug mode, notify how the relationship was effected
            if (Regression.config.Debug)
                Animations.Say(string.Format("{0} ({1}) changed friendship from {2} by {3}.", npc.Name, npc.Age, heartLevelForNpc, friendshipLoss), (Body)null);

            //If we didn't lose any friendship, or we disabled friendship penalties, then don't adjust the value
            if (friendshipLoss < 0 && !Regression.config.NoFriendshipPenalty)
                Animations.GetWho().changeFriendship(friendshipLoss, npc);


            List<string> stringList3 = new List<string>();
            foreach (string key2 in npcType)
            {
                Dictionary<string, string[]> dictionary;
                string[] strArray;
                if (Animations.GetData().Villager_Reactions.TryGetValue(key2, out dictionary) && dictionary.TryGetValue(responseKey, out strArray))
                    stringList3.AddRange((IEnumerable<string>)strArray);
            }

            //Construct and say Statement
            string npcStatement = npcName + Strings.InsertVariables(Strings.ReplaceAndOr(Strings.RandString(stringList3.ToArray()), !mess, mess, "&"), b, (Container)null);
            npc.setNewDialogue(npcStatement, true, true);
            Game1.drawDialogue(npc);
            return someoneNoticed;
        }

        public static Texture2D LoadTexture(string file)
        {
            return Animations.Bitmap2Texture(new Bitmap(Image.FromFile(Path.Combine(Regression.help.DirectoryPath, "Assets", file))));
        }

        public static void Say(string msg, Body b = null)
        {
            Game1.showGlobalMessage(Strings.InsertVariables(msg, b, (Container)null));
        }

        public static void Say(string[] msgs, Body b = null)
        {
            Animations.Say(Strings.RandString(msgs), b);
        }

        public static Microsoft.Xna.Framework.Rectangle UnderwearRectangle(Container c, string type = null, int height = LARGE_SPRITE_DIM)
        {
            if (c.spriteIndex == -1)
                throw new Exception("Invalid sprite index.");
            int num = type != null ? (!(type == "drying") ? (!(type == "messy") ? (!(type == "wet") ? 0 : LARGE_SPRITE_DIM) : LARGE_SPRITE_DIM*2) : LARGE_SPRITE_DIM*3) : (!c.IsDrying() ? ((double)c.messiness <= 0.0 ? ((double)c.wetness <= 0.0 ? 0 : LARGE_SPRITE_DIM) : LARGE_SPRITE_DIM*2) : LARGE_SPRITE_DIM*3);
            return new Microsoft.Xna.Framework.Rectangle(c.spriteIndex * LARGE_SPRITE_DIM, num + (LARGE_SPRITE_DIM - height), LARGE_SPRITE_DIM, height);
        }

        public static void Warn(string msg, Body b = null)
        {
            Game1.addHUDMessage(new HUDMessage(Strings.InsertVariables(msg, b, (Container)null), 2));
        }

        public static void Warn(string[] msgs, Body b = null)
        {
            Animations.Warn(Strings.RandString(msgs), b);
        }

        public static void Write(string msg, Body b = null, int delay = 0)
        {
            DelayedAction.showDialogueAfterDelay(Strings.InsertVariables(msg, b, (Container)null), delay);
        }

        public static void Write(string[] msgs, Body b = null, int delay = 0)
        {
            Animations.Write(Strings.RandString(msgs), b, delay);
        }
    }
}
