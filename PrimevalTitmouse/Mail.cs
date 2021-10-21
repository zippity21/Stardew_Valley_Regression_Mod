using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace PrimevalTitmouse
{
    public static class Mail
    {
        private static string currentLetter;
        private static string nextLetterId;
        private static List<Item> nextLetterItems;
        private static string nextLetterText;
        public static bool showingLetter;
        public static IModHelper helper;

        public static void CheckMail(Body b)
        {
            if (!b.lettersReceived.Contains("jodi"))
            {
                nextLetterId = "jodi";
                nextLetterText = "Dear $FARMERNAME$,^Welcome to town! Here are some veggies from the garden to tide you over while you move in! Also, I feel it's important to note that there's an... odd effect around here. You'll see what I mean soon enough, probably, but I've still enclosed some supplies. Visit Pierre if you run out.^      <, Jodi";
                if (Regression.config.Easymode)
                {
                    List<Item> objList = new List<Item>();
                    objList.Add(new StardewValley.Object(399, 20, false, -1, 0));
                    objList.Add(new Underwear("pawprint diaper", 0.0f, 0.0f, 40));
                    nextLetterItems = objList;
                }
                else
                {
                    List<Item> objList = new List<Item>();
                    objList.Add(new StardewValley.Object(399, 20, false, -1, 0));
                    objList.Add(new Underwear("lavender pullup", 0.0f, 0.0f, 15));
                    objList.Add(new Underwear("pawprint diaper", 0.0f, 0.0f, 3));
                    nextLetterItems = objList;
                }
            }
            else
                nextLetterId = (string)null;
            if (nextLetterId != null && !(Game1.mailbox.Contains("robinWell")))
                Game1.mailbox.Insert(0, "robinWell");
            if ((Game1.mailbox.Count > 0))
                currentLetter = Game1.mailbox[0];
            else
                currentLetter = "none";
        }

        private static void OnMenuClosed(object sender, MenuChangedEventArgs e)
        {
            nextLetterId = null;
            currentLetter = Game1.mailbox.Count <= 0 ? "none" : Game1.mailbox[0];
            helper.Events.Display.MenuChanged -= new EventHandler<MenuChangedEventArgs>(OnMenuClosed);
            Game1.activeClickableMenu = new ItemGrabMenu(nextLetterItems);
            showingLetter = false;
        }

        public static void ShowLetter(Body b, IModHelper h)
        {
            helper = h;
            if (nextLetterId != null && currentLetter == "robinWell")
            {
                showingLetter = true;
                if (nextLetterId == "jodi")
                    b.lettersReceived.Add("jodi");
                Game1.activeClickableMenu = new LetterViewerMenu(Strings.InsertVariables(nextLetterText, b, null), nextLetterId);
                helper.Events.Display.MenuChanged += new EventHandler<MenuChangedEventArgs>(OnMenuClosed);
            }
            else if (Game1.mailbox.Count > 0)
                currentLetter = Game1.mailbox[0];
            else
                currentLetter = "none";
        }
    }
}
