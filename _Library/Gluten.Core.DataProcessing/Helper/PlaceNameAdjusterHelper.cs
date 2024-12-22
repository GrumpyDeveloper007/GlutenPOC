using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gluten.Core.DataProcessing.Helper
{
    public static class PlaceNameAdjusterHelper
    {
        public static string FixUserErrorsInPlaceNames(string placeName, string country)
        {


            if (country == "France" || country == "")
            {
                placeName = placeName.Replace("Bananas", "La Bananas");
            }
            if (country == "Italy" || country == "")
            {
                placeName = placeName.Replace("Cafe Roma", "Caffè Roma");
                placeName = placeName.Replace("Graffe", "La graffa");
                if (!placeName.Contains("Pizzeria Vesi")) placeName = placeName.Replace("Vesi", "Pizzeria Vesi");

            }
            if (country == "United Kingdom" || country == "")
            {
                placeName = placeName.Replace("Sardi", "Sarti");
                placeName = placeName.Replace("Nonna Rosa's", "Nonna Rosa");
                placeName = placeName.Replace("Venchi Chocolate and Gelato", "Venchi");
            }
            if (country == "Indonesia" || country == "")
            {
                placeName = placeName.Replace("Belle Italian", "Bella Italia");
                placeName = placeName.Replace("Hotel Mexicola", "Motel Mexicola");
            }
            if (country == "Spain" || country == "")
            {
                placeName = placeName.Replace("Chök bakery", "Chök");
                placeName = placeName.Replace("L’Ancora", "Restaurant L'Àncora");
                placeName = placeName.Replace("Nico's Restaurant", "Niko’s");
            }
            if (country == "Poland" || country == "")
            {
                placeName = placeName.Replace("MooMoo Steak and Wine", "Moo Moo");
                placeName = placeName.Replace("Moo Moo Steak Restaurants", "Moo Moo");

            }
            if (country == "Croatia" || country == "")
            {
                placeName = placeName.Replace("Salt House", "SALT Gluten-Free House");
            }
            if (country == "Germany" || country == "")
            {
                placeName = placeName.Replace("Izabella's", "Isabella Gluten");
            }
            if (country == "Canada" || country == "")
            {
                placeName = placeName.Replace("My Fries", "myFRIES");
                placeName = placeName.Replace("Old Spaghetti Factory", "The Old Spaghetti Factory");
            }
            if (country == "Malaysia" || country == "")
            {
            }
            if (country == "Hungary" || country == "")
            {
                if (placeName == "Lemonade") placeName = placeName.Replace("Lemonade", "Pink Lemonade Bar");
            }
            if (country == "United Arab Emirates" || country == "")
            {
                placeName = placeName.Replace("Pizza Express", "PizzaExpress");
                placeName = placeName.Replace("Mani's Cafe", "Mani’s Cafe");
            }
            if (country == "United States" || country == "")
            {
                placeName = placeName.Replace("Wholefoods", "Whole Foods Market");
            }

            // TODO: Add parsing of name for spelling errors?
            // TODO: Remove restaurant types from place name?
            // TODO: Apply fuzzy logic or spell check?

            placeName = placeName.Replace("Becky Boos", "Beccy Boos Bar");
            placeName = placeName.Replace("Woolies", "Woolworths");
            placeName = placeName.Replace("Hola Guacamole", "Holy Guacamole");
            placeName = placeName.Replace("Nandos", "Nando's");
            placeName = placeName.Replace("Cheesecake Factory", "The Cheesecake Factory");
            placeName = placeName.Replace("Sushi-ichi", "Sushiichi");
            placeName = placeName.Replace("Le Jardin", "Brasserie Le Jardin");
            placeName = placeName.Replace("7-11", "7-Eleven");
            placeName = placeName.Replace("Grom gelato", "Grom");
            placeName = placeName.Replace("GROM gelato", "Grom");
            placeName = placeName.Replace("Rowie's Cakes", "Rowies Cakes");
            placeName = placeName.Replace("La Barocca", "La Baracca");
            placeName = placeName.Replace("Fish on", "FishOn");
            placeName = placeName.Replace("Amino's", "Amino");
            placeName = placeName.Replace("Basil Cafe", "Basil");
            placeName = placeName.Replace("McDonald’s", "McDonald's");
            placeName = placeName.Replace("McDonalds", "McDonald's");
            placeName = placeName.Replace("MacDonald's", "McDonald's");
            placeName = placeName.Replace("Macca's", "McDonald's");
            placeName = placeName.Replace("McD's", "McDonald's");
            placeName = placeName.Replace("McDs", "McDonald's");
            placeName = placeName.Replace("Mc Ds", "McDonald's");
            placeName = placeName.Replace("Gonoachi", "Gonpachi");
            placeName = placeName.Replace("Menbakka", "Menbaka");
            placeName = placeName.Replace("Ikispari", "Ikspiari");

            placeName = placeName.Replace("Mie by me", "BY-MIE NOODLES HOUSE");
            placeName = placeName.Replace("The Hard Rock Cafe", "Hard Rock Cafe Bali");
            placeName = placeName.Replace("Gluten Free T's", "Gluten Free T’s Kitchen");
            placeName = placeName.Replace("gluten free Ts", "Gluten Free T’s Kitchen");


            placeName = placeName.Replace("Samurai Ramen", "Samurai");
            placeName = placeName.Replace("WITHGREEN salad place", "WITHGREEN");
            placeName = placeName.Replace("Jonathan's", "ジョナサン");
            placeName = placeName.Replace("New Days", "NewDays");
            placeName = placeName.Replace("Ninos", "Nino");

            placeName = placeName.Replace("Wendy's First Kitchen", "Wendy’s First Kitchen");

            placeName = placeName.Replace("Coco Curry", "Coco");
            placeName = placeName.Replace("10 Soba restaurant", "10 Soba Japanese Noodle Gokomachi Daimonjicho, Nakagyo Ward");
            placeName = placeName.Replace("Lawson natural", "Lawson");


            placeName = placeName.Replace("CoCo Ichibanya Curry", "CoCo Ichibanya");
            placeName = placeName.Replace("CoCo Ichibanya Curry", "CoCo Ichibanya");
            placeName = placeName.Replace("Pink Berry", "PinkBerry");
            placeName = placeName.Replace("Pizzeria Porto Rotondo", "Il Pomodoro Porto Rotondo");
            placeName = placeName.Replace("Pizza positano", "Pizzeria Positano");
            placeName = placeName.Replace("Fatamorgana Gelato", "Fatamorgana");
            placeName = placeName.Replace("Pizzeria Ristorante Il Pizzeria Il Cavaliere", "Pizzeria-Ristorante Il Cavaliere");
            placeName = placeName.Replace("DisPensa Cibo e Cultura", "Dispensa Caffe e Cucina");
            placeName = placeName.Replace("1970 Bakery", "Bakery 1970 gluten free");
            placeName = placeName.Replace("Sgrano Gluten-Free", "Sgrano");
            placeName = placeName.Replace("Guzman and Gomez", "Guzman y Gomez");
            placeName = placeName.Replace("GYG", "Guzman y Gomez");
            placeName = placeName.Replace("The Botaniste", "The Botanist");
            placeName = placeName.Replace("Panta Rei", "Pantha Rei");
            placeName = placeName.Replace("The Queen of Tarts", "Queen of Tarts Dame Street");
            placeName = placeName.Replace("さらしな-", "Sarashina");
            placeName = placeName.Replace("Crepes Aimer", "Crepe Aimer");
            placeName = placeName.Replace("Cafe de Crie", "Café de Crié");
            placeName = placeName.Replace("Bikkuri Donki", "Bikkuri Donkey");
            placeName = placeName.Replace("Koguma Okinomiyaki", "Koguma");
            placeName = placeName.Replace("Lawson natural", "Natural Lawson");
            placeName = placeName.Replace("Teddy's Better Burgers", "Teddy's Bigger Burgers");
            placeName = placeName.Replace("Seijo Isshi", "Seijo Ishii");
            placeName = placeName.Replace("WITHGREEN salad place (Narita airport)", "WithGreen");
            placeName = placeName.Replace("Comeru’s", "Comeru");
            placeName = placeName.Replace("Soranoiro ramen", "Soranoiro");
            placeName = placeName.Replace("Kinosuke Grill", "Kinosuke");
            placeName = placeName.Replace("Enen Kyoto", "Enen");
            placeName = placeName.Replace("Cowboy Cook House", "Cowboy Cookhouse");
            placeName = placeName.Replace("Sushirazuka Yanoki", "Sushirakuza Yanoki");
            placeName = placeName.Replace("flippers", "flipper's");
            placeName = placeName.Replace("Marks n Spencer", "Marks & Spencer");
            placeName = placeName.Replace("Manduca Coffee & Lunch", "Manduca Cafe");
            placeName = placeName.Replace("Topo Gogio", "Topo Gigio");
            placeName = placeName.Replace("TUI BLUE Sensatori Akra", "AKRA FETHİYE Tui Blue Sensatori");
            placeName = placeName.Replace("Sticks n Sushi", "Sticks'n'Sushi");
            placeName = placeName.Replace("HUG cafe", "H.U.G Bageri");
            placeName = placeName.Replace("Pizza Positano", "Pizzeria Positano Milano");
            placeName = placeName.Replace("Bagels and Beans", "Bagels & Beans");
            placeName = placeName.Replace("Bagels and beans", "Bagels & Beans");
            placeName = placeName.Replace("Grosso Pizzaria", "Grosso Napoletano");
            placeName = placeName.Replace("Photo 1 brunch cafe", "brunch cafe");
            placeName = placeName.Replace("Edes Mckos", "Édes Mackó");
            placeName = placeName.Replace("Port house", "The Port House");
            placeName = placeName.Replace("Port house", "The Port House");
            placeName = placeName.Replace("Rice me cafe and restaurant", "Rice Me Deli - cafetaria &bakery");
            placeName = placeName.Replace("Robs ranch house", "Rob's ranch house");
            placeName = placeName.Replace("art-isa", "ARTiSA");
            placeName = placeName.Replace("mai atza", "Maiatza");
            placeName = placeName.Replace("Tapas n friends", "Tapas n' Friends");
            placeName = placeName.Replace("Gurame", "Gurumê");
            placeName = placeName.Replace("Senza Glutine", "Senza Gluten");
            placeName = placeName.Replace("Hibbou", "Le Hibou");
            placeName = placeName.Replace("Salty Swami cafe", "Salty Swamis");
            placeName = placeName.Replace("New food Cafe", "New Food Gluten Free");
            placeName = placeName.Replace("BJ's Brewhouse", "BJ's Restaurant & Brewhouse");
            placeName = placeName.Replace("Waggamamas", "Wagamama");
            placeName = placeName.Replace("Stephanie’s Creperie", "Stephanie's Crepes");
            if (placeName == "Porthouse")
                placeName = placeName.Replace("Porthouse", "The Port House");
            placeName = placeName.Replace("Panifizio Delizie", "Panificio Delizie");
            placeName = placeName.Replace("Vapianos", "Vapiano");
            placeName = placeName.Replace("Moo Moo steakhouse and wine", "Moo Moo Steak & Wine");
            placeName = placeName.Replace("Terasse La Maison des epices", "Terrasse des épices");
            placeName = placeName.Replace("lapetititalia", "La Petitalia");
            placeName = placeName.Replace("Mamma Trattoria", "Mama trattoria");
            placeName = placeName.Replace("Pizza Colleseum", "Pizza Coloseum");
            placeName = placeName.Replace("Pasta Corso", "Pasta in Corso");
            placeName = placeName.Replace("Risotto Merlotti", "Risotteria Melotti");
            placeName = placeName.Replace("Le delizie Senza gluten", "Le Delizie Gluten Free");
            placeName = placeName.Replace("Groms", "Grom");
            placeName = placeName.Replace("Ristorante Kona", "Kona");
            placeName = placeName.Replace("Fatamorgana gelato", "Fatamorgana");
            placeName = placeName.Replace("Panther Rei", "Pantha Rei");
            placeName = placeName.Replace("Pantheon Rei", "Pantha Rei");
            placeName = placeName.Replace("Hostería Farnese", "Hostaria Farnese");
            placeName = placeName.Replace("Ristorant pizzeria da gaetano", "Ristorante Pizzeria Da Gaetano");
            placeName = placeName.Replace("Mr Pizza", "Mister Pizza");
            placeName = placeName.Replace("Misterpizza", "Mister Pizza");
            placeName = placeName.Replace("Mister Pizza Florence", "Mister Pizza");
            placeName = placeName.Replace("CHaT Sweets", "CHaT");
            placeName = placeName.Replace("Aeon malls/markets", "Aeon");
            placeName = placeName.Replace("Over Macrons", "over macaron");
            placeName = placeName.Replace("GF T's Kitchen", "Gluten Free T’s Kitchen");
            placeName = placeName.Replace("Torikizoku asakusa", "YAKITORI Torikizoku");
            placeName = placeName.Replace("Tokyu Supermarket", "Tokyu Store");
            placeName = placeName.Replace("Luciole bakery", "Glutenfree Shop Luciole");
            placeName = placeName.Replace("Great! Kebab!", "GREAT! KEBAB!, Toshima City");
            placeName = placeName.Replace("Lawson's", "Lawson");
            placeName = placeName.Replace("Torikizoku Asakusa", "YAKITORI Torikizoku Asakusa");
            placeName = placeName.Replace("Konekotenpurako", "Komeko Tempura Kobou");
            placeName = placeName.Replace("Senzanbansui", "九州創作 千山万水");
            placeName = placeName.Replace("Tajmahal Indian restaurant", "Taj Mahal");
            placeName = placeName.Replace("Otoya", "Ootoya");
            placeName = placeName.Replace("Marbre Café", "marbre vegan");
            placeName = placeName.Replace("Sakura-jaja", "Sakurajaya");
            placeName = placeName.Replace("Mayan Curry Ikebukuro", "Moyan Curry Ikebukuro");
            placeName = placeName.Replace("Shunbu Sakiya Shibuya", "Shinbu Sakiya");
            placeName = placeName.Replace("Omega Cafe Yokohama", "Ω Cafe Sakuragicho Shop");
            placeName = placeName.Replace("Mr Donut", "Mister Donut");
            placeName = placeName.Replace("Eggs and Things", "Eggs 'n Things");
            placeName = placeName.Replace("Cafe Maru-", "Cafe & Bar Maru");
            placeName = placeName.Replace("Dean and Deluca", "DEAN & DELUCA");
            placeName = placeName.Replace("Bake Lab", "星製粉所 BAKE LAB");
            placeName = placeName.Replace("Cups Cafe", "THE CUPS SAKAE");
            placeName = placeName.Replace("Cafe Bar Shisha", "Cafe and Shisha Bar ZEN");
            placeName = placeName.Replace("mmusubi-cafe", "musubi cafe");
            placeName = placeName.Replace("my bahn mi gf", "My Bánh Mì by Gluten Free TOKYO");
            placeName = placeName.Replace("Breizh Creperie", "BREIZH Café Crêperie");
            placeName = placeName.Replace("Briezh Cafe", "BREIZH Café Crêperie");
            placeName = placeName.Replace("Tokyo Cafe: Spread", "Spread");
            placeName = placeName.Replace("Kura sushi at Skytree", "Kura sushi");
            placeName = placeName.Replace("NA Cafe", "nacafe");
            placeName = placeName.Replace("ITAMAE SUSHI Akasaka", "Itamae Sushi Hanare");
            placeName = placeName.Replace("Nino’s Italian", "Nino");
            placeName = placeName.Replace("Moya Curry", "Moyan Curry");
            placeName = placeName.Replace("Ts gluten free kitchen", "Gluten Free T’s Kitchen");
            placeName = placeName.Replace("Dontonburi Kamukura", "Dotombori Kamukura");
            placeName = placeName.Replace("Siesta Cafe", "Siesta Organic Cafe");
            placeName = placeName.Replace("DAMONDE", "DA MONDE");
            placeName = placeName.Replace("SakaeSushi", "Sakae Sushi");

            placeName = placeName.Replace("CafeLumiere", "cafe Lumiere");

            placeName = placeName.Replace("SekaiNoYamachan", "Sekai no Yamachan");

            placeName = placeName.Replace("TorikaiSohonke", "Torikai Sohonke");

            placeName = placeName.Replace("CURRY HOUSE CoCo ICHIBANYA", "CURRY HOUSE CoCoICHIBANYA WORLD");

            //placeName = placeName.Replace("Chibo Dotonbori Osaka", "Chibo Okonomiyaki Restaurant");

            placeName = placeName.Replace("Pizzaioli vetaci", "Pizzaioli Veraci");
            placeName = placeName.Replace("Le Gelateria", "La Gelatiera");
            placeName = placeName.Replace("Torce Gelateria / Yogurteria", "Torcè");
            placeName = placeName.Replace("Celiachamo Lab!", "Celiachiamo Lab");
            placeName = placeName.Replace("#ungluten", "un-gluten");
            placeName = placeName.Replace("Frankies", "Frankie's");
            placeName = placeName.Replace("Oasis Restaurant", "Restaurante Oasis");
            placeName = placeName.Replace("Bella Italia Restaurant", "Bella Italia");
            placeName = placeName.Replace("Mercadona Supermarket", "Mercadona");

            if (!placeName.Contains("Baker Hansen")) placeName = placeName.Replace("Hansen", "Baker Hansen");
            placeName = placeName.Replace("Kamat Restaurant", "Kamat Vegetarian");
            placeName = placeName.Replace("Freedom Pizza Restaurant", "Freedom Pizza");
            placeName = placeName.Replace("Baskin Robins", "Baskin Robbins");
            placeName = placeName.Replace("Long John Silvers", "Long John Silver's");
            placeName = placeName.Replace("Ciacho Bakery", "Ciacho");

            placeName = placeName.Replace("restaurant", "");
            placeName = placeName.Trim();
            return placeName;
        }
    }
}

