// TV2 for Tribes 1.40/1.41
// Install to config/Modules
// By Smokey
// v0.2
//
// Enhanced TV script with additional features:
// - Game binds in Options menu
// - Player scrolling with scroll wheel
// - Menu toggle feature
// - Additional fixes
//

function TV2::GameBinds::Init() after GameBinds::Init
{
	$GameBinds::CurrentMapHandle = GameBinds::GetActionMap2( "playMap.sae");
	$GameBinds::CurrentMap = "playMap.sae";
	GameBinds::addBindCommand( "TV2 Toggle Menu", "TV2::ToggleMenu();");
	GameBinds::addBindCommand( "TV2 Observe Carrier", "TV2::Carrier();", "TV2::Cancel();");
}

function TV2::Init() {

    $TV2::MenuEnabled = false;
    $TV2::Observing = false;
	$TV2::Key = false;
    $TV2::FlagCarrier = "";
	$TV2::ClientID = "";

}

function TV2::ToggleMenu() {

    if ($TV2::Observing) {
		TV2::Cancel();
		return;
	}

    if ($TV2::MenuEnabled) {
		$TV2::MenuEnabled = false;
		PopActionMap("TV2.sae");
		remoteBP(2048, "", 0);
		return;
	}

    $TV2::MenuEnabled = true;

    NewActionMap("TV2.sae");

    %MenuString = "<jc><f0>TV2 - Select Player or Scroll Up/Down\n";
    %MyTeamID = Client::GetTeam(getManagerID());
    %Count = 0;

    for (%ClientID = 2048; %ClientID < 2180; %ClientID++) {

        %ClientTeamID = Client::GetTeam(%ClientID);

		if (Client::GetName(%ClientID) != "" && %ClientTeamID == %MyTeamID) {

			%Count++;

			if (%FirstClient == "") %FirstClient = %ClientID;

			if (%Count <= 9)
                %key = %Count;
			else if (%Count == 10)
                %key = "q";
			else if (%Count == 11)
                %key = "w";
			else if (%Count == 12)
                %key = "e";
			else if (%Count == 13)
                %key = "r";
			else if (%Count == 14)
                %key = "t";
			else if (%Count == 15)
                %key = "y";
			else if (%Count == 16)
                %key = "u";

            %MenuString = %MenuString @ "\n<jl><f1>[" ~ %key ~ "] <f1>" ~ Client::GetName(%ClientID);

			bindCommand(keyboard0, make, %key, TO, "TV2::Observe(" ~ %ClientID ~ ", true);");
			bindCommand(keyboard0, break, %key, TO, "TV2::Cancel();");

		}
	}

	bindCommand(mouse0, make, zaxis0, TO, "TV2::ObserveNext();");
	bindCommand(mouse0, make, zaxis1, TO, "TV2::ObservePrev();");

	PushActionMap("TV2.sae");

	remoteCP(2048, %MenuString, 999);

}

function TV2::Observe(%ClientID, %key) {

    if (PSC::getControlMode() != "playing" && PSC::getControlMode() != "commander" )
		return;

    if (Client::getName(%ClientID) == "") {
		remoteBP(2048, "<jc><f1>Invalid ClientID. Unable to observe!", 2);
		return;
	}

	$TV2::ClientID = %ClientID;

	if (!$TV2::Observing) {

		if (PSC::getControlMode() != "playing")
			return;

        $TV2::Observing = true;

		if (isObject("CommandGui"))
			renameObject("CommandGui", "_CommandGui");

		if (!isObject("playGui/ObsTV"))
			addToSet("playGui", newObject("ObsTV", FearGui::CMDObserve, 0, 0, 1, 1));

		CmdObserve::setFocus("playGui/ObsTV");

		remoteEval(2048, CommandMode);

		remoteEval(2048, "scom", -1);

		Schedule::Add("CursorOff(mainwindow);", 0.3);

		Client::ToggleCmdObserver("True");

	}

	if ($TV2::Key || %key) {

		$TV2::Key = true;

		remoteBP(2048, "<jc><f1>Observing: <f2>" ~ String::escapeFormatting(Client::getName(%ClientID)) ~ "\n\n<f1> Release key to exit.", 999);

	} else {

		remoteBP(2048, "<jc><f1>Observing: <f2>" ~ String::escapeFormatting(Client::getName(%ClientID)) ~ "\n\n<f1> Press <f2>[SPACE] <f1> or <f2>[TOGGLE MENU] <f1>to exit.", 999);
		EditActionMap("TV2.sae");
		bindCommand(keyboard0, make, "space", TO, "TV2::Cancel();");

	}

	Client::cmdObservePlayer(%ClientID);

}

