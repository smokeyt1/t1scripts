function WeaponSwitch::nextWeapon() {
    if ($servermod == "base") {
        %weap = getitemdesc(getMountedItem(0));

        if (%weap == "Disc Launcher" && GetItemCount("Grenade Launcher") == 1) {
            use("Grenade Launcher");
        } else if (%weap != "Disc Launcher" && GetItemCount("Disc Launcher") == 1) {
            use("Disc Launcher");
        } else {
            nextWeapon();
        }
    } else {
        nextWeapon();
    }
}

EditActionMap("playMap.sae");
bindCommand(keyboard0, make, "q", TO, "WeaponSwitch::nextWeapon();");
