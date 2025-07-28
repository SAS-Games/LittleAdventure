-> introduction

== introduction ==
Welcome to the Unity Localization Tutorial! In this interactive guide, you will learn the basics of localization, including setting up your project and translating text for different languages.#local:introduction

-> choose_a_topic

== choose_a_topic ==
Choose a topic to get started:#local:get_started

* [What is Localization? #local:what_is_localization] -> what_is_localization
* [Setting Up Localization in Unity #local:setting_up_localization] -> setup_localization
* [Translating Text #local:how_to_translate] -> translating_text
* [Done #local:done] -> END

== what_is_localization ==
Localization is the process of adapting your game for different regions, cultures, and languages. This involves translating text, adjusting date formats, adapting graphics, and sometimes even changing gameplay elements to better suit the target audience.#local:what_is_localization1

Localization is crucial for reaching a global audience and ensuring your game is accessible to players from diverse backgrounds.#local:what_is_localization2

* [Go Back] -> choose_a_topic

== setup_localization ==
Setting up localization in Unity involves a few key steps:#local:setup_localization

1. **Install the Localization Package**: Go to the Unity Package Manager and install the official Unity Localization package.#local:setup_localization_step1
2. **Create a Localization Settings Asset**: This asset will store all your localization settings, including supported languages and default language.#local:setup_localization_step2
3. **Set Up Locales**: Locales represent the different languages and cultures your game supports. Create a new Locale for each language.#local:setup_localization_step3
4. **Create String Tables**: String tables store all the text that needs to be translated. Each language will have its own string table.#local:setup_localization_step4

* [Go Back] -> choose_a_topic

== translating_text ==
Translating text is a key part of localization. In Unity, you use string tables to store translations for different languages.#local:translating_text

1. **Add Entries to String Tables**: Each entry in a string table represents a piece of text that needs to be translated. For example, the English string table might have an entry for "Start Game," and the Spanish table would have "Iniciar Juego."#local:translating_text_step1
2. **Use the Localize Component**: Attach the `Localize String Event` component to UI elements like text fields or buttons. This component will automatically display the correct translation based on the selected locale.#local:translating_text_step2
3. **Preview Translations**: In the Localization Settings, you can switch between different locales to preview how the translated text looks in your game.#local:translating_text_step3

* [Go Back] -> choose_a_topic