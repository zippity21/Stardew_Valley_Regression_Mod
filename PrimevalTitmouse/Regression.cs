using Microsoft.Xna.Framework;
using Regression;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PrimevalTitmouse
{
    public class Regression : Mod
    {
        public static string foodToHandle = null;
        public static int lastTimeOfDay = 0;
        public static bool morningHandled = true;
        public static Random rnd = new Random();
        public static bool started = false;
        public Body body;
        public static Config config;
        public static IModHelper help;
        public static IMonitor monitor;
        public bool shiftHeld;
        public static Data t;
        public static Farmer who;

        public override void Entry(IModHelper h)
        {
            help = h;
            monitor = Monitor;
            config = Helper.ReadConfig<Config>();
            t = Helper.Data.ReadJsonFile<Data>(string.Format("{0}.json", (object)config.Lang)) ?? Helper.Data.ReadJsonFile<Data>("en.json");
            h.Events.GameLoop.Saving += new EventHandler<SavingEventArgs>(this.BeforeSave);
            h.Events.GameLoop.DayStarted += new EventHandler<DayStartedEventArgs>(ReceiveAfterDayStarted);
            h.Events.GameLoop.UpdateTicked += new EventHandler<UpdateTickedEventArgs>(ReceiveEighthUpdateTick);
            h.Events.GameLoop.TimeChanged += new EventHandler<TimeChangedEventArgs>(ReceiveTimeOfDayChanged);
            h.Events.Input.ButtonPressed += new EventHandler<ButtonPressedEventArgs>(ReceiveKeyPress);
            h.Events.Input.ButtonReleased += new EventHandler<ButtonReleasedEventArgs>(ReceiveKeyReleased);
            h.Events.Input.ButtonPressed += new EventHandler<ButtonPressedEventArgs>(ReceiveMouseChanged);
            h.Events.Display.MenuChanged += new EventHandler<MenuChangedEventArgs>(ReceiveMenuChanged);
            h.Events.Display.RenderingHud += new EventHandler<RenderingHudEventArgs>(ReceivePreRenderHudEvent);
        }

        public void DrawStatusBars()
        {
            int x1 = Game1.viewport.Width - (65 + StatusBars.barWidth);
            int y1 = Game1.viewport.Height - (25 + StatusBars.barHeight);
            if (Game1.currentLocation is MineShaft || Game1.currentLocation is Woods || Game1.currentLocation is SlimeHutch || who.health < who.maxHealth)
                x1 -= 58;
            if (!config.NoHungerAndThirst || PrimevalTitmouse.Regression.config.Debug)
            {
                float percentage1 = this.body.food / body.maxFood;
                StatusBars.DrawStatusBar(x1, y1, percentage1, new Color(115, byte.MaxValue, 56));
                int x2 = x1 - (10 + StatusBars.barWidth);
                float percentage2 = body.water / body.maxWater;
                StatusBars.DrawStatusBar(x2, y1, percentage2, new Color(117, 225, byte.MaxValue));
                x1 = x2 - (10 + StatusBars.barWidth);
            }
            if (config.Debug)
            {
                if (config.Messing)
                {
                    float percentage = body.bowels / body.maxBowels;
                    StatusBars.DrawStatusBar(x1, y1, percentage, new Color(146, 111, 91));
                    x1 -= 10 + StatusBars.barWidth;
                }
                if (config.Wetting)
                {
                    float percentage = body.bladder / body.maxBladder;
                    StatusBars.DrawStatusBar(x1, y1, percentage, new Color(byte.MaxValue, 225, 56));
                }
            }
            if (!config.Wetting && !config.Messing)
                return;
            int y2 = (Game1.player.questLog).Count == 0 ? 250 : 310;
            Animations.DrawUnderwearIcon(body.underwear, Game1.viewport.Width - 94, y2);
        }

        private void GiveUnderwear()
        {
            List<Item> objList = new List<Item>();
            foreach (string validUnderwearType in Strings.ValidUnderwearTypes())
                objList.Add(new Underwear(validUnderwearType, 0.0f, 0.0f, 20));
            objList.Add(new StardewValley.Object(399, 99, false, -1, 0));
            objList.Add(new StardewValley.Object(348, 99, false, -1, 0));
            Game1.activeClickableMenu = new ItemGrabMenu(objList);
        }

        private void ReceiveAfterDayStarted(object sender, DayStartedEventArgs e)
        {
            body = Helper.Data.ReadJsonFile<Body>(string.Format("{0}/RegressionSave.json", Constants.SaveFolderName)) ?? new Body();
            started = true;
            who = Game1.player;
            morningHandled = false;
            Animations.AnimateNight(body);
        }

        private void BeforeSave(object Sender, SavingEventArgs e)
        {
            body.bedtime = lastTimeOfDay;
            if (Game1.dayOfMonth != 1 || Game1.currentSeason != "spring" || Game1.year != 1)
                body.HandleNight();
            if (string.IsNullOrWhiteSpace(Constants.SaveFolderName))
                return;

            Helper.Data.WriteJsonFile(string.Format("{0}/RegressionSave.json", Constants.SaveFolderName), body);
        }



        private void ReceiveEighthUpdateTick(object sender, UpdateTickedEventArgs e)
        {
            if (e.IsMultipleOf(8))
            {
                if (!started)
                    return;
                if (!morningHandled && !Game1.fadeToBlack && who.canMove)
                {
                    body.HandleMorning();
                    morningHandled = true;
                }
                if ((Game1.game1.IsActive || Game1.options.pauseWhenOutOfFocus == false) && (Game1.paused == false && Game1.dialogueUp == false) && (Game1.currentMinigame == null && Game1.eventUp == false && (Game1.activeClickableMenu == null && Game1.menuUp == false)) && Game1.fadeToBlack == false)
                    this.body.HandleTime(0.003100775f);

                if (Game1.player.isEating && Game1.activeClickableMenu == null && foodToHandle == null)
                    foodToHandle = who.itemToEat.Name.ToLower();
                else if (foodToHandle != null && !Game1.player.isEating)
                {
                    if (new Regex("(beer|ale|wine|juice|mead|coffee|milk)").IsMatch(foodToHandle))
                        body.DrinkBeverage();
                    else
                        body.Eat();
                    foodToHandle = null;
                }
            }
        }

        private void ReceiveKeyPress(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.LeftShift)
            {
                shiftHeld = true;
            }
            else
            {
                if (!started)
                    return;
                switch (e.Button)
                {
                    case SButton.L:
                        if (config.Debug && this.shiftHeld)
                        {
                            body.DecreaseFoodAndWater();
                            break;
                        }
                        break;
                    case SButton.S:
                        if (config.Debug && shiftHeld)
                        {
                            body.IncreaseEverything();
                            break;
                        }
                        break;
                    case SButton.F1:
                        if (!body.isWetting && !body.isMessing && !body.IsFishing())
                        {
                            body.StartWetting(true, !shiftHeld);
                            break;
                        }
                        break;
                    case SButton.F2:
                        if (!body.isWetting && !body.isMessing && !body.IsFishing())
                        {
                            body.StartMessing(true, !shiftHeld);
                            break;
                        }
                        break;
                    case SButton.F3:
                        if (config.Debug)
                        {
                            GiveUnderwear();
                            break;
                        }
                        break;
                    case SButton.F5:
                        Animations.CheckUnderwear(body);
                        break;
                    case SButton.F6:
                        Animations.CheckPants(body);
                        break;
                    case SButton.F7:
                        if (config.Debug)
                        {
                            TimeMagic.doMagic();
                            break;
                        }
                        break;
                    case SButton.F8:
                        config.Wetting = !config.Wetting;
                        config.Messing = !config.Messing;
                        break;
                    case SButton.F9:
                        config.Debug = !config.Debug;
                        break;
                }
            }
        }

        private void ReceiveKeyReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (e.Button != SButton.LeftShift)
                return;
            shiftHeld = false;
        }

        private void ReceiveMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!started)
                return;
            DialogueBox newMenu1;
            if (Game1.currentLocation is FarmHouse && (newMenu1 = e.NewMenu as DialogueBox) != null && Game1.currentLocation.lastQuestionKey == "Sleep" && !config.Easymode)
            {
                if (body.beddingDryTime > Game1.timeOfDay)
                {
                    List<Response> privateValue = newMenu1.responses;
                    if (privateValue.Count == 2)
                    {
                        Response response = privateValue[1];
                        Game1.currentLocation.answerDialogue(response);
                        Game1.currentLocation.lastQuestionKey = null;
                        newMenu1.closeDialogue();
                        Animations.AnimateDryingBedding(body);
                    }
                }
            }
            else
            {
                ShopMenu newMenu2;
                if (Game1.currentLocation is SeedShop && (newMenu2 = e.NewMenu as ShopMenu) != null)
                {
                    string fieldName1 = "forSale";
                    List<ISalable> field1 = ((object)newMenu2).GetField<List<ISalable>>(fieldName1);

                    string fieldName2 = "itemPriceAndStock";

                    Dictionary<ISalable, int[]> field2 = ((object)newMenu2).GetField<Dictionary<ISalable, int[]>>(fieldName2);
                    List<string> stringList = Strings.ValidUnderwearTypes();
                    stringList.Remove("Joja diaper");
                    foreach (string type in stringList)
                    {
                        Underwear underwear = new Underwear(type, 0.0f, 0.0f, 1);
                        field1.Add(underwear);
                        int[] numArray = new int[2]
                        {
                            underwear.container.price,
                            999
                        };
                        field2.Add(underwear, numArray);
                    }
                }
                else
                {
                    ShopMenu newMenu3;
                    if (Game1.currentLocation is JojaMart && (newMenu3 = e.NewMenu as ShopMenu) != null)
                    {
                        string fieldName1 = "forSale";
                        List<ISalable> field1 = ((object)newMenu3).GetField<List<ISalable>>(fieldName1);

                        string fieldName2 = "itemPriceAndStock";

                        Dictionary<ISalable, int[]> field2 = ((object)newMenu3).GetField<Dictionary<ISalable, int[]>>(fieldName2);
                        string[] strArray = new string[2]
                        {
                            "Joja diaper",
                            "cloth diaper"
                        };
                        foreach (string type in strArray)
                        {
                            Underwear underwear = new Underwear(type, 0.0f, 0.0f, 1);
                            field1.Add((Item)underwear);
                            int[] numArray = new int[2]
                            {
                                (int) ( underwear.container.price * 1.20000004768372),
                                999
                            };
                            field2.Add(underwear, numArray);
                        }
                    }
                    else if (Game1.currentLocation is Farm && (e.NewMenu is LetterViewerMenu && !Mail.showingLetter))
                        Mail.ShowLetter(body, Helper);
                }
            }
        }

        private void ReceiveMouseChanged(object sender, ButtonPressedEventArgs e)
        {
            if (Game1.game1.IsActive && !Game1.paused && started)
            {
                if (e.Button == SButton.MouseRight)
                {
                    if ((Game1.dialogueUp || Game1.currentMinigame != null || (Game1.eventUp || Game1.activeClickableMenu != null) || Game1.menuUp || Game1.fadeToBlack) || (who.isRidingHorse() || !who.canMove || (Game1.player.isEating || who.canOnlyWalk) || who.FarmerSprite.pauseForSingleAnimation))
                        return;
                    if (who.CurrentTool != null && who.CurrentTool is WateringCan)
                    {
                        this.body.DrinkWateringCan();
                    }
                    else
                    {
                        Underwear activeObject = who.ActiveObject as Underwear;
                        if (activeObject != null)
                        {
                            if ((double)activeObject.container.wetness + (double)activeObject.container.messiness == 0.0 && !activeObject.container.drying)
                            {
                                who.reduceActiveItemByOne();
                                Container container = body.ChangeUnderwear(activeObject);
                                Underwear underwear = new Underwear(container.name, container.wetness, container.messiness, 1);
                                if (!who.addItemToInventoryBool(underwear, false))
                                {
                                    List<Item> objList = new List<Item>();
                                    objList.Add(underwear);
                                    Game1.activeClickableMenu = new ItemGrabMenu(objList);
                                }
                            }
                            else if (activeObject.container.washable)
                            {
                                GameLocation currentLocation = Game1.currentLocation;
                                Vector2 toolLocation = who.GetToolLocation(false);
                                int x = (int)toolLocation.X;
                                int y = (int)toolLocation.Y;
                                if (currentLocation.doesTileHaveProperty(x / Game1.tileSize, y / Game1.tileSize, "Water", "Back") != null || currentLocation.doesTileHaveProperty(x / Game1.tileSize, y / Game1.tileSize, "WaterSource", "Back") != null)
                                {
                                    Animations.AnimateWashingUnderwear(activeObject.container);
                                    activeObject.container.wetness = 0.0f;
                                    activeObject.container.messiness = 0.0f;
                                    activeObject.container.drying = true;
                                }
                            }
                        }
                        else
                        {
                            GameLocation currentLocation = Game1.currentLocation;
                            Vector2 toolLocation = who.GetToolLocation(false);
                            int x = (int)toolLocation.X;
                            int y = (int)toolLocation.Y;
                            Vector2 vector2 = new Vector2((float)(x / Game1.tileSize), y / Game1.tileSize);
                            if (currentLocation.doesTileHaveProperty(x / Game1.tileSize, y / Game1.tileSize, "Water", "Back") != null || currentLocation.doesTileHaveProperty(x / Game1.tileSize, y / Game1.tileSize, "WaterSource", "Back") != null || currentLocation is BuildableGameLocation && (currentLocation as BuildableGameLocation).getBuildingAt(vector2) != null && ((currentLocation as BuildableGameLocation).getBuildingAt(vector2).buildingType.Value.Equals("Well") && (currentLocation as BuildableGameLocation).getBuildingAt(vector2).daysOfConstructionLeft.Value <= 0))
                                this.body.DrinkWaterSource();
                        }
                    }
                }
            }
        }

        public void ReceivePreRenderHudEvent(object sender, RenderingHudEventArgs args)
        {
            if (!started || Game1.currentMinigame != null || Game1.eventUp || Game1.globalFade)
                return;
            DrawStatusBars();
        }

        private void ReceiveTimeOfDayChanged(object sender, TimeChangedEventArgs e)
        {
            lastTimeOfDay = Game1.timeOfDay;
            if (Game1.timeOfDay == 610)
                Mail.CheckMail(this.body);
            if (rnd.NextDouble() >= 0.0555555559694767 || body.underwear.wetness + (double)body.underwear.messiness <= 0.0 || Game1.timeOfDay < 630)
                return;
            Animations.AnimateStillSoiled(this.body);
        }

        public Regression()
        {
            //base.Actor();
        }
    }
}