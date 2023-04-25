using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;

namespace PrimevalTitmouse
{
    //<TODO> A lot of bladder and bowel stuff is processed similarly. Consider refactor with arrays and Function pointers.
    public class Body
    {
        //Lets think of Food in Calories, and water in mL
        //For a day Laborer (like a farmer) that should be ~3500 Cal, and 14000 mL
        //Of course this is dependent on amount of work, but let's go one step at a time
        private static readonly float requiredCaloriesPerDay = 3500f;
        private static readonly float requiredWaterPerDay = 8000f; //8oz glasses: every 20min for 8 hours + every 40 min for 8 hour
        //private static readonly float maxWaterInCan = 4000f; //How much water does the watering can hold? Max is 40, so *100

        //Average # of Pees per day is ~3
        public static readonly float maxBladderCapacity = 600; //about 600mL
        private static readonly float minBladderCapacity = maxBladderCapacity * 0.20f;
        private static readonly float waterToBladderConversion = 0.225f;//Only ~1/4 water becomes pee, rest is sweat etc.

        //Average # of poops per day varies wildly. Let's say about 1.5 per day.
        private static readonly float foodToBowelConversion = 0.67f;
        private static readonly float maxBowelCapacity = (requiredCaloriesPerDay*foodToBowelConversion) / 2f;
        private static readonly float minBowelCapacity = maxBowelCapacity * 0.20f;

        //Setup Thresholds and messages
        private static readonly float[] WETTING_THRESHOLDS = { 0.15f, 0.4f, 0.6f };
        private static readonly string[][] WETTING_MESSAGES = { Regression.t.Bladder_Red, Regression.t.Bladder_Orange, Regression.t.Bladder_Yellow };
        private static readonly float[] MESSING_THRESHOLDS = { 0.15f, 0.4f, 0.6f };
        private static readonly string[][] MESSING_MESSAGES = { Regression.t.Bowels_Red, Regression.t.Bowels_Orange, Regression.t.Bowels_Yellow };
        private static readonly float[] BLADDER_CONTINENCE_THRESHOLDS = { 0.2f, 0.5f, 0.6f, 0.8f };
        private static readonly string[][] BLADDER_CONTINENCE_MESSAGES = { Regression.t.Bladder_Continence_Min, Regression.t.Bladder_Continence_Red, Regression.t.Bladder_Continence_Orange, Regression.t.Bladder_Continence_Yellow };
        private static readonly float[] BOWEL_CONTINENCE_THRESHOLDS = { 0.2f, 0.5f, 0.6f, 0.8f };
        private static readonly string[][] BOWEL_CONTINENCE_MESSAGES = { Regression.t.Bowel_Continence_Min, Regression.t.Bowel_Continence_Red, Regression.t.Bowel_Continence_Orange, Regression.t.Bowel_Continence_Yellow };
        private static readonly float[] HUNGER_THRESHOLDS = { 0.0f, 0.25f };
        private static readonly string[][] HUNGER_MESSAGES = { Regression.t.Food_None, Regression.t.Food_Low };
        private static readonly float[] THIRST_THRESHOLDS = { 0.0f, 0.25f };
        private static readonly string[][] THIRST_MESSAGES = { Regression.t.Water_None, Regression.t.Water_Low };
        private static readonly int MESSY_DEBUFF = 222;
        private static readonly int WET_DEBUFF = 111;
        private static readonly int wakeUpPenalty = 4;

        //Things that describe an individual
        public int bedtime = 0;
        public float bladderCapacity = maxBladderCapacity;
        public float bladderContinence = 1f;
        public float bladderFullness = 0f;
        public float bowelCapacity = maxBowelCapacity;
        public float bowelContinence = 1f;
        public float bowelFullness = 0f;
        public float hunger = 0f;
        public float thirst = 0f;
        public bool isSleeping = false;
        public Container bed;
        public Container pants;
        public Container underwear;
        public int numPottyPooAtNight = 0;
        public int numPottyPeeAtNight = 0;
        public int numAccidentPooAtNight = 0;
        public int numAccidentPeeAtNight = 0;
        private float lastStamina = 0;

