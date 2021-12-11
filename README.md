# Stardew Valley Regression Mod
Ported version of abandoned Mod for Stardew Valley that added a system for hunger, thirst and incontinence.

## Authorship and Attributions
I am unable to locate the original author of this code, who posted it back in 2017. However, there is a list of contributors and playtesters inside one of the files, identified by their reddit.com usernames.

### Thank you to the identified previous maintainers
- Fox Tale Times User: oniryuuko
- Fox Tale Times User: SabrinaM
- Fox Tale Times User: Jon

### Thanks to contributors:
- u/BabyKunoichi
- u/abdlnikki
- u/FurryDestiny

### Thanks to alpha testers:
- u/zombiekarasu
- u/dr_daddy
- u/errycupid
- u/rosebush413
- u/mrnogee
- u/crinklycuddles
- u/vulpix77

## Installation Instructions
### Prerequisites
- Stardew Valley (version 1.5.5-beta)
- SMAPI (version 3.13.0-beta.20211018)
- PyTk (version 1.22.8)

## Installation
1) Download the latest release. Be careful that you don't download the source code. The release is the one that contains Regression.dll. (https://github.com/zippity21/Stardew_Valley_Regression_Mod/releases)
2) Unzip contents of the release into the Mods directory created during SMAPI installation. The directory  with the DLL should looks like <Stardew Valley Installation Directory>\Mods\Regression\Regression.dll

## Controls (Changed compared to original mod)
### Typical
- F1: Attempt to Pee in Pants
- F2: Attempt to Poop Pants
- F4: (Reserved by Game for Screenshot Mode)
- F5: Check State of Underwear
- F6: Check State of Pants
- F9: Toggle Debug Mode
- (Use Left shift to pull down pants. If in your house, it'll be considered making it to the toilet. Elsewhere, its on the ground.)
- Left Shift + F1: Pull down pants and attempt to pee.
- Left Shift + F2: Pull down pants and attempt to poop.
- Left Shift + Right Click: Drink when near water source or holding watering can.

### In Debug
- Left Alt + F1: Make more thirsty and hungry
- Left Alt + F2: Make less thirsy and hungry. Make Bladder and Bowels fuller.
- Left Alt + F3: Open a menu with all valid underwear types. ( + some wine ;) )
- Left Alt + F4: (Reserved by Windows to force quit)
- Left Alt + F5: Fast forward time a bit (do not hold down. Wait for fast forward to finish before pressing again.)
- Left Alt + F6: Toggle Wetting Option
- Left Alt + F7: Toggle Messing Option
- Left Alt + F8: Toggle Easy Mode Option
- Left Alt + W: Increase Bladder Continence
- Left Alt + Left Shift + W: Increase Bowel Continence
- Left Alt + S: Decrease Bladder Incontinence
- Left Alt + Left Shift + S: Decrease Bowel Continence

## Basic Mechanics
You now get hungry and thristy over time, and from doing work. Getting too hungry/thirsty gives you de-buffs. You can become less hungry by eating food, and less thirst by drinking beverages (including water from a water source, your watering can, or things like wine, coffee, etc.).

Of course, that food and water have to go somewhere :). You now also have to worry about having to go potty. Your bladder and bowels increase naturally over time, and will increase more/faster depending on what you eat/drink. Currently the only toilet is in your house, so if you're out and about, you have a few options:
- Try to hold it and run home to use the potty like a big boy/girl.
- Pull down your pants and go right where you are. (watch out if anyone can see/smell you)
- Go in your pants! Of course, if you want to do that, you may want to consider some protection. (Available at the seed shop.)
 - If you do this too much, you may just find that it becomes a little harder to stop yourself. If you even notice, that is.

## Issues and Ideas
Please feel free to ask questions, report bugs, and request features on the GitHub page.
