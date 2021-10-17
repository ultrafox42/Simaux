## Simaux
# Simulator Auxilarity Tool to manage Msfs2020 pop out windows

Simaux saves the positions of all pop out windows to automatically restore them on the next flight. 
The tool automatically recognizes which aircraft is loaded and therefore always uses the correct profile.
A separate file is saved for each aircraft.

I tested the tool with G1000 / G3000 / G5000, the backup instruments and the special MDF/PDF in the Cessna Longitude,
Especially the bottom mounted MDF and PDF programming screens needed all my attention.

All windows from the toolbar (ATC, VFR map, etc.) should also work.

# SysTray application
Simaux is a pure SysTray application. That means there is no program window. Instead there is a NotifyIcon in the Systray.
If you click with the right mouse button on this icon, a popup menu appears.

* Set PopOut Positions Win+Ctrl+Home
* Save PopOut Positions Win+Ctrl+Print
* Avoid rendering when inactive Win+Ctrl+Insert

Point 1 is only enabled if point 2 was used before. 
Point 3 can be used to enable/disable a mode that prevents Msfs rendering when Msfs is not the active application.
This is very useful if you actually want to pause the flight to do something else, but don’t want to exit Msfs completely.
The function saves power, but has nothing to do with the actual function regarding window positioning.

By the way: As you can see, all menu items can be reached via ShortCut (see above). Selecting the functions via ShortCut,
is also recommended by me.

# Procedure (save positions):

Start the tool at any time - no matter if the FS is already running or not.
position your FS windows as you want them - even on multiple screens.
when everything is positioned, choose point 2 (Save PopOut Positions).
That’s it regarding saving the positions.

# Procedure (Restore positions):

Get all desired PopOuts from the Flightsimulator,
detach these windows one by one from the combination window created by the FS (important: the PopOut you attached first should be detached last).
It doesn’t matter where you place the windows or if they are on top of each other.
Select menu item 1 (or Win+Ctrl+Home) to position the PopOuts automatically.

# Tips:
when PopOuting you should make sure that the content is reasonably correct regarding the aspect ratio. Don’t have too big edges at the top, bottom or sides.
you may need to download Microsoft.NET 4 from Microsoft for the program to work. First test if it starts for you, if not, please download Microsoft.NET 4.


I would be glad about feedback from you.
