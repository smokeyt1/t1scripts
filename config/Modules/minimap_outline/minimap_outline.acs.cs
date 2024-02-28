// Minimap Outline
// Smokey
// v0.1

function MinimapOutline::Init() {
	if($MinimapOutline:Loaded)
		return;
	$MinimapOutline:Loaded = true;

	HUD::New( "MinimapOutline::Container", 0, 0, 401, 401, MinimapOutline::WakeSleep, MinimapOutline::WakeSleep );
	newObject("MinimapOutline::Texture", FearGuiFormattedText, 0, 0, 401, 401);
	HUD::Add("MinimapOutline::Container","MinimapOutline::Texture");

    MinimapOutline::Reset();
}

function MinimapOutline::WakeSleep() { }

function MinimapOutline::Reset() {
    %filename = "minimap_outline_" @ $pref::miniMapWidth @ ".png";

    if(isFile("config/Modules/minimap_outline/" @ %filename)) {
        Control::SetValue("MinimapOutline::Texture", "<B0,0:Modules/minimap_outline/" @ %filename @ ">");
    } else {
        Control::SetValue("MinimapOutline::Texture", "");
    }

}

MinimapOutline::Init();
