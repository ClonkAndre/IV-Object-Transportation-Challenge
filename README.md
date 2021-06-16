# About this mod 
The IV-Object-Transportation-Challenge Mod, is a mod where you can play mission like the **#IVFlowerPotChallenge** by Rob or create mission like the **#IVFlowerPotChallenge**. It was inspired by the funny little challenge made by Rob called the **#IVFlowerPotChallenge**.  

Everyone can create their own missions and share them around the world. It's easy!  
Every challenge works in the same way: There is a initial object location where you can find the object that is needed to complete the challenge. And then there is a delivery point, where you have to bring the needed object to. 

Press F9 to open the menu. (Keybind can be changed in the .ini file)  

# #IVFlowerPotChallenge Info 
You dont know what the **#IVFlowerPotChallenge** is?  
Watch [this](https://youtu.be/A8illRd3KWE) video.

# How do i create a mission? 
Open the menu (default F9) and click on "Create new mission". Now the menu will close and you are in the mission editor. In the upper left corner of the screen you will have some shortcut informations.  

Open the menu again to edit your mission/challenge settings.  
- Mission name: The name of your mission  
- Are vehicles allowed: This sets if vehicles are allowed in your challenge. If you want to use vehicles, press on the button and it should be **green**. That means it is on. In the "textbox" below, you can enter the models names of the vehicles that are allowed for your challenge, enter "**ALL**" to allow all vehicles.  
- Is player wanted after picking up object: If **true**, the player will be wanted by police with the entered amount of start in the "textbox" below. (Please **only** enter numbers from 0 - 6, otherwise the game might **crash**).  
- If player enters a vehicle that is not allowed, in how many seconds should the mission fail?: If the player enters a vehicle that is **not** allowed a countdown will appear, with the amount of seconds you entered. (The recommended value for the minimum is **5**).  
- Remove all weapons on mission start: Removes **all** weapons on mission start.  
- Object textbox: The object that is **needed** for the mission.  
- initial location of the object: The **initial location** where the object can be found.  
- Finish location: Where the player **needs** to go to finish your challenge.  

### Shortcut info  
- CNTRL + O: Copies the **current object** that the player has in his hands in to the "Object textbox" (hexadecimal hash).  
- CNTRL + I: Copies the **object inital position** from the players **current position** in to the "initial location of the object textbox" (custom vector3 value).  
- CNTRL + P: Copies the **finish position** from the players **current position** in to the "Finish location textbox" (custom vector3 value).  

### Editors button info  
- Discard mission: The current mission/challenge you are working on will be **discarded** and the editor will **close**. So, if you just want to close the window, hit the **X** button in the upper right corner of the window.  
- CREATE MISSION: Creates the mission. (Mission path: GTAIV\Scripts\IVObjectTransportationChallenge\Missions)  

## How to Contribute
Do you have an idea to improve this mod, or did you happen to run into a bug? Please share your idea or the bug you found in the **[issues page](https://github.com/ClonkAndre/DeathMusicIV/issues)**, or even better: feel free to fork and contribute to this project with a **[Pull Request](https://github.com/ClonkAndre/DeathMusicIV/pulls)**. Feel free to take the source code and do whatever you want to do with it.  

Make sure you have **Visual Studio** installed, and that you add a **reference** to the file "**ScriptHookDotNet.dll**" in **Visual Studio**, otherwise you will run in to a hole lot of **errors**.

If you dont have the **ScriptHookDotNet.dll** file, then here is a link for you to download this file: https://www.dropbox.com/s/9mhvnmy101aspkw/ScriptHookDotNet.dll?dl=1
