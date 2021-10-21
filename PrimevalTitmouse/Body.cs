using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;

namespace PrimevalTitmouse
{
    public class Body
    {
        public static float baseFoodDay = 75f;
        public static float baseMaxBladder = 500f;
        public static float baseMaxBowels = 150f;
        public static float baseWaterDay = 1250f;
        public static float glassOfWater = 240f;
        public int beddingDryTime = 0;
        public int bedtime = 0;
        public float bladder = 0.0f;
        public float bladderContinence = 1f;
        public float bowelContinence = 1f;
        public float bowels = 0.0f;
        public float maxFood = 750f;
        public float maxWater = 1750f;
        public float food = 45f;
        public float foodDay = baseFoodDay;
        public bool isMessing = false;
        public bool isWetting = false;
        public List<string> lettersReceived = new List<string>();
        public float maxBladder = baseMaxBladder;
        public float maxBowels = baseMaxBowels;
        public bool messingUnderwear = false;
        public bool messingVoluntarily = false;
        public Container pants = new Container("blue jeans", 0.0f, 0.0f);
        public int peedToiletLastNight = 0;
        public int poopedToiletLastNight = 0;
        public bool sleeping = false;
        public float[] stomach = new float[2];
        public Container underwear = new Container("dinosaur undies", 0.0f, 0.0f);
        public float water = 850f;
        public float waterDay = baseWaterDay;
        public bool wettingUnderwear = false;
        public bool wettingVoluntarily = false;
        public float lastStamina;

        public void AddBladder(float amount)
        {
            if (!Regression.config.Wetting)
                return;
            float oldPercent = (maxBladder - bladder) / maxBladder;
            this.bladder += amount;
            if (!isWetting)
            {
                if (bladder >= (double)maxBladder)
                {
                    if (!isWetting && !isMessing && !IsFishing())
                        StartWetting(false, true);
                }
                else
                {
                    this.bladder = Math.Max(bladder, 0.0f);
                    float newPercent = (maxBladder - bladder) / maxBladder;
                    if ((newPercent <= 0.0 ? 1.0 : bladderContinence / (4f * newPercent)) > Regression.rnd.NextDouble())
                    {
                        string[][] msgs = new string[3][]
                        {
                            Regression.t.Bladder_Red,
                            Regression.t.Bladder_Orange,
                            Regression.t.Bladder_Yellow
                        };

                        Warn(oldPercent, newPercent, 
                            new float[3]
                            {
                                0.1f,
                                0.3f,
                                0.5f
                            }, msgs, false);
                    }
                }
            }
        }

        public void AddBowel(float value)
        {
            if (!Regression.config.Messing)
                return;
            float oldPercent = (maxBowels - bowels) / maxBowels;
            bowels += value;
            if (!isMessing)
            {
                if ((double)bowels >= maxBowels)
                {
                    if (!isWetting && !isMessing && !IsFishing())
                        StartMessing(false, true);
                }
                else
                {
                    bowels = Math.Max(bowels, 0.0f);
                    float newPercent = (maxBowels - bowels) / maxBowels;
                    if ((newPercent <= 0.0 ? 1.0 : bowelContinence / (4f * newPercent)) > Regression.rnd.NextDouble())
                    {
                        string[][] msgs = new string[3][]
                        {
                            Regression.t.Bowels_Red,
                            Regression.t.Bowels_Orange,
                            Regression.t.Bowels_Yellow
                        };

                        Warn(oldPercent, newPercent, new float[3]
                        {
                            0.05f,
                            0.1f,
                            0.3f
                        }, msgs, false);
                    }
                }
            }
        }