        public Body()
        {
            bed = new("bed");
            pants = new("blue jeans");
            underwear = new("dinosaur undies");
    }

        public float GetBladderTrainingThreshold()
        {
            return bladderCapacity * 0.5f;
         }

        public float GetBowelTrainingThreshold()
        {

            return bowelCapacity * 0.5f;
        }

        public float GetBladderAttemptThreshold()
        {
            return bladderCapacity * 0.1f;
        }

        public float GetBowelAttemptThreshold()
        {
                return bowelCapacity * 0.1f;
        }

        public float GetHungerPercent()
        {
            return (requiredCaloriesPerDay - hunger) / requiredCaloriesPerDay;
        }

        public float GetThirstPercent()
        {
            return (requiredWaterPerDay - thirst) / requiredWaterPerDay;
        }

        public float GetBowelPercent()
        {
            return bowelFullness / bowelCapacity;
        }

        public float GetBladderPercent()
        {
            return bladderFullness / bladderCapacity;
        }

        //Change current bladder value and handle warning messages
        public void AddBladder(float amount)
        {
            //If Wetting is disabled, don't do anything
            if (!Regression.config.Wetting)
                return;

            //Increment the current amount
            //We allow bladder to go over-full, to simulate the possibility of multiple night wettings
            //This is determined by the amount of water you have in your system when you go to bed
            float oldFullness = bladderFullness / maxBladderCapacity;
            bladderFullness += amount;
            float newFullness = bladderFullness / maxBladderCapacity;

            //Did we go over? Then have an accident.
            if (bladderFullness >= bladderCapacity)
            {
                Wet(voluntary: false, inUnderwear: true);
                newFullness = bladderFullness / maxBladderCapacity;
                //Otherwise, calculate the new value
            } else
            {
                //If we have no room left, or randomly based on our current continence level warn about how badly we need to pee
                if ((newFullness <= 0.0 ? 1.0 : bladderContinence / (4f * newFullness)) > Regression.rnd.NextDouble())
                {
                    Warn(1-oldFullness, 1-newFullness, WETTING_THRESHOLDS, WETTING_MESSAGES, false);
                }
            }
        }

        //Change current bowels value and handle warning messages
        public void AddBowel(float amount)
        {
            //If Wetting is disabled, don't do anything
            if (!Regression.config.Messing)
                return;

            //Increment the current amount
            //We allow bowels to go over-full, to simulate the possibility of multiple night messes
            //This is determined by the amount of ffod you have in your system when you go to bed
            float oldFullness = bowelFullness / maxBowelCapacity;
            bowelFullness += amount;
            float newFullness = bowelFullness / maxBowelCapacity;

            //Did we go over? Then have an accident.
            if (bowelFullness >= bowelCapacity)
            {
                Mess(voluntary: false, inUnderwear: true);
                newFullness = bowelFullness / maxBowelCapacity;
            }
            else
            {
                //If we have no room left, or randomly based on our current continence level warn about how badly we need to pee
                if ((newFullness <= 0.0 ? 1.0 : bowelContinence / (4f * newFullness)) > Regression.rnd.NextDouble())
                {
                    Warn(1-oldFullness, 1-newFullness, MESSING_THRESHOLDS, MESSING_MESSAGES, false);
                }
            }
        }

        //Change current Food value and handle warning messages
        //Notice that we do things here even if Hunger and Thirst are disabled
        //This is due to Food and Water's effect on Wetting/Messing
        public void AddFood(float amount, float conversionRatio = 1f)
        {
            //How full are we?
            float oldPercent = (requiredCaloriesPerDay - hunger) / requiredCaloriesPerDay;
            hunger -= amount;
            float newPercent = (requiredCaloriesPerDay - hunger) / requiredCaloriesPerDay;

            //Convert food lost into poo at half rate
            if (amount < 0 && hunger < requiredCaloriesPerDay)
                AddBowel(amount * -1f * conversionRatio * foodToBowelConversion);

            //If we go over full, add additional to bowels at half rate
            if (hunger < 0)
            {
                AddBowel(hunger * -0.5f * conversionRatio * foodToBowelConversion);
                hunger = 0f;
                newPercent =(requiredCaloriesPerDay - hunger) / requiredCaloriesPerDay;
            }

            if (Regression.config.NoHungerAndThirst)
            {
                hunger = 0; //Reset if disabled
                return;
            }

            //If we're starving and not eating, take a stamina hit
            if (hunger > requiredCaloriesPerDay && amount < 0)
            {
                //Take percentage off stamina equal to percentage above max hunger
                Game1.player.stamina += newPercent * Game1.player.MaxStamina;
                hunger = requiredCaloriesPerDay;
                newPercent = 1;
            }

            Warn(oldPercent, newPercent, HUNGER_THRESHOLDS, HUNGER_MESSAGES, false);
        }