function TV2::ObserveNext() {

	TV2::Observe(TV2::NextClient($TV2::ClientID ? $TV2::ClientID : 2047));

}

function TV2::ObservePrev() {

	TV2::Observe(TV2::PrevClient($TV2::ClientID ? $TV2::ClientID : 2180));

}

function TV2::Carrier() {

	if ($TV2::Observing) {
		TV2::Cancel();
		return;
	}

    if ($TV2::MenuEnabled) {
		$TV2::MenuEnabled = false;
		PopActionMap("TV2.sae");
        remoteBP(2048, "", 0);
	}

	if ($TV2::FlagCarrier == "") {
		remoteBP(2048, "<jc><f1>No Flag Carrier. Unable to observe!", 2);
		return;
	}

	NewActionMap("TV2.sae");
	bindCommand(mouse0, make, zaxis0, TO, "TV2::ObserveNext();");
	bindCommand(mouse0, make, zaxis1, TO, "TV2::ObservePrev();");
	PushActionMap("TV2.sae");
	TV2::Observe($TV2::FlagCarrier, true);

}

function TV2::Cancel() {

	if (!$TV2::Observing)
		return;

    if ($TV2::MenuEnabled) {
		$TV2::MenuEnabled = false;
		PopActionMap("TV2.sae");
	}

	Client::ToggleCmdObserver("False");

	remoteEval(2048, PlayMode);

	if (isObject("PlayGui/ObsTV"))
		deleteObject("PlayGui/ObsTV");

	if (isObject("_CommandGui"))
		renameObject("_CommandGui", "CommandGui");

	remoteBP(2048, "", 0);

	$TV2::Observing = false;
	$TV2::Key = false;

}

function TV2::NextClient(%CurrentID) {

	if (%CurrentID == 2179) %StartID = 2048;
	else %StartID = %CurrentID + 1;

	%MyTeamID = Client::GetTeam(getManagerID());

	for (%ClientID = %StartID; %ClientID < 2180; %ClientID++) {

        %ClientTeamID = Client::GetTeam(%ClientID);

		if (Client::GetName(%ClientID) != "" && %ClientTeamID == %MyTeamID) {
			%NextID = %ClientID;
			break;
		}

		if (%ClientID == 2179) {
			%ClientID = 2047;
		}
	}

	return %NextID;
}

function TV2::PrevClient(%CurrentID) {

	if (%CurrentID == 2048) %StartID = 2179;
	else %StartID = %CurrentID - 1;

	%MyTeamID = Client::GetTeam(getManagerID());

	for (%ClientID = %StartID; %ClientID >= 2048; %ClientID--) {

        %ClientTeamID = Client::GetTeam(%ClientID);

		if (Client::GetName(%ClientID) != "" && %ClientTeamID == %MyTeamID) {
			%NextID = %ClientID;
			break;
		}

		if (%ClientID == 2048) {
			%ClientID = 2180;
		}
	}

	return %NextID;
}

function TV2::OnFlagTaken(%team, %cl) {

	%self = getManagerId();

    if (%self == %cl) {
        $TV2::FlagCarrier = "";
        return;
    }

	if (Client::getTeam(%self) == Client::getTeam(%cl))
		$TV2::FlagCarrier = %cl;

}

function TV2::OnGuiChange(%gui) {

	if ($TV2::Observing)
		TV2::Cancel();

	if ($TV2::MenuEnabled) {
		$TV2::MenuEnabled = false;
		PopActionMap("TV2.sae");
		remoteBP(2048, "", 0);
	}

}

Event::Attach(eventConnected, TV2::Init);
Event::Attach(eventGuiClose, TV2::OnGuiChange);
Event::Attach(eventGuiOpen, TV2::OnGuiChange);
Event::Attach(eventFlagGrab, TV2::OnFlagTaken);
Event::Attach(eventFlagPickup, TV2::OnFlagTaken);