        public void AddFood(float amount)
        {
            float oldPercent = food / maxFood;
            float num = 0.5f;
            if (amount < 0.0)
                AddStomach(Math.Min(-amount, food) * num, 0.0f);
            food += amount;
            if (food < maxFood / 10.0 && Regression.config.NoHungerAndThirst)
                food = maxFood;
            if (food > (double)maxFood)
            {
                AddStomach((food - maxFood) * num, 0.0f);
                food = maxFood;
            }
            else if (food < 0.0)
            {
                Game1.player.stamina = Math.Max(0.0f, Game1.player.stamina + (int)(food / (double)maxFood * 200.0));
                lastStamina = Game1.player.stamina;
                food = 0.0f;
            }
            if (amount >= 0.0 || Regression.config.NoHungerAndThirst)
                return;
            float[] thresholds = new float[2] { 0.0f, 0.25f };
            string[][] msgs = new string[2][]
            {
                Regression.t.Food_None,
                Regression.t.Food_Low
            };
            Warn(oldPercent, food / maxFood, thresholds, msgs, false);
        }

        public void AddStomach(float food, float water)
        {
            stomach[0] = Math.Max(stomach[0] + food, 0.0f);
            stomach[1] = Math.Max(stomach[1] + water, 0.0f);
        }

        public void AddWater(float amount, float conversionRatio = 0.65f)
        {
            float oldPercent = this.water / this.maxWater;
            if (amount < 0.0)
                this.AddStomach(0.0f, Math.Min(-amount, water) * conversionRatio);
            water += amount;
            if (water < maxWater / 10.0 && Regression.config.NoHungerAndThirst)
                water = maxWater;
            if (water > (double)maxWater)
            {
                AddStomach(0.0f, water - maxWater);
                water = maxWater;
            }
            else if (water < 0.0)
            {
                Game1.player.health = !Regression.config.Easymode ? Math.Max(0, Game1.player.health + (int)Math.Ceiling(water * 100.0 / maxWater)) : Math.Max(0, Game1.player.health + (int)Math.Ceiling(water * 50.0 / maxWater));
                this.water = 0.0f;
            }
            if (amount >= 0.0 || Regression.config.NoHungerAndThirst)
                return;
            float[] thresholds = new float[2] { 0.0f, 0.25f };
            string[][] msgs = new string[2][]
            {
                Regression.t.Water_None,
                Regression.t.Water_Low
            };

            Warn(oldPercent, water / maxWater, thresholds, msgs, false);
        }

        public void ChangeBladderContinence(bool decrease = true, float percent = 0.01f)
        {
            if (decrease)
                percent = -percent;
            float bladderContinence = this.bladderContinence;
            this.bladderContinence += percent;
            this.bladderContinence = Math.Max(Math.Min(this.bladderContinence, 1f), 0.05f);
            this.maxBladder += percent * Body.baseMaxBladder;
            this.maxBladder = Math.Max(Math.Min(this.maxBladder, Body.baseMaxBladder), Body.baseMaxBladder * 0.25f);
            if (!decrease)
                return;

            string[][] msgs = new string[4][]
            {
                Regression.t.Bladder_Continence_Min,
                Regression.t.Bladder_Continence_Red,
                Regression.t.Bladder_Continence_Orange,
                Regression.t.Bladder_Continence_Yellow
            };

            Warn(bladderContinence, this.bladderContinence, 
                new float[4]
                {
                    0.06f,
                    0.2f,
                    0.5f,
                    0.8f
                }, msgs, true);
        }

        public void ChangeBowelContinence(bool decrease = true, float percent = 0.01f)
        {
            if (decrease)
                percent = -percent;
            float bowelContinence = this.bowelContinence;
            this.bowelContinence += 2f * percent;
            this.bowelContinence = Math.Max(Math.Min(this.bowelContinence, 1f), 0.05f);
            this.maxBowels += percent * Body.baseMaxBowels;
            this.maxBowels = Math.Max(Math.Min(this.maxBowels, Body.baseMaxBowels), Body.baseMaxBowels * 0.25f);
            if (!decrease)
                return;
            string[][] msgs = new string[4][]
            {
                Regression.t.Bowel_Continence_Min,
                Regression.t.Bowel_Continence_Red,
                Regression.t.Bowel_Continence_Orange,
                Regression.t.Bowel_Continence_Yellow
            };
            this.Warn(bowelContinence, this.bowelContinence, new float[4]
            {
                0.06f,
                0.2f,
                0.5f,
                0.8f
            }, msgs, true);
        }