        public void AddWater(float amount, float conversionRatio = 1f)
        {
            //How full are we?
            float oldPercent = (requiredWaterPerDay - thirst) / requiredWaterPerDay;
            thirst -= amount;
            float newPercent = (requiredWaterPerDay - thirst) / requiredWaterPerDay;

            //Convert water lost into pee at half rate
            if (amount < 0 && thirst < requiredWaterPerDay)
                AddBladder(amount * -1f * conversionRatio * waterToBladderConversion);

            //Also if we go over full, add additional to Bladder at half rate
            if (thirst < 0)
            {
                AddBladder((thirst * -0.5f * conversionRatio * waterToBladderConversion));
                thirst = 0f;
                newPercent = (requiredWaterPerDay - thirst) / requiredWaterPerDay;
            }

            if (Regression.config.NoHungerAndThirst)
            {
                thirst = 0; //Reset if disabled
                return;
            }

            //If we're starving and not eating, take a stamina hit
            if (thirst > requiredWaterPerDay && amount < 0)
            {
                //Take percentage off health equal to percentage above max thirst
                float lostHealth = newPercent * (float)Game1.player.maxHealth;
                Game1.player.health = Game1.player.health + (int)lostHealth;
                thirst = requiredWaterPerDay;
                newPercent = (requiredWaterPerDay - thirst) / requiredWaterPerDay;
            }

            Warn(oldPercent, newPercent, THIRST_THRESHOLDS, THIRST_MESSAGES, false);
        }

        //Apply changes to the Maximum capacity of the bladder, and the rate at which it fills.
        //Note that Positive percent is a LOSS of continence
        public void ChangeBladderContinence(float percent = 0.01f)
        {
            float previousContinence = bladderContinence;

            //Modify the continence factor (inversely proportional to rate at which the bladder fills)
            bladderContinence -= percent;

            //Put a ceiling at 100%, and  a floor at 5%
            bladderContinence = Math.Max(Math.Min(bladderContinence, 1f), 0.05f);

            //Decrease our maximum capacity (bladder shrinks as we become incontinent)
            bladderCapacity = bladderContinence * maxBladderCapacity;

            //Ceiling at base value and floor at 25% base value
            bladderCapacity = Math.Max(bladderCapacity, minBladderCapacity);

            //If we're increasing, no need to warn. (maybe we should tell people that they're regaining?)
            if (percent <= 0)
                return;

            //Warn that we may be losing control
            Warn(previousContinence, bladderContinence, BLADDER_CONTINENCE_THRESHOLDS, BLADDER_CONTINENCE_MESSAGES, true);
        }

        //Apply changes to the Maximum capacity of the bowels, and the rate at which they fill.
        public void ChangeBowelContinence(float percent = 0.01f)
        {
            float previousContinence = bowelContinence;

            //Modify the continence factor (inversely proportional to rate at which the bowels fills)
            bowelContinence -= percent;

            //Put a ceiling at 100%, and  a floor at 5%
            bowelContinence = Math.Max(Math.Min(bowelContinence, 1f), 0.05f);

            //Decrease our maximum capacity (bowel shrinks as we become incontinent)
            bowelCapacity = bowelContinence * maxBowelCapacity;

            //Ceiling at base value and floor at 25% base value
            bowelCapacity = Math.Max(bowelCapacity, minBowelCapacity);

            //If we're increasing, no need to warn. (maybe we should tell people that they're regaining?)
            if (percent <= 0)
                return;

            //Warn that we may be losing control
            Warn(previousContinence, bowelContinence, BOWEL_CONTINENCE_THRESHOLDS, BOWEL_CONTINENCE_MESSAGES, true);
        }

