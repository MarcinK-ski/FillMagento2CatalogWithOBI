using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Collections;
using System.Text.RegularExpressions;
using UnidecodeSharpFork;

namespace ReadObiCatalog
{
    class Program
    {

        public static bool isHttpLink(string URI)
        {
            string[] testing = URI.Split(':');

            Console.WriteLine(URI + "\n\n\n");

            if (testing[0] == "http" || testing[0] == "https")
                return true;

            return false;
        }
        

        static void Main(string[] args)
        {

            SiteReader sr;      //Zmienna do klasy pobierajacej kod strony
            HtmlDocument htmlDoc;
            string basePage = "https://obi.pl";

            sr = new SiteReader(basePage);
            htmlDoc = new HtmlDocument();

            string content;    //Zawartosc strony w stringu
            content = sr.WebContent;   //reloading content

            int licznik = 1;    //Licznik poprawnych
            string exceptions = "";   //File content
            int licznik_b = 0;  //Licznik blednych

            string lastMainCategory = "";   //Do sprawdzania, czy trzeba dodać nową kategorię
            string currentMainCategory;
            string lastSubCategory = "";   //Do sprawdzania, czy trzeba dodać nową SUB-kategorię
            string currentSubCategory;
            int categoriesNumber = 0;
            int subCategoriesNumber = 0;
			
			string login = "LOGIN";
			string passwd = "PASS";

            JsonMagento test = new JsonMagento(login, passwd);

            ArrayList firstResult = SiteReader.findInDocument(htmlDoc, content, "//body/div/header/div/nav/ul/li/div/div/div/div/ul/li/div/ul/li/a", "wt_name", "flyoutmenu.level3", "href");
            ArrayList secondResult;
            foreach (string element in firstResult) //Czytanie pierwszego poziomu, dla uzyskania drugiego poziomu
            {
                if (test.isTokenNull())
                {
                    Console.WriteLine("Problem z autoryzacją");
                    test = new JsonMagento(login, passwd);
                    break;
                }

                Console.WriteLine("--------------------------------------------------------");

                if (isHttpLink(element))
                {
                    string[] categoryPrepare = element.Split('/');
                    currentMainCategory = categoryPrepare[3];
                    currentSubCategory = categoryPrepare[4];

                    if (currentMainCategory != lastMainCategory)    //Sprawdzanie kategorii
                    {
                        lastMainCategory = currentMainCategory;
                        string categoryToShow = currentMainCategory.ToUpper().Replace('-', ' ');
                        string jsonCategory    = "{"
                                               + "  \"category\":"
                                               + "  {"
                                               + $"     \"name\": \"{categoryToShow}\","
                                               + "      \"parent_id\": 2,"
                                               + "      \"isActive\": true"
                                               + "  },"
                                               + "  \"saveOptions\": true"
                                               + "}";
                        categoriesNumber = test.addNewCategory(jsonCategory);
                        if (categoriesNumber < 0)//Ta opcja jest dopóki nie zrobię pozyskiwania ID na postsstaawie nazwy kategorii!!!!!
                        {
                            lastMainCategory = "";
                            Console.WriteLine("Continue MAIN");
                            continue;
                        }
                    }

                    if (currentSubCategory != lastSubCategory)    //Sprawdzanie SUB-kategorii
                    {
                        lastSubCategory = currentSubCategory;
                        string subCategoryToShow = currentSubCategory.ToUpper().Replace('-', ' ');
                        string jsonSubCategory = "{"
                                               + "  \"category\":"
                                               + "  {"
                                               + $"     \"name\": \"{subCategoryToShow}\","
                                               + $"      \"parent_id\": {categoriesNumber},"
                                               + "      \"isActive\": true"
                                               + "  },"
                                               + "  \"saveOptions\": true"
                                               + "}";

                        subCategoriesNumber = test.addNewCategory(jsonSubCategory);
                        if (subCategoriesNumber < 0)//Ta opcja jest dopóki nie zrobię pozyskiwania ID na postsstaawie nazwy kategorii!!!!!
                        {
                            lastSubCategory = "";
                            Console.WriteLine("Continue SUB");
                            continue;
                        }
                    }

                    sr.WebPageURL = element;
                    content = sr.WebContent;   //reloading content
                    secondResult = SiteReader.findInDocument(htmlDoc, content, "//body/div/section/section/div/div/div/div/div/div/div/div/ul/li/a", "class", "product-wrapper wt_ignore", "href");
                    if (secondResult == null)
                    {
                        Console.Write("Pominieto \n-----------\n\n\n");
                        continue;
                    }
                    
                    sr.WebPageURL = basePage;   //Reset mainpage URL for foreach

                    int licznik_internal = 0; //Licznik dla drugiego poziomu
                    foreach (string element2 in secondResult)
                    {
                        sr.SubPagePath = element2;  //Set subpage
                        content = sr.WebContent;   //reloading content
                        //string categoryMagento_s = "\"Default Category/" + lastCategory.Replace("-", " ") + "\"";

                        try
                        {
                            /* NAZWA */
                            ArrayList itemName = SiteReader.findInDocument(htmlDoc, content, "//body/div/section/article/section/div/section/h1", "itemprop", "name", "none");
                            string itemName_s = itemName[0].ToString();
                            itemName_s = "\"" + itemName_s.Replace("'", "\'") + "\"";

                            /* PODSTAWOWE INFORMACJE (w punktach obok ilustracji) */
                            ArrayList basicsText = SiteReader.findInDocument(htmlDoc, content, "//body/div/section/article/section/div/section/form/div/div/div/div/ul", "class", "overview__detail-list normal black", "none", false);
                            string basicsText_s = "";
                            if (basicsText[0] != null)   //Ten warunek ze względu na to, że ta kategoria nie jest obowiązkowa, a bez niego leci wyjatek
                            {
                                basicsText_s = basicsText[0].ToString();
                                basicsText_s = basicsText_s.Replace("'", "\'");
                            }

                            /* CENA */
                            ArrayList priceText = SiteReader.findInDocument(htmlDoc, content, "//body/div/section/article/section/div/section/form/div/div/div/div/div/div/div/div/span/strong/strong", "itemprop", "price", "none");
                            string priceText_s = priceText[0].ToString().Replace(",", ".");

                            /* CENA/X */
                            ArrayList priceXText = SiteReader.findInDocument(htmlDoc, content, "//body/div/section/article/section/div/section/form/div/div/div/div/div/div/div/div/div", "class", "optional-hidden font-xs", "none");
                            string priceXText_s = priceXText[0].ToString();
                            priceXText_s = "\"" + priceXText_s.Replace("'", "\'") + "\"";

                            /* OPIS */
                            ArrayList descriptionText = SiteReader.findInDocument(htmlDoc, content, "//body/div/section/article/section/div/div/section/div/p", "class", "no-margin", "none", false);
                            string descriptionText_s = descriptionText[0].ToString();
                            descriptionText_s = "\"" + descriptionText_s.Replace("'", "\'") + "\"";

                            /* CECHY */
                            ArrayList detailsText = SiteReader.findInDocument(htmlDoc, content, "//body/div/section/article/section/div/div/section/div/div/dl", "class", "c-datalist c-datalist--33", "none", false);
                            string detailsText_s = "";
                            if (detailsText[0] != null)     //Ten warunek ze względu na to, że ta kategoria nie jest obowiązkowa, a bez niego leci wyjatek
                            {
                                detailsText_s = detailsText[0].ToString();
                                detailsText_s = detailsText_s.Replace("'", "\'");
                            }


                            /* ITEM'S WEIGHT */
                            string weight = "null";
                            try
                            {
                                /* Szukanie pola z wagą */
                                string pattern = @"\d{1,3},?\d{0,3}[  ]?</span>[ ]?k?g";
                                Regex expr = new Regex(pattern);

                                string[] matches = expr.Matches(detailsText_s)[0].ToString().Split('<');

                                string kilograms = matches[1].Split('>')[1].Trim();

                                /* Konwertowanie do double i przypisanie odpowiedniej wartosci w zalezności od jednostki wagi */
                                var converted = Convert.ToDecimal(matches[0].Trim()) * 1.000M;

                                if (kilograms == "kg")
                                    weight = converted.ToString().Replace(',', '.');
                                else if (kilograms == "g")
                                    weight = (converted / 1000).ToString().Replace(',', '.');
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }

                            /* PHOTO URI */
                            ArrayList photoUri = SiteReader.findInDocument(htmlDoc, content, "//head/meta", "property", "og:image", "content");
                            string photoUri_s = photoUri[0].ToString();
                            photoUri_s = "\"" + photoUri_s.Replace("'", "\'") + "\"";

                            /* CREATING SHORT DESCRIPTION FIELD */
                            string shortDescription = "\"" + basicsText_s + "<br>" + detailsText_s + "\"";

                            /* PREPARE STRING WITH LINK (it'll use by magento) */
                            string urlNameString = itemName_s.Unidecode().Replace(" ", "-");

                            /* Prepare write to file content */
                            string json = "{"
                                        + "\"product\":"
                                        + "{"
                                        + $"   \"sku\": {urlNameString},"
                                        + $"   \"name\": {itemName_s},"
                                        + $"   \"price\": {priceText_s},"
                                        + $"   \"weight\": {weight},"
                                        + "    \"attribute_set_id\": 4,"
                                        + "    \"type_id\": \"simple\","
                                        + "    \"status\": 1,"
                                        + "    \"extension_attributes\":"
                                        + "    {"
                                        + "        \"stock_item\":"
                                        + "        {"
                                        + "            \"qty\": 100, "
                                        + "            \"is_in_stock\": true"
                                        + "        }"
                                        + "    },"
                                        + "    \"custom_attributes\": "
                                        + "    {"
                                        + $"        \"category_ids\": {subCategoriesNumber},"
                                        + $"        \"short_description\": {shortDescription},"
                                        + $"        \"description\": {descriptionText_s},"
                                        + $"        \"meta_description\": {itemName_s},"
                                        + $"        \"image\" : {photoUri_s},"
                                        + $"        \"small_image\": {photoUri_s},"
                                        + $"        \"thumbnail\": {photoUri_s},"
                                        + $"        \"swatch_image\": {photoUri_s},"
                                        + $"        \"additional_images\": {photoUri_s}"
                                        + "    }"
                                        + " }"
                                        + "}";

                            
                            if (test.addNewProducts(json) < 0)
                                Console.WriteLine("NIE UDAŁO SIĘ DODAC!!!\n\n");
                            else
                            {
                                Console.WriteLine("DODANO!\n");
                                licznik++;
                                licznik_internal++;
                            }

                        }
                        catch (Exception ex)    //Olać to jakie są wyjątki, trzeba pominąć jeśli taki wyskoczy
                        {
                            licznik_b++;
                            exceptions += licznik_b + sr.WebPageURL + sr.SubPagePath + "\n" + ex + "\n";
                        }
                        //break;
                        if (licznik_internal > 20)	//Ograniczenie w ramach jedengo typu (tj. max ok 20 elementów z danej kategorii do wrzucenia)
                            break;
                    }
                    //break;
                    if (licznik > 500)	//Ograniczenie w ramach całości (tj. max ok 500 elementów do wrzucenia)
                        break;
                }
            }
            
            /****************WYJATKI*******************/

            FileStream exc = new FileStream("exceptions", FileMode.Create);

            string excep_sum = "\n\nW sumie blednych requestow: " + licznik_b;
            var enc = UnicodeEncoding.UTF8;
            exc.Write(enc.GetBytes(exceptions), 0, enc.GetByteCount(exceptions));
            exc.Write(enc.GetBytes(excep_sum), 0, enc.GetByteCount(excep_sum));

            exc.Close();

        }
    }
}