        private Container ChangeUnderwear(Container container)
        {
            Container underwear = this.underwear;
            this.underwear = container;
            pants = new Container("blue jeans", 0.0f, 0.0f);
            CleanPants();
            Animations.Say(Regression.t.Change, this);
            return underwear;
        }

        public Container ChangeUnderwear(Underwear uw)
        {
            return ChangeUnderwear(new Container(uw.container.name, uw.container.wetness, uw.container.messiness));
        }

        public Container ChangeUnderwear(string type)
        {
            return ChangeUnderwear(new Container(type, 0.0f, 0.0f));
        }

        public void CleanPants()
        {
            RemoveBuff(111);
            RemoveBuff(222);
        }

        public void DecreaseFoodAndWater()
        {
            AddWater(maxWater / -20f, 0.65f);
            AddFood(maxFood / -30f);
            AddBladder(maxBladder / -20f);
            AddBowel(maxBowels / -30f);
        }

        public void DrinkBeverage()
        {
            this.AddWater(glassOfWater * 2f, 0.65f);
        }

        public void DrinkWateringCan()
        {
            Farmer player = Game1.player;
            WateringCan currentTool = (WateringCan)player.CurrentTool;
            if (currentTool.WaterLeft > 0)
            {
                float amount1 = Math.Max(maxWater - water, glassOfWater);
                float amount2 = currentTool.WaterLeft * 100;
                if ((double)amount2 < amount1)
                {
                    this.AddWater(amount2, 0.65f);
                    currentTool.WaterLeft = 0;
                }
                else
                {
                    WateringCan wateringCan = currentTool;
                    wateringCan.WaterLeft = wateringCan.WaterLeft - (int)(amount1 / 100.0);
                    AddWater(amount1, 0.65f);
                }
                Animations.AnimateDrinking(false);
            }
            else
            {
                player.doEmote(4);
                Game1.showRedMessage("Out of water");
            }
        }

        public void DrinkWaterSource()
        {
            AddWater(Math.Max(maxWater - water, glassOfWater), 0.65f);
            Animations.AnimateDrinking(true);
        }

        public void Eat()
        {
            this.AddFood(Math.Max(maxFood / 4f, (float)((maxFood - (double)food) / 3.0)));
        }

        public void EndMessing()
        {
            isMessing = false;
            Animations.AnimateMessingEnd();
            if (sleeping || (Animations.HandleVillager(this, true, messingUnderwear, pants.messiness > 0.0, false, 20, 3) || pants.messiness <= 0.0 || !messingUnderwear))
                return;
            HandlePoopOverflow(pants);
        }

        public void EndWetting()
        {
            this.isWetting = false;
            Animations.AnimateWettingEnd(this);
            if (sleeping || (Animations.HandleVillager(this, false, wettingUnderwear, pants.wetness > 0.0, false, 20, 3) || pants.wetness <= 0.0 || !wettingUnderwear))
                return;
            HandlePeeOverflow(pants);
        }

        public void HandleMorning()
        {
            if (Regression.config.Easymode)
            {
                food = maxFood;
                water = maxWater;
            }
            if (!Regression.config.Wetting && !Regression.config.Messing)
            {
                peedToiletLastNight = 0;
                poopedToiletLastNight = 0;
                sleeping = false;
                pants = new Container("blue jeans", 0.0f, 0.0f);
            }
            else
            {
                if (!Regression.config.Easymode)
                {
                    int num = new Random().Next(1, 13);
                    if (num <= 2 && (pants.messiness > 0.0 || pants.wetness > (double)glassOfWater))
                    {
                        beddingDryTime = Game1.timeOfDay + 1000;
                        Farmer player = Game1.player;
                        player.stamina = (player.stamina - 20f);
                    }
                    else if (num <= 5 && pants.wetness > 0.0)
                    {
                        beddingDryTime = Game1.timeOfDay + 600;
                        Farmer player = Game1.player;
                        player.stamina = (player.stamina - 10f);
                    }
                    else
                        beddingDryTime = 0;
                }
                Animations.AnimateMorning(this);
                peedToiletLastNight = 0;
                poopedToiletLastNight = 0;
                sleeping = false;
                pants = new Container("blue jeans", 0.0f, 0.0f);
            }
        }