        //Put on underwear and clean pants
        private Container ChangeUnderwear(Container container)
        {
            Container oldUnderwear = this.underwear;
            Container newPants;
            if (!oldUnderwear.removable)
                Animations.Warn(Regression.t.Change_Destroyed, this);
            this.underwear = container;
            Regression.t.Underwear_Options.TryGetValue("blue jeans", out newPants);
            pants = new Container(newPants);
            CleanPants();
            Animations.Say(Regression.t.Change, this);
            return oldUnderwear;
        }

        public Container ChangeUnderwear(Underwear uw)
        {
            return ChangeUnderwear(new Container(uw.container.name, uw.container.wetness, uw.container.messiness, uw.container.durability));
        }

        public Container ChangeUnderwear(string type)
        {
            Container newPants, refPants;
            Regression.t.Underwear_Options.TryGetValue("type", out refPants);
            newPants = new Container(refPants);
            newPants.messiness = 0;
            newPants.wetness = 0;
            return ChangeUnderwear(newPants);
        }

        //If we put on our pants, remove wet/messy debuffs
        public void CleanPants()
        {
            RemoveBuff(WET_DEBUFF);
            RemoveBuff(MESSY_DEBUFF);
        }

        //Debug Function, Add a bit of everything
        public void DecreaseEverything()
        {
            AddWater(requiredWaterPerDay * -0.1f, 0f);
            AddFood(requiredCaloriesPerDay * -0.1f, 0f);
            AddBladder(maxBladderCapacity * -0.1f);
            AddBowel(maxBladderCapacity * -0.1f);
        }

        public void IncreaseEverything()
        {
            AddWater(requiredWaterPerDay * 0.1f, 0f);
            AddFood(requiredCaloriesPerDay * 0.1f, 0f);
            AddBladder(maxBladderCapacity * 0.1f);
            AddBowel(maxBladderCapacity * 0.1f);
        }

