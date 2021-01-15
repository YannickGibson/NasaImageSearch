using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Newtonsoft.Json;
using System.Windows;
using Java.Net;
using Plugin.Connectivity;
using System.Configuration;
using System.Net;

namespace NasaImageSearch
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();


            SearchTask();

            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (availabilitySpan == null)
                {
                    return true;
                } //Fakt nevim jak to obejít jinak

                Task.Run(async () =>
                {
                    

                    if (IsNetworkAvailable())
                    {
                        networkAvailableString = "Available";
                        availabilitySpan.ForegroundColor = Color.Green;
                    }
                    else
                    {
                        networkAvailableString = "Not Available";
                        availabilitySpan.ForegroundColor = Color.Red;
                    }
                });
                return true; // return true to repeat counting, false to stop timer
            });

        }
        string networkAvailableString;

        public async Task SearchTask()
        {
            HttpClient client = new HttpClient();
            
            try
            {
                var result = await client.GetAsync($"https://images-api.nasa.gov/search?q={searchBar.Text}");
                string jsonString = await result.Content.ReadAsStringAsync();

                ImageSearch imageObject = JsonConvert.DeserializeObject<ImageSearch>(jsonString);

;
                Item[] items = imageObject.collection.items;

                scrollLayout.Children.Clear();
                for (int i = 0; i < items.Length; i++)
                {
                    URL url = new URL(items[i].links[0].href);
                    Android.Graphics.Bitmap bmp = await Task.Run(() => Android.Graphics.BitmapFactory.DecodeStream(url.OpenConnection().InputStream));
                    double aspectRatio = (double)bmp.Width / (double)bmp.Height;

                    Image img = new Image()
                    {
                        Source = items[i].links[0].href
                    };
                    img.WidthRequest = scrollLayout.Width;

                    img.HeightRequest = Convert.ToInt32(scrollLayout.Width / aspectRatio);

                    //img.Aspect = Aspect.AspectFit;

                    scrollLayout.Children.Add(img);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        class ImageSearch
        {
            [JsonProperty("collection")]
            public Collection collection { get; set; }

        }
        class Collection
        {
            public Dictionary<string, int> metadata { get; set; }
            public string version { get; set; }
            public string href { get; set; }
            public Item[] items { get; set; }
            public Links[] links { get; set; }
        }

        class ItemsCollection
        {
            public Item[] item { get; set; }
        }
        class Item
        {
            public string href { get; set; }
            //public List<object> data { get; set; }
            public Links[] links { get; set; }
        }
        class Links
        {
            public string href { get; set; }
            public string render { get; set; }
            public string rel { get; set; }

        }

        public  static bool IsNetworkAvailable()
        {
            try
            {

                using (var client = new WebClient())
                using (client.OpenRead("http://google.com/generate_204"))
                    return true;

            }
            catch
            {
                return false;
            }
            return false;
        }

        private void SearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            if (networkAvailableString == "Available")
            {
                SearchTask();
            }
        }

    }
}
