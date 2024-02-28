// Revised version based on soundtrack.acs.cs
// Smokey
// 0.1

$Music::verbose = "False";
$Music::ignoremapsongs = "True";

function Music::Init(){
	if ($Music::Loaded)
		return;

    if(!isFunction(gmp3::status)) {
        echo("GMp3.dll not loaded.");
        return;
    }

    $cdTrack = 0;

    $Music::Loaded = true;
}

function Music::onLeaveServer() {
    gmp3::stop();
    Schedule::Cancel("Music::playloop();");
}

function Music::Play() {
    if ($gmp3::x1 == "") gmp3::list();

    $cdTrack = floor(getRandom() * ($gmp3::songs + 1));
    gmp3::load($gmp3::x[$cdTrack]);
    gmp3::setvolume((1-$pref::CDVolume)*40);

    remoteEP("<f2>Music HUD: <f1>Playing Track " @ $cdTrack, 3, 2, 2, 10, 300);

    gmp3::play();
    Music::playloop();
}

function Music::Toggle() {
    switch(gmp3::status()) {
		case "PLAYING":
            remoteEP("<f2>Music HUD: <f1>Playing Stopped", 3, 2, 2, 10, 300);
			gmp3::stop();
            Schedule::Cancel("Music::playloop();");
			break;
		case "NOT LOADED":
            Music::Play();
			break;
		default :
            Music::Play();
			break;
	}
}

function Music::playloop()
{
    if ($Music::verbose) echoc(2,"check for song ending");

	switch(gmp3::status()) {
		case "PLAYING":
			schedule::add("Music::playloop();", 5);
			break;
		case "FINISHED":
			if ($Music::verbose) echoc(3,"song finished, playing next");
			Music::Next();
			schedule::add("Music::playloop();", 5);
			break;
		case "NOT LOADED":
			if ($Music::verbose) echoc(2,"no song loaded, starting loop");
			Music::Play();
			schedule::add("Music::playloop();", 5);
			break;
		default :
			if ($Music::verbose) echoc(1,"not playing, aborting loop - status: ", gmp3::status() );
			break;
	}
}

function Music::Next() {
    if ($cdTrack == $gmp3::songs)
        $cdTrack = 1;
    else
        $cdTrack++;

    remoteEP("<f2>Music HUD: <f1>Playing Track " @ $cdTrack, 3, 2, 2, 10, 300);
    gmp3::load($gmp3::x[$cdTrack]);
    gmp3::setvolume((1-$pref::CDVolume)*40);
    gmp3::play();
}

function Music::Prev() {
    if ($cdTrack == 1)
        $cdTrack = $gmp3::songs;
    else
        $cdTrack--;

    remoteEP("<f2>Music HUD: <f1>Playing Track " @ $cdTrack, 3, 2, 2, 10, 300);
    gmp3::load($gmp3::x[$cdTrack]);
    gmp3::setvolume((1-$pref::CDVolume)*40);
    gmp3::play();
}

// restore cd volume slider functionality to options.cs
function setvolguy() after OptionsSoundCDVolume::onAction {
    $pref::cdVolume = (Control::getValue(OptionsSoundCDVolume));
    gmp3::setvolume((1-$pref::CDVolume)*40);
}

// restore cd music togglebox functionality to options.cs
function setcdmusic() after OptionsSoundCDMusic::onAction {
    if ($pref::CDMusic)
    {
        Music::Play();
    }
    else
    {
        gmp3::stop();
        Schedule::Cancel("Music::playloop();");
    }
}

// restore play certain songs for certain maps func
function remoteSetMusic(%player, %track, %mode) {
    if ($Music::verbose) echoc(2,"remoteSetMusic setting Track:"~ %track ~" Player:" ~ %player ~" Mode:" ~ %mode );

    if(!$Music::ignoremapsongs)
    {
        $cdPlayMode = %mode; // im not using this yet
        $cdTrack = %track - 1; // -1 cause yeah

        if ($pref::CDMusic)
        {
            if ($gmp3::x1 == "") gmp3::list();
            gmp3::load($gmp3::x[$cdTrack]);
            gmp3::setvolume((1-$pref::CDVolume)*40);
            gmp3::play();
            Music::playloop();
        }
    }
}

Event::Attach( eventLeaveServer, Music::onLeaveServer );

EditActionMap("actionMap.sae");
bindCommand(keyboard0, make, "f11", TO, "Music::Next();");
bindCommand(keyboard0, make, control, "f11", TO, "Music::Prev();");
bindCommand(keyboard0, make, "f12", TO, "Music::Toggle();");

Music::Init();
