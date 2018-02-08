using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Language{Russian, English};

public static class Localization {
	public static string item_button_drop, general_use;
	public static Language currentLanguage;

	public static void ChangeLanguage(Language lan ) {
		if (lan == currentLanguage) return;
		switch (lan) {
		case  Language.Russian: 
			item_button_drop = "Выбросить";
			general_use = "Использовать";
			break;
		case Language.English:
			item_button_drop = "Drop";
			general_use = "Use";
			break;
		}
		currentLanguage = lan;
	}
}