        public void HandleNight()
        {
            lastStamina = Game1.player.stamina;
            pants = new Container("bed", 0.0f, 0.0f);
            sleeping = true;
            if (bedtime <= 0)
                return;
            HandleTime(Math.Max(4, (Game1.timeOfDay + 2400 - bedtime) / 100) / 3f);
        }

        public void HandlePeeOverflow(Container pants)
        {
            Animations.Write(Regression.t.Pee_Overflow, this);
            int num = -Math.Max(Math.Min((int)(pants.wetness / pants.absorbency * 10.0), 10), 1);
            Buff buff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, num, 0, 15, "", "")
            {
                description = string.Format("{0} {1} Defense.", Strings.RandString(Regression.t.Debuff_Wet_Pants), num),
                millisecondsDuration = 1080000
            };
            buff.glow = pants.messiness != 0.0 ? Color.Brown : Color.Yellow;
            buff.sheetIndex = -1;
            buff.which = 111;
            if (Game1.buffsDisplay.hasBuff(111))
                this.RemoveBuff(111);
            Game1.buffsDisplay.addOtherBuff(buff);
        }

        public void HandlePoopOverflow(Container pants)
        {
            Animations.Write(Regression.t.Poop_Overflow, this);
            float num1 = pants.messiness / pants.containment;
            int num2 = num1 >= 0.5 ? (num1 > 1.0 ? -3 : -2) : -1;
            Buff buff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, num2, 0, 0, 15, "", "")
            {
                description = string.Format("{0} {1} Speed.", Strings.RandString(Regression.t.Debuff_Messy_Pants), (object)num2),
                millisecondsDuration = 1080000,
                glow = Color.Brown,
                sheetIndex = -1,
                which = 222
            };
            if (Game1.buffsDisplay.hasBuff(222))
                this.RemoveBuff(222);
            Game1.buffsDisplay.addOtherBuff(buff);
        }

        public void HandleStamina()
        {
            float num = (float)((Game1.player.stamina - (double)this.lastStamina) / 4.0);
            if ((double)num == 0.0)
                return;
            if (num < 0.0)
            {
                this.AddFood(num / 300f * this.maxFood);
                this.AddWater(num / 100f * this.maxWater, 0.05f);
            }
            this.lastStamina = Game1.player.stamina;
        }

        public void HandleStomach(float hours)
        {
            float val2_1 = this.foodDay * hours;
            float val2_2 = Body.glassOfWater * 2f * hours;
            float num = Math.Min(this.stomach[0], val2_1);
            float amount = Math.Min(this.stomach[1], val2_2);
            this.AddBowel(num);
            this.AddBladder(amount);
            this.AddStomach(-num, -amount);
        }

        public void HandleTime(float hours)
        {
            this.HandleStamina();
            this.AddWater((float)(waterDay * (double)hours / -24.0), 1f);
            this.AddFood((float)(foodDay * (double)hours / -24.0));
            this.HandleStomach(hours);
            if (this.isWetting)
                this.Wet(hours);
            if (!this.isMessing)
                return;
            this.Mess(hours);
        }

        public void IncreaseEverything()
        {
            this.AddWater(this.maxWater - this.water, 0.65f);
            this.AddFood(this.maxFood - this.food);
            this.AddBladder(this.maxBladder / 4f);
            this.AddBowel(this.maxBowels / 4f);
        }

        public bool IsFishing()
        {
            FishingRod currentTool;
            return (currentTool = Game1.player.CurrentTool as FishingRod) != null && (currentTool.isCasting || currentTool.isTimingCast || (currentTool.isNibbling || currentTool.isReeling) || currentTool.castedButBobberStillInAir || currentTool.pullingOutOfWater);
        }

        public void Mess(float hours)
        {
            float amount = (float)((double)this.maxBowels * (double)hours * 20.0);
            this.bowels -= amount;
            if (this.sleeping)
            {
                this.messingVoluntarily = Regression.rnd.NextDouble() < (double)this.bowelContinence;
                if (this.messingVoluntarily)
                {
                    ++this.poopedToiletLastNight;
                }
                else
                {
                    double num = (double)this.pants.AddPoop(this.underwear.AddPoop(amount));
                }
            }
            else if (this.messingUnderwear)
            {
                double num1 = (double)this.pants.AddPoop(this.underwear.AddPoop(amount));
            }
            if ((double)this.bowels > 0.0)
                return;
            this.bowels = 0.0f;
            this.EndMessing();
        }

        public void RemoveBuff(int which)
        {
            BuffsDisplay buffsDisplay = Game1.buffsDisplay;
            for (int index = buffsDisplay.otherBuffs.Count - 1; index >= 0; --index)
            {
                if (buffsDisplay.otherBuffs[index].which == which)
                {
                    buffsDisplay.otherBuffs[index].removeBuff();
                    buffsDisplay.otherBuffs.RemoveAt(index);
                    buffsDisplay.syncIcons();
                }
            }
        }

        public void StartMessing(bool voluntary = false, bool inUnderwear = true)
        {
            if (!Regression.config.Messing)
                return;
            if (bowels < (double)this.maxBowels / 10.0)
            {
                Animations.AnimatePoopAttempt(this, inUnderwear, Game1.currentLocation is FarmHouse);
                if (!inUnderwear)
                    Animations.HandleVillager(this, true, inUnderwear, false, true, 20, 3);
            }
            else
            {
                if (!voluntary || (double)this.bowels < (double)this.maxBowels * 0.5)
                    this.ChangeBowelContinence(true, 0.01f);
                else
                    this.ChangeBowelContinence(false, 0.01f);
                this.messingVoluntarily = voluntary;
                this.messingUnderwear = inUnderwear;
                this.isMessing = true;
                Animations.AnimateMessingStart(this, this.messingVoluntarily, this.messingUnderwear, Game1.currentLocation is FarmHouse);
            }
        }

        public void StartWetting(bool voluntary = false, bool inUnderwear = true)
        {
            if (!Regression.config.Wetting)
                return;
            if ((double)this.bladder < maxBladder / 10.0)
            {
                Animations.AnimatePeeAttempt(this, inUnderwear, Game1.currentLocation is FarmHouse);
                if (!inUnderwear)
                    Animations.HandleVillager(this, false, inUnderwear, false, true, 20, 3);
            }
            else
            {
                if (!voluntary || bladder < maxBladder * 0.5)
                    this.ChangeBladderContinence(true, 0.01f);
                else
                    this.ChangeBladderContinence(false, 0.01f);
                this.wettingVoluntarily = voluntary;
                this.wettingUnderwear = inUnderwear;
                this.isWetting = true;
                Animations.AnimateWettingStart(this, this.wettingVoluntarily, this.wettingUnderwear, Game1.currentLocation is FarmHouse);
            }
        }

        public void Warn(float oldPercent, float newPercent, float[] thresholds, string[][] msgs, bool write = false)
        {
            if (this.sleeping)
                return;
            for (int index = 0; index < thresholds.Length; ++index)
            {
                if ((double)oldPercent > (double)thresholds[index] && (double)newPercent <= (double)thresholds[index])
                {
                    if (write)
                    {
                        Animations.Write(msgs[index], this);
                        break;
                    }
                    Animations.Warn(msgs[index], this);
                    break;
                }
            }
        }

        public void Wet(float hours)
        {
            float amount = (float)((double)this.maxBladder * (double)hours * 30.0);
            this.bladder -= amount;
            if (this.sleeping)
            {
                this.wettingVoluntarily = Regression.rnd.NextDouble() < (double)this.bladderContinence;
                if (this.wettingVoluntarily)
                {
                    ++this.peedToiletLastNight;
                }
                else
                {
                    double num = this.pants.AddPee(this.underwear.AddPee(amount));
                }
            }
            else if (this.wettingUnderwear)
            {
                double num1 = this.pants.AddPee(this.underwear.AddPee(amount));
            }
            if (bladder > 0.0)
                return;
            this.bladder = 0.0f;
            this.EndWetting();
        }
    }
}