        public void DrinkWateringCan()
        {
            Farmer player = Game1.player;
            WateringCan currentTool = (WateringCan)player.CurrentTool;
            if (currentTool.WaterLeft * 100 >= thirst)
            {
                this.AddWater(thirst);
                currentTool.WaterLeft -= (int)(thirst / 100f);
                Animations.AnimateDrinking(false);
            }
            else if (currentTool.WaterLeft > 0)
            {
                this.AddWater(currentTool.WaterLeft * 100);
                currentTool.WaterLeft = 0;
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
            this.AddWater(thirst);
            Animations.AnimateDrinking(true);
        }

        public bool InToilet(bool inUnderwear)
        {
            return !inUnderwear && (Game1.currentLocation is FarmHouse || Game1.currentLocation is JojaMart || Game1.currentLocation is Club || Game1.currentLocation is MovieTheater || Game1.currentLocation is IslandFarmHouse || Game1.currentLocation.Name == "Saloon" || Game1.currentLocation.Name == "Hospital" || Game1.currentLocation.Name == "BathHouse_MensLocker" || Game1.currentLocation.Name == "BathHouse_WomensLocker");
        }

        public void Mess(bool voluntary = false, bool inUnderwear = true)
        {
            numPottyPooAtNight = 0;
            numAccidentPooAtNight = 0;
            //If we're sleeping check if we have an accident or get up to use the potty
            if (isSleeping)
            {
                //When we're sleeping, our bowel fullness can exceed our capacity since we calculate for the whole night at once
                //Hehehe, this may be evil, but with a smaller bladder, you'll have to pee multiple times a night
                //So roll the dice each time >:)
                //<TODO>: Give stamina penalty every time you get up to go potty. Since you disrupted sleep.
                int numMesses = (int)((bowelFullness - GetBowelAttemptThreshold()) / bowelCapacity);
                float additionalAmount = bowelFullness - (numMesses * bowelCapacity);
                int numAccidents = 0;
                int numPotty = 0;

                if (additionalAmount > 0)
                    numMesses++;

                for (int i = 0; i < numMesses; i++)
                {
                    //Randomly decide if we get up. Less likely if we have lower continence
                    bool lclVoluntary = voluntary || Regression.rnd.NextDouble() < (double)this.bowelContinence;
                    StartWetting(lclVoluntary && underwear.removable, true); //Always in underwear in bed
                    float amountToLose = (i != numMesses - 1) ? bowelCapacity : additionalAmount;
                    if (!lclVoluntary)
                    {
                        numAccidents++;
                        //Any overage in the container, add to the pants. Ignore overage over that.
                        //When sleeping, the pants are actually the bed
                        _ = this.bed.AddPoop(this.pants.AddPoop(this.underwear.AddPoop(amountToLose)));
                        bowelFullness -= amountToLose;

                    }
                    else
                    {
                        numPotty++;
                        bowelFullness -= amountToLose;
                        if (!underwear.removable) //Certain underwear can't be taken off to use the toilet (ie diapers)
                        {
                            _ = this.bed.AddPoop(this.pants.AddPoop(this.underwear.AddPoop(amountToLose)));
                            numAccidents++;
                        }
                    }
                }
                numPottyPooAtNight = numPotty;
                numAccidentPooAtNight = numAccidents;
            }
            else if (inUnderwear)
            {

                StartMessing(voluntary, true); //Always in underwear in bed
                                               //Any overage in the container, add to the pants. Ignore overage over that.
                if (bowelFullness >= GetBowelAttemptThreshold())
                {
                    _ = this.pants.AddPoop(this.underwear.AddPoop(bowelFullness));
                    this.bowelFullness = 0.0f;
                }
            }
            else
            {
                StartMessing(voluntary, false);
                if (underwear.removable)
                {
                    if (bowelFullness >= GetBowelAttemptThreshold())
                    {
                        this.bowelFullness = 0.0f;
                    }
                }
            }
        }

        public void StartMessing(bool voluntary = false, bool inUnderwear = true)
        {
            if (!Regression.config.Messing)
                return;

            if (bowelFullness < GetBowelAttemptThreshold())
            {
                Animations.AnimatePoopAttempt(this, inUnderwear);
            }
            else
            {

                //If we have an accident (not voluntary), decrease continence
                //If we use the potty before we REALLY have to go (we go before we reach some threshold), increase continence
                //Otherwise, if it is voluntary but waited until we almost had an accident (fullness above some threshold) don't change anything
                if (!voluntary)
                    this.ChangeBowelContinence(0.01f * Regression.config.BowelLossContinenceRate);
                else if (bowelFullness < GetBowelTrainingThreshold())
                    this.ChangeBowelContinence(-0.01f * Regression.config.BowelGainContinenceRate);

                Animations.AnimateMessingStart(this, voluntary, inUnderwear);
            }

            Animations.AnimateMessingEnd(this);
            if (!this.InToilet(inUnderwear))
                _ = Animations.HandleVillager(this, true, inUnderwear, pants.messiness > 0.0, false, 20, 3);
            if (pants.messiness <= 0.0 || !inUnderwear)
                return;
            HandlePoopOverflow();
        }

        private void HandlePoopOverflow()
        {
            if (isSleeping)
                return;

            Animations.Write(Regression.t.Poop_Overflow, this, Animations.poopAnimationTime);
            float howMessy = pants.messiness / pants.containment;
            int speedReduction = howMessy >= 0.5 ? (howMessy > 1.0 ? -3 : -2) : -1;
            Buff buff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, speedReduction, 0, 0, 15, "", "")
            {
                description = string.Format("{0} {1} Speed.", Strings.RandString(Regression.t.Debuff_Messy_Pants), (object)speedReduction),
                millisecondsDuration = 1080000,
                glow = Color.Brown,
                sheetIndex = -1,
                which = MESSY_DEBUFF
            };
            if (Game1.buffsDisplay.hasBuff(MESSY_DEBUFF))
                this.RemoveBuff(MESSY_DEBUFF);
            Game1.buffsDisplay.addOtherBuff(buff);
        }

