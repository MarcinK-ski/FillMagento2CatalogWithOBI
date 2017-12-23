using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace ReadObiCatalog
{
    /*
     * Klasa czytajaca zawartosc wybranej strony internetowej  
     */

    class SiteReader
    {
        string _webPageURL; //Domena strony
        string _subPagePath;    //Dalszy adres strony, podstron
        string webContent;  //Zrodlo strony

        public SiteReader(string url)
        {
            _webPageURL = url;
            _subPagePath = "";  
        }

        public string SubPagePath
        {
            get
            {
                return this._subPagePath;
            }
            set
            {
                if (value[0] != '/')
                    value = "/" + value;
                this._subPagePath = value;
            }
        }


        public string WebPageURL
        {
            get
            {
                return this._webPageURL;
            }

            set
            {
                this._webPageURL = value;
            }
        }


        public string WebContent
        {
            get
            {
                try
                {
                    WebClient client = new WebClient();

                    // Pobieranie zawartosci strony
                    Byte[] pageData = client.DownloadData(this.WebPageURL + this.SubPagePath);
                    WebContent = Encoding.UTF8.GetString(pageData);

                }
                catch (WebException ex)
                {
                    WebContent = "Wystapil wyjatek: " + ex;
                }


                return this.webContent;
            }

            set
            {
                this.webContent = value;
            }
        }

        /*
         * node - sciezka do wezla, gdzie znajduje sie szykany element
         * mainAtribute - atrybut elementu, dzieki ktoremu wiadomo, ze tego szukamy. np. class, id itp.
         * valueMainAtribute - value atrybutu wspomnianego wyzej
         * valueFromAtribute - wartosc atrybutu, ktora jest do wyciagniecia (w przypadku null, wyciagamy text z pomiedzy znacznikow html-owych [InnerText])
         */
        public static ArrayList findInDocument(HtmlDocument htmlDoc, string content, string node, string mainAtribute, string valueMainAtribute, string valueFromAtribute, bool innerText = true)
        {
            ArrayList element = new ArrayList();

            //Wczytywanie contentu strony oraz wyszukiwanie okreslonych node-ow
            htmlDoc.LoadHtml(content);
            var htmlBody = htmlDoc.DocumentNode.SelectNodes(node);

            if (htmlBody == null)
                return null;

            //Przechodzenie po kazdym z node-ow i szukanie okreslonego odpowiednimi atrybutami
            foreach (var tmp in htmlBody)
            {
                try
                {
                    //Gdy mainAtribute nie jest okreslony, to dodaje od razu "value"
                    if (mainAtribute == "none" || valueMainAtribute == "none")
                    {
                        if (tmp.Attributes[valueFromAtribute] != null)
                            element.Add(tmp.Attributes[valueFromAtribute].Value);

                    }
                    else
                        if (tmp.Attributes[mainAtribute] != null)
                        if (tmp.Attributes[mainAtribute].Value == valueMainAtribute) //Spr, czy atrybut bierzacego elementu, jest zgodny z podanym "value(...)" 
                        {
                            if (valueFromAtribute != "none")
                                element.Add(tmp.Attributes[valueFromAtribute].Value); //dodanie do elementow, wartosci wybranego atrybutu
                            else
                            {
                                if (innerText)
                                    element.Add(tmp.InnerText);    //dodanie do elementow textu znajdujacego sie pomiedzy znacznikami html np. dla "<b> XYZ </b>" => "XYZ"
                                else
                                    element.Add(tmp.InnerHtml);    //dodanie do elementow textu znajdujacego sie pomiedzy znacznikami html np. dla "<b> XYZ </b>" => "XYZ"
                            }
                                
                        }
                }
                catch (Exception ex)
                {
                    element.Add(ex + "\n");
                }

            }

            return element;

        }
    }
}
