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

        //Magic Constants
        public const string SPRITES = "Assets/sprites.png";
        public const int PAUSE_TIME = 20000;
        public const float DRINK_ANIMATION_INTERVAL = 80f;
        public const int DRINK_ANIMATION_FRAMES = 8;
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

        //<TODO> Figure out what's happening
        public static void AnimateMessingEnd()
        {
            Game1.playSound("coin");
            Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(192, 1152, Game1.tileSize, Game1.tileSize), 50f, 4, 0, Animations.GetWho().position - new Vector2(Animations.GetWho().facingDirection == 1 ? 0.0f : -Game1.tileSize, Game1.tileSize * 2), false, Animations.GetWho().facingDirection == 1, Animations.GetWho().getStandingY() / 10000f, 0.01f, Microsoft.Xna.Framework.Color.White, 1f, 0.0f, 0.0f, 0.0f, false));
            Animations.GetWho().completelyStopAnimatingOrDoingAction();
            Animations.GetWho().forceCanMove();
        }

        public static void AnimateMessingStart(Body b, bool voluntary, bool inUnderwear, bool inToilet)
        {
            Game1.playSound("slosh");
            if (b.sleeping || !voluntary && !Regression.config.AlwaysNoticeAccidents && (double)b.bowelContinence + 0.449999988079071 <= Regression.rnd.NextDouble())
                return;
            if (!inUnderwear)
            {
                if (inToilet)
                    Say(Animations.GetData().Poop_Toilet, b);
                else
                    Say(Animations.GetData().Poop_Voluntary, b);
            }
            else if (voluntary)
                Say(Animations.GetData().Mess_Voluntary, b);
            else
                Say(Animations.GetData().Mess_Accident, b);
            Animations.GetWho().forceCanMove();
            Animations.GetWho().completelyStopAnimatingOrDoingAction();
            Animations.GetWho().jitterStrength = 1.0f;
            Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(192, 1152, Game1.tileSize, Game1.tileSize), 50f, 4, 0, Animations.GetWho().position - new Vector2(((Character)Animations.GetWho()).facingDirection == 1 ? 0.0f : (float)-Game1.tileSize, (float)(Game1.tileSize * 2)), false, ((Character)Animations.GetWho()).facingDirection == 1, (float)((Character)Animations.GetWho()).getStandingY() / 10000f, 0.01f, Microsoft.Xna.Framework.Color.White, 1f, 0.0f, 0.0f, 0.0f, false));
            Animations.GetWho().doEmote(12, false);
            Animations.GetWho().freezePause = 20000;
            Animations.GetWho().canMove = false;
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
            bool first = b.peedToiletLastNight > 0;
            bool second = b.poopedToiletLastNight > 0;
            if (!(first | second) || !Regression.config.Wetting && !Regression.config.Messing)
                return;
            Write(Strings.ReplaceAndOr(Strings.RandString(Animations.GetData().Toilet_Night), first, second, "&"), b);
        }

        public static void AnimatePeeAttempt(Body b, bool inUnderwear, bool inToilet)
        {
            if (inUnderwear)
                Say(Animations.GetData().Wet_Attempt, b);
            else if (inToilet)
                Say(Animations.GetData().Pee_Toilet_Attempt, b);
            else
                Say(Animations.GetData().Pee_Attempt, b);
        }

        public static void AnimatePoopAttempt(Body b, bool inUnderwear, bool inToilet)
        {
            if (inUnderwear)
                Animations.Say(Animations.GetData().Mess_Attempt, b);
            else if (inToilet)
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
            Animations.Write(Strings.InsertVariables(Strings.RandString(Animations.GetData().Washing_Underwear), (Body)null, c), (Body)null);
        }

        public static void AnimateWettingEnd(Body b)
        {
            if (b.wettingUnderwear && (double)b.pants.wetness > (double)b.pants.absorbency)
            {
                ((List<TemporaryAnimatedSprite>)((GameLocation)Animations.GetWho().currentLocation).temporarySprites).Add(new TemporaryAnimatedSprite(13, (Vector2)((Character)Game1.player).position, Microsoft.Xna.Framework.Color.White, 10, ((Random)Game1.random).NextDouble() < 0.5, 70f, 0, (int)Game1.tileSize, 0.05f, -1, 0));
                HoeDirt terrainFeature;
                if (Animations.GetWho().currentLocation.terrainFeatures.ContainsKey(((Character)Animations.GetWho()).getTileLocation()) && (terrainFeature = Animations.GetWho().currentLocation.terrainFeatures[((Character)Animations.GetWho()).getTileLocation()] as HoeDirt) != null)
                    terrainFeature.state.Value = 1;
            }
            Animations.GetWho().completelyStopAnimatingOrDoingAction();
            Animations.GetWho().forceCanMove();
        }

        public static void AnimateWettingStart(Body b, bool voluntary, bool inUnderwear, bool inToilet)
        {
            Game1.playSound("wateringCan");
            if (b.sleeping || !voluntary && !Regression.config.AlwaysNoticeAccidents && (double)b.bladderContinence + 0.200000002980232 <= Regression.rnd.NextDouble())
                return;
            if (!inUnderwear)
            {
                if (inToilet)
                    Animations.Say(Animations.GetData().Pee_Toilet, b);
                else
                    Animations.Say(Animations.GetData().Pee_Voluntary, b);
            }
            else if (voluntary)
                Animations.Say(Animations.GetData().Wet_Voluntary, b);
            else
                Animations.Say(Animations.GetData().Wet_Accident, b);
            Animations.GetWho().forceCanMove();
            Animations.GetWho().completelyStopAnimatingOrDoingAction();
            Animations.GetWho().jitterStrength = 0.5f;
            ((Character)Animations.GetWho()).doEmote(28, false);
            if (!inUnderwear)
            {
                ((List<TemporaryAnimatedSprite>)((GameLocation)Animations.GetWho().currentLocation).temporarySprites).Add(new TemporaryAnimatedSprite(13, (Vector2)((Character)Game1.player).position, Microsoft.Xna.Framework.Color.White, 10, ((Random)Game1.random).NextDouble() < 0.5, 70f, 0, (int)Game1.tileSize, 0.05f, -1, 0));
                HoeDirt terrainFeature;
                if (Animations.GetWho().currentLocation.terrainFeatures.ContainsKey(((Character)Animations.GetWho()).getTileLocation()) && (terrainFeature = Animations.GetWho().currentLocation.terrainFeatures[((Character)Animations.GetWho()).getTileLocation()] as HoeDirt) != null)
                    terrainFeature.state.Value = 1;
            }
            Animations.GetWho().freezePause = 20000;
            Animations.GetWho().canMove = false;
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
            Animations.Say(Animations.GetData().LookPants[0] + " " + Strings.DescribeUnderwear(b.pants, (string)null) + ".", b);
        }

        public static void CheckUnderwear(Body b)
        {
            Say(Animations.GetData().PeekWaistband[0] + " " + Strings.DescribeUnderwear(b.underwear, (string)null) + ".", b);
        }

        public static void DrawUnderwearIcon(Container c, int x, int y)
        {
            if (c.drying)
            {
                ((SpriteBatch)Game1.spriteBatch).Draw(Animations.GetSprites(), new Microsoft.Xna.Framework.Rectangle(x, y, 64, 64), new Microsoft.Xna.Framework.Rectangle?(Animations.UnderwearRectangle(c, "drying", 16)), Microsoft.Xna.Framework.Color.White);
            }
            else
            {
                ((SpriteBatch)Game1.spriteBatch).Draw(Animations.GetSprites(), new Microsoft.Xna.Framework.Rectangle(x, y, 64, 64), new Microsoft.Xna.Framework.Rectangle?(Animations.UnderwearRectangle(c, "clean", 16)), Microsoft.Xna.Framework.Color.White);
                int height1 = Math.Min((int)((double)c.wetness / (double)c.absorbency * 16.0), 16);
                int height2 = Math.Min((int)((double)c.messiness / (double)c.containment * 16.0), 16);
                if (height1 > 0 && height1 >= height2)
                    ((SpriteBatch)Game1.spriteBatch).Draw(Animations.GetSprites(), new Microsoft.Xna.Framework.Rectangle(x, y + (64 - height1 * 4), 64, height1 * 4), new Microsoft.Xna.Framework.Rectangle?(Animations.UnderwearRectangle(c, "wet", height1)), Microsoft.Xna.Framework.Color.White);
                if (height2 > 0)
                    ((SpriteBatch)Game1.spriteBatch).Draw(Animations.GetSprites(), new Microsoft.Xna.Framework.Rectangle(x, y + (64 - height2 * 4), 64, height2 * 4), new Microsoft.Xna.Framework.Rectangle?(Animations.UnderwearRectangle(c, "messy", height2)), Microsoft.Xna.Framework.Color.White);
                if (height1 > 0 && height1 < height2)
                    ((SpriteBatch)Game1.spriteBatch).Draw(Animations.GetSprites(), new Microsoft.Xna.Framework.Rectangle(x, y + (64 - height1 * 4), 64, height1 * 4), new Microsoft.Xna.Framework.Rectangle?(Animations.UnderwearRectangle(c, "wet", height1)), Microsoft.Xna.Framework.Color.White);
                if (Game1.getMouseX() >= x && Game1.getMouseX() <= x + 64 && Game1.getMouseY() >= y && Game1.getMouseY() <= y + 64)
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
            return true;
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

        public static Microsoft.Xna.Framework.Rectangle UnderwearRectangle(Container c, string type = null, int height = 16)
        {
            if (c.spriteIndex == -1)
                throw new Exception("Invalid sprite index.");
            int num = type != null ? (!(type == "drying") ? (!(type == "messy") ? (!(type == "wet") ? 0 : 16) : 32) : 48) : (!c.drying ? ((double)c.messiness <= 0.0 ? ((double)c.wetness <= 0.0 ? 0 : 16) : 32) : 48);
            return new Microsoft.Xna.Framework.Rectangle(c.spriteIndex * 16, num + (16 - height), 16, height);
        }

        public static void Warn(string msg, Body b = null)
        {
            Game1.addHUDMessage(new HUDMessage(Strings.InsertVariables(msg, b, (Container)null), 2));
        }

        public static void Warn(string[] msgs, Body b = null)
        {
            Animations.Warn(Strings.RandString(msgs), b);
        }

        public static void Write(string msg, Body b = null)
        {
            Game1.drawObjectDialogue(Strings.InsertVariables(msg, b, (Container)null));
        }

        public static void Write(string[] msgs, Body b = null)
        {
            Animations.Write(Strings.RandString(msgs), b);
        }
    }
}
