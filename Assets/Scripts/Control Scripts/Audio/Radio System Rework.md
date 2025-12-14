Radio System Rework

-Idea is to reduce stutter and load time for when switching radio. I imagine this happens because when we switch radio, we have to load the clip, (mp3 decompression on main thread) which essential causes the main thread to hiccup.
-So instead of 1 audio source, we will have 3 seperate sources, each representing a station.
-When we turn the radio on/intialise the radio, we will load a random track to each station, rather than make the switch then load
-When then play the loaded track when the player switches station. When the player, turns the radio off, we load another set of random songs on each station, to ensure songs are randomised.

Ended up fixing it by finding out that the hiccup is caused by decompression on main thread. I had to change some settings that allow loading on a seperate thread, which means no more hiccup when swithcing station. Thanks for the suggestion chatgpt lol. 
Im deffo getting replaced by ai.