        public void StartWetting(bool voluntary = false, bool inUnderwear = true)
        {

            if ((double)bladderFullness < GetBladderAttemptThreshold())
            {
                Animations.AnimatePeeAttempt(this, inUnderwear);
            }
            else
            {
                //If we have an accident (not voluntary), decrease continence
                //If we use the potty before we REALLY have to go (we go before we reach some threshold), increase continence
                //Otherwise, if it is voluntary but waited until we almost had an accident (fullness above some threshold) don't change anything
                if (!voluntary)
                    this.ChangeBladderContinence(0.01f * Regression.config.BladderLossContinenceRate);
                else if(bladderFullness < GetBladderTrainingThreshold())
                    this.ChangeBladderContinence(-0.01f * Regression.config.BladderGainContinenceRate);
                Animations.AnimateWettingStart(this, voluntary, inUnderwear);
            }

            Animations.AnimateWettingEnd(this);
            if (!this.InToilet(inUnderwear))
                _ = Animations.HandleVillager(this, false, inUnderwear, pants.wetness > 0.0, false, 20, 3);
            if ((pants.wetness <= 0.0 || !inUnderwear))
                return;
            HandlePeeOverflow();
        }
        private void HandlePeeOverflow()
        {
            if (isSleeping)
                return;

            Animations.Write(Regression.t.Pee_Overflow, this, Animations.peeAnimationTime);

            int defenseReduction = -Math.Max(Math.Min((int)(pants.wetness / pants.absorbency * 10.0), 10), 1);
            Buff buff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, defenseReduction, 0, 15, "", "")
            {
                description = string.Format("{0} {1} Defense.", Strings.RandString(Regression.t.Debuff_Wet_Pants), defenseReduction),
                millisecondsDuration = 1080000
            };
            buff.glow = pants.messiness != 0.0 ? Color.Brown : Color.Yellow;
            buff.sheetIndex = -1;
            buff.which = WET_DEBUFF;
            if (Game1.buffsDisplay.hasBuff(WET_DEBUFF))
                this.RemoveBuff(WET_DEBUFF);
            Game1.buffsDisplay.addOtherBuff(buff);
        }

        public void Wet(bool voluntary = false, bool inUnderwear = true)
        {
            if (!Regression.config.Wetting)
                return;

            numPottyPeeAtNight = 0;
            numAccidentPeeAtNight = 0;
            //If we're sleeping check if we have an accident or get up to use the potty
            if (isSleeping)
            {
                //When we're sleeping, our bladder fullness can exceed our capacity since we calculate for the whole night at once
                //Hehehe, this may be evil, but with a smaller bladder, you'll have to pee multiple times a night
                //So roll the dice each time >:)
                int numWettings = (int)((bladderFullness - GetBladderAttemptThreshold()) / bladderCapacity);
                float additionalAmount = bladderFullness - (numWettings * bladderCapacity);
                int numAccidents = 0;
                int numPotty = 0;

                if (additionalAmount > 0)
                    numWettings++;

                for (int i = 0; i < numWettings; i++)
                {
                    //Randomly decide if we get up. Less likely if we have lower continence
                    bool lclVoluntary = voluntary || Regression.rnd.NextDouble() < (double)this.bladderContinence;
                    StartWetting(lclVoluntary && underwear.removable, true); //Always in underwear in bed
                    float amountToLose = (i != numWettings - 1) ? bladderCapacity : additionalAmount;
                    if (!lclVoluntary)
                    {
                        numAccidents++;
                        //Any overage in the container, add to the pants. Ignore overage over that.
                        //When sleeping, the pants are actually the bed
                        _ = this.bed.AddPee(this.pants.AddPee(this.underwear.AddPee(amountToLose)));
                        bladderFullness -= amountToLose;

                    }
                    else
                    {
                        numPotty++;
                        bladderFullness -= amountToLose;
                        if (!underwear.removable) //Certain underwear can't be taken off to use the toilet (ie diapers)
                        {
                            _ = this.bed.AddPee(this.pants.AddPee(this.underwear.AddPee(amountToLose)));
                            numAccidents++;
                        }
                    }
                }
                numPottyPeeAtNight = numPotty;
                numAccidentPeeAtNight = numAccidents;
            }
            else if (inUnderwear)
            {
                StartWetting(voluntary, true);
                //Any overage in the container, add to the pants. Ignore overage over that.
                if (bladderFullness >= GetBladderAttemptThreshold())
                {
                    _ = this.pants.AddPee(this.underwear.AddPee(bladderFullness));
                    this.bladderFullness = 0.0f;
                }
            } else
            {
                StartWetting(voluntary, false);
                if (underwear.removable) {
                    if (bladderFullness >= GetBladderAttemptThreshold())
                    {
                    this.bladderFullness = 0.0f;
                    }
                }
            }
            if (bladderFullness < 0) bladderFullness = 0;
        }

