using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.TextField;
using Newtonsoft.Json.Linq;
using Plugin.Connectivity;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using weather_app.Fragment;

namespace weather_app
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        AppCompatButton btnGetWeather;
        TextView txtPlace;
        TextView txtTemp;
        TextView txtWeatherDesc;
        EditText edtCityName;
        ImageView imgWeather;
        ProgressDialogFragment progressDialog;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            edtCityName = FindViewById<EditText>(Resource.Id.edtCityName);
            txtPlace = FindViewById<TextView>(Resource.Id.txtPlace);
            txtTemp = FindViewById<TextView>(Resource.Id.txtTemp);
            txtWeatherDesc = FindViewById<TextView>(Resource.Id.txtWeatherDesc);
            btnGetWeather = FindViewById<AppCompatButton>(Resource.Id.btnGetWeather);
            imgWeather = FindViewById<ImageView>(Resource.Id.imgWeather);
            btnGetWeather.Click += BtnGetWeather_Click;
            GetWeather("Texas");


        }

        private void BtnGetWeather_Click(object sender, System.EventArgs e)
        {
            string place = edtCityName.Text;
            GetWeather(place);
            edtCityName.Text = "";
        }
        async void GetWeather(string place)
        {
            string apiKey = "ced5e6c2b4bb9435f9fb35c240eaeecb";
            string apiBase = "https://api.openweathermap.org/data/2.5/weather?q=";
            string unit = "metric";

            if (string.IsNullOrEmpty(place))
            {
                Toast.MakeText(this, "Please enter a valid city name", ToastLength.Short).Show();
                return;
            }
            if (!CrossConnectivity.Current.IsConnected)
            {
                Toast.MakeText(this, "No internet connection", ToastLength.Short).Show();
                return ;
            }

            ShowProgressDialog("Fetching data...");

            string url = apiBase + place + "&appid=" + apiKey + "&units=" + unit;

            var handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);
            var result = await client.GetAsync(url);
            if (result.IsSuccessStatusCode)
            {
                var mResult = await result.Content.ReadAsStringAsync();
                var resultObject = JObject.Parse(mResult);
                string weatherDescription = resultObject["weather"][0]["description"].ToString();
                string icon = resultObject["weather"][0]["icon"].ToString();
                string temperature = resultObject["main"]["temp"].ToString();
                string placeName = resultObject["name"].ToString();
                string country = resultObject["sys"]["country"].ToString();
                weatherDescription = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(weatherDescription);

                txtWeatherDesc.Text = weatherDescription;
                txtPlace.Text = placeName + ", " + country;
                txtTemp.Text = temperature;
                //download image using webrequest
                string imageUrl = "http://openweathermap.org/img/wn/" + icon + ".png";
                WebRequest request = default(WebRequest);
                request = WebRequest.Create(imageUrl);
                request.Timeout = int.MaxValue;
                request.Method = "GET";

                WebResponse response = default(WebResponse);
                response = await request.GetResponseAsync();
                MemoryStream ms = new MemoryStream();
                response.GetResponseStream().CopyTo(ms);
                byte[] imageData = ms.ToArray();

                Bitmap bitmap = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
                imgWeather.SetImageBitmap(bitmap);

                CloseProgressDialog();
            }
            else
            {
                Toast.MakeText(this, "Please enter a valid city name", ToastLength.Short).Show();
                CloseProgressDialog();
            }
            
        }

        void ShowProgressDialog(string status)
        {
            progressDialog = new ProgressDialogFragment(status);
            var trans = FragmentManager.BeginTransaction();
            progressDialog.Cancelable = false;
            progressDialog.Show(trans, "progress");

        }

        void CloseProgressDialog()
        {
            if (progressDialog != null)
            {
                progressDialog.Dismiss();
                progressDialog = null;
            }
        }
    }
}