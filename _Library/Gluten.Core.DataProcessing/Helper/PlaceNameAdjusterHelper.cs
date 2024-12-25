using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gluten.Core.DataProcessing.Helper
{
    public static class PlaceNameAdjusterHelper
    {
        public static string FixUserErrorsInPlaceNames(string placeName, string country, string city)
        {
            placeName = RemoveSuffix(placeName, "Supermarket");
            placeName = RemoveSuffix(placeName, "Restaurants");
            placeName = RemoveSuffix(placeName, "Restaurant");
            placeName = RemoveSuffix(placeName, "Gelato");
            placeName = RemoveSuffix(placeName, "grocery store");
            placeName = RemoveSuffix(placeName, "convenience store");
            placeName = RemoveSuffix(placeName, "malls/markets");
            placeName = RemoveSuffix(placeName, "salad place");
            placeName = RemoveSuffix(placeName, "-");
            placeName = RemoveSuffix(placeName, " bakery");


            if (!placeName.Contains("Pizzeria Vesi")) placeName = placeName.Replace("Vesi", "Pizzeria Vesi");
            if (country == "France" || country == "")
            {
                placeName = placeName.Replace("Bananas", "La Bananas");
            }

            if (country == "Hungary" || country == "")
            {
                if (placeName == "Lemonade") placeName = placeName.Replace("Lemonade", "Pink Lemonade Bar");
            }
            if (country == "United States" || country == "")
            {
                placeName = placeName.Replace("Wholefoods", "Whole Foods Market");
            }
            placeName = placeName.Replace("Old Spaghetti Factory", "The Old Spaghetti Factory");

            // TODO: Add parsing of name for spelling errors?
            // TODO: Remove restaurant types from place name?
            // TODO: Apply fuzzy logic or spell check?


            placeName = placeName.Replace("Blacktap", "Black Tap");
            placeName = placeName.Replace("MooMoo Steak and Wine", "Moo Moo");
            placeName = placeName.Replace("Moo Moo Steak", "Moo Moo");
            placeName = placeName.Replace("Sushi Go Round", "Sushi-Go-Round");

            placeName = placeName.Replace("Jurassic Park Cafe", "Jurassic Park");
            placeName = placeName.Replace("Cafe Komoya", "Cafe Komaya - gluten free cafe");
            placeName = placeName.Replace("Origin Bento", "Kitchen Origin Bento");
            placeName = placeName.Replace("Starbucks / Starbucks Reserve", "Starbucks Reserve");
            placeName = placeName.Replace("Moyan Curry Shinjuku Dining", "Moyan Curry");
            placeName = placeName.Replace("Shimauma Burgers", "Shimauma Burger");
            placeName = placeName.Replace("Asakusa Milkrepe", "Asakusa");
            placeName = placeName.Replace("Taiwanese", "HAKONE PICNIC");
            placeName = placeName.Replace("Nara -Onwa Cafe", "onwa");
            placeName = placeName.Replace("Kotobuki Monja", "Kachidoki");
            placeName = placeName.Replace("Estación", "Katsukura - Kyoto Porta");
            placeName = placeName.Replace("Wacos Crepes", "waco crepes");
            placeName = placeName.Replace("Chibo Dotonbori Osaka", "Chibo");
            placeName = placeName.Replace("Kaki Ichiban", "Hiroshima specialty oyster first");
            placeName = placeName.Replace("Salama at EAT PLAY WORKS-THE", "Salam");

            placeName = placeName.Replace("Ikinari steakhouse", "Ikinari Steak");
            placeName = placeName.Replace("shinbusakiya", "Shinbu Sakiya Ramen");
            placeName = placeName.Replace("ramen shop sinbusakiya", "Shinbu Sakiya Ramen");
            placeName = placeName.Replace("さらしな- 2F in Okonomiyaki building", "Sarashina");
            placeName = placeName.Replace("Chichu Art Museum Cafe", "Chichu Art Museum");
            placeName = placeName.Replace("800 degrees Neapolitan pizza", "800 Degrees Neapolitan Pizzeria");
            placeName = placeName.Replace("Ikinari Steakhouse", "Ikinari Steak");
            placeName = placeName.Replace("Shochikeun Cafe", "Shochikuen Cafe");
            placeName = placeName.Replace("Super Loco Mexican", "Super Loco");
            placeName = placeName.Replace("Medya", "MEIDI-YA");
            placeName = placeName.Replace("Manacitas", "Mamacitas");
            placeName = placeName.Replace("Ta Wai Thai", "Tawai Thai Cuisine");
            placeName = placeName.Replace("Skai At Swissotel", "SKAI Bar");

            placeName = placeName.Replace("Mercado tsukiji", "Tsukiji");
            placeName = placeName.Replace("Ohagi 3", "OHAGI 3 Shimokitazawaten");

            placeName = placeName.Replace("+ Plus One", "PlusOne");
            placeName = placeName.Replace("Akita Prefecture Souvenir Shop", "Akita Prefectural Goods Plaza “Akitano”");

            placeName = placeName.Replace("Ungluten- Tokyo", "un-gluten(アングルテン)");

            placeName = placeName.Replace("Becky Boos", "Beccy Boos Bar");
            placeName = placeName.Replace("Hola Guacamole", "Holy Guacamole");
            placeName = placeName.Replace("Cheesecake Factory", "The Cheesecake Factory");
            placeName = placeName.Replace("Sushi-ichi", "Sushiichi");
            placeName = placeName.Replace("Le Jardin", "Brasserie Le Jardin");
            placeName = placeName.Replace("Cafe Beviamo", "CAFFE BEVIAMO");
            placeName = placeName.Replace("Mishima Base", "ミヤジマベース MIYAJIMA BASE");


            placeName = placeName.Replace("Cafe Barraca", "Barraca");

            placeName = placeName.Replace("Basil Cafe", "Basil");
            placeName = placeName.Replace("Menbakka", "Menbaka");
            placeName = placeName.Replace("Ikispari", "Ikspiari");
            placeName = placeName.Replace("Hybrid￼", "Hybrid");
            placeName = placeName.Replace("Alladin Indian", "ALLADIN INDO RESTAURANT");

            placeName = placeName.Replace("Toraji Tokyo Ebisu", "Yakiniku Toraji Ebisu");
            placeName = placeName.Replace("FALAFEL SABARA", "Falafel Sababa");

            placeName = placeName.Replace("Mie by me", "BY-MIE NOODLES HOUSE");
            placeName = placeName.Replace("The Hard Rock Cafe", "Hard Rock Cafe Bali");
            placeName = placeName.Replace("Coco Curry", "Coco");
            placeName = placeName.Replace("10 Soba", "10 Soba Japanese Noodle Gokomachi Daimonjicho, Nakagyo Ward");

            placeName = placeName.Replace("CoCo Ichibanya Curry", "CoCo Ichibanya");
            placeName = placeName.Replace("Pink Berry", "PinkBerry");
            placeName = placeName.Replace("Pizzeria Porto Rotondo", "Il Pomodoro Porto Rotondo");
            placeName = placeName.Replace("Pizza positano", "Pizzeria Positano");
            placeName = placeName.Replace("Pizzeria Ristorante Il Pizzeria Il Cavaliere", "Pizzeria-Ristorante Il Cavaliere");
            placeName = placeName.Replace("DisPensa Cibo e Cultura", "Dispensa Caffe e Cucina");
            placeName = placeName.Replace("1970", "Bakery 1970 gluten free");
            placeName = placeName.Replace("Sgrano Gluten-Free", "Sgrano");
            placeName = placeName.Replace("The Queen of Tarts", "Queen of Tarts Dame Street");
            placeName = placeName.Replace("さらしな-", "Sarashina");
            placeName = placeName.Replace("Crepes Aimer", "Crepe Aimer");
            placeName = placeName.Replace("Bikkuri Donki", "Bikkuri Donkey");
            placeName = placeName.Replace("Koguma Okinomiyaki", "Koguma");
            placeName = placeName.Replace("Lawson natural", "Natural Lawson");
            placeName = placeName.Replace("Seijo Isshi", "Seijo Ishii");
            placeName = placeName.Replace("Soranoiro ramen", "Soranoiro");
            placeName = placeName.Replace("Kinosuke Grill", "Kinosuke");
            placeName = placeName.Replace("Cowboy Cook House", "Cowboy Cookhouse");
            placeName = placeName.Replace("Sushirazuka Yanoki", "Sushirakuza Yanoki");
            placeName = placeName.Replace("Topo Gogio", "Topo Gigio");
            placeName = placeName.Replace("TUI BLUE Sensatori Akra", "AKRA FETHİYE Tui Blue Sensatori");
            placeName = placeName.Replace("HUG cafe", "H.U.G Bageri");
            placeName = placeName.Replace("Pizza Positano", "Pizzeria Positano Milano");
            placeName = placeName.Replace("Grosso Pizzaria", "Grosso Napoletano");
            placeName = placeName.Replace("Photo 1 brunch cafe", "brunch cafe");
            placeName = placeName.Replace("Senza Glutine", "Senza Gluten");
            placeName = placeName.Replace("Hibbou", "Le Hibou");
            placeName = placeName.Replace("Salty Swami cafe", "Salty Swamis");
            placeName = placeName.Replace("New food Cafe", "New Food Gluten Free");
            if (placeName == "Porthouse")
                placeName = placeName.Replace("Porthouse", "The Port House");
            placeName = placeName.Replace("Port house", "The Port House");
            placeName = placeName.Replace("Pizza Colleseum", "Pizza Coloseum");
            placeName = placeName.Replace("Pasta Corso", "Pasta in Corso");
            placeName = placeName.Replace("Risotto Merlotti", "Risotteria Melotti");
            placeName = placeName.Replace("Le delizie Senza gluten", "Le Delizie Gluten Free");
            placeName = placeName.Replace("Hostería Farnese", "Hostaria Farnese");
            placeName = placeName.Replace("Ristorant pizzeria da gaetano", "Ristorante Pizzeria Da Gaetano");
            placeName = placeName.Replace("Torikizoku asakusa", "YAKITORI Torikizoku");
            placeName = placeName.Replace("Luciole", "Glutenfree Shop Luciole");
            placeName = placeName.Replace("Great! Kebab!", "GREAT! KEBAB!, Toshima City");
            placeName = placeName.Replace("Torikizoku Asakusa", "YAKITORI Torikizoku Asakusa");
            placeName = placeName.Replace("Tajmahal Indian", "Taj Mahal");
            placeName = placeName.Replace("Marbre Café", "marbre vegan");
            placeName = placeName.Replace("Shunbu Sakiya Shibuya", "Shinbu Sakiya");
            placeName = placeName.Replace("Omega Cafe Yokohama", "Ω Cafe Sakuragicho Shop");
            placeName = placeName.Replace("Breizh Creperie", "BREIZH Café Crêperie");
            placeName = placeName.Replace("Briezh Cafe", "BREIZH Café Crêperie");
            placeName = placeName.Replace("ITAMAE SUSHI Akasaka", "Itamae Sushi Hanare");
            placeName = placeName.Replace("Siesta Cafe", "Siesta Organic Cafe");
            placeName = placeName.Replace("CURRY HOUSE CoCo ICHIBANYA", "CURRY HOUSE CoCoICHIBANYA WORLD");


            if (!placeName.Contains("Baker Hansen")) placeName = placeName.Replace("Hansen", "Baker Hansen");

            //////////////////////////////////////////////////////////////

            // Prefix
            placeName = placeName.Replace("Tokyo Cafe: Spread", "Spread");
            placeName = placeName.Replace("Coffee Academics", "The Coffee Academics");
            placeName = placeName.Replace("Cups Cafe", "THE CUPS SAKAE");
            placeName = placeName.Replace("Graffe", "La graffa");
            placeName = placeName.Replace("Ristorante Kona", "Kona");

            // Extra Suffix
            placeName = placeName.Replace("Nukafuku donuts", "nukafuku");
            placeName = placeName.Replace("GROM", "Grom");
            placeName = placeName.Replace("Enen Kyoto", "Enen");
            placeName = placeName.Replace("Groms", "Grom");
            placeName = placeName.Replace("CHaT Sweets", "CHaT");
            placeName = placeName.Replace("Kura sushi at Skytree", "Kura sushi");
            placeName = placeName.Replace("Sunrise Tacos ~ Promenade Mall", "Sunrise Tacos");
            placeName = placeName.Replace("Sugarhill Kyoto", "Sugarhill Kyoto");
            placeName = placeName.Replace("Venchi Chocolate and", "Venchi");
            placeName = placeName.Replace("Ginza Isomera Kushiage", "Ginza Isomura");
            placeName = placeName.Replace("Ninos", "Nino");
            placeName = placeName.Replace("Blue bird cafe", "Blue bird");
            placeName = placeName.Replace("Han no Daidokoro Kafochikq", "Han no daidokoro");
            placeName = placeName.Replace("Han no Daidokoro Bazaar", "Han no daidokoro");
            placeName = placeName.Replace("Samurai Ramen", "Samurai");
            placeName = placeName.Replace("my bahn mi gf", "My Bánh Mì by Gluten Free TOKYO");
            placeName = placeName.Replace("Oasis", "Restaurante Oasis");
            placeName = placeName.Replace("Torce Gelateria / Yogurteria", "Torcè");
            placeName = placeName.Replace("Celiachamo Lab!", "Celiachiamo Lab");
            placeName = placeName.Replace("Kamat", "Kamat Vegetarian");
            placeName = placeName.Replace("Nico's", "Niko’s");
            placeName = placeName.Replace("Nino’s Italian", "Nino");


            // Error with spaces / Missing spaces
            placeName = placeName.Replace("Star Bucks", "Starbucks");
            placeName = placeName.Replace("Taco Taco", "TacoTaco");
            placeName = placeName.Replace("Salad Stop", "SaladStop!");
            placeName = placeName.Replace("IsomaruSuisan", "Isomaru Suisan");
            placeName = placeName.Replace("Nikuazabu", "NIKU-AZABU");
            placeName = placeName.Replace("lapetititalia", "La Petitalia");
            placeName = placeName.Replace("My Fries", "myFRIES");
            placeName = placeName.Replace("Pizza Express", "PizzaExpress");
            placeName = placeName.Replace("Fish on", "FishOn");
            placeName = placeName.Replace("Family Mart", "FamilyMart");
            placeName = placeName.Replace("FamilyMarts", "FamilyMart");
            placeName = placeName.Replace("RoseBakery", "Rose Bakery");
            placeName = placeName.Replace("Genijsoba", "Genji-soba");
            placeName = placeName.Replace("Ginja Soba", "Genji-soba");
            placeName = placeName.Replace("Healthy Tokyo", "HealthyTokyo");
            placeName = placeName.Replace("New Days", "NewDays");
            placeName = placeName.Replace("art-isa", "ARTiSA");
            placeName = placeName.Replace("mai atza", "Maiatza");
            placeName = placeName.Replace("Konekotenpurako", "Komeko Tempura Kobou");
            placeName = placeName.Replace("Kurasushi", "Kura sushi");
            placeName = placeName.Replace("NA Cafe", "nacafe");
            placeName = placeName.Replace("Sakura-jaja", "Sakurajaya");
            placeName = placeName.Replace("mmusubi-cafe", "musubi cafe");
            placeName = placeName.Replace("DAMONDE", "DA MONDE");
            placeName = placeName.Replace("SakaeSushi", "Sakae Sushi");
            placeName = placeName.Replace("CafeLumiere", "cafe Lumiere");
            placeName = placeName.Replace("SekaiNoYamachan", "Sekai no Yamachan");
            placeName = placeName.Replace("TorikaiSohonke", "Torikai Sohonke");


            // Ands
            placeName = placeName.Replace("Cafe Bar Shisha", "Cafe and Shisha Bar ZEN");
            placeName = placeName.Replace("Plus One Cafe and Dining", "Plus1 coffee");
            placeName = placeName.Replace("Moo Moo steakhouse and wine", "Moo Moo Steak & Wine");
            placeName = placeName.Replace("Tomato and Onion", "Tomato & Onion");
            placeName = placeName.Replace("The campfire sushi and cafe", "The Campfire Sushi & Cafe Bar");
            placeName = placeName.Replace("Guzman and Gomez", "Guzman y Gomez");
            placeName = placeName.Replace("Bagels and Beans", "Bagels & Beans");
            placeName = placeName.Replace("Bagels and beans", "Bagels & Beans");
            placeName = placeName.Replace("Rice me cafe and", "Rice Me Deli - cafetaria &bakery");
            placeName = placeName.Replace("Dean and Deluca", "DEAN & DELUCA");
            placeName = placeName.Replace("Marks n Spencer", "Marks & Spencer");
            placeName = placeName.Replace("Manduca Coffee & Lunch", "Manduca Cafe");
            placeName = placeName.Replace("Cafe Maru", "Cafe & Bar Maru");
            placeName = placeName.Replace("Maru Bar& Cafe", "Cafe & Bar Maru");
            placeName = placeName.Replace("Cafe Maru-", "Cafe & Bar Maru");
            placeName = placeName.Replace("Takeru 1 Pound Steakhouse", "1 Pound Steak & Hamburg Takeru");
            placeName = placeName.Replace("Café & Bar SQOL", "Cafe＆Bar SQOL");
            placeName = placeName.Replace("Eggs and Things", "Eggs 'n Things");

            // hyphens 
            placeName = placeName.Replace("Cafe de Crie", "Café de Crié");
            placeName = placeName.Replace("DIVER'S INN", "Diver´s Inn Steak House");
            placeName = placeName.Replace("Diver's Inn", "Diver´s Inn Steak House");
            placeName = placeName.Replace("Diver’s Inn", "Diver´s Inn Steak House");
            placeName = placeName.Replace("Lawson's", "Lawson");
            placeName = placeName.Replace("Nonna Rosa's", "Nonna Rosa");
            placeName = placeName.Replace("L’Ancora", "Restaurant L'Àncora");
            placeName = placeName.Replace("Mamacita's restaurant", "Mamacitas");
            placeName = placeName.Replace("Izabella's", "Isabella Gluten");
            placeName = placeName.Replace("Long John Silvers", "Long John Silver's");
            placeName = placeName.Replace("Sticks n Sushi", "Sticks'n'Sushi");
            placeName = placeName.Replace("Robs ranch house", "Rob's ranch house");
            placeName = placeName.Replace("Tapas n friends", "Tapas n' Friends");
            placeName = placeName.Replace("BJ's Brewhouse", "BJ's Restaurant & Brewhouse");
            placeName = placeName.Replace("Frankies", "Frankie's");
            placeName = placeName.Replace("Stephanie’s Creperie", "Stephanie's Crepes");
            placeName = placeName.Replace("Jonathan's", "ジョナサン");
            placeName = placeName.Replace("Wendy's First Kitchen", "Wendy’s First Kitchen");
            placeName = placeName.Replace("flippers", "flipper's");
            placeName = placeName.Replace("Teddy's Better Burgers", "Teddy's Bigger Burgers");
            placeName = placeName.Replace("Mani's Cafe", "Mani’s Cafe");
            placeName = placeName.Replace("Teddy’s Bigger Burgers", "Teddy's Bigger Burgers");
            placeName = placeName.Replace("Watson's", "Watsons");
            placeName = placeName.Replace("Smith's", "Smiths");
            placeName = placeName.Replace("Alfrescos", "Al Fresco's");
            placeName = placeName.Replace("Dominos Pizza", "Domino's Pizza");
            placeName = placeName.Replace("McDonald’s", "McDonald's");
            placeName = placeName.Replace("McDonalds", "McDonald's");
            placeName = placeName.Replace("MacDonald's", "McDonald's");
            placeName = placeName.Replace("MCDonalds", "McDonald's");
            placeName = placeName.Replace("Mcdonalds", "McDonald's");
            placeName = placeName.Replace("Macca's", "McDonald's");
            placeName = placeName.Replace("McD's", "McDonald's");
            placeName = placeName.Replace("McDs", "McDonald's");
            placeName = placeName.Replace("Mc Ds", "McDonald's");
            placeName = placeName.Replace("Bio C Bon", "Bio c' Bon");
            placeName = placeName.Replace("Nandos", "Nando's");
            placeName = placeName.Replace("Becks Coffee", "Beck's Coffee Shop");
            placeName = placeName.Replace("Rowie's Cakes", "Rowies Cakes");
            placeName = placeName.Replace("Amino's", "Amino");
            placeName = placeName.Replace("Mo's Burger", "Mos Burger");
            placeName = placeName.Replace("Comeru’s", "Comeru");
            placeName = placeName.Replace("Lawson’s", "Lawson");
            placeName = placeName.Replace("Tiannes", "TIANN’S");
            placeName = placeName.Replace("Mo’s burger", "Mos Burger");
            placeName = placeName.Replace("Cafe Roma", "Caffè Roma");



            placeName = FixTypos(placeName);

            if (!string.IsNullOrWhiteSpace(city)) placeName = placeName.Replace(city, "").Trim();
            if (placeName.StartsWith('-')) placeName = placeName.Substring(1);
            placeName = placeName.Trim();
            return placeName;
        }


        private static string FixTypos(string placeName)
        {
            placeName = placeName.Replace("Gonoachi", "Gonpachi");
            placeName = placeName.Replace("7-11", "7-Eleven");
            placeName = placeName.Replace("7/11", "7-Eleven");
            placeName = placeName.Replace("711", "7-Eleven");
            placeName = placeName.Replace("Seven-11", "7-Eleven");
            placeName = placeName.Replace("7eleven", "7-Eleven");
            placeName = placeName.Replace("7 eleven", "7-Eleven");
            placeName = placeName.Replace("Mayan Curry Ikebukuro", "Moyan Curry Ikebukuro");
            placeName = placeName.Replace("Over Macrons", "over macaron");
            placeName = placeName.Replace("Mr Donut", "Mister Donut");
            placeName = placeName.Replace("Moya Curry", "Moyan Curry");
            placeName = placeName.Replace("GF T's Kitchen", "Gluten Free T’s Kitchen");
            placeName = placeName.Replace("Ts gluten free kitchen", "Gluten Free T’s Kitchen");
            placeName = placeName.Replace("Gluten Free T's", "Gluten Free T’s Kitchen");
            placeName = placeName.Replace("gluten free Ts", "Gluten Free T’s Kitchen");
            placeName = placeName.Replace("Gluten free T’s Rappongi", "Gluten Free T’s Kitchen");
            placeName = placeName.Replace("Gluten Free T’s in Ueno", "Gluten Free T’s Kitchen");
            placeName = placeName.Replace("T’s Kitchen", "Gluten Free T’s Kitchen");
            placeName = placeName.Replace("Kind Cones", "Kind Kones");
            placeName = placeName.Replace("Panta Rei", "Pantha Rei");
            placeName = placeName.Replace("Gyomu Supa", "Gyomu Super");
            placeName = placeName.Replace("Baskin Robins", "Baskin Robbins");
            placeName = placeName.Replace("Tandori Palace", "Tandoori Palace");
            placeName = placeName.Replace("Waggamamas", "Wagamama");
            placeName = placeName.Replace("Haubis Vietnam", "Der Imbiss Saigon");
            placeName = placeName.Replace("MOS burgers", "Mos Burger");
            placeName = placeName.Replace("モスバーガー（MOS BURGER）", "Mos Burger");
            placeName = placeName.Replace("Moss burger", "Mos Burger");
            placeName = placeName.Replace("MosBurger", "Mos Burger");
            placeName = placeName.Replace("Woolies", "Woolworths");
            placeName = placeName.Replace("Sardi", "Sarti");
            placeName = placeName.Replace("Belle Italian", "Bella Italia");
            placeName = placeName.Replace("Hotel Mexicola", "Motel Mexicola");
            placeName = placeName.Replace("Where is a Dog", "genuine gluten free Where is a dog?");
            placeName = placeName.Replace("Where is the Dog", "genuine gluten free Where is a dog?");
            placeName = placeName.Replace("Genuine Where Is a Dog", "genuine gluten free Where is a dog?");
            placeName = placeName.Replace("Bake Lab", "星製粉所 BAKE LAB");
            placeName = placeName.Replace("Mochi eui Haru", "모찌의하루 서면본점");
            placeName = placeName.Replace("ANNIE 𝐬𝐰𝐞𝐭𝐞𝐫𝐲 & 𝐞𝐚𝐭𝐞𝐫𝐲", "ANNIE 𝐬𝐰𝐞𝐞𝐭𝐞𝐫𝐲 & 𝐞𝐚𝐭𝐞𝐫𝐲");
            placeName = placeName.Replace("Pizzaioli vetaci", "Pizzaioli Veraci");
            placeName = placeName.Replace("Le Gelateria", "La Gelatiera");
            placeName = placeName.Replace("#ungluten", "un-gluten");
            placeName = placeName.Replace("Dontonburi Kamukura", "Dotombori Kamukura");
            placeName = placeName.Replace("Otoya", "Ootoya");
            placeName = placeName.Replace("Mr Pizza", "Mister Pizza");
            placeName = placeName.Replace("Misterpizza", "Mister Pizza");
            placeName = placeName.Replace("Mister Pizza Florence", "Mister Pizza");
            placeName = placeName.Replace("Panther Rei", "Pantha Rei");
            placeName = placeName.Replace("Pantheon Rei", "Pantha Rei");
            placeName = placeName.Replace("Senzanbansui", "九州創作 千山万水");
            placeName = placeName.Replace("Panifizio Delizie", "Panificio Delizie");
            placeName = placeName.Replace("Vapianos", "Vapiano");
            placeName = placeName.Replace("Terasse La Maison des epices", "Terrasse des épices");
            placeName = placeName.Replace("GYG", "Guzman y Gomez");
            placeName = placeName.Replace("The Botaniste", "The Botanist");
            placeName = placeName.Replace("La Barocca", "La Baracca");
            placeName = placeName.Replace("Salt House", "SALT Gluten-Free House");
            placeName = placeName.Replace("Gyoumu", "Gyōmu");
            placeName = placeName.Replace("Cafe Manma", "Caffè Mamma");
            placeName = placeName.Replace("Edes Mckos", "Édes Mackó");
            placeName = placeName.Replace("Gurame", "Gurumê");
            placeName = placeName.Replace("Mamma Trattoria", "Mama trattoria");
            placeName = placeName.Replace("Soranoira", "Soranoiro");
            placeName = placeName.Replace("Soba Han", "そば半 馬渕店");
            placeName = placeName.Replace("Cocoichi", "CoCo Ichibanya");
            placeName = placeName.Replace("CoCo Curry", "CoCo Ichibanya");
            placeName = placeName.Replace("Cocos", "CoCo Ichibanya");
            placeName = placeName.Replace("michinori bento", "みちのり弁当（Gluten-Free Michinori Bento）");
            placeName = placeName.Replace("Kuma Cafe", "Kuma Kafe");
            placeName = placeName.Replace("Takagiya", "TAKAGIYA (タカギヤ) 東京・巣鴨 高木家");
            placeName = placeName.Replace("Enishi", "縁-enishi-");
            return placeName;
        }

        private static string RemoveSuffix(string placeName, string suffix)
        {
            placeName = placeName.Trim();
            if (placeName.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase))
            {
                placeName = placeName.Substring(0, placeName.Length - suffix.Length);
            }
            placeName = placeName.Trim();
            return placeName;
        }

    }
}