        public void HandleMorning()
        {
            isSleeping = false;
            if (Regression.config.Easymode)
            {
                hunger = 0;
                thirst = 0;
                bed.dryingTime = 0;
            }
            else
            {

                Farmer player = Game1.player;
                if (bed.messiness > 0.0 || bed.wetness > 0.0)
                {
                    bed.dryingTime = 1000;
                    player.stamina -= 20f;
                }
                else if (bed.wetness > 0.0)
                {
                    bed.dryingTime = 600;
                    player.stamina -= 10f;
                }
                else
                    bed.dryingTime = 0;

                int timesUpAtNight = Math.Max(numPottyPeeAtNight, numPottyPooAtNight);
                player.stamina -= (timesUpAtNight * wakeUpPenalty);

            }

            Animations.AnimateMorning(this);
            bed.Wash();
        }

        public void HandleNight()
        {
            isSleeping = true;
            if (bedtime <= 0)
                return;

            //How long are we sleeping? (Minimum of 4 hours)
            const int timeInDay = 2400;
            const int wakeUpTime = timeInDay + 600;
            const float sleepRate = 3.0f; //Let's say body functions change @ 1/3 speed while sleeping. Arbitrary.
            int timeSlept = wakeUpTime - bedtime; //Bedtime will never exceed passout-time of 2:00AM (2600)
            HandleTime(timeSlept / 100.0f / sleepRate);
        }

        //If Stamina has decreased, Use up Food and water along with it
        public void HandleStamina()
        {
            float staminaDifference = (float)(Game1.player.stamina - this.lastStamina) / Game1.player.maxStamina.Value;
            if ((double)staminaDifference == 0.0)
                return;
            if (staminaDifference < 0.0)
            {
                this.AddFood( staminaDifference * requiredCaloriesPerDay * 0.25f);
                this.AddWater(staminaDifference * requiredWaterPerDay    * 0.10f);
            }
            this.lastStamina = Game1.player.stamina;
        }


        public void HandleTime(float hours)
        {
            this.HandleStamina();
            //normally divide 24hr/day, but this only happens while awake,
            //We have night set to go at 1/3 rate. Assume 8hr sleep. So we need to adjust by 8*(2/3)
            this.AddWater((float)(requiredWaterPerDay * (double)hours / -18.67));
            this.AddFood((float)(requiredCaloriesPerDay * (double)hours / -18.67));
        }

        public bool IsFishing()
        {
            FishingRod currentTool;
            return (currentTool = Game1.player.CurrentTool as FishingRod) != null && (currentTool.isCasting || currentTool.isTimingCast || (currentTool.isNibbling || currentTool.isReeling) || currentTool.castedButBobberStillInAir || currentTool.pullingOutOfWater);
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

        public void Warn(float oldPercent, float newPercent, float[] thresholds, string[][] msgs, bool write = false)
        {
            if (isSleeping)
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

        //<TODO> Expand Consumables to add food. But we'd need a lot more info. For now, treat all food the same.
        public void Consume(string itemName)
        {
            Consumable item;
            if(Animations.GetData().Consumables.TryGetValue(itemName, out item))
            {
                this.AddFood(item.calorieContent);
                this.AddWater(item.waterContent);
            } else
            {
                this.AddFood(400);
                this.AddWater(10);
            }
        }
    }
}